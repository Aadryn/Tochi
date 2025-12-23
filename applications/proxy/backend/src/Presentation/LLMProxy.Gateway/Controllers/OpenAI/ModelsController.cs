using LLMProxy.Application.ApiTransformation;
using LLMProxy.Application.ApiTransformation.Interfaces;
using LLMProxy.Application.ApiTransformation.OpenAI.Contracts;
using LLMProxy.Domain.LLM;
using Microsoft.AspNetCore.Mvc;

namespace LLMProxy.Gateway.Controllers.OpenAI;

/// <summary>
/// Contrôleur pour l'endpoint OpenAI Models.
/// </summary>
/// <remarks>
/// <para>
/// Ce contrôleur implémente l'API OpenAI Models standard (/v1/models).
/// Il retourne la liste des modèles disponibles au format OpenAI.
/// </para>
/// </remarks>
[ApiController]
[Route("v1")]
[Produces("application/json")]
public sealed class ModelsController : ControllerBase
{
    private readonly ITransformerFactory _transformerFactory;
    private readonly ILLMService _llmService;
    private readonly ILogger<ModelsController> _logger;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="ModelsController"/>.
    /// </summary>
    public ModelsController(
        ITransformerFactory transformerFactory,
        ILLMService llmService,
        ILogger<ModelsController> logger)
    {
        _transformerFactory = transformerFactory;
        _llmService = llmService;
        _logger = logger;
    }

    /// <summary>
    /// Liste les modèles disponibles au format OpenAI.
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>La liste des modèles au format OpenAI.</returns>
    /// <response code="200">Liste des modèles récupérée avec succès.</response>
    /// <response code="500">Erreur interne du serveur.</response>
    [HttpGet("models")]
    [ProducesResponseType(typeof(OpenAIModelsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ListModels(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Received OpenAI models list request");

            var responseTransformer = _transformerFactory.GetResponseTransformer(ApiFormat.OpenAI);

            // Récupérer les modèles via le service LLM
            var models = await _llmService.GetModelsAsync(cancellationToken);

            // Transformer la réponse en format OpenAI
            var openAIResponse = responseTransformer.TransformModelsResponse(models);

            _logger.LogDebug("OpenAI models list completed, {Count} models returned", models.Count);

            return Ok(openAIResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OpenAI models list request");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = new { message = "An error occurred processing your request." } });
        }
    }

    /// <summary>
    /// Récupère un modèle spécifique par son identifiant.
    /// </summary>
    /// <param name="modelId">L'identifiant du modèle.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Les informations du modèle au format OpenAI.</returns>
    /// <response code="200">Modèle récupéré avec succès.</response>
    /// <response code="404">Modèle non trouvé.</response>
    /// <response code="500">Erreur interne du serveur.</response>
    [HttpGet("models/{modelId}")]
    [ProducesResponseType(typeof(OpenAIModelInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetModel(
        [FromRoute] string modelId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Received OpenAI model retrieval request for {ModelId}", modelId);

            // Récupérer tous les modèles et chercher le bon
            var models = await _llmService.GetModelsAsync(cancellationToken);
            var model = models.FirstOrDefault(m =>
                m.Id.Value.Equals(modelId, StringComparison.OrdinalIgnoreCase));

            if (model is null)
            {
                return NotFound(new { error = new { message = $"Model '{modelId}' not found." } });
            }

            var modelInfo = new OpenAIModelInfo
            {
                Id = model.Id.Value,
                OwnedBy = model.OwnedBy ?? "unknown",
                Created = model.CreatedAt?.ToUnixTimeSeconds() ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            return Ok(modelInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OpenAI model retrieval request");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = new { message = "An error occurred processing your request." } });
        }
    }
}
