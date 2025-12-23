namespace LLMProxy.Domain.LLM;

/// <summary>
/// Représente un message dans une conversation avec un LLM.
/// </summary>
public sealed record LLMMessage
{
    /// <summary>
    /// Rôle du message (system, user, assistant, tool).
    /// </summary>
    public required MessageRole Role { get; init; }

    /// <summary>
    /// Contenu textuel du message.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Nom optionnel pour identifier le participant.
    /// Utilisé pour les conversations multi-participants.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// ID de l'outil pour les messages de type Tool.
    /// </summary>
    public string? ToolCallId { get; init; }

    /// <summary>
    /// Crée un message système.
    /// </summary>
    /// <param name="content">Contenu du message système.</param>
    /// <returns>Message système.</returns>
    public static LLMMessage System(string content) => new()
    {
        Role = MessageRole.System,
        Content = content
    };

    /// <summary>
    /// Crée un message utilisateur.
    /// </summary>
    /// <param name="content">Contenu du message utilisateur.</param>
    /// <param name="name">Nom optionnel de l'utilisateur.</param>
    /// <returns>Message utilisateur.</returns>
    public static LLMMessage User(string content, string? name = null) => new()
    {
        Role = MessageRole.User,
        Content = content,
        Name = name
    };

    /// <summary>
    /// Crée un message assistant.
    /// </summary>
    /// <param name="content">Contenu de la réponse.</param>
    /// <returns>Message assistant.</returns>
    public static LLMMessage Assistant(string content) => new()
    {
        Role = MessageRole.Assistant,
        Content = content
    };

    /// <summary>
    /// Crée un message de résultat d'outil.
    /// </summary>
    /// <param name="toolCallId">ID de l'appel d'outil.</param>
    /// <param name="content">Résultat de l'outil.</param>
    /// <returns>Message outil.</returns>
    public static LLMMessage ToolResult(string toolCallId, string content) => new()
    {
        Role = MessageRole.Tool,
        Content = content,
        ToolCallId = toolCallId
    };
}
