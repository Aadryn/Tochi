namespace LLMProxy.Domain.LLM;

/// <summary>
/// Interface du service LLM pour le traitement des requêtes de chat et d'embedding.
/// </summary>
/// <remarks>
/// <para>
/// Ce service abstrait les interactions avec les différents providers LLM (OpenAI, Ollama, etc.)
/// en utilisant un format canonique interne. Les contrôleurs de l'API utilisent ce service
/// après avoir transformé les requêtes dans le format canonique.
/// </para>
/// <para>
/// <strong>Responsabilités</strong> :
/// </para>
/// <list type="bullet">
/// <item><description>Routage vers le provider approprié selon la configuration</description></item>
/// <item><description>Gestion du streaming pour les réponses de chat</description></item>
/// <item><description>Récupération de la liste des modèles disponibles</description></item>
/// </list>
/// </remarks>
public interface ILLMService
{
    /// <summary>
    /// Exécute une requête de chat et retourne la réponse complète.
    /// </summary>
    /// <param name="request">La requête de chat en format canonique.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>La réponse de chat en format canonique.</returns>
    Task<LLMResponse> ChatAsync(LLMRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exécute une requête de chat et retourne les chunks en streaming.
    /// </summary>
    /// <param name="request">La requête de chat en format canonique.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Un flux asynchrone de chunks de réponse.</returns>
    IAsyncEnumerable<LLMResponse> ChatStreamAsync(
        LLMRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Génère des embeddings pour le texte fourni.
    /// </summary>
    /// <param name="request">La requête d'embedding en format canonique.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>La réponse d'embedding en format canonique.</returns>
    Task<EmbeddingResponse> EmbeddingsAsync(EmbeddingRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère la liste des modèles disponibles.
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>La liste des modèles disponibles.</returns>
    Task<IReadOnlyList<LLMModel>> GetModelsAsync(CancellationToken cancellationToken = default);
}
