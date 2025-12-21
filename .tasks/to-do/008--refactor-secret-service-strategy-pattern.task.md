# Tâche 008 - Refactor SecretService (Strategy Pattern - ADR-005 SRP)

**Priorité** : CRITIQUE
**Estimation** : 8-10 heures

## OBJECTIF
Refactorer SecretService (312 lignes, 4 responsabilités) avec Strategy Pattern.

## ARCHITECTURE CIBLE
- ISecretProvider interface
- 4 providers concrets
- SecretEncryptor séparé
- SecretService réduit à ~80 lignes

## CRITÈRES DE SUCCÈS
- [ ] < 100 lignes
- [ ] Switch cases éliminés
- [ ] Build + Tests OK
