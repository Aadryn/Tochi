using LLMProxy.Application.ApiTransformation;
using LLMProxy.Application.ApiTransformation.Interfaces;
using LLMProxy.Application.ApiTransformation.Ollama.Contracts;
using LLMProxy.Domain.LLM;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace LLMProxy.Gateway.Controllers.Ollama;

/// <summary>
/// Contrôleur pour l'endpoint Ollama Chat.
/// </summary>
/// <remarks>
/// <para>
/// Ce contrôleur implémente l'API Ollama Chat (/api/chat).
/// Il accepte les requêtes au format Ollama et les transforme en format canonique interne.
/// </para>
/// </remarks>
[ApiController]
[Route("api")]
[Produces("application/json")]
public sealed class OllamaChatController : ControllerBase
{
    private readonly ITransformerFactory _transformerFactory;
    private readonly ILLMService _llmService;
    private readonly ILogger<OllamaChatController> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="OllamaChatController"/>.
    /// </summary>
    public OllamaChatController(
        ITransformerFactory transformerFactory,
        ILLMService llmService,
        ILogger<OllamaChatController> logger)
    {
        _transformerFactory = transformerFactory;
        _llmService = llmService;
        _logger = logger;
    }

    /// <summary>
    /// Génère une réponse de chat au format Ollama.
    /// </summary>
    /// <param name="request">La requête de chat au format Ollama.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>La réponse de chat au format Ollama.</returns>
    /// <response code="200">Chat généré avec succès.</response>
    /// <response code="400">Requête invalide.</response>
    /// <response code="500">Erreur interne du serveur.</response>
    [HttpPost("chat")]
    [ProducesResponseType(typeof(OllamaChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Chat(
        [FromBody] OllamaChatRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { error = "Request body is required." });
        }

        try
        {
            _logger.LogDebug(
                "Received Ollama chat request for model {Model}",
                request.Model);

            var requestTransformer = _transformerFactory.GetRequestTransformer(ApiFormat.Ollama);
            var responseTransformer = _transformerFactory.GetResponseTransformer(ApiFormat.Ollama);

            // Transformer la requête Ollama en format canonique
            var canonicalRequest = requestTransformer.TransformChatRequest(request);

            // Vérifier si streaming est demandé (par défaut true pour Ollama)
            var shouldStream = request.Stream;

            if (shouldStream)
            {
                return await StreamChatResponse(
                    canonicalRequest,
                    responseTransformer,
                    cancellationToken);
            }

            // Exécuter la requête via le service LLM
            var canonicalResponse = await _llmService.ChatAsync(canonicalRequest, cancellationToken);

            // Transformer la réponse en format Ollama
            var ollamaResponse = responseTransformer.TransformChatResponse(canonicalResponse);

            _logger.LogDebug(
                "Ollama chat completed for model {Model}",
                request.Model);

            return Ok(ollamaResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Ollama chat request");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "An error occurred processing your request." });
        }
    }

    /// <summary>
    /// Génère une completion de texte au format Ollama (endpoint legacy).
    /// </summary>
    /// <param name="request">La requête de génération au format Ollama.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>La réponse de génération au format Ollama.</returns>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(OllamaChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Generate(
        [FromBody] OllamaChatRequest request,
        CancellationToken cancellationToken)
    {
        // Pour l'endpoint generate, on délègue à l'endpoint chat
        // car notre implémentation utilise le même format canonique
        return await Chat(request, cancellationToken);
    }

    /// <summary>
    /// Gère le streaming de la réponse au format Ollama (NDJSON).
    /// </summary>
    private async Task<IActionResult> StreamChatResponse(
        LLMRequest canonicalRequest,
        IResponseTransformer responseTransformer,
        CancellationToken cancellationToken)
    {
        // Ollama utilise NDJSON (newline-delimited JSON) pour le streaming
        Response.ContentType = "application/x-ndjson";
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        await foreach (var chunk in _llmService.ChatStreamAsync(canonicalRequest, cancellationToken))
        {
            var jsonLine = responseTransformer.TransformStreamChunk(chunk);
            await Response.WriteAsync($"{jsonLine}\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }

        return new EmptyResult();
    }
}
