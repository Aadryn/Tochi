# 49. Database Migrations Strategy

Date: 2025-12-21

## Statut

Accepté

## Contexte

Un proxy LLM en production nécessite des évolutions de schéma fréquentes. Sans stratégie :
- Migrations manuelles → Erreurs, oublis
- Downtime lors des migrations
- Rollback impossible en cas de problème
- Incohérence entre environnements

```csharp
// ❌ SANS STRATÉGIE : Migrations manuelles et risquées
public class Startup
{
    public void Configure(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LlmProxyDbContext>();
        
        // DANGER : Migration automatique au démarrage
        // - Bloque le démarrage de l'app
        // - Peut échouer et bloquer le déploiement
        // - Pas de rollback possible
        db.Database.Migrate();
    }
}
```

### Les risques des migrations non contrôlées

```
┌─────────────────────────────────────────────────────────────────┐
│                    MIGRATIONS DANGEREUSES                       │
│                                                                 │
│  ❌ ALTER TABLE en production sans test                         │
│     → Lock table pendant minutes                                │
│     → Timeout pour toutes les requêtes                          │
│                                                                 │
│  ❌ Migration au démarrage de l'app                             │
│     → Toutes les instances migrent en même temps                │
│     → Deadlocks                                                 │
│                                                                 │
│  ❌ Pas de rollback prévu                                       │
│     → Bug en prod → Impossible de revenir                       │
│     → Downtime prolongé                                         │
│                                                                 │
│  ❌ Données perdues                                             │
│     → DROP COLUMN avant migration des données                   │
│     → Données critiques effacées                                │
└─────────────────────────────────────────────────────────────────┘
```

## Décision

**Implémenter une stratégie de migrations avec versioning, rollback, et déploiement zero-downtime.**

### Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    MIGRATION STRATEGY                           │
│                                                                 │
│  1. DÉVELOPPEMENT                                               │
│     ├── Créer migration EF Core                                 │
│     ├── Générer script SQL                                      │
│     └── Review et tests                                         │
│                                                                 │
│  2. CI/CD                                                       │
│     ├── Valider migration (idempotente)                         │
│     ├── Tester sur DB de test                                   │
│     └── Générer artefact migration                              │
│                                                                 │
│  3. DÉPLOIEMENT                                                 │
│     ├── Appliquer migration AVANT déploiement app               │
│     ├── Déployer nouvelle version app                           │
│     └── (Rollback si problème)                                  │
│                                                                 │
│  4. POST-DÉPLOIEMENT                                            │
│     ├── Cleanup (colonnes deprecated)                           │
│     └── Vérification intégrité                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 1. Structure des Migrations

```csharp
/// <summary>
/// Organisation des migrations par version.
/// </summary>
// Structure:
// src/Infrastructure/LLMProxy.Infrastructure.PostgreSQL/
// └── Migrations/
//     ├── V1_0_0/
//     │   ├── 20251221_001_CreateTenantsTable.cs
//     │   └── 20251221_002_CreateApiKeysTable.cs
//     ├── V1_1_0/
//     │   └── 20251222_001_AddQuotaColumns.cs
//     └── Scripts/
//         ├── V1_0_0_to_V1_1_0.sql
//         └── V1_1_0_to_V1_0_0_rollback.sql

/// <summary>
/// Migration de base avec métadonnées.
/// </summary>
public abstract class LlmProxyMigration : Migration
{
    /// <summary>Version sémantique de la migration.</summary>
    public abstract string Version { get; }
    
    /// <summary>Description pour les logs.</summary>
    public abstract string Description { get; }
    
    /// <summary>Estimation du temps d'exécution.</summary>
    public virtual TimeSpan EstimatedDuration => TimeSpan.FromSeconds(30);
    
    /// <summary>La migration est-elle réversible ?</summary>
    public virtual bool IsReversible => true;
    
    /// <summary>Nécessite-t-elle un lock exclusif ?</summary>
    public virtual bool RequiresExclusiveLock => false;
}
```

### 2. Migration EF Core avec Bonnes Pratiques

```csharp
/// <summary>
/// Exemple de migration bien structurée.
/// </summary>
[Migration("20251221143000_AddUsageStatistics")]
public partial class AddUsageStatistics : LlmProxyMigration
{
    public override string Version => "1.2.0";
    public override string Description => "Ajout des tables de statistiques d'usage";
    public override TimeSpan EstimatedDuration => TimeSpan.FromMinutes(2);
    
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // 1. Créer la nouvelle table (non-bloquant)
        migrationBuilder.CreateTable(
            name: "usage_statistics",
            schema: "statistics",
            columns: table => new
            {
                id = table.Column<Guid>(nullable: false),
                tenant_id = table.Column<Guid>(nullable: false),
                date = table.Column<DateOnly>(nullable: false),
                request_count = table.Column<long>(nullable: false, defaultValue: 0),
                token_count = table.Column<long>(nullable: false, defaultValue: 0),
                created_at = table.Column<DateTime>(nullable: false),
                updated_at = table.Column<DateTime>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_usage_statistics", x => x.id);
                table.ForeignKey(
                    name: "fk_usage_statistics_tenant",
                    column: x => x.tenant_id,
                    principalSchema: "tenants",
                    principalTable: "tenants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });
        
        // 2. Index pour les requêtes fréquentes (CONCURRENTLY sur PostgreSQL)
        migrationBuilder.Sql(@"
            CREATE INDEX CONCURRENTLY IF NOT EXISTS 
            ix_usage_statistics_tenant_date 
            ON statistics.usage_statistics (tenant_id, date);
        ");
        
        // 3. Commentaire pour documentation
        migrationBuilder.Sql(@"
            COMMENT ON TABLE statistics.usage_statistics IS 
            'Statistiques d''usage agrégées par tenant et par jour. V1.2.0';
        ");
    }
    
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Rollback complet
        migrationBuilder.DropTable(
            name: "usage_statistics",
            schema: "statistics");
    }
}
```

### 3. Expand/Contract Pattern pour Zero-Downtime

```csharp
/// <summary>
/// Pattern Expand/Contract pour renommer une colonne sans downtime.
/// </summary>
// Phase 1: EXPAND - Ajouter la nouvelle colonne
[Migration("20251221_001_ExpandAddNewColumn")]
public partial class ExpandAddNewColumn : LlmProxyMigration
{
    public override string Version => "1.3.0-expand";
    public override string Description => "Expand: Ajouter colonne email pour remplacer mail";
    
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Ajouter nouvelle colonne (nullable pour compatibilité)
        migrationBuilder.AddColumn<string>(
            name: "email",
            schema: "users",
            table: "users",
            type: "varchar(255)",
            nullable: true);
        
        // Trigger pour synchroniser les deux colonnes
        migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION sync_email_columns()
            RETURNS TRIGGER AS $$
            BEGIN
                IF TG_OP = 'INSERT' OR TG_OP = 'UPDATE' THEN
                    IF NEW.email IS NULL AND NEW.mail IS NOT NULL THEN
                        NEW.email := NEW.mail;
                    ELSIF NEW.mail IS NULL AND NEW.email IS NOT NULL THEN
                        NEW.mail := NEW.email;
                    END IF;
                END IF;
                RETURN NEW;
            END;
            $$ LANGUAGE plpgsql;
            
            CREATE TRIGGER trg_sync_email
            BEFORE INSERT OR UPDATE ON users.users
            FOR EACH ROW EXECUTE FUNCTION sync_email_columns();
        ");
        
        // Migrer les données existantes
        migrationBuilder.Sql(@"
            UPDATE users.users SET email = mail WHERE email IS NULL;
        ");
    }
    
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_sync_email ON users.users;");
        migrationBuilder.Sql("DROP FUNCTION IF EXISTS sync_email_columns();");
        migrationBuilder.DropColumn(name: "email", schema: "users", table: "users");
    }
}

// Phase 2: Déployer le code utilisant la nouvelle colonne
// (Le code doit lire/écrire les DEUX colonnes pendant la transition)

// Phase 3: CONTRACT - Supprimer l'ancienne colonne
[Migration("20251222_001_ContractRemoveOldColumn")]
public partial class ContractRemoveOldColumn : LlmProxyMigration
{
    public override string Version => "1.3.0-contract";
    public override string Description => "Contract: Supprimer ancienne colonne mail";
    public override bool IsReversible => false; // Attention !
    
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Supprimer le trigger de synchronisation
        migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_sync_email ON users.users;");
        migrationBuilder.Sql("DROP FUNCTION IF EXISTS sync_email_columns();");
        
        // Rendre la nouvelle colonne NOT NULL
        migrationBuilder.AlterColumn<string>(
            name: "email",
            schema: "users",
            table: "users",
            nullable: false,
            oldNullable: true);
        
        // Supprimer l'ancienne colonne
        migrationBuilder.DropColumn(
            name: "mail",
            schema: "users",
            table: "users");
    }
    
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // NON RÉVERSIBLE - Les données de 'mail' sont perdues
        throw new InvalidOperationException(
            "Cette migration n'est pas réversible. " +
            "Restaurer depuis backup si nécessaire.");
    }
}
```

### 4. Service de Migration Contrôlé

```csharp
/// <summary>
/// Service pour appliquer les migrations de manière contrôlée.
/// </summary>
public interface IMigrationService
{
    Task<MigrationResult> ApplyPendingMigrationsAsync(CancellationToken ct = default);
    Task<MigrationResult> RollbackToVersionAsync(string version, CancellationToken ct = default);
    Task<IReadOnlyList<MigrationInfo>> GetPendingMigrationsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MigrationInfo>> GetAppliedMigrationsAsync(CancellationToken ct = default);
}

public sealed class MigrationService : IMigrationService
{
    private readonly LlmProxyDbContext _dbContext;
    private readonly ILogger<MigrationService> _logger;
    private readonly IDistributedLock _distributedLock;
    
    public async Task<MigrationResult> ApplyPendingMigrationsAsync(CancellationToken ct = default)
    {
        // Acquérir un lock distribué pour éviter les migrations concurrentes
        await using var lockHandle = await _distributedLock.AcquireAsync(
            "db-migration-lock",
            TimeSpan.FromMinutes(30),
            ct);
        
        if (lockHandle is null)
        {
            return MigrationResult.Failed("Could not acquire migration lock. Another migration in progress?");
        }
        
        var pending = await GetPendingMigrationsAsync(ct);
        
        if (!pending.Any())
        {
            _logger.LogInformation("No pending migrations");
            return MigrationResult.Success(Array.Empty<string>());
        }
        
        _logger.LogWarning(
            "Applying {Count} pending migrations: {Migrations}",
            pending.Count,
            string.Join(", ", pending.Select(m => m.Name)));
        
        var applied = new List<string>();
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            foreach (var migration in pending)
            {
                _logger.LogInformation(
                    "Applying migration {Name} (estimated: {Duration})...",
                    migration.Name,
                    migration.EstimatedDuration);
                
                var migrationStopwatch = Stopwatch.StartNew();
                
                await _dbContext.Database.ExecuteSqlRawAsync(
                    migration.UpScript,
                    ct);
                
                // Enregistrer la migration dans la table historique
                await RecordMigrationAsync(migration, ct);
                
                applied.Add(migration.Name);
                
                _logger.LogInformation(
                    "Migration {Name} applied in {Duration}ms",
                    migration.Name,
                    migrationStopwatch.ElapsedMilliseconds);
            }
            
            _logger.LogInformation(
                "All {Count} migrations applied successfully in {Duration}ms",
                applied.Count,
                stopwatch.ElapsedMilliseconds);
            
            return MigrationResult.Success(applied);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Migration failed after applying: {Applied}", applied);
            return MigrationResult.Failed(ex.Message, applied);
        }
    }
    
    public async Task<MigrationResult> RollbackToVersionAsync(
        string targetVersion,
        CancellationToken ct = default)
    {
        await using var lockHandle = await _distributedLock.AcquireAsync(
            "db-migration-lock",
            TimeSpan.FromMinutes(30),
            ct);
        
        var applied = await GetAppliedMigrationsAsync(ct);
        var toRollback = applied
            .Where(m => string.Compare(m.Version, targetVersion, StringComparison.Ordinal) > 0)
            .OrderByDescending(m => m.AppliedAt)
            .ToList();
        
        if (!toRollback.Any())
        {
            return MigrationResult.Success(Array.Empty<string>());
        }
        
        // Vérifier que toutes sont réversibles
        var nonReversible = toRollback.Where(m => !m.IsReversible).ToList();
        if (nonReversible.Any())
        {
            return MigrationResult.Failed(
                $"Cannot rollback: {string.Join(", ", nonReversible.Select(m => m.Name))} are not reversible");
        }
        
        _logger.LogWarning(
            "Rolling back {Count} migrations to version {Version}",
            toRollback.Count,
            targetVersion);
        
        var rolledBack = new List<string>();
        
        foreach (var migration in toRollback)
        {
            _logger.LogInformation("Rolling back {Name}...", migration.Name);
            
            await _dbContext.Database.ExecuteSqlRawAsync(migration.DownScript, ct);
            await RemoveMigrationRecordAsync(migration, ct);
            
            rolledBack.Add(migration.Name);
        }
        
        return MigrationResult.Success(rolledBack);
    }
}
```

### 5. CLI de Migration

```csharp
/// <summary>
/// Commandes CLI pour les migrations.
/// </summary>
// Usage: dotnet run --project src/Tools/MigrationTool -- [command] [options]

public sealed class MigrationCommands
{
    private readonly IMigrationService _migrationService;
    
    [Command("status")]
    public async Task<int> StatusAsync()
    {
        var applied = await _migrationService.GetAppliedMigrationsAsync();
        var pending = await _migrationService.GetPendingMigrationsAsync();
        
        Console.WriteLine("=== Applied Migrations ===");
        foreach (var m in applied)
        {
            Console.WriteLine($"  ✅ {m.Name} (v{m.Version}) - Applied: {m.AppliedAt:u}");
        }
        
        Console.WriteLine("\n=== Pending Migrations ===");
        foreach (var m in pending)
        {
            Console.WriteLine($"  ⏳ {m.Name} (v{m.Version}) - Est: {m.EstimatedDuration}");
        }
        
        return pending.Any() ? 1 : 0; // Exit code pour CI/CD
    }
    
    [Command("apply")]
    public async Task<int> ApplyAsync(
        [Option("--dry-run")] bool dryRun = false)
    {
        if (dryRun)
        {
            var pending = await _migrationService.GetPendingMigrationsAsync();
            Console.WriteLine($"Would apply {pending.Count} migrations:");
            foreach (var m in pending)
            {
                Console.WriteLine($"  - {m.Name}");
            }
            return 0;
        }
        
        var result = await _migrationService.ApplyPendingMigrationsAsync();
        
        if (result.IsSuccess)
        {
            Console.WriteLine($"✅ Applied {result.AppliedMigrations.Count} migrations");
            return 0;
        }
        
        Console.Error.WriteLine($"❌ Migration failed: {result.Error}");
        return 1;
    }
    
    [Command("rollback")]
    public async Task<int> RollbackAsync(
        [Argument] string targetVersion,
        [Option("--confirm")] bool confirm = false)
    {
        if (!confirm)
        {
            Console.Error.WriteLine("⚠️ Rollback requires --confirm flag");
            return 1;
        }
        
        var result = await _migrationService.RollbackToVersionAsync(targetVersion);
        
        if (result.IsSuccess)
        {
            Console.WriteLine($"✅ Rolled back {result.AppliedMigrations.Count} migrations");
            return 0;
        }
        
        Console.Error.WriteLine($"❌ Rollback failed: {result.Error}");
        return 1;
    }
    
    [Command("generate-script")]
    public async Task GenerateScriptAsync(
        [Option("--from")] string? fromVersion = null,
        [Option("--to")] string? toVersion = null,
        [Option("--output")] string? outputPath = null)
    {
        var script = await _migrationService.GenerateSqlScriptAsync(fromVersion, toVersion);
        
        if (outputPath is not null)
        {
            await File.WriteAllTextAsync(outputPath, script);
            Console.WriteLine($"Script written to {outputPath}");
        }
        else
        {
            Console.WriteLine(script);
        }
    }
}
```

### 6. Validation des Migrations en CI/CD

```csharp
/// <summary>
/// Tests automatisés pour les migrations.
/// </summary>
public sealed class MigrationTests
{
    [Fact]
    public async Task All_Migrations_Are_Idempotent()
    {
        await using var container = new PostgreSqlContainer()
            .WithImage("postgres:16")
            .Build();
        
        await container.StartAsync();
        
        var options = new DbContextOptionsBuilder<LlmProxyDbContext>()
            .UseNpgsql(container.GetConnectionString())
            .Options;
        
        // Appliquer toutes les migrations
        await using (var context = new LlmProxyDbContext(options))
        {
            await context.Database.MigrateAsync();
        }
        
        // Ré-appliquer (doit être idempotent)
        await using (var context = new LlmProxyDbContext(options))
        {
            // Ne doit pas lever d'exception
            await context.Database.MigrateAsync();
        }
    }
    
    [Fact]
    public async Task Migrations_Are_Reversible()
    {
        await using var container = new PostgreSqlContainer().Build();
        await container.StartAsync();
        
        var options = new DbContextOptionsBuilder<LlmProxyDbContext>()
            .UseNpgsql(container.GetConnectionString())
            .Options;
        
        // Appliquer toutes les migrations
        await using var context = new LlmProxyDbContext(options);
        await context.Database.MigrateAsync();
        
        // Vérifier que chaque migration réversible peut être rollback
        var migrations = context.Database.GetMigrations().ToList();
        
        foreach (var migration in migrations.AsEnumerable().Reverse())
        {
            // Simuler rollback (vérifier que Down() ne lève pas)
            await context.Database.ExecuteSqlRawAsync(
                $"SELECT 1; -- Placeholder for rollback test of {migration}");
        }
    }
    
    [Fact]
    public async Task New_Migration_Does_Not_Break_Existing_Data()
    {
        // Setup avec données de test
        await using var container = new PostgreSqlContainer().Build();
        await container.StartAsync();
        
        var options = new DbContextOptionsBuilder<LlmProxyDbContext>()
            .UseNpgsql(container.GetConnectionString())
            .Options;
        
        // Appliquer migrations N-1
        // Insérer données de test
        // Appliquer migration N
        // Vérifier intégrité des données
    }
}
```

### 7. Job Kubernetes pour Migration

```yaml
# migration-job.yaml
apiVersion: batch/v1
kind: Job
metadata:
  name: llm-proxy-migration-{{ .Release.Revision }}
  annotations:
    "helm.sh/hook": pre-upgrade
    "helm.sh/hook-weight": "-5"
    "helm.sh/hook-delete-policy": before-hook-creation
spec:
  backoffLimit: 1
  template:
    spec:
      restartPolicy: Never
      containers:
      - name: migration
        image: {{ .Values.image.repository }}:{{ .Values.image.tag }}
        command: ["dotnet", "MigrationTool.dll", "apply"]
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: llm-proxy-db
              key: connection-string
        resources:
          limits:
            memory: "256Mi"
            cpu: "500m"
```

## Conséquences

### Positives

- **Zero-downtime** : Expand/Contract pour évolutions
- **Traçabilité** : Historique complet des changements
- **Rollback** : Retour arrière possible
- **Automatisation** : CI/CD intégré

### Négatives

- **Complexité** : Plus de migrations pour un changement
  - *Mitigation* : Tooling et documentation
- **Délai** : Expand/Contract = 2 déploiements
  - *Mitigation* : Acceptable pour la stabilité
- **Coordination** : Sync code/migrations
  - *Mitigation* : Migration AVANT déploiement code

### Neutres

- Standard EF Core
- Compatible tous environnements

## Alternatives considérées

### Option A : Migration auto au démarrage

- **Description** : `db.Database.Migrate()` dans Startup
- **Avantages** : Simple
- **Inconvénients** : Dangereux en production
- **Raison du rejet** : Risque de deadlock, pas de contrôle

### Option B : SQL pur (sans EF migrations)

- **Description** : Scripts SQL versionnés manuellement
- **Avantages** : Contrôle total
- **Inconvénients** : Pas de génération automatique
- **Raison du rejet** : Productivité réduite

## Références

- [EF Core Migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Evolutionary Database Design](https://martinfowler.com/articles/evodb.html)
- [Expand and Contract Pattern](https://www.tim-wellhausen.de/papers/ExpandAndContract.pdf)
