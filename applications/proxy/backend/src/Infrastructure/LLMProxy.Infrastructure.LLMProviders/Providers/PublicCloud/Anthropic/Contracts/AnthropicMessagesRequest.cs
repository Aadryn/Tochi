namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Anthropic.Contracts;

/// <summary>
/// Représente une requête de messages vers l'API Anthropic.
/// </summary>
internal sealed record AnthropicMessagesRequest
{
    /// <summary>
    /// Identifiant du modèle Claude à utiliser.
    /// </summary>
    public required string Model { get; init; }
    
    /// <summary>
    /// Nombre maximum de tokens à générer.
    /// </summary>
    public required int MaxTokens { get; init; }
    
    /// <summary>
    /// Liste des messages de la conversation.
    /// </summary>
    public required List<AnthropicMessage> Messages { get; init; }
    
    /// <summary>
    /// Instructions système optionnelles.
    /// </summary>
    public string? System { get; init; }
    
    /// <summary>
    /// Température de génération.
    /// </summary>
    public decimal? Temperature { get; init; }
    
    /// <summary>
    /// Paramètre top-p.
    /// </summary>
    public decimal? TopP { get; init; }
    
    /// <summary>
    /// Séquences d'arrêt.
    /// </summary>
    public List<string>? StopSequences { get; init; }
    
    /// <summary>
    /// Métadonnées de la requête.
    /// </summary>
    public AnthropicMetadata? Metadata { get; init; }
    
    /// <summary>
    /// Indique si le streaming est activé.
    /// </summary>
    public bool Stream { get; init; }
}
