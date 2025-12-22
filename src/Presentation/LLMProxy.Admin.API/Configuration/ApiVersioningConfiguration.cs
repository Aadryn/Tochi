using Asp.Versioning;

namespace LLMProxy.Admin.API.Configuration;

/// <summary>
/// Configuration centralisée du versioning d'API REST pour Admin API.
/// Conforme à ADR-037 (API Versioning Strategy).
/// </summary>
/// <remarks>
/// Stratégie choisie : URL-based versioning (`/api/v{version}/...`)
/// - Plus visible et explicite pour les clients
/// - Cache-friendly (différentes URLs = différents caches)
/// - SEO-friendly pour documentation API
/// - Support multi-version simultané en production
/// </remarks>
public static class ApiVersioningConfiguration
{
    /// <summary>
    /// Configure le versioning d'API avec stratégie URL-based.
    /// </summary>
    /// <param name="services">Collection de services.</param>
    /// <returns>Collection de services pour chaînage.</returns>
    /// <remarks>
    /// Configuration appliquée :
    /// - Version par défaut : 1.0 (backward compatibility)
    /// - Versioning par URL : `/api/v{version}/resource`
    /// - Headers de version exposés : `api-supported-versions`, `api-deprecated-versions`
    /// - Assume version par défaut si non spécifiée
    /// </remarks>
    public static IServiceCollection AddApiVersioningConfiguration(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            // Stratégie de lecture : URL Segment (/api/v1/users, /api/v2/users)
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
            
            // Version par défaut si client ne spécifie pas de version
            // Permet backward compatibility pour clients existants
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            
            // Reporter les versions supportées et dépréciées dans response headers
            // Headers ajoutés : api-supported-versions, api-deprecated-versions
            options.ReportApiVersions = true;
        })
        .AddApiExplorer(options =>
        {
            // Format du nom de groupe pour Swagger : 'v'major[.minor][-status]
            // Exemple : v1, v2, v2.1
            options.GroupNameFormat = "'v'VVV";
            
            // Substituer {version:apiVersion} dans routes par valeur réelle
            // /api/v{version:apiVersion}/users → /api/v1/users
            options.SubstituteApiVersionInUrl = true;
        });
        
        return services;
    }
}
