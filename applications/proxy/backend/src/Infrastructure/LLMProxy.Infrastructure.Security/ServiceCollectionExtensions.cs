using LLMProxy.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LLMProxy.Infrastructure.Security;

/// <summary>
/// Extensions de configuration pour l'injection de dépendances des services de sécurité.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Ajoute les services de sécurité au conteneur d'injection de dépendances.
    /// </summary>
    /// <param name="services">Collection de services à enrichir.</param>
    /// <param name="configuration">Configuration de l'application (actuellement non utilisée).</param>
    /// <returns>La collection de services enrichie pour permettre le chaînage fluent.</returns>
    /// <remarks>
    /// Enregistre <see cref="ISecretService"/> comme singleton pour la gestion sécurisée des secrets.
    /// </remarks>
    public static IServiceCollection AddSecurityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ISecretService, SecretService>();

        return services;
    }
}
