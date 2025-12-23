using LLMProxy.Infrastructure.Authorization.Abstractions;
using LLMProxy.Infrastructure.Authorization.Configuration;
using LLMProxy.Infrastructure.Authorization.Middleware;
using LLMProxy.Infrastructure.Authorization.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;
using Polly.Extensions.Http;

namespace LLMProxy.Infrastructure.Authorization;

/// <summary>
/// Extensions pour l'enregistrement des services d'autorisation.
/// </summary>
/// <remarks>
/// <para>
/// Deux modes d'intégration sont disponibles :
/// </para>
/// <list type="bullet">
/// <item>
/// <term>OpenFGA direct</term>
/// <description>Connexion directe au serveur OpenFGA (pour applications avec accès réseau direct)</description>
/// </item>
/// <item>
/// <term>HTTP client</term>
/// <description>Appels via API REST Authorization (pour applications isolées ou multi-tenants)</description>
/// </item>
/// </list>
/// </remarks>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Ajoute les services d'autorisation OpenFGA au conteneur d'injection de dépendances.
    /// </summary>
    /// <param name="services">Collection de services.</param>
    /// <param name="configuration">Configuration de l'application.</param>
    /// <returns>La collection de services pour chaînage.</returns>
    /// <example>
    /// <code>
    /// // Dans Program.cs ou Startup.cs
    /// builder.Services.AddOpenFgaAuthorization(builder.Configuration);
    /// </code>
    /// </example>
    public static IServiceCollection AddOpenFgaAuthorization(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Configuration
        services.Configure<OpenFgaConfiguration>(
            configuration.GetSection(OpenFgaConfiguration.SectionName));

        // Enregistrement du service d'autorisation
        services.TryAddSingleton<IAuthorizationService, OpenFgaAuthorizationService>();

        return services;
    }

    /// <summary>
    /// Ajoute les services d'autorisation OpenFGA avec configuration personnalisée.
    /// </summary>
    /// <param name="services">Collection de services.</param>
    /// <param name="configureOptions">Action de configuration.</param>
    /// <returns>La collection de services pour chaînage.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddOpenFgaAuthorization(options =>
    /// {
    ///     options.ApiUrl = "http://openfga:8080";
    ///     options.StoreId = "my-store-id";
    ///     options.Enabled = true;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddOpenFgaAuthorization(
        this IServiceCollection services,
        Action<OpenFgaConfiguration> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        // Configuration
        services.Configure(configureOptions);

        // Enregistrement du service d'autorisation
        services.TryAddSingleton<IAuthorizationService, OpenFgaAuthorizationService>();

        return services;
    }

    /// <summary>
    /// Ajoute les services d'autorisation via client HTTP vers l'API Authorization.
    /// </summary>
    /// <param name="services">Collection de services.</param>
    /// <param name="configuration">Configuration de l'application.</param>
    /// <returns>La collection de services pour chaînage.</returns>
    /// <remarks>
    /// <para>
    /// Cette méthode configure un client HTTP avec résilience (retry, circuit breaker)
    /// pour communiquer avec l'API Authorization externe au lieu d'OpenFGA directement.
    /// </para>
    /// <para>
    /// Configuration requise dans appsettings.json :
    /// </para>
    /// <code>
    /// {
    ///   "Authorization": {
    ///     "BaseUrl": "http://authorization-api:5100",
    ///     "TimeoutSeconds": 30,
    ///     "MaxRetries": 3,
    ///     "Enabled": true
    ///   }
    /// }
    /// </code>
    /// </remarks>
    /// <example>
    /// <code>
    /// builder.Services.AddHttpAuthorization(builder.Configuration);
    /// </code>
    /// </example>
    public static IServiceCollection AddHttpAuthorization(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var config = configuration
            .GetSection(AuthorizationClientConfiguration.SectionName)
            .Get<AuthorizationClientConfiguration>() ?? new AuthorizationClientConfiguration();

        return services.AddHttpAuthorization(options =>
        {
            options.BaseUrl = config.BaseUrl;
            options.TimeoutSeconds = config.TimeoutSeconds;
            options.MaxRetries = config.MaxRetries;
            options.RetryDelayMilliseconds = config.RetryDelayMilliseconds;
            options.CacheDurationSeconds = config.CacheDurationSeconds;
            options.Enabled = config.Enabled;
            options.ApiKey = config.ApiKey;
        });
    }

    /// <summary>
    /// Ajoute les services d'autorisation via client HTTP avec configuration personnalisée.
    /// </summary>
    /// <param name="services">Collection de services.</param>
    /// <param name="configureOptions">Action de configuration.</param>
    /// <returns>La collection de services pour chaînage.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddHttpAuthorization(options =>
    /// {
    ///     options.BaseUrl = "http://authorization-api:5100";
    ///     options.TimeoutSeconds = 30;
    ///     options.MaxRetries = 3;
    ///     options.Enabled = true;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddHttpAuthorization(
        this IServiceCollection services,
        Action<AuthorizationClientConfiguration> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        // Configuration
        var config = new AuthorizationClientConfiguration();
        configureOptions(config);
        services.Configure(configureOptions);

        // Cache mémoire
        services.AddMemoryCache();

        // Configuration du client HTTP avec résilience Polly
        services.AddHttpClient<HttpAuthorizationService>(client =>
        {
            client.BaseAddress = new Uri(config.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            if (!string.IsNullOrWhiteSpace(config.ApiKey))
            {
                client.DefaultRequestHeaders.Add("X-Api-Key", config.ApiKey);
            }
        })
        .AddPolicyHandler(GetRetryPolicy(config))
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        // Enregistrement du service d'autorisation
        services.TryAddSingleton<IAuthorizationService, HttpAuthorizationService>();

        return services;
    }

    /// <summary>
    /// Utilise le middleware d'autorisation automatique.
    /// </summary>
    /// <param name="app">Application builder.</param>
    /// <returns>L'application builder pour chaînage.</returns>
    /// <remarks>
    /// <para>
    /// Ce middleware intercepte les requêtes vers les endpoints décorés avec
    /// <see cref="RequirePermissionAttribute"/> et vérifie automatiquement les permissions.
    /// </para>
    /// <para>
    /// À placer après UseAuthentication et UseRouting, mais avant UseEndpoints.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// app.UseAuthentication();
    /// app.UseRouting();
    /// app.UseAuthorizationMiddleware(); // Vérification automatique des permissions
    /// app.UseEndpoints(...);
    /// </code>
    /// </example>
    public static IApplicationBuilder UseAuthorizationMiddleware(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<AuthorizationMiddleware>();
    }

    /// <summary>
    /// Crée la politique de retry avec jitter exponentiel.
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(AuthorizationClientConfiguration config)
    {
        var jitterRandom = new Random();

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                config.MaxRetries,
                retryAttempt =>
                {
                    // Backoff exponentiel avec jitter
                    var exponentialDelay = TimeSpan.FromMilliseconds(
                        config.RetryDelayMilliseconds * Math.Pow(2, retryAttempt - 1));
                    var jitter = TimeSpan.FromMilliseconds(jitterRandom.Next(0, 100));
                    return exponentialDelay + jitter;
                });
    }

    /// <summary>
    /// Crée la politique de circuit breaker.
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30));
    }

    /// <summary>
    /// Ajoute le health check OpenFGA.
    /// </summary>
    /// <param name="builder">Builder des health checks.</param>
    /// <param name="name">Nom du health check.</param>
    /// <param name="tags">Tags pour le health check.</param>
    /// <returns>Le builder pour chaînage.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddHealthChecks()
    ///     .AddOpenFgaHealthCheck();
    /// </code>
    /// </example>
    public static IHealthChecksBuilder AddOpenFgaHealthCheck(
        this IHealthChecksBuilder builder,
        string name = "openfga",
        params string[] tags)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddCheck<OpenFgaHealthCheck>(
            name,
            tags: tags.Length > 0 ? tags : new[] { "ready", "authorization" });
    }
}
