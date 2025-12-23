using LLMProxy.Domain.LLM;

namespace LLMProxy.Application.ApiTransformation.Interfaces;

/// <summary>
/// Interface pour la transformation des réponses canoniques internes
/// vers le format API demandé par le client.
/// </summary>
public interface IResponseTransformer
{
    /// <summary>
    /// Format d'API supporté par ce transformer.
    /// </summary>
    ApiFormat SupportedFormat { get; }

    /// <summary>
    /// Transforme une réponse de chat vers le format API spécifique.
    /// </summary>
    /// <param name="response">Réponse dans le format canonique interne.</param>
    /// <returns>Réponse dans le format API spécifique.</returns>
    object TransformChatResponse(LLMResponse response);

    /// <summary>
    /// Transforme un chunk de streaming vers le format API spécifique.
    /// </summary>
    /// <param name="chunk">Chunk dans le format canonique interne.</param>
    /// <returns>Chunk dans le format API spécifique (sérialisé JSON).</returns>
    string TransformStreamChunk(LLMResponse chunk);

    /// <summary>
    /// Transforme une réponse d'embedding vers le format API spécifique.
    /// </summary>
    /// <param name="response">Réponse dans le format canonique interne.</param>
    /// <returns>Réponse dans le format API spécifique.</returns>
    object TransformEmbeddingResponse(EmbeddingResponse response);

    /// <summary>
    /// Transforme une liste de modèles vers le format API spécifique.
    /// </summary>
    /// <param name="models">Liste des modèles disponibles.</param>
    /// <returns>Réponse dans le format API spécifique.</returns>
    object TransformModelsResponse(IReadOnlyList<LLMModel> models);
}
