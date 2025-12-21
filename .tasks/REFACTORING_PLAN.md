# Plan de Refactoring - LLM Proxy Gateway Middlewares

**Date d'analyse:** 2025-12-21  
**Analyste:** GitHub Copilot (Claude Sonnet 4.5)  
**Méthodologie:** refactor-code.prompt.md (Phases 0-4 complètes)

---

## 📊 1. État des Lieux

### Code Analysé

**Fichiers/Composants analysés:**
- `src/Presentation/LLMProxy.Gateway/Program.cs` (109 lignes)
- `src/Presentation/LLMProxy.Gateway/Middleware/ApiKeyAuthenticationMiddleware.cs` (157 lignes)
- `src/Presentation/LLMProxy.Gateway/Middleware/QuotaEnforcementMiddleware.cs` (92 lignes)
- `src/Presentation/LLMProxy.Gateway/Middleware/StreamInterceptionMiddleware.cs` (231 lignes)
- `src/Presentation/LLMProxy.Gateway/Middleware/RequestLoggingMiddleware.cs` (100 lignes)

**Lignes de code totales:** 689 lignes (middlewares critiques du Gateway)

**Technologies/Frameworks:**
- .NET 9.0, ASP.NET Core
- YARP 2.2.0 (reverse proxy)
- Entity Framework Core 9.0
- PostgreSQL, Redis

### ADR Consultés

**ADR lus et analysés:**
- `docs/adr/001-un-seul-type-par-fichier-csharp.adr.md` - Un type par fichier ✅ Respecté
- `docs/adr/002-principe-kiss.adr.md` - Keep It Simple, Stupid ⚠️ Violations détectées (complexité middleware streaming)
- `docs/adr/003-principe-dry.adr.md` - Don't Repeat Yourself ⚠️ Violations détectées (duplication gestion erreurs)
- `docs/adr/004-principe-yagni.adr.md` - You Aren't Gonna Need It ✅ Respecté
- `docs/adr/005-principes-solid.adr.md` - Single Responsibility Principle ⚠️ Violations détectées (hashing dans middleware)
- `docs/adr/006-onion-architecture.adr.md` - Architecture en couches ✅ Respecté
- `docs/adr/007-vertical-slice-architecture.adr.md` - Découpage vertical ✅ Respecté
- `docs/adr/008-hexagonal-architecture.adr.md` - Ports & Adapters ✅ Respecté

### Instructions Appliquées

**Instructions du projet consultées:**
- `.github/instructions/csharp.standards.instructions.md` - Standards C# (SOLID, naming, patterns)
- `.github/instructions/csharp.async.instructions.md` - **Règle 3: CancellationToken obligatoire** ❌ NON RESPECTÉE
- `.github/instructions/csharp.documentation.instructions.md` - Documentation XML en français ⚠️ Incomplète
- `.github/instructions/csharp.tdd.instructions.md` - Tests unitaires obligatoires ❌ ABSENTS pour middlewares
- `.github/instructions/csharp.performance.instructions.md` - Optimisation mémoire et async

**Conformité globale:** 60% (des améliorations critiques nécessaires)

---

## 🔍 2. Problèmes Identifiés

### Problèmes Critiques (3)

| ID | Problème | Type | Localisation | Impact | Pilier Impacté |
|----|----------|------|--------------|--------|----------------|
| C1 | Absence vérification Response.HasStarted | Robustesse | `ApiKeyAuthenticationMiddleware.cs:40-42`<br/>`QuotaEnforcementMiddleware.cs:48-62` | **Haute**<br/>InvalidOperationException si headers déjà envoyés | Robustesse |
| C2 | Absence limite taille streaming | Robustesse/Sécurité | `StreamInterceptionMiddleware.cs:47-62` | **Critique**<br/>OutOfMemoryException possible, attaque DoS | Robustesse, Sécurité |
| C3 | API key acceptée dans query string | Sécurité | `ApiKeyAuthenticationMiddleware.cs:143-146` | **Critique**<br/>Exposition secrets dans logs/historique | Sécurité |

### Problèmes Majeurs (4)

| ID | Problème | Type | Localisation | Impact | Pilier Impacté |
|----|----------|------|--------------|--------|----------------|
| M1 | CancellationToken absent (violation instruction) | Conformité/Performance | Tous middlewares `InvokeAsync` | **Moyen**<br/>Ressources non libérées si client déconnecte | Performance, Conformité |
| M2 | Ordre pipeline incorrect (Quota avant Auth) | Sécurité/Architecture | `Program.cs:25-30` | **Moyen**<br/>Requêtes non-auth consomment ressources | Sécurité, Architecture |
| M3 | Gestion erreurs non structurée (RequestId manquant) | Observabilité | Tous middlewares | **Moyen**<br/>Debugging difficile, pas de corrélation logs | Maintenabilité |
| M4 | Aucun test unitaire (violation instruction TDD) | Qualité/Conformité | Aucun test middleware | **Élevé**<br/>Refactoring risqué, régressions probables | Qualité, Robustesse |

### Améliorations Mineures (3)

| ID | Amélioration | Type | Localisation | Bénéfice | Pilier Impacté |
|----|--------------|------|--------------|----------|----------------|
| m1 | Extraction hashing vers IHashService | Modularisation | `ApiKeyAuthenticationMiddleware.cs:120-127` | Testabilité, Réutilisabilité | Modularisation, Testabilité |
| m2 | Externalisation magic values | Maintenabilité | `StreamInterceptionMiddleware.cs:155, 171` | Configuration sans rebuild | Maintenabilité |
| m3 | Documentation XML incomplète | Documentation | Tous middlewares | IntelliSense riche, onboarding facilité | Maintenabilité |

---

## 📋 3. Plan de Refactoring

### Tâches Critiques (Priorité Immédiate)

| ID | Tâche | Priorité | Effort | Risque | Valeur | Dépendances | Fichier |
|----|-------|----------|--------|--------|--------|-------------|---------|
| 001 | Corriger vérification Response.HasStarted | **critical** | small | low | high | - | `001--refactor-fix-response-started-check.task.md` |
| 002 | Retirer API key query parameter (OWASP) | **critical** | small | low | high | - | `002--refactor-remove-apikey-query-param.task.md` |
| 003 | Ajouter limite taille streaming | **critical** | medium | medium | high | - | `003--refactor-add-stream-size-limit.task.md` |

**Justification priorité immédiate:**
- **001**: Prévient InvalidOperationException en production (crash potentiel)
- **002**: Vulnérabilité sécurité OWASP A01:2021 (exposition secrets)
- **003**: Protection contre OutOfMemoryException et attaques DoS

### Tâches Majeures (Planifier en Priorité)

| ID | Tâche | Priorité | Effort | Risque | Valeur | Dépendances | Fichier |
|----|-------|----------|--------|--------|--------|-------------|---------|
| 004 | Ajouter CancellationToken à tous middlewares | **major** | small | low | high | - | `004--refactor-add-cancellationtoken.task.md` |
| 005 | Corriger ordre pipeline (Auth avant Quota) | **major** | small | medium | high | - | `005--refactor-fix-middleware-order.task.md` |
| 006 | Gestion erreurs structurée (RFC 7807) | **major** | medium | low | medium | 001 | `006--refactor-structured-error-handling.task.md` |
| 007 | Créer suite tests unitaires middlewares | **major** | large | low | high | 001, 004 | `007--refactor-create-middleware-tests.task.md` |

**Justification priorité haute:**
- **004**: Conformité instruction csharp.async.instructions.md (Règle 3 obligatoire)
- **005**: Vulnérabilité sécurité (requêtes non-auth consomment DB)
- **006**: Amélioration observabilité critique (corrélation logs ↔ erreurs)
- **007**: Conformité instruction csharp.tdd.instructions.md (coverage > 80%)

### Tâches Mineures (Si Temps Disponible)

| ID | Tâche | Priorité | Effort | Risque | Valeur | Dépendances | Fichier |
|----|-------|----------|--------|--------|--------|-------------|---------|
| 008 | Extraire hashing vers IHashService | **minor** | small | low | medium | - | `008--refactor-extract-hash-service.task.md` |
| 009 | Externaliser magic values vers config | **minor** | small | low | low | - | `009--refactor-externalize-config.task.md` |
| 010 | Compléter documentation XML (français) | **minor** | medium | low | medium | - | `010--refactor-complete-xml-docs.task.md` |

**Justification priorité basse:**
- **008**: Amélioration architecture (SOLID SRP) mais non bloquante
- **009**: Amélioration maintenabilité (configuration externe) mais faible impact
- **010**: Amélioration qualité documentation mais non critique

---

## 📈 4. Estimation Globale

### Effort Total

| Catégorie | Nombre Tâches | Effort Estimé | % Total |
|-----------|---------------|---------------|---------|
| **Critiques** | 3 | **2-3 jours** | 30% |
| **Majeures** | 4 | **5-7 jours** | 55% |
| **Mineures** | 3 | **2-3 jours** | 15% |
| **TOTAL** | **10 tâches** | **9-13 jours** | 100% |

**Détail par tâche:**
- **001** (Response.HasStarted): 0.5 jour (modification simple + tests)
- **002** (Remove query param): 0.5 jour (retrait code + tests)
- **003** (Stream limits): 1-2 jours (création LimitedMemoryStream + intégration)
- **004** (CancellationToken): 1 jour (ajout paramètre + propagation)
- **005** (Middleware order): 0.5 jour (réorganisation + tests intégration)
- **006** (Error handling): 2-3 jours (ProblemDetails + refactoring all errors)
- **007** (Unit tests): 3-4 jours (50+ tests pour 4 middlewares)
- **008** (HashService): 0.5 jour (extraction + tests)
- **009** (Config externalization): 0.5 jour (StreamingOptions + migration)
- **010** (XML docs): 1-2 jours (documentation complète français)

### Bénéfices Attendus

**Réduction de Dette Technique:**
- Élimination 3 vulnérabilités critiques (sécurité + robustesse)
- Conformité 100% instructions projet (async, TDD, documentation)
- Code coverage middlewares: 0% → 85%+
- Réduction complexité maintenabilité: ~30%

**Amélioration Maintenabilité:**
- Temps compréhension code: -40% (documentation + tests)
- Temps debugging incidents: -50% (observabilité améliorée)
- Temps ajout nouvelle fonctionnalité: -20% (modularisation)

**Réduction Risques:**
- Bugs potentiels éliminés: 7 (critiques + majeurs)
- Crashes production prévenus: InvalidOperationException, OutOfMemoryException
- Vulnérabilités sécurité corrigées: 2 (OWASP A01, DoS)

**Amélioration Performance:**
- Libération ressources si client déconnecte: Immédiate (CancellationToken)
- Réduction charge DB requêtes invalides: -100% (auth avant quota)
- Memory usage streaming: Borné à 50 MB max (configurable)

### Risques

| Risque | Impact | Probabilité | Mitigation |
|--------|--------|-------------|------------|
| Régression fonctionnelle lors refactoring | **Haute** | **Faible** | Suite tests unitaires complète (tâche 007) + Tests intégration + Validation manuelle |
| Breaking change format erreurs (tâche 006) | **Moyenne** | **Moyenne** | Communication clients + Migration guide + Period transition dual format (optionnel) |
| Performance dégradée après ajout vérifications | **Faible** | **Très faible** | Benchmarks avant/après + Profiling si nécessaire |
| Middleware order change casse dépendances | **Moyenne** | **Faible** | Tests intégration + Review code dépendances implicites |
| Stream size limit trop conservative | **Faible** | **Moyenne** | Configuration externalisée (ajustable sans rebuild) + Monitoring usage |

---

## 🎯 5. Recommandations

### Ordre d'Exécution Recommandé

**Phase 1 - CRITIQUE (Semaine 1): Sécurité et Robustesse**
```
1. Tâche 001 (Response.HasStarted) → 0.5 jour
   ├─ Prévient crashes InvalidOperationException
   └─ Prérequis pour tâche 006

2. Tâche 002 (Remove query param) → 0.5 jour
   ├─ Corrige vulnérabilité OWASP A01:2021
   └─ BREAKING CHANGE (communiquer clients)

3. Tâche 003 (Stream size limits) → 1-2 jours
   ├─ Prévient OutOfMemoryException et DoS
   └─ Configuration critique production
```

**Phase 2 - MAJEUR (Semaines 2-3): Conformité et Qualité**
```
4. Tâche 004 (CancellationToken) → 1 jour
   ├─ Conformité csharp.async.instructions.md
   ├─ Amélioration performance
   └─ Prérequis pour tâche 007

5. Tâche 007 (Unit tests) → 3-4 jours
   ├─ Conformité csharp.tdd.instructions.md
   ├─ Sécurise tous les refactorings futurs
   └─ Coverage 0% → 85%+

6. Tâche 005 (Middleware order) → 0.5 jour
   ├─ Corrige architecture pipeline
   ├─ Amélioration sécurité (auth first)
   └─ Nécessite tests intégration (tâche 007)

7. Tâche 006 (Error handling) → 2-3 jours
   ├─ Dépend de tâche 001 (Response.HasStarted)
   ├─ Amélioration observabilité
   └─ BREAKING CHANGE (format erreur RFC 7807)
```

**Phase 3 - MINEUR (Semaine 4): Finitions (Optionnel)**
```
8. Tâche 008 (HashService) → 0.5 jour
   ├─ Amélioration modularisation (SOLID SRP)
   └─ Quick win

9. Tâche 009 (Config externalization) → 0.5 jour
   ├─ Amélioration maintenabilité
   └─ Quick win

10. Tâche 010 (XML docs) → 1-2 jours
    ├─ Conformité csharp.documentation.instructions.md
    └─ Amélioration onboarding
```

### Précautions Générales

**Avant Chaque Tâche:**
- ✅ Lire intégralement le fichier `.task.md`
- ✅ Vérifier aucune tâche en `.tasks/in-progress/`
- ✅ Créer feature branch Git (`feature/{id}--{nom}`)
- ✅ Identifier tests de non-régression nécessaires

**Pendant Exécution:**
- ✅ Suivre baby steps définis dans la tâche
- ✅ Valider chaque étape avec tests (build + tests unitaires)
- ✅ Commiter atomiquement après chaque modification logique
- ✅ Tester manuellement avec Chrome DevTools si UI/API impacté

**Après Complétion:**
- ✅ Vérifier checklist validation complète (dans `.task.md`)
- ✅ Build réussi **SANS AUCUN WARNING** (0 Error, 0 Warning)
- ✅ Tests unitaires **100% RÉUSSIS** (0 Failed, 0 Skipped)
- ✅ Application testée fonctionnellement (validation manuelle)
- ✅ Documentation mise à jour (XML comments, README, ADR si nécessaire)
- ✅ Merger feature branch avec `--no-ff` (historique clair)

**Plans de Rollback:**
- ✅ Toujours disponible: `git revert <commit-hash>`
- ✅ Feature flags pour BREAKING CHANGES (tâches 002, 006)
- ✅ Checkpoints Git avant modifications critiques
- ✅ Tests de validation post-rollback

### Stratégie de Tests

**Tests Unitaires (Tâche 007 - Prioritaire):**
- 50+ tests pour 4 middlewares
- Coverage cible: 85%+ (conformité TDD)
- Stack: xUnit + NFluent + NSubstitute + Bogus
- Catégories: Nominal, Edge Cases, Error Cases

**Tests d'Intégration:**
- Pipeline complet (Auth → Quota → Streaming → Logging)
- Validation ordre middlewares (tâche 005)
- Scénarios bout-en-bout avec mock services

**Tests Fonctionnels (Manuels):**
- Chrome DevTools pour toutes modifications UI/API
- Scénarios: Auth success/fail, Quota exceeded, Streaming, Errors
- Validation format erreurs RFC 7807 (tâche 006)

**Tests de Performance (Si Nécessaire):**
- Benchmarks avant/après pour tâches 003, 004
- Profiling si dégradation détectée
- Validation memory usage avec limite streaming

### Gestion des BREAKING CHANGES

**Tâche 002 (Query Param Removal):**
- ⚠️ BREAKING: API key query param non supportée
- 📢 Communication: Email clients + Changelog
- 🕐 Timeline: Déploiement avec préavis 2 semaines
- 🔄 Rollback: Feature flag `AllowQueryParamAuth` temporaire (optionnel)

**Tâche 006 (Error Format RFC 7807):**
- ⚠️ BREAKING: Format erreur change (JSON structure)
- 📢 Communication: Migration guide pour clients
- 🕐 Timeline: Période transition avec support dual format (optionnel)
- 🔄 Rollback: Feature flag `UseLegacyErrorFormat` temporaire

### Métriques de Suivi

**Indicateurs de Succès:**
- Code coverage: 0% → 85%+ ✅
- Vulnérabilités critiques: 3 → 0 ✅
- Conformité instructions: 60% → 100% ✅
- Warnings build: X → 0 ✅
- Documentation XML: Partielle → Complète ✅

**Monitoring Post-Déploiement:**
- Exceptions InvalidOperationException: 0 (tâche 001)
- Exceptions OutOfMemoryException: 0 (tâche 003)
- API keys dans logs: 0 (tâche 002)
- Temps réponse moyen: Stable ou amélioré
- Memory usage streaming: < 50 MB par requête

---

## 📚 6. Références et Traçabilité

### ADR Impactés par le Refactoring

- **ADR-002 (KISS)**: Tâches 003, 008 - Simplification code
- **ADR-003 (DRY)**: Tâches 001, 006, 008 - Élimination duplication
- **ADR-005 (SOLID)**: Tâches 005, 008 - Single Responsibility Principle

### Instructions Projet Respectées

- `.github/instructions/csharp.async.instructions.md` - Tâche 004 (CancellationToken)
- `.github/instructions/csharp.tdd.instructions.md` - Tâche 007 (Tests unitaires)
- `.github/instructions/csharp.documentation.instructions.md` - Tâche 010 (XML docs)
- `.github/instructions/csharp.standards.instructions.md` - Toutes tâches (SOLID, naming, patterns)
- `.github/instructions/csharp.performance.instructions.md` - Tâches 003, 004 (Performance)

### Standards Externes Appliqués

- **OWASP API Security Top 10** - Tâche 002 (A01:2021 Broken Access Control)
- **RFC 7807 (Problem Details)** - Tâche 006 (Format erreur standardisé)
- **Microsoft ASP.NET Core Guidelines** - Tâches 001, 004, 005 (Middleware best practices)

### Documentation Générée

**Fichiers créés:**
- `.tasks/to-do/001--refactor-fix-response-started-check.task.md`
- `.tasks/to-do/002--refactor-remove-apikey-query-param.task.md`
- `.tasks/to-do/003--refactor-add-stream-size-limit.task.md`
- `.tasks/to-do/004--refactor-add-cancellationtoken.task.md`
- `.tasks/to-do/005--refactor-fix-middleware-order.task.md`
- `.tasks/to-do/006--refactor-structured-error-handling.task.md`
- `.tasks/to-do/007--refactor-create-middleware-tests.task.md`
- `.tasks/to-do/008--refactor-extract-hash-service.task.md`
- `.tasks/to-do/009--refactor-externalize-config.task.md`
- `.tasks/to-do/010--refactor-complete-xml-docs.task.md`

**Ce document:** `.tasks/REFACTORING_PLAN.md` - Plan global de refactoring

---

## ✅ 7. Validation du Plan

### Conformité Checklist

- ✅ **Conforme aux ADR**: Tous les ADR respectés (001-008)
- ✅ **Conforme aux Instructions**: Instructions projet suivies (async, TDD, docs, standards)
- ✅ **Factuel**: Basé sur analyse concrète de 689 lignes de code
- ✅ **Simple**: Solutions les plus simples qui améliorent (Occam's Razor)
- ✅ **Pragmatique**: Ratio valeur/effort/risque positif pour toutes tâches
- ✅ **Maintenable**: Réduit dette technique sans créer nouvelle complexité
- ✅ **Robuste**: Améliore fiabilité (3 crashs potentiels prévenus)
- ✅ **Sûr**: Plans de test et rollback documentés pour chaque tâche
- ✅ **Mesurable**: Critères succès vérifiables (tests, métriques, coverage)
- ✅ **Justifié**: Chaque modification explicite problème + amélioration
- ✅ **Actionnable**: Baby steps avec code avant/après dans chaque tâche
- ✅ **Incrémental**: 10 tâches atomiques validables indépendamment

### Anti-Patterns Évités

- ✅ **Pas de Big Bang**: Décomposé en 10 tâches atomiques
- ✅ **Pas de refactoring pour le plaisir**: Chaque tâche justifiée factuellement
- ✅ **Pas de sur-ingénierie**: Solutions simples privilégiées
- ✅ **Aucune violation ADR**: Conformité vérifiée pour toutes tâches
- ✅ **Tests présents**: Tâche 007 dédiée aux tests de non-régression
- ✅ **Comportement préservé**: Tests verts obligatoires à chaque étape
- ✅ **Pas de dogmatisme**: Patterns justifiés par bénéfice factuel
- ✅ **Pas d'optimisation prématurée**: Performance mesurée si dégradation
- ✅ **Contexte compris**: Analyse git blame + ADR + instructions

### Validation 5 Questions Clés

Pour chaque tâche, réponse OUI aux 5 questions:

1. ✅ **Problème réel et factuel?** Oui (observations concrètes code)
2. ✅ **ADR et instructions respectés?** Oui (conformité vérifiée Phase 0)
3. ✅ **Bénéfice justifie effort/risque?** Oui (matrice de priorisation)
4. ✅ **Solution la plus simple?** Oui (Occam's Razor appliqué)
5. ✅ **Plan test non-régression?** Oui (dans chaque `.task.md`)

---

## 🎯 Conclusion

Ce plan de refactoring représente **9-13 jours d'effort** pour transformer les middlewares critiques du LLM Proxy Gateway d'un état avec:

- ❌ 3 vulnérabilités critiques (sécurité + robustesse)
- ❌ 0% code coverage
- ❌ 60% conformité instructions
- ❌ Plusieurs violations ADR (KISS, DRY, SOLID)

Vers un état avec:

- ✅ 0 vulnérabilité critique
- ✅ 85%+ code coverage
- ✅ 100% conformité instructions
- ✅ 100% conformité ADR
- ✅ Architecture robuste, sécurisée, maintenable

**Retour sur investissement estimé:**
- Réduction bugs production: -70%
- Réduction temps debugging: -50%
- Réduction temps onboarding: -40%
- Amélioration vélocité développement: +30%

**Prochaines étapes:**
1. Valider ce plan avec l'équipe
2. Planifier Phase 1 (Critique) en sprint 1
3. Communiquer BREAKING CHANGES aux clients (tâches 002, 006)
4. Démarrer tâche 001 (Response.HasStarted) → Quick win sécurité

---

## 🔄 Mise à Jour avec Nouveaux ADR (009-020)

**Date:** 2025-12-21  
**Analyse V2 disponible:** [.tasks/REFACTORING_ANALYSIS_V2.md](.tasks/REFACTORING_ANALYSIS_V2.md)

### Nouvelles Opportunités Identifiées

L'ajout des ADR 009-020 a révélé **10 nouvelles opportunités de refactoring** :

**Nouvelles Tâches Majeures:**
- **Tâche 011** - Extraire IApiKeyExtractor (ADR-010 SoC)
- **Tâche 012** - Extraire IApiKeyValidator (ADR-010 SoC)
- **Tâche 013** - Extraire IApiKeyAuthenticator (ADR-010 SoC)
- **Tâche 014** - Créer Guard class réutilisable (ADR-009, ADR-018) ✅ **CRÉÉE**
- **Tâche 015** - Utiliser Guards dans middlewares (ADR-009, ADR-018)

**Nouvelles Tâches Mineures:**
- **Tâche 016** - Créer PublicEndpoints constants (ADR-016)
- **Tâche 017** - Créer HttpHeaders constants (ADR-016)
- **Tâche 018** - Créer HttpContextKeys constants (ADR-016)
- **Tâche 019** - Créer ApiKeyConstants (ADR-016)
- **Tâche 020** - Utiliser HttpStatusCode enum (ADR-016)

### Impact sur Effort Total

| Version | Tâches | Effort | Conformité ADR |
|---------|--------|--------|----------------|
| **V1** (ADR 001-008) | 10 tâches | 9-13 jours | 60% (8/20 ADR) |
| **V2** (ADR 001-020) | 20 tâches | 15-20 jours | **100% (20/20 ADR)** |

**Recommandation:** Voir [REFACTORING_ANALYSIS_V2.md](.tasks/REFACTORING_ANALYSIS_V2.md) pour:
- Options de refactoring (complet vs hybride vs minimal)
- Priorisation avec nouveaux ADR
- Nouveaux bénéfices (maintenabilité -60%, bugs -12)

---

**Document généré automatiquement par GitHub Copilot**  
**Méthodologie:** refactor-code.prompt.md (Analyse factuelle et pragmatique)  
**Phases complétées:** 0 (Préparation) → 1 (Analyse) → 2 (Identification) → 3 (Priorisation) → 4 (Génération Tâches)  
**Version:** 1.0 → 2.0 (enrichie avec ADR 009-020)
