# 038 - Implémenter ADR-040 : outbox pattern

**Date création:** 2025-12-22 16:19:58 UTC

## OBJECTIF

Implémenter l'ADR-040 (outbox pattern) qui est actuellement au statut "Accepté" mais pas encore implémenté dans le code.

## DESCRIPTION

Cet ADR définit une décision architecturale acceptée qui doit être mise en œuvre dans le codebase.

**Fichier ADR:** `docs/adr/040-outbox-pattern.adr.md`

## ACTIONS

1. **Lire** complètement l'ADR-040
2. **Comprendre** les exigences, contraintes et décisions
3. **Analyser** le code existant pour identifier ce qui manque
4. **Implémenter** selon les directives de l'ADR:
   - Créer les fichiers nécessaires
   - Modifier le code existant si requis
   - Respecter les patterns définis
5. **Tester** l'implémentation:
   - Créer/mettre à jour tests unitaires
   - Vérifier build (0 erreurs, 0 warnings)
   - Valider avec Chrome DevTools si UI/API
6. **Documenter**:
   - Ajouter commentaires XML si nouveaux types
   - Mettre à jour README si nécessaire
7. **Finaliser**:
   - Mettre à jour le statut de l'ADR à "Implémenté"
   - Commiter atomiquement

## CRITÈRES DE SUCCÈS

- [ ] ADR-040 lu et compris intégralement
- [ ] Code implémenté conforme à 100% aux directives de l'ADR
- [ ] Tests unitaires passent (100% réussite)
- [ ] Build réussit (0 erreurs, 0 warnings)
- [ ] Application testée fonctionnellement (si applicable)
- [ ] Documentation XML à jour (français, didactique)
- [ ] README mis à jour si nécessaire
- [ ] Statut ADR changé à "Implémenté" dans le fichier .adr.md
- [ ] Aucun conflit avec autres ADR
- [ ] Commits atomiques avec messages conventionnels

## DÉPENDANCES

- Lire `docs/adr/040-outbox-pattern.adr.md` AVANT tout développement
- Vérifier compatibilité avec ADR existants (001-041)
- Vérifier si d'autres ADR dépendent de celui-ci

## RÉFÉRENCES

- `docs/adr/040-outbox-pattern.adr.md`
- `.github/instructions/adr.documentation.instructions.md`
- `.github/instructions/csharp.*.instructions.md`

## NOTES

Ce fichier sera déplacé dans `.tasks/in-progress/` au démarrage de l'implémentation.
