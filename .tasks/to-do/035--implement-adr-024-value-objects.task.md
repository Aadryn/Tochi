# Tâche 035 - Implémenter ADR-024 Value Objects

## OBJECTIF

Implémenter Value Objects pour renforcer le Domain Model avec type safety et validation métier intégrée.

## JUSTIFICATION

- **Type safety** : Éviter primitive obsession
- **Validation métier** : Règles encapsulées dans les types
- **Immutabilité** : Objets value immuables par défaut
- **Synergie** : Utilisera Result<T> pour validation

## CRITÈRES DE SUCCÈS

- [ ] Value Objects pour concepts métier (Email, Password, ApiKeyValue, TenantSlug, etc.)
- [ ] Validation dans constructeurs privés + méthodes factory
- [ ] Utilisation de Result<T> pour création
- [ ] Opérateurs égalité/comparaison
- [ ] Migration entités vers Value Objects
- [ ] Tests unitaires validation
- [ ] Build SUCCESS sans warnings
- [ ] Documentation README.md

## DÉPENDANCES

-  ADR-023 Result Pattern (implémenté)
-  ADR-018 Guard Clauses (à vérifier/implémenter)
-  ADR-015 Immutability (à vérifier)

## PÉRIMÈTRE

**Value Objects prioritaires** :
- Email (validation format RFC 5322)
- Password (force, hashing)
- ApiKeyValue (génération, format)
- TenantSlug (unicité, format URL-safe)
- UserId, TenantId (strong-typed IDs)

**Exclusions** :
- Money/Currency (pas dans le domaine actuel)
- Address (pas dans le domaine actuel)
