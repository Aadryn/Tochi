# DIRECTIVES AGENT IA - WORKFLOW DÉVELOPPEMENT

## 🎯 PRIORITÉS DE GÉNÉRATION DE CODE

**DIRECTIVE FONDAMENTALE** : Lors de TOUTE génération de code, TOUJOURS privilégier ces aspects dans l'ordre de priorité suivant :

### 1. Modularité
- Découper le code en modules indépendants et cohésifs
- Respecter le principe de responsabilité unique (SRP)
- Favoriser les interfaces claires et le couplage faible
- Permettre le remplacement et l'évolution indépendante des modules

### 2. Composabilité
- Concevoir des composants qui peuvent être combinés pour créer des solutions complexes
- Privilégier la composition à l'héritage (composition over inheritance)
- Créer des interfaces cohérentes permettant l'assemblage fluide des composants
- Garantir que chaque composant peut fonctionner de manière autonome
- Favoriser les fonctions pures et les transformations de données

### 3. Rationalisation
- Justifier chaque choix technique par un besoin réel et mesurable
- Éviter les décisions basées sur des préférences personnelles
- Documenter explicitement les compromis (trade-offs)
- Éliminer tout code ou abstraction sans justification claire

### 4. Réutilisabilité
- Extraire les fonctionnalités communes en composants réutilisables
- Concevoir des abstractions génériques quand approprié
- Centraliser les utilitaires et configurations partagées
- Favoriser la composition plutôt que la duplication

### 5. Maintenabilité
- Écrire du code lisible et auto-documenté
- Utiliser des nommages explicites et cohérents
- Maintenir une documentation à jour
- Limiter la dette technique

### 6. Simplicité (KISS)
- Préférer la solution la plus simple qui fonctionne
- Éviter la sur-ingénierie et la complexité accidentelle
- Minimiser le nombre d'abstractions et de concepts
- Rendre le code compréhensible par un développeur junior

### 7. Sécurité
- Appliquer le principe du moindre privilège
- Valider et assainir toutes les entrées
- Protéger les données sensibles (chiffrement, masquage)
- Gérer les secrets de manière sécurisée (pas de hardcoding)
- Adresser les vulnérabilités OWASP connues

**APPLICATION OBLIGATOIRE** : À chaque génération de code, vérifier que ces 7 aspects sont respectés. En cas de conflit, privilégier l'aspect le plus haut dans la liste.

## 🎨 DESIGN SYSTEM - RESPECT ET AMÉLIORATION CONTINUE

**PRINCIPE FONDAMENTAL** : Le design system est un actif vivant qui DOIT être respecté ET amélioré continuellement.

### Respect du Design System Existant

**OBLIGATOIRE pour TOUT composant UI (MudBlazor, Vue/PrimeVue, CSS) :**

1. **VÉRIFIER** le design system avant toute création/modification UI
   - Palette de couleurs définie (`.github/instructions/mudblazor.design.principles.instructions.md`)
   - Espacement standardisé (`.github/instructions/mudblazor.spacing.instructions.md`)
   - Composants réutilisables existants (`.github/instructions/mudblazor.components.instructions.md`)
   - Variables CSS/SCSS (`.github/instructions/css.variables.instructions.md`)

2. **UTILISER** les éléments du design system existant
   - Composants MudBlazor : MudCard, MudButton, MudDataGrid, etc.
   - Palette : Primary=#0288d1, Background=#f5f5f5, Surface=#ffffff
   - Espacement : Spacing="3" (24px) par défaut
   - Élévation : Elevation="0" ou "1" maximum (design plat)
   - Typographie : h5/h6 pour titres, body1/body2 pour texte
   - Icônes : Material Design uniquement (`Icons.Material.*`)

3. **RESPECTER** les conventions de design
   - Design minimaliste clair/gris/blanc
   - Espaces blancs généreux
   - Bordures subtiles au lieu d'ombres fortes
   - Contraste suffisant pour accessibilité
   - Cohérence visuelle entre toutes les pages

### Amélioration et Amendement du Design System

**CAPITALISER SYSTÉMATIQUEMENT** toute amélioration du design system :

1. **IDENTIFIER** les opportunités d'amélioration
   - Nouveau pattern UI récurrent détecté
   - Incohérence visuelle entre composants
   - Besoin de standardisation manquant
   - Composant réutilisable non documenté
   - Variable/mixin CSS/SCSS manquant

2. **DOCUMENTER** immédiatement dans les fichiers d'instructions appropriés
   - **Nouveau composant MudBlazor** → Amender `.github/instructions/mudblazor.components.instructions.md`
   - **Nouvelle convention de design** → Amender `.github/instructions/mudblazor.design.principles.instructions.md`
   - **Nouveau pattern d'espacement** → Amender `.github/instructions/mudblazor.spacing.instructions.md`
   - **Nouvelle variable CSS** → Amender `.github/instructions/css.variables.instructions.md`
   - **Nouveau mixin SCSS** → Amender `.github/instructions/scss.mixins.instructions.md`
   - **Nouveau composant Vue** → Amender `.github/instructions/vue3.components.instructions.md`

3. **CRÉER** des exemples concrets et réutilisables
   - Code complet du composant avec tous les paramètres
   - Cas d'usage typiques (✅ BON)
   - Anti-patterns à éviter (❌ MAUVAIS)
   - Screenshots ou descriptions visuelles si pertinent

4. **PROPAGER** les changements dans le code existant
   - Identifier les composants impactés par la nouvelle règle
   - Proposer un plan de migration si changement majeur
   - Créer une tâche de refactoring si nécessaire

### Workflow Design System

**AVANT de créer un composant UI :**

1. ✅ **CHERCHER** si un composant similaire existe déjà
2. ✅ **VÉRIFIER** la palette, l'espacement, la typographie du design system
3. ✅ **RÉUTILISER** ou composer les composants existants
4. ✅ Si nouveau composant nécessaire → DOCUMENTER dans les instructions

**PENDANT le développement UI :**

1. ✅ **APPLIQUER** strictement les règles du design system
2. ✅ **VALIDER** visuellement avec Chrome DevTools (cohérence, espacement, couleurs)
3. ✅ **TESTER** le responsive (mobile, tablette, desktop)
4. ✅ **VÉRIFIER** l'accessibilité (contraste, navigation clavier)

**APRÈS la création d'un composant :**

1. ✅ **EXTRAIRE** les patterns réutilisables en composants
2. ✅ **DOCUMENTER** dans `.github/instructions/*.instructions.md`
3. ✅ **CRÉER** des exemples d'utilisation
4. ✅ **CAPITALISER** les leçons apprises (ADR si décision architecturale)

### Validation Design System

**CHECKLIST OBLIGATOIRE pour tout composant UI :**

- [ ] Palette de couleurs respectée (Primary=#0288d1, Background=#f5f5f5)
- [ ] Espacement cohérent (Spacing="3" par défaut)
- [ ] Élévation minimale (Elevation="0" ou "1")
- [ ] Typographie appropriée (h5/h6, body1/body2)
- [ ] Icônes Material Design uniquement
- [ ] Contraste suffisant (accessibilité WCAG AA minimum)
- [ ] Responsive testé (mobile, tablette, desktop)
- [ ] Cohérence visuelle avec le reste de l'application
- [ ] Pas de code CSS/SCSS custom sans justification
- [ ] Variables/mixins utilisés au lieu de valeurs en dur
- [ ] Composant documenté si réutilisable
- [ ] Design system amendé si nouveau pattern identifié

## 🎯 CONTEXTE TECHNIQUE

**Environnement disponible :**
- **Shell** : PowerShell (Windows)
- **Conteneurisation** : Docker Desktop
- **Base de données** : PostgreSQL (conteneur `environments-postgres`, port `15432`)
  - Database: `development`
  - User: `postgres`
  - Password: `password`
  - Table principale: `statistics.metric_deltas`
- **Automatisation** : VS Code Tasks (obligatoire pour builds/tests répétitifs)
- **Validation UI** : Chrome DevTools MCP (pour tests fonctionnels)

**RÈGLE CRITIQUE** : Le schéma de base de données est STABLE. Aucune modification de schéma sans validation utilisateur préalable.

## ⏱️ TEMPS ET PERSÉVÉRANCE

**PRINCIPE FONDAMENTAL** : L'agent dispose de **TOUT LE TEMPS NÉCESSAIRE** pour réaliser les tâches avec excellence.

**RÈGLES IMPÉRATIVES :**
1. **Aucune limitation temporelle** : Prendre le temps qu'il faut pour bien faire
2. **Persévérance absolue** : Ne JAMAIS rechigner à la tâche, quelle que soit sa complexité
3. **Subdivision encouragée** : Si une tâche est trop importante, la subdiviser en sous-tâches logiques
4. **Effort continu** : Persévérer systématiquement jusqu'à complétion totale
5. **Qualité > Vitesse** : La qualité et la complétude priment sur la rapidité
6. **OBLIGATION MORALE DE CAPITALISATION** : Chaque leçon apprise DOIT être documentée formellement pour bénéficier au projet à long terme

### Subdivision de Tâches Complexes

**QUAND subdiviser :**
- Tâche estimée à plus de 4 heures de travail
- Tâche avec multiples aspects techniques distincts
- Tâche nécessitant plusieurs cycles de validation
- Tâche avec dépendances séquentielles clairement identifiables

**COMMENT subdiviser :**

1. **ANALYSER** la tâche principale et identifier les sous-objectifs logiques

2. **CRÉER** les sous-tâches dans `.tasks/to-do/` :
   ```powershell
   # Exemple : Tâche 005 subdivisée en 005.1, 005.2, 005.3
   $utf8NoBom = New-Object System.Text.UTF8Encoding $false
   
   # Sous-tâche 1
   $content1 = @"
# Sous-tâche 005.1 - [Titre court]

**Tâche parent :** 005--titre-tache-principale.task.md

## OBJECTIF
[Description sous-objectif 1]

## CRITÈRES DE SUCCÈS
- [ ] Critère 1
- [ ] Critère 2

## DÉPENDANCES
- Aucune (ou liste des dépendances)
"@
   [System.IO.File]::WriteAllLines("$PWD\.tasks\to-do\005.1--sous-titre.task.md", $content1, $utf8NoBom)
   
   # Répéter pour chaque sous-tâche
   ```

3. **DOCUMENTER** la subdivision dans la tâche parent :
   ```markdown
   ## SUBDIVISION
   
   Cette tâche a été subdivisée en sous-tâches logiques :
   - [ ] 005.1 - Sous-titre 1
   - [ ] 005.2 - Sous-titre 2
   - [ ] 005.3 - Sous-titre 3
   
   **Raison de la subdivision :** [Explication détaillée]
   ```

4. **DÉPLACER** la tâche parent vers `.tasks/blocked/` avec statut "En subdivision"

5. **TRAITER** les sous-tâches dans l'ordre numérique

6. **COMPLÉTER** la tâche parent une fois toutes les sous-tâches terminées

### Gestion Proactive de l'Incertitude

**PRINCIPE** : Ne JAMAIS rester bloqué par manque d'information. Toujours demander de l'aide de manière structurée.

**SI informations manquantes ou ambiguës :**

1. **IDENTIFIER** précisément ce qui manque ou est ambigu

2. **PROPOSER** plusieurs solutions avec analyse détaillée :
   ```markdown
   ## DEMANDE DE CLARIFICATION
   
   **Contexte :** [Où en est la tâche]
   
   **Problème identifié :** [Ce qui manque/est ambigu]
   
   **Impact :** [Pourquoi c'est bloquant]
   
   **Solutions proposées :**
   
   ### Solution 1 : [Titre]
   **Description :** [Explication détaillée]
   **Avantages :**
   - [Avantage 1]
   - [Avantage 2]
   **Inconvénients :**
   - [Inconvénient 1]
   - [Inconvénient 2]
   **Effort estimé :** [Temps/complexité]
   **Recommandation :** [Oui/Non + pourquoi]
   
   ### Solution 2 : [Titre]
   [Même structure...]
   
   ### Solution 3 : [Titre]
   [Même structure...]
   
   **Ma recommandation :** Solution X car [argumentation factuelle]
   
   **Question :** Quelle solution préférez-vous ou avez-vous une autre approche ?
   ```

3. **DOCUMENTER** dans le fichier `.task.md` en section `## EN ATTENTE DE RÉPONSE`

4. **ATTENDRE** la réponse utilisateur avant de continuer

5. **INTÉGRER** la décision dans un ADR si applicable

**INTERDICTION ABSOLUE** : Ne JAMAIS dire "je ne peux pas faire" ou "c'est trop complexe". Toujours proposer des alternatives ou une subdivision.

## 🚫 INTERDICTIONS ABSOLUES

**JAMAIS :**
1. Livrer du code non testé, incomplet ou avec erreurs de build
2. Livrer du code avec des warnings de build (zéro warning obligatoire)
3. Livrer du code dont les tests unitaires échouent (100% de réussite obligatoire)
4. Commencer une tâche si `.tasks/in-progress/` contient un fichier
4. Traiter des tâches dans le désordre (ordre numérique strict : 001, 002, 003...)
5. Ignorer `.tasks/blocked/` avant de démarrer une nouvelle tâche de `.tasks/to-do/`
6. Coder sans avoir lu INTÉGRALEMENT le fichier de tâche
7. Violer les ADR (`docs/adr/*.adr.md`) sans justification documentée
8. Laisser des fichiers temporaires (`.tmp`, `.swp`, `~`)
9. Traiter des tâches CI/CD (GitHub Actions, Azure DevOps, GitLab CI, Jenkins, etc.)
10. Créer des fichiers `.md` hors de `.github/`, `.tasks/`, `docs/` (sauf `README.md`, `CHANGELOG.md` à la racine)
11. Coder sans créer de feature branch Git (format : `feature/{id}--{nom}`)
12. Faire des gros commits (commits atomiques obligatoires)
13. Commiter du code qui ne compile pas ou avec tests en échec
14. Merger avec fast-forward (`--no-ff` obligatoire)
15. Abandonner une tâche sans avoir exploré toutes les alternatives (subdivision, clarification, etc.)
16. Rester bloqué sans demander de clarification avec solutions proposées
17. **LAISSER UNE LEÇON APPRISE NON DOCUMENTÉE** : Toute connaissance acquise DOIT être capitalisée (ADR, Requirements ou `.github/instructions/*.instructions.md`)

## ✅ OBLIGATIONS SYSTÉMATIQUES

### Avant TOUTE Intervention

**EXÉCUTER cette séquence de vérification OBLIGATOIRE :**

```powershell
# 1. VÉRIFIER in-progress (doit être vide pour démarrer nouvelle tâche)
$inProgress = @(Get-ChildItem .tasks\in-progress\*.task.md -ErrorAction SilentlyContinue)
if ($inProgress.Count -gt 0) {
    Write-Host "❌ TÂCHE EN COURS: $($inProgress[0].Name)" -ForegroundColor Red
    exit 1
}

# 2. VÉRIFIER blocked (analyser si débloquage possible)
$blocked = @(Get-ChildItem .tasks\blocked\*.task.md -ErrorAction SilentlyContinue)
if ($blocked.Count -gt 0) {
    Write-Host "⚠️ $($blocked.Count) TÂCHE(S) BLOQUÉE(S) - ANALYSER D'ABORD" -ForegroundColor Yellow
    $blocked | Sort-Object Name | ForEach-Object { Write-Host "  - $($_.Name)" }
}

# 3. LISTER to-do par ordre numérique
$todo = @(Get-ChildItem .tasks\to-do\*.task.md -ErrorAction SilentlyContinue)
if ($todo.Count -gt 0) {
    $next = $todo | Sort-Object Name | Select-Object -First 1
    Write-Host "✅ PROCHAINE TÂCHE: $($next.Name)" -ForegroundColor Green
}
```

**ACTION selon résultat :**
- Si **in-progress non vide** → Compléter la tâche en cours
- Si **blocked contient tâche débloquable** → Traiter la tâche bloquée en priorité
- Si **blocked non débloquable** → Traiter la prochaine tâche de `to-do/` (par ordre numérique)

### Démarrage d'une Tâche

**SÉQUENCE OBLIGATOIRE :**

1. **LIRE** intégralement le fichier `.task.md` :
   - Identifier QUOI (objectifs)
   - Comprendre POURQUOI (justification)
   - Analyser dépendances et contraintes
   - Repérer critères de succès

2. **VÉRIFIER** conformité avec `docs/adr/*.adr.md` et `docs/requirements/**/*.requirements.md` :
   - **ADR (Architecture Decision Records)** : Lister ADR applicables, valider compatibilité de l'approche
   - **Requirements (Spécifications Fonctionnelles)** : Vérifier que la tâche respecte les exigences métier documentées
   - Si nouvelle décision architecturale → Créer ADR (format `{id}-{titre}.adr.md`)
   - Si nouveau besoin métier identifié → Créer ou mettre à jour Requirements (format `{id}-{titre}.requirements.md`)

3. **ANNOTER** heure de début (UTC, format ISO 8601) :
   ```powershell
   $utf8NoBom = New-Object System.Text.UTF8Encoding $false
   $start = (Get-Date).ToUniversalTime().ToString("o")
   $content = [System.IO.File]::ReadAllText("$PWD\.tasks\to-do\{tache}.task.md", $utf8NoBom)
   $content += "`n`n## TRACKING`nDébut: $start`n"
   [System.IO.File]::WriteAllLines("$PWD\.tasks\to-do\{tache}.task.md", $content, $utf8NoBom)
   ```

4. **DÉPLACER** vers `in-progress/` :
   ```powershell
   Move-Item .tasks\to-do\{tache}.task.md .tasks\in-progress\
   ```

5. **CRÉER** feature branch Git :
   ```powershell
   # Format: feature/{task-id}--{task-name}
   $taskId = "001"
   $taskName = "fix-authentication"
   $branchName = "feature/$taskId--$taskName"
   
   git checkout -b $branchName
   git push -u origin $branchName
   
   Write-Host "✅ BRANCH CRÉÉE: $branchName" -ForegroundColor Green
   ```

### Pendant l'Exécution

**RÈGLES D'EXÉCUTION :**

1. **APPLIQUER** à la lettre les directives de la tâche
2. **UTILISER** VS Code Tasks existantes (si absentes, créer avec `create_and_run_task`)
3. **COMMITER** atomiquement après chaque modification logique (voir section Git)
4. **VALIDER** chaque étape majeure avant de continuer
5. **BUILD** obligatoire après modification : `run_task` → `dotnet: build` (exitCode = 0, zéro warning)
6. **TESTS** obligatoires après modification : `run_task` → `dotnet: test` (exitCode = 0, 100% réussite)
7. **VALIDER FONCTIONNELLEMENT** avec Chrome DevTools après toute modification UI/API
8. **DOCUMENTER** décisions techniques et besoins métier :
   - **Décisions techniques/architecturales** : Créer ADR dans `docs/adr/`
   - **Besoins métier/fonctionnels** : Créer ou mettre à jour Requirements dans `docs/requirements/{domaine}/{sous-domaine}/`
   - **Vérifier cohérence** : Requirements ↔ ADR (fonctionnel ↔ technique)
9. **CAPITALISER L'APPRENTISSAGE** (IMPÉRATIF) :
   - **À CHAQUE fois qu'une information utile est découverte** (bonne pratique, pattern, contrainte, règle métier, comportement technique)
   - **CRÉER IMMÉDIATEMENT** un ADR (technique) OU Requirements (métier) pour spécialiser et pérenniser cette connaissance
   - **AMENDER IMMÉDIATEMENT** le design system si nouveau pattern UI identifié
   - **Ne JAMAIS laisser de connaissance implicite** : Tout apprentissage DOIT être documenté formellement
   - **Exemples de situations déclenchant documentation** :
     * Pattern technique efficace découvert (→ ADR)
     * Règle métier identifiée lors du développement (→ Requirements)
     * Contrainte système/librairie découverte (→ ADR)
     * **Pattern UI réutilisable créé** (→ Amender `.github/instructions/mudblazor.*.instructions.md` ou `.github/instructions/vue3.*.instructions.md`)
     * **Nouvelle convention de design** (→ Amender `.github/instructions/mudblazor.design.principles.instructions.md`)
     * **Nouveau composant CSS/SCSS réutilisable** (→ Amender `.github/instructions/css.*.instructions.md` ou `.github/instructions/scss.*.instructions.md`)
     * Comportement attendu clarifié par utilisateur (→ Requirements si métier, ADR si technique)
     * Solution à un problème récurrent (→ ADR)
     * Validation d'hypothèse métier (→ Requirements)

**CRITÈRES DE VALIDATION BUILD/TESTS :**
- Build : `exitCode = 0` ET `0 Warning(s)` (zéro warning accepté)
- Tests : `exitCode = 0` ET `Test Run Successful` (100% passed, 0 failed, 0 skipped)
- Si échec ou warning détecté → CORRIGER IMMÉDIATEMENT avant de continuer

**VALIDATION FONCTIONNELLE OBLIGATOIRE :**

Après TOUTE modification impactant l'interface utilisateur ou les APIs :

1. **DÉMARRER** l'application concernée :
   ```powershell
   # WebApp Management
   run_task → "dotnet: watch WebApp Management"
   
   # WebApp Default
   run_task → "dotnet: watch WebApp Default"
   
   # WebApi Default
   run_task → "dotnet: watch WebApi Default"
   ```

2. **TESTER** méticuleusement avec Chrome DevTools :
   - Naviguer vers les pages/fonctionnalités modifiées
   - Vérifier le comportement attendu (interactions, affichage, données)
   - Contrôler la console (aucune erreur JavaScript)
   - Vérifier les appels réseau (Network tab - statuts HTTP 2xx)
   - Tester les cas limites et erreurs

3. **VALIDER** critères fonctionnels :
   - Fonctionnalité utilisable de bout en bout
   - Aucune régression sur fonctionnalités existantes
   - **UI STRICTEMENT COHÉRENTE avec le design system** (palette, espacement, typographie)
   - **Design system respecté à 100%** (vérifier `.github/instructions/mudblazor.*.instructions.md`)
   - Messages d'erreur explicites si applicable
   - Responsive fonctionnel (mobile, tablette, desktop)
   - Accessibilité validée (contraste, navigation clavier)

4. **DOCUMENTER** résultats validation :
   ```powershell
   # Ajouter dans le fichier .task.md
   $utf8NoBom = New-Object System.Text.UTF8Encoding $false
   $validation = @"
   
## VALIDATION FONCTIONNELLE

**Application testée :** [WebApp Management/Default ou WebApi]
**URL :** [URL testée]
**Scénarios validés :**
- [Scénario 1] ✅
- [Scénario 2] ✅
**Console DevTools :** Aucune erreur
**Network :** Tous appels HTTP réussis
**Timestamp :** $(Get-Date -Format 'o')
"@
   $taskFile = Get-Item .tasks\in-progress\*.task.md
   $content = [System.IO.File]::ReadAllText($taskFile.FullName, $utf8NoBom)
   $content += $validation
   [System.IO.File]::WriteAllLines($taskFile.FullName, $content, $utf8NoBom)
   ```

**SI ERREUR FONCTIONNELLE DÉTECTÉE :**
- Ne PAS continuer
- Corriger immédiatement
- Rebuilder + Retester
- Documenter le problème et la correction dans le commit

**EN CAS DE BLOCAGE :**

1. **IDENTIFIER** cause précise (dépendance, ambiguïté, conflit ADR, erreur technique, décision utilisateur, hors périmètre)
2. **ARGUMENTER** dans le fichier `.task.md` :
   - Description détaillée du problème
   - Explication POURQUOI non complétable
   - Actions concrètes pour débloquer
   - Liste tentatives effectuées
   - Alternatives considérées et rejetées
3. **DÉPLACER** vers `blocked/` :
   ```powershell
   Move-Item .tasks\in-progress\{tache}.task.md .tasks\blocked\
   ```
4. **NOTIFIER** utilisateur avec message clair et actionnable

**SI BLOQUÉ, POSER DES QUESTIONS :**
- Poser UNE question à la fois (claire, précise, liée à la tâche)
- Analyser TOUTES les réponses avant de coder
- Si réponses ajoutent contraintes/changent périmètre → Documenter (ADR ou spécifications `docs/`)

### Complétion d'une Tâche

**CHECKLIST DE VALIDATION OBLIGATOIRE :**

- [ ] Tous fichiers requis créés/modifiés
- [ ] **Build réussi sans aucun warning**
  - [ ] `dotnet: build` exitCode = 0
  - [ ] `0 Error(s)` dans la sortie
  - [ ] `0 Warning(s)` dans la sortie (AUCUN warning accepté)
- [ ] **Tests unitaires 100% réussis**
  - [ ] `dotnet: test` exitCode = 0
  - [ ] `Test Run Successful` dans la sortie
  - [ ] `Passed: X` (tous les tests passés)
  - [ ] `Failed: 0` (aucun test en échec)
  - [ ] `Skipped: 0` (aucun test ignoré)
- [ ] **Application testée fonctionnellement avec Chrome DevTools (OBLIGATOIRE)**
  - [ ] Application démarrée et accessible
  - [ ] Fonctionnalités modifiées testées méticuleusement
  - [ ] Console DevTools sans erreurs
  - [ ] Network tab : appels HTTP réussis
  - [ ] Aucune régression détectée
  - [ ] Validation documentée dans `.task.md`
- [ ] **Documentation validée selon les règles du projet**
  - [ ] Fichiers C# : Commentaires XML présents sur tous les membres publics (`.github/instructions/csharp.documentation.instructions.md`)
  - [ ] Documentation en français uniquement (aucun texte anglais)
  - [ ] Fichiers Razor : Conformité avec `.github/instructions/blazor.*.instructions.md` et `.github/instructions/mudblazor.*.instructions.md`
  - [ ] **ADR créés/mis à jour** si décisions architecturales (voir `.github/instructions/adr.documentation.instructions.md`)
  - [ ] **Requirements créés/mis à jour** si nouveaux besoins métier (voir `.github/instructions/requirements.documentation.instructions.md`)
- [ ] Code conforme standards projet (formatage, conventions)
- [ ] **Aucun conflit avec ADR existants** (décisions techniques)
- [ ] **Aucun conflit avec Requirements existants** (exigences métier)
- [ ] Critères de succès de la tâche satisfaits
- [ ] Aucun fichier temporaire laissé

**SI TOUS CRITÈRES VALIDÉS :**

1. **ANNOTER** heure de fin et calculer durée :
   ```powershell
   $utf8NoBom = New-Object System.Text.UTF8Encoding $false
   $end = (Get-Date).ToUniversalTime().ToString("o")
   $content = [System.IO.File]::ReadAllText("$PWD\.tasks\in-progress\{tache}.task.md", $utf8NoBom)
   
   # Extraire heure début pour calcul durée
   if ($content -match "Début:\s*(.+)") {
       $start = [DateTime]::Parse($Matches[1])
       $duration = ([DateTime]::Parse($end) - $start).ToString("hh\:mm\:ss")
       $content += "Fin: $end`nDurée: $duration`n"
   }
   [System.IO.File]::WriteAllLines("$PWD\.tasks\in-progress\{tache}.task.md", $content, $utf8NoBom)
   ```

2. **DÉPLACER** vers `done/` :
   ```powershell
   Move-Item .tasks\in-progress\{tache}.task.md .tasks\done\
   ```

3. **MERGER** la feature branch :
   ```powershell
   # Merger dans main/develop
   git checkout main
   git merge --no-ff feature/001--fix-authentication -m "Merge feature/001: Fix authentication"
   git push origin main
   
   # Supprimer la feature branch (locale et distante)
   git branch -d feature/001--fix-authentication
   git push origin --delete feature/001--fix-authentication
   ```

4. **RETOURNER** à la vérification initiale (étape 1)

## 🌿 GESTION GIT - FEATURE BRANCHES & COMMITS ATOMIQUES

### Règles Fondamentales

**OBLIGATOIRE** : Chaque tâche = 1 feature branch + commits atomiques.

**Principes :**
1. **Une feature branch par tâche** : Isolation complète des modifications
2. **Commits atomiques** : Une modification logique = un commit
3. **Messages conventionnels** : Format standardisé pour traçabilité
4. **Push régulier** : Synchronisation après chaque commit
5. **Merge sans fast-forward** : Historique clair avec `--no-ff`

### Workflow Git Complet

**1. DÉMARRAGE - Créer feature branch :**
```powershell
# À exécuter APRÈS déplacement vers .tasks/in-progress/
$taskFile = Get-Item .tasks\in-progress\*.task.md
$taskId = $taskFile.Name -replace '^(\d+)--.*', '$1'
$taskName = $taskFile.Name -replace '^\d+--(.+)\.task\.md$', '$1'
$branchName = "feature/$taskId--$taskName"

# Vérifier qu'on est sur main/develop
$currentBranch = git rev-parse --abbrev-ref HEAD
if ($currentBranch -ne "main" -and $currentBranch -ne "develop") {
    Write-Host "⚠️ ATTENTION: Pas sur main/develop (actuellement: $currentBranch)" -ForegroundColor Yellow
    git checkout main
}

# Créer et pusher la feature branch
git checkout -b $branchName
git push -u origin $branchName

Write-Host "✅ BRANCH CRÉÉE: $branchName" -ForegroundColor Green
Write-Host "   Branche locale: $branchName" -ForegroundColor Gray
Write-Host "   Branche distante: origin/$branchName" -ForegroundColor Gray
```

**2. DÉVELOPPEMENT - Commits atomiques :**

**RÈGLE** : Commiter après CHAQUE modification logique complète.

**Exemples de modifications atomiques :**
- Ajout d'une méthode + son test unitaire
- Modification d'une classe existante
- Création d'un fichier de configuration
- Mise à jour d'un ADR
- Refactoring d'un composant

**Format de message de commit (Conventional Commits) :**
```
<type>(<scope>): <description>

[body optionnel]

[footer optionnel]
```

**Types de commit :**
- `feat`: Nouvelle fonctionnalité
- `fix`: Correction de bug
- `refactor`: Refactoring (sans changement fonctionnel)
- `test`: Ajout ou modification de tests
- `docs`: Documentation uniquement
- `style`: Formatage, point-virgules, etc. (sans changement de code)
- `perf`: Amélioration de performance
- `chore`: Tâches de maintenance (build, config, etc.)

**Script de commit atomique :**
```powershell
function Invoke-AtomicCommit {
    param(
        [Parameter(Mandatory=$true)]
        [string]$Type,
        
        [Parameter(Mandatory=$true)]
        [string]$Scope,
        
        [Parameter(Mandatory=$true)]
        [string]$Description,
        
        [string]$Body = ""
    )
    
    # Vérifier qu'on est bien sur une feature branch
    $currentBranch = git rev-parse --abbrev-ref HEAD
    if ($currentBranch -notmatch "^feature/") {
        Write-Host "❌ ERREUR: Pas sur une feature branch (actuellement: $currentBranch)" -ForegroundColor Red
        return
    }
    
    # VALIDATION DOCUMENTATION OBLIGATOIRE
    Write-Host "`n🔍 VALIDATION DOCUMENTATION..." -ForegroundColor Cyan
    
    # 1. Vérifier fichiers C# modifiés
    $modifiedCsFiles = git diff --cached --name-only --diff-filter=AM | Where-Object { $_ -match '\.cs$' }
    
    if ($modifiedCsFiles) {
        Write-Host "Fichiers C# à valider: $($modifiedCsFiles.Count)" -ForegroundColor Yellow
        
        foreach ($file in $modifiedCsFiles) {
            if (Test-Path $file) {
                $content = Get-Content $file -Raw
                
                # Vérifier présence commentaires XML sur classes/méthodes/propriétés publiques
                $publicMembers = [regex]::Matches($content, 'public\s+(class|interface|enum|struct|record|delegate)\s+\w+')
                $publicMembers += [regex]::Matches($content, 'public\s+\w+\s+\w+\s*\(')  # Méthodes
                $publicMembers += [regex]::Matches($content, 'public\s+\w+\s+\w+\s*\{')  # Propriétés
                
                $xmlComments = [regex]::Matches($content, '///\s*<summary>')
                
                if ($publicMembers.Count -gt 0 -and $xmlComments.Count -eq 0) {
                    Write-Host "❌ ERREUR: $file - Membres publics sans documentation XML" -ForegroundColor Red
                    Write-Host "   Règles: .github/instructions/csharp.documentation.instructions.md" -ForegroundColor Gray
                    return
                }
                
                # Vérifier langue française dans les commentaires
                $englishPattern = '\b(the|calculate|get|set|return|create|update|delete|process)\b'
                $englishInComments = [regex]::Matches($content, "///.*($englishPattern)")
                
                if ($englishInComments.Count -gt 0) {
                    Write-Host "⚠️  ATTENTION: $file - Documentation potentiellement en anglais" -ForegroundColor Yellow
                    Write-Host "   Règle: Documentation DOIT être en français" -ForegroundColor Gray
                }
            }
        }
        
        Write-Host "✅ Documentation validée" -ForegroundColor Green
    }
    
    # 2. Vérifier fichiers Razor modifiés (si applicable)
    $modifiedRazorFiles = git diff --cached --name-only --diff-filter=AM | Where-Object { $_ -match '\.razor$' }
    
    if ($modifiedRazorFiles) {
        Write-Host "Fichiers Razor détectés: $($modifiedRazorFiles.Count)" -ForegroundColor Yellow
        Write-Host "   Vérifier conformité: .github/instructions/blazor.*.instructions.md" -ForegroundColor Gray
        Write-Host "   Vérifier conformité: .github/instructions/mudblazor.*.instructions.md" -ForegroundColor Gray
    }
    
    # Construire le message de commit
    $message = "$Type($Scope): $Description"
    if ($Body) {
        $message += "`n`n$Body"
    }
    
    # Commit
    git add -A
    git commit -m $message
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ COMMIT: $message" -ForegroundColor Green
        
        # Push automatique
        git push origin $currentBranch
        Write-Host "✅ PUSHED: origin/$currentBranch" -ForegroundColor Green
    } else {
        Write-Host "❌ COMMIT FAILED" -ForegroundColor Red
    }
}

# Exemples d'utilisation
Invoke-AtomicCommit -Type "feat" -Scope "auth" -Description "Add JWT token validation"
Invoke-AtomicCommit -Type "test" -Scope "auth" -Description "Add unit tests for JWT validation"
Invoke-AtomicCommit -Type "fix" -Scope "login" -Description "Fix null reference in LoginService"
Invoke-AtomicCommit -Type "refactor" -Scope "user" -Description "Extract user validation to separate method"
Invoke-AtomicCommit -Type "docs" -Scope "adr" -Description "Add ADR-005 for authentication strategy"
```

**Séquence de développement typique :**
```powershell
# 1. Créer fichier + tests
Invoke-AtomicCommit -Type "feat" -Scope "domain" -Description "Add User entity class"
Invoke-AtomicCommit -Type "test" -Scope "domain" -Description "Add User entity unit tests"

# 2. Build + Tests
dotnet build --no-restore
dotnet test --no-build

# 3. Ajouter service
Invoke-AtomicCommit -Type "feat" -Scope "services" -Description "Add UserService with CRUD operations"
Invoke-AtomicCommit -Type "test" -Scope "services" -Description "Add UserService unit tests"

# 4. Build + Tests
dotnet build --no-restore
dotnet test --no-build

# 5. Refactoring si nécessaire
Invoke-AtomicCommit -Type "refactor" -Scope "services" -Description "Extract validation logic to separate validator"

# 6. Documentation
Invoke-AtomicCommit -Type "docs" -Scope "readme" -Description "Update README with UserService usage"
```

**3. VALIDATION - Avant merge :**
```powershell
# 1. Vérifier l'état de la branche
git status

# 2. Vérifier l'historique des commits
git log --oneline --graph -10

# 3. Build complet
dotnet build --no-restore

# 4. Tests complets
dotnet test --no-build

# 5. Vérifier qu'on est à jour avec main
git fetch origin main
$behind = git rev-list --count HEAD..origin/main
if ($behind -gt 0) {
    Write-Host "⚠️ La branche main a avancé ($behind commits)" -ForegroundColor Yellow
    Write-Host "   Rebase recommandé avant merge" -ForegroundColor Yellow
    
    # Optionnel: Rebase interactif pour nettoyer l'historique
    git rebase -i origin/main
}

Write-Host "✅ BRANCHE PRÊTE POUR MERGE" -ForegroundColor Green
```

**4. COMPLÉTION - Merge et cleanup :**
```powershell
# Script de merge complet
$taskFile = Get-Item .tasks\in-progress\*.task.md
$taskId = $taskFile.Name -replace '^(\d+)--.*', '$1'
$taskName = $taskFile.Name -replace '^\d+--(.+)\.task\.md$', '$1'
$featureBranch = "feature/$taskId--$taskName"

# 1. Vérifier qu'on est sur la feature branch
$currentBranch = git rev-parse --abbrev-ref HEAD
if ($currentBranch -ne $featureBranch) {
    Write-Host "❌ ERREUR: Pas sur la bonne feature branch" -ForegroundColor Red
    Write-Host "   Attendu: $featureBranch" -ForegroundColor Gray
    Write-Host "   Actuel: $currentBranch" -ForegroundColor Gray
    exit 1
}

# 2. Dernier push
git push origin $featureBranch

# 3. Merger dans main (ou develop)
git checkout main
git pull origin main
git merge --no-ff $featureBranch -m "Merge $featureBranch - $taskName"

# 4. Push main
git push origin main

# 5. Supprimer la feature branch
git branch -d $featureBranch
git push origin --delete $featureBranch

Write-Host "`n✅ MERGE COMPLÉTÉ" -ForegroundColor Green
Write-Host "   Feature: $featureBranch → main" -ForegroundColor Gray
Write-Host "   Branche supprimée (locale + distante)" -ForegroundColor Gray
```

### Bonnes Pratiques Git

**✅ À FAIRE :**
- Créer feature branch IMMÉDIATEMENT après déplacement vers `in-progress/`
- Commiter APRÈS chaque modification logique (pas de gros commits)
- Pusher APRÈS chaque commit (sauvegarde automatique)
- Messages de commit DESCRIPTIFS et PRÉCIS
- Build + Tests AVANT chaque commit
- Rebase sur main AVANT merge si main a avancé
- Merge avec `--no-ff` pour garder l'historique
- Supprimer feature branch APRÈS merge

**❌ À ÉVITER :**
- Gros commits avec multiples modifications logiques
- Messages de commit vagues ("fix", "update", "changes")
- Commiter du code qui ne compile pas
- Commiter des tests qui échouent
- Laisser des feature branches non mergées
- Merge avec fast-forward (perte d'historique)
- Garder des feature branches après merge

### Situations Spéciales

**Abandon d'une tâche (bloquée) :**
```powershell
# 1. Commit l'état actuel (WIP)
git add -A
git commit -m "chore(task): WIP - Task blocked, see .tasks/blocked/"
git push origin feature/001--task-name

# 2. Revenir sur main sans merger
git checkout main

# Note: La feature branch reste disponible pour reprise ultérieure
Write-Host "⚠️ Feature branch conservée pour reprise future" -ForegroundColor Yellow
```

**Reprise d'une tâche bloquée :**
```powershell
# 1. Récupérer la feature branch existante
$taskFile = Get-Item .tasks\in-progress\*.task.md
$taskId = $taskFile.Name -replace '^(\d+)--.*', '$1'
$taskName = $taskFile.Name -replace '^\d+--(.+)\.task\.md$', '$1'
$featureBranch = "feature/$taskId--$taskName"

# 2. Checkout de la branche existante
git checkout $featureBranch
git pull origin $featureBranch

Write-Host "✅ REPRISE: $featureBranch" -ForegroundColor Green
```

### Commandes Git de Référence

**Informations sur l'état :**
```powershell
# Branche actuelle
git rev-parse --abbrev-ref HEAD

# Statut des fichiers
git status --short

# Historique compact
git log --oneline --graph --decorate -20

# Voir les différences non commitées
git diff

# Voir les différences stagées
git diff --cached

# Lister toutes les branches (locales + distantes)
git branch -a

# Comparer avec main
git diff main...HEAD
```

**Nettoyage :**
```powershell
# Supprimer branches locales déjà mergées
git branch --merged main | Where-Object { $_ -notmatch "main|develop|\*" } | ForEach-Object { git branch -d $_.Trim() }

# Nettoyer références distantes obsolètes
git remote prune origin

# Lister branches distantes supprimées mais encore en cache local
git branch -vv | Select-String ": gone]"
```

## 📁 STRUCTURE DES TÂCHES

```
.tasks/
├── to-do/          # File d'attente (traiter par ordre numérique)
├── in-progress/    # Tâche active (UNE SEULE à la fois)
├── done/           # Tâches complétées
└── blocked/        # Tâches bloquées (raison documentée)
```

### Format de Nommage des Tâches

**FORMAT OBLIGATOIRE** : `{ID}--{titre-court}.task.md`
- **ID** : 3 chiffres, unique dans tout `.tasks/` (ex: `001`, `002`, `015`)
- **Titre** : Court, kebab-case, descriptif (ex: `fix-login-bug`, `add-user-api`)
- **Extension** : `.task.md` (obligatoire)

**Exemple** : `003--implement-authentication.task.md`

### Unicité des IDs

**RÈGLE CRITIQUE** : Chaque ID doit être UNIQUE dans tout `.tasks/` (récursif).

**AVANT de créer une nouvelle tâche, EXÉCUTER :**

```powershell
# 1. Lister tous les IDs existants
$existingIds = Get-ChildItem .tasks -Filter *.task.md -Recurse | 
    ForEach-Object { $_.Name -replace '^(\d+)--.*', '$1' } | 
    Sort-Object -Unique

# 2. Trouver le prochain ID disponible
$maxId = Get-ChildItem .tasks -Filter *.task.md -Recurse | 
    ForEach-Object { [int]($_.Name -replace '^(\d+)--.*', '$1') } | 
    Measure-Object -Maximum | 
    Select-Object -ExpandProperty Maximum

$nextId = "{0:D3}" -f ($maxId + 1)
Write-Host "Prochain ID disponible: $nextId"
```

**Bénéfice** : Permet déplacements entre `to-do/`, `in-progress/`, `done/`, `blocked/` sans conflit.

### Archivage et Historique

**OBLIGATOIRE** : Conserver l'historique complet des tâches.

**Structure d'archivage :**
```
.tasks/
└── done/
    ├── 2025-12/          # Archivage mensuel
    │   ├── 001--fix-login.task.md
    │   └── 002--add-api.task.md
    └── archive.log       # Journal des complétions
```

**Script d'archivage mensuel :**
```powershell
# Exécuter le 1er de chaque mois
$utf8NoBom = New-Object System.Text.UTF8Encoding $false
$currentMonth = (Get-Date).ToString("yyyy-MM")
$archiveDir = ".tasks\done\$currentMonth"

if (-not (Test-Path $archiveDir)) {
    New-Item -ItemType Directory -Path $archiveDir -Force | Out-Null
}

# Déplacer tâches complétées le mois précédent
$lastMonth = (Get-Date).AddMonths(-1).ToString("yyyy-MM")
Get-ChildItem .tasks\done\*.task.md | Where-Object {
    $_.LastWriteTime.ToString("yyyy-MM") -eq $lastMonth
} | ForEach-Object {
    $dest = Join-Path $archiveDir $_.Name
    Move-Item $_.FullName $dest -Force
    "$((Get-Date).ToString('o')) | ARCHIVED | $($_.Name)" | 
        Add-Content .tasks\done\archive.log -Encoding UTF8
}
```

### Sauvegarde et Récupération

**RÈGLE CRITIQUE** : Sauvegarder avant chaque modification critique.

**Avant modification de fichier critique (ADR, code métier) :**
```powershell
# Créer snapshot avant modification
$timestamp = (Get-Date).ToString("yyyyMMdd-HHmmss")
$file = "src\critical\file.cs"
$backup = ".backups\$timestamp--$(Split-Path $file -Leaf)"

New-Item -ItemType Directory -Path .backups -Force | Out-Null
Copy-Item $file $backup
Write-Host "✅ BACKUP: $backup" -ForegroundColor Green
```

**Récupération en cas d'erreur :**
```powershell
# Lister les backups disponibles
Get-ChildItem .backups | Sort-Object Name -Descending | Select-Object -First 10

# Restaurer un backup
$latest = Get-ChildItem .backups | Sort-Object Name -Descending | Select-Object -First 1
$original = ".backups\$($latest.Name)" -replace '\d{8}-\d{6}--', ''
Copy-Item $latest.FullName $original -Force
Write-Host "✅ RESTORED: $original" -ForegroundColor Green
```

## 📝 ENCODAGE FICHIERS (UTF-8 sans BOM)

**RÈGLE ABSOLUE** : TOUJOURS utiliser UTF-8 **SANS BOM** pour `.md`, `.yml`, `.yaml`.

**Pourquoi ?** UTF-8 avec BOM provoque :
- Échecs parsing YAML/Markdown
- Problèmes Git (détection modifications)
- Caractères parasites dans éditeurs

### PowerShell - Manipulation Fichiers

**LIRE un fichier :**
```powershell
$content = [System.IO.File]::ReadAllText("$PWD\fichier.md", [System.Text.UTF8Encoding]::new($false))
```

**ÉCRIRE un fichier (UTF-8 sans BOM) :**
```powershell
$utf8NoBom = New-Object System.Text.UTF8Encoding $false
$content = "Nouveau contenu"
[System.IO.File]::WriteAllLines("$PWD\fichier.md", $content, $utf8NoBom)
```

**AJOUTER au fichier :**
```powershell
$utf8NoBom = New-Object System.Text.UTF8Encoding $false
$existing = [System.IO.File]::ReadAllText("$PWD\fichier.md", $utf8NoBom)
$updated = $existing + "`n## Nouvelle Section`n"
[System.IO.File]::WriteAllLines("$PWD\fichier.md", $updated, $utf8NoBom)
```

**VÉRIFIER encodage (détection BOM) :**
```powershell
$bytes = Get-Content "fichier.md" -Encoding Byte -TotalCount 3
if ($bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
    Write-Warning "⚠️ BOM détecté!"
} else {
    Write-Host "✅ UTF-8 sans BOM" -ForegroundColor Green
}
```

### Règles Strictes

✅ **OBLIGATOIRE** :
- `[System.IO.File]::WriteAllLines()` avec `UTF8Encoding($false)`
- Vérifier encodage après chaque écriture
- Appliquer à TOUS les fichiers `.md`, `.yml`, `.yaml`

❌ **INTERDIT** :
- `Out-File` sans `-Encoding UTF8`
- Opérateurs `>` ou `>>` (encodage imprévisible)
- `-Encoding Unicode` ou `-Encoding UTF32`
- `Add-Content` sans vérification encodage

## 🚧 PÉRIMÈTRE EXCLU

**HORS PÉRIMÈTRE - NE JAMAIS TRAITER :**

- 🚫 CI/CD : GitHub Actions, Azure DevOps, GitLab CI, Jenkins, CircleCI
- 🚫 Fichiers : `.github/workflows/`, `.azure-pipelines.yml`, `.gitlab-ci.yml`, `Jenkinsfile`
- 🚫 Automatisation déploiement continu

**Si tâche CI/CD détectée :**
1. BLOQUER immédiatement
2. DOCUMENTER "CI/CD hors périmètre" dans `.task.md`
3. DÉPLACER vers `.tasks/blocked/`
4. NOTIFIER utilisateur

## 🎯 PRINCIPES EXÉCUTION

1. **Une tâche à la fois** : Ordre numérique strict (001 → 002 → 003...)
2. **Priorité blocages** : Vérifier `.tasks/blocked/` AVANT `.tasks/to-do/`
3. **Traçabilité totale** : Documenter chaque mouvement, argumenter chaque blocage
4. **Autonomie** : Comprendre → Exécuter → Valider (pas d'ambiguïté)
5. **Conformité ADR** : Respecter 100% des décisions architecturales
6. **Qualité non négociable** : Build ✅ + Tests ✅ + Standards ✅ avant complétion

## 🔄 RÉSILIENCE ET RÉCUPÉRATION

### Gestion des Erreurs

**PRINCIPE** : Toujours prévoir un plan de récupération.

**En cas d'échec de build :**
```powershell
# 1. CAPTURER l'erreur complète
dotnet build 2>&1 | Tee-Object -FilePath .logs\build-error.log

# 2. ANALYSER l'erreur
$errorLog = Get-Content .logs\build-error.log
$errorType = if ($errorLog -match "CS\d{4}") { "Compilation" }
            elseif ($errorLog -match "NU\d{4}") { "NuGet" }
            else { "Inconnu" }

Write-Host "Type d'erreur: $errorType" -ForegroundColor Red

# 3. RESTAURER état stable si nécessaire
git status
# Si modifications non commitées, créer branche de sauvegarde
git stash save "backup-before-rollback-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
```

**En cas d'échec de tests :**
```powershell
# 1. EXÉCUTER tests avec logs détaillés
dotnet test --logger "console;verbosity=detailed" 2>&1 | 
    Tee-Object -FilePath .logs\test-failure.log

# 2. ISOLER le test en échec
$failedTests = Get-Content .logs\test-failure.log | 
    Select-String "Failed\s+(.+)\[" | 
    ForEach-Object { $_.Matches.Groups[1].Value }

# 3. EXÉCUTER test isolé pour diagnostic
foreach ($test in $failedTests) {
    Write-Host "`nTest en échec: $test" -ForegroundColor Yellow
    dotnet test --filter "FullyQualifiedName~$test" --logger "console;verbosity=detailed"
}
```

### Points de Contrôle (Checkpoints)

**OBLIGATOIRE** : Créer des checkpoints avant actions critiques.

**Avant modification majeure :**
```powershell
# Créer checkpoint Git
$checkpointName = "checkpoint-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
git add -A
git commit -m "CHECKPOINT: $checkpointName - Avant modification critique"
git tag -a $checkpointName -m "Point de restauration automatique"

Write-Host "✅ CHECKPOINT créé: $checkpointName" -ForegroundColor Green
Write-Host "   Restauration: git reset --hard $checkpointName" -ForegroundColor Gray
```

**Restauration d'un checkpoint :**
```powershell
# Lister les checkpoints disponibles
git tag -l "checkpoint-*" | Sort-Object -Descending | Select-Object -First 5

# Restaurer (sans perdre les modifications)
$checkpoint = "checkpoint-20251205-143000"
git stash save "backup-avant-restore-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
git reset --hard $checkpoint
git stash list  # Voir les sauvegardes disponibles
```

### Validation Continue

**PRINCIPE** : Valider à chaque étape critique, pas seulement à la fin.

**Étapes de validation :**
1. **Après chaque fichier modifié** → Build incrémental
2. **Après chaque méthode ajoutée** → Test unitaire de la méthode
3. **Après chaque composant** → Test d'intégration du composant
4. **Avant commit** → Build complet + Tous les tests

**Script de validation rapide :**
```powershell
function Test-QuickValidation {
    param([string]$FilePath)
    
    Write-Host "`n=== VALIDATION RAPIDE ===" -ForegroundColor Cyan
    
    # 1. Build du projet concerné
    $projectDir = Split-Path $FilePath -Parent
    while ($projectDir -and -not (Test-Path "$projectDir\*.csproj")) {
        $projectDir = Split-Path $projectDir -Parent
    }
    
    if ($projectDir) {
        Write-Host "Build: $projectDir" -ForegroundColor Yellow
        dotnet build $projectDir --no-restore -v quiet
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ BUILD FAILED" -ForegroundColor Red
            return $false
        }
    }
    
    # 2. Tests du projet concerné
    $testProject = $projectDir -replace '([^\\]+)$', '$1.Unit.Tests'
    if (Test-Path "$testProject\*.csproj") {
        Write-Host "Tests: $testProject" -ForegroundColor Yellow
        dotnet test $testProject --no-build --no-restore -v quiet
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ TESTS FAILED" -ForegroundColor Red
            return $false
        }
    }
    
    Write-Host "✅ VALIDATION OK" -ForegroundColor Green
    return $true
}

# Utilisation
Test-QuickValidation "src\Domain\MyClass.cs"
```

## 🏛️ Piliers Fondamentaux de Qualité

Chaque implémentation et décision technique DOIT être évaluée selon ces 12 piliers :

### 1. Maintenabilité
- Le code est-il lisible et compréhensible par un nouveau développeur ?
- La dette technique est-elle maîtrisée ou s'accumule-t-elle ?
- La documentation permet-elle une prise en main rapide ?
- Les patterns utilisés sont-ils cohérents et reconnaissables ?

### 2. Simplicité (KISS - Keep It Simple, Stupid)
- La solution est-elle la plus simple possible ?
- Y a-t-il de la sur-ingénierie ou de la complexité accidentelle ?
- Le nombre de concepts/abstractions est-il minimal ?
- Un développeur junior pourrait-il comprendre la solution ?

### 3. Précision
- Les comportements sont-ils déterministes et prévisibles ?
- Les contrats (API, interfaces) sont-ils clairement définis ?
- Les cas limites sont-ils gérés explicitement ?
- Les types et contraintes sont-ils correctement définis ?

### 4. Robustesse
- Comment le système réagit-il aux erreurs (fail gracefully) ?
- Les points de défaillance uniques (SPOF) sont-ils identifiés et gérés ?
- La résilience face aux pannes est-elle assurée (retry, circuit breaker) ?
- Les validations et contrôles d'intégrité sont-ils en place ?

### 5. Sécurité
- Les données sensibles sont-elles correctement protégées ?
- L'authentification et l'autorisation sont-elles robustes ?
- Les vulnérabilités connues (OWASP, CVE) sont-elles adressées ?
- Le principe du moindre privilège est-il appliqué ?
- Les secrets sont-ils gérés de manière sécurisée (Vault, variables chiffrées) ?

### 6. Extensibilité
- Le système peut-il absorber des changements sans refonte majeure ?
- Les dépendances sont-elles découplées ?
- L'architecture permet-elle l'ajout de fonctionnalités ?
- Les points d'extension sont-ils documentés et testables ?
- Les interfaces sont-elles stables et versionnées ?

### 7. Performance et Scalabilité
- Les performances sont-elles mesurées et optimisées selon les besoins réels ?
- Le système peut-il gérer une charge croissante ?
- Les goulots d'étranglement sont-ils identifiés ?
- Les ressources sont-elles utilisées efficacement ?

### 8. Factualité
- Chaque constat est-il vérifiable et reproductible ?
- Les métriques sont-elles mesurables (performance, erreurs, couverture) ?
- Les recommandations sont-elles justifiées par des faits observés ?
- Les décisions sont-elles tracées avec leur contexte ?

### 9. Rationalisation
- Les choix techniques sont-ils justifiés par des besoins réels et mesurables ?
- Chaque composant/fonctionnalité a-t-il une raison d'être claire ?
- Les décisions sont-elles basées sur des critères objectifs plutôt que des préférences personnelles ?
- Les alternatives ont-elles été évaluées de manière systématique ?
- Les compromis (trade-offs) sont-ils explicitement documentés ?

### 10. Harmonisation
- Les conventions de nommage sont-elles cohérentes dans tout le projet ?
- Les patterns et idiomes sont-ils utilisés de manière uniforme ?
- Les styles de code (indentation, formatage) suivent-ils les mêmes règles ?
- Les approches techniques similaires sont-elles traitées de manière similaire ?
- La structure des composants suit-elle un modèle prévisible ?

### 11. Mutualisation
- Le code dupliqué est-il identifié et éliminé (DRY - Don't Repeat Yourself) ?
- Les fonctionnalités communes sont-elles extraites en composants réutilisables ?
- Les ressources partagées (configurations, utilitaires) sont-elles centralisées ?
- Les dépendances communes sont-elles factorisées ?
- Le partage de code compromet-il la simplicité ou crée-t-il du couplage excessif ?

### 12. Modularisation
- Les responsabilités sont-elles clairement séparées (Single Responsibility Principle) ?
- Les modules ont-ils des interfaces claires et des couplages faibles ?
- La hiérarchie des dépendances est-elle acyclique et compréhensible ?
- Les modules peuvent-ils être testés, déployés ou remplacés indépendamment ?
- La granularité des modules est-elle appropriée (ni trop fine, ni trop grossière) ?

### Application des Piliers

**VÉRIFIER systématiquement** :
- Avant toute implémentation : Quelle est la solution la plus simple qui respecte les 12 piliers ?
- Pendant le développement : Les piliers sont-ils respectés à chaque étape ?
- Après complétion : Tous les piliers ont-ils été validés ?

**DOCUMENTER** dans les ADR :
- Quels piliers sont impactés par la décision ?
- Comment la décision respecte-t-elle chaque pilier concerné ?
- Quels compromis ont été faits et pourquoi ?

**ÉQUILIBRER les piliers** :
- La **Rationalisation** guide les choix : chaque décision doit être justifiée
- L'**Harmonisation** assure la cohérence : les patterns doivent être uniformes
- La **Mutualisation** élimine la duplication : le code commun est partagé
- La **Modularisation** structure l'architecture : les responsabilités sont séparées
- Ces 4 piliers structurels soutiennent les 8 piliers de qualité technique

## 📋 PRIORITÉS TÂCHES

**ORDRE STRICT :**

1. 🔴 **ABSOLU** : Tâche en cours (`.tasks/in-progress/`)
2. 🟠 **HAUTE** : Tâches débloquées (`.tasks/blocked/`, ordre numérique)
3. 🟢 **NORMALE** : Prochaine tâche (`.tasks/to-do/`, ordre numérique)

## 🔧 GESTION BLOCAGES

### Bloquer une Tâche

**1. IDENTIFIER cause :**
- Dépendance manquante (fichier, service, variable)
- Information ambiguë/incomplète
- Conflit ADR
- Erreur technique (build, test, runtime)
- Décision utilisateur requise
- Hors périmètre (CI/CD)

**2. ARGUMENTER (dans `.task.md`) :**
```markdown
## BLOCAGE

**Raison :** [Cause précise]

**Problème détaillé :**
[Description complète du problème]

**Pourquoi non complétable :**
[Explication argumentée]

**Actions pour débloquer :**
1. [Action 1]
2. [Action 2]

**Tentatives effectuées :**
- [Tentative 1] → [Résultat]
- [Tentative 2] → [Résultat]

**Alternatives considérées :**
- [Alternative 1] → Rejetée car [raison]
- [Alternative 2] → Rejetée car [raison]
```

**3. DÉPLACER :**
```powershell
Move-Item .tasks\in-progress\{tache}.task.md .tasks\blocked\
```

**4. NOTIFIER utilisateur (message clair + actionnable)**

### Vérifier Tâches Bloquées

**AVANT chaque nouvelle tâche de `.tasks/to-do/` :**

```powershell
$blocked = Get-ChildItem .tasks\blocked\*.task.md | Sort-Object Name
foreach ($task in $blocked) {
    Write-Host "Analyse: $($task.Name)" -ForegroundColor Yellow
    # Analyser si débloquable maintenant
}
```

**DÉCISION :**
- Tâche débloquable → TRAITER EN PRIORITÉ
- Aucune tâche débloquable → Prochaine tâche `.tasks/to-do/`

### Échec Partiel

**Si tâche partiellement complétée :**

1. COMPLÉTER ce qui est possible
2. DOCUMENTER reste à faire (détail dans `.task.md`)
3. CHOIX :
   - Si proche de la fin → LAISSER dans `.tasks/in-progress/`
   - Si blocage majeur → DÉPLACER vers `.tasks/blocked/`
4. Si nécessaire, CRÉER sous-tâche dans `.tasks/to-do/` (nouvel ID)

## ✅ CHECKLIST VALIDATION

**AVANT de compléter une tâche, VÉRIFIER :**

- [ ] Tous fichiers requis créés/modifiés
- [ ] **Build réussi sans aucun warning**
  - [ ] `dotnet: build` exitCode = 0
  - [ ] `0 Error(s)` dans la sortie
  - [ ] `0 Warning(s)` dans la sortie (AUCUN warning accepté)
- [ ] **Tests unitaires 100% réussis**
  - [ ] `dotnet: test` exitCode = 0
  - [ ] `Test Run Successful` dans la sortie
  - [ ] `Passed: X` (tous les tests passés)
  - [ ] `Failed: 0` (aucun test en échec)
  - [ ] `Skipped: 0` (aucun test ignoré)
- [ ] Application testée fonctionnellement
- [ ] Code conforme standards (formatage, conventions)
- [ ] Documentation à jour (commentaires, README, ADR si applicable)
- [ ] Aucun conflit ADR
- [ ] Critères succès tâche satisfaits
- [ ] Aucun fichier temporaire (`.tmp`, `.swp`, `~`)

## ⚡ PERFORMANCE ET OPTIMISATION

### Build Incrémental

**PRINCIPE** : Ne rebuilder que le nécessaire.

**Stratégie de build :**
```powershell
# 1. Identifier les projets modifiés
$modifiedFiles = git diff --name-only HEAD

$affectedProjects = $modifiedFiles | 
    ForEach-Object { 
        $dir = Split-Path $_ -Parent
        while ($dir -and -not (Test-Path "$dir\*.csproj")) {
            $dir = Split-Path $dir -Parent
        }
        if ($dir) { Get-Item "$dir\*.csproj" }
    } | 
    Select-Object -Unique

# 2. Build uniquement les projets affectés
foreach ($project in $affectedProjects) {
    Write-Host "Build: $($project.Name)" -ForegroundColor Cyan
    dotnet build $project.FullName --no-restore
}

# 3. Tests uniquement des projets affectés
$testProjects = $affectedProjects | 
    ForEach-Object { 
        $testPath = $_.FullName -replace '\.csproj$', '.Unit.Tests.csproj'
        if (Test-Path $testPath) { Get-Item $testPath }
    }

foreach ($testProject in $testProjects) {
    Write-Host "Test: $($testProject.Name)" -ForegroundColor Cyan
    dotnet test $testProject.FullName --no-build
}
```

### Cache et Réutilisation

**OBLIGATOIRE** : Réutiliser les résultats de build/tests.

**Cache NuGet local :**
```powershell
# Configurer cache local (une fois)
$cacheDir = "$env:LOCALAPPDATA\.nuget\cache"
dotnet nuget locals all --list

# Restore avec cache
dotnet restore --packages $cacheDir
```

**Cache de build :**
```powershell
# Build avec cache binaire
dotnet build `
    --configuration Release `
    --no-restore `
    -p:BuildInParallel=true `
    -p:UseSharedCompilation=true `
    -maxcpucount:$env:NUMBER_OF_PROCESSORS
```

### Parallélisation

**PRINCIPE** : Exécuter les tâches indépendantes en parallèle.

**Tests parallèles :**
```powershell
# Exécuter tous les tests en parallèle
dotnet test --parallel --maxcpucount:$env:NUMBER_OF_PROCESSORS

# Ou avec contrôle du nombre de threads
dotnet test -p:ParallelizeTestCollections=true -p:MaxParallelThreads=4
```

**Build parallèle de plusieurs projets :**
```powershell
$projects = Get-ChildItem -Recurse -Filter *.csproj | 
    Where-Object { $_.FullName -notmatch 'Tests' }

$jobs = $projects | ForEach-Object {
    Start-Job -ScriptBlock {
        param($projectPath)
        dotnet build $projectPath --no-restore
    } -ArgumentList $_.FullName
}

# Attendre la fin de tous les jobs
$jobs | Wait-Job | Receive-Job
$jobs | Remove-Job
```

### Monitoring et Métriques

**OBLIGATOIRE** : Mesurer pour optimiser.

**Temps d'exécution des tâches :**
```powershell
function Measure-TaskDuration {
    param(
        [string]$TaskName,
        [scriptblock]$ScriptBlock
    )
    
    Write-Host "`n⏱️ DÉBUT: $TaskName" -ForegroundColor Cyan
    $start = Get-Date
    
    try {
        & $ScriptBlock
        $success = $LASTEXITCODE -eq 0
    }
    catch {
        $success = $false
        Write-Host "❌ ERREUR: $_" -ForegroundColor Red
    }
    
    $end = Get-Date
    $duration = $end - $start
    
    $status = if ($success) { "✅ SUCCÈS" } else { "❌ ÉCHEC" }
    Write-Host "$status: $TaskName - Durée: $($duration.ToString('hh\:mm\:ss'))" -ForegroundColor $(if ($success) { 'Green' } else { 'Red' })
    
    # Logger dans fichier de métriques
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    $logEntry = "$(Get-Date -Format 'o')|$TaskName|$($duration.TotalSeconds)|$success"
    Add-Content -Path ".logs\metrics.log" -Value $logEntry -Encoding UTF8
    
    return $success
}

# Utilisation
Measure-TaskDuration "Build Solution" {
    dotnet build --no-restore
}

Measure-TaskDuration "Run Tests" {
    dotnet test --no-build
}
```

**Analyse des métriques :**
```powershell
# Analyser les performances
$metrics = Import-Csv .logs\metrics.log -Delimiter '|' -Header Date,Task,Duration,Success

# Top 5 tâches les plus lentes
$metrics | 
    Sort-Object { [double]$_.Duration } -Descending | 
    Select-Object -First 5 | 
    Format-Table Task, @{L="Durée (s)";E={[math]::Round([double]$_.Duration, 2)}}

# Taux de succès par tâche
$metrics | 
    Group-Object Task | 
    Select-Object Name, Count, @{
        L="Succès (%)";
        E={[math]::Round(($_.Group | Where-Object Success -eq 'True').Count / $_.Count * 100, 1)}
    }
```

## 💡 WORKFLOW EXEMPLES

### Exemple : Workflow Complet

```powershell
# 1. Vérifier in-progress (vide)
# 2. Vérifier blocked (003-deploy-service → non débloquable)
# 3. Lister to-do (001-fix-config, 002-add-health-check)
# 4. Lire 001-fix-config.task.md intégralement
# 5. Vérifier ADR applicables
# 6. Annoter début + déplacer vers in-progress
# 7. Exécuter (build + tests + validation Chrome DevTools)
# 8. Valider checklist complète
# 9. Annoter fin + durée
# 10. Déplacer vers done
# 11. Retour étape 1
```

### Exemple : Blocage Documenté

```markdown
## BLOCAGE

**Raison :** Variable `DB_CONNECTION_STRING` non définie

**Problème :** L'application ne peut pas se connecter à PostgreSQL

**Actions pour débloquer :**
1. Définir variable dans `appsettings.json`
2. OU obtenir chaîne de connexion de l'équipe infra

**Tentatives :**
- Cherché dans fichiers config → Introuvable
- Vérifié docs/adr/ → Pas de décision sur config DB
```

### Exemple : Workflow avec Résilience

```powershell
# Workflow complet avec gestion d'erreurs et checkpoints
$taskFile = ".tasks\to-do\001--fix-authentication.task.md"

try {
    # 1. CHECKPOINT avant démarrage
    git add -A
    git commit -m "CHECKPOINT: Avant tâche 001"
    
    # 2. DÉMARRER tâche
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    $start = (Get-Date).ToUniversalTime().ToString("o")
    $content = [System.IO.File]::ReadAllText($taskFile, $utf8NoBom)
    $content += "`n`n## TRACKING`nDébut: $start`n"
    [System.IO.File]::WriteAllLines($taskFile, $content, $utf8NoBom)
    Move-Item $taskFile .tasks\in-progress\
    
    # 3. EXÉCUTER avec validation stricte continue
    Measure-TaskDuration "Validation Initiale" {
        Test-StrictValidation
    }
    
    if (-not $?) {
        throw "Validation initiale échouée"
    }
    
    # Modifications du code...
    
    Measure-TaskDuration "Validation Après Modif" {
        Test-StrictValidation
    }
    
    if (-not $?) {
        throw "Validation échouée (build warnings ou tests failed)"
    }
    
    # 4. COMPLÉTER tâche
    $end = (Get-Date).ToUniversalTime().ToString("o")
    $taskFileNew = ".tasks\in-progress\001--fix-authentication.task.md"
    $content = [System.IO.File]::ReadAllText($taskFileNew, $utf8NoBom)
    $duration = ([DateTime]::Parse($end) - [DateTime]::Parse($start)).ToString("hh\:mm\:ss")
    $content += "Fin: $end`nDurée: $duration`n"
    [System.IO.File]::WriteAllLines($taskFileNew, $content, $utf8NoBom)
    Move-Item $taskFileNew .tasks\done\
    
    Write-Host "`n✅ TÂCHE COMPLÉTÉE" -ForegroundColor Green
}
catch {
    Write-Host "`n❌ ERREUR: $_" -ForegroundColor Red
    
    # BLOQUER la tâche avec contexte complet
    $blockContent = @"

## BLOCAGE

**Raison :** Erreur technique - $_

**Problème détaillé :**
$(Get-Content .logs\build-error.log -ErrorAction SilentlyContinue)

**Actions pour débloquer :**
1. Analyser les logs dans .logs/
2. Restaurer checkpoint si nécessaire: git reset --hard HEAD~1
3. Corriger l'erreur identifiée

**Timestamp :** $(Get-Date -Format 'o')
"@
    
    $taskFileError = ".tasks\in-progress\001--fix-authentication.task.md"
    if (Test-Path $taskFileError) {
        Add-Content $taskFileError $blockContent
        Move-Item $taskFileError .tasks\blocked\
    }
}
```

## 🎓 COMMUNICATION

### Avec Utilisateur

- EXPLIQUER quelle tâche est traitée
- SIGNALER blocages immédiatement
- PROPOSER solutions concrètes
- CONFIRMER complétion (résumé factuel)

### Temporalité

- UTILISER UTC, format ISO 8601
- ANNOTER début/fin/blocage systématiquement
- CALCULER durées (actif, blocage, total)
- UNE tâche active à la fois
- VALIDER avant de continuer

### Documentation

- TRACER décisions techniques non triviales
- PROPOSER ADR si décision architecturale majeure
- RESPECTER format `*.adr.md` obligatoire
- ENRICHIR fichiers tâches (notes suivi)

#### Documentation Technique (ADR)

**OBLIGATOIRE** : Créer systématiquement un fichier `*.adr.md` dans `docs/adr/` quand l'utilisateur apporte :
- Des **contraintes techniques** (choix d'architecture, pattern, technologie)
- Des **décisions architecturales** (structure, découpage, organisation)
- Des **précisions techniques** qui impactent la conception ou l'implémentation

**Format obligatoire :** `{id}-{titre-court}.adr.md` (ex: `005-use-redis-for-caching.adr.md`)

**Contenu minimal d'un ADR :**
```markdown
# {ID}. {Titre de la Décision}

Date: {YYYY-MM-DD}
Statut: {Accepté|Proposé|Rejeté|Déprécié|Remplacé}

## Contexte

[Description du contexte et du problème à résoudre]

## Décision

[Description de la décision prise]

## Conséquences

### Positives
- [Bénéfice 1]
- [Bénéfice 2]

### Négatives
- [Compromis 1]
- [Compromis 2]

## Alternatives Considérées

### Alternative 1: {Nom}
- Avantages: [liste]
- Inconvénients: [liste]
- Raison du rejet: [explication]

## Références

- [Lien vers documentation]
- [Lien vers discussion]
```

#### Documentation Fonctionnelle (Requirements)

**OBLIGATOIRE** : Créer systématiquement un fichier `*.requirements.md` dans `docs/requirements/{domaine}/{sous-domaine}/` quand l'utilisateur apporte :
- Des **contraintes fonctionnelles** (règles métier, processus, workflows)
- Des **besoins utilisateur** (cas d'usage, scénarios, comportements attendus)
- Des **précisions métier** qui définissent le périmètre fonctionnel

**RÈGLES STRICTES** :
1. **Organisation par domaines** : Requirements classés par domaine métier (ex: `authentication/`, `prompts/favorites/`, `analytics/statistics/`)
2. **Numérotation globale** : IDs uniques dans tout le projet (001, 002, 003...)
3. **Séparation fonctionnel/technique** : Requirements = QUOI métier, ADR = COMMENT technique
4. **Format standardisé** : `.requirements.md` uniquement (voir `.github/instructions/requirements.documentation.instructions.md`)
5. **Validation exhaustive** : Critères d'acceptation complets (cas nominaux, limites, exceptions)
6. **Vérification cohérence** : Aucune duplication, aucune contradiction avec corpus existant

**Nomenclature OBLIGATOIRE :** `{id}-{titre-court}.requirements.md` (ex: `020-favorite-prompts.requirements.md`)

**Emplacement par domaine :**
```
docs/requirements/
├── authentication/
│   └── 001-user-login.requirements.md
├── prompts/
│   ├── library/
│   │   └── 010-prompt-library.requirements.md
│   └── favorites/
│       └── 020-favorite-prompts.requirements.md
└── analytics/
    └── statistics/
        └── 040-usage-statistics.requirements.md
```

**Structure de fichier OBLIGATOIRE (`*.requirements.md`) :**
```markdown
# REQ-{id}. {Titre court et descriptif}

Date: YYYY-MM-DD

## Statut

{Draft | Validé | Implémenté | Obsolète | Remplacé par REQ-XXX}

## Contexte Métier

[Besoin métier, problématique utilisateur, justification]

## Objectifs Métier

- [Objectif 1 - mesurable]
- [Objectif 2 - mesurable]

## Acteurs Concernés

- **Acteur 1** : [Rôle et responsabilités]
- **Acteur 2** : [Rôle et responsabilités]

## Exigences Fonctionnelles

### EF-{id}.1 - {Titre}
- **Description** : [Le système DOIT/DEVRAIT/PEUT...]
- **Priorité** : {Critique|Haute|Moyenne|Basse}
- **Justification priorité** : [Pourquoi cette priorité]
- **Critères de validation** :
  - [Critère mesurable 1 avec seuil quantifié]
  - [Critère mesurable 2 avec seuil quantifié]
- **Exemples concrets** : [Scénarios réels avec données]
- **Seuils quantifiés** : [Volumes min/max, temps de réponse]

## Règles Métier

### RG-{id}.1 - {Titre}
- **Description** : [Énoncé clair et non ambigu]
- **Conditions** : [Quand la règle s'applique]
- **Actions** : [Comportement PRÉCIS attendu]
- **Formule/Algorithme** : [Si calcul, formule exacte + exemple]
- **Origine** : [Réglementation, processus métier, décision business]

## Scénarios d'Usage

### Scénario 1: {Nom}
**Acteur** : {Qui}
**Objectif** : {Quoi}

**Préconditions** :
- [Condition 1]

**Flux nominal** :
1. L'acteur...
2. Le système...

**Flux alternatifs** :
- **2a.** Si..., alors...

**Critères d'acceptation** :
- [ ] **CAC-1** : Étant donné..., quand..., alors... (mesurable)

**Données de test** :
- Jeu 1 : [Données entrée] → Résultat attendu : [Sortie exacte]

## Critères d'Acceptation Globaux

### Cas Nominaux (Happy Path)
- [ ] **CFN-1** : Étant donné..., quand..., alors... (avec seuils)

### Cas aux Limites (Edge Cases)
- [ ] **CFL-1** : Liste vide, valeur nulle, min/max...

### Cas d'Exception (Error Cases)
- [ ] **CFE-1** : Erreur réseau, timeout, données invalides...

## Relations avec Autres Requirements

### Dépendances (Prérequis)
- **REQ-XXX** - {Titre} : [Explication dépendance]

### Amende
- **REQ-YYY** - {Titre} : [Changements apportés]

### Désavoue
- **REQ-ZZZ** - {Titre} : [Raison du remplacement]

## Matrice de Couverture

| Objectif Métier | Exigences | Scénarios | Critères |
|-----------------|-----------|-----------|----------|
| Objectif 1      | EF-1, EF-3| Scén. 1,3 | CFN-1... |

## Analyse de Cohérence

- [ ] Aucun requirement existant ne couvre déjà ce besoin
- [ ] Aucune contradiction avec REQ-XXX
- [ ] Terminologie cohérente avec glossaire métier

## Risques et Mitigations

### Risque Métier 1 : {Titre}
- **Probabilité** : {Élevée|Moyenne|Faible}
- **Impact** : {Critique|Majeur|Mineur}
- **Mitigation** : [Actions préventives]

## Critères de Succès Métier

### Métriques d'Adoption
- [ ] **MA-1** : 60% utilisateurs utilisent la fonctionnalité

### Métriques de Performance Métier
- [ ] **MP-1** : Réduction temps de 90s à 10s

## Définition de "Fini"

### Développement
- [ ] Toutes exigences EF-XXX implémentées
- [ ] Code reviewé et approuvé

### Tests
- [ ] Tous critères CFN, CFL, CFE validés par tests automatisés
- [ ] Tests de performance métier validés

### Documentation
- [ ] Documentation utilisateur créée/mise à jour
- [ ] ADR techniques créés si nécessaire

### Validation Métier
- [ ] Démo effectuée au Product Owner
- [ ] Validation par chaque persona concerné

## Références

- [Documentation métier]
- [ADR-XXX: Décisions techniques associées]
- [Glossaire métier]
```

**PRINCIPES CLÉS pour Requirements :**
1. **Langage DIRECTIF** : Utiliser "DOIT", "DEVRAIT", "PEUT", "NE DOIT PAS"
2. **Seuils QUANTIFIÉS** : Toujours inclure nombres, temps, pourcentages
3. **Exemples CONCRETS** : Données réelles, pas de "ex: données"
4. **Format Given-When-Then** : Critères d'acceptation testables
5. **Vérification EXHAUSTIVE** : Cas nominaux + limites + exceptions
6. **Cohérence GLOBALE** : Pas de duplication ni contradiction
7. **Relations EXPLICITES** : Dépendances, amendements, désaveux clairs

## 📊 Métriques de Qualité

### Indicateurs de Succès

- ✅ Taux de complétion: Nombre de tâches dans `.tasks/done/`
- ✅ Taux de blocage: Nombre de tâches dans `.tasks/blocked/` (à minimiser)
- ✅ **Conformité ADR** : 100% des décisions architecturales respectées (aspects techniques)
- ✅ **Conformité Requirements** : 100% des exigences métier respectées (aspects fonctionnels)
- ✅ Qualité du code: Tous les critères de la checklist validés

### Signaux d'Alerte

- 🔴 Plusieurs tâches dans `.tasks/in-progress/` simultanément
- 🔴 Tâches traitées dans le désordre (numéros non séquentiels)
- 🔴 Tâches bloquées non vérifiées avant de démarrer une nouvelle tâche
- 🔴 Blocage sans argumentation détaillée dans le fichier de tâche
- 🔴 Code commité sans validation de la checklist
- 🔴 **ADR violé sans justification documentée** (décisions techniques)
- 🔴 **Requirements violés sans justification documentée** (exigences métier)
- 🔴 Tâche CI/CD traitée alors qu'elle est hors périmètre
