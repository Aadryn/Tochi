using LLMProxy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LLMProxy.Infrastructure.PostgreSQL.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(a => a.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(a => a.UserId)
            .HasColumnName("user_id");

        builder.Property(a => a.ApiKeyId)
            .HasColumnName("api_key_id");

        builder.Property(a => a.ProviderId)
            .HasColumnName("provider_id");

        builder.Property(a => a.RequestId)
            .HasColumnName("request_id")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Endpoint)
            .HasColumnName("endpoint")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.HttpMethod)
            .HasColumnName("http_method")
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(a => a.RequestPath)
            .HasColumnName("request_path")
            .HasMaxLength(1000);

        builder.Property(a => a.StatusCode)
            .HasColumnName("status_code")
            .IsRequired();

        builder.Property(a => a.RequestBody)
            .HasColumnName("request_body")
            .HasColumnType("text");

        builder.Property(a => a.ResponseBody)
            .HasColumnName("response_body")
            .HasColumnType("text");

        builder.Property(a => a.IsAnonymized)
            .HasColumnName("is_anonymized")
            .IsRequired();

        builder.Property(a => a.RequestTokens)
            .HasColumnName("request_tokens")
            .IsRequired();

        builder.Property(a => a.ResponseTokens)
            .HasColumnName("response_tokens")
            .IsRequired();

        builder.Property(a => a.TotalTokens)
            .HasColumnName("total_tokens")
            .IsRequired();

        builder.Property(a => a.DurationMs)
            .HasColumnName("duration_ms")
            .IsRequired();

        builder.Property(a => a.ClientIp)
            .HasColumnName("client_ip")
            .HasMaxLength(45);

        builder.Property(a => a.UserAgent)
            .HasColumnName("user_agent")
            .HasMaxLength(500);

        builder.Property(a => a.IsError)
            .HasColumnName("is_error")
            .IsRequired();

        builder.Property(a => a.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(1000);

        builder.Property(a => a.ErrorStackTrace)
            .HasColumnName("error_stack_trace")
            .HasColumnType("text");

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at");

        // Store metadata as JSON
        builder.Property(a => a.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb");

        // Indexes for efficient querying
        builder.HasIndex(a => a.TenantId)
            .HasDatabaseName("ix_audit_logs_tenant");

        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("ix_audit_logs_user");

        builder.HasIndex(a => a.CreatedAt)
            .HasDatabaseName("ix_audit_logs_created");

        builder.HasIndex(a => new { a.TenantId, a.CreatedAt })
            .HasDatabaseName("ix_audit_logs_tenant_created");

        builder.HasIndex(a => a.RequestId)
            .HasDatabaseName("ix_audit_logs_request_id");

        // Ignore domain events
        builder.Ignore(a => a.DomainEvents);
    }
}
