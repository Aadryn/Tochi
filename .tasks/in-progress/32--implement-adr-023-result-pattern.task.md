# Tâche 32 - Implémenter ADR-023 Result Pattern

**ADR associé :** docs/adr/023-result-pattern.adr.md

## OBJECTIF

Implémenter le Result Pattern pour gérer les erreurs de manière explicite et type-safe, en remplaçant l'utilisation d'exceptions pour les cas métier prévisibles.

## JUSTIFICATION

Le Result Pattern apporte :
- **Performance** : Évite le coût des exceptions (~85% plus rapide)
- **Explicite** : Le type de retour indique que l'opération peut échouer
- **Lisibilité** : Flux linéaire sans try/catch dispersés
- **Composition** : Chaînage facile avec Bind/Map (Railway Oriented)
- **Type-safety** : Le compilateur force à gérer les erreurs

## CRITÈRES DE SUCCÈS

- [ ] Créer les types de base (Result, Result<T>, Error)
- [ ] Créer les erreurs par domaine (Error.User, Error.Tenant, etc.)
- [ ] Créer les extensions pour chaînage (Bind, Map, Tap, Match)
- [ ] Migrer services existants vers Result Pattern
- [ ] Adapter contrôleurs pour gérer les Results
- [ ] Tests unitaires pour Result et extensions
- [ ] Documentation README avec exemples
- [ ] Build SUCCESS (0 erreurs, 0 warnings)

## DÉPENDANCES

-  ADR-031 (Structured Logging) - pour logger les erreurs

## ÉTAPES D'IMPLÉMENTATION

1. **Créer projet Domain primitives** (si absent)
   - LLMProxy.Domain.Primitives ou dans LLMProxy.Domain

2. **Créer types de base**
   - Result.cs (sans valeur)
   - Result{T}.cs (avec valeur)
   - Error.cs (record avec Code + Message)

3. **Créer erreurs par domaine**
   - Error.User (NotFound, EmailExists, InvalidCredentials)
   - Error.Tenant (NotFound, Inactive, QuotaExceeded)
   - Error.ApiKey (Invalid, Expired, Revoked)
   - Error.Quota (Exceeded, Invalid)

4. **Créer extensions Railway-Oriented**
   - ResultExtensions.cs (Bind, Map, Tap, Match)
   - ValidationResult{T}.cs (pour erreurs multiples)

5. **Migrer services existants**
   - UserService  Result<User>
   - TenantService  Result<Tenant>
   - ApiKeyAuthenticator  Result<AuthResult>

6. **Adapter contrôleurs**
   - Créer ControllerExtensions.ToActionResult()
   - Mapper Error.Code vers HTTP Status

7. **Tests unitaires**
   - ResultTests.cs
   - ResultExtensionsTests.cs
   - ErrorTests.cs

8. **Documentation**
   - README.md avec exemples complets
   - Migration guide (Exception  Result)

## CONFORMITÉ ADR

-  ADR-023 : Utiliser Result pour cas métier prévisibles
-  ADR-027 : Defensive Programming (Guard clauses)
-  ADR-031 : Structured Logging pour erreurs
-  ADR-015 : Immutability (Result et Error immutables)
-  ADR-016 : Explicit over Implicit (erreurs explicites)

## ESTIMATION

**Durée estimée :** 5-6h
**Complexité :** Moyenne

## NOTES

- Conserver les exceptions pour cas vraiment exceptionnels (bugs, infrastructure)
- Utiliser implicit operators pour conversions fluides
- Pattern match pour mapper Error.Code vers HTTP status


## TRACKING
Début: 2025-12-22T11:03:22.0401426Z

