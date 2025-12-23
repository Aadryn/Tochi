# TÂCHE 122 : Refactor ADR-001 - Error.cs (7 types)

**Créée le** : 2025-12-24T00:00:00Z UTC  
**Priorité** : 🟡 Majeure  
**ADR Violée** : ADR-001

---

## CONTEXTE

**Fichier** : `LLMProxy.Domain/Common/Error.cs`  
**Types** : 7 classes statiques imbriquées  
**Lignes** : 274

### Structure Actuelle

```csharp
public readonly record struct Error
{
    public static class User { /* 5 methods */ }
    public static class Tenant { /* 5 methods */ }
    public static class ApiKey { /* 6 methods */ }
    public static class Quota { /* 3 methods */ }
    public static class Validation { /* 5 methods */ }
    public static class Database { /* 7 methods */ }
    // etc.
}
```

---

## OBJECTIF

**ATTENTION** : Analyse préalable requise (tâche 092 conclut conformité SRP).

Options :
1. **Conserver** structure actuelle (Factory Pattern légitime)
2. **Extraire** classes statiques vers fichiers séparés

---

## DÉCISION RECOMMANDÉE

**CONSERVER** l'état actuel car :
- Factory Method Pattern reconnu (GoF)
- Cohésion maximale (toutes erreurs pour Result Pattern)
- API naturelle : `Error.User.NotFound(id)`
- 274 lignes acceptables
- Nested classes = namespacing C# idiomatique

---

## CRITÈRES DE SUCCÈS

- [ ] Revue décision avec équipe
- [ ] Si extraction : 7 fichiers créés
- [ ] Si conservation : documenter justification

---

## MÉTADONNÉES

- **Effort** : 2h (si extraction) ou 0.5h (documentation)
- **Risque** : 5/10 (changement API majeur)
