namespace LLMProxy.Domain.LLM;

/// <summary>
/// Capacités d'un modèle LLM.
/// </summary>
[Flags]
public enum ModelCapabilities
{
    /// <summary>
    /// Aucune capacité spécifique.
    /// </summary>
    None = 0,

    /// <summary>
    /// Complétion de chat (messages).
    /// </summary>
    ChatCompletion = 1 << 0,

    /// <summary>
    /// Complétion de texte (legacy).
    /// </summary>
    TextCompletion = 1 << 1,

    /// <summary>
    /// Génération d'embeddings.
    /// </summary>
    Embeddings = 1 << 2,

    /// <summary>
    /// Support du streaming.
    /// </summary>
    Streaming = 1 << 3,

    /// <summary>
    /// Appel de fonctions/outils.
    /// </summary>
    FunctionCalling = 1 << 4,

    /// <summary>
    /// Analyse d'images (vision).
    /// </summary>
    Vision = 1 << 5,

    /// <summary>
    /// Génération d'images.
    /// </summary>
    ImageGeneration = 1 << 6,

    /// <summary>
    /// Analyse audio (speech-to-text).
    /// </summary>
    Audio = 1 << 7,

    /// <summary>
    /// Sortie JSON structurée garantie.
    /// </summary>
    JsonMode = 1 << 8
}
