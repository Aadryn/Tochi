using LLMProxy.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LLMProxy.Infrastructure.Security;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSecurityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ISecretService, SecretService>();

        return services;
    }
}
