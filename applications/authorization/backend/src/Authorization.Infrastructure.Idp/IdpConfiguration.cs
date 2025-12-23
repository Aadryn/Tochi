namespace Authorization.Infrastructure.Idp;

/// <summary>
/// Configuration générale de l'IDP.
/// </summary>
public class IdpConfiguration
{
    /// <summary>
    /// Nom de la section de configuration.
    /// </summary>
    public const string SectionName = "Idp";

    /// <summary>
    /// Type de fournisseur d'identité par défaut.
    /// </summary>
    public IdpProviderType DefaultProvider { get; set; } = IdpProviderType.Keycloak;

    /// <summary>
    /// Active la synchronisation JIT (Just-In-Time).
    /// </summary>
    public bool EnableJitSync { get; set; } = true;

    /// <summary>
    /// Active la synchronisation batch.
    /// </summary>
    public bool EnableBatchSync { get; set; } = true;

    /// <summary>
    /// Expression cron pour la synchronisation batch (défaut: toutes les 15 minutes).
    /// </summary>
    public string BatchSyncCronExpression { get; set; } = "*/15 * * * *";

    /// <summary>
    /// Active la synchronisation par webhooks.
    /// </summary>
    public bool EnableWebhookSync { get; set; } = false;
}
