using Authorization.Infrastructure.OpenFGA.Configuration;
using Authorization.Infrastructure.OpenFGA.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Authorization.Infrastructure.OpenFGA;

/// <summary>
/// Extensions pour l'enregistrement des services OpenFGA.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Ajoute les services OpenFGA au conteneur DI.
    /// </summary>
    /// <param name="services">Collection de services.</param>
    /// <param name="configuration">Configuration.</param>
    /// <returns>Collection de services pour chaînage.</returns>
    public static IServiceCollection AddOpenFgaAuthorization(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<OpenFgaOptions>(
            configuration.GetSection(OpenFgaOptions.SectionName));

        services.AddSingleton<IOpenFgaStoreProvider, OpenFgaStoreProvider>();
        services.AddSingleton<IOpenFgaService, OpenFgaService>();

        return services;
    }

    /// <summary>
    /// Ajoute les services OpenFGA avec configuration personnalisée.
    /// </summary>
    /// <param name="services">Collection de services.</param>
    /// <param name="configure">Action de configuration.</param>
    /// <returns>Collection de services pour chaînage.</returns>
    public static IServiceCollection AddOpenFgaAuthorization(
        this IServiceCollection services,
        Action<OpenFgaOptions> configure)
    {
        services.Configure(configure);

        services.AddSingleton<IOpenFgaStoreProvider, OpenFgaStoreProvider>();
        services.AddSingleton<IOpenFgaService, OpenFgaService>();

        return services;
    }
}
