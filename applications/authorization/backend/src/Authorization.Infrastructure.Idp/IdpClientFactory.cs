using Authorization.Infrastructure.Idp.AzureAd;
using Authorization.Infrastructure.Idp.Keycloak;
using Authorization.Infrastructure.Idp.Okta;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

/// <summary>
/// Implémentation de la factory IDP.
/// </summary>
public class IdpClientFactory : IIdpClientFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IdpConfiguration _configuration;

    /// <summary>
    /// Initialise une nouvelle instance de la factory.
    /// </summary>
    public IdpClientFactory(
        IServiceProvider serviceProvider,
        IOptions<IdpConfiguration> configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration.Value;
    }

    /// <inheritdoc />
    public IIdpClient GetClient(IdpProviderType providerType)
    {
        return providerType switch
        {
            IdpProviderType.Keycloak => _serviceProvider.GetRequiredService<KeycloakClient>(),
            IdpProviderType.AzureAd => _serviceProvider.GetRequiredService<AzureAdClient>(),
            IdpProviderType.Okta => _serviceProvider.GetRequiredService<OktaClient>(),
            _ => throw new ArgumentException($"Unknown IDP provider type: {providerType}", nameof(providerType))
        };
    }

    /// <inheritdoc />
    public IIdpClient GetDefaultClient()
    {
        return GetClient(_configuration.DefaultProvider);
    }
}
