using LLMProxy.Application.ApiTransformation;
using LLMProxy.Application.ApiTransformation.Interfaces;
using LLMProxy.Application.ApiTransformation.OpenAI.Contracts;
using LLMProxy.Domain.LLM;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace LLMProxy.Gateway.Controllers.OpenAI;

/// <summary>
/// Contrôleur pour l'endpoint OpenAI Chat Completions.
/// </summary>
/// <remarks>
/// <para>
/// Ce contrôleur implémente l'API OpenAI Chat Completions standard (/v1/chat/completions).
/// Il accepte les requêtes au format OpenAI et les transforme en format canonique interne
/// pour être routées vers le provider LLM configuré.
/// </para>
/// <para>
/// <strong>Endpoints supportés</strong> :
/// </para>
/// <list type="bullet">
/// <item><description>POST /v1/chat/completions - Génération de completion</description></item>
/// </list>
/// </remarks>
[ApiController]
[Route("v1/chat")]
[Produces("application/json")]
public sealed class ChatCompletionsController : ControllerBase
{
    private readonly ITransformerFactory _transformerFactory;
    private readonly ILLMService _llmService;
    private readonly ILogger<ChatCompletionsController> _logger;

    private static readonly JsonSerializerOptions StreamJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="ChatCompletionsController"/>.
    /// </summary>
    /// <param name="transformerFactory">Factory pour obtenir les transformers.</param>
    /// <param name="llmService">Service LLM pour le traitement des requêtes.</param>
    /// <param name="logger">Logger pour la journalisation.</param>
    public ChatCompletionsController(
        ITransformerFactory transformerFactory,
        ILLMService llmService,
        ILogger<ChatCompletionsController> logger)
    {
        _transformerFactory = transformerFactory;
        _llmService = llmService;
        _logger = logger;
    }

    /// <summary>
    /// Crée une completion de chat au format OpenAI.
    /// </summary>
    /// <param name="request">La requête de chat au format OpenAI.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>La réponse de completion au format OpenAI.</returns>
    /// <response code="200">Completion générée avec succès.</response>
    /// <response code="400">Requête invalide.</response>
    /// <response code="500">Erreur interne du serveur.</response>
    [HttpPost("completions")]
    [ProducesResponseType(typeof(OpenAIChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateChatCompletion(
        [FromBody] OpenAIChatRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { error = new { message = "Request body is required." } });
        }

        try
        {
            _logger.LogDebug(
                "Received OpenAI chat completion request for model {Model}",
                request.Model);

            var requestTransformer = _transformerFactory.GetRequestTransformer(ApiFormat.OpenAI);
            var responseTransformer = _transformerFactory.GetResponseTransformer(ApiFormat.OpenAI);

            // Transformer la requête OpenAI en format canonique
            var canonicalRequest = requestTransformer.TransformChatRequest(request);

            // Vérifier si streaming est demandé
            if (request.Stream == true)
            {
                return await StreamChatCompletion(
                    canonicalRequest,
                    responseTransformer,
                    cancellationToken);
            }

            // Exécuter la requête via le service LLM
            var canonicalResponse = await _llmService.ChatAsync(canonicalRequest, cancellationToken);

            // Transformer la réponse en format OpenAI
            var openAIResponse = responseTransformer.TransformChatResponse(canonicalResponse);

            _logger.LogDebug(
                "OpenAI chat completion completed for model {Model}",
                request.Model);

            return Ok(openAIResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OpenAI chat completion request");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = new { message = "An error occurred processing your request." } });
        }
    }

    /// <summary>
    /// Gère le streaming de la réponse au format SSE.
    /// </summary>
    private async Task<IActionResult> StreamChatCompletion(
        LLMRequest canonicalRequest,
        IResponseTransformer responseTransformer,
        CancellationToken cancellationToken)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        await foreach (var chunk in _llmService.ChatStreamAsync(canonicalRequest, cancellationToken))
        {
            var sseData = responseTransformer.TransformStreamChunk(chunk);
            await Response.WriteAsync($"data: {sseData}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }

        // Envoyer le message de fin
        await Response.WriteAsync("data: [DONE]\n\n", cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);

        return new EmptyResult();
    }
}
