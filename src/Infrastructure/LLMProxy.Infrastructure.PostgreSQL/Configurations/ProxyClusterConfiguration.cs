using LLMProxy.Domain.Entities.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LLMProxy.Infrastructure.PostgreSQL.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité <see cref="ProxyCluster"/>.
/// </summary>
/// <remarks>
/// <para>
/// Configure la table <c>routing.proxy_clusters</c> avec les colonnes suivantes :
/// <list type="bullet">
/// <item><c>id</c> - Identifiant technique (GUID)</item>
/// <item><c>cluster_id</c> - Identifiant métier YARP (unique)</item>
/// <item><c>tenant_id</c> - Tenant propriétaire (multi-tenant)</item>
/// <item><c>load_balancing_policy</c> - Politique de répartition de charge</item>
/// <item><c>health_check</c> - Configuration du health check (JSON)</item>
/// <item><c>http_client</c> - Configuration du client HTTP (JSON)</item>
/// <item><c>session_affinity</c> - Configuration de l'affinité de session (JSON)</item>
/// <item><c>is_enabled</c> - Activation/désactivation</item>
/// </list>
/// </para>
/// <para>
/// <strong>Relations :</strong>
/// Un cluster possède une collection de <see cref="ClusterDestination"/> (One-to-Many).
/// </para>
/// </remarks>
public class ProxyClusterConfiguration : IEntityTypeConfiguration<ProxyCluster>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProxyCluster> builder)
    {
        // Table et schéma
        builder.ToTable("proxy_clusters", "routing");

        // Clé primaire
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        // Cluster ID (identifiant métier YARP)
        builder.Property(c => c.ClusterId)
            .HasColumnName("cluster_id")
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(c => c.ClusterId)
            .IsUnique()
            .HasDatabaseName("ix_proxy_clusters_cluster_id");

        // Tenant ID (multi-tenant)
        builder.Property(c => c.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.HasIndex(c => c.TenantId)
            .HasDatabaseName("ix_proxy_clusters_tenant_id");

        // Load Balancing Policy
        builder.Property(c => c.LoadBalancingPolicy)
            .HasColumnName("load_balancing_policy")
            .HasMaxLength(50)
            .HasDefaultValue("RoundRobin");

        // Is Enabled
        builder.Property(c => c.IsEnabled)
            .HasColumnName("is_enabled")
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(c => c.IsEnabled)
            .HasDatabaseName("ix_proxy_clusters_is_enabled")
            .HasFilter("is_enabled = true");

        // Health Check Configuration (Owned Type)
        builder.OwnsOne(c => c.HealthCheck, hc =>
        {
            hc.Property(h => h.Enabled)
                .HasColumnName("health_check_enabled")
                .HasDefaultValue(true);

            hc.Property(h => h.Interval)
                .HasColumnName("health_check_interval")
                .HasConversion(
                    v => v.TotalSeconds,
                    v => TimeSpan.FromSeconds(v))
                .HasDefaultValue(TimeSpan.FromSeconds(30));

            hc.Property(h => h.Timeout)
                .HasColumnName("health_check_timeout")
                .HasConversion(
                    v => v.TotalSeconds,
                    v => TimeSpan.FromSeconds(v))
                .HasDefaultValue(TimeSpan.FromSeconds(10));

            hc.Property(h => h.Path)
                .HasColumnName("health_check_path")
                .HasMaxLength(200)
                .HasDefaultValue("/health");

            hc.Property(h => h.Policy)
                .HasColumnName("health_check_policy")
                .HasMaxLength(50)
                .HasDefaultValue("ConsecutiveFailures");
        });

        // HTTP Client Configuration (Owned Type)
        builder.OwnsOne(c => c.HttpClient, client =>
        {
            client.Property(h => h.DangerousAcceptAnyServerCertificate)
                .HasColumnName("http_accept_any_certificate")
                .HasDefaultValue(false);

            client.Property(h => h.MaxConnectionsPerServer)
                .HasColumnName("http_max_connections_per_server")
                .HasDefaultValue(100);

            client.Property(h => h.EnableMultipleHttp2Connections)
                .HasColumnName("http_enable_multiple_http2")
                .HasDefaultValue(true);

            client.Property(h => h.RequestHeaderEncoding)
                .HasColumnName("http_request_header_encoding")
                .HasMaxLength(50);
        });

        // Session Affinity Configuration (Owned Type)
        builder.OwnsOne(c => c.SessionAffinity, sa =>
        {
            sa.Property(s => s.Enabled)
                .HasColumnName("session_affinity_enabled")
                .HasDefaultValue(false);

            sa.Property(s => s.Policy)
                .HasColumnName("session_affinity_policy")
                .HasMaxLength(50)
                .HasDefaultValue("Cookie");

            sa.Property(s => s.AffinityKeyName)
                .HasColumnName("session_affinity_key_name")
                .HasMaxLength(100)
                .HasDefaultValue(".Yarp.Affinity");
        });

        // Request Timeout
        builder.Property(c => c.RequestTimeout)
            .HasColumnName("request_timeout")
            .HasConversion(
                v => v.HasValue ? v.Value.TotalSeconds : (double?)null,
                v => v.HasValue ? TimeSpan.FromSeconds(v.Value) : null);

        // Timestamps
        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        // Relation One-to-Many avec ClusterDestination
        builder.HasMany(c => c.Destinations)
            .WithOne(d => d.Cluster)
            .HasForeignKey(d => d.ClusterId)
            .HasPrincipalKey(c => c.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
