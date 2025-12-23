---
description: Organisation et documentation des fichiers de tâches du workflow - structure, nommage, tracking temporel et gestion des blocages
name: Workflow_Tasks_Organization
applyTo: "**/.tasks/**/*"
---

# Organisation des Tâches du Workflow - Guide Expert

Guide pour structurer, documenter et suivre les fichiers de tâches dans le dossier `.tasks/` du workflow de développement.

## ⛔ À NE PAS FAIRE

- **Ne commence jamais** une tâche sans lire intégralement `.github/copilot-instructions.md`
- **Ne démarre jamais** une nouvelle tâche si `.tasks/in-progress/` contient déjà un fichier
- **Ne traite jamais** les tâches dans le désordre (respecte l'ordre numérique 001, 002, 003...)
- **Ne bloque jamais** une tâche sans argumentation complète et détaillée
- **Ne marque jamais** une tâche "Done" sans validation de la checklist complète
- **Ne livre jamais** du code non testé ou sans tests unitaires
- **Ne produis jamais** du code de POC ou de démonstration inutile
- **Ne viole jamais** une décision d'ADR sans justification explicite
- **N'ajoute jamais** de dépendances sans vérification préalable

## ✅ À FAIRE

- **Lis toujours** le fichier de tâche intégralement avant de commencer
- **Vérifie toujours** les ADR applicables dans `docs/adr/`
- **Annote toujours** l'heure de début (UTC, ISO 8601) dans le fichier de tâche
- **Crée toujours** une feature branch Git avant de coder
- **Commite toujours** de façon atomique après chaque modification logique
- **Valide toujours** build + tests avant de compléter
- **Documente toujours** les blocages avec les actions pour débloquer
- **Déplace toujours** la tâche dans le bon dossier (`in-progress/`, `done/`, `blocked/`)

## ⚠️ RÈGLES IMPÉRATIVES (NON NÉGOCIABLES)

### 🚫 INTERDICTIONS ABSOLUES

**AVANT toute intervention sur une tâche :**

- ❌ **INTERDICTION ABSOLUE** de commencer une tâche sans avoir lu intégralement `.github/copilot-instructions.md`
- ❌ **INTERDICTION ABSOLUE** de commencer une tâche sans avoir vérifié TOUS les ADR dans `docs/adr/*.adr.md`
- ❌ **INTERDICTION ABSOLUE** de violer les règles du fichier copilot-instructions.md
- ❌ **INTERDICTION ABSOLUE** de violer une décision architecturale d'un ADR sans justification explicite et création d'un nouvel ADR de remplacement
- ❌ **INTERDICTION ABSOLUE** de démarrer une nouvelle tâche si `.tasks/in-progress/` contient déjà un fichier
- ❌ **INTERDICTION ABSOLUE** de traiter les tâches dans le désordre - TOUJOURS respecter l'ordre numérique strict (001, 002, 003...)
- ❌ **INTERDICTION ABSOLUE** de bloquer une tâche sans argumentation complète et détaillée
- ❌ **INTERDICTION ABSOLUE** de marquer une tâche comme "Done" sans validation complète de la checklist
- ❌ **INTERDICTION ABSOLUE** de livrer du code non testé ou sans tests unitaires associés
- ❌ **INTERDICTION ABSOLUE** de produire du code de POC (Proof of Concept) ou de démonstration inutile
- ❌ **INTERDICTION ABSOLUE** de produire du code incomplet ou partiellement implémenté
- ❌ **INTERDICTION ABSOLUE** d'ajouter des dépendances sans vérification préalable de leur nécessité et disponibilité
- ❌ **INTERDICTION ABSOLUE** de sur-ingénierie - Implémenter uniquement ce qui est strictement nécessaire

### ✅ ACTIONS OBLIGATOIRES AVANT TOUTE TÂCHE

**SYSTÉMATIQUEMENT, sans exception, dans cet ordre précis :**

1. **LIRE** l'intégralité de `.github/copilot-instructions.md` pour comprendre le workflow global
2. **LISTER** tous les ADR dans `docs/adr/*.adr.md` et **LIRE** ceux pertinents pour la tâche
3. **VÉRIFIER** les tâches en cours dans `.tasks/in-progress/` - DOIT être vide
4. **VÉRIFIER** les tâches bloquées dans `.tasks/blocked/` - Tenter de débloquer AVANT de prendre une nouvelle tâche
5. **LISTER** le contenu de `.tasks/to-do/` par ordre numérique croissant
6. **LIRE** l'intégralité du fichier de la première tâche à traiter (numéro le plus bas)
7. **IDENTIFIER** les ADR concernés par la tâche et les **RELIRE** intégralement
8. **VÉRIFIER** que la tâche ne viole aucune règle du copilot-instructions.md ni aucun ADR
9. **PLANIFIER** l'implémentation minimale nécessaire (pas de sur-ingénierie, pas de code inutile)
10. **IDENTIFIER** et **LISTER** tous les tests unitaires requis AVANT de coder
11. **VÉRIFIER** toutes les dépendances requises (packages NuGet, services, fichiers)
12. **ANNOTER** l'heure de début dans le fichier de tâche
13. **DÉPLACER** immédiatement le fichier vers `.tasks/in-progress/`

## 🎯 Actions Obligatoires (Mandatory)

### Structure des Dossiers .tasks/

```
.tasks/
├── to-do/          # Tâches à traiter en priorité (file d'attente)
├── in-progress/    # Tâche en cours d'exécution (une seule à la fois)
├── done/           # Tâches complétées et archivées
└── blocked/        # Tâches bloquées avec raison documentée
```

### Format des Fichiers de Tâches

**Nommage OBLIGATOIRE** : `NNN-descriptif-action.task.md`
- `NNN` : Numéro séquentiel (001, 002, 003, ...)
- Descriptif court et clair de l'action
- Extension `.task.md` pour identification

**Exemples :**
```
001-fix-nginx-config.task.md
002-add-health-check.task.md
003-deploy-new-service.task.md
```

## 📝 Structure d'un Fichier de Tâche

### Template de Base

```markdown
# Titre de la Tâche

## OBJECTIF (QUOI)
[Description précise de ce qui doit être accompli]

## JUSTIFICATION (POURQUOI)
[Raison d'être de cette tâche, contexte, problème résolu]

## DÉPENDANCES
- Fichier X doit exister
- Variable Y doit être définie
- Tâche Z doit être complétée

## CONTRAINTES
- Respecter l'ADR-XXX pour [sujet]
- Ne pas modifier le fichier Y
- Utiliser la structure Z

## CRITÈRES DE SUCCÈS
- [ ] Critère 1 validé
- [ ] Critère 2 validé
- [ ] Tests passent
- [ ] Documentation mise à jour

## PLAN D'IMPLÉMENTATION
[Section ajoutée automatiquement AVANT l'exécution]

### Code Minimum à Produire
[Liste exhaustive des fichiers/classes/méthodes strictement nécessaires]
[AUCUN code POC, démo ou inutile]

### Tests Unitaires Requis
[Liste EXHAUSTIVE et COMPLÈTE des tests à implémenter AVANT ou EN PARALLÈLE du code]
[OBLIGATOIRE: Couvrir TOUS les cas nominaux, limites et exceptions]

#### Tests Cas Nominaux (Happy Path)
- [ ] Test 1: Scénario principal - Comportement attendu standard
- [ ] Test 2: Scénario alternatif 1 - Variation valide du comportement
- [ ] Test 3: Scénario alternatif 2 - Autre variation valide

#### Tests Cas Limites (Edge Cases)
- [ ] Test 4: Valeurs limites min/max (0, Int.MaxValue, String.Empty, etc.)
- [ ] Test 5: Collections vides ou avec un seul élément
- [ ] Test 6: Valeurs exactement à la frontière des conditions
- [ ] Test 7: Cas de concurrence ou race conditions si applicable

#### Tests Cas d'Exception (Error Cases)
- [ ] Test 8: Paramètres null - ArgumentNullException attendue
- [ ] Test 9: Paramètres invalides - ArgumentException attendue
- [ ] Test 10: État incohérent - InvalidOperationException attendue
- [ ] Test 11: Ressource non trouvée - NotFoundException attendue
- [ ] Test 12: Timeout ou dépassement - TimeoutException attendue

### Dépendances à Vérifier
[Liste des packages NuGet, services externes, fichiers requis]
- [ ] Dépendance 1: Vérifier disponibilité et version
- [ ] Dépendance 2: Vérifier configuration

### Ordre d'Implémentation
1. Écrire les tests unitaires (TDD - Test Driven Development)
2. Implémenter le code minimum pour faire passer les tests
3. Refactorer si nécessaire (en maintenant les tests verts)
4. Valider la couverture de tests (> 80%)

## TRACKING
[Section ajoutée automatiquement lors de l'exécution]
```

### Section VALIDATION ADR/INSTRUCTIONS (Obligatoire)

**OBLIGATOIRE AVANT de démarrer l'exécution :**

```yaml
## VALIDATION PRÉ-EXÉCUTION

### Fichiers de Référence Lus
- [ ] `.github/copilot-instructions.md` - Lu intégralement
- [ ] `docs/adr/001-decision-name.adr.md` - Lu et compris
- [ ] `docs/adr/003-another-decision.adr.md` - Lu et compris
[Lister TOUS les ADR pertinents]

### Conformité Vérifiée
- [ ] Aucune violation des règles du copilot-instructions.md détectée
- [ ] Aucune violation des décisions ADR détectée
- [ ] Toutes les dépendances requises sont disponibles
- [ ] Tous les critères de succès sont mesurables et atteignables

### Décisions Architecturales Impactées
- ADR-001 : [Expliquer l'impact ou la conformité]
- ADR-003 : [Expliquer l'impact ou la conformité]
[Documenter TOUS les ADR impactés]

Date validation: 2025-11-27 14:25:00
Validé par: Agent Copilot
```

### Section TRACKING (Gérée par l'Agent)

**Ajoutée automatiquement lors de l'exécution :**

```yaml
## TRACKING
Début: 2025-11-27 14:30:15
Fin: 2025-11-27 14:45:30
Durée: 15min 15s
```

**En cas de blocage :**

```yaml
## TRACKING
Début: 2025-11-27 14:30:15

## HISTORIQUE DES BLOCAGES

### BLOCAGE #1
Début blocage: 2025-11-27 14:35:45
Temps actif avant blocage: 5min 30s

#### Raison du blocage
[Description précise et détaillée du problème rencontré]

#### Pourquoi ce blocage empêche la progression
[Expliquer POURQUOI cette tâche ne peut pas être complétée]
[Argumenter l'impact et les conséquences]

#### Action requise pour débloquer
[Lister les actions concrètes nécessaires]

#### Tentatives effectuées
- [Ce qui a été essayé - option 1]
- [Ce qui a été essayé - option 2]
- [Ce qui a été essayé - option 3]

#### Alternatives considérées
[Solutions alternatives envisagées et pourquoi elles ne conviennent pas]
```

**En cas de blocages multiples :**

```yaml
### BLOCAGE #2
Reprise après blocage #1: 2025-11-27 16:20:00
Durée blocage #1: 1h 44min 15s
Début blocage #2: 2025-11-27 16:45:30
Temps actif entre reprises: 25min 30s

#### Raison du blocage
[Nouveau problème rencontré]
...
```

**Après complétion avec blocages :**

```yaml
## TRACKING
Début: 2025-11-27 14:30:15
Fin: 2025-11-27 18:10:45

## RÉSUMÉ TEMPOREL
Temps actif total: 45min 20s
Temps de blocage total: 2h 55min 10s (blocage #1: 1h 44min 15s + blocage #2: 1h 10min 55s)
Durée totale: 3h 40min 30s
Nombre de blocages: 2
```

## 🔄 Cycle de Vie d'une Tâche

### 1. Création dans `.tasks/to-do/`

**Fichier créé manuellement avec :**
- Numéro séquentiel approprié (UNIQUE sur tout le dossier `.tasks/`)
- Description complète (QUOI, POURQUOI, DÉPENDANCES, CONTRAINTES, CRITÈRES)
- Aucune section TRACKING ni VALIDATION (seront ajoutées lors de l'exécution)

### 2. Pré-Validation AVANT Exécution

**OBLIGATOIRE - Actions à effectuer dans cet ordre strict :**

1. **LIRE** `.github/copilot-instructions.md` - Intégralement, sans exception
2. **LISTER** tous les ADR : `dir docs\adr\*.adr.md` (PowerShell) ou `ls docs/adr/*.adr.md` (Bash)
3. **IDENTIFIER** les ADR pertinents pour la tâche en cours
4. **LIRE** intégralement chaque ADR pertinent identifié
5. **VÉRIFIER** la conformité de la tâche avec :
   - Toutes les règles du copilot-instructions.md
   - Toutes les décisions architecturales des ADR
   - Toutes les dépendances et contraintes listées
6. **PLANIFIER** l'implémentation minimale dans la section "PLAN D'IMPLÉMENTATION" :
   - Lister le code minimum strictement nécessaire (pas de POC, pas de démo)
   - Lister TOUS les tests unitaires requis (un par scénario, cas limite, erreur)
   - Identifier et vérifier TOUTES les dépendances (NuGet, services, fichiers)
   - Définir l'ordre TDD : Tests → Code → Refactor → Validation
7. **DOCUMENTER** la validation dans une section "VALIDATION PRÉ-EXÉCUTION" du fichier de tâche
8. **ANNOTER** l'heure exacte de validation (utiliser l'outil `time`)

**Si NON-CONFORMITÉ détectée :**
- **STOPPER** immédiatement
- **DOCUMENTER** la non-conformité dans le fichier de tâche
- **DÉPLACER** vers `.tasks/blocked/`
- **NOTIFIER** l'utilisateur avec argumentation complète

### 3. Démarrage de l'Exécution (Après Validation)

**Actions automatiques :**
1. Ajout de la section VALIDATION PRÉ-EXÉCUTION (si pas déjà faite)
2. Annotation de l'heure de début dans la section TRACKING
3. Déplacement vers `.tasks/in-progress/`
4. Travail sur la tâche en respectant strictement :
   - Toutes les règles du copilot-instructions.md
   - Toutes les décisions des ADR validés
   - Tous les critères de succès définis

### 4. Complétion ou Blocage

**Si complétée avec succès - VÉRIFICATION FINALE OBLIGATOIRE :**

1. **EXÉCUTER** la checklist de validation complète (voir section dédiée)
2. **VALIDER** que TOUS les critères de succès sont satisfaits
3. **VÉRIFIER** qu'aucune règle du copilot-instructions.md n'a été violée
4. **VÉRIFIER** qu'aucune décision ADR n'a été violée
5. **TESTER** que le code fonctionne (build, tests unitaires, tests d'intégration)
6. **ANNOTER** l'heure de fin et calculer la durée exacte
7. **AJOUTER** un résumé de conformité dans le fichier de tâche
8. **DÉPLACER** vers `.tasks/done/`

**Si bloquée - DOCUMENTATION EXHAUSTIVE OBLIGATOIRE :**

1. **IDENTIFIER** la cause racine exacte du blocage
2. **EXPLIQUER** POURQUOI la progression est impossible (argumenter)
3. **LISTER** TOUTES les tentatives effectuées (avec détails)
4. **DOCUMENTER** toutes les alternatives considérées et pourquoi elles sont rejetées
5. **SPÉCIFIER** les actions concrètes requises pour débloquer
6. **ANNOTER** l'heure exacte de blocage et le temps actif avant blocage
7. **AJOUTER** l'HISTORIQUE DES BLOCAGES avec argumentation complète
8. **DÉPLACER** vers `.tasks/blocked/`
9. **NOTIFIER** l'utilisateur avec un message clair, factuel et actionnable

### 5. Reprise après Déblocage

**AVANT toute reprise - VÉRIFICATION OBLIGATOIRE :**

1. **VÉRIFIER** que la cause du blocage a été résolue (preuve concrète)
2. **RELIRE** `.github/copilot-instructions.md` - Vérifier si des règles ont changé
3. **RELIRE** les ADR pertinents - Vérifier si des décisions ont changé
4. **DOCUMENTER** la résolution du blocage dans le fichier de tâche
5. **ANNOTER** l'heure exacte de reprise et calculer la durée du blocage
6. **DÉPLACER** de `.tasks/blocked/` vers `.tasks/in-progress/`
7. **REPRENDRE** le travail en appliquant strictement les règles
8. **ANNOTER** l'heure de fin après complétion
9. **CALCULER** et documenter le RÉSUMÉ TEMPOREL complet (temps actif, temps bloqué, durée totale)

## 📋 Checklist de Validation (OBLIGATOIRE)

**CETTE CHECKLIST DOIT ÊTRE VALIDÉE À 100% AVANT de marquer une tâche comme "Done"**

### 🔍 Conformité aux Référentiels

- [ ] **`.github/copilot-instructions.md`** : Relu intégralement avant de commencer
- [ ] **Règles du copilot-instructions.md** : AUCUNE règle violée (vérification point par point)
- [ ] **ADR pertinents** : Tous identifiés et relus intégralement
- [ ] **Décisions ADR** : AUCUNE décision violée ou contournée
- [ ] **Piliers de qualité** : Les 12 piliers du copilot-instructions.md respectés et documentés

### ✅ Complétion Technique

- [ ] **Fichiers requis** : Tous créés/modifiés selon spécifications
- [ ] **Code conforme** : Standards du projet respectés (formatage, conventions, architecture)
- [ ] **Code minimum** : UNIQUEMENT le code strictement nécessaire (pas de POC, pas de démo, pas de sur-ingénierie)
- [ ] **Dépendances vérifiées** : Toutes les dépendances sont disponibles, compatibles et configurées
- [ ] **Build réussi** : `dotnet build` passe sans erreur ni warning
- [ ] **Tests unitaires écrits** : TOUS les tests du plan d'implémentation sont implémentés
- [ ] **Tests COMPLETS** : Les 3 catégories obligatoires sont couvertes :
  - [ ] **Cas nominaux** : Tous les scénarios de succès testés (minimum 1 par méthode)
  - [ ] **Cas limites** : Toutes les valeurs frontières testées (minimum 2 par méthode)
  - [ ] **Cas d'exception** : Toutes les erreurs possibles testées (minimum 2 par méthode)
- [ ] **Tests PERTINENTS** : Chaque test vérifie un comportement spécifique et utile
- [ ] **Tests MULTIPLES** : Minimum 3-5 tests par méthode publique implémentée
- [ ] **Tests unitaires passent** : 100% de succès (aucun test rouge ou ignoré)
- [ ] **Couverture de tests** : > 80% du nouveau code couvert par des tests
- [ ] **Couverture branches** : > 70% des branches (if/else/switch) couvertes
- [ ] **Tests d'intégration** : Passent si applicables
- [ ] **Tests structure AAA** : Arrange-Act-Assert respecté dans chaque test
- [ ] **Tests assertions** : Chaque test contient au moins une assertion explicite
- [ ] **Tests nommage** : Format `MethodName_Scenario_ExpectedBehavior` respecté
- [ ] **Pas de régression** : Aucun test précédemment fonctionnel ne doit échouer
- [ ] **TDD respecté** : Tests écrits AVANT ou EN PARALLÈLE du code de production

### 📚 Documentation

- [ ] **Documentation code** : Commentaires XML pour API publiques
- [ ] **Documentation technique** : Mise à jour si changement d'architecture
- [ ] **ADR créé** : Si nouvelle décision architecturale majeure (format `*.adr.md`)
- [ ] **README/CHANGELOG** : Mis à jour si changement visible utilisateur

### 🎯 Critères de Succès

- [ ] **Tous les critères de succès définis dans la tâche sont validés** (vérification un par un)
- [ ] **Comportement attendu** : Vérifié manuellement ou par test automatisé
- [ ] **Edge cases gérés** : Cas limites identifiés et testés

### 🧹 Qualité du Code

- [ ] **Aucun fichier temporaire** : Pas de .tmp, .swp, ~, .bak dans le commit
- [ ] **Aucun code commenté** : Pas de code mort laissé en commentaire
- [ ] **Aucun TODO/FIXME** : Tous résolus ou documentés dans une nouvelle tâche
- [ ] **Aucun hardcoding** : Pas de valeurs codées en dur (utiliser configuration)
- [ ] **Gestion d'erreurs** : Tous les chemins d'erreur gérés explicitement
- [ ] **Logs appropriés** : Niveaux de log corrects (Debug, Info, Warning, Error)

### 🔐 Sécurité

- [ ] **Aucun secret exposé** : Pas de mots de passe, tokens, clés API dans le code
- [ ] **Validation des entrées** : Toutes les entrées utilisateur validées
- [ ] **Pas de faille de sécurité** : Injection SQL, XSS, CSRF prévenus

### 🚀 Performance

- [ ] **Pas de régression de performance** : Mesuré si applicable
- [ ] **Optimisations nécessaires** : Appliquées si identifiées
- [ ] **Requêtes N+1 évitées** : Si accès base de données

### ✍️ Traçabilité

- [ ] **TRACKING complet** : Heures de début/fin documentées avec durée exacte
- [ ] **VALIDATION PRÉ-EXÉCUTION** : Section ajoutée avec ADR et copilot-instructions.md validés
- [ ] **Historique complet** : Si blocages, tout est documenté avec argumentations

## 💡 Exemples de Tâches

### Exemple 1 : Tâche Simple

```markdown
# Corriger la configuration SSL de nginx

## OBJECTIF (QUOI)
Mettre à jour la configuration SSL de nginx pour utiliser TLS 1.3 uniquement
et désactiver les protocoles obsolètes (TLS 1.0, 1.1, 1.2).

## JUSTIFICATION (POURQUOI)
Suite à l'audit de sécurité, les protocoles TLS < 1.3 sont considérés comme
vulnérables. Mise en conformité avec les recommandations ANSSI.

## DÉPENDANCES
- Nginx version >= 1.18
- Certificats SSL valides présents dans /etc/ssl/certs/

## CONTRAINTES
- Respecter l'ADR-003 sur la configuration des services web
- Ne pas casser la compatibilité avec les clients modernes
- Prévoir un plan de rollback

## CRITÈRES DE SUCCÈS
- [ ] Configuration nginx mise à jour
- [ ] Tests de connexion SSL réussis
- [ ] Scan SSL Labs grade A+ obtenu
- [ ] Documentation mise à jour
```

### Exemple 2 : Tâche Bloquée (Après Exécution)

```markdown
# Déployer le nouveau service API

## OBJECTIF (QUOI)
Déployer le nouveau service API sur les serveurs de production.

## JUSTIFICATION (POURQUOI)
Nouvelle fonctionnalité demandée par le métier pour l'intégration avec
le système tiers.

## DÉPENDANCES
- Variable 'service_port' définie dans l'inventaire
- Certificats SSL configurés
- Base de données initialisée

## CONTRAINTES
- Respecter l'ADR-005 sur les déploiements en production
- Zero downtime requis

## CRITÈRES DE SUCCÈS
- [ ] Service déployé et fonctionnel
- [ ] Health check endpoint répond
- [ ] Logs accessibles
- [ ] Monitoring activé

## TRACKING
Début: 2025-11-27 14:30:15

## HISTORIQUE DES BLOCAGES

### BLOCAGE #1
Début blocage: 2025-11-27 14:35:45
Temps actif avant blocage: 5min 30s

#### Raison du blocage
La variable 'service_port' est requise pour le déploiement du nouveau service
mais n'est définie dans AUCUN fichier d'inventaire ou de configuration.

#### Pourquoi ce blocage empêche la progression
Sans le port de service défini, impossible de:
- Générer la configuration systemd (service ne peut pas bind sur un port)
- Configurer nginx en reverse proxy (pas de upstream port)
- Configurer le firewall (aucun port à autoriser)
Le déploiement est donc bloqué à 0% de complétion.

#### Action requise pour débloquer
L'utilisateur DOIT définir 'service_port' dans l'un des fichiers suivants:
- inventory/dev/group_vars/dotnet_servers.yml (recommandé)
- inventory/prod/group_vars/dotnet_servers.yml
- roles/dotnet-app/defaults/main.yml (si valeur par défaut acceptable)

#### Tentatives effectuées
- Recherche dans inventory/dev/group_vars/ - variable NON trouvée
- Recherche dans inventory/prod/group_vars/ - variable NON trouvée  
- Recherche dans roles/dotnet-app/defaults/main.yml - variable NON définie
- Recherche dans roles/dotnet-app/vars/main.yml - variable NON définie
- Grep dans tout le projet - AUCUNE occurrence trouvée

#### Alternatives considérées
- Utiliser un port par défaut (8080) → REJETÉ car risque de conflit
- Dériver du nom du service → REJETÉ car non standard et imprévisible
- Demander au runtime → REJETÉ car contraire au principe d'infrastructure as code
```

## 🎯 Bonnes Pratiques (Impératives)

### Pour la Création de Tâches

1. **Un objectif clair et atomique** : Une tâche = une action précise et complète
2. **Numérotation séquentielle unique** : ID unique sur tout le dossier `.tasks/`, ordre strict de traitement
3. **Critères de succès SMART** : Spécifiques, Mesurables, Atteignables, Réalistes, Temporellement définis
4. **Dépendances explicites et vérifiables** : Tout ce qui est requis AVANT de commencer
5. **Contraintes documentées exhaustivement** : ADR concernés (avec numéros), standards, limitations techniques
6. **Référence aux ADR** : Lister explicitement les ADR impactés avec leur numéro et titre
7. **Justification métier** : POURQUOI cette tâche existe (valeur, problème résolu)

### Pour la Validation Pré-Exécution

1. **TOUJOURS lire copilot-instructions.md AVANT** : Sans exception, même si déjà lu récemment
2. **TOUJOURS lister et lire les ADR pertinents** : Identifier tous ceux impactés par la tâche
3. **DOCUMENTER la validation** : Section VALIDATION PRÉ-EXÉCUTION obligatoire dans le fichier
4. **VÉRIFIER la non-violation** : Checklist explicite des règles et décisions respectées
5. **BLOQUER si non-conforme** : Ne JAMAIS forcer l'exécution d'une tâche non conforme

### Pour la Gestion des Blocages

1. **Argumentation exhaustive et factuelle** : Expliquer POURQUOI avec preuves concrètes
2. **Tentatives documentées avec détails** : Lister TOUTES les actions tentées et leurs résultats
3. **Actions de déblocage précises** : Dire EXACTEMENT ce qui est requis (pas de vague)
4. **Alternatives évaluées systématiquement** : Au moins 3 alternatives considérées avec analyse comparative
5. **Impact documenté** : Expliquer les conséquences du blocage sur le projet
6. **JAMAIS de blocage "léger"** : Si bloqué, c'est avec argumentation complète ou ça ne l'est pas

### Pour le Suivi Temporel

1. **Utiliser l'outil `time` SYSTÉMATIQUEMENT** : Heures exactes au format ISO 8601
2. **Calculer les durées avec précision** : Minutes et secondes (ex: 15min 30s)
3. **Distinguer temps actif vs bloqué** : Métriques séparées et sommées
4. **Résumé temporel final obligatoire** : Vue d'ensemble complète avec toutes les métriques
5. **Historique chronologique complet** : Traçabilité de TOUS les événements dans l'ordre

### Pour les Tests (Impératif)

1. **TDD OBLIGATOIRE** : Écrire les tests AVANT ou EN PARALLÈLE du code de production
2. **Couverture minimale 80%** : Toute nouvelle fonctionnalité doit être couverte à > 80%
3. **Tests COMPLETS et PERTINENTS** : Couvrir OBLIGATOIREMENT les 3 catégories :
   - **Cas nominaux** (Happy Path) : Tous les scénarios de succès attendus
   - **Cas limites** (Edge Cases) : Valeurs min/max, collections vides, frontières de conditions
   - **Cas d'exception** (Error Cases) : Toutes les erreurs possibles (null, invalid, not found, timeout)
4. **Tests MULTIPLES** : Minimum 3-5 tests par méthode publique (1+ nominal, 2+ limites, 2+ exceptions)
5. **Tests atomiques** : Un test = une assertion = un comportement vérifié
6. **Tests nommés explicitement** : `MethodName_Scenario_ExpectedBehavior` (ex: `GetUser_WhenUserNotFound_ThrowsNotFoundException`)
7. **Pas de tests ignorés** : Aucun `[Ignore]`, `[Skip]` ou test commenté dans le code final
8. **Tests rapides** : Tests unitaires < 100ms chacun (pas d'appels externes, pas de Thread.Sleep)
9. **Tests déterministes** : Résultats reproductibles (pas de dépendance à l'heure, random, etc.)
10. **Assertions claires** : Messages d'erreur explicites pour faciliter le diagnostic
11. **Arrange-Act-Assert** : Structure AAA obligatoire dans chaque test
12. **Vérifier AVANT de marquer Done** : `dotnet test --no-build` doit afficher 100% de succès

#### Exemples de Tests COMPLETS, PERTINENTS et MULTIPLES

**Pour une méthode `CalculateDiscount(decimal price, int quantity)` :**

```csharp
// ═══════════════════════════════════════════════════════════
// CAS NOMINAUX (Happy Path)
// ═══════════════════════════════════════════════════════════

[Fact]
public void CalculateDiscount_WithStandardPriceAndQuantity_ReturnsExpectedDiscount()
{
    // Arrange
    var calculator = new PriceCalculator();
    var price = 100m;
    var quantity = 5;
    
    // Act
    var result = calculator.CalculateDiscount(price, quantity);
    
    // Assert
    Assert.Equal(50m, result); // 10% de remise attendue
}

[Fact]
public void CalculateDiscount_WithHighQuantity_ReturnsHigherDiscount()
{
    // Arrange
    var calculator = new PriceCalculator();
    var price = 100m;
    var quantity = 20; // Quantité élevée
    
    // Act
    var result = calculator.CalculateDiscount(price, quantity);
    
    // Assert
    Assert.Equal(200m, result); // 20% de remise pour quantité élevée
}

// ═══════════════════════════════════════════════════════════
// CAS LIMITES (Edge Cases)
// ═══════════════════════════════════════════════════════════

[Fact]
public void CalculateDiscount_WithZeroPrice_ReturnsZero()
{
    // Arrange
    var calculator = new PriceCalculator();
    var price = 0m;
    var quantity = 5;
    
    // Act
    var result = calculator.CalculateDiscount(price, quantity);
    
    // Assert
    Assert.Equal(0m, result);
}

[Fact]
public void CalculateDiscount_WithOneQuantity_ReturnsMinimalDiscount()
{
    // Arrange
    var calculator = new PriceCalculator();
    var price = 100m;
    var quantity = 1; // Quantité minimale
    
    // Act
    var result = calculator.CalculateDiscount(price, quantity);
    
    // Assert
    Assert.Equal(0m, result); // Pas de remise pour quantité = 1
}

[Fact]
public void CalculateDiscount_WithMaxDecimalValue_DoesNotOverflow()
{
    // Arrange
    var calculator = new PriceCalculator();
    var price = decimal.MaxValue;
    var quantity = 1;
    
    // Act
    var result = calculator.CalculateDiscount(price, quantity);
    
    // Assert
    Assert.True(result >= 0); // Pas d'overflow
}

// ═══════════════════════════════════════════════════════════
// CAS D'EXCEPTION (Error Cases)
// ═══════════════════════════════════════════════════════════

[Fact]
public void CalculateDiscount_WithNegativePrice_ThrowsArgumentException()
{
    // Arrange
    var calculator = new PriceCalculator();
    var price = -100m;
    var quantity = 5;
    
    // Act & Assert
    var exception = Assert.Throws<ArgumentException>(() => 
        calculator.CalculateDiscount(price, quantity));
    Assert.Contains("price", exception.Message, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void CalculateDiscount_WithNegativeQuantity_ThrowsArgumentException()
{
    // Arrange
    var calculator = new PriceCalculator();
    var price = 100m;
    var quantity = -5;
    
    // Act & Assert
    var exception = Assert.Throws<ArgumentException>(() => 
        calculator.CalculateDiscount(price, quantity));
    Assert.Contains("quantity", exception.Message, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void CalculateDiscount_WithZeroQuantity_ThrowsArgumentException()
{
    // Arrange
    var calculator = new PriceCalculator();
    var price = 100m;
    var quantity = 0;
    
    // Act & Assert
    Assert.Throws<ArgumentException>(() => 
        calculator.CalculateDiscount(price, quantity));
}
```

**Résultat : 8 tests pour une seule méthode**
- ✅ 2 tests cas nominaux (scénarios de succès)
- ✅ 3 tests cas limites (valeurs frontières)
- ✅ 3 tests cas d'exception (gestion d'erreurs)
- ✅ Couverture : 100% lignes, 100% branches
- ✅ Tous les comportements validés et documentés

### Pour la Minimisation du Code

1. **YAGNI** (You Aren't Gonna Need It) : Implémenter UNIQUEMENT ce qui est requis MAINTENANT
2. **Pas de code spéculatif** : Pas de "au cas où", "peut-être utile plus tard"
3. **Pas de POC/démo** : Code de production uniquement, pas de prototypes jetables
4. **Pas de code mort** : Supprimer le code commenté, méthodes inutilisées, imports superflus
5. **Pas de duplication** : Factoriser le code dupliqué (DRY - Don't Repeat Yourself)
6. **Simplicité maximale** : Solution la plus simple qui respecte les 12 piliers de qualité
7. **Refactoring constant** : Simplifier et nettoyer au fur et à mesure

### Pour les Dépendances

1. **Vérifier AVANT d'ajouter** : Chaque nouvelle dépendance doit être justifiée et documentée
2. **Privilégier l'existant** : Utiliser les dépendances déjà présentes dans le projet
3. **Versions compatibles** : Vérifier la compatibilité avec les autres packages (.NET version, conflits)
4. **Licences vérifiées** : S'assurer que la licence est compatible avec le projet
5. **Maintenance active** : Privilégier les packages maintenus activement (dernière mise à jour < 1 an)
6. **Pas de dépendances lourdes** : Éviter les packages avec beaucoup de dépendances transitives
7. **Configuration validée** : Toute dépendance externe (API, DB, service) doit être configurée et testée

### Pour la Qualité du Code

1. **Respecter les 12 piliers du copilot-instructions.md** : Maintenabilité, Simplicité, Précision, Robustesse, Sécurité, Extensibilité, Performance, Factualité, Rationalisation, Harmonisation, Mutualisation, Modularisation
2. **Tests AVANT de marquer Done** : Build + Tests unitaires + Tests d'intégration
3. **Zéro fichier temporaire** : Nettoyer tous les fichiers de debug, tmp, swap
4. **Code review auto** : Relire son propre code avec œil critique avant validation
5. **Documentation synchronisée** : Mise à jour immédiate de la doc avec le code

### Pour la Communication

1. **Factuelle et précise** : Pas de "je pense", "peut-être", "probablement"
2. **Actionnable** : Toujours donner les prochaines étapes concrètes
3. **Traçable** : Références aux fichiers, lignes, commits, ADR
4. **Complète sans être verbeuse** : Toute l'information nécessaire, rien de superflu
5. **Structurée** : Sections claires, listes à puces, formatage markdown

## 📊 Métriques et Indicateurs de Qualité

### Indicateurs de Performance du Workflow

**Métriques à suivre pour chaque tâche :**

- **Temps de validation pré-exécution** : Temps passé à lire copilot-instructions.md + ADR (target: < 10min)
- **Temps actif d'exécution** : Temps réel de travail sur la tâche (sans blocages)
- **Temps de blocage total** : Somme de tous les blocages (target: 0min, objectif: minimiser)
- **Nombre de blocages** : Nombre de fois où la tâche a été bloquée (target: 0, objectif: minimiser)
- **Taux de conformité ADR** : Pourcentage de décisions ADR respectées (target: 100%)
- **Taux de conformité copilot-instructions** : Pourcentage de règles respectées (target: 100%)
- **Couverture de tests** : Pourcentage de code couvert par tests unitaires (target: > 80%)
- **Nombre de tests réussis/échoués** : Tous les tests doivent passer (target: 100% succès)

### Indicateurs de Qualité Globale

**Vue d'ensemble du projet `.tasks/` :**

```powershell
# Compter les tâches par statut
$todo = (Get-ChildItem .tasks\to-do\*.task.md).Count
$inProgress = (Get-ChildItem .tasks\in-progress\*.task.md).Count
$blocked = (Get-ChildItem .tasks\blocked\*.task.md).Count
$done = (Get-ChildItem .tasks\done\*.task.md).Count

# Calculer les métriques
$total = $todo + $inProgress + $blocked + $done
$completionRate = [math]::Round(($done / $total) * 100, 2)
$blockageRate = [math]::Round(($blocked / $total) * 100, 2)

Write-Host "📊 MÉTRIQUES WORKFLOW"
Write-Host "Total tâches: $total"
Write-Host "✅ Done: $done ($completionRate%)"
Write-Host "🔴 Blocked: $blocked ($blockageRate%)"
Write-Host "🟢 To-Do: $todo"
Write-Host "🟡 In-Progress: $inProgress"
```

**Objectifs de qualité :**
- ✅ **Taux de complétion** : > 70% (nombre de tâches done / total)
- ✅ **Taux de blocage** : < 10% (nombre de tâches bloquées / total)
- ✅ **Tâches en cours** : Toujours 0 ou 1 (jamais plus)
- ✅ **Conformité ADR** : 100% (aucune violation tolérée)
- ✅ **Conformité copilot-instructions** : 100% (aucune violation tolérée)

### Signaux d'Alerte (Red Flags)

**🔴 ALERTE CRITIQUE - Arrêt immédiat requis :**

- Plusieurs tâches dans `.tasks/in-progress/` simultanément (DOIT être 0 ou 1)
- Tâche marquée "Done" sans validation complète de la checklist
- Violation d'un ADR sans justification ni nouvel ADR de remplacement
- Violation d'une règle du copilot-instructions.md
- Code commité qui ne build pas ou avec tests en échec
- Tâche bloquée sans argumentation complète dans le fichier
- Tâche hors périmètre (ex: CI/CD) traitée alors qu'interdite

**🟠 ALERTE MODÉRÉE - Correction nécessaire :**

- Tâches traitées dans le désordre (numéros non séquentiels)
- Tâches bloquées non vérifiées avant démarrage d'une nouvelle tâche
- Section VALIDATION PRÉ-EXÉCUTION manquante ou incomplète
- Section TRACKING incomplète (heures/durées manquantes)
- Critères de succès non mesurables ou ambigus
- Documentation technique non synchronisée avec le code

**🟡 ALERTE MINEURE - Amélioration souhaitée :**

- Temps de validation pré-exécution > 15min (revoir l'efficacité de lecture)
- Taux de blocage entre 5% et 10% (optimiser la préparation des tâches)
- Durée de blocage > 2h sans tentative de déblocage
- Manque de références ADR explicites dans les tâches

## 🛠️ Commandes PowerShell Essentielles

### Vérifications Pré-Exécution

```powershell
# Vérifier les tâches en cours (DOIT retourner vide ou 1 fichier maximum)
Get-ChildItem .tasks\in-progress\*.task.md

# Lister les tâches bloquées par ordre numérique
Get-ChildItem .tasks\blocked\*.task.md | Sort-Object Name

# Lister les tâches à faire par ordre numérique (traiter dans cet ordre)
Get-ChildItem .tasks\to-do\*.task.md | Sort-Object Name

# Lister tous les ADR disponibles
Get-ChildItem docs\adr\*.adr.md | Sort-Object Name | Select-Object Name, LastWriteTime

# Vérifier l'unicité des IDs de tâches (détecter les doublons)
$ids = Get-ChildItem -Path .tasks -Filter *.task.md -Recurse | ForEach-Object { $_.Name -replace '^(\d+)--.*', '$1' }
$duplicates = $ids | Group-Object | Where-Object { $_.Count -gt 1 }
if ($duplicates) {
    Write-Warning "⚠️ IDs dupliqués détectés: $($duplicates.Name -join ', ')"
} else {
    Write-Host "✅ Tous les IDs de tâches sont uniques"
}
```

### Déplacement de Tâches

```powershell
# Démarrer une tâche (déplacer vers in-progress)
Move-Item .tasks\to-do\001-nom-tache.task.md .tasks\in-progress\

# Compléter une tâche (déplacer vers done)
Move-Item .tasks\in-progress\001-nom-tache.task.md .tasks\done\

# Bloquer une tâche (déplacer vers blocked)
Move-Item .tasks\in-progress\001-nom-tache.task.md .tasks\blocked\

# Reprendre une tâche bloquée (déplacer vers in-progress)
Move-Item .tasks\blocked\001-nom-tache.task.md .tasks\in-progress\
```

### Annotation Temporelle

```powershell
# Obtenir l'heure actuelle au format ISO 8601
$currentTime = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
Write-Host "Heure actuelle: $currentTime"

# Ajouter l'heure de début à une tâche (UTF-8 sans BOM)
$taskFile = ".tasks\in-progress\001-nom-tache.task.md"
$utf8NoBom = New-Object System.Text.UTF8Encoding $false
$content = [System.IO.File]::ReadAllText($taskFile, $utf8NoBom)
$content += "`n`n## TRACKING`nDébut: $currentTime`n"
[System.IO.File]::WriteAllLines($taskFile, $content, $utf8NoBom)

# Calculer la durée entre deux timestamps
$start = Get-Date "2025-11-27 14:30:15"
$end = Get-Date "2025-11-27 14:45:30"
$duration = $end - $start
Write-Host "Durée: $($duration.Minutes)min $($duration.Seconds)s"
```

### Validation et Qualité

```powershell
# Vérifier que le build passe
dotnet build --no-incremental
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build réussi"
} else {
    Write-Error "❌ Build échoué - Tâche ne peut pas être marquée Done"
    exit 1
}

# Exécuter tous les tests unitaires
dotnet test --no-build
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Tous les tests passent"
} else {
    Write-Error "❌ Tests en échec - Tâche ne peut pas être marquée Done"
    exit 1
}

# Vérifier la couverture de tests (si coverlet installé)
dotnet test --collect:"XPlat Code Coverage" --results-directory:"./TestResults"
# Analyser le fichier coverage.cobertura.xml généré
# Target: > 80% de couverture pour le nouveau code

# Lister les dépendances NuGet du projet
dotnet list package

# Vérifier les dépendances obsolètes ou vulnérables
dotnet list package --outdated
dotnet list package --vulnerable

# Compter le nombre de tests par projet
Get-ChildItem -Path . -Filter *.Unit.Tests.csproj -Recurse | ForEach-Object {
    $projectName = $_.Name
    $testCount = (Select-String -Path $_.FullName -Pattern "<PackageReference.*xunit" -AllMatches).Matches.Count
    Write-Host "$projectName : Tests détectés"
}

# Vérifier l'encodage UTF-8 sans BOM d'un fichier
$filePath = ".tasks\to-do\001-tache.task.md"
$bytes = Get-Content -Path $filePath -Encoding Byte -TotalCount 3
if ($bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
    Write-Warning "⚠️ Le fichier contient un BOM UTF-8 - Correction nécessaire"
} else {
    Write-Host "✅ Fichier UTF-8 sans BOM"
}
```

### Métriques et Reporting

```powershell
# Générer un rapport de statut complet
function Get-TasksReport {
    $todo = (Get-ChildItem .tasks\to-do\*.task.md -ErrorAction SilentlyContinue).Count
    $inProgress = (Get-ChildItem .tasks\in-progress\*.task.md -ErrorAction SilentlyContinue).Count
    $blocked = (Get-ChildItem .tasks\blocked\*.task.md -ErrorAction SilentlyContinue).Count
    $done = (Get-ChildItem .tasks\done\*.task.md -ErrorAction SilentlyContinue).Count
    $total = $todo + $inProgress + $blocked + $done
    
    if ($total -gt 0) {
        $completionRate = [math]::Round(($done / $total) * 100, 2)
        $blockageRate = [math]::Round(($blocked / $total) * 100, 2)
    } else {
        $completionRate = 0
        $blockageRate = 0
    }
    
    Write-Host "`n📊 RAPPORT WORKFLOW - $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
    Write-Host "Total tâches      : $total"
    Write-Host "✅ Done           : $done ($completionRate%)" -ForegroundColor Green
    Write-Host "🔴 Blocked        : $blocked ($blockageRate%)" -ForegroundColor Red
    Write-Host "🟢 To-Do          : $todo"
    Write-Host "🟡 In-Progress    : $inProgress"
    
    if ($inProgress -gt 1) {
        Write-Host "`n⚠️ ALERTE CRITIQUE: Plusieurs tâches en cours simultanément!" -ForegroundColor Red
    }
    if ($blockageRate -gt 10) {
        Write-Host "`n⚠️ ALERTE: Taux de blocage élevé (> 10%)" -ForegroundColor Yellow
    }
}

# Exécuter le rapport
Get-TasksReport
```

## 🎓 Exemples Complets et Commentés

### Exemple 1 : Workflow Complet Réussi (Sans Blocage)

```powershell
# ═══════════════════════════════════════════════════════════
# PHASE 1: VÉRIFICATIONS PRÉ-EXÉCUTION OBLIGATOIRES
# ═══════════════════════════════════════════════════════════

# 1.1. Vérifier qu'aucune tâche n'est en cours
Get-ChildItem .tasks\in-progress\*.task.md
# ✅ Résultat attendu: Vide (aucun fichier)

# 1.2. Vérifier les tâches bloquées
Get-ChildItem .tasks\blocked\*.task.md | Sort-Object Name
# ✅ Si vide: Continuer
# ⚠️ Si fichiers présents: Vérifier si déblocables AVANT de prendre nouvelle tâche

# 1.3. Lister les tâches à faire (ordre numérique strict)
Get-ChildItem .tasks\to-do\*.task.md | Sort-Object Name
# Résultat: 001-fix-service-config.task.md, 002-add-health-check.task.md

# ═══════════════════════════════════════════════════════════
# PHASE 2: LECTURE DES RÉFÉRENTIELS OBLIGATOIRES
# ═══════════════════════════════════════════════════════════

# 2.1. Lire intégralement copilot-instructions.md
Get-Content .github\copilot-instructions.md
# ✅ OBLIGATOIRE: Comprendre workflow, interdictions, piliers de qualité

# 2.2. Lister et identifier les ADR pertinents
Get-ChildItem docs\adr\*.adr.md | Sort-Object Name
# Résultat: 001-use-mudblazor.adr.md, 003-service-architecture.adr.md

# 2.3. Lire les ADR pertinents pour la tâche
Get-Content docs\adr\003-service-architecture.adr.md
# ✅ OBLIGATOIRE: Comprendre les décisions architecturales à respecter

# ═══════════════════════════════════════════════════════════
# PHASE 3: LECTURE ET VALIDATION DE LA TÂCHE
# ═══════════════════════════════════════════════════════════

# 3.1. Lire intégralement le fichier de tâche
Get-Content .tasks\to-do\001-fix-service-config.task.md
# ✅ Comprendre: QUOI, POURQUOI, DÉPENDANCES, CONTRAINTES, CRITÈRES

# 3.2. Vérifier la conformité avec copilot-instructions.md et ADR
# - Aucune règle violée ✅
# - ADR-003 respecté ✅
# - Toutes dépendances disponibles ✅

# 3.3. Documenter la validation pré-exécution dans le fichier
$taskFile = ".tasks\to-do\001-fix-service-config.task.md"
$validation = @"

## VALIDATION PRÉ-EXÉCUTION

### Fichiers de Référence Lus
- [x] ``.github/copilot-instructions.md`` - Lu intégralement
- [x] ``docs/adr/003-service-architecture.adr.md`` - Lu et compris

### Conformité Vérifiée
- [x] Aucune violation des règles du copilot-instructions.md détectée
- [x] Aucune violation des décisions ADR détectée
- [x] Toutes les dépendances requises sont disponibles
- [x] Tous les critères de succès sont mesurables et atteignables

### Décisions Architecturales Impactées
- ADR-003 : Configuration des services respecte l'architecture en couches

Date validation: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
Validé par: Agent Copilot
"@
$utf8NoBom = New-Object System.Text.UTF8Encoding $false
$content = [System.IO.File]::ReadAllText($taskFile, $utf8NoBom)
$content += $validation
[System.IO.File]::WriteAllLines($taskFile, $content, $utf8NoBom)

# ═══════════════════════════════════════════════════════════
# PHASE 4: DÉMARRAGE DE L'EXÉCUTION
# ═══════════════════════════════════════════════════════════

# 4.1. Annoter l'heure de début
$startTime = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$tracking = "`n## TRACKING`nDébut: $startTime`n"
$content = [System.IO.File]::ReadAllText($taskFile, $utf8NoBom)
$content += $tracking
[System.IO.File]::WriteAllLines($taskFile, $content, $utf8NoBom)

# 4.2. Déplacer vers in-progress
Move-Item $taskFile .tasks\in-progress\
$taskFile = ".tasks\in-progress\001-fix-service-config.task.md"

# 4.3. Exécuter le travail (modifications des fichiers)
# ... [Travail effectué ici] ...

# ═══════════════════════════════════════════════════════════
# PHASE 5: VALIDATION AVANT COMPLÉTION
# ═══════════════════════════════════════════════════════════

# 5.1. Vérifier que le build passe
dotnet build --no-incremental
if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Build échoué - Corriger avant de continuer"
    exit 1
}
Write-Host "✅ Build réussi"

# 5.2. Exécuter les tests unitaires
dotnet test --no-build
if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Tests échoués - Corriger avant de continuer"
    exit 1
}
Write-Host "✅ Tous les tests passent"

# 5.3. Vérifier la checklist de validation (manuellement)
# ✅ Tous les critères de la checklist validés

# ═══════════════════════════════════════════════════════════
# PHASE 6: COMPLÉTION DE LA TÂCHE
# ═══════════════════════════════════════════════════════════

# 6.1. Calculer la durée
$endTime = Get-Date
$start = Get-Date $startTime
$duration = $endTime - $start
$durationStr = "$($duration.Minutes)min $($duration.Seconds)s"

# 6.2. Annoter la fin
$completion = "Fin: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')`nDurée: $durationStr`n"
$content = [System.IO.File]::ReadAllText($taskFile, $utf8NoBom)
$content = $content -replace '(Début: [\d\-]+ [\d:]+)', "`$1`n$completion"
[System.IO.File]::WriteAllLines($taskFile, $content, $utf8NoBom)

# 6.3. Déplacer vers done
Move-Item $taskFile .tasks\done\

Write-Host "✅ Tâche 001 complétée avec succès en $durationStr" -ForegroundColor Green
```

### Exemple 2 : Workflow avec Blocage et Reprise

```powershell
# ═══════════════════════════════════════════════════════════
# PHASES 1-4: Identiques à l'exemple 1
# (Vérifications, Lecture, Validation, Démarrage)
# ═══════════════════════════════════════════════════════════

# [Supposons que le travail a démarré à 14:30:15]
# [Blocage détecté à 14:35:45]

# ═══════════════════════════════════════════════════════════
# PHASE 5: GESTION DU BLOCAGE
# ═══════════════════════════════════════════════════════════

$taskFile = ".tasks\in-progress\003-deploy-new-service.task.md"
$utf8NoBom = New-Object System.Text.UTF8Encoding $false

# 5.1. Calculer le temps actif avant blocage
$startTime = Get-Date "2025-11-27 14:30:15"
$blockTime = Get-Date "2025-11-27 14:35:45"
$activeTime = $blockTime - $startTime
$activeTimeStr = "$($activeTime.Minutes)min $($activeTime.Seconds)s"

# 5.2. Documenter le blocage avec argumentation COMPLÈTE
$blockageDoc = @"

## HISTORIQUE DES BLOCAGES

### BLOCAGE #1
Début blocage: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
Temps actif avant blocage: $activeTimeStr

#### Raison du blocage
La variable 'service_port' est requise pour le déploiement du nouveau service
mais n'est définie dans AUCUN fichier d'inventaire ou de configuration.

#### Pourquoi ce blocage empêche la progression
Sans le port de service défini, impossible de:
- Générer la configuration systemd (service ne peut pas bind sur un port)
- Configurer nginx en reverse proxy (pas de upstream port)
- Configurer le firewall (aucun port à autoriser)
Le déploiement est donc bloqué à 0% de complétion.

#### Action requise pour débloquer
L'utilisateur DOIT définir 'service_port' dans l'un des fichiers suivants:
- inventory/dev/group_vars/dotnet_servers.yml (recommandé)
- inventory/prod/group_vars/dotnet_servers.yml
- roles/dotnet-app/defaults/main.yml (si valeur par défaut acceptable)

Valeur suggérée: 8080 (ou tout port libre entre 8000-9000)

#### Tentatives effectuées
- Recherche dans inventory/dev/group_vars/ - variable NON trouvée
- Recherche dans inventory/prod/group_vars/ - variable NON trouvée  
- Recherche dans roles/dotnet-app/defaults/main.yml - variable NON définie
- Recherche dans roles/dotnet-app/vars/main.yml - variable NON définie
- Grep récursif dans tout le projet - AUCUNE occurrence trouvée
- Vérification dans les variables d'environnement - NON définie

#### Alternatives considérées
1. **Utiliser un port par défaut (8080)**
   - Avantages: Déblocage immédiat, valeur standard
   - Inconvénients: Risque de conflit avec d'autres services
   - Raison du rejet: Violation du principe "explicite > implicite" (ADR-005)

2. **Dériver le port du nom du service**
   - Avantages: Automatique, pas de configuration manuelle
   - Inconvénients: Non standard, imprévisible, difficile à débugger
   - Raison du rejet: Contraire aux bonnes pratiques infrastructure as code

3. **Demander le port au runtime**
   - Avantages: Flexibilité maximale
   - Inconvénients: Contraire au principe d'infrastructure as code
   - Raison du rejet: Violation de l'ADR-005 sur la reproductibilité
"@

$content = [System.IO.File]::ReadAllText($taskFile, $utf8NoBom)
$content += $blockageDoc
[System.IO.File]::WriteAllLines($taskFile, $content, $utf8NoBom)

# 5.3. Déplacer vers blocked
Move-Item $taskFile .tasks\blocked\

Write-Warning "🔴 Tâche 003 bloquée - Action utilisateur requise"

# ═══════════════════════════════════════════════════════════
# [L'utilisateur définit la variable service_port = 8080]
# [Déblocage possible - Reprise de la tâche]
# ═══════════════════════════════════════════════════════════

# PHASE 6: REPRISE APRÈS DÉBLOCAGE
# ═══════════════════════════════════════════════════════════

$taskFile = ".tasks\blocked\003-deploy-new-service.task.md"

# 6.1. Relire les référentiels (au cas où mis à jour)
Get-Content .github\copilot-instructions.md  # Vérifier si changements
Get-Content docs\adr\005-infrastructure-principles.adr.md  # Revérifier ADR

# 6.2. Vérifier que le blocage est résolu
$portDefined = (Select-String -Path "inventory/dev/group_vars/dotnet_servers.yml" -Pattern "service_port")
if (-not $portDefined) {
    Write-Error "❌ Variable 'service_port' toujours non définie - Blocage non résolu"
    exit 1
}
Write-Host "✅ Variable 'service_port' définie - Blocage résolu"

# 6.3. Documenter la reprise
$resumeTime = Get-Date
$blockStart = Get-Date "2025-11-27 14:35:45"
$blockDuration = $resumeTime - $blockStart
$blockDurationStr = "$([int]$blockDuration.TotalHours)h $($blockDuration.Minutes)min $($blockDuration.Seconds)s"

$resumeDoc = @"

### RÉSOLUTION BLOCAGE #1
Fin blocage: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
Durée blocage: $blockDurationStr
Résolution: Variable 'service_port' définie à 8080 dans inventory/dev/group_vars/dotnet_servers.yml
Reprise: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
"@

$content = [System.IO.File]::ReadAllText($taskFile, $utf8NoBom)
$content += $resumeDoc
[System.IO.File]::WriteAllLines($taskFile, $content, $utf8NoBom)

# 6.4. Déplacer vers in-progress
Move-Item $taskFile .tasks\in-progress\
$taskFile = ".tasks\in-progress\003-deploy-new-service.task.md"

# 6.5. Reprendre et compléter le travail
# ... [Travail complété] ...

# 6.6. Validation (build + tests)
dotnet build --no-incremental
dotnet test --no-build

# 6.7. Calculer les métriques finales
$finalEndTime = Get-Date
$totalDuration = $finalEndTime - $startTime
$activeTime2 = $finalEndTime - $resumeTime
$totalActiveTime = $activeTime + $activeTime2

$summary = @"

## RÉSUMÉ TEMPOREL
Début: 2025-11-27 14:30:15
Fin: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')

Temps actif total: $($totalActiveTime.Minutes)min $($totalActiveTime.Seconds)s
Temps de blocage total: $blockDurationStr
Durée totale: $([int]$totalDuration.TotalHours)h $($totalDuration.Minutes)min $($totalDuration.Seconds)s
Nombre de blocages: 1
"@

$content = [System.IO.File]::ReadAllText($taskFile, $utf8NoBom)
$content += $summary
[System.IO.File]::WriteAllLines($taskFile, $content, $utf8NoBom)

# 6.8. Archiver
Move-Item $taskFile .tasks\done\

Write-Host "✅ Tâche 003 complétée après résolution de blocage" -ForegroundColor Green
Write-Host "   Temps actif: $($totalActiveTime.Minutes)min | Temps bloqué: $blockDurationStr"
```
