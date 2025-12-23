# Tâche 025 - Implémenter ADR-038 : Health Checks Complets

**Statut** : À faire  
**Priorité** : 🟡 MOYENNE (P2)  
**Conformité cible** : ADR-038 de 40% → 95%  
**Dépendances** : Aucune

## CONTEXTE

**Analyse ADR-038** : `docs/ANALYSE_CONFORMITE_ADR-031-041.md` (lignes 962-1156)  
**ADR** : `docs/adr/038-health-checks-readiness-probes.adr.md`

**Conformité actuelle** : **40%** (health checks basiques uniquement)

**Problème identifié** :
- 🟡 **Endpoint `/health` basique** : Retourne toujours 200 OK (non informatif)
- 🟡 **Pas de vérification dépendances** : PostgreSQL, Redis, LLM providers non testés
- 🟡 **Pas de distinction liveness/readiness** : Kubernetes probes non supportées
- 🟡 **Pas de health checks UI** : Monitoring manuel difficile
- 🟡 **Pas de checks personnalisés** : Quotas, disk, memory non surveillés

**Risques sans health checks complets** :
- 🟡 **Déploiements dangereux** : Kubernetes démarre pods non prêts (DB down)
- 🟡 **Debugging difficile** : Impossible savoir quelle dépendance en panne
- 🟡 **Monitoring incomplet** : Pas d'alerting automatique sur dégradations
- 🟡 **Rolling updates risqués** : Pas de garantie que nouvelle instance fonctionne
- 🟡 **Coûts élevés** : Instances non healthy continuent de consommer ressources

**Violations détectées** :
```csharp
// ❌ V-HC-001 : Health check trop basique (Program.cs)
app.MapHealthChecks("/health");  // Retourne toujours 200 OK !

// ❌ V-HC-002 : Aucune vérification PostgreSQL
// ❌ V-HC-003 : Aucune vérification Redis  
// ❌ V-HC-004 : Aucune vérification providers LLM
// ❌ V-HC-005 : Pas de distinction /health/live vs /health/ready
// ❌ V-HC-006 : Pas de UI pour visualiser statut dépendances
```

## OBJECTIF

Implémenter health checks complets avec vérifications de toutes les dépendances critiques.

**Spécifications ADR-038** :
- Endpoint `/health/live` : Liveness probe (processus vivant)
- Endpoint `/health/ready` : Readiness probe (toutes dépendances OK)
- Health checks PostgreSQL, Redis, Disk, Memory
- Health UI pour visualisation (optionnel pour dev/staging)
- Tags pour grouper checks (liveness, readiness)
- HealthStatus: Healthy, Degraded, Unhealthy

## CRITÈRES DE SUCCÈS

### Fonctionnels
- [ ] Endpoint `/health/live` retourne 200 si processus actif
- [ ] Endpoint `/health/ready` retourne 200 si toutes dépendances OK
- [ ] PostgreSQL vérifié (connexion + simple query)
- [ ] Redis vérifié (ping command)
- [ ] Disk space vérifié (minimum 1GB libre)
- [ ] Memory vérifié (maximum 2GB allouée)
- [ ] Health UI accessible sur `/healthchecks-ui` (dev/staging)
- [ ] Réponse JSON structurée avec détail par dépendance

### Techniques
- [ ] Package `AspNetCore.HealthChecks.NpgSql` installé
- [ ] Package `AspNetCore.HealthChecks.Redis` installé
- [ ] Package `AspNetCore.HealthChecks.System` installé
- [ ] Package `AspNetCore.HealthChecks.UI` installé (dev/staging)
- [ ] Configuration dans `Program.cs`
- [ ] Tests unitaires : au moins 5 scénarios

### Qualité
- [ ] **Build** : 0 erreurs, 0 warnings
- [ ] **Tests** : 5+ nouveaux tests health checks
  - Liveness probe toujours OK
  - Readiness probe OK si deps OK
  - Readiness probe Unhealthy si PostgreSQL down
  - Readiness probe Degraded si Redis down
  - JSON response structure valide
- [ ] Tests existants : 100% passing (non-régression)
- [ ] Documentation README.md mise à jour

## ÉTAPES D'IMPLÉMENTATION

### 1. Installer packages NuGet (30 min)

**Packages à ajouter** :

```powershell
# Health checks core
dotnet add src/Presentation/LLMProxy.Gateway/LLMProxy.Gateway.csproj package AspNetCore.HealthChecks.NpgSql

dotnet add src/Presentation/LLMProxy.Gateway/LLMProxy.Gateway.csproj package AspNetCore.HealthChecks.Redis

dotnet add src/Presentation/LLMProxy.Gateway/LLMProxy.Gateway.csproj package AspNetCore.HealthChecks.System

# Health checks UI (pour dev/staging uniquement)
dotnet add src/Presentation/LLMProxy.Gateway/LLMProxy.Gateway.csproj package AspNetCore.HealthChecks.UI
dotnet add src/Presentation/LLMProxy.Gateway/LLMProxy.Gateway.csproj package AspNetCore.HealthChecks.UI.Client
dotnet add src/Presentation/LLMProxy.Gateway/LLMProxy.Gateway.csproj package AspNetCore.HealthChecks.UI.InMemory.Storage
```

**Action** : Installer les 6 packages NuGet.

---

### 2. Configurer Health Checks dans Program.cs (2h)

**Fichier** : `src/Presentation/LLMProxy.Gateway/Program.cs`

**Modification** : Remplacer le health check basique par checks complets.

```csharp
// ═══════════════════════════════════════════════════════════════
// HEALTH CHECKS (ADR-038)
// ═══════════════════════════════════════════════════════════════

builder.Services.AddHealthChecks()
    
    // ═══ LIVENESS CHECKS (Process vivant) ═══
    .AddCheck("self", () => HealthCheckResult.Healthy("Gateway is alive"), tags: new[] { "live" })
    
    // ═══ READINESS CHECKS (Dépendances critiques) ═══
    
    // PostgreSQL - Base de données principale
    .AddNpgSql(
        builder.Configuration.GetConnectionString("PostgreSQL")!,
        name: "postgresql",
        failureStatus: HealthStatus.Unhealthy, // CRITIQUE : app non fonctionnelle sans DB
        tags: new[] { "ready", "db" },
        timeout: TimeSpan.FromSeconds(5))
    
    // Redis - Cache et quotas temps réel
    .AddRedis(
        builder.Configuration.GetConnectionString("Redis")!,
        name: "redis",
        failureStatus: HealthStatus.Degraded, // Dégradé mais app reste fonctionnelle
        tags: new[] { "ready", "cache" },
        timeout: TimeSpan.FromSeconds(3))
    
    // ═══ INFRASTRUCTURE CHECKS ═══
    
    // Disk Space - Minimum 1GB disponible
    .AddDiskStorageHealthCheck(
        options => options.AddDrive("C:\\", 1024), // 1GB minimum
        name: "disk",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "ready", "infrastructure" })
    
    // Memory - Maximum 2GB allouée au processus
    .AddProcessAllocatedMemoryHealthCheck(
        maximumMegabytesAllocated: 2048, // 2GB max
        name: "memory",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "ready", "infrastructure" });

// ═══ HEALTH CHECKS UI (Dev/Staging uniquement) ═══
if (builder.Environment.IsDevelopment() || builder.Environment.IsStaging())
{
    builder.Services.AddHealthChecksUI(setup =>
    {
        setup.SetEvaluationTimeInSeconds(30); // Refresh toutes les 30s
        setup.MaximumHistoryEntriesPerEndpoint(50);
        setup.AddHealthCheckEndpoint("Gateway", "/health");
    })
    .AddInMemoryStorage();
}
```

**Action** : Ajouter configuration health checks avec PostgreSQL, Redis, Disk, Memory.

---

### 3. Configurer endpoints health checks (1h)

**Fichier** : `src/Presentation/LLMProxy.Gateway/Program.cs`

**Modification** : Remplacer `app.MapHealthChecks("/health")` par endpoints multiples.

```csharp
// ═══════════════════════════════════════════════════════════════
// HEALTH CHECK ENDPOINTS (ADR-038)
// ═══════════════════════════════════════════════════════════════

// Liveness probe - Pour Kubernetes liveness
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse // JSON détaillé
});

// Readiness probe - Pour Kubernetes readiness + Load Balancer
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse // JSON détaillé
});

// Endpoint complet (tous les checks) - Pour monitoring
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse // JSON détaillé
});

// Health Checks UI (Dev/Staging uniquement)
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.MapHealthChecksUI(options =>
    {
        options.UIPath = "/healthchecks-ui";
        options.ApiPath = "/healthchecks-api";
    });
}
```

**Action** : Configurer 3 endpoints `/health/live`, `/health/ready`, `/health` + UI.

---

### 4. Créer HealthCheckOptions helper (1h)

**Fichier** : `src/Presentation/LLMProxy.Gateway/Configuration/HealthCheckOptionsHelper.cs`

**Création** : Helper pour centraliser configuration options.

```csharp
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LLMProxy.Gateway.Configuration;

/// <summary>
/// Helper pour configurer les options des health checks.
/// Centralise la configuration pour éviter duplication.
/// Conforme à ADR-038 (Health Checks).
/// </summary>
public static class HealthCheckOptionsHelper
{
    /// <summary>
    /// Crée les options pour le endpoint liveness probe.
    /// Utilisé par Kubernetes pour vérifier que le processus est vivant.
    /// </summary>
    public static HealthCheckOptions CreateLivenessOptions()
    {
        return new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            AllowCachingResponses = false,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK, // Liveness tolérant
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        };
    }

    /// <summary>
    /// Crée les options pour le endpoint readiness probe.
    /// Utilisé par Kubernetes et load balancers pour savoir si l'instance peut recevoir du trafic.
    /// </summary>
    public static HealthCheckOptions CreateReadinessOptions()
    {
        return new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            AllowCachingResponses = false,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK, // Readiness tolérant aux dégradations mineures
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        };
    }

    /// <summary>
    /// Crée les options pour le endpoint de monitoring complet.
    /// Retourne le détail de tous les checks avec statuts individuels.
    /// </summary>
    public static HealthCheckOptions CreateMonitoringOptions()
    {
        return new HealthCheckOptions
        {
            AllowCachingResponses = false,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        };
    }
}
```

**Action** : Créer helper pour centraliser options.

---

### 5. Créer custom health check pour quotas (1h30)

**Fichier** : `src/Presentation/LLMProxy.Gateway/HealthChecks/QuotaServiceHealthCheck.cs`

**Création** : Custom check pour vérifier service de quotas.

```csharp
using LLMProxy.Domain.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LLMProxy.Gateway.HealthChecks;

/// <summary>
/// Health check personnalisé pour vérifier la disponibilité du service de quotas.
/// Vérifie que Redis est accessible via le QuotaService.
/// Conforme à ADR-038 (Health Checks).
/// </summary>
public sealed class QuotaServiceHealthCheck : IHealthCheck
{
    private readonly IQuotaService _quotaService;
    private readonly ILogger<QuotaServiceHealthCheck> _logger;

    public QuotaServiceHealthCheck(
        IQuotaService quotaService,
        ILogger<QuotaServiceHealthCheck> logger)
    {
        _quotaService = quotaService;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Tenter de récupérer un quota fictif pour tester Redis
            var testUserId = Guid.Empty; // Utilisateur fictif
            var testType = Domain.Entities.QuotaType.RequestsPerMinute;

            var usage = await _quotaService.GetUsageAsync(testUserId, testType, cancellationToken);

            // Si aucune exception, Redis est accessible
            return HealthCheckResult.Healthy("Quota service accessible (Redis OK)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Quota service health check failed");
            
            return HealthCheckResult.Degraded(
                "Quota service unavailable (Redis down or unreachable)",
                exception: ex);
        }
    }
}
```

**Action** : Créer custom health check pour quotas Redis.

**Enregistrement dans Program.cs** :

```csharp
builder.Services.AddHealthChecks()
    // ... checks existants ...
    
    .AddCheck<QuotaServiceHealthCheck>(
        name: "quota-service",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "ready", "business" });
```

---

### 6. Ajouter configuration Kubernetes (30 min)

**Fichier** : `k8s/deployment.yaml` (créer si inexistant)

**Création** : Configuration Kubernetes avec probes.

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: llmproxy-gateway
spec:
  replicas: 3
  selector:
    matchLabels:
      app: llmproxy-gateway
  template:
    metadata:
      labels:
        app: llmproxy-gateway
    spec:
      containers:
      - name: gateway
        image: llmproxy-gateway:latest
        ports:
        - containerPort: 80
        
        # Liveness Probe - Redémarre le pod si échec
        livenessProbe:
          httpGet:
            path: /health/live
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 10
          timeoutSeconds: 3
          failureThreshold: 3
        
        # Readiness Probe - Retire du load balancer si échec
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 5
          timeoutSeconds: 3
          failureThreshold: 2
        
        # Startup Probe - Pour démarrage lent (migration DB)
        startupProbe:
          httpGet:
            path: /health/live
            port: 80
          initialDelaySeconds: 0
          periodSeconds: 5
          timeoutSeconds: 3
          failureThreshold: 30 # 150s max pour démarrer
        
        resources:
          requests:
            memory: "256Mi"
            cpu: "100m"
          limits:
            memory: "2048Mi"
            cpu: "1000m"
```

**Action** : Créer manifeste Kubernetes avec probes configurées.

---

### 7. Créer tests unitaires (2h)

**Fichier** : `tests/LLMProxy.Gateway.Tests/HealthChecks/HealthCheckTests.cs`

**Création** : Tests pour health checks endpoints.

```csharp
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace LLMProxy.Gateway.Tests.HealthChecks;

/// <summary>
/// Tests pour les health checks endpoints.
/// Conforme à ADR-038 (Health Checks).
/// </summary>
public sealed class HealthCheckTests
{
    [Fact]
    public async Task LivenessProbe_AlwaysReturns200_WhenProcessAlive()
    {
        // Le processus est vivant → /health/live retourne 200 OK
    }

    [Fact]
    public async Task ReadinessProbe_Returns200_WhenAllDependenciesHealthy()
    {
        // PostgreSQL OK + Redis OK → /health/ready retourne 200 OK
    }

    [Fact]
    public async Task ReadinessProbe_Returns503_WhenPostgreSQLDown()
    {
        // PostgreSQL DOWN → /health/ready retourne 503 Service Unavailable
    }

    [Fact]
    public async Task ReadinessProbe_Returns200_WhenRedisDown()
    {
        // Redis DOWN → /health/ready retourne 200 OK (Degraded mais acceptable)
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsJsonWithAllChecks()
    {
        // /health retourne JSON avec détail de tous les checks
        // {
        //   "status": "Healthy",
        //   "totalDuration": "00:00:00.123",
        //   "entries": {
        //     "postgresql": { "status": "Healthy", "duration": "00:00:00.050" },
        //     "redis": { "status": "Healthy", "duration": "00:00:00.030" },
        //     ...
        //   }
        // }
    }

    [Fact]
    public async Task QuotaServiceHealthCheck_ReturnsHealthy_WhenRedisAccessible()
    {
        // QuotaService peut accéder Redis → Healthy
    }

    [Fact]
    public async Task QuotaServiceHealthCheck_ReturnsDegraded_WhenRedisUnavailable()
    {
        // Redis inaccessible → Degraded
    }
}
```

**Action** : Créer 7 tests couvrant tous les scénarios.

---

### 8. Mettre à jour README.md (30 min)

**Fichier** : `README.md`

**Ajout** : Section "Health Checks (ADR-038)".

```markdown
### Health Checks (ADR-038)

**Endpoints de surveillance pour Kubernetes et monitoring.**

**Endpoints disponibles :**

| Endpoint | Usage | Description |
|----------|-------|-------------|
| `/health/live` | Kubernetes liveness probe | Vérifie que le processus est vivant |
| `/health/ready` | Kubernetes readiness probe | Vérifie que toutes les dépendances sont OK |
| `/health` | Monitoring complet | Retourne le détail de tous les checks |
| `/healthchecks-ui` | UI (dev/staging) | Interface visuelle des health checks |

**Vérifications effectuées :**

- ✅ **PostgreSQL** : Connexion et query simple (Unhealthy si échec)
- ✅ **Redis** : Ping command (Degraded si échec)
- ✅ **Disk Space** : Minimum 1GB disponible (Degraded si insuffisant)
- ✅ **Memory** : Maximum 2GB allouée au processus (Degraded si dépassé)
- ✅ **Quota Service** : Accès Redis via QuotaService (Degraded si échec)

**Réponse JSON (exemple `/health`) :**

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "self": {
      "status": "Healthy",
      "description": "Gateway is alive",
      "duration": "00:00:00.0001234"
    },
    "postgresql": {
      "status": "Healthy",
      "duration": "00:00:00.0501234",
      "tags": ["ready", "db"]
    },
    "redis": {
      "status": "Healthy",
      "duration": "00:00:00.0301234",
      "tags": ["ready", "cache"]
    },
    "disk": {
      "status": "Healthy",
      "data": {
        "FreeSpace": "50GB"
      },
      "duration": "00:00:00.0051234",
      "tags": ["ready", "infrastructure"]
    },
    "memory": {
      "status": "Healthy",
      "data": {
        "AllocatedMemory": "512MB",
        "Threshold": "2048MB"
      },
      "duration": "00:00:00.0011234",
      "tags": ["ready", "infrastructure"]
    }
  }
}
```

**Codes de statut HTTP :**
- `200 OK` : Healthy ou Degraded (service opérationnel)
- `503 Service Unavailable` : Unhealthy (service non fonctionnel)

**Kubernetes Configuration :**

```yaml
livenessProbe:
  httpGet:
    path: /health/live
    port: 80
  initialDelaySeconds: 5
  periodSeconds: 10
  failureThreshold: 3

readinessProbe:
  httpGet:
    path: /health/ready
    port: 80
  initialDelaySeconds: 5
  periodSeconds: 5
  failureThreshold: 2
```

**Health Checks UI (dev/staging) :**

Accessible sur `http://localhost:5000/healthchecks-ui` pour visualiser l'état des dépendances en temps réel.
```

**Action** : Documenter health checks endpoints et configuration.

---

### 9. Build, test et validation (1h)

**Commandes** :

```powershell
# Build
dotnet build --no-restore

# Tests
dotnet test --no-build --no-restore

# Validation : Vérifier sortie
# - 0 errors, 0 warnings
# - Tous tests passing (79 anciens + 7 nouveaux = 86 total)
```

**Action** :
1. Compiler sans erreurs ni warnings
2. Exécuter tests (100% passing)
3. Démarrer Gateway et tester endpoints :
   ```powershell
   dotnet run --project src/Presentation/LLMProxy.Gateway
   
   # Tester endpoints
   curl http://localhost:5000/health/live
   curl http://localhost:5000/health/ready
   curl http://localhost:5000/health
   curl http://localhost:5000/healthchecks-ui  # Dev uniquement
   ```

---

### 10. Commit et merge (30 min)

**Commits atomiques** :

```powershell
# Commit 1: Packages NuGet
git add src/Presentation/LLMProxy.Gateway/LLMProxy.Gateway.csproj
git commit -m "chore(health-checks): Add NuGet packages for health checks

- AspNetCore.HealthChecks.NpgSql
- AspNetCore.HealthChecks.Redis
- AspNetCore.HealthChecks.System
- AspNetCore.HealthChecks.UI (dev/staging)

ADR-038 conformity: Dependencies added"

# Commit 2: Configuration Program.cs
git add src/Presentation/LLMProxy.Gateway/Program.cs
git commit -m "feat(health-checks): Configure comprehensive health checks

- PostgreSQL check (Unhealthy if down)
- Redis check (Degraded if down)
- Disk space check (1GB minimum)
- Memory check (2GB maximum)
- Endpoints: /health/live, /health/ready, /health
- Health UI enabled (dev/staging only)

ADR-038 conformity: Configuration complete"

# Commit 3: HealthCheckOptionsHelper
git add src/Presentation/LLMProxy.Gateway/Configuration/HealthCheckOptionsHelper.cs
git commit -m "feat(health-checks): Add HealthCheckOptionsHelper

- Centralizes liveness, readiness, monitoring options
- Configures status code mappings
- Reduces duplication in Program.cs

ADR-038 conformity: Helper created"

# Commit 4: Custom health check
git add src/Presentation/LLMProxy.Gateway/HealthChecks/QuotaServiceHealthCheck.cs
git commit -m "feat(health-checks): Add QuotaServiceHealthCheck

- Custom check for quota service availability
- Tests Redis access via QuotaService
- Returns Degraded if Redis unavailable

ADR-038 conformity: Custom check added"

# Commit 5: Kubernetes configuration
git add k8s/deployment.yaml
git commit -m "feat(health-checks): Add Kubernetes deployment with probes

- Liveness probe: /health/live
- Readiness probe: /health/ready
- Startup probe for slow starts (migrations)
- Resource limits: 2GB memory, 1 CPU

ADR-038 conformity: Kubernetes integration"

# Commit 6: Tests
git add tests/LLMProxy.Gateway.Tests/HealthChecks/HealthCheckTests.cs
git commit -m "test(health-checks): Add health checks tests

- 7 tests covering all scenarios
- Liveness always OK
- Readiness varies by dependencies
- JSON response validation

ADR-038 conformity: Test coverage complete"

# Commit 7: Documentation
git add README.md
git commit -m "docs(health-checks): Document health checks endpoints

- Endpoints table with usage
- Checks description
- JSON response example
- Kubernetes configuration

ADR-038 conformity: Documentation complete"
```

**Merge** :

```powershell
git checkout main
git merge --no-ff feature/025--implement-adr-038-health-checks -m "Merge feature/025 - Implement ADR-038 Health Checks"
git branch -d feature/025--implement-adr-038-health-checks
```

**Action** : 7 commits atomiques, merge, supprimer feature branch.

---

## RÉFÉRENCE ADR

**ADR-038** : `docs/adr/038-health-checks-readiness-probes.adr.md`

**Principes clés** :
1. **Liveness** : Processus vivant (redémarre si échec)
2. **Readiness** : Dépendances OK (retire du load balancer si échec)
3. **Startup** : Permet démarrage lent (migrations DB)
4. **Tags** : Grouper checks par type (live, ready, infrastructure, business)
5. **FailureStatus** : Unhealthy (critique) vs Degraded (tolérable)

**Dépendances critiques** :
- PostgreSQL : Unhealthy si down (app non fonctionnelle)
- Redis : Degraded si down (app fonctionne sans cache)
- Disk/Memory : Degraded si limites atteintes

**Stratégie Kubernetes** :
- **Liveness** → Redémarre pod si échec (dernier recours)
- **Readiness** → Retire du load balancer (traffic vers instances saines)
- **Startup** → Tolère démarrage lent (migration DB)

---

## DURÉE ESTIMÉE

**Total** : 11h  
- Installer packages : 30 min
- Configurer Program.cs : 2h
- Configurer endpoints : 1h
- HealthCheckOptionsHelper : 1h
- QuotaServiceHealthCheck : 1h30
- Kubernetes deployment : 30 min
- Tests unitaires : 2h
- README.md : 30 min
- Build/test/validation : 1h
- Commits/merge : 30 min

---

## NOTES

**Impacts sur architecture** :
- Dépendances NuGet : 6 packages (health checks + UI)
- Pipeline HTTP : Endpoints `/health/*` ajoutés
- Kubernetes : Probes configurées (liveness, readiness, startup)
- Monitoring : UI disponible en dev/staging

**Trade-offs** :
- ✅ Monitoring production robuste (détection pannes dépendances)
- ✅ Déploiements sûrs (Kubernetes ne démarre que pods sains)
- ✅ Debugging facilité (UI visuelle des statuts)
- ⚠️ Latence ajoutée (~50ms par health check)
- ⚠️ Charge Redis/PostgreSQL (queries périodiques)

**Migration existant** :
- Endpoint `/health` conservé (backward compatible)
- Ajouter `/health/live` et `/health/ready` pour Kubernetes
- Activer UI uniquement en dev/staging (pas en production)

**Monitoring recommandé** :
- Alerting si `/health/ready` retourne 503 pendant >2 minutes
- Métriques : temps de réponse par check, taux d'échec
- Dashboard : Grafana avec health checks UI


## TRACKING

Début: 2025-12-22T08:09:21.2102799Z


Fin: 2025-12-22T08:15:52.6241405Z
Durée: 00:06:31

