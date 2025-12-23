using LLMProxy.Application.ApiTransformation;
using LLMProxy.Application.ApiTransformation.Interfaces;
using LLMProxy.Application.ApiTransformation.Ollama.Contracts;
using LLMProxy.Domain.LLM;
using Microsoft.AspNetCore.Mvc;

namespace LLMProxy.Gateway.Controllers.Ollama;

/// <summary>
/// Contrôleur pour l'endpoint Ollama Tags (liste des modèles).
/// </summary>
/// <remarks>
/// <para>
/// Ce contrôleur implémente l'API Ollama Tags (/api/tags).
/// Il retourne la liste des modèles disponibles au format Ollama.
/// </para>
/// </remarks>
[ApiController]
[Route("api")]
[Produces("application/json")]
public sealed class OllamaTagsController : ControllerBase
{
    private readonly ITransformerFactory _transformerFactory;
    private readonly ILLMService _llmService;
    private readonly ILogger<OllamaTagsController> _logger;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="OllamaTagsController"/>.
    /// </summary>
    public OllamaTagsController(
        ITransformerFactory transformerFactory,
        ILLMService llmService,
        ILogger<OllamaTagsController> logger)
    {
        _transformerFactory = transformerFactory;
        _llmService = llmService;
        _logger = logger;
    }

    /// <summary>
    /// Liste les modèles disponibles au format Ollama.
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>La liste des modèles au format Ollama.</returns>
    /// <response code="200">Liste des modèles récupérée avec succès.</response>
    /// <response code="500">Erreur interne du serveur.</response>
    [HttpGet("tags")]
    [ProducesResponseType(typeof(OllamaTagsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ListTags(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Received Ollama tags list request");

            var responseTransformer = _transformerFactory.GetResponseTransformer(ApiFormat.Ollama);

            // Récupérer les modèles via le service LLM
            var models = await _llmService.GetModelsAsync(cancellationToken);

            // Transformer la réponse en format Ollama
            var ollamaResponse = responseTransformer.TransformModelsResponse(models);

            _logger.LogDebug("Ollama tags list completed, {Count} models returned", models.Count);

            return Ok(ollamaResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Ollama tags list request");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "An error occurred processing your request." });
        }
    }

    /// <summary>
    /// Récupère les informations d'un modèle spécifique.
    /// </summary>
    /// <param name="name">Le nom du modèle.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Les informations du modèle au format Ollama.</returns>
    /// <response code="200">Modèle récupéré avec succès.</response>
    /// <response code="404">Modèle non trouvé.</response>
    /// <response code="500">Erreur interne du serveur.</response>
    [HttpPost("show")]
    [ProducesResponseType(typeof(OllamaModelInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ShowModel(
        [FromBody] ShowModelRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrEmpty(request.Name))
        {
            return BadRequest(new { error = "Model name is required." });
        }

        try
        {
            _logger.LogDebug("Received Ollama show model request for {Name}", request.Name);

            // Récupérer tous les modèles et chercher le bon
            var models = await _llmService.GetModelsAsync(cancellationToken);
            var model = models.FirstOrDefault(m =>
                m.Id.Value.Equals(request.Name, StringComparison.OrdinalIgnoreCase) ||
                m.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase));

            if (model is null)
            {
                return NotFound(new { error = $"Model '{request.Name}' not found." });
            }

            var modelInfo = new OllamaModelInfo
            {
                Name = model.Id.Value,
                Model = model.Id.Value,
                ModifiedAt = model.CreatedAt?.ToString("o") ?? DateTimeOffset.UtcNow.ToString("o"),
                Size = 0,
                Digest = "sha256:" + Guid.NewGuid().ToString("N")[..12],
                Details = new OllamaModelDetails
                {
                    Family = model.OwnedBy ?? "unknown",
                    ParameterSize = "unknown"
                }
            };

            return Ok(modelInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Ollama show model request");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "An error occurred processing your request." });
        }
    }
}

/// <summary>
/// Requête pour l'endpoint show.
/// </summary>
public sealed record ShowModelRequest
{
    /// <summary>
    /// Nom du modèle à afficher.
    /// </summary>
    public string? Name { get; init; }
}
