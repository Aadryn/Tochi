using LLMProxy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LLMProxy.Infrastructure.PostgreSQL.Configurations;

public class LLMProviderConfiguration : IEntityTypeConfiguration<LLMProvider>
{
    public void Configure(EntityTypeBuilder<LLMProvider> builder)
    {
        builder.ToTable("llm_providers");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(p => p.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Type)
            .HasColumnName("type")
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.BaseUrl)
            .HasColumnName("base_url")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.Model)
            .HasColumnName("model")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(p => p.Priority)
            .HasColumnName("priority")
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        // Owned types in separate tables to avoid shadow FK conflicts
        builder.OwnsOne(p => p.Configuration, config =>
        {
            config.ToTable("llm_provider_configurations");
            
            config.Property(c => c.ApiKeySecretName)
                .HasColumnName("api_key_secret_name")
                .IsRequired()
                .HasMaxLength(200);

            config.Property(c => c.TimeoutSeconds)
                .HasColumnName("timeout_seconds")
                .IsRequired();

            config.Property(c => c.MaxRetries)
                .HasColumnName("max_retries")
                .IsRequired();

            config.Property(c => c.SupportsStreaming)
                .HasColumnName("supports_streaming")
                .IsRequired();

            config.Property(c => c.RequiresCertificate)
                .HasColumnName("requires_certificate")
                .IsRequired();

            config.Property(c => c.CertificateSecretName)
                .HasColumnName("certificate_secret_name")
                .HasMaxLength(200);

            // Store headers as JSON
            config.Property(c => c.Headers)
                .HasColumnName("headers")
                .HasColumnType("jsonb");
        });

        builder.OwnsOne(p => p.RoutingStrategy, strategy =>
        {
            strategy.ToTable("llm_provider_routing_strategies");
            
            strategy.Property(s => s.Method)
                .HasColumnName("routing_method")
                .IsRequired()
                .HasConversion<int>();

            strategy.Property(s => s.PathPattern)
                .HasColumnName("routing_path_pattern")
                .HasMaxLength(200);

            strategy.Property(s => s.HeaderName)
                .HasColumnName("routing_header_name")
                .HasMaxLength(100);

            strategy.Property(s => s.Subdomain)
                .HasColumnName("routing_subdomain")
                .HasMaxLength(100);
        });

        // Indexes
        builder.HasIndex(p => new { p.TenantId, p.IsActive })
            .HasDatabaseName("ix_llm_providers_tenant_active");

        builder.HasIndex(p => p.Priority)
            .HasDatabaseName("ix_llm_providers_priority");

        // Navigation
        builder.HasOne(p => p.Tenant)
            .WithMany(t => t.Providers)
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(p => p.DomainEvents);
    }
}
