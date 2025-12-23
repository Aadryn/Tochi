using Authorization.Infrastructure.Idp.AzureAd;
using Authorization.Infrastructure.Idp.Jobs;
using Authorization.Infrastructure.Idp.Keycloak;
using Authorization.Infrastructure.Idp.Okta;
using Authorization.Infrastructure.Idp.Sync;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Authorization.Infrastructure.Idp;

/// <summary>
/// Extensions pour l'enregistrement des services IDP.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Ajoute les services d'intégration IDP.
    /// </summary>
    /// <param name="services">Collection de services.</param>
    /// <param name="configuration">Configuration de l'application.</param>
    /// <returns>La collection de services pour chaînage.</returns>
    public static IServiceCollection AddIdpIntegration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configuration générale
        services.Configure<IdpConfiguration>(configuration.GetSection(IdpConfiguration.SectionName));

        // Factory
        services.AddSingleton<IIdpClientFactory, IdpClientFactory>();

        // Service de synchronisation
        services.AddScoped<IIdpSyncService, IdpSyncService>();

        // Configurer les clients selon la configuration
        var idpConfig = configuration.GetSection(IdpConfiguration.SectionName).Get<IdpConfiguration>();

        // Toujours configurer Keycloak (pour le développement)
        services.AddKeycloak(configuration);

        // Configurer Azure AD si présent
        if (configuration.GetSection(AzureAdConfiguration.SectionName).Exists())
        {
            services.AddAzureAd(configuration);
        }

        // Configurer Okta si présent
        if (configuration.GetSection(OktaConfiguration.SectionName).Exists())
        {
            services.AddOkta(configuration);
        }

        // Job de synchronisation batch
        if (idpConfig?.EnableBatchSync ?? false)
        {
            services.AddHostedService<IdpSyncJob>();
        }

        return services;
    }

    /// <summary>
    /// Ajoute le client Keycloak.
    /// </summary>
    public static IServiceCollection AddKeycloak(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var keycloakConfig = configuration
            .GetSection(KeycloakConfiguration.SectionName)
            .Get<KeycloakConfiguration>();

        if (keycloakConfig is null)
        {
            // Configuration par défaut pour le développement
            keycloakConfig = new KeycloakConfiguration
            {
                BaseUrl = "http://localhost:8080",
                Realm = "master",
                ClientId = "admin-cli",
                ClientSecret = "admin"
            };
        }

        services.AddSingleton(keycloakConfig);
        services.AddHttpClient<KeycloakClient>();
        services.AddScoped<KeycloakClient>();

        return services;
    }

    /// <summary>
    /// Ajoute le client Azure AD.
    /// </summary>
    public static IServiceCollection AddAzureAd(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AzureAdConfiguration>(
            configuration.GetSection(AzureAdConfiguration.SectionName));

        var azureConfig = configuration
            .GetSection(AzureAdConfiguration.SectionName)
            .Get<AzureAdConfiguration>();

        if (azureConfig is not null)
        {
            services.AddSingleton(azureConfig);
            services.AddScoped<AzureAdClient>();
        }

        return services;
    }

    /// <summary>
    /// Ajoute le client Okta.
    /// </summary>
    public static IServiceCollection AddOkta(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<OktaConfiguration>(
            configuration.GetSection(OktaConfiguration.SectionName));

        var oktaConfig = configuration
            .GetSection(OktaConfiguration.SectionName)
            .Get<OktaConfiguration>();

        if (oktaConfig is not null)
        {
            services.AddSingleton(oktaConfig);
            services.AddScoped<OktaClient>();
        }

        return services;
    }
}
