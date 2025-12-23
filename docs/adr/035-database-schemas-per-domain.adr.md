# 35. Schémas de Base de Données par Domaine

Date: 2025-12-21

## Statut

Accepté

## Contexte

L'utilisation d'un schéma unique (`public` ou `dbo`) pour toutes les tables pose problème :
- **Confusion** : Difficile de savoir quel domaine possède quelle table
- **Conflits de noms** : Préfixes obligatoires (`auth_users`, `billing_invoices`)
- **Permissions** : Impossible d'isoler les accès par domaine
- **Migration** : Difficile d'extraire un domaine en microservice

```sql
-- ❌ SCHÉMA UNIQUE : Tout mélangé dans public
public.users
public.user_roles
public.api_keys
public.tenants
public.tenant_quotas
public.llm_requests
public.llm_responses
public.invoices
public.invoice_lines
public.audit_logs
-- 50 tables dans le même namespace...
-- Qui possède quoi ? Quelles sont les frontières ?
```

## Décision

**Utiliser les schémas PostgreSQL pour séparer les tables par domaine métier (Bounded Context), reflétant l'architecture logicielle dans la structure de la base de données.**

### 1. Organisation des schémas par domaine

```sql
-- ═══════════════════════════════════════════════════════════════
-- SCHÉMAS PAR BOUNDED CONTEXT
-- ═══════════════════════════════════════════════════════════════

-- Domaine : Gestion des tenants (multi-tenancy)
CREATE SCHEMA IF NOT EXISTS tenants;
COMMENT ON SCHEMA tenants IS 'Gestion des organisations clientes et leur configuration';

-- Domaine : Authentification et autorisation
CREATE SCHEMA IF NOT EXISTS auth;
COMMENT ON SCHEMA auth IS 'Identités, credentials, roles et permissions';

-- Domaine : Clés API et accès
CREATE SCHEMA IF NOT EXISTS apikeys;
COMMENT ON SCHEMA apikeys IS 'Gestion des clés API et leur cycle de vie';

-- Domaine : Quotas et limitations
CREATE SCHEMA IF NOT EXISTS quotas;
COMMENT ON SCHEMA quotas IS 'Quotas, limites et usage par tenant';

-- Domaine : Métriques et statistiques
CREATE SCHEMA IF NOT EXISTS metrics;
COMMENT ON SCHEMA metrics IS 'Métriques d''usage, statistiques et analytics';

-- Domaine : Facturation
CREATE SCHEMA IF NOT EXISTS billing;
COMMENT ON SCHEMA billing IS 'Facturation, tarifs et paiements';

-- Domaine : Audit et traçabilité
CREATE SCHEMA IF NOT EXISTS audit;
COMMENT ON SCHEMA audit IS 'Logs d''audit, historique des actions';

-- Domaine : Configuration
CREATE SCHEMA IF NOT EXISTS config;
COMMENT ON SCHEMA config IS 'Feature flags, paramètres système';
```

### 2. Tables dans leurs schémas respectifs

```sql
-- ═══════════════════════════════════════════════════════════════
-- SCHÉMA TENANTS
-- ═══════════════════════════════════════════════════════════════

CREATE TABLE tenants.tenants (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    slug VARCHAR(100) NOT NULL UNIQUE,
    status VARCHAR(50) NOT NULL DEFAULT 'active',
    settings JSONB NOT NULL DEFAULT '{}',
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE tenants.tenant_contacts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants.tenants(id),
    type VARCHAR(50) NOT NULL, -- 'billing', 'technical', 'admin'
    email VARCHAR(255) NOT NULL,
    name VARCHAR(255),
    phone VARCHAR(50)
);

CREATE INDEX idx_tenant_contacts_tenant ON tenants.tenant_contacts(tenant_id);

-- ═══════════════════════════════════════════════════════════════
-- SCHÉMA AUTH
-- ═══════════════════════════════════════════════════════════════

CREATE TABLE auth.users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL, -- Référence cross-schema
    email VARCHAR(255) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'active',
    last_login_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    UNIQUE(tenant_id, email)
);

CREATE TABLE auth.roles (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    permissions JSONB NOT NULL DEFAULT '[]',
    is_system BOOLEAN NOT NULL DEFAULT false,
    UNIQUE(tenant_id, name)
);

CREATE TABLE auth.user_roles (
    user_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
    role_id UUID NOT NULL REFERENCES auth.roles(id) ON DELETE CASCADE,
    assigned_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (user_id, role_id)
);

-- ═══════════════════════════════════════════════════════════════
-- SCHÉMA APIKEYS
-- ═══════════════════════════════════════════════════════════════

CREATE TABLE apikeys.api_keys (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
    name VARCHAR(255) NOT NULL,
    key_hash VARCHAR(255) NOT NULL UNIQUE,
    key_prefix VARCHAR(10) NOT NULL, -- Pour identification (sk_live_xxx...)
    scopes JSONB NOT NULL DEFAULT '["*"]',
    rate_limit_per_minute INT,
    expires_at TIMESTAMPTZ,
    last_used_at TIMESTAMPTZ,
    status VARCHAR(50) NOT NULL DEFAULT 'active',
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_api_keys_tenant ON apikeys.api_keys(tenant_id);
CREATE INDEX idx_api_keys_prefix ON apikeys.api_keys(key_prefix);

-- ═══════════════════════════════════════════════════════════════
-- SCHÉMA QUOTAS
-- ═══════════════════════════════════════════════════════════════

CREATE TABLE quotas.quota_definitions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    default_limit BIGINT NOT NULL,
    reset_period VARCHAR(50) NOT NULL, -- 'monthly', 'daily', 'none'
    unit VARCHAR(50) NOT NULL -- 'tokens', 'requests', 'bytes'
);

CREATE TABLE quotas.tenant_quotas (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
    quota_id UUID NOT NULL REFERENCES quotas.quota_definitions(id),
    limit_value BIGINT NOT NULL,
    current_usage BIGINT NOT NULL DEFAULT 0,
    reset_at TIMESTAMPTZ,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    UNIQUE(tenant_id, quota_id)
);

CREATE INDEX idx_tenant_quotas_tenant ON quotas.tenant_quotas(tenant_id);

-- ═══════════════════════════════════════════════════════════════
-- SCHÉMA METRICS
-- ═══════════════════════════════════════════════════════════════

CREATE TABLE metrics.llm_requests (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
    api_key_id UUID NOT NULL,
    model VARCHAR(100) NOT NULL,
    provider VARCHAR(50) NOT NULL,
    input_tokens INT NOT NULL,
    output_tokens INT NOT NULL,
    duration_ms INT NOT NULL,
    status VARCHAR(50) NOT NULL,
    error_code VARCHAR(100),
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- Partitionnement par date pour les grosses tables
CREATE TABLE metrics.llm_requests_partitioned (
    id UUID NOT NULL DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    -- ... autres colonnes
    PRIMARY KEY (id, created_at)
) PARTITION BY RANGE (created_at);

-- Partitions mensuelles
CREATE TABLE metrics.llm_requests_2025_01 
    PARTITION OF metrics.llm_requests_partitioned
    FOR VALUES FROM ('2025-01-01') TO ('2025-02-01');

-- ═══════════════════════════════════════════════════════════════
-- SCHÉMA AUDIT
-- ═══════════════════════════════════════════════════════════════

CREATE TABLE audit.audit_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID,
    user_id UUID,
    action VARCHAR(100) NOT NULL,
    entity_type VARCHAR(100) NOT NULL,
    entity_id UUID,
    old_values JSONB,
    new_values JSONB,
    ip_address INET,
    user_agent TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_audit_logs_tenant ON audit.audit_logs(tenant_id);
CREATE INDEX idx_audit_logs_entity ON audit.audit_logs(entity_type, entity_id);
CREATE INDEX idx_audit_logs_created ON audit.audit_logs(created_at);
```

### 3. Références cross-schema

```sql
-- ═══════════════════════════════════════════════════════════════
-- CONTRAINTES DE RÉFÉRENCES ENTRE SCHÉMAS
-- ═══════════════════════════════════════════════════════════════

-- auth.users référence tenants.tenants
ALTER TABLE auth.users
    ADD CONSTRAINT fk_users_tenant 
    FOREIGN KEY (tenant_id) 
    REFERENCES tenants.tenants(id) 
    ON DELETE CASCADE;

-- apikeys.api_keys référence tenants.tenants
ALTER TABLE apikeys.api_keys
    ADD CONSTRAINT fk_api_keys_tenant 
    FOREIGN KEY (tenant_id) 
    REFERENCES tenants.tenants(id) 
    ON DELETE CASCADE;

-- quotas.tenant_quotas référence tenants.tenants
ALTER TABLE quotas.tenant_quotas
    ADD CONSTRAINT fk_tenant_quotas_tenant 
    FOREIGN KEY (tenant_id) 
    REFERENCES tenants.tenants(id) 
    ON DELETE CASCADE;

-- ═══════════════════════════════════════════════════════════════
-- VUES CROSS-SCHEMA (pour requêtes complexes)
-- ═══════════════════════════════════════════════════════════════

CREATE VIEW tenants.tenant_overview AS
SELECT 
    t.id,
    t.name,
    t.status,
    (SELECT COUNT(*) FROM auth.users u WHERE u.tenant_id = t.id) as user_count,
    (SELECT COUNT(*) FROM apikeys.api_keys k WHERE k.tenant_id = t.id AND k.status = 'active') as active_keys,
    (SELECT SUM(current_usage) FROM quotas.tenant_quotas q WHERE q.tenant_id = t.id) as total_usage
FROM tenants.tenants t;
```

### 4. Configuration EF Core avec schémas

```csharp
/// <summary>
/// Configuration EF Core pour les schémas par domaine.
/// </summary>
public class LlmProxyDbContext : DbContext
{
    // Schéma Tenants
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantContact> TenantContacts => Set<TenantContact>();
    
    // Schéma Auth
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    
    // Schéma ApiKeys
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    
    // Schéma Quotas
    public DbSet<QuotaDefinition> QuotaDefinitions => Set<QuotaDefinition>();
    public DbSet<TenantQuota> TenantQuotas => Set<TenantQuota>();
    
    // Schéma Metrics
    public DbSet<LlmRequest> LlmRequests => Set<LlmRequest>();
    
    // Schéma Audit
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ═══ SCHÉMA TENANTS ═══
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("tenants", "tenants");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Slug).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Slug).IsUnique();
        });
        
        modelBuilder.Entity<TenantContact>(entity =>
        {
            entity.ToTable("tenant_contacts", "tenants");
            entity.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(e => e.TenantId);
        });
        
        // ═══ SCHÉMA AUTH ═══
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users", "auth");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();
        });
        
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles", "auth");
            entity.Property(e => e.Permissions)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)!);
        });
        
        // ═══ SCHÉMA APIKEYS ═══
        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.ToTable("api_keys", "apikeys");
            entity.Property(e => e.KeyHash).HasMaxLength(255).IsRequired();
            entity.HasIndex(e => e.KeyHash).IsUnique();
            entity.HasIndex(e => e.KeyPrefix);
        });
        
        // ═══ SCHÉMA QUOTAS ═══
        modelBuilder.Entity<QuotaDefinition>(entity =>
        {
            entity.ToTable("quota_definitions", "quotas");
        });
        
        modelBuilder.Entity<TenantQuota>(entity =>
        {
            entity.ToTable("tenant_quotas", "quotas");
            entity.HasIndex(e => new { e.TenantId, e.QuotaId }).IsUnique();
        });
        
        // ═══ SCHÉMA METRICS ═══
        modelBuilder.Entity<LlmRequest>(entity =>
        {
            entity.ToTable("llm_requests", "metrics");
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.CreatedAt);
        });
        
        // ═══ SCHÉMA AUDIT ═══
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs", "audit");
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
        });
    }
}
```

### 5. Migrations EF Core avec schémas

```csharp
/// <summary>
/// Migration qui crée les schémas.
/// </summary>
public partial class CreateSchemas : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Créer les schémas d'abord
        migrationBuilder.Sql("CREATE SCHEMA IF NOT EXISTS tenants;");
        migrationBuilder.Sql("CREATE SCHEMA IF NOT EXISTS auth;");
        migrationBuilder.Sql("CREATE SCHEMA IF NOT EXISTS apikeys;");
        migrationBuilder.Sql("CREATE SCHEMA IF NOT EXISTS quotas;");
        migrationBuilder.Sql("CREATE SCHEMA IF NOT EXISTS metrics;");
        migrationBuilder.Sql("CREATE SCHEMA IF NOT EXISTS audit;");
        migrationBuilder.Sql("CREATE SCHEMA IF NOT EXISTS config;");
        
        // Commentaires pour documentation
        migrationBuilder.Sql(
            "COMMENT ON SCHEMA tenants IS 'Gestion des organisations clientes';");
        migrationBuilder.Sql(
            "COMMENT ON SCHEMA auth IS 'Authentification et autorisation';");
        // ... etc
    }
    
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Supprimer dans l'ordre inverse (dépendances)
        migrationBuilder.Sql("DROP SCHEMA IF EXISTS config CASCADE;");
        migrationBuilder.Sql("DROP SCHEMA IF EXISTS audit CASCADE;");
        migrationBuilder.Sql("DROP SCHEMA IF EXISTS metrics CASCADE;");
        migrationBuilder.Sql("DROP SCHEMA IF EXISTS quotas CASCADE;");
        migrationBuilder.Sql("DROP SCHEMA IF EXISTS apikeys CASCADE;");
        migrationBuilder.Sql("DROP SCHEMA IF EXISTS auth CASCADE;");
        migrationBuilder.Sql("DROP SCHEMA IF EXISTS tenants CASCADE;");
    }
}
```

### 6. Permissions granulaires par schéma

```sql
-- ═══════════════════════════════════════════════════════════════
-- RÔLES ET PERMISSIONS PAR SCHÉMA
-- ═══════════════════════════════════════════════════════════════

-- Rôle pour l'application principale
CREATE ROLE llmproxy_app;
GRANT USAGE ON SCHEMA tenants, auth, apikeys, quotas, metrics TO llmproxy_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA tenants TO llmproxy_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA auth TO llmproxy_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA apikeys TO llmproxy_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA quotas TO llmproxy_app;
GRANT SELECT, INSERT ON ALL TABLES IN SCHEMA metrics TO llmproxy_app; -- Pas de UPDATE/DELETE

-- Rôle read-only pour le reporting
CREATE ROLE llmproxy_reporting;
GRANT USAGE ON SCHEMA metrics, quotas TO llmproxy_reporting;
GRANT SELECT ON ALL TABLES IN SCHEMA metrics TO llmproxy_reporting;
GRANT SELECT ON ALL TABLES IN SCHEMA quotas TO llmproxy_reporting;

-- Rôle pour l'audit (append-only)
CREATE ROLE llmproxy_audit;
GRANT USAGE ON SCHEMA audit TO llmproxy_audit;
GRANT INSERT ON ALL TABLES IN SCHEMA audit TO llmproxy_audit;
-- Pas de SELECT (audit écrit seulement)

-- Rôle admin pour les migrations
CREATE ROLE llmproxy_admin;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA tenants TO llmproxy_admin;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA auth TO llmproxy_admin;
-- ... etc

-- Utilisateurs
CREATE USER llmproxy_gateway WITH PASSWORD 'xxx';
GRANT llmproxy_app TO llmproxy_gateway;

CREATE USER llmproxy_reporter WITH PASSWORD 'xxx';
GRANT llmproxy_reporting TO llmproxy_reporter;
```

### 7. Requêtes optimisées par schéma

```csharp
/// <summary>
/// Repository qui tire parti de l'organisation par schéma.
/// </summary>
public sealed class TenantRepository : ITenantRepository
{
    private readonly LlmProxyDbContext _context;
    
    /// <summary>
    /// Requête optimisée avec jointures cross-schema.
    /// </summary>
    public async Task<TenantOverview?> GetTenantOverviewAsync(
        Guid tenantId, 
        CancellationToken ct)
    {
        // EF Core gère les schémas automatiquement
        var tenant = await _context.Tenants
            .Where(t => t.Id == tenantId)
            .Select(t => new TenantOverview
            {
                Id = t.Id,
                Name = t.Name,
                Status = t.Status,
                
                // Sous-requête vers schéma auth
                UserCount = _context.Users
                    .Count(u => u.TenantId == t.Id),
                
                // Sous-requête vers schéma apikeys
                ActiveKeyCount = _context.ApiKeys
                    .Count(k => k.TenantId == t.Id && k.Status == "active"),
                
                // Sous-requête vers schéma quotas
                TotalUsage = _context.TenantQuotas
                    .Where(q => q.TenantId == t.Id)
                    .Sum(q => q.CurrentUsage)
            })
            .FirstOrDefaultAsync(ct);
        
        return tenant;
    }
}
```

### 8. Mapping visuel domaine ↔ schéma

```
┌─────────────────────────────────────────────────────────────────┐
│                    ARCHITECTURE DOMAINE                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐       │
│  │   Tenants    │    │     Auth     │    │   ApiKeys    │       │
│  │   Domain     │───▶│    Domain    │◀───│   Domain     │       │
│  └──────────────┘    └──────────────┘    └──────────────┘       │
│         │                   │                   │                │
│         │                   │                   │                │
│         ▼                   ▼                   ▼                │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐       │
│  │   SCHEMA     │    │   SCHEMA     │    │   SCHEMA     │       │
│  │   tenants    │    │     auth     │    │   apikeys    │       │
│  │              │    │              │    │              │       │
│  │ • tenants    │    │ • users      │    │ • api_keys   │       │
│  │ • contacts   │    │ • roles      │    │              │       │
│  └──────────────┘    └──────────────┘    └──────────────┘       │
│                                                                  │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐       │
│  │   Quotas     │    │   Metrics    │    │    Audit     │       │
│  │   Domain     │    │   Domain     │    │   Domain     │       │
│  └──────────────┘    └──────────────┘    └──────────────┘       │
│         │                   │                   │                │
│         ▼                   ▼                   ▼                │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐       │
│  │   SCHEMA     │    │   SCHEMA     │    │   SCHEMA     │       │
│  │   quotas     │    │   metrics    │    │    audit     │       │
│  │              │    │              │    │              │       │
│  │ • definitions│    │ • llm_reqs   │    │ • audit_logs │       │
│  │ • tenant_q.  │    │ • stats      │    │              │       │
│  └──────────────┘    └──────────────┘    └──────────────┘       │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

## Conséquences

### Positives

- **Clarté** : Organisation visuelle des domaines
- **Isolation** : Permissions granulaires par schéma
- **Évolution** : Extraire un schéma en microservice facilité
- **Maintenance** : Noms courts sans préfixes (`users` vs `auth_users`)
- **Documentation** : Structure auto-documentée

### Négatives

- **Jointures cross-schema** : Légèrement plus verbeux en SQL brut
  - *Mitigation* : EF Core gère transparemment
- **Migrations** : Créer les schémas en premier
  - *Mitigation* : Migration initiale dédiée
- **Backup** : Plus complexe si backup par schéma
  - *Mitigation* : Backup base complète standard

### Neutres

- PostgreSQL supporte nativement les schémas
- Pattern standard dans les architectures DDD

## Alternatives considérées

### Option A : Préfixes de tables

- **Description** : `auth_users`, `tenant_contacts`, etc.
- **Avantages** : Simple, pas de schémas à gérer
- **Inconvénients** : Pas d'isolation, noms longs
- **Raison du rejet** : Ne scale pas, pas de permissions granulaires

### Option B : Bases de données séparées

- **Description** : Une DB par domaine
- **Avantages** : Isolation totale
- **Inconvénients** : Pas de FK cross-DB, transactions distribuées
- **Raison du rejet** : Complexité excessive pour le besoin

## Références

- [PostgreSQL Schemas](https://www.postgresql.org/docs/current/ddl-schemas.html)
- [EF Core Schema Configuration](https://docs.microsoft.com/en-us/ef/core/modeling/relational/tables)
- [DDD Bounded Contexts](https://martinfowler.com/bliki/BoundedContext.html)
- [Database per Service pattern](https://microservices.io/patterns/data/database-per-service.html)
