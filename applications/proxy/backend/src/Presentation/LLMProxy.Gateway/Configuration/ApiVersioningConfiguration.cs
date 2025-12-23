using Asp.Versioning;
using Asp.Versioning.Conventions;

namespace LLMProxy.Gateway.Configuration;

/// <summary>
/// Configuration du versioning d'API avec détection par namespace
/// </summary>
/// <remarks>
/// Stratégie : Namespace-based versioning avec fallbacks multiples
/// - Détection automatique depuis namespace
/// - Formats supportés : v{major}.{minor}, v{year}, v{year}-{month}-{day}
/// - Readers : UrlSegment, Header (X-Api-Version), QueryString (api-version)
/// - Routes : /api/v{version:apiVersion}/[controller]
/// </remarks>
public static class ApiVersioningConfiguration
{
    /// <summary>
    /// Configure le versioning d'API avec namespace convention
    /// </summary>
    public static IServiceCollection AddApiVersioningWithNamespaceConvention(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            // Détection par namespace + URL + header + query string
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Api-Version"),
                new QueryStringApiVersionReader("api-version")
            );

            // Version par défaut : date UTC actuelle
            options.DefaultApiVersion = new ApiVersion(DateOnly.FromDateTime(DateTime.UtcNow));
            options.AssumeDefaultVersionWhenUnspecified = true;

            // Inclure version dans headers de réponse
            options.ReportApiVersions = true;
        })
        .AddMvc(options =>
        {
            // Convention : Détection automatique depuis namespace
            options.Conventions.Add(new VersionByNamespaceConvention());
        })
        .AddApiExplorer(options =>
        {
            // Format d'affichage pour Swagger (v2025-12-22)
            options.GroupNameFormat = "'v'yyyy-MM-dd";
            
            // Activer version dans URL
            options.SubstituteApiVersionInUrl = true;
        });
        
        return services;
    }
}
