using LLMProxy.Gateway.Services;
using Yarp.ReverseProxy.Configuration;

namespace LLMProxy.Gateway.Extensions;

/// <summary>
/// Extensions pour configurer YARP avec une source de configuration dynamique.
/// </summary>
/// <remarks>
/// <para>
/// Ces extensions permettent de remplacer la configuration statique YARP
/// (appsettings.json) par une configuration chargée depuis PostgreSQL.
/// </para>
/// <para>
/// <b>Exemple d'utilisation :</b>
/// <code>
/// // Program.cs
/// var builder = WebApplication.CreateBuilder(args);
/// 
/// // Option 1 : Configuration dynamique depuis la base de données
/// builder.Services.AddReverseProxy()
///     .LoadFromDatabase(builder.Configuration);
/// 
/// // Option 2 : Configuration hybride (DB + fallback fichier)
/// builder.Services.AddReverseProxy()
///     .LoadFromDatabase(builder.Configuration)
///     .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
/// </code>
/// </para>
/// </remarks>
public static class YarpDynamicConfigExtensions
{
    /// <summary>
    /// Configure YARP pour charger sa configuration depuis PostgreSQL.
    /// </summary>
    /// <param name="builder">Builder YARP.</param>
    /// <param name="configuration">Configuration de l'application.</param>
    /// <returns>Le builder pour le chaînage.</returns>
    /// <remarks>
    /// <para>
    /// La configuration utilise les paramètres suivants :
    /// <list type="bullet">
    /// <item><c>Yarp:Database:PollingIntervalSeconds</c> - Intervalle de polling (défaut: 30)</item>
    /// <item><c>Yarp:Database:Enabled</c> - Active/désactive le chargement DB (défaut: true)</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static IReverseProxyBuilder LoadFromDatabase(
        this IReverseProxyBuilder builder,
        IConfiguration configuration)
    {
        var enabled = configuration.GetValue("Yarp:Database:Enabled", true);

        if (!enabled)
        {
            return builder;
        }

        var pollingIntervalSeconds = configuration.GetValue("Yarp:Database:PollingIntervalSeconds", 30);
        var pollingInterval = TimeSpan.FromSeconds(pollingIntervalSeconds);

        builder.Services.AddSingleton<IProxyConfigProvider>(sp =>
        {
            var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
            var logger = sp.GetRequiredService<ILogger<DatabaseProxyConfigProvider>>();

            return new DatabaseProxyConfigProvider(scopeFactory, logger, pollingInterval);
        });

        return builder;
    }

    /// <summary>
    /// Ajoute un endpoint pour recharger manuellement la configuration YARP.
    /// </summary>
    /// <param name="app">Application builder.</param>
    /// <param name="path">Chemin de l'endpoint (défaut: /admin/yarp/reload).</param>
    /// <returns>L'application builder pour le chaînage.</returns>
    /// <remarks>
    /// <para>
    /// Cet endpoint permet de forcer un rechargement de la configuration
    /// sans attendre le prochain cycle de polling.
    /// </para>
    /// <para>
    /// <b>Sécurité :</b> Cet endpoint devrait être protégé par une policy
    /// d'autorisation appropriée (ex: RequireAdmin).
    /// </para>
    /// </remarks>
    public static IEndpointRouteBuilder MapYarpReloadEndpoint(
        this IEndpointRouteBuilder endpoints,
        string path = "/admin/yarp/reload")
    {
        endpoints.MapPost(path, async (HttpContext context) =>
        {
            var provider = context.RequestServices.GetService<IProxyConfigProvider>();

            if (provider is DatabaseProxyConfigProvider dbProvider)
            {
                await dbProvider.ReloadAsync(context.RequestAborted);
                return Results.Ok(new { Message = "Configuration YARP rechargée avec succès" });
            }

            return Results.BadRequest(new { Error = "Le provider de configuration dynamique n'est pas actif" });
        })
        .WithName("ReloadYarpConfig")
        .WithTags("Administration")
        .RequireAuthorization("RequireAdmin");

        return endpoints;
    }

    /// <summary>
    /// Ajoute un endpoint pour consulter la configuration YARP actuelle.
    /// </summary>
    /// <param name="endpoints">Endpoint route builder.</param>
    /// <param name="path">Chemin de l'endpoint (défaut: /admin/yarp/config).</param>
    /// <returns>L'endpoint route builder pour le chaînage.</returns>
    public static IEndpointRouteBuilder MapYarpConfigEndpoint(
        this IEndpointRouteBuilder endpoints,
        string path = "/admin/yarp/config")
    {
        endpoints.MapGet(path, (HttpContext context) =>
        {
            var provider = context.RequestServices.GetRequiredService<IProxyConfigProvider>();
            var config = provider.GetConfig();

            return Results.Ok(new
            {
                Routes = config.Routes.Select(r => new
                {
                    r.RouteId,
                    r.ClusterId,
                    Match = new
                    {
                        r.Match.Path,
                        r.Match.Methods,
                        r.Match.Hosts
                    },
                    r.Order,
                    r.AuthorizationPolicy,
                    r.Metadata
                }),
                Clusters = config.Clusters.Select(c => new
                {
                    c.ClusterId,
                    c.LoadBalancingPolicy,
                    Destinations = c.Destinations?.Select(d => new
                    {
                        Id = d.Key,
                        d.Value.Address,
                        d.Value.Health
                    }),
                    HealthCheckEnabled = c.HealthCheck?.Active?.Enabled ?? false
                })
            });
        })
        .WithName("GetYarpConfig")
        .WithTags("Administration")
        .RequireAuthorization("RequireAdmin");

        return endpoints;
    }
}
