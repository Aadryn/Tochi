using Authorization.Infrastructure.Redis.Configuration;
using Authorization.Infrastructure.Redis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Authorization.Infrastructure.Redis;

/// <summary>
/// Extensions pour l'enregistrement des services Redis de cache.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Ajoute les services Redis de cache au conteneur DI.
    /// </summary>
    /// <param name="services">Collection de services.</param>
    /// <param name="configuration">Configuration.</param>
    /// <returns>Collection de services pour chaînage.</returns>
    public static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = new RedisCacheOptions();
        configuration.GetSection(RedisCacheOptions.SectionName).Bind(options);

        services.Configure<RedisCacheOptions>(
            configuration.GetSection(RedisCacheOptions.SectionName));

        if (options.Enabled)
        {
            services.AddStackExchangeRedisCache(redisOptions =>
            {
                redisOptions.Configuration = options.ConnectionString;
                redisOptions.InstanceName = options.InstanceName;
            });
        }

        services.AddSingleton<IPermissionCacheService, PermissionCacheService>();

        return services;
    }

    /// <summary>
    /// Ajoute les services Redis de cache avec configuration personnalisée.
    /// </summary>
    /// <param name="services">Collection de services.</param>
    /// <param name="connectionString">Chaîne de connexion Redis.</param>
    /// <param name="configure">Action de configuration optionnelle.</param>
    /// <returns>Collection de services pour chaînage.</returns>
    public static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        string connectionString,
        Action<RedisCacheOptions>? configure = null)
    {
        var options = new RedisCacheOptions { ConnectionString = connectionString };
        configure?.Invoke(options);

        services.Configure<RedisCacheOptions>(opt =>
        {
            opt.ConnectionString = options.ConnectionString;
            opt.InstanceName = options.InstanceName;
            opt.Enabled = options.Enabled;
            opt.PermissionCheckTtlSeconds = options.PermissionCheckTtlSeconds;
            opt.PrincipalTtlSeconds = options.PrincipalTtlSeconds;
            opt.RoleDefinitionTtlSeconds = options.RoleDefinitionTtlSeconds;
            opt.RoleAssignmentTtlSeconds = options.RoleAssignmentTtlSeconds;
            opt.ConnectTimeoutMs = options.ConnectTimeoutMs;
            opt.SyncTimeoutMs = options.SyncTimeoutMs;
            opt.ConnectRetry = options.ConnectRetry;
            opt.AbortOnConnectFail = options.AbortOnConnectFail;
        });

        if (options.Enabled)
        {
            services.AddStackExchangeRedisCache(redisOptions =>
            {
                redisOptions.Configuration = options.ConnectionString;
                redisOptions.InstanceName = options.InstanceName;
            });
        }

        services.AddSingleton<IPermissionCacheService, PermissionCacheService>();

        return services;
    }
}
