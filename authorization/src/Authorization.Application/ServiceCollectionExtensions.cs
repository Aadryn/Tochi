using Authorization.Application.Services;
using FluentValidation;
using MediatR;
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
}
