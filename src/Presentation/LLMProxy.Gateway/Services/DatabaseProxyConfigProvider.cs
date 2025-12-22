using LLMProxy.Domain.Entities.Routing;
using LLMProxy.Domain.Interfaces;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;
using YarpHealthCheckConfig = Yarp.ReverseProxy.Configuration.HealthCheckConfig;
using YarpHttpClientConfig = Yarp.ReverseProxy.Configuration.HttpClientConfig;
using YarpSessionAffinityConfig = Yarp.ReverseProxy.Configuration.SessionAffinityConfig;

namespace LLMProxy.Gateway.Services;

/// <summary>
/// Provider de configuration YARP chargeant les routes et clusters depuis PostgreSQL.
/// </summary>
/// <remarks>
/// <para>
/// Ce provider implémente <see cref="IProxyConfigProvider"/> pour permettre
/// le chargement dynamique de la configuration YARP depuis la base de données.
/// </para>
/// <para>
/// <b>Fonctionnalités :</b>
/// <list type="bullet">
/// <item>Chargement initial depuis PostgreSQL au démarrage</item>
/// <item>Rechargement périodique (polling) configurable</item>
/// <item>Rechargement à la demande via <see cref="ReloadAsync"/></item>
/// <item>Support multi-tenant (filtrage par tenant)</item>
/// </list>
/// </para>
/// <para>
/// <b>Exemple d'utilisation :</b>
/// <code>
/// // Dans Program.cs
/// builder.Services.AddReverseProxy()
///     .LoadFromDatabase(builder.Configuration);
/// 
/// // Pour forcer un rechargement
/// var provider = app.Services.GetRequiredService&lt;DatabaseProxyConfigProvider&gt;();
/// await provider.ReloadAsync();
/// </code>
/// </para>
/// </remarks>
public class DatabaseProxyConfigProvider : IProxyConfigProvider, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DatabaseProxyConfigProvider> _logger;
    private readonly TimeSpan _pollingInterval;
    private readonly CancellationTokenSource _cts = new();
    private readonly object _lock = new();

    private volatile DatabaseProxyConfig _config;
    private CancellationTokenSource? _reloadCts = new();
    private Task? _pollingTask;

    /// <summary>
    /// Initialise une nouvelle instance du provider.
    /// </summary>
    /// <param name="scopeFactory">Factory pour créer des scopes DI.</param>
    /// <param name="logger">Logger pour les diagnostics.</param>
    /// <param name="pollingInterval">Intervalle de polling (défaut: 30 secondes).</param>
    public DatabaseProxyConfigProvider(
        IServiceScopeFactory scopeFactory,
        ILogger<DatabaseProxyConfigProvider> logger,
        TimeSpan? pollingInterval = null)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _pollingInterval = pollingInterval ?? TimeSpan.FromSeconds(30);

        // Configuration initiale vide
        _config = new DatabaseProxyConfig(
            Array.Empty<RouteConfig>(),
            Array.Empty<ClusterConfig>());

        // Démarrer le polling en arrière-plan
        _pollingTask = StartPollingAsync(_cts.Token);
    }

    /// <inheritdoc/>
    public IProxyConfig GetConfig() => _config;

    /// <summary>
    /// Force le rechargement de la configuration depuis la base de données.
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation.</param>
    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Rechargement de la configuration YARP depuis la base de données...");

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var routeRepository = scope.ServiceProvider.GetRequiredService<IProxyRouteRepository>();
            var clusterRepository = scope.ServiceProvider.GetRequiredService<IProxyClusterRepository>();

            // Charger les données depuis PostgreSQL
            var routes = await routeRepository.GetAllActiveAsync(cancellationToken);
            var clusters = await clusterRepository.GetAllActiveWithDestinationsAsync(cancellationToken);

            // Convertir vers les types YARP
            var routeConfigs = routes.Select(ConvertToRouteConfig).ToList();
            var clusterConfigs = clusters.Select(ConvertToClusterConfig).ToList();

            // Créer la nouvelle configuration
            var newConfig = new DatabaseProxyConfig(routeConfigs, clusterConfigs);

            // Signaler le changement
            lock (_lock)
            {
                var oldConfig = _config;
                _config = newConfig;
                oldConfig.SignalChange();
            }

            _logger.LogInformation(
                "Configuration YARP rechargée : {RouteCount} routes, {ClusterCount} clusters",
                routeConfigs.Count,
                clusterConfigs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du rechargement de la configuration YARP");
            throw;
        }
    }

    private async Task StartPollingAsync(CancellationToken cancellationToken)
    {
        // Attendre un peu avant le premier chargement pour laisser le temps à l'application de démarrer
        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

        // Charger la configuration initiale
        try
        {
            await ReloadAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Échec du chargement initial de la configuration YARP");
        }

        // Polling périodique
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_pollingInterval, cancellationToken);
                await ReloadAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erreur lors du polling de la configuration YARP, nouvelle tentative dans {Interval}", _pollingInterval);
            }
        }
    }

    private RouteConfig ConvertToRouteConfig(ProxyRoute route)
    {
        var match = new RouteMatch
        {
            Path = route.MatchPath,
            Methods = route.MatchMethods,
            Hosts = route.MatchHosts,
            Headers = route.MatchHeaders?.Select(h => new RouteHeader
            {
                Name = h.Key,
                Values = h.Value,
                Mode = HeaderMatchMode.ExactHeader
            }).ToList()
        };

        var metadata = route.Metadata ?? new Dictionary<string, string>();
        if (route.TenantId.HasValue)
        {
            metadata["TenantId"] = route.TenantId.Value.ToString();
        }

        return new RouteConfig
        {
            RouteId = route.RouteId,
            ClusterId = route.ClusterId,
            Match = match,
            Order = route.Order,
            AuthorizationPolicy = route.AuthorizationPolicy,
            RateLimiterPolicy = route.RateLimiterPolicy,
            Timeout = route.Timeout,
            Metadata = metadata.Count > 0 ? metadata : null,
            Transforms = route.Transforms?.Select(t => t.AsReadOnly()).ToList()
        };
    }

    private ClusterConfig ConvertToClusterConfig(ProxyCluster cluster)
    {
        var destinations = cluster.Destinations
            .Where(d => d.IsEnabled)
            .ToDictionary(
                d => d.DestinationId,
                d => new DestinationConfig
                {
                    Address = d.Address,
                    Health = d.Health,
                    Metadata = d.Metadata?.AsReadOnly()
                });

        YarpHealthCheckConfig? healthCheck = null;
        if (cluster.HealthCheck?.Enabled == true)
        {
            healthCheck = new YarpHealthCheckConfig
            {
                Active = new ActiveHealthCheckConfig
                {
                    Enabled = true,
                    Interval = cluster.HealthCheck.Interval,
                    Timeout = cluster.HealthCheck.Timeout,
                    Path = cluster.HealthCheck.Path,
                    Policy = cluster.HealthCheck.Policy
                }
            };
        }

        YarpHttpClientConfig? httpClient = null;
        if (cluster.HttpClient != null)
        {
            httpClient = new YarpHttpClientConfig
            {
                DangerousAcceptAnyServerCertificate = cluster.HttpClient.DangerousAcceptAnyServerCertificate,
                MaxConnectionsPerServer = cluster.HttpClient.MaxConnectionsPerServer,
                EnableMultipleHttp2Connections = cluster.HttpClient.EnableMultipleHttp2Connections
            };
        }

        YarpSessionAffinityConfig? sessionAffinity = null;
        if (cluster.SessionAffinity?.Enabled == true)
        {
            sessionAffinity = new YarpSessionAffinityConfig
            {
                Enabled = true,
                Policy = cluster.SessionAffinity.Policy,
                AffinityKeyName = cluster.SessionAffinity.AffinityKeyName
            };
        }

        var metadata = cluster.Metadata ?? new Dictionary<string, string>();
        if (cluster.TenantId.HasValue)
        {
            metadata["TenantId"] = cluster.TenantId.Value.ToString();
        }

        return new ClusterConfig
        {
            ClusterId = cluster.ClusterId,
            Destinations = destinations,
            LoadBalancingPolicy = cluster.LoadBalancingPolicy,
            HealthCheck = healthCheck,
            HttpClient = httpClient,
            SessionAffinity = sessionAffinity,
            Metadata = metadata.Count > 0 ? metadata.AsReadOnly() : null
        };
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        _reloadCts?.Dispose();
    }
}

/// <summary>
/// Configuration YARP immutable avec support de notification de changement.
/// </summary>
internal sealed class DatabaseProxyConfig : IProxyConfig
{
    private readonly CancellationTokenSource _cts = new();

    /// <summary>
    /// Crée une nouvelle configuration.
    /// </summary>
    public DatabaseProxyConfig(
        IReadOnlyList<RouteConfig> routes,
        IReadOnlyList<ClusterConfig> clusters)
    {
        Routes = routes;
        Clusters = clusters;
        ChangeToken = new CancellationChangeToken(_cts.Token);
    }

    /// <inheritdoc/>
    public IReadOnlyList<RouteConfig> Routes { get; }

    /// <inheritdoc/>
    public IReadOnlyList<ClusterConfig> Clusters { get; }

    /// <inheritdoc/>
    public IChangeToken ChangeToken { get; }

    /// <summary>
    /// Signale que cette configuration est obsolète.
    /// </summary>
    internal void SignalChange()
    {
        _cts.Cancel();
    }
}
