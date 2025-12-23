namespace LLMProxy.Domain.LLM;

/// <summary>
/// Format de réponse demandé.
/// </summary>
public enum ResponseFormat
{
    /// <summary>
    /// Texte libre (défaut).
    /// </summary>
    Text = 0,

    /// <summary>
    /// JSON structuré.
    /// </summary>
    Json = 1
}
