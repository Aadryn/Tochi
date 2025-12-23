using LLMProxy.Application.ApiTransformation;
using LLMProxy.Application.ApiTransformation.Interfaces;
using LLMProxy.Application.ApiTransformation.Ollama.Contracts;
using LLMProxy.Domain.LLM;
using Microsoft.AspNetCore.Mvc;

namespace LLMProxy.Gateway.Controllers.Ollama;

/// <summary>
/// Contrôleur pour l'endpoint Ollama Embeddings.
/// </summary>
/// <remarks>
/// <para>
/// Ce contrôleur implémente l'API Ollama Embeddings (/api/embeddings et /api/embed).
/// Il accepte les requêtes au format Ollama et les transforme en format canonique interne.
/// </para>
/// </remarks>
[ApiController]
[Route("api")]
[Produces("application/json")]
public sealed class OllamaEmbeddingsController : ControllerBase
{
    private readonly ITransformerFactory _transformerFactory;
    private readonly ILLMService _llmService;
    private readonly ILogger<OllamaEmbeddingsController> _logger;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="OllamaEmbeddingsController"/>.
    /// </summary>
    public OllamaEmbeddingsController(
        ITransformerFactory transformerFactory,
        ILLMService llmService,
        ILogger<OllamaEmbeddingsController> logger)
    {
        _transformerFactory = transformerFactory;
        _llmService = llmService;
        _logger = logger;
    }

    /// <summary>
    /// Génère des embeddings au format Ollama.
    /// </summary>
    /// <param name="request">La requête d'embeddings au format Ollama.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>La réponse d'embeddings au format Ollama.</returns>
    /// <response code="200">Embeddings générés avec succès.</response>
    /// <response code="400">Requête invalide.</response>
    /// <response code="500">Erreur interne du serveur.</response>
    [HttpPost("embeddings")]
    [HttpPost("embed")]
    [ProducesResponseType(typeof(OllamaEmbeddingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Embeddings(
        [FromBody] OllamaEmbeddingRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { error = "Request body is required." });
        }

        try
        {
            _logger.LogDebug(
                "Received Ollama embeddings request for model {Model}",
                request.Model);

            var requestTransformer = _transformerFactory.GetRequestTransformer(ApiFormat.Ollama);
            var responseTransformer = _transformerFactory.GetResponseTransformer(ApiFormat.Ollama);

            // Transformer la requête Ollama en format canonique
            var canonicalRequest = requestTransformer.TransformEmbeddingRequest(request);

            // Exécuter la requête via le service LLM
            var canonicalResponse = await _llmService.EmbeddingsAsync(canonicalRequest, cancellationToken);

            // Transformer la réponse en format Ollama
            var ollamaResponse = responseTransformer.TransformEmbeddingResponse(canonicalResponse);

            _logger.LogDebug(
                "Ollama embeddings completed for model {Model}",
                request.Model);

            return Ok(ollamaResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Ollama embeddings request");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "An error occurred processing your request." });
        }
    }
}
