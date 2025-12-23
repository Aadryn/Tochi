using LLMProxy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LLMProxy.Infrastructure.PostgreSQL.Configurations;

/// <summary>
/// Configuration Entity Framework Core pour l'entité <see cref="TokenUsageMetric"/>.
/// </summary>
/// <remarks>
/// Définit le schéma de table et les index pour la table <c>token_usage_metrics</c>.
/// Configure le stockage des métriques d'utilisation des tokens par période et par fournisseur.
/// </remarks>
public class TokenUsageMetricConfiguration : IEntityTypeConfiguration<TokenUsageMetric>
{
    /// <summary>
    /// Configure le mapping de l'entité <see cref="TokenUsageMetric"/> vers la base de données PostgreSQL.
    /// </summary>
    /// <param name="builder">Constructeur de configuration pour définir les propriétés, index composites et conversions.</param>
    public void Configure(EntityTypeBuilder<TokenUsageMetric> builder)
    {
        builder.ToTable("token_usage_metrics");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(m => m.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(m => m.UserId)
            .HasColumnName("user_id");

        builder.Property(m => m.ProviderId)
            .HasColumnName("provider_id");

        builder.Property(m => m.PeriodStart)
            .HasColumnName("period_start")
            .IsRequired();

        builder.Property(m => m.PeriodEnd)
            .HasColumnName("period_end")
            .IsRequired();

        builder.Property(m => m.Period)
            .HasColumnName("period")
            .IsRequired()
            .HasConversion<int>();

        builder.Property(m => m.TotalRequests)
            .HasColumnName("total_requests")
            .IsRequired();

        builder.Property(m => m.SuccessfulRequests)
            .HasColumnName("successful_requests")
            .IsRequired();

        builder.Property(m => m.FailedRequests)
            .HasColumnName("failed_requests")
            .IsRequired();

        builder.Property(m => m.TotalInputTokens)
            .HasColumnName("total_input_tokens")
            .IsRequired();

        builder.Property(m => m.TotalOutputTokens)
            .HasColumnName("total_output_tokens")
            .IsRequired();

        builder.Property(m => m.TotalTokens)
            .HasColumnName("total_tokens")
            .IsRequired();

        builder.Property(m => m.TotalDurationMs)
            .HasColumnName("total_duration_ms")
            .IsRequired();

        builder.Property(m => m.AverageDurationMs)
            .HasColumnName("average_duration_ms")
            .IsRequired();

        builder.Property(m => m.EstimatedCost)
            .HasColumnName("estimated_cost")
            .HasColumnType("decimal(18,6)")
            .IsRequired();

        builder.Property(m => m.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(m => m.UpdatedAt)
            .HasColumnName("updated_at");

        // Indexes
        builder.HasIndex(m => new { m.TenantId, m.PeriodStart, m.Period })
            .HasDatabaseName("ix_token_metrics_tenant_period");

        builder.HasIndex(m => new { m.UserId, m.PeriodStart })
            .HasDatabaseName("ix_token_metrics_user_period");

        builder.HasIndex(m => new { m.ProviderId, m.PeriodStart })
            .HasDatabaseName("ix_token_metrics_provider_period");

        // Unique constraint for aggregation key
        builder.HasIndex(m => new { m.TenantId, m.UserId, m.ProviderId, m.PeriodStart, m.Period })
            .IsUnique()
            .HasDatabaseName("ix_token_metrics_unique_key");

        // Ignore domain events
        builder.Ignore(m => m.DomainEvents);
    }
}
