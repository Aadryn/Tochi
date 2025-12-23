namespace LLMProxy.Domain.LLM;

/// <summary>
/// Format d'encodage des embeddings.
/// </summary>
public enum EmbeddingEncoding
{
    /// <summary>
    /// Tableau de flottants (défaut).
    /// </summary>
    Float = 0,

    /// <summary>
    /// Base64 encodé.
    /// </summary>
    Base64 = 1
}
