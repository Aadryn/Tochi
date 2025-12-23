# Tâche 074 - Corriger ADR-044 : Async/Await Best Practices

## PRIORITÉ
🟢 **P4 - MINEURE** (Quick Fix)

## OBJECTIF

Corriger les 3 violations ADR-044 identifiées où `.Result` est utilisé après `Task.WhenAll`.

## CONTEXTE

### ADR-044 Règle Violée
> Pas de `Task.Wait()` ou `.Result` - utiliser `await` à la place.

### Fichier Concerné

**Fichier :** `applications/proxy/backend/src/Infrastructure/LLMProxy.Infrastructure.Redis/QuotaService.cs`

**Lignes :** 143-144

### Code Actuel (Violation)

```csharp
var usageTask = _db.StringGetAsync(key);
var limitTask = _db.StringGetAsync(limitKey);

await Task.WhenAll(usageTask, limitTask);

var used = usageTask.Result.HasValue ? long.Parse(usageTask.Result!) : 0;  // ❌
var limitData = limitTask.Result;  // ❌
```

### Code Corrigé (Conforme)

```csharp
var usageTask = _db.StringGetAsync(key);
var limitTask = _db.StringGetAsync(limitKey);

var results = await Task.WhenAll(usageTask, limitTask);

var usageValue = results[0];
var limitValue = results[1];

var used = usageValue.HasValue ? long.Parse(usageValue!) : 0;  // ✅
var limitData = limitValue;  // ✅
```

**Alternative (plus explicite) :**

```csharp
var (usageValue, limitValue) = await (
    _db.StringGetAsync(key),
    _db.StringGetAsync(limitKey)
).WhenAll();

var used = usageValue.HasValue ? long.Parse(usageValue!) : 0;
var limitData = limitValue;
```

## CRITÈRES DE SUCCÈS

- [ ] 0 utilisation de `.Result` sur Task dans QuotaService.cs
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Tests passent

## ESTIMATION

**Effort** : 30min
**Risque** : Très faible
**Valeur** : Moyenne (prévention deadlocks)

## RÉFÉRENCES

- ADR-044 : Async/Await Best Practices

