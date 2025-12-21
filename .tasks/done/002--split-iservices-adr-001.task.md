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


## TRACKING
Début: 2025-12-21T16:38:50.6104175Z



## RÉSULTATS

**Fin:** 2025-12-21T16:40:18.2445681Z
**Durée:** 00:01:27

**Fichiers créés (5):**
- IQuotaService.cs (40 lignes)
- QuotaCheckResult.cs (44 lignes - Result Pattern)
- ICacheService.cs (38 lignes)
- ISecretService.cs (23 lignes)
- ITokenCounterService.cs (22 lignes)

**Fichiers supprimés (1):**
- IServices.cs (103 lignes - 5 types)

**Build:**
- Résultat: SUCCÈS
- Erreurs: 0
- Warnings: 0 (2 pré-existants dans TenantTests.cs - xUnit1012)

**Tests:**
- Total: 66/66 (100%)
- Domaine: 17/17
- Sécurité: 35/35
- Gateway: 14/14
- Échecs: 0
- Ignorés: 0

**Conformité ADR-001:**
 VIOLATION CORRIGÉE - 5 types séparés en 5 fichiers distincts

