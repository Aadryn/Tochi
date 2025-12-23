using Microsoft.OpenApi.Models;

namespace Authorization.API.Configuration;

/// <summary>
/// Configuration Swagger/OpenAPI pour l'API Authorization.
/// </summary>
public static class SwaggerConfiguration
{
    /// <summary>
    /// Ajoute la configuration Swagger aux services.
    /// </summary>
    /// <param name="services">Collection de services.</param>
    /// <returns>Collection de services pour chaînage.</returns>
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Authorization API",
                Version = "v1",
                Description = "API d'autorisation style Azure RBAC avec OpenFGA",
                Contact = new OpenApiContact
                {
                    Name = "LLMProxy Team"
                }
            });

            // Configuration de l'authentification Bearer JWT
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Token JWT obtenu depuis l'IDP (Keycloak). Format: Bearer {token}"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Inclure les commentaires XML
            var xmlFiles = Directory.GetFiles(
                AppContext.BaseDirectory,
                "Authorization.*.xml",
                SearchOption.TopDirectoryOnly);

            foreach (var xmlFile in xmlFiles)
            {
                options.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);
            }
        });

        return services;
    }
}
