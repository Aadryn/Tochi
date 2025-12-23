using LLMProxy.Domain.Entities.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LLMProxy.Infrastructure.PostgreSQL.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité <see cref="ProxyRoute"/>.
/// </summary>
/// <remarks>
/// <para>
/// Configure la table <c>routing.proxy_routes</c> avec les colonnes suivantes :
/// <list type="bullet">
/// <item><c>id</c> - Identifiant technique (GUID)</item>
/// <item><c>route_id</c> - Identifiant métier YARP (unique)</item>
/// <item><c>cluster_id</c> - Référence au cluster cible</item>
/// <item><c>match_path</c> - Pattern de correspondance du chemin</item>
/// <item><c>match_methods</c> - Méthodes HTTP autorisées (JSON array)</item>
/// <item><c>match_headers</c> - Headers requis (JSON object)</item>
/// <item><c>transforms</c> - Transformations à appliquer (JSON object)</item>
/// <item><c>tenant_id</c> - Tenant propriétaire (multi-tenant)</item>
/// <item><c>order</c> - Ordre d'évaluation des routes</item>
/// <item><c>is_enabled</c> - Activation/désactivation</item>
/// <item><c>authorization_policy</c> - Politique d'autorisation</item>
/// <item><c>rate_limiter_policy</c> - Politique de rate limiting</item>
/// </list>
/// </para>
/// </remarks>
public class ProxyRouteConfiguration : IEntityTypeConfiguration<ProxyRoute>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProxyRoute> builder)
    {
        // Table et schéma
        builder.ToTable("proxy_routes", "routing");

        // Clé primaire
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        // Route ID (identifiant métier YARP)
        builder.Property(r => r.RouteId)
            .HasColumnName("route_id")
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(r => r.RouteId)
            .IsUnique()
            .HasDatabaseName("ix_proxy_routes_route_id");

        // Cluster ID (référence au cluster cible)
        builder.Property(r => r.ClusterId)
            .HasColumnName("cluster_id")
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(r => r.ClusterId)
            .HasDatabaseName("ix_proxy_routes_cluster_id");

        // Match Path (pattern de correspondance)
        builder.Property(r => r.MatchPath)
            .HasColumnName("match_path")
            .IsRequired()
            .HasMaxLength(500);

        // Match Methods (JSON array)
        builder.Property(r => r.MatchMethods)
            .HasColumnName("match_methods")
            .HasColumnType("jsonb");

        // Match Headers (JSON object)
        builder.Property(r => r.MatchHeaders)
            .HasColumnName("match_headers")
            .HasColumnType("jsonb");

        // Transforms (JSON object)
        builder.Property(r => r.Transforms)
            .HasColumnName("transforms")
            .HasColumnType("jsonb");

        // Tenant ID (multi-tenant)
        builder.Property(r => r.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.HasIndex(r => r.TenantId)
            .HasDatabaseName("ix_proxy_routes_tenant_id");

        // Index composite pour requêtes fréquentes (tenant + enabled)
        builder.HasIndex(r => new { r.TenantId, r.IsEnabled })
            .HasDatabaseName("ix_proxy_routes_tenant_enabled");

        // Order (priorité d'évaluation)
        builder.Property(r => r.Order)
            .HasColumnName("order")
            .IsRequired()
            .HasDefaultValue(0);

        // Is Enabled
        builder.Property(r => r.IsEnabled)
            .HasColumnName("is_enabled")
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(r => r.IsEnabled)
            .HasDatabaseName("ix_proxy_routes_is_enabled")
            .HasFilter("is_enabled = true");

        // Authorization Policy
        builder.Property(r => r.AuthorizationPolicy)
            .HasColumnName("authorization_policy")
            .HasMaxLength(100);

        // Rate Limiter Policy
        builder.Property(r => r.RateLimiterPolicy)
            .HasColumnName("rate_limiter_policy")
            .HasMaxLength(100);

        // Timestamps
        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at");
    }
}
