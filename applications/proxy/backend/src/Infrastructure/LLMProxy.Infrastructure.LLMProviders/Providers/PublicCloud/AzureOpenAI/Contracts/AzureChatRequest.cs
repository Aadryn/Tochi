namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.AzureOpenAI.Contracts;

/// <summary>
/// Représente une requête de chat vers Azure OpenAI.
/// </summary>
internal sealed record AzureChatRequest
{
    /// <summary>
    /// Liste des messages de la conversation.
    /// </summary>
    public required List<AzureMessage> Messages { get; init; }
    
    /// <summary>
    /// Température de génération (0.0 à 2.0).
    /// </summary>
    public decimal? Temperature { get; init; }
    
    /// <summary>
    /// Nombre maximum de tokens à générer.
    /// </summary>
    public int? MaxTokens { get; init; }
    
    /// <summary>
    /// Paramètre top-p pour le nucleus sampling.
    /// </summary>
    public decimal? TopP { get; init; }
    
    /// <summary>
    /// Nombre de complétions à générer.
    /// </summary>
    public int? N { get; init; }
    
    /// <summary>
    /// Séquences d'arrêt.
    /// </summary>
    public List<string>? Stop { get; init; }
    
    /// <summary>
    /// Pénalité de présence (-2.0 à 2.0).
    /// </summary>
    public decimal? PresencePenalty { get; init; }
    
    /// <summary>
    /// Pénalité de fréquence (-2.0 à 2.0).
    /// </summary>
    public decimal? FrequencyPenalty { get; init; }
    
    /// <summary>
    /// Identifiant de l'utilisateur final.
    /// </summary>
    public string? User { get; init; }
    
    /// <summary>
    /// Seed pour la génération déterministe.
    /// </summary>
    public int? Seed { get; init; }
    
    /// <summary>
    /// Format de réponse souhaité.
    /// </summary>
    public AzureResponseFormat? ResponseFormat { get; init; }
    
    /// <summary>
    /// Indique si le streaming est activé.
    /// </summary>
    public bool Stream { get; init; }
}
