using LLMProxy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LLMProxy.Infrastructure.PostgreSQL.Configurations;

/// <summary>
/// Configuration Entity Framework Core pour l'entité <see cref="ApiKey"/>.
/// </summary>
/// <remarks>
/// Définit le schéma de table, les index, les contraintes et les relations pour la table <c>api_keys</c>.
/// Configure le hachage des clés d'API et les règles de validation.
/// </remarks>
public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    /// <summary>
    /// Configure le mapping de l'entité <see cref="ApiKey"/> vers la base de données PostgreSQL.
    /// </summary>
    /// <param name="builder">Constructeur de configuration pour définir les propriétés, index et relations.</param>
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable("api_keys");

        builder.HasKey(k => k.Id);

        builder.Property(k => k.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(k => k.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(k => k.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(k => k.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(k => k.KeyHash)
            .HasColumnName("key_hash")
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(k => k.KeyPrefix)
            .HasColumnName("key_prefix")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(k => k.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(k => k.ExpiresAt)
            .HasColumnName("expires_at");

        builder.Property(k => k.RevokedAt)
            .HasColumnName("revoked_at");

        builder.Property(k => k.LastUsedAt)
            .HasColumnName("last_used_at");

        builder.Property(k => k.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(k => k.UpdatedAt)
            .HasColumnName("updated_at");

        // Indexes
        builder.HasIndex(k => k.KeyHash)
            .IsUnique()
            .HasDatabaseName("ix_api_keys_key_hash");

        builder.HasIndex(k => k.KeyPrefix)
            .HasDatabaseName("ix_api_keys_key_prefix");

        builder.HasIndex(k => new { k.UserId, k.IsActive })
            .HasDatabaseName("ix_api_keys_user_active");

        // Navigation
        builder.HasOne(k => k.User)
            .WithMany(u => u.ApiKeys)
            .HasForeignKey(k => k.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(k => k.DomainEvents);
    }
}
