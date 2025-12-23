namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Cohere.Contracts;

/// <summary>
/// Réponse de chat Cohere.
/// </summary>
internal sealed record CohereChatResponse
{
    /// <summary>
    /// Identifiant de la réponse.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Contenu du message.
    /// </summary>
    public CohereMessageContent? Message { get; init; }

    /// <summary>
    /// Raison d'arrêt.
    /// </summary>
    public string? FinishReason { get; init; }

    /// <summary>
    /// Informations d'utilisation.
    /// </summary>
    public CohereUsage? Usage { get; init; }
}
