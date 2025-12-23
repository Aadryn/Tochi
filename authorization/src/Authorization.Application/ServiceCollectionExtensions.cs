using Authorization.Application.Jobs;
using Authorization.Application.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Authorization.Application;

/// <summary>
/// Extensions pour l'enregistrement des services de la couche Application.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Ajoute les services de la couche Application.
    /// </summary>
    /// <param name="services">Collection de services.</param>
    /// <returns>Collection de services pour chaînage.</returns>
    public static IServiceCollection AddAuthorizationApplication(this IServiceCollection services)
    {
        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Services
        services.AddScoped<IRbacAuthorizationService, AuthorizationService>();

        return services;
    }

    /// <summary>
    /// Ajoute le job de nettoyage des assignations expirées.
    /// </summary>
    /// <param name="services">Collection de services.</param>
    /// <param name="configuration">Configuration.</param>
    /// <returns>Collection de services pour chaînage.</returns>
    /// <remarks>
    /// <para>
    /// Ce job s'exécute en arrière-plan et supprime automatiquement
    /// les assignations de rôles dont la date d'expiration est dépassée.
    /// </para>
    /// <para>
    /// Configuration dans appsettings.json :
    /// </para>
    /// <code>
    /// {
    ///   "ExpirationCleanup": {
    ///     "IntervalMinutes": 5,
    ///     "BatchSize": 100,
    ///     "Enabled": true
    ///   }
    /// }
    /// </code>
    /// </remarks>
    public static IServiceCollection AddExpirationCleanup(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ExpirationCleanupOptions>(
            configuration.GetSection(ExpirationCleanupOptions.SectionName));

        services.AddHostedService<ExpirationCleanupJob>();

        return services;
    }
}
