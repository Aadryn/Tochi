using LLMProxy.Application.ApiTransformation;
using LLMProxy.Application.ApiTransformation.Interfaces;
using LLMProxy.Application.ApiTransformation.OpenAI.Contracts;
using LLMProxy.Domain.LLM;
using Microsoft.AspNetCore.Mvc;

namespace LLMProxy.Gateway.Controllers.OpenAI;

/// <summary>
/// Contrôleur pour l'endpoint OpenAI Embeddings.
/// </summary>
/// <remarks>
/// <para>
/// Ce contrôleur implémente l'API OpenAI Embeddings standard (/v1/embeddings).
/// Il accepte les requêtes au format OpenAI et les transforme en format canonique interne.
/// </para>
/// </remarks>
[ApiController]
[Route("v1")]
[Produces("application/json")]
public sealed class EmbeddingsController : ControllerBase
{
    private readonly ITransformerFactory _transformerFactory;
    private readonly ILLMService _llmService;
    private readonly ILogger<EmbeddingsController> _logger;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="EmbeddingsController"/>.
    /// </summary>
    public EmbeddingsController(
        ITransformerFactory transformerFactory,
        ILLMService llmService,
        ILogger<EmbeddingsController> logger)
    {
        _transformerFactory = transformerFactory;
        _llmService = llmService;
        _logger = logger;
    }

    /// <summary>
    /// Crée des embeddings au format OpenAI.
    /// </summary>
    /// <param name="request">La requête d'embeddings au format OpenAI.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>La réponse d'embeddings au format OpenAI.</returns>
    /// <response code="200">Embeddings générés avec succès.</response>
    /// <response code="400">Requête invalide.</response>
    /// <response code="500">Erreur interne du serveur.</response>
    [HttpPost("embeddings")]
    [ProducesResponseType(typeof(OpenAIEmbeddingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateEmbeddings(
        [FromBody] OpenAIEmbeddingRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { error = new { message = "Request body is required." } });
        }

        try
        {
            _logger.LogDebug(
                "Received OpenAI embeddings request for model {Model}",
                request.Model);

            var requestTransformer = _transformerFactory.GetRequestTransformer(ApiFormat.OpenAI);
            var responseTransformer = _transformerFactory.GetResponseTransformer(ApiFormat.OpenAI);

            // Transformer la requête OpenAI en format canonique
            var canonicalRequest = requestTransformer.TransformEmbeddingRequest(request);

            // Exécuter la requête via le service LLM
            var canonicalResponse = await _llmService.EmbeddingsAsync(canonicalRequest, cancellationToken);

            // Transformer la réponse en format OpenAI
            var openAIResponse = responseTransformer.TransformEmbeddingResponse(canonicalResponse);

            _logger.LogDebug(
                "OpenAI embeddings completed for model {Model}",
                request.Model);

            return Ok(openAIResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OpenAI embeddings request");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = new { message = "An error occurred processing your request." } });
        }
    }
}
