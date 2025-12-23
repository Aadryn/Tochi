# TÂCHE 123 : Refactor ADR-005 - ProviderOrchestrator SRP

**Créée le** : 2025-12-24T00:00:00Z UTC  
**Priorité** : 🔴 Critique  
**ADR Violée** : ADR-005 - SOLID Principles (SRP)

---

## CONTEXTE

**Fichier** : `ProviderOrchestrator.cs`  
**Lignes** : 638  
**Responsabilités** : 4 distinctes

### Responsabilités Identifiées

1. **Routing** : Sélection du provider approprié
2. **Failover** : Gestion des échecs et retry
3. **Metrics** : Collecte et agrégation des métriques
4. **Streaming** : Gestion des flux SSE

### Violation SRP

> Une classe ne doit avoir qu'une seule raison de changer.

ProviderOrchestrator change pour :
- Nouvelle stratégie de routing → Changement 1
- Nouvelle politique de failover → Changement 2
- Nouvelles métriques → Changement 3
- Nouveau format streaming → Changement 4

**4 raisons de changer = Violation SRP**

---

## OBJECTIF

Décomposer en 4 classes :
1. `ProviderRouter` : Routing uniquement
2. `FailoverCoordinator` : Failover + retry
3. `MetricsCollector` : Collecte métriques
4. `StreamOrchestrator` : Gestion streaming

---

## ÉTAPES

### Étape 1 : Extraire ProviderRouter

**Responsabilité** : Sélectionner le meilleur provider

```csharp
public class ProviderRouter
{
    public async Task<LLMProvider> SelectProviderAsync(
        RoutingContext context,
        CancellationToken ct);
}
```

### Étape 2 : Extraire FailoverCoordinator

**Responsabilité** : Gérer retry et failover

```csharp
public class FailoverCoordinator
{
    public async Task<Result<T>> ExecuteWithFailoverAsync<T>(
        Func<LLMProvider, Task<Result<T>>> action,
        FailoverPolicy policy,
        CancellationToken ct);
}
```

### Étape 3 : Extraire MetricsCollector

**Responsabilité** : Collecter métriques d'exécution

```csharp
public class MetricsCollector
{
    public void RecordProviderCall(
        LLMProvider provider,
        TimeSpan duration,
        bool success);
}
```

### Étape 4 : Extraire StreamOrchestrator

**Responsabilité** : Orchestrer streaming SSE

```csharp
public class StreamOrchestrator
{
    public async IAsyncEnumerable<StreamChunk> StreamAsync(
        LLMProvider provider,
        ChatRequest request,
        CancellationToken ct);
}
```

### Étape 5 : Refactor ProviderOrchestrator

**Nouveau rôle** : Coordonner les 4 composants

```csharp
public class ProviderOrchestrator
{
    private readonly ProviderRouter _router;
    private readonly FailoverCoordinator _failover;
    private readonly MetricsCollector _metrics;
    private readonly StreamOrchestrator _stream;
    
    // Orchestrer appels aux composants
}
```

---

## CRITÈRES DE SUCCÈS

- [ ] 4 nouvelles classes créées
- [ ] ProviderOrchestrator < 200 lignes
- [ ] Chaque classe < 300 lignes
- [ ] Tests unitaires mis à jour
- [ ] Build + Tests OK

---

## MÉTADONNÉES

- **Effort** : 6h
- **Risque** : 7/10 (refactoring majeur)
- **Impact** : 9/10 (maintenabilité)
