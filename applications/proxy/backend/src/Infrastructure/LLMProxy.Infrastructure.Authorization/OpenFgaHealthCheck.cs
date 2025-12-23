using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenFga.Sdk.Client;

namespace LLMProxy.Infrastructure.Authorization;

/// <summary>
/// Health check pour le service OpenFGA.
/// </summary>
/// <remarks>
/// Vérifie la connectivité au serveur OpenFGA et la validité du store configuré.
/// </remarks>
public sealed class OpenFgaHealthCheck : IHealthCheck, IDisposable
{
    private readonly OpenFgaConfiguration _config;
    private readonly ILogger<OpenFgaHealthCheck> _logger;
    private readonly OpenFgaClient? _client;
    private bool _disposed;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="OpenFgaHealthCheck"/>.
    /// </summary>
    /// <param name="options">Options de configuration OpenFGA.</param>
    /// <param name="logger">Logger pour les traces.</param>
    public OpenFgaHealthCheck(
        IOptions<OpenFgaConfiguration> options,
        ILogger<OpenFgaHealthCheck> logger)
    {
        _config = options.Value;
        _logger = logger;

        if (_config.Enabled && !string.IsNullOrEmpty(_config.StoreId))
        {
            var clientConfig = new ClientConfiguration
            {
                ApiUrl = _config.ApiUrl,
                StoreId = _config.StoreId
            };

            _client = new OpenFgaClient(clientConfig);
        }
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (!_config.Enabled)
        {
            return HealthCheckResult.Healthy("OpenFGA désactivé");
        }

        if (string.IsNullOrEmpty(_config.StoreId))
        {
            return HealthCheckResult.Degraded(
                "OpenFGA activé mais StoreId non configuré. Exécutez init-openfga.sh.");
        }

        if (_client is null)
        {
            return HealthCheckResult.Unhealthy("Client OpenFGA non initialisé");
        }

        try
        {
            // Vérifie que le store existe et est accessible
            var store = await _client.GetStore(null, cancellationToken)
                .ConfigureAwait(false);

            var data = new Dictionary<string, object>
            {
                ["store_id"] = store.Id ?? "inconnu",
                ["store_name"] = store.Name ?? "inconnu",
                ["api_url"] = _config.ApiUrl
            };

            _logger.LogDebug(
                "OpenFGA health check OK - Store: {StoreName} ({StoreId})",
                store.Name,
                store.Id);

            return HealthCheckResult.Healthy(
                $"OpenFGA opérationnel - Store: {store.Name}",
                data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenFGA health check failed");

            return HealthCheckResult.Unhealthy(
                $"OpenFGA inaccessible: {ex.Message}",
                ex);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _client?.Dispose();
        _disposed = true;
    }
}
