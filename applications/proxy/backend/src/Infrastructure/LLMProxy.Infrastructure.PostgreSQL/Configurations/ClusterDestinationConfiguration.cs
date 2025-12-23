using LLMProxy.Domain.Entities.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LLMProxy.Infrastructure.PostgreSQL.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité <see cref="ClusterDestination"/>.
/// </summary>
/// <remarks>
/// <para>
/// Configure la table <c>routing.cluster_destinations</c> avec les colonnes suivantes :
/// <list type="bullet">
/// <item><c>id</c> - Identifiant technique (GUID)</item>
/// <item><c>cluster_id</c> - Référence au cluster parent (FK)</item>
/// <item><c>destination_id</c> - Identifiant métier YARP</item>
/// <item><c>address</c> - URL du backend</item>
/// <item><c>health</c> - État de santé (Healthy/Unhealthy/Unknown)</item>
/// <item><c>weight</c> - Poids pour le load balancing</item>
/// <item><c>is_enabled</c> - Activation/désactivation</item>
/// <item><c>metadata</c> - Métadonnées additionnelles (JSON)</item>
/// </list>
/// </para>
/// <para>
/// <strong>Relations :</strong>
/// Une destination appartient à un seul <see cref="ProxyCluster"/> (Many-to-One).
/// </para>
/// </remarks>
public class ClusterDestinationConfiguration : IEntityTypeConfiguration<ClusterDestination>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ClusterDestination> builder)
    {
        // Table et schéma
        builder.ToTable("cluster_destinations", "routing");

        // Clé primaire
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        // Cluster ID (clé étrangère)
        builder.Property(d => d.ClusterId)
            .HasColumnName("cluster_id")
            .IsRequired();

        builder.HasIndex(d => d.ClusterId)
            .HasDatabaseName("ix_cluster_destinations_cluster_id");

        // Destination ID (identifiant métier YARP)
        builder.Property(d => d.DestinationId)
            .HasColumnName("destination_id")
            .IsRequired()
            .HasMaxLength(100);

        // Index unique composite (cluster_id + destination_id)
        builder.HasIndex(d => new { d.ClusterId, d.DestinationId })
            .IsUnique()
            .HasDatabaseName("ix_cluster_destinations_cluster_destination");

        // Address (URL du backend)
        builder.Property(d => d.Address)
            .HasColumnName("address")
            .IsRequired()
            .HasMaxLength(500);

        // Health (état de santé)
        builder.Property(d => d.Health)
            .HasColumnName("health")
            .HasMaxLength(20)
            .HasDefaultValue("Unknown");

        // Weight (poids pour load balancing)
        builder.Property(d => d.Weight)
            .HasColumnName("weight")
            .HasDefaultValue(1);

        // Is Enabled
        builder.Property(d => d.IsEnabled)
            .HasColumnName("is_enabled")
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(d => d.IsEnabled)
            .HasDatabaseName("ix_cluster_destinations_is_enabled")
            .HasFilter("is_enabled = true");

        // Metadata (JSON)
        builder.Property(d => d.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb");

        // Timestamps
        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at");
    }
}
