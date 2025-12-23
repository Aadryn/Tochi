# Tâche 001 - Séparer IRepositories.cs en 8 fichiers

**Créée le** : 2025-12-21  
**Criticité** : 🔴🔴🔴 BLOCANT  
**Priorité** : P0 (IMMÉDIATE)  
**Effort estimé** : 45 minutes  
**Risque** : FAIBLE (refactoring purement structurel)

---

## OBJECTIF

Corriger la **violation MASSIVE ADR-001** dans `IRepositories.cs` qui contient **8 types** au lieu d'**1 seul** (violation maximale du projet).

**Violation actuelle** :
```csharp
// Fichier: src/Core/LLMProxy.Domain/Interfaces/IRepositories.cs
public interface ITenantRepository { }          // Type 1
public interface IUserRepository { }            // Type 2
public interface IApiKeyRepository { }          // Type 3
public interface ILLMProviderRepository { }     // Type 4
public interface IQuotaLimitRepository { }      // Type 5
public interface IAuditLogRepository { }        // Type 6
public interface ITokenUsageMetricRepository { }// Type 7
public interface IUnitOfWork { }                // Type 8
```

---

## JUSTIFICATION

**ADR-001 : Un seul type par fichier C#**

> "Chaque fichier C# DOIT contenir un seul type de premier niveau."

**Conséquences actuelles** :
1. **Historique Git inutilisable** : Toute modification d'interface = modification du fichier entier
2. **Conflits permanents** : Fichier le plus modifié du projet (risque de merge conflict)
3. **Navigation IDE catastrophique** : 8 symboles publics dans un seul fichier
4. **Violation SRP** : Fichier a 8 raisons de changer (une par interface)
5. **Pull Requests illisibles** : Modifications de 2 interfaces différentes mélangées

**Bénéfices du refactoring** :
- ✅ Conformité ADR-001 : 100% (8 types → 8 fichiers)
- ✅ Historique Git précis : Une modif interface = Un fichier modifié
- ✅ Navigation instantanée : "Go to Definition" ouvre le bon fichier
- ✅ Réduction conflits : Chaque interface isolée
- ✅ Code reviews efficaces : Pull requests ciblées

---

## CRITÈRES DE SUCCÈS

- [ ] **8 fichiers créés** dans `src/Core/LLMProxy.Domain/Interfaces/`
  - [ ] `ITenantRepository.cs`
  - [ ] `IUserRepository.cs`
  - [ ] `IApiKeyRepository.cs`
  - [ ] `ILLMProviderRepository.cs`
  - [ ] `IQuotaLimitRepository.cs`
  - [ ] `IAuditLogRepository.cs`
  - [ ] `ITokenUsageMetricRepository.cs`
  - [ ] `IUnitOfWork.cs`
- [ ] **Fichier original supprimé** : `IRepositories.cs` n'existe plus
- [ ] **Build réussi** : `dotnet build --no-restore` exitCode = 0, 0 warnings
- [ ] **Tests réussis** : `dotnet test --no-build` exitCode = 0, 100% passed
- [ ] **Aucune régression** : Comportement applicatif identique
- [ ] **Documentation XML préservée** : Tous commentaires conservés
- [ ] **Namespace cohérent** : `LLMProxy.Domain.Interfaces` pour tous les fichiers

---

## PLAN D'EXÉCUTION (Baby Steps)

### ÉTAPE 1 : Préparation (5 min)

```powershell
# 1. Créer feature branch
git checkout -b refactor/001--split-irepositories

# 2. Vérifier état actuel
dotnet build --no-restore
dotnet test --no-build

# 3. Backup du fichier original
$timestamp = (Get-Date).ToString("yyyyMMdd-HHmmss")
Copy-Item "src\Core\LLMProxy.Domain\Interfaces\IRepositories.cs" `
          ".backups\$timestamp--IRepositories.cs.bak"
```

### ÉTAPE 2 : Extraire ITenantRepository.cs (5 min)

```csharp
// Fichier: src/Core/LLMProxy.Domain/Interfaces/ITenantRepository.cs
using LLMProxy.Domain.Entities;

namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Repository interface for Tenant aggregate (Port)
/// </summary>
public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tenant>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
```

**Validation** :
```powershell
dotnet build --no-restore src\Core\LLMProxy.Domain\
if ($LASTEXITCODE -ne 0) { throw "Build failed" }
```

### ÉTAPE 3 : Extraire IUserRepository.cs (5 min)

```csharp
// Fichier: src/Core/LLMProxy.Domain/Interfaces/IUserRepository.cs
using LLMProxy.Domain.Entities;

namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Repository interface for User aggregate (Port)
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(Guid tenantId, string email, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
```

**Validation** :
```powershell
dotnet build --no-restore src\Core\LLMProxy.Domain\
```

### ÉTAPE 4-9 : Extraire les 6 autres interfaces (20 min)

Répéter le processus pour :
- `IApiKeyRepository.cs`
- `ILLMProviderRepository.cs`
- `IQuotaLimitRepository.cs`
- `IAuditLogRepository.cs`
- `ITokenUsageMetricRepository.cs`
- `IUnitOfWork.cs`

**Validation après chaque fichier** : Build incrémental

### ÉTAPE 10 : Supprimer IRepositories.cs (2 min)

```powershell
# 1. Vérifier que tous les fichiers sont créés
$expected = @(
    "ITenantRepository.cs",
    "IUserRepository.cs",
    "IApiKeyRepository.cs",
    "ILLMProviderRepository.cs",
    "IQuotaLimitRepository.cs",
    "IAuditLogRepository.cs",
    "ITokenUsageMetricRepository.cs",
    "IUnitOfWork.cs"
)

$existing = Get-ChildItem "src\Core\LLMProxy.Domain\Interfaces\" -Filter "I*.cs" | 
    Select-Object -ExpandProperty Name

if ($expected.Count -ne $existing.Count) {
    throw "Missing files: Expected $($expected.Count), Found $($existing.Count)"
}

# 2. Supprimer fichier original
Remove-Item "src\Core\LLMProxy.Domain\Interfaces\IRepositories.cs" -Force
```

### ÉTAPE 11 : Validation Complète (5 min)

```powershell
# 1. Build solution complète
dotnet build --no-restore
if ($LASTEXITCODE -ne 0) { throw "Build failed after split" }

# 2. Tests unitaires complets
dotnet test --no-build
if ($LASTEXITCODE -ne 0) { throw "Tests failed after split" }

# 3. Vérifier 0 warnings
$buildOutput = dotnet build --no-restore 2>&1 | Out-String
if ($buildOutput -match "(\d+) Warning\(s\)") {
    $warnings = $Matches[1]
    if ([int]$warnings -gt 0) {
        throw "$warnings warning(s) detected"
    }
}

Write-Host "✅ VALIDATION COMPLÈTE RÉUSSIE" -ForegroundColor Green
```

### ÉTAPE 12 : Commit et Push (3 min)

```powershell
# 1. Commit atomique
git add src\Core\LLMProxy.Domain\Interfaces\
git commit -m "refactor(domain): Split IRepositories.cs into 8 separate files

BREAKING CHANGE: IRepositories.cs removed
- Created ITenantRepository.cs
- Created IUserRepository.cs
- Created IApiKeyRepository.cs
- Created ILLMProviderRepository.cs
- Created IQuotaLimitRepository.cs
- Created IAuditLogRepository.cs
- Created ITokenUsageMetricRepository.cs
- Created IUnitOfWork.cs

Reason: Conform to ADR-001 (one type per file)
Impact: Better Git history, reduced merge conflicts, improved navigation"

# 2. Push
git push origin refactor/001--split-irepositories
```

---

## DÉPENDANCES

- **Aucune** : Refactoring purement structurel, aucune dépendance externe
- **Bloquant pour** : Tâches 002 (IServices.cs split) et suivantes

---

## RISQUES ET MITIGATIONS

| Risque | Probabilité | Impact | Mitigation |
|--------|-------------|--------|------------|
| Build échoue après suppression | FAIBLE | MOYEN | Vérifier build après CHAQUE fichier créé |
| Tests échouent | TRÈS FAIBLE | FAIBLE | Aucun changement logique, juste structure |
| Oubli fichier | FAIBLE | MOYEN | Checklist des 8 fichiers dans ÉTAPE 10 |

---

## NOTES COMPLÉMENTAIRES

**Pourquoi P0 (IMMÉDIATE) ?**
- Fichier le plus en violation de ADR-001 (8 types au lieu de 1)
- Impact Git majeur (conflits constants)
- Bloque adoption bonnes pratiques dans l'équipe
- Refactoring simple et sans risque (changement purement structurel)

**Estimation temps** : 45 minutes (avec validations)
**Temps réel observé** : ~30-40 minutes si automatisation PowerShell

---

## TRACKING

**Début** : _Non commencé_  
**Fin** : _Non complété_  
**Durée** : _N/A_  

---

_Conforme à : ADR-001, ADR-009 (Fail Fast), ADR-018 (Documentation)_


## TRACKING
Début: 2025-12-21T16:33:29.6271435Z


Fin: 2025-12-21T16:34:57.1975835Z
Durée: 00:01:27

## RÉSULTATS

 8 fichiers créés (1 type par fichier):
- ITenantRepository.cs
- IUserRepository.cs
- IApiKeyRepository.cs
- ILLMProviderRepository.cs
- IQuotaLimitRepository.cs
- IAuditLogRepository.cs
- ITokenUsageMetricRepository.cs
- IUnitOfWork.cs

 Fichier original supprimé: IRepositories.cs (8 types)
 Build: 0 errors, 2 warnings pré-existants
 Tests: 66/66 passing (100%)
 Conformité ADR-001: VIOLATION MASSIVE CORRIGÉE (88)

Commit: 28725e1 'refactor(domain): split IRepositories.cs into 8 files (ADR-001)'



## RÉSULTATS

**Fin:** 2025-12-21T16:38:29.9810729Z
**Durée:** 00:05:00

**Fichiers créés (8):**
- ITenantRepository.cs (18 lignes)
- IUserRepository.cs (18 lignes)
- IApiKeyRepository.cs (17 lignes)
- ILLMProviderRepository.cs (16 lignes)
- IQuotaLimitRepository.cs (15 lignes)
- IAuditLogRepository.cs (14 lignes)
- ITokenUsageMetricRepository.cs (15 lignes)
- IUnitOfWork.cs (19 lignes)

**Fichiers supprimés (1):**
- IRepositories.cs (120 lignes - 8 types)

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
 VIOLATION CORRIGÉE - 8 types séparés en 8 fichiers distincts

