---
id: 066
title: Implémenter configuration dynamique Rate Limiting
concerns: rate-limiting, configuration, database, cache
priority: critical
effort: large
dependencies: []
status: to-do
created: 2025-12-23
---

# Implémenter configuration dynamique Rate Limiting

## 🎯 Objectif

Remplacer la configuration hardcodée de rate limiting par un système de configuration dynamique basé sur PostgreSQL avec cache Redis, permettant la personnalisation des limites par tenant sans redéploiement.

## 📊 Contexte

### Problème identifié

Fichier `backend/src/Application/LLMProxy.Application/Services/RateLimiting/RateLimitConfigurationService.cs` (ligne 68) contient TODO critique :

```csharp
// TODO: Remplacer par chargement depuis DB + cache Redis en production
var config = new TenantRateLimitConfiguration
{
    TenantId = tenantId,
    GlobalLimit = new GlobalLimit
    {
        RequestsPerMinute = 1000,  // ← HARDCODÉ pour tous les tenants
        TokensPerMinute = 100_000
    },
    // (...)
};
```

**Documentation XML du fichier indique explicitement** :
> "Dans un scénario de production, cette classe devrait être remplacée par une version qui charge les configurations depuis une base de données avec cache Redis."

### Impact actuel

- **Fonctionnalité dégradée** : Tous les tenants ont exactement les mêmes limites
- **Flexibilité ZÉRO** : Impossible de personnaliser les limites par tenant sans redéploiement
- **Risque métier CRITIQUE** : Tenant premium et tenant freemium ont les mêmes quotas → perte de revenu potentielle
- **Blocage production** : Commentaire dit explicitement "en production"
- **Conformité ADR-041 partielle** : Rate limiting existe mais implémentation non-production-ready

### Bénéfice attendu

- **Différenciation business** : Limites personnalisées par plan (freemium, premium, enterprise)
- **Flexibilité opérationnelle** : Ajustement limites sans redéploiement
- **Performance** : Cache Redis réduit latence récupération config (< 5ms)
- **Scalabilité** : Gestion centralisée de milliers de tenants
- **Conformité ADR-041** : Implémentation complète et production-ready

## 🔧 Implémentation

### Fichiers à créer

```
backend/src/Infrastructure/LLMProxy.Infrastructure.PostgreSQL/
├── Configurations/
│   └── TenantRateLimitConfigurationDbConfiguration.cs
├── Entities/
│   ├── TenantRateLimitConfigurationEntity.cs
│   └── EndpointLimitEntity.cs
├── Repositories/
│   └── TenantRateLimitConfigurationRepository.cs
└── Migrations/
    └── YYYYMMDDHHMMSS_AddTenantRateLimitConfiguration.cs

backend/src/Application/LLMProxy.Application/
├── Interfaces/
│   └── ITenantRateLimitConfigurationRepository.cs
└── Services/RateLimiting/
    └── DatabaseRateLimitConfigurationService.cs  (remplace actuel)

backend/tests/
└── LLMProxy.Infrastructure.PostgreSQL.Tests/
    └── Repositories/
        └── TenantRateLimitConfigurationRepositoryTests.cs
```

### Fichiers à modifier

```
backend/src/Application/LLMProxy.Application/Services/RateLimiting/
└── RateLimitConfigurationService.cs  (renommer en DefaultRateLimitConfigurationService.cs)

backend/src/Presentation/LLMProxy.Admin.API/
└── Program.cs  (enregistrement DI)

backend/src/Presentation/LLMProxy.Gateway/
└── Program.cs  (enregistrement DI)
```

### Modifications détaillées

#### 1. Créer entités PostgreSQL

##### `TenantRateLimitConfigurationEntity.cs`

```csharp
namespace LLMProxy.Infrastructure.PostgreSQL.Entities;

/// <summary>
/// Entité EF Core pour la configuration de rate limiting par tenant.
/// </summary>
/// <remarks>
/// Conforme à l'ADR-041 Rate Limiting et Throttling.
/// Stockage PostgreSQL avec cache Redis (ADR-042).
/// </remarks>
public sealed class TenantRateLimitConfigurationEntity
{
    /// <summary>
    /// Identifiant unique de la configuration.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identifiant du tenant concerné.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Requêtes par minute (limite globale).
    /// </summary>
    public int GlobalRequestsPerMinute { get; set; }

    /// <summary>
    /// Requêtes par jour (limite globale).
    /// </summary>
    public int GlobalRequestsPerDay { get; set; }

    /// <summary>
    /// Tokens par minute (limite globale).
    /// </summary>
    public int GlobalTokensPerMinute { get; set; }

    /// <summary>
    /// Tokens par jour (limite globale).
    /// </summary>
    public int GlobalTokensPerDay { get; set; }

    /// <summary>
    /// Requêtes par minute pour une API Key.
    /// </summary>
    public int ApiKeyRequestsPerMinute { get; set; }

    /// <summary>
    /// Tokens par minute pour une API Key.
    /// </summary>
    public int ApiKeyTokensPerMinute { get; set; }

    /// <summary>
    /// Limites spécifiques par endpoint.
    /// </summary>
    public ICollection<EndpointLimitEntity> EndpointLimits { get; set; } = new List<EndpointLimitEntity>();

    /// <summary>
    /// Date de création de la configuration.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date de dernière modification.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

##### `EndpointLimitEntity.cs`

```csharp
namespace LLMProxy.Infrastructure.PostgreSQL.Entities;

/// <summary>
/// Entité EF Core pour les limites spécifiques par endpoint.
/// </summary>
public sealed class EndpointLimitEntity
{
    public Guid Id { get; set; }
    public Guid TenantRateLimitConfigurationId { get; set; }
    
    /// <summary>
    /// Chemin de l'endpoint (ex: "/v1/chat/completions").
    /// </summary>
    public string EndpointPath { get; set; } = string.Empty;
    
    public int RequestsPerMinute { get; set; }
    public int TokensPerMinute { get; set; }
    public int BurstCapacity { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public TenantRateLimitConfigurationEntity Configuration { get; set; } = null!;
}
```

#### 2. Configuration EF Core

##### `TenantRateLimitConfigurationDbConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LLMProxy.Infrastructure.PostgreSQL.Entities;

namespace LLMProxy.Infrastructure.PostgreSQL.Configurations;

/// <summary>
/// Configuration EF Core pour TenantRateLimitConfiguration.
/// </summary>
public sealed class TenantRateLimitConfigurationDbConfiguration 
    : IEntityTypeConfiguration<TenantRateLimitConfigurationEntity>
{
    public void Configure(EntityTypeBuilder<TenantRateLimitConfigurationEntity> builder)
    {
        builder.ToTable("tenant_ratelimit_configurations", "configuration");

        builder.HasKey(e => e.Id);

        // Index unique sur TenantId (une seule config par tenant)
        builder.HasIndex(e => e.TenantId)
            .IsUnique()
            .HasDatabaseName("ix_tenant_ratelimit_configurations_tenant_id");

        // Foreign key vers Tenants
        builder.HasOne<TenantEntity>()
            .WithMany()
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Limites globales (non nullables)
        builder.Property(e => e.GlobalRequestsPerMinute).IsRequired();
        builder.Property(e => e.GlobalRequestsPerDay).IsRequired();
        builder.Property(e => e.GlobalTokensPerMinute).IsRequired();
        builder.Property(e => e.GlobalTokensPerDay).IsRequired();

        // Limites API Key
        builder.Property(e => e.ApiKeyRequestsPerMinute).IsRequired();
        builder.Property(e => e.ApiKeyTokensPerMinute).IsRequired();

        // Timestamps
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relation 1-N avec EndpointLimits
        builder.HasMany(e => e.EndpointLimits)
            .WithOne(el => el.Configuration)
            .HasForeignKey(el => el.TenantRateLimitConfigurationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class EndpointLimitDbConfiguration 
    : IEntityTypeConfiguration<EndpointLimitEntity>
{
    public void Configure(EntityTypeBuilder<EndpointLimitEntity> builder)
    {
        builder.ToTable("endpoint_limits", "configuration");

        builder.HasKey(e => e.Id);

        // Index composite sur (TenantConfig, EndpointPath)
        builder.HasIndex(e => new { e.TenantRateLimitConfigurationId, e.EndpointPath })
            .IsUnique()
            .HasDatabaseName("ix_endpoint_limits_config_path");

        builder.Property(e => e.EndpointPath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.RequestsPerMinute).IsRequired();
        builder.Property(e => e.TokensPerMinute).IsRequired();
        builder.Property(e => e.BurstCapacity).IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
```

#### 3. Migration EF Core

```powershell
# Commande pour générer la migration
cd backend
dotnet ef migrations add AddTenantRateLimitConfiguration `
    --project src\Infrastructure\LLMProxy.Infrastructure.PostgreSQL `
    --startup-project src\Presentation\LLMProxy.Admin.API `
    --context ApplicationDbContext
```

Migration générée (exemple) :

```csharp
public partial class AddTenantRateLimitConfiguration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Créer table tenant_ratelimit_configurations
        migrationBuilder.CreateTable(
            name: "tenant_ratelimit_configurations",
            schema: "configuration",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                global_requests_per_minute = table.Column<int>(type: "integer", nullable: false),
                global_requests_per_day = table.Column<int>(type: "integer", nullable: false),
                global_tokens_per_minute = table.Column<int>(type: "integer", nullable: false),
                global_tokens_per_day = table.Column<int>(type: "integer", nullable: false),
                apikey_requests_per_minute = table.Column<int>(type: "integer", nullable: false),
                apikey_tokens_per_minute = table.Column<int>(type: "integer", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tenant_ratelimit_configurations", x => x.id);
                table.ForeignKey(
                    name: "fk_tenant_ratelimit_configurations_tenants",
                    column: x => x.tenant_id,
                    principalSchema: "tenants",
                    principalTable: "tenants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        // Créer table endpoint_limits
        migrationBuilder.CreateTable(
            name: "endpoint_limits",
            schema: "configuration",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_ratelimit_configuration_id = table.Column<Guid>(type: "uuid", nullable: false),
                endpoint_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                requests_per_minute = table.Column<int>(type: "integer", nullable: false),
                tokens_per_minute = table.Column<int>(type: "integer", nullable: false),
                burst_capacity = table.Column<int>(type: "integer", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_endpoint_limits", x => x.id);
                table.ForeignKey(
                    name: "fk_endpoint_limits_configurations",
                    column: x => x.tenant_ratelimit_configuration_id,
                    principalSchema: "configuration",
                    principalTable: "tenant_ratelimit_configurations",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        // Index unique sur tenant_id
        migrationBuilder.CreateIndex(
            name: "ix_tenant_ratelimit_configurations_tenant_id",
            schema: "configuration",
            table: "tenant_ratelimit_configurations",
            column: "tenant_id",
            unique: true);

        // Index composite sur (config_id, endpoint_path)
        migrationBuilder.CreateIndex(
            name: "ix_endpoint_limits_config_path",
            schema: "configuration",
            table: "endpoint_limits",
            columns: new[] { "tenant_ratelimit_configuration_id", "endpoint_path" },
            unique: true);

        // Seed data par défaut (configurations initiales pour tenants existants)
        migrationBuilder.Sql(@"
            INSERT INTO configuration.tenant_ratelimit_configurations 
            (id, tenant_id, global_requests_per_minute, global_requests_per_day, global_tokens_per_minute, global_tokens_per_day, apikey_requests_per_minute, apikey_tokens_per_minute)
            SELECT 
                gen_random_uuid(),
                t.id,
                1000,      -- Valeurs par défaut
                100000,
                100000,
                10000000,
                100,
                10000
            FROM tenants.tenants t
            WHERE NOT EXISTS (
                SELECT 1 FROM configuration.tenant_ratelimit_configurations trlc WHERE trlc.tenant_id = t.id
            );
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "endpoint_limits", schema: "configuration");
        migrationBuilder.DropTable(name: "tenant_ratelimit_configurations", schema: "configuration");
    }
}
```

#### 4. Repository Pattern

##### Interface `ITenantRateLimitConfigurationRepository.cs`

```csharp
namespace LLMProxy.Application.Interfaces;

public interface ITenantRateLimitConfigurationRepository
{
    /// <summary>
    /// Récupère la configuration de rate limiting pour un tenant.
    /// </summary>
    Task<TenantRateLimitConfiguration?> GetByTenantIdAsync(
        Guid tenantId, 
        CancellationToken ct = default);

    /// <summary>
    /// Crée ou met à jour la configuration d'un tenant.
    /// </summary>
    Task<TenantRateLimitConfiguration> UpsertAsync(
        TenantRateLimitConfiguration config, 
        CancellationToken ct = default);

    /// <summary>
    /// Supprime la configuration d'un tenant (utilise config par défaut).
    /// </summary>
    Task DeleteAsync(Guid tenantId, CancellationToken ct = default);
}
```

##### Implémentation `TenantRateLimitConfigurationRepository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using LLMProxy.Application.Configuration.RateLimiting;
using LLMProxy.Application.Interfaces;
using LLMProxy.Infrastructure.PostgreSQL.Context;
using LLMProxy.Infrastructure.PostgreSQL.Entities;

namespace LLMProxy.Infrastructure.PostgreSQL.Repositories;

/// <summary>
/// Repository pour la gestion des configurations de rate limiting.
/// </summary>
public sealed class TenantRateLimitConfigurationRepository 
    : ITenantRateLimitConfigurationRepository
{
    private readonly ApplicationDbContext _context;

    public TenantRateLimitConfigurationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TenantRateLimitConfiguration?> GetByTenantIdAsync(
        Guid tenantId, 
        CancellationToken ct = default)
    {
        var entity = await _context.Set<TenantRateLimitConfigurationEntity>()
            .Include(e => e.EndpointLimits)
            .FirstOrDefaultAsync(e => e.TenantId == tenantId, ct);

        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<TenantRateLimitConfiguration> UpsertAsync(
        TenantRateLimitConfiguration config, 
        CancellationToken ct = default)
    {
        var existing = await _context.Set<TenantRateLimitConfigurationEntity>()
            .Include(e => e.EndpointLimits)
            .FirstOrDefaultAsync(e => e.TenantId == config.TenantId, ct);

        if (existing == null)
        {
            // INSERT
            var entity = MapToEntity(config);
            _context.Set<TenantRateLimitConfigurationEntity>().Add(entity);
        }
        else
        {
            // UPDATE
            existing.GlobalRequestsPerMinute = config.GlobalLimit.RequestsPerMinute;
            existing.GlobalRequestsPerDay = config.GlobalLimit.RequestsPerDay;
            existing.GlobalTokensPerMinute = config.GlobalLimit.TokensPerMinute;
            existing.GlobalTokensPerDay = config.GlobalLimit.TokensPerDay;
            existing.ApiKeyRequestsPerMinute = config.ApiKeyLimit.RequestsPerMinute;
            existing.ApiKeyTokensPerMinute = config.ApiKeyLimit.TokensPerMinute;
            existing.UpdatedAt = DateTime.UtcNow;

            // Supprimer anciennes endpoint limits et recréer
            _context.RemoveRange(existing.EndpointLimits);
            existing.EndpointLimits = config.EndpointLimits
                .Select(kv => new EndpointLimitEntity
                {
                    EndpointPath = kv.Key,
                    RequestsPerMinute = kv.Value.RequestsPerMinute,
                    TokensPerMinute = kv.Value.TokensPerMinute,
                    BurstCapacity = kv.Value.BurstCapacity
                })
                .ToList();
        }

        await _context.SaveChangesAsync(ct);
        return config;
    }

    public async Task DeleteAsync(Guid tenantId, CancellationToken ct = default)
    {
        var entity = await _context.Set<TenantRateLimitConfigurationEntity>()
            .FirstOrDefaultAsync(e => e.TenantId == tenantId, ct);

        if (entity != null)
        {
            _context.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }

    // Mapping Entity → Domain
    private static TenantRateLimitConfiguration MapToDomain(TenantRateLimitConfigurationEntity entity)
    {
        return new TenantRateLimitConfiguration
        {
            TenantId = entity.TenantId,
            GlobalLimit = new GlobalLimit
            {
                RequestsPerMinute = entity.GlobalRequestsPerMinute,
                RequestsPerDay = entity.GlobalRequestsPerDay,
                TokensPerMinute = entity.GlobalTokensPerMinute,
                TokensPerDay = entity.GlobalTokensPerDay
            },
            ApiKeyLimit = new ApiKeyLimit
            {
                RequestsPerMinute = entity.ApiKeyRequestsPerMinute,
                TokensPerMinute = entity.ApiKeyTokensPerMinute
            },
            EndpointLimits = entity.EndpointLimits.ToDictionary(
                el => el.EndpointPath,
                el => new EndpointLimit
                {
                    RequestsPerMinute = el.RequestsPerMinute,
                    TokensPerMinute = el.TokensPerMinute,
                    BurstCapacity = el.BurstCapacity
                })
        };
    }

    // Mapping Domain → Entity
    private static TenantRateLimitConfigurationEntity MapToEntity(TenantRateLimitConfiguration config)
    {
        return new TenantRateLimitConfigurationEntity
        {
            Id = Guid.NewGuid(),
            TenantId = config.TenantId,
            GlobalRequestsPerMinute = config.GlobalLimit.RequestsPerMinute,
            GlobalRequestsPerDay = config.GlobalLimit.RequestsPerDay,
            GlobalTokensPerMinute = config.GlobalLimit.TokensPerMinute,
            GlobalTokensPerDay = config.GlobalLimit.TokensPerDay,
            ApiKeyRequestsPerMinute = config.ApiKeyLimit.RequestsPerMinute,
            ApiKeyTokensPerMinute = config.ApiKeyLimit.TokensPerMinute,
            EndpointLimits = config.EndpointLimits
                .Select(kv => new EndpointLimitEntity
                {
                    EndpointPath = kv.Key,
                    RequestsPerMinute = kv.Value.RequestsPerMinute,
                    TokensPerMinute = kv.Value.TokensPerMinute,
                    BurstCapacity = kv.Value.BurstCapacity
                })
                .ToList()
        };
    }
}
```

#### 5. Service avec Cache Redis

##### `DatabaseRateLimitConfigurationService.cs`

```csharp
using LLMProxy.Application.Configuration.RateLimiting;
using LLMProxy.Application.Interfaces;

namespace LLMProxy.Application.Services.RateLimiting;

/// <summary>
/// Implémentation de configuration de rate limiting avec base de données + cache Redis.
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-041 Rate Limiting et Throttling.
/// Conforme à l'ADR-042 Stratégie de Cache Distribué (L2 Redis).
/// </para>
/// <para>
/// <strong>Stratégie de cache :</strong>
/// <list type="number">
/// <item>Vérifier cache Redis (TTL : 1 minute)</item>
/// <item>Si absent, charger depuis PostgreSQL</item>
/// <item>Si absent en DB, utiliser configuration par défaut</item>
/// <item>Mettre en cache le résultat</item>
/// </list>
/// </para>
/// </remarks>
public sealed class DatabaseRateLimitConfigurationService : IRateLimitConfigurationService
{
    private readonly ITenantRateLimitConfigurationRepository _repository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<DatabaseRateLimitConfigurationService> _logger;

    public DatabaseRateLimitConfigurationService(
        ITenantRateLimitConfigurationRepository repository,
        ICacheService cacheService,
        ILogger<DatabaseRateLimitConfigurationService> logger)
    {
        _repository = repository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TenantRateLimitConfiguration> GetConfigurationAsync(
        Guid tenantId,
        CancellationToken ct = default)
    {
        // 1. Vérifier cache Redis
        var cacheKey = $"ratelimit:config:{tenantId}";
        var cached = await _cacheService.GetAsync<TenantRateLimitConfiguration>(cacheKey, ct);
        
        if (cached != null)
        {
            _logger.LogDebug("Rate limit configuration for tenant {TenantId} loaded from cache", tenantId);
            return cached;
        }

        // 2. Charger depuis PostgreSQL
        var config = await _repository.GetByTenantIdAsync(tenantId, ct);

        // 3. Si non trouvé, utiliser config par défaut
        if (config == null)
        {
            _logger.LogInformation(
                "No custom rate limit configuration for tenant {TenantId}, using default", 
                tenantId);
            
            config = CreateDefaultConfiguration(tenantId);
        }

        // 4. Mettre en cache (TTL : 1 minute)
        await _cacheService.SetAsync(cacheKey, config, TimeSpan.FromMinutes(1), ct);

        _logger.LogDebug("Rate limit configuration for tenant {TenantId} loaded from database", tenantId);
        return config;
    }

    /// <summary>
    /// Crée une configuration par défaut pour un tenant.
    /// </summary>
    private static TenantRateLimitConfiguration CreateDefaultConfiguration(Guid tenantId)
    {
        return new TenantRateLimitConfiguration
        {
            TenantId = tenantId,
            GlobalLimit = new GlobalLimit
            {
                RequestsPerMinute = 1000,
                RequestsPerDay = 100_000,
                TokensPerMinute = 100_000,
                TokensPerDay = 10_000_000
            },
            ApiKeyLimit = new ApiKeyLimit
            {
                RequestsPerMinute = 100,
                TokensPerMinute = 10_000
            },
            EndpointLimits = new Dictionary<string, EndpointLimit>
            {
                ["/v1/chat/completions"] = new EndpointLimit
                {
                    RequestsPerMinute = 60,
                    TokensPerMinute = 100_000,
                    BurstCapacity = 120
                },
                ["/v1/embeddings"] = new EndpointLimit
                {
                    RequestsPerMinute = 1000,
                    TokensPerMinute = 500_000,
                    BurstCapacity = 2000
                }
            }
        };
    }
}
```

#### 6. Enregistrement Dependency Injection

##### `Program.cs` (Admin API + Gateway)

```csharp
// Dans ConfigureServices ou equivalent

// Repository
builder.Services.AddScoped<ITenantRateLimitConfigurationRepository, TenantRateLimitConfigurationRepository>();

// Service de configuration Rate Limiting (remplace l'ancienne implémentation)
builder.Services.AddScoped<IRateLimitConfigurationService, DatabaseRateLimitConfigurationService>();
```

#### 7. Tests Unitaires

##### `TenantRateLimitConfigurationRepositoryTests.cs`

```csharp
using FluentAssertions;
using LLMProxy.Application.Configuration.RateLimiting;
using LLMProxy.Infrastructure.PostgreSQL.Repositories;
using LLMProxy.Infrastructure.PostgreSQL.Context;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LLMProxy.Infrastructure.PostgreSQL.Tests.Repositories;

public sealed class TenantRateLimitConfigurationRepositoryTests : IAsyncLifetime
{
    private ApplicationDbContext _context = null!;
    private TenantRateLimitConfigurationRepository _sut = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"RateLimitConfigTest_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
        await _context.Database.EnsureCreatedAsync();

        _sut = new TenantRateLimitConfigurationRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task GetByTenantIdAsync_WhenConfigExists_ShouldReturnConfiguration()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new TenantRateLimitConfiguration
        {
            TenantId = tenantId,
            GlobalLimit = new GlobalLimit { RequestsPerMinute = 500 }
        };
        await _sut.UpsertAsync(config);

        // Act
        var result = await _sut.GetByTenantIdAsync(tenantId);

        // Assert
        result.Should().NotBeNull();
        result!.TenantId.Should().Be(tenantId);
        result.GlobalLimit.RequestsPerMinute.Should().Be(500);
    }

    [Fact]
    public async Task UpsertAsync_WhenConfigNotExists_ShouldInsert()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = new TenantRateLimitConfiguration
        {
            TenantId = tenantId,
            GlobalLimit = new GlobalLimit { RequestsPerMinute = 1000 }
        };

        // Act
        var result = await _sut.UpsertAsync(config);

        // Assert
        var saved = await _sut.GetByTenantIdAsync(tenantId);
        saved.Should().NotBeNull();
        saved!.GlobalLimit.RequestsPerMinute.Should().Be(1000);
    }

    // (Ajouter tests pour Update, Delete, Edge cases...)
}
```

### Considérations techniques

**Points d'attention :**
- **Migration données existantes** : Seed configurations par défaut pour tenants existants
- **Backward compatibility** : L'ancienne implémentation reste fonctionnelle si DB indisponible (fallback)
- **Performance cache** : TTL de 1 minute = compromis fraîcheur/performance
- **Invalidation cache** : Après modification config, invalider cache explicitement

**Pièges à éviter :**
- Oublier d'invalider cache Redis après UPDATE de configuration
- Ne pas tester le fallback en cas d'erreur DB ou Redis
- Ne pas documenter le format de la clé cache (`ratelimit:config:{tenantId}`)
- Créer des migrations sans seed data → tenants existants sans config

**Bonnes pratiques :**
1. **Toujours** fournir configuration par défaut en fallback
2. **Invalider cache** après toute modification de configuration
3. **Logger** chaque chargement de config (source : cache, DB, default)
4. **Monitorer** hit rate du cache Redis (métrique de performance)
5. **Documenter** stratégie de cache dans ADR-042

## ✅ Critères de validation

### Migration et Base de Données

- [ ] Migration EF Core créée et appliquée
- [ ] Tables `tenant_ratelimit_configurations` et `endpoint_limits` créées en schéma `configuration`
- [ ] Index unique sur `tenant_id` créé
- [ ] Foreign keys vers `tenants` configurées avec `ON DELETE CASCADE`
- [ ] Seed data créé pour tenants existants (valeurs par défaut)
- [ ] Migration testée avec `dotnet ef database update`

### Repository Pattern

- [ ] Interface `ITenantRateLimitConfigurationRepository` créée
- [ ] Implémentation `TenantRateLimitConfigurationRepository` créée et testée
- [ ] Mapping Entity ↔ Domain implémenté sans perte de données
- [ ] Tests unitaires repository (GET, INSERT, UPDATE, DELETE) passent
- [ ] Gestion erreurs PostgreSQL (retry, logging)

### Service avec Cache

- [ ] `DatabaseRateLimitConfigurationService` implémenté
- [ ] Cache Redis intégré avec clé `ratelimit:config:{tenantId}`
- [ ] TTL cache configuré à 1 minute
- [ ] Fallback vers config par défaut si DB/Redis indisponible
- [ ] Logging de la source de config (cache, DB, default)
- [ ] Tests unitaires service avec mocks cache + repository passent

### Intégration

- [ ] Dependency Injection configurée dans `Program.cs` (Admin API + Gateway)
- [ ] Ancien `RateLimitConfigurationService.cs` renommé en `DefaultRateLimitConfigurationService.cs`
- [ ] Build backend réussit sans warnings
- [ ] Tests backend passent à 100%
- [ ] Tests d'intégration avec PostgreSQL + Redis passent

### Validation Fonctionnelle

- [ ] Configuration par défaut appliquée pour nouveau tenant (sans config DB)
- [ ] Configuration personnalisée chargée depuis DB pour tenant avec config
- [ ] Cache Redis utilisé (2ème appel plus rapide que 1er)
- [ ] Modification config DB invalidée du cache automatiquement
- [ ] Performance : Latence < 5ms avec cache, < 50ms sans cache

### Documentation

- [ ] ADR-041 mis à jour avec référence implémentation DB
- [ ] ADR-042 mis à jour avec stratégie cache rate limiting
- [ ] README.md mis à jour avec section configuration rate limiting
- [ ] Documentation API Admin (Swagger) avec endpoints CRUD config
- [ ] Revue de code effectuée
- [ ] Commit atomique : "feat: implement dynamic rate limiting configuration with PostgreSQL + Redis cache"

## 🔗 Références

- **ADR-041** : Rate Limiting et Throttling
- **ADR-042** : Stratégie de Cache Distribué (L1 Memory, L2 Redis, L3 Database)
- **ADR-017** : Repository Pattern
- **Code existant** : `RateLimitConfigurationService.cs:38-59` (exemple de code attendu)
- Pilier de qualité : **Évolutivité** (configuration dynamique sans redéploiement)
- Principe appliqué : **Separation of Concerns** (config séparée du code)
