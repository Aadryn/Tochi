namespace LLMProxy.Domain.LLM;

/// <summary>
/// Rôle d'un message dans une conversation LLM.
/// </summary>
public enum MessageRole
{
    /// <summary>
    /// Message système définissant le comportement de l'assistant.
    /// </summary>
    System = 0,

    /// <summary>
    /// Message de l'utilisateur.
    /// </summary>
    User = 1,

    /// <summary>
    /// Réponse de l'assistant (modèle LLM).
    /// </summary>
    Assistant = 2,

    /// <summary>
    /// Résultat d'un appel de fonction/outil.
    /// </summary>
    Tool = 3,

    /// <summary>
    /// Appel de fonction par l'assistant (OpenAI legacy).
    /// </summary>
    Function = 4
}
