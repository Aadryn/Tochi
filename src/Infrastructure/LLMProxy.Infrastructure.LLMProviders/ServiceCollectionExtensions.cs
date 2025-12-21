using LLMProxy.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LLMProxy.Infrastructure.LLMProviders;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLLMProviderInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ITokenCounterService, TokenCounterService>();

        return services;
    }
}
