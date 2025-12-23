using LLMProxy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LLMProxy.Infrastructure.PostgreSQL.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Slug)
            .HasColumnName("slug")
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(t => t.Slug)
            .IsUnique()
            .HasDatabaseName("ix_tenants_slug");

        builder.Property(t => t.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(t => t.DeactivatedAt)
            .HasColumnName("deactivated_at");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        // Owned type for TenantSettings in separate table
        builder.OwnsOne(t => t.Settings, settings =>
        {
            settings.ToTable("tenant_settings");
            
            settings.Property(s => s.MaxUsers)
                .HasColumnName("max_users")
                .IsRequired();

            settings.Property(s => s.MaxProviders)
                .HasColumnName("max_providers")
                .IsRequired();

            settings.Property(s => s.EnableAuditLogging)
                .HasColumnName("enable_audit_logging")
                .IsRequired();

            settings.Property(s => s.AuditRetentionDays)
                .HasColumnName("audit_retention_days")
                .IsRequired();

            settings.Property(s => s.EnableResponseCache)
                .HasColumnName("enable_response_cache")
                .IsRequired();
        });

        // Navigation properties
        builder.HasMany(t => t.Users)
            .WithOne(u => u.Tenant)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Providers)
            .WithOne(p => p.Tenant)
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(t => t.DomainEvents);
    }
}
