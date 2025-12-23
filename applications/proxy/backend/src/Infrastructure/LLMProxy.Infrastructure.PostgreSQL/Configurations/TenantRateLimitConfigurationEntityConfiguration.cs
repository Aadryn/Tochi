using LLMProxy.Domain.Entities;
using LLMProxy.Infrastructure.PostgreSQL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LLMProxy.Infrastructure.PostgreSQL.Configurations;

/// <summary>
/// Configuration EF Core pour <see cref="TenantRateLimitConfigurationEntity"/>.
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-041 Rate Limiting et Throttling.
/// </para>
/// <para>
/// Table : <c>configuration.tenant_ratelimit_configurations</c>
/// </para>
/// </remarks>
public sealed class TenantRateLimitConfigurationEntityConfiguration
    : IEntityTypeConfiguration<TenantRateLimitConfigurationEntity>
{
    /// <summary>
    /// Configure la table et les propriétés de l'entité.
    /// </summary>
    /// <param name="builder">Constructeur de type d'entité EF Core.</param>
    public void Configure(EntityTypeBuilder<TenantRateLimitConfigurationEntity> builder)
    {
        builder.ToTable("tenant_ratelimit_configurations", "configuration");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        // Index unique sur TenantId (une seule config par tenant)
        builder.HasIndex(e => e.TenantId)
            .IsUnique()
            .HasDatabaseName("ix_tenant_ratelimit_configurations_tenant_id");

        // Foreign key vers Tenants
        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Limites globales
        builder.Property(e => e.GlobalRequestsPerMinute)
            .HasColumnName("global_requests_per_minute")
            .IsRequired();

        builder.Property(e => e.GlobalRequestsPerDay)
            .HasColumnName("global_requests_per_day")
            .IsRequired();

        builder.Property(e => e.GlobalTokensPerMinute)
            .HasColumnName("global_tokens_per_minute")
            .IsRequired();

        builder.Property(e => e.GlobalTokensPerDay)
            .HasColumnName("global_tokens_per_day")
            .IsRequired();

        // Limites API Key
        builder.Property(e => e.ApiKeyRequestsPerMinute)
            .HasColumnName("apikey_requests_per_minute")
            .IsRequired();

        builder.Property(e => e.ApiKeyTokensPerMinute)
            .HasColumnName("apikey_tokens_per_minute")
            .IsRequired();

        // Timestamps (hérités de Entity via Entity.CreatedAt et Entity.UpdatedAt)
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relation 1-N avec EndpointLimits
        builder.HasMany(e => e.EndpointLimits)
            .WithOne(el => el.Configuration)
            .HasForeignKey(el => el.TenantRateLimitConfigurationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// Configuration EF Core pour <see cref="EndpointLimitEntity"/>.
/// </summary>
/// <remarks>
/// <para>
/// Table : <c>configuration.endpoint_limits</c>
/// </para>
/// </remarks>
public sealed class EndpointLimitEntityConfiguration
    : IEntityTypeConfiguration<EndpointLimitEntity>
{
    /// <summary>
    /// Configure la table et les propriétés de l'entité.
    /// </summary>
    /// <param name="builder">Constructeur de type d'entité EF Core.</param>
    public void Configure(EntityTypeBuilder<EndpointLimitEntity> builder)
    {
        builder.ToTable("endpoint_limits", "configuration");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(e => e.TenantRateLimitConfigurationId)
            .HasColumnName("tenant_ratelimit_configuration_id")
            .IsRequired();

        // Index composite sur (TenantConfig, EndpointPath)
        builder.HasIndex(e => new { e.TenantRateLimitConfigurationId, e.EndpointPath })
            .IsUnique()
            .HasDatabaseName("ix_endpoint_limits_config_path");

        builder.Property(e => e.EndpointPath)
            .HasColumnName("endpoint_path")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.RequestsPerMinute)
            .HasColumnName("requests_per_minute")
            .IsRequired();

        builder.Property(e => e.TokensPerMinute)
            .HasColumnName("tokens_per_minute")
            .IsRequired();

        builder.Property(e => e.BurstCapacity)
            .HasColumnName("burst_capacity")
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
