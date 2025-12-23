# Tâche 002 - Health Checks Endpoints Backend

## OBJECTIF
Implémenter des endpoints de health checks pour le backend selon ADR-038.

## CONTEXTE
Conformément à [ADR-038: Health Checks & Readiness Probes](docs/adr/038-health-checks-readiness-probes.adr.md), chaque service doit exposer :
- `/health/live` - Liveness probe (service actif)
- `/health/ready` - Readiness probe (service prêt à traiter du trafic)

## PÉRIMÈTRE
### Services Concernés
1. **Gateway** (ASP.NET Core)
2. **WebApi Default** (ASP.NET Core)  
3. **WebApp Management** (ASP.NET Core Blazor)
4. **WebApp Default** (ASP.NET Core Blazor)

### Endpoints à Créer

Pour chaque service :

**GET /health/live**
- Retourne : `200 OK` avec `{ "status": "UP" }`
- Purpose : Vérifier que le process est vivant
- Checks : Minimal (process runs)

**GET /health/ready**
- Retourne : `200 OK` si prêt, `503 Service Unavailable` si non prêt
- Purpose : Vérifier que le service peut traiter des requêtes
- Checks :
  - Database connectivity (pour services avec DB)
  - Dependencies disponibles
  - Configuration valide

## CRITÈRES DE SUCCÈS
- [ ] 4 services exposent `/health/live`
- [ ] 4 services exposent `/health/ready`
- [ ] Tests unitaires pour les health checks
- [ ] Documentation API (Swagger/OpenAPI)
- [ ] Réponse JSON standardisée
- [ ] Logs structurés pour les échecs

## APPROCHE TECHNIQUE

### ASP.NET Core Health Checks
Utiliser le package Microsoft :
```bash
dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks
```

### Configuration Program.cs
```csharp
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddNpgSql(connectionString, name: "database");

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = WriteHealthCheckResponse
});
```

### Custom Health Checks
```csharp
public class DatabaseHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Database.CanConnectAsync(cancellationToken);
            return HealthCheckResult.Healthy("Database connection successful");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }
}
```

## DÉPENDANCES
- **Bloquants** : Aucun  
- **Prérequis** : Connexion PostgreSQL configurée

## ESTIMATION
- **Complexité** : Faible/Moyenne
- **Durée estimée** : 2-3 heures
- **Fichiers impactés** : ~8 fichiers (4 Program.cs, 4 fichiers tests)

## RÉFÉRENCES
- ADR-038: Health Checks & Readiness Probes
- [ASP.NET Core Health Checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- [Kubernetes Liveness/Readiness](https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/)

## TRACKING
- **Créé** : 2025-01-XX 01:45:00Z
- **Status** : TO-DO

