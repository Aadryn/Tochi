using Authorization.Infrastructure.Idp.AzureAd;
using Authorization.Infrastructure.Idp.Keycloak;
using Authorization.Infrastructure.Idp.Okta;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Authorization.Infrastructure.Idp;

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
