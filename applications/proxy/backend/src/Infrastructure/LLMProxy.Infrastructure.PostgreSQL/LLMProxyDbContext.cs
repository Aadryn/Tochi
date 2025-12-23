using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Entities.Routing;
using LLMProxy.Infrastructure.PostgreSQL.Configurations;
using Microsoft.EntityFrameworkCore;

namespace LLMProxy.Infrastructure.PostgreSQL;

/// <summary>
/// Contexte de base de données principal pour LLMProxy avec Entity Framework Core.
/// </summary>
/// <remarks>
/// Fournit l'accès aux entités du domaine, applique la configuration des schémas PostgreSQL,
/// convertit les noms en snake_case et gère automatiquement les timestamps (CreatedAt/UpdatedAt).
/// </remarks>
public class LLMProxyDbContext : DbContext
{
    /// <summary>
    /// Initialise une nouvelle instance de <see cref="LLMProxyDbContext"/>.
    /// </summary>
    /// <param name="options">Options de configuration du contexte.</param>
    public LLMProxyDbContext(DbContextOptions<LLMProxyDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Obtient le DbSet des tenants.
    /// </summary>
    public DbSet<Tenant> Tenants => Set<Tenant>();

    /// <summary>
    /// Obtient le DbSet des utilisateurs.
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// Obtient le DbSet des clés API.
    /// </summary>
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();

    /// <summary>
    /// Obtient le DbSet des fournisseurs LLM.
    /// </summary>
    public DbSet<LLMProvider> LLMProviders => Set<LLMProvider>();

    /// <summary>
    /// Obtient le DbSet des limites de quotas.
    /// </summary>
    public DbSet<QuotaLimit> QuotaLimits => Set<QuotaLimit>();

    /// <summary>
    /// Obtient le DbSet des journaux d'audit.
    /// </summary>
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    /// <summary>
    /// Obtient le DbSet des métriques d'utilisation de tokens.
    /// </summary>
    public DbSet<TokenUsageMetric> TokenUsageMetrics => Set<TokenUsageMetric>();

    /// <summary>
    /// Obtient le DbSet des routes du proxy YARP.
    /// </summary>
    public DbSet<ProxyRoute> ProxyRoutes => Set<ProxyRoute>();

    /// <summary>
    /// Obtient le DbSet des clusters du proxy YARP.
    /// </summary>
    public DbSet<ProxyCluster> ProxyClusters => Set<ProxyCluster>();

    /// <summary>
    /// Obtient le DbSet des destinations de clusters YARP.
    /// </summary>
    public DbSet<ClusterDestination> ClusterDestinations => Set<ClusterDestination>();

    /// <summary>
    /// Configure le modèle de base de données lors de la création.
    /// </summary>
    /// <param name="modelBuilder">Constructeur de modèle EF Core.</param>
    /// <remarks>
    /// Applique les configurations depuis l'assembly, convertit tous les noms en snake_case pour PostgreSQL,
    /// et configure les contraintes (clés, index, foreign keys).
    /// </remarks>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LLMProxyDbContext).Assembly);

        // Use snake_case naming convention for PostgreSQL
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Table names to snake_case
            entity.SetTableName(entity.GetTableName()?.ToSnakeCase());

            // Column names to snake_case
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(property.GetColumnName().ToSnakeCase());
            }

            // Keys
            foreach (var key in entity.GetKeys())
            {
                key.SetName(key.GetName()?.ToSnakeCase());
            }

            // Foreign keys
            foreach (var foreignKey in entity.GetForeignKeys())
            {
                foreignKey.SetConstraintName(foreignKey.GetConstraintName()?.ToSnakeCase());
            }

            // Indexes
            foreach (var index in entity.GetIndexes())
            {
                index.SetDatabaseName(index.GetDatabaseName()?.ToSnakeCase());
            }
        }
    }

    /// <summary>
    /// Sauvegarde les modifications en base de données avec mise à jour automatique des timestamps.
    /// </summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Nombre d'entités affectées.</returns>
    /// <remarks>
    /// Met automatiquement à jour la propriété UpdatedAt pour les entités modifiées.
    /// </remarks>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Domain.Common.Entity &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
