using Authorization.Infrastructure.PostgreSQL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Infrastructure.PostgreSQL.Data;

/// <summary>
/// DbContext pour les données d'audit d'autorisation.
/// Utilise PostgreSQL pour le stockage persistant des logs.
/// </summary>
public class AuthorizationAuditDbContext : DbContext
{
    /// <summary>
    /// Initialise une nouvelle instance du contexte.
    /// </summary>
    /// <param name="options">Options de configuration du DbContext.</param>
    public AuthorizationAuditDbContext(DbContextOptions<AuthorizationAuditDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Table des logs d'audit.
    /// </summary>
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("authorization");

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            entity.Property(e => e.TenantId)
                .HasColumnName("tenant_id")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.OperationType)
                .HasColumnName("operation_type")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.ResourceType)
                .HasColumnName("resource_type")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.ResourceId)
                .HasColumnName("resource_id")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.ActorId)
                .HasColumnName("actor_id")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.ActorType)
                .HasColumnName("actor_type")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.TargetPrincipalId)
                .HasColumnName("target_principal_id")
                .HasMaxLength(100);

            entity.Property(e => e.TargetPrincipalType)
                .HasColumnName("target_principal_type")
                .HasMaxLength(50);

            entity.Property(e => e.Scope)
                .HasColumnName("scope")
                .HasMaxLength(500);

            entity.Property(e => e.Permission)
                .HasColumnName("permission")
                .HasMaxLength(100);

            entity.Property(e => e.RoleId)
                .HasColumnName("role_id")
                .HasMaxLength(100);

            entity.Property(e => e.Result)
                .HasColumnName("result")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.ErrorMessage)
                .HasColumnName("error_message")
                .HasMaxLength(1000);

            entity.Property(e => e.DurationMs)
                .HasColumnName("duration_ms")
                .HasPrecision(10, 2);

            entity.Property(e => e.CacheHit)
                .HasColumnName("cache_hit")
                .HasDefaultValue(false);

            entity.Property(e => e.IpAddress)
                .HasColumnName("ip_address")
                .HasMaxLength(45);

            entity.Property(e => e.UserAgent)
                .HasColumnName("user_agent")
                .HasMaxLength(500);

            entity.Property(e => e.CorrelationId)
                .HasColumnName("correlation_id")
                .HasMaxLength(100);

            entity.Property(e => e.AdditionalData)
                .HasColumnName("additional_data")
                .HasColumnType("jsonb");

            entity.Property(e => e.Timestamp)
                .HasColumnName("timestamp")
                .IsRequired();

            // Index pour les requêtes fréquentes
            entity.HasIndex(e => e.TenantId)
                .HasDatabaseName("ix_audit_logs_tenant_id");

            entity.HasIndex(e => e.Timestamp)
                .HasDatabaseName("ix_audit_logs_timestamp");

            entity.HasIndex(e => e.ActorId)
                .HasDatabaseName("ix_audit_logs_actor_id");

            entity.HasIndex(e => e.OperationType)
                .HasDatabaseName("ix_audit_logs_operation_type");

            entity.HasIndex(e => e.Result)
                .HasDatabaseName("ix_audit_logs_result");

            entity.HasIndex(e => e.CorrelationId)
                .HasDatabaseName("ix_audit_logs_correlation_id")
                .HasFilter("correlation_id IS NOT NULL");

            // Index composite pour requêtes courantes
            entity.HasIndex(e => new { e.TenantId, e.Timestamp })
                .HasDatabaseName("ix_audit_logs_tenant_timestamp")
                .IsDescending(false, true);

            entity.HasIndex(e => new { e.TenantId, e.ActorId, e.Timestamp })
                .HasDatabaseName("ix_audit_logs_tenant_actor_timestamp")
                .IsDescending(false, false, true);
        });
    }
}
