using LLMProxy.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace LLMProxy.Infrastructure.Redis;

public static class ServiceCollectionExtensions
{
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
