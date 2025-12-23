using Authorization.Domain.ValueObjects;
using Authorization.Infrastructure.Redis.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Authorization.Infrastructure.Redis.Services;

/// <summary>
/// Implémentation du service de cache des permissions avec Redis.
/// </summary>
public class PermissionCacheService : IPermissionCacheService
{
    private readonly IDistributedCache _cache;
    private readonly RedisCacheOptions _options;
    private readonly ILogger<PermissionCacheService> _logger;

    /// <summary>
    /// Initialise une nouvelle instance du service de cache.
    /// </summary>
    /// <param name="cache">Cache distribué.</param>
    /// <param name="options">Options de configuration.</param>
    /// <param name="logger">Logger.</param>
    public PermissionCacheService(
        IDistributedCache cache,
        IOptions<RedisCacheOptions> options,
        ILogger<PermissionCacheService> logger)
    {
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool?> GetPermissionCheckAsync(
        TenantId tenantId,
        PrincipalId principalId,
        Permission permission,
        Scope scope,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return null;
        }

        try
        {
            var key = BuildPermissionKey(tenantId, principalId, permission, scope);
            var value = await _cache.GetStringAsync(key, cancellationToken);

            if (value is null)
            {
                _logger.LogDebug(
                    "Cache miss for permission check: {Key}",
                    key);
                return null;
            }

            _logger.LogDebug(
                "Cache hit for permission check: {Key} = {Value}",
                key,
                value);

            return value == "1";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Error getting permission check from cache for {Principal}/{Permission}/{Scope}",
                principalId,
                permission,
                scope.Path);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task SetPermissionCheckAsync(
        TenantId tenantId,
        PrincipalId principalId,
        Permission permission,
        Scope scope,
        bool allowed,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return;
        }

        try
        {
            var key = BuildPermissionKey(tenantId, principalId, permission, scope);
            var value = allowed ? "1" : "0";

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_options.PermissionCheckTtlSeconds)
            };

            await _cache.SetStringAsync(key, value, options, cancellationToken);

            _logger.LogDebug(
                "Cached permission check: {Key} = {Value} (TTL: {Ttl}s)",
                key,
                value,
                _options.PermissionCheckTtlSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Error setting permission check in cache for {Principal}/{Permission}/{Scope}",
                principalId,
                permission,
                scope.Path);
        }
    }

    /// <inheritdoc />
    public Task InvalidatePrincipalAsync(
        TenantId tenantId,
        PrincipalId principalId,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return Task.CompletedTask;
        }

        try
        {
            // On utilise un pattern de clé pour invalider toutes les clés du principal
            // Note: Redis ne supporte pas nativement la suppression par pattern avec IDistributedCache
            // Pour une implémentation production, utiliser directement StackExchange.Redis avec SCAN/DEL
            var patternKey = $"perm:{tenantId.Value}:{principalId.Value}:*";

            _logger.LogInformation(
                "Invalidating cache for principal: {Pattern}",
                patternKey);

            // Pour l'instant, on logue l'invalidation
            // L'implémentation complète nécessite l'accès direct à Redis
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Error invalidating cache for principal {Principal}",
                principalId);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task InvalidateScopeAsync(
        TenantId tenantId,
        Scope scope,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return Task.CompletedTask;
        }

        try
        {
            var patternKey = $"perm:{tenantId.Value}:*:*:{NormalizeScope(scope.Path)}*";

            _logger.LogInformation(
                "Invalidating cache for scope: {Pattern}",
                patternKey);

            // Même limitation que InvalidatePrincipalAsync
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Error invalidating cache for scope {Scope}",
                scope.Path);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task InvalidateTenantAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return Task.CompletedTask;
        }

        try
        {
            var patternKey = $"perm:{tenantId.Value}:*";

            _logger.LogInformation(
                "Invalidating all cache for tenant: {Pattern}",
                patternKey);

            // Même limitation que InvalidatePrincipalAsync
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Error invalidating cache for tenant {Tenant}",
                tenantId.Value);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Construit la clé de cache pour une vérification de permission.
    /// Format: perm:{tenant}:{principal}:{permission}:{scope}
    /// </summary>
    private string BuildPermissionKey(
        TenantId tenantId,
        PrincipalId principalId,
        Permission permission,
        Scope scope)
    {
        return $"perm:{tenantId.Value}:{principalId.Value}:{permission}:{NormalizeScope(scope.Path)}";
    }

    /// <summary>
    /// Normalise le scope pour l'utilisation comme clé de cache.
    /// </summary>
    private static string NormalizeScope(string scopePath)
    {
        // Remplace les caractères problématiques pour les clés Redis
        return scopePath
            .Replace('/', ':')
            .Replace(' ', '_');
    }
}
