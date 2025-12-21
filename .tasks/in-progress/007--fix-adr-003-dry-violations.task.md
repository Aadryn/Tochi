# 007 - Corriger Violations ADR-003 (DRY)

## OBJECTIF

Éliminer les 12 duplications de code détectées dans le rapport d'analyse ADR-003 pour atteindre 100% de conformité au principe DRY (Don't Repeat Yourself).

## JUSTIFICATION

**Problème** : 12 duplications détectées (6 critiques, 4 moyennes) générant :
- 149 lignes de code dupliqué
- Risque d'incohérences lors des modifications
- Dette technique accumulée
- Violation directe ADR-003

**Bénéfices attendus** :
- ✅ Réduction de 149 lignes de code
- ✅ Centralisation de la logique métier
- ✅ Amélioration maintenabilité (+50%)
- ✅ Conformité 100% ADR-003

## PÉRIMÈTRE

### Phase 1 : Duplications CRITIQUES (6 violations)

1. **Logique suppression repositories** (5 occurrences)
   - Créer `RepositoryBase<TEntity>` avec `DeleteAsync()` générique
   - Migrer UserRepository, TenantRepository, ApiKeyRepository, LLMProviderRepository, QuotaLimitRepository
   - Impact : -30 lignes

2. **Validation URL** (4 occurrences - LLMProvider)
   - Créer `UrlValidator` statique ou extension method
   - Centraliser dans `Domain/Common/` ou `Domain/ValueObjects/`
   - Impact : -16 lignes

3. **Hachage hexadécimal** (2 occurrences)
   - Créer `CryptographyExtensions` ou `HashingService`
   - Centraliser dans `Infrastructure/Common/`
   - Impact : -16 lignes

4. **Logique AuditLog** (17 occurrences)
   - Créer `AuditLogFactory` ou extension methods
   - Centraliser création/normalisation
   - Impact : -51 lignes

5. **Logique TokenUsageMetric** (11 occurrences)
   - Créer `TokenUsageMetricFactory`
   - Centraliser calculs (coût, durée)
   - Impact : -22 lignes

6. **Extraction User/Tenant/ApiKey** (6 occurrences)
   - Créer `HttpContextExtensions` avec méthodes dédiées
   - Centraliser dans `Presentation/Extensions/`
   - Impact : -14 lignes

### Phase 2 : Duplications MOYENNES (4 violations)

7. **Messages validation FluentValidation** (30+ messages)
   - Créer `ValidationMessages` statique
   - Centraliser dans `Application/Common/`
   - Impact : -20 lignes

8. **Configuration JSON** (3 occurrences)
   - Créer `JsonSerializerOptionsFactory`
   - Centraliser options (camelCase, ignoreNull)
   - Impact : -10 lignes

9. **Pattern `?? throw`** (acceptable dans constructeurs)
   - Vérifier usage, documenter si pattern intentionnel
   - Impact : 0 lignes (acceptable)

10. **Codes statut HTTP** (nombres magiques)
    - Utiliser constantes `StatusCodes.*` ou créer enum
    - Impact : amélioration lisibilité

## CRITÈRES DE SUCCÈS

- [ ] Phase 1 complétée (6 duplications critiques éliminées)
- [ ] Phase 2 complétée (4 duplications moyennes corrigées)
- [ ] Build réussi : `dotnet build --no-restore` (0 erreurs, 0 warnings)
- [ ] Tests réussis : `dotnet test --no-build` (100% passed)
- [ ] Code conforme ADR-003 (principe DRY)
- [ ] Rapport ANALYSE_CONFORMITE_ADR-003.md mis à jour (statut CONFORME)
- [ ] Documentation XML complète sur nouvelles classes/méthodes
- [ ] Commits atomiques par duplication corrigée

## DÉPENDANCES

- ✅ ADR-001 vérifié (100% conforme)
- ✅ ADR-002 vérifié (97% conforme)
- ✅ ADR-003 analysé (rapport généré)

## CONTRAINTES

- **Respect ADR-005 (SOLID)** : Single Responsibility, Open/Closed
- **Respect ADR-006 (Onion Architecture)** : Placer helpers dans bonnes couches
- **Respect ADR-014 (Dependency Injection)** : Services injectables si nécessaire
- **Documentation française** : Commentaires XML en français uniquement

## PLAN D'ACTION

### Étape 1 : RepositoryBase<TEntity>
1. Créer `src/Infrastructure/LLMProxy.Infrastructure.PostgreSQL/Repositories/RepositoryBase.cs`
2. Implémenter méthodes génériques (GetByIdAsync, DeleteAsync, AddAsync, UpdateAsync)
3. Migrer 5 repositories vers héritage de RepositoryBase
4. Build + Tests
5. Commit atomique

### Étape 2 : UrlValidator
1. Créer `src/Core/LLMProxy.Domain/Common/Validators/UrlValidator.cs`
2. Méthode statique `IsValidUrl(string url)`
3. Remplacer 4 occurrences dans LLMProvider
4. Build + Tests
5. Commit atomique

### Étape 3 : CryptographyExtensions
1. Créer `src/Infrastructure/LLMProxy.Infrastructure.Security/Extensions/CryptographyExtensions.cs`
2. Méthode extension `ToHexString(this byte[] bytes)`
3. Remplacer 2 occurrences (ApiKeyService, TokenService)
4. Build + Tests
5. Commit atomique

### Étape 4 : AuditLogFactory
1. Créer `src/Infrastructure/LLMProxy.Infrastructure.PostgreSQL/Factories/AuditLogFactory.cs`
2. Méthodes statiques de création/normalisation
3. Remplacer 17 occurrences
4. Build + Tests
5. Commit atomique

### Étape 5 : TokenUsageMetricFactory
1. Créer `src/Infrastructure/LLMProxy.Infrastructure.PostgreSQL/Factories/TokenUsageMetricFactory.cs`
2. Méthodes de calcul (coût, durée)
3. Remplacer 11 occurrences
4. Build + Tests
5. Commit atomique

### Étape 6 : HttpContextExtensions
1. Créer `src/Presentation/LLMProxy.Gateway/Extensions/HttpContextExtensions.cs`
2. Méthodes `GetUserId()`, `GetTenantId()`, `GetApiKeyId()`
3. Remplacer 6 occurrences dans middlewares
4. Build + Tests
5. Commit atomique

### Étape 7 : ValidationMessages
1. Créer `src/Application/LLMProxy.Application/Common/ValidationMessages.cs`
2. Constantes statiques pour messages récurrents
3. Remplacer 30+ messages dans validators
4. Build + Tests
5. Commit atomique

### Étape 8 : JsonSerializerOptionsFactory
1. Créer `src/Infrastructure/LLMProxy.Infrastructure.Redis/Factories/JsonSerializerOptionsFactory.cs`
2. Méthode statique `CreateDefault()`
3. Remplacer 3 occurrences
4. Build + Tests
5. Commit atomique

### Étape 9 : Vérification pattern `?? throw`
1. Analyser usage dans constructeurs
2. Documenter si pattern intentionnel (guard clauses)
3. Pas de modification si acceptable

### Étape 10 : Codes statut HTTP
1. Remplacer nombres magiques par `StatusCodes.*`
2. Exemple : `200` → `StatusCodes.Status200OK`
3. Build + Tests
4. Commit atomique

### Étape 11 : Validation finale
1. Exécuter build complet : `dotnet build --no-restore`
2. Exécuter tests complets : `dotnet test --no-build`
3. Re-vérifier conformité ADR-003 (100% attendu)
4. Mettre à jour rapport ANALYSE_CONFORMITE_ADR-003.md

## ESTIMATION

- **Durée** : 8-12 heures
- **Complexité** : Moyenne à Haute
- **Risque** : Faible (tests existants valideront non-régression)

## NOTES

- Chaque étape DOIT être validée par build + tests avant de continuer
- Commits atomiques obligatoires (1 commit par duplication corrigée)
- Documentation XML complète sur toutes les nouvelles classes/méthodes
- Respecter conventions de nommage et organisation du projet


## TRACKING
Début: 2025-12-21T22:34:33.3063794Z

**Session 2 Reprise**: 2025-12-22T01:30:00Z

### Progression (Session 2 - 2025-12-22)

**Reprise tâche** : 2025-12-22T01:30:00Z (87/149 lines, 58%)
**Branch** : feature/007b--complete-adr-003-dry

**Objectif** : Atteindre 149/149 lines (100% conformité ADR-003)

- ✅ **Duplication Guid.Empty** : Guard.AgainstEmptyGuid dans 6 entités (-16 lines) - Commit 8
- ✅ **Duplication JsonSerializerOptions** : JsonSerializerOptionsFactory créé (-10 lines) - Commit 9  
- ✅ **Duplication HTTP Status Codes** : StatusCodes.* constants (-6 lignes estimées) - Commit 10

**Total Session 2** : +32 lines saved  
**Total Global** : 119/149 lines saved (80%)  
**Commits** : 10 atomiques (7 session 1 + 3 session 2)

### Analyse Finale

**Progression** :
- Session 1 : 87/149 (58%)
- Session 2 : 119/149 (80%)
- Restant : 30/149 (20%)

**30 lignes restantes** identifiées dans rapport ADR-003 :
1. ✅ **Guid.Empty validations** (8 occurrences) → Guard.AgainstEmptyGuid appliqué
2. ✅ **JsonSerializerOptions** (3 occurrences) → Factory créé  
3. ✅ **HTTP status codes** (6 occurrences) → StatusCodes.* constants appliqués
4. ⏳ **Autres duplications mineures** (voir rapport complet ADR-003)


Fin: 2025-12-21T22:55:18.0582893Z

