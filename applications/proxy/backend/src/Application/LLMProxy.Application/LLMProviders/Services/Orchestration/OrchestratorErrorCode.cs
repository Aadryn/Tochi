namespace LLMProxy.Application.LLMProviders.Services.Orchestration;

/// <summary>
/// Codes d'erreur de l'orchestrateur.
/// </summary>
public enum OrchestratorErrorCode
{
    /// <summary>
    /// Aucun provider disponible.
    /// </summary>
    NoProviderAvailable,

    /// <summary>
    /// Aucun modèle compatible trouvé.
    /// </summary>
    NoCompatibleModel,

    /// <summary>
    /// Tous les providers ont échoué.
    /// </summary>
    AllProvidersFailed,

    /// <summary>
    /// Requête invalide.
    /// </summary>
    InvalidRequest,

    /// <summary>
    /// Timeout global dépassé.
    /// </summary>
    GlobalTimeout,

    /// <summary>
    /// Opération annulée.
    /// </summary>
    Cancelled
}
