using LLMProxy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LLMProxy.Infrastructure.PostgreSQL.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(u => u.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(u => u.Role)
            .HasColumnName("role")
            .IsRequired()
            .HasConversion<int>();

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at");

        // Indexes
        builder.HasIndex(u => new { u.TenantId, u.Email })
            .IsUnique()
            .HasDatabaseName("ix_users_tenant_email");

        builder.HasIndex(u => u.Email)
            .HasDatabaseName("ix_users_email");

        // Navigation properties
        builder.HasOne(u => u.Tenant)
            .WithMany(t => t.Users)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.ApiKeys)
            .WithOne(k => k.User)
            .HasForeignKey(k => k.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.QuotaLimits)
            .WithOne(q => q.User)
            .HasForeignKey(q => q.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(u => u.DomainEvents);
    }
}
