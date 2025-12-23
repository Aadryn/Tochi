using LLMProxy.Domain.LLM;

namespace LLMProxy.Application.ApiTransformation.Interfaces;

/// <summary>
/// Interface pour la transformation des requêtes API entrantes
/// vers le format canonique interne.
/// </summary>
public interface IRequestTransformer
{
    /// <summary>
    /// Format d'API supporté par ce transformer.
    /// </summary>
    ApiFormat SupportedFormat { get; }

    /// <summary>
    /// Transforme une requête de chat vers le format canonique.
    /// </summary>
    /// <param name="request">Requête dans le format spécifique de l'API.</param>
    /// <returns>Requête dans le format canonique interne.</returns>
    LLMRequest TransformChatRequest(object request);

    /// <summary>
    /// Transforme une requête d'embedding vers le format canonique.
    /// </summary>
    /// <param name="request">Requête dans le format spécifique de l'API.</param>
    /// <returns>Requête dans le format canonique interne.</returns>
    EmbeddingRequest TransformEmbeddingRequest(object request);
}
