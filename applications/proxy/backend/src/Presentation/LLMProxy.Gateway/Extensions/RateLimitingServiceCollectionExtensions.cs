using LLMProxy.Application.Interfaces;
using LLMProxy.Application.Services.RateLimiting;
using LLMProxy.Domain.Interfaces;
using LLMProxy.Infrastructure.Redis.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace LLMProxy.Presentation.Extensions;

/// <summary>
/// Extensions pour enregistrer les services de rate limiting dans le conteneur DI.
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-041 Rate Limiting et Throttling.
/// </para>
/// <para>
/// Cette classe fournit les méthodes d'extension pour configurer tous les services
/// nécessaires au fonctionnement du rate limiting distribué.
/// </para>
/// </remarks>
public static class RateLimitingServiceCollectionExtensions
{
    /// <summary>
    /// Enregistre tous les services de rate limiting dans le conteneur DI.
    /// </summary>
    /// <param name="services">Collection de services.</param>
    /// <param name="configuration">Configuration de l'application.</param>
    /// <returns>Collection de services pour chaînage fluent.</returns>
    /// <remarks>
    /// <para>
    /// Cette méthode configure :
    /// <list type="number">
    /// <item><description>Connexion Redis (singleton)</description></item>
    /// <item><description>IRateLimiter → RedisRateLimiter (scoped)</description></item>
    /// <item><description>IRateLimitConfigurationService → RateLimitConfigurationService (scoped)</description></item>
    /// <item><description>TokenBasedRateLimiter (scoped)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Configuration appsettings.json requise :</strong>
    /// </para>
    /// <code>
    /// {
    ///   "Redis": {
    ///     "ConnectionString": "localhost:6379",
    ///     "InstanceName": "llmproxy:"
    ///   }
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// <strong>Exemple d'utilisation dans Program.cs :</strong>
    /// </para>
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// 
    /// // Enregistrer les services de rate limiting
    /// builder.Services.AddRateLimiting(builder.Configuration);
    /// 
    /// var app = builder.Build();
    /// 
    /// // Activer le middleware de rate limiting
    /// app.UseRateLimiting();
    /// 
    /// app.Run();
    /// </code>
    /// </para>
    /// </remarks>
    public static IServiceCollection AddRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        // 1. Enregistrer Redis Connection (Singleton)
        var redisConnectionString = configuration["Redis:ConnectionString"] 
            ?? throw new InvalidOperationException("Redis:ConnectionString manquant dans la configuration");

        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var configOptions = ConfigurationOptions.Parse(redisConnectionString);
            configOptions.AbortOnConnectFail = false; // Tolérance aux pannes transitoires
            configOptions.ConnectRetry = 3;
            configOptions.ConnectTimeout = 5000; // 5 secondes
            configOptions.SyncTimeout = 5000;
            
            return ConnectionMultiplexer.Connect(configOptions);
        });

        // 2. Enregistrer IRateLimiter → RedisRateLimiter (Scoped)
        services.AddScoped<IRateLimiter, RedisRateLimiter>();

        // 3. Enregistrer IRateLimitConfigurationService → RateLimitConfigurationService (Scoped)
        services.AddScoped<IRateLimitConfigurationService, RateLimitConfigurationService>();

        // 4. Enregistrer TokenBasedRateLimiter (Scoped)
        services.AddScoped<TokenBasedRateLimiter>();

        return services;
    }
}
