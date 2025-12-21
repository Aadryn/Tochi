# Tâche 002 - Séparer IServices.cs en 5 fichiers + Result

**Créée le** : 2025-12-21  
**Criticité** : 🔴🔴 CRITIQUE  
**Priorité** : P1 (HAUTE)  
**Effort estimé** : 35 minutes  
**Risque** : FAIBLE

---

## OBJECTIF

Corriger violation ADR-001 dans `IServices.cs` contenant **7 types** (5 interfaces + 1 Result + 1 enum).

**Violation actuelle** :
```csharp
// Fichier: src/Core/LLMProxy.Domain/Interfaces/IServices.cs
public interface IQuotaService { }              // Type 1
public class QuotaCheckResult { }               // Type 2 (Result)
public interface ICacheService { }              // Type 3
public interface ISecretService { }             // Type 4
public interface ITokenCounterService { }       // Type 5
```

---

## CRITÈRES DE SUCCÈS

- [ ] **6 fichiers créés** dans `src/Core/LLMProxy.Domain/Interfaces/`
  - [ ] `IQuotaService.cs` (interface uniquement)
  - [ ] `QuotaCheckResult.cs` (Result pattern)
  - [ ] `ICacheService.cs`
  - [ ] `ISecretService.cs`
  - [ ] `ITokenCounterService.cs`
- [ ] **Fichier original supprimé** : `IServices.cs`
- [ ] **Build : 0 errors, 0 warnings**
- [ ] **Tests : 100% passed**

---

## PLAN D'EXÉCUTION

### ÉTAPE 1 : Création feature branch (2 min)

```powershell
git checkout -b refactor/002--split-iservices
dotnet build --no-restore
dotnet test --no-build
```

### ÉTAPE 2-6 : Extraire chaque interface (25 min)

Créer les 5 fichiers d'interface + 1 Result, valider build après chacun.

### ÉTAPE 7 : Supprimer IServices.cs et valider (5 min)

### ÉTAPE 8 : Commit (3 min)

```powershell
git add src\Core\LLMProxy.Domain\Interfaces\
git commit -m "refactor(domain): Split IServices.cs into 6 separate files

Conform to ADR-001 (one type per file)"
git push origin refactor/002--split-iservices
```

---

## DÉPENDANCES

- **Bloqué par** : Tâche 001 (pattern similaire, apprendre du premier split)
- **Bloquant pour** : Tâches P2 (splits services Infrastructure)

---

_Conforme à : ADR-001_
