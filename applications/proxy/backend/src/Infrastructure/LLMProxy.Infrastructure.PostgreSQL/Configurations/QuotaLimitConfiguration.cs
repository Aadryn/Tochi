using LLMProxy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LLMProxy.Infrastructure.PostgreSQL.Configurations;

public class QuotaLimitConfiguration : IEntityTypeConfiguration<QuotaLimit>
{
    public void Configure(EntityTypeBuilder<QuotaLimit> builder)
    {
        builder.ToTable("quota_limits");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(q => q.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(q => q.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(q => q.QuotaType)
            .HasColumnName("quota_type")
            .IsRequired()
            .HasConversion<int>();

        builder.Property(q => q.Limit)
            .HasColumnName("limit")
            .IsRequired();

        builder.Property(q => q.Period)
            .HasColumnName("period")
            .IsRequired()
            .HasConversion<int>();

        builder.Property(q => q.IsEnabled)
            .HasColumnName("is_enabled")
            .IsRequired();

        builder.Property(q => q.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(q => q.UpdatedAt)
            .HasColumnName("updated_at");

        // Indexes
        builder.HasIndex(q => new { q.UserId, q.QuotaType, q.Period })
            .IsUnique()
            .HasDatabaseName("ix_quota_limits_user_type_period");

        // Navigation
        builder.HasOne(q => q.User)
            .WithMany(u => u.QuotaLimits)
            .HasForeignKey(q => q.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(q => q.DomainEvents);
    }
}
