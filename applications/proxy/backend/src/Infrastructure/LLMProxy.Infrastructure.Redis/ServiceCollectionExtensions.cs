using LLMProxy.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace LLMProxy.Infrastructure.Redis;

/// <summary>
/// Extensions de configuration pour l'injection de dépendances des services Redis.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Ajoute les services Redis au conteneur d'injection de dépendances.
    /// </summary>
    /// <param name="services">Collection de services à enrichir.</param>
    /// <param name="configuration">Configuration de l'application contenant la chaîne de connexion Redis.</param>
    /// <returns>La collection de services enrichie pour permettre le chaînage fluent.</returns>
    /// <remarks>
    /// Enregistre <see cref="IConnectionMultiplexer"/> comme singleton avec configuration de résilience
    /// (3 tentatives de reconnexion, timeout de 5 secondes).
    /// Enregistre également <see cref="IQuotaService"/> et <see cref="ICacheService"/>.
    /// </remarks>
    public static IServiceCollection AddRedisInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Redis connection
        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configOptions = ConfigurationOptions.Parse(redisConnectionString);
            configOptions.AbortOnConnectFail = false;
            configOptions.ConnectRetry = 3;
            configOptions.ConnectTimeout = 5000;
            configOptions.SyncTimeout = 5000;
            
            return ConnectionMultiplexer.Connect(configOptions);
        });

        // Register services
        services.AddSingleton<IQuotaService, QuotaService>();
        services.AddSingleton<ICacheService, CacheService>();

        return services;
    }
}
