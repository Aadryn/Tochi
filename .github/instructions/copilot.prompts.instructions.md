---
description: Création de prompt files réutilisables dans .github/prompts
name: Copilot_Prompt_Files
applyTo: "**/.github/prompts/*.prompt.md"
---

# Prompt Files Copilot - Guide Expert

Guide pour créer des prompt files réutilisables dans `.github/prompts/` conformément à la [documentation officielle Microsoft](https://code.visualstudio.com/docs/copilot/customization/prompt-files).

## ⛔ À NE PAS FAIRE

- **N'utilise jamais** d'extension autre que `.prompt.md`
- **N'omets jamais** le frontmatter YAML (description, name, agent, model)
- **Ne crée jamais** de prompt sans description claire de son usage
- **N'oublie jamais** de spécifier le champ `agent` (souvent `agent: agent`)
- **Ne laisse jamais** le champ `model` sans valeur
- **N'utilise jamais** d'espaces dans le champ `name`
- **Ne crée jamais** de prompt trop générique (un prompt = un cas d'usage précis)

## ✅ À FAIRE

- **Utilise toujours** l'extension `.prompt.md`
- **Définis toujours** le frontmatter YAML complet (description, name, agent, model)
- **Décris toujours** clairement ce que fait le prompt en 1 ligne
- **Spécifie toujours** le modèle (`model: claude-sonnet-4.5` ou autre)
- **Inclus toujours** les variables nécessaires avec `{{variable}}`
- **Structure toujours** le prompt avec des sections claires
- **Documente toujours** les paramètres attendus et les sorties produites

## 🎯 Actions Obligatoires (Mandatory)

**À TOUJOURS respecter lors de la création/modification d'un prompt file :**

1. ✅ **Extension `.prompt.md`** : OBLIGATOIRE
   - ✅ Bon : `code-review.prompt.md`, `generate-component.prompt.md`
   - ❌ Mauvais : `prompt.md`, `code-review.md`, `generate.txt`

2. ✅ **Frontmatter YAML obligatoire** : Chaque prompt DOIT commencer par un bloc YAML
   ```yaml
   ---
   description: Description courte et précise du prompt
   name: nom-du-prompt
   agent: agent
   model: claude-sonnet-4.5
   ---
   ```

3. ✅ **Champ `description` obligatoire** : Décrire clairement ce que fait le prompt (1 ligne)
   - ✅ Bon : `"Generate a React form component with validation"`
   - ❌ Mauvais : `"Prompt"`, `"Code generator"`

4. ✅ **Champ `name` obligatoire** : Nom utilisé après `/` dans le chat
   - Format : `kebab-case` (minuscules avec tirets)
   - ✅ Bon : `code-review`, `generate-component`, `security-audit`
   - ❌ Mauvais : `CodeReview`, `generate_component`, `Security Audit`

5. ✅ **Champ `agent` recommandé** : Spécifier l'agent approprié
   - `ask` - Questions et explications
   - `edit` - Modifications de code
   - `agent` - Agent par défaut (recommandé pour flexibilité)
   - `custom-agent-name` - Nom d'un agent personnalisé

6. ✅ **Champ `model: claude-sonnet-4.5`** : OBLIGATOIRE pour utiliser Claude Sonnet 4.5
   ```yaml
   model: claude-sonnet-4.5
   ```

7. ✅ **Corps du prompt clair et directif** : Instructions précises et actionnables
8. ✅ **Variables pour flexibilité** : Utiliser `${variable}` pour réutilisabilité
9. ✅ **Références aux fichiers** : Liens Markdown relatifs vers contexte
10. ✅ **Testabilité** : Tester le prompt avec le bouton play de l'éditeur

## Frontmatter YAML

### Structure Complète

```yaml
---
description: Description courte du prompt (obligatoire)
name: nom-du-prompt (obligatoire)
argument-hint: Indication sur les arguments attendus (optionnel)
agent: agent (recommandé)
model: claude-sonnet-4.5 (obligatoire pour Claude)
tools:
  - tool-name-1
  - tool-name-2
---
```

### Champs du Frontmatter

#### `description` (OBLIGATOIRE)

Description courte et précise de ce que fait le prompt.

```yaml
# ✅ Bon - Descriptif et précis
description: Generate a REST API endpoint with validation and error handling
description: Perform security review of authentication implementation
description: Create unit tests for a given function with edge cases

# ❌ Mauvais - Trop vague
description: Generate code
description: Review
description: Helper
```

#### `name` (OBLIGATOIRE)

Nom utilisé pour invoquer le prompt avec `/` dans le chat.

**Format :** `kebab-case` (minuscules, tirets)

```yaml
# ✅ Bon
name: generate-api-endpoint
name: security-review
name: create-tests
name: refactor-code

# ❌ Mauvais
name: GenerateAPIEndpoint  # PascalCase interdit
name: generate_api_endpoint  # Underscores non recommandés
name: Generate API  # Espaces interdits
name: gen  # Trop court, pas descriptif
```

#### `argument-hint` (OPTIONNEL)

Texte d'indication affiché dans le champ de chat pour guider l'utilisateur.

```yaml
# ✅ Exemples
argument-hint: "component name and props"
argument-hint: "file path to review"
argument-hint: "function name"
argument-hint: "API endpoint path"
```

#### `agent` (RECOMMANDÉ)

Agent utilisé pour exécuter le prompt.

**Valeurs possibles :**
- `ask` - Poser des questions, obtenir des explications
- `edit` - Modifier du code existant
- `agent` - Agent par défaut (recommandé pour flexibilité)
- `nom-agent-custom` - Nom d'un agent personnalisé

```yaml
# ✅ Bon - Spécifier l'agent approprié
agent: agent        # Par défaut, flexible
agent: ask          # Pour questions/explications
agent: edit         # Pour modifications de code
agent: custom-agent # Agent personnalisé

# ⚠️ Acceptable - Agent par défaut sera utilisé
# agent: non spécifié
```

#### `model: claude-sonnet-4.5` (OBLIGATOIRE)

**TOUJOURS spécifier Claude Sonnet 4.5 comme modèle.**

```yaml
# ✅ OBLIGATOIRE
model: claude-sonnet-4.5

# ❌ Interdit - Ne pas laisser vide ou utiliser autre modèle
model: gpt-4
# model: non spécifié
```

**Justification :** Claude Sonnet 4.5 offre les meilleures performances pour les tâches complexes de développement.

#### `tools` (OPTIONNEL)

Liste des outils disponibles pour ce prompt.

```yaml
# Outils spécifiques
tools:
  - fileSearch
  - githubRepo
  - codebase

# Tous les outils d'un serveur MCP
tools:
  - server-name/*

# Combinaison
tools:
  - fileSearch
  - githubRepo
  - mcp-server/*
```

**Outils courants :**
- `fileSearch` - Recherche de fichiers
- `githubRepo` - Accès au dépôt GitHub
- `codebase` - Analyse du code
- `terminal` - Exécution de commandes
- `browser` - Navigation web

## Corps du Prompt

### Structure Recommandée

```markdown
---
description: Description du prompt
name: nom-du-prompt
agent: agent
model: claude-sonnet-4.5
---

# Titre du Prompt

Description détaillée de ce que fait le prompt et de son objectif.

## Instructions

Liste claire et directe des instructions pour l'agent :

1. Première instruction précise
2. Deuxième instruction avec détails
3. Troisième instruction actionnable

## Contexte

Contexte additionnel si nécessaire :
- Point de contexte 1
- Point de contexte 2

## Format de Sortie Attendu

Description du format de réponse souhaité :

```format
Structure attendue de la réponse
```

## Exemples

### Exemple 1

Entrée : `exemple d'entrée`
Sortie attendue : 
```
exemple de sortie
```
```

### Instructions Claires et Directives

**Le corps du prompt DOIT être :**
- ✅ **Directif** : Utiliser l'impératif (Analyse, Génère, Vérifie)
- ✅ **Précis** : Éviter les ambiguïtés
- ✅ **Actionnable** : Chaque instruction doit être exécutable
- ✅ **Structuré** : Organisation logique avec sections

```markdown
# ❌ MAUVAIS - Vague et non directif
Tu pourrais peut-être regarder le code et me dire ce que tu en penses.
Il faudrait aussi vérifier s'il y a des problèmes.

# ✅ BON - Clair et directif
## Instructions

1. Analyse le code pour identifier les vulnérabilités de sécurité
2. Vérifie la conformité aux standards OWASP Top 10
3. Liste chaque problème avec sa sévérité (Critique, Élevée, Moyenne, Faible)
4. Propose une solution concrète pour chaque problème identifié

## Format de Sortie

Pour chaque vulnérabilité :
- **Type** : [Type de vulnérabilité]
- **Sévérité** : [Critique/Élevée/Moyenne/Faible]
- **Description** : [Description du problème]
- **Localisation** : [Fichier et ligne]
- **Solution** : [Action corrective recommandée]
```

### Variables

Utiliser des variables pour rendre les prompts réutilisables.

**Variables disponibles :**

```markdown
# Variables de workspace
${workspaceFolder}          # Chemin du dossier workspace
${workspaceFolderBasename}  # Nom du dossier workspace

# Variables de sélection
${selection}                # Texte sélectionné
${selectedText}             # Texte sélectionné (alias)

# Variables de fichier
${file}                     # Chemin complet du fichier
${fileBasename}             # Nom du fichier avec extension
${fileDirname}              # Chemin du répertoire du fichier
${fileBasenameNoExtension}  # Nom du fichier sans extension

# Variables d'entrée
${input:variableName}                    # Variable d'entrée
${input:variableName:placeholder}        # Variable avec placeholder
```

**Exemples d'utilisation :**

```markdown
---
description: Generate unit tests for selected function
name: generate-tests
argument-hint: test framework (jest, mocha, vitest)
agent: agent
model: claude-sonnet-4.5
---

# Generate Unit Tests

Génère des tests unitaires pour la fonction suivante :

```
${selection}
```

## Instructions

1. Analyse la fonction sélectionnée dans `${fileBasename}`
2. Utilise le framework de test : ${input:framework:jest}
3. Génère des tests couvrant :
   - Les cas nominaux
   - Les cas limites
   - Les cas d'erreur
4. Assure une couverture de code > 90%

## Localisation

Workspace : ${workspaceFolder}
Fichier : ${file}
```

### Références aux Fichiers

Utiliser des liens Markdown relatifs pour référencer des fichiers du workspace.

```markdown
---
description: Code review with project standards
name: code-review
agent: agent
model: claude-sonnet-4.5
---

# Code Review

Effectue une revue de code selon les standards du projet.

## Standards à Appliquer

Consulte les standards dans :
- [Coding Standards](../../docs/coding-standards.md)
- [Architecture Guidelines](../../docs/architecture.md)

## Instructions

1. Vérifie la conformité aux [standards de code](../../docs/coding-standards.md)
2. Valide l'architecture selon [les guidelines](../../docs/architecture.md)
3. Identifie les problèmes de sécurité
4. Suggère des améliorations de performance
```

**Règles pour les liens :**
- ✅ Utiliser des chemins relatifs au fichier prompt
- ✅ Vérifier que les fichiers référencés existent
- ✅ Préférer les liens vers instructions existantes plutôt que dupliquer

### Référence aux Outils (#tool:)

Référencer des outils dans le corps du prompt avec `#tool:nom-outil`.

```markdown
---
description: Search codebase for pattern
name: search-pattern
agent: agent
model: claude-sonnet-4.5
tools:
  - fileSearch
  - codebase
---

# Search Pattern in Codebase

Recherche un pattern dans tout le codebase.

## Instructions

1. Utilise #tool:fileSearch pour localiser les fichiers pertinents
2. Utilise #tool:codebase pour analyser le contenu
3. Liste tous les occurrences avec contexte
4. Suggère des refactorings si duplications détectées
```

## Exemples de Prompt Files

### Exemple 1 : Génération de Composant

```markdown
---
description: Generate a form component with validation
name: generate-form
argument-hint: form name and fields
agent: agent
model: claude-sonnet-4.5
---

# Generate Form Component

Génère un composant de formulaire avec validation.

## Arguments

Format : `formName field1:type field2:type ...`

Exemple : `/generate-form UserForm name:string email:email age:number`

## Instructions

1. Crée un composant formulaire nommé `${input:formName}`
2. Génère les champs spécifiés avec leurs types
3. Ajoute la validation appropriée pour chaque type :
   - `string` : longueur min/max
   - `email` : format email valide
   - `number` : valeur min/max
4. Inclut la gestion d'erreurs et les messages d'erreur
5. Ajoute les styles CSS de base

## Format de Sortie

Génère deux fichiers :
- `${input:formName}.tsx` - Composant
- `${input:formName}.test.tsx` - Tests unitaires

## Exemple

Pour : `UserForm name:string email:email`

Le composant doit inclure :
- Champ nom avec validation longueur (2-50 caractères)
- Champ email avec validation format
- Bouton submit avec état loading
- Messages d'erreur sous chaque champ
- Tests couvrant validation et soumission
```

### Exemple 2 : Revue de Sécurité

```markdown
---
description: Perform security review of selected code
name: security-review
agent: agent
model: claude-sonnet-4.5
tools:
  - codebase
---

# Security Review

Effectue une revue de sécurité approfondie du code sélectionné.

## Code à Analyser

```
${selection}
```

Fichier : `${file}`

## Instructions

1. Analyse le code pour identifier les vulnérabilités selon OWASP Top 10 :
   - Injection (SQL, NoSQL, Command)
   - Broken Authentication
   - Sensitive Data Exposure
   - XML External Entities (XXE)
   - Broken Access Control
   - Security Misconfiguration
   - Cross-Site Scripting (XSS)
   - Insecure Deserialization
   - Using Components with Known Vulnerabilities
   - Insufficient Logging & Monitoring

2. Vérifie les bonnes pratiques :
   - Validation et sanitization des entrées
   - Gestion sécurisée des secrets
   - Authentification et autorisation
   - Logging approprié
   - Gestion d'erreurs sans fuite d'information

3. Recherche dans #tool:codebase les patterns similaires problématiques

## Format de Sortie

### Vulnérabilités Identifiées

Pour chaque problème :

#### [Titre de la Vulnérabilité]
- **Sévérité** : [Critique/Élevée/Moyenne/Faible]
- **Type OWASP** : [Catégorie OWASP]
- **Localisation** : Ligne X-Y
- **Description** : [Explication du problème]
- **Impact** : [Conséquences possibles]
- **Recommandation** : [Action corrective précise]
- **Code corrigé** :
```[language]
// Code sécurisé proposé
```

### Recommandations Générales

- [Recommandation 1]
- [Recommandation 2]
```

### Exemple 3 : Génération de Tests

```markdown
---
description: Generate comprehensive unit tests for selected code
name: generate-tests
argument-hint: test framework (optional, defaults to jest)
agent: agent
model: claude-sonnet-4.5
---

# Generate Unit Tests

Génère des tests unitaires complets pour le code sélectionné.

## Code à Tester

```
${selection}
```

Fichier : `${fileBasename}`

## Configuration

Framework : ${input:framework:jest}
Workspace : ${workspaceFolder}

## Instructions

1. Analyse la fonction/classe/composant sélectionné
2. Identifie tous les cas de test nécessaires :
   - **Cas nominaux** : Comportement normal attendu
   - **Cas limites** : Valeurs min/max, vide, null, undefined
   - **Cas d'erreur** : Exceptions, erreurs attendues
   - **Cas edge** : Situations inhabituelles mais valides

3. Génère les tests avec ${input:framework:jest}
4. Utilise les meilleures pratiques :
   - Structure AAA (Arrange-Act-Assert)
   - Noms descriptifs des tests
   - Un assert par test (quand possible)
   - Mocks appropriés pour dépendances
   - Nettoyage après chaque test

5. Assure une couverture > 90%

## Format de Sortie

Génère le fichier : `${fileBasenameNoExtension}.test.${fileExtension}`

### Structure des Tests

```typescript
describe('[Nom du composant/fonction]', () => {
  // Setup commun
  beforeEach(() => {
    // Initialisation
  });

  afterEach(() => {
    // Nettoyage
  });

  describe('Cas nominaux', () => {
    it('should [comportement attendu]', () => {
      // Arrange
      // Act
      // Assert
    });
  });

  describe('Cas limites', () => {
    it('should handle empty input', () => {});
    it('should handle null value', () => {});
  });

  describe('Cas d\'erreur', () => {
    it('should throw error when invalid', () => {});
  });
});
```
```

### Exemple 4 : Refactoring

```markdown
---
description: Refactor code to improve maintainability and performance
name: refactor-code
agent: edit
model: claude-sonnet-4.5
tools:
  - codebase
---

# Refactor Code

Refactorise le code sélectionné pour améliorer la maintenabilité et les performances.

## Code à Refactorer

```
${selection}
```

Fichier : `${file}`
Workspace : ${workspaceFolder}

## Instructions

1. Analyse le code pour identifier les problèmes :
   - Duplication de code
   - Complexité cyclomatique élevée
   - Fonctions trop longues (>50 lignes)
   - Responsabilités multiples (violation SRP)
   - Couplage fort
   - Nommage peu clair
   - Performance sous-optimale

2. Recherche dans #tool:codebase les patterns réutilisables

3. Applique les principes SOLID :
   - **S**ingle Responsibility
   - **O**pen/Closed
   - **L**iskov Substitution
   - **I**nterface Segregation
   - **D**ependency Inversion

4. Optimise les performances :
   - Élimination des calculs redondants
   - Mise en cache appropriée
   - Réduction de la complexité algorithmique

5. Améliore la lisibilité :
   - Nommage explicite
   - Commentaires uniquement pour le "pourquoi"
   - Extraction de fonctions pour clarté

## Format de Sortie

### Analyse

**Problèmes identifiés :**
1. [Problème 1] - Sévérité : [Haute/Moyenne/Faible]
2. [Problème 2] - Sévérité : [Haute/Moyenne/Faible]

### Code Refactorisé

```[language]
// Code refactorisé avec commentaires explicatifs
```

### Justifications

Pour chaque changement majeur :
- **Avant** : [Description du problème]
- **Après** : [Description de la solution]
- **Bénéfice** : [Amélioration apportée]

### Tests Requis

Liste des tests à mettre à jour/ajouter suite au refactoring.
```

## Organisation des Prompt Files

### Structure du Dossier `.github/prompts/`

```
.github/prompts/
├── code-generation/
│   ├── generate-component.prompt.md
│   ├── generate-api.prompt.md
│   └── generate-tests.prompt.md
├── code-review/
│   ├── security-review.prompt.md
│   ├── performance-review.prompt.md
│   └── code-quality.prompt.md
├── refactoring/
│   ├── refactor-code.prompt.md
│   ├── extract-function.prompt.md
│   └── simplify-logic.prompt.md
└── documentation/
    ├── generate-readme.prompt.md
    ├── generate-api-docs.prompt.md
    └── generate-comments.prompt.md
```

**Règles d'organisation :**
- ✅ Grouper par catégorie fonctionnelle
- ✅ Nommer les fichiers de manière descriptive
- ✅ Utiliser des sous-dossiers pour grande quantité de prompts
- ✅ Éviter la duplication (référencer plutôt)

## Utilisation des Prompt Files

### Invocation dans le Chat

```
# Format de base
/nom-du-prompt

# Avec arguments
/generate-form UserForm name:string email:email

# Avec contexte additionnel
/security-review for authentication module
```

### Test et Itération

1. ✅ Ouvrir le fichier `.prompt.md` dans l'éditeur
2. ✅ Cliquer sur le bouton "play" dans la barre de titre
3. ✅ Choisir de lancer dans session actuelle ou nouvelle
4. ✅ Tester le résultat
5. ✅ Affiner le prompt selon les résultats
6. ✅ Re-tester jusqu'à satisfaction

**Boucle d'amélioration :**
```
Créer prompt → Tester → Analyser résultat → Affiner → Re-tester
```

### Recommandations de Prompts

Configurer les prompts recommandés au démarrage d'une session :

```json
// settings.json
{
  "chat.promptFilesRecommendations": [
    "code-review",
    "generate-tests",
    "refactor-code"
  ]
}
```

## Bonnes Pratiques

### Do's ✅

1. ✅ **Spécifier `model: claude-sonnet-4.5`** TOUJOURS
2. ✅ **Décrire clairement l'objectif** dans description et corps
3. ✅ **Utiliser des variables** pour flexibilité (`${input:}`, `${selection}`)
4. ✅ **Référencer les instructions** plutôt que dupliquer
5. ✅ **Fournir des exemples** d'entrée/sortie attendus
6. ✅ **Structurer le prompt** avec sections claires
7. ✅ **Tester régulièrement** avec le bouton play
8. ✅ **Documenter les arguments** avec `argument-hint`
9. ✅ **Spécifier les outils** nécessaires dans `tools`
10. ✅ **Versionner les prompts** dans le contrôle de version

### Don'ts ❌

1. ❌ **Ne PAS omettre `model: claude-sonnet-4.5`**
2. ❌ **Ne PAS dupliquer** les instructions existantes
3. ❌ **Ne PAS créer** de prompts trop génériques
4. ❌ **Ne PAS oublier** de tester avant commit
5. ❌ **Ne PAS utiliser** de chemins absolus dans références
6. ❌ **Ne PAS mélanger** plusieurs responsabilités dans un prompt
7. ❌ **Ne PAS créer** de prompts sans description claire
8. ❌ **Ne PAS ignorer** les retours d'utilisation
9. ❌ **Ne PAS surcharger** le prompt avec trop d'instructions
10. ❌ **Ne PAS oublier** les variables pour réutilisabilité

## Checklist de Validation

Avant de committer un prompt file :

### Structure
- [ ] Extension `.prompt.md` présente
- [ ] Frontmatter YAML valide avec délimiteurs `---`
- [ ] Champ `description` présent et descriptif
- [ ] Champ `name` présent en `kebab-case`
- [ ] Champ `model: claude-sonnet-4.5` présent
- [ ] Champ `agent` spécifié (recommandé)

### Contenu
- [ ] Instructions claires et directives
- [ ] Sections organisées logiquement
- [ ] Format de sortie attendu spécifié
- [ ] Exemples fournis (si pertinent)
- [ ] Variables utilisées pour flexibilité
- [ ] Références aux fichiers valides (chemins relatifs)

### Qualité
- [ ] Prompt testé avec bouton play
- [ ] Résultats conformes aux attentes
- [ ] Pas de duplication avec autres prompts
- [ ] Pas de duplication d'instructions existantes
- [ ] Documentation suffisante pour utilisateurs
- [ ] Nom et description clairs pour découvrabilité

### Technique
- [ ] Outils (`tools`) spécifiés si nécessaires
- [ ] Variables correctement formatées `${variable}`
- [ ] Liens Markdown valides et relatifs
- [ ] Syntaxe `#tool:nom` correcte pour outils
- [ ] Pas de hardcoding de valeurs réutilisables

## Ressources

### Documentation Officielle Microsoft
- [Prompt Files - VS Code](https://code.visualstudio.com/docs/copilot/customization/prompt-files)
- [Customize AI Responses - Overview](https://code.visualstudio.com/docs/copilot/customization/overview)
- [Custom Instructions](https://code.visualstudio.com/docs/copilot/customization/custom-instructions)
- [Chat Tools](https://code.visualstudio.com/docs/copilot/chat/chat-tools)

### Exemples Communautaires
- [Awesome Copilot - Prompts](https://github.com/github/awesome-copilot/tree/main)

### Configuration VS Code
- Setting : `chat.promptFiles`
- Setting : `chat.promptFilesLocations`
- Setting : `chat.promptFilesRecommendations`
- Commande : `Chat: New Prompt File`
- Commande : `Chat: Configure Prompt Files`
- Commande : `Chat: Run Prompt`
