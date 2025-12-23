namespace LLMProxy.Domain.LLM;

/// <summary>
/// Format d'API supporté en entrée par le gateway.
/// Permet aux clients d'utiliser différents formats de requête
/// qui sont ensuite normalisés vers le format interne.
/// </summary>
public enum ApiFormat
{
    /// <summary>
    /// Format API OpenAI (standard de facto).
    /// Endpoints: /v1/chat/completions, /v1/embeddings, /v1/models
    /// </summary>
    OpenAI = 0,

    /// <summary>
    /// Format API Ollama (déploiements locaux).
    /// Endpoints: /api/chat, /api/generate, /api/embeddings, /api/tags
    /// </summary>
    Ollama = 1
}
