# Tâche 004 - Séparation Fichiers Multi-Types (ADR-001)

**Créée le** : 2025-12-21  
**Criticité** : 🟢 MINEUR  
**Priorité** : P3 (OPTIONNELLE)  
**Effort estimé** : 30 minutes  
**Risque** : TRÈS FAIBLE

---

## OBJECTIF

Séparer les fichiers contenant plusieurs types pour respecter strictement **ADR-001 (Un seul type par fichier C#)**, même pour les types intimement liés (Result + Interface + Impl).

**Situation actuelle** :
- ApiKeyAuthenticator.cs : 3 types (Result + Interface + Impl)
- ApiKeyValidator.cs : 3 types (Result + Interface + Impl)
- HashService.cs : 2 types (Interface + Impl)
- ApiKeyExtractor.cs : 3 types (Constantes + Interface + Impl)

**Impact** : Cohérence architecturale - respect strict de ADR-001

---

## CRITÈRES DE SUCCÈS

- [ ] **Chaque type dans son propre fichier** :
  - [ ] ApiKeyAuthenticationResult.cs (Result seul)
  - [ ] IApiKeyAuthenticator.cs (Interface seule)
  - [ ] ApiKeyAuthenticator.cs (Implémentation seule)
  - [ ] ApiKeyValidationResult.cs (Result seul)
  - [ ] IApiKeyValidator.cs (Interface seule)
  - [ ] ApiKeyValidator.cs (Implémentation seule)
  - [ ] IHashService.cs (Interface seule)
  - [ ] Sha256HashService.cs (Implémentation seule)
  - [ ] ApiKeyExtractorConstants.cs (Constantes seules)
  - [ ] IApiKeyExtractor.cs (Interface seule)
  - [ ] HeaderApiKeyExtractor.cs (Implémentation seule)
- [ ] **Namespaces corrects** : Tous les types dans le bon namespace
- [ ] **Documentation XML** : Préservée et cohérente
- [ ] **Build : 0 errors, 0 warnings**
- [ ] **Tests : 100% passed**
- [ ] **Aucun using cassé** : Imports mis à jour si nécessaire

---

## PLAN D'EXÉCUTION

### ÉTAPE 1 : Créer feature branch (2 min)

```powershell
git checkout -b feature/004--split-multi-type-files
```

### ÉTAPE 2 : Séparer ApiKeyAuthenticator.cs (5 min)

- Créer `ApiKeyAuthenticationResult.cs` (Result seul)
- Créer `IApiKeyAuthenticator.cs` (Interface seule)
- Modifier `ApiKeyAuthenticator.cs` (Implémentation seule)

### ÉTAPE 3 : Séparer ApiKeyValidator.cs (5 min)

- Créer `ApiKeyValidationResult.cs` (Result seul)
- Créer `IApiKeyValidator.cs` (Interface seule)
- Modifier `ApiKeyValidator.cs` (Implémentation seule)

### ÉTAPE 4 : Séparer HashService.cs (3 min)

- Créer `IHashService.cs` (Interface seule)
- Renommer `HashService.cs` en `Sha256HashService.cs` (Implémentation seule)

### ÉTAPE 5 : Séparer ApiKeyExtractor.cs (5 min)

- Créer `ApiKeyExtractorConstants.cs` (Constantes seules)
- Créer `IApiKeyExtractor.cs` (Interface seule)
- Renommer `ApiKeyExtractor.cs` en `HeaderApiKeyExtractor.cs` (Implémentation seule)

### ÉTAPE 6 : Build et tests (5 min)

```powershell
dotnet build --no-restore
dotnet test --no-build
```

### ÉTAPE 7 : Commit et merge (5 min)

```powershell
git add .
git commit -m "refactor(security): Split multi-type files for strict ADR-001 compliance

Separate Result, Interface, and Implementation types into dedicated files:
- ApiKeyAuthenticator: 1 file → 3 files
- ApiKeyValidator: 1 file → 3 files
- HashService: 1 file → 2 files
- ApiKeyExtractor: 1 file → 3 files

Total: 4 files → 11 files

Benefits:
- Strict ADR-001 compliance (one type per file)
- Clearer file organization
- Easier navigation and discovery

ADR-001: 100% compliant (puriste)
Tests: XX/XX passing
Build: 0 errors, 0 warnings"

git checkout main
git merge --no-ff feature/004--split-multi-type-files
git branch -d feature/004--split-multi-type-files
```

---

## DÉPENDANCES

- **Aucune dépendance bloquante**
- **Note** : Tâche purement cosmétique, n'affecte pas la fonctionnalité

---

## RÉFÉRENCES

- **ADR-001** : Un seul type par fichier C#
- **Rapport** : `docs/ANALYSE_CONFORMITE_ADR.md` (Problème M2)

---

_Conforme à : ADR-001 (Un seul type par fichier - strict)_
