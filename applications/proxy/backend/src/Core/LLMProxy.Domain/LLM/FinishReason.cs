namespace LLMProxy.Domain.LLM;

/// <summary>
/// Raison de fin de génération.
/// </summary>
public enum FinishReason
{
    /// <summary>
    /// Génération terminée normalement.
    /// </summary>
    Stop = 0,

    /// <summary>
    /// Limite de tokens atteinte.
    /// </summary>
    Length = 1,

    /// <summary>
    /// Appel de fonction/outil demandé.
    /// </summary>
    ToolCalls = 2,

    /// <summary>
    /// Contenu filtré par modération.
    /// </summary>
    ContentFilter = 3,

    /// <summary>
    /// Erreur pendant la génération.
    /// </summary>
    Error = 4,

    /// <summary>
    /// Annulé par l'utilisateur.
    /// </summary>
    Cancelled = 5
}
