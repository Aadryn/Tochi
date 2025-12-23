---
description: Structure et métadonnées obligatoires des fichiers d'instructions
name: Copilot_Instructions_Structure
applyTo: "**/.github/instructions/*.instructions.md"
---

# Structure des Fichiers d'Instructions Copilot

Guide pour la structure, le frontmatter YAML et les métadonnées des fichiers `*.instructions.md`.

## 📚 Instructions Spécifiques Liées

| Fichier | Description | Fichiers ciblés |
|---------|-------------|----------------|
| [copilot.content.instructions.md](copilot.content.instructions.md) | Rédaction du contenu, ton directif | `*.instructions.md` |
| [copilot.agents.instructions.md](copilot.agents.instructions.md) | Création d'agents personnalisés | `agents/*.agent.md` |
| [copilot.prompts.instructions.md](copilot.prompts.instructions.md) | Prompt files réutilisables | `prompts/*.prompt.md` |

## ⛔ À NE PAS FAIRE

- **N'utilise jamais** d'extension autre que `.instructions.md`
- **N'omets jamais** le frontmatter YAML (description, name, applyTo)
- **Ne laisse jamais** le champ `description` vide ou générique
- **N'utilise jamais** d'espaces dans le champ `name`
- **N'oublie jamais** le champ `applyTo` avec le pattern glob
- **Ne crée jamais** de fichier sans titre H1 après le frontmatter
- **N'utilise jamais** de patterns `applyTo` trop larges (`**/*`)

## ✅ À FAIRE

- **Utilise toujours** l'extension `.instructions.md`
- **Définis toujours** le frontmatter YAML complet (description, name, applyTo)
- **Écris toujours** une description précise en 1 ligne
- **Nomme toujours** avec le pattern `technologie.aspect.instructions.md`
- **Spécifie toujours** `applyTo` avec un pattern glob précis
- **Ajoute toujours** un titre H1 descriptif après le frontmatter
- **Structure toujours** avec les sections ⛔/✅ puis les détails

## 🎯 Actions Obligatoires (Mandatory)

**Structure à TOUJOURS respecter :**

1. ✅ **Extension `.instructions.md`** : OBLIGATOIRE
   - ✅ Bon : `ansible.tasks.instructions.md`, `python.testing.instructions.md`
   - ❌ Mauvais : `instructions.md`, `tasks.md`, `guide.instructions.txt`

2. ✅ **Frontmatter YAML en premier** : TOUJOURS en début de fichier
   ```yaml
   ---
   description: Description courte (1 ligne, obligatoire)
   name: Nom_Unique_Sans_Espaces
   applyTo: "pattern/glob/**/*.ext"
   ---
   ```

3. ✅ **Champ `description`** : OBLIGATOIRE, 1 ligne, descriptif précis
   - ✅ Bon : `"Writing idempotent tasks with error handling and conditions"`
   - ❌ Mauvais : `"Instructions"`, `"Guide"`, `"Best practices"`

4. ✅ **Champ `name`** : OBLIGATOIRE, identifiant unique pour l'UI
   - Format : `Snake_Case` avec underscores
   - Préfixe recommandé : Technologie ou domaine
   - ✅ Bon : `Ansible_Tasks_Best_Practices`, `Python_Type_Hints`
   - ❌ Mauvais : `my instructions`, `guide`, `BestPractices`

5. ✅ **Champ `applyTo`** : OBLIGATOIRE, pattern glob précis
   - Relatif à la racine du workspace
   - Si omis, les instructions ne s'appliquent JAMAIS automatiquement
   - ✅ Bon : `"**/roles/**/*.{yml,yaml}"`, `"**/*.py"`
   - ❌ Mauvais : `"**"` (trop large), `"*.yml"` (pas récursif)

6. ✅ **Titre principal H1** : Immédiatement après le frontmatter
   ```markdown
   # Ansible Tasks - Guide Expert
   ```

7. ✅ **Section "Actions Obligatoires"** : TOUJOURS en premier après le titre
   ```markdown
   ## 🎯 Actions Obligatoires (Mandatory)
   
   **À TOUJOURS respecter lors de [contexte] :**
   ```

## Frontmatter YAML

### Champs Obligatoires

```yaml
---
description: Role development best practices - structure, tasks, variables, handlers
name: Ansible_Roles_Development
applyTo: "**/roles/**/*.{yml,yaml}"
---
```

### Règles pour `description`

- ✅ **Longueur** : 1 ligne, maximum 80 caractères
- ✅ **Contenu** : Décrire QUOI (pas comment)
- ✅ **Syntaxe** : Pas de guillemets dans le YAML (sauf si caractères spéciaux)
- ✅ **Langue** : Anglais pour cohérence (ou langue du projet)

**Exemples par domaine :**

```yaml
# Infrastructure as Code
description: Writing idempotent, robust tasks with proper error handling and conditions
description: Configuration management structure and best practices for orchestration
description: Static and dynamic inventory management, host groups, and variables

# Scripting
description: Best practices for shell scripting - robust, maintainable, and testable

# Testing
description: Testing, validation, linting, and CI/CD integration
```

### Règles pour `name`

- ✅ **Format** : `Snake_Case` avec underscores (pas de tirets, espaces, accents)
- ✅ **Unicité** : DOIT être unique dans tout le workspace
- ✅ **Structure** : `Domaine_Sujet_Type` (ex: `Ansible_Tasks_Best_Practices`)
- ✅ **Lisibilité** : Compréhensible dans l'UI de VS Code

**Convention de nommage :**

```yaml
# Pattern recommandé : Technologie_Composant_Type
name: IaC_Roles_Development
name: IaC_Tasks_Best_Practices
name: IaC_Playbooks_Structure
name: Language_Testing_Standards
name: Container_Compose_Configuration

# ❌ Mauvais
name: tech       # Trop vague
name: best-practices  # Tirets non supportés
name: Guide Tech   # Espaces interdits
```

### Règles pour `applyTo`

- ✅ **Guillemets obligatoires** : Toujours entre guillemets
- ✅ **Relatif au workspace** : Chemins relatifs à la racine
- ✅ **Récursivité** : `**` pour tous les sous-dossiers
- ✅ **Extensions multiples** : `{ext1,ext2}` pour grouper
- ✅ **Précision** : Le plus spécifique possible pour éviter les faux positifs

**Patterns par contexte :**

```yaml
# Infrastructure/Configuration - par composant
applyTo: "**/roles/**/*.{yml,yaml}"           # Tous les fichiers dans roles/
applyTo: "**/config/**/*.{yml,yaml}"          # Configuration
applyTo: "**/{tasks,handlers}/**/*.{yml,yaml}" # Tasks et handlers
applyTo: "**/inventory/**/*.{yml,yaml,ini}"   # Inventaires (multi-format)

# Scripts - par langage
applyTo: "**/*.{sh,bash}"                     # Scripts shell
applyTo: "**/*.{ps1,psm1}"                    # Scripts PowerShell
applyTo: "**/*.{bat,cmd}"                     # Scripts Windows

# Code - par contexte
applyTo: "**/tests/**/*.ext"                  # Tests
applyTo: "**/src/**/*.ext"                    # Code source
applyTo: "**/*.ext"                           # Tous les fichiers d'un type

# Multi-langages
applyTo: "**/*.{js,ts,jsx,tsx}"               # Frontend
applyTo: "**/*.{yml,yaml,json}"               # Configuration

# Répertoire spécifique
applyTo: "scripts/**/*.ext"                   # Seulement dans scripts/
applyTo: "components/*/src/*.ext"             # Sous-composants (1 niveau)
```

**Anti-patterns à éviter :**

```yaml
# ❌ Trop large - s'applique à TOUT
applyTo: "**"

# ❌ Pas de récursivité - 1 seul niveau
applyTo: "*.yml"

# ❌ Sans guillemets - erreur YAML
applyTo: **/*.py

# ❌ Espaces dans le pattern
applyTo: "** / *.yml"

# ❌ Pattern ambigu - trop de faux positifs
applyTo: "**/*test*"
```

## Organisation Hiérarchique

### Structure Standard d'un Fichier

```markdown
---
description: [Description obligatoire]
name: [Nom_Unique]
applyTo: "[pattern/glob]"
---

# [Titre Principal] - Guide Expert

## 🎯 Actions Obligatoires (Mandatory)

[5-10 règles essentielles numérotées]

## [Section Thématique 1]

### [Sous-section 1.1]

[Contenu avec exemples]

### [Sous-section 1.2]

## [Section Thématique 2]

## Checklist de Validation

[Liste des critères à vérifier]

## Ressources et Références

[Liens vers documentation officielle]
```

### Hiérarchie des Titres

```markdown
# Titre Principal (H1)                    # 1 seul par fichier
## Section Principale (H2)                # Grandes sections
### Sous-section (H3)                     # Détails d'une section
#### Point Spécifique (H4)                # Rarement utilisé
```

**Règles :**

- ✅ **Un seul H1** : Le titre principal du document
- ✅ **H2 pour sections** : Actions Obligatoires, puis sections thématiques
- ✅ **H3 pour sous-sections** : Détails dans une section
- ✅ **Éviter H4+** : Revoir la structure si nécessaire
- ✅ **Ordre logique** : Du général au spécifique

## Découpage Modulaire

### Principe de Responsabilité Unique

**Un fichier d'instructions = Un contexte précis**

✅ **Bon découpage (spécialisé) :**
```
iac.tasks.instructions.md           → Tasks IaC uniquement
iac.roles.instructions.md           → Rôles IaC uniquement
iac.orchestration.instructions.md   → Orchestration uniquement
iac.inventory.instructions.md       → Inventaires uniquement
iac.security.instructions.md        → Sécurité IaC (tous fichiers)
iac.testing.instructions.md         → Tests IaC (tous fichiers)
```

❌ **Mauvais découpage (trop large) :**
```
iac.instructions.md                 → Tout IaC (trop vague)
iac.best-practices.instructions.md  → Tout IaC (trop général)
```

### Critères de Découpage

**Créer un fichier séparé si :**

1. ✅ Le `applyTo` cible un pattern différent
2. ✅ Le contexte d'application est distinct (rôles ≠ playbooks ≠ tasks)
3. ✅ Les règles sont orthogonales (indépendantes)
4. ✅ La responsabilité est clairement délimitée

**Fusionner dans un seul fichier si :**

1. ❌ Le `applyTo` est identique
2. ❌ Les règles se chevauchent fortement
3. ❌ Le découpage créerait de la redondance
4. ❌ Les contextes sont trop similaires

### Exemples de Découpage

**Cas 1 : Infrastructure as Code - Découpage par composant**

```yaml
# iac.tasks.instructions.md
applyTo: "**/{tasks,handlers}/**/*.{yml,yaml}"

# iac.roles.instructions.md
applyTo: "**/roles/**/*.{yml,yaml}"

# iac.orchestration.instructions.md
applyTo: "**/playbooks/**/*.{yml,yaml}"
```
✅ **Justification** : Chaque composant a des règles spécifiques différentes

**Cas 2 : Infrastructure as Code - Découpage par préoccupation transverse**

```yaml
# iac.security.instructions.md
applyTo: "**/*.{yml,yaml}"  # Tous les fichiers IaC

# iac.testing.instructions.md
applyTo: "**/*.{yml,yaml}"  # Tous les fichiers IaC
```
✅ **Justification** : Sécurité et tests s'appliquent à TOUS les fichiers IaC

**Cas 3 : Scripts - Découpage par langage**

```yaml
# scripting.shell.instructions.md
applyTo: "**/*.{sh,bash}"

# scripting.powershell.instructions.md
applyTo: "**/*.{ps1,psm1}"

# scripting.windows.instructions.md
applyTo: "**/*.{bat,cmd}"
```
✅ **Alternative possible** : Un seul fichier `scripting.best-practices.instructions.md` avec `applyTo: "**/*.{sh,bash,bat,cmd,ps1,psm1}"` si les règles sont très similaires

## Nommage des Fichiers

### Convention Standard

**Format recommandé :** `domaine.composant.instructions.md`

```
[technologie].[contexte].[type].instructions.md
```

**Exemples :**

```
iac.tasks.instructions.md
iac.roles.instructions.md
iac.orchestration.instructions.md
iac.inventory.instructions.md
iac.security.instructions.md
iac.testing.instructions.md
language.testing.instructions.md
language.typing.instructions.md
container.compose.instructions.md
container.orchestration.instructions.md
scripting.best-practices.instructions.md
```

### Règles de Nommage

1. ✅ **Minuscules uniquement** : `ansible.tasks` pas `Ansible.Tasks`
2. ✅ **Points comme séparateurs** : `ansible.tasks` pas `ansible-tasks` ou `ansible_tasks`
3. ✅ **Extension `.instructions.md`** : OBLIGATOIRE
4. ✅ **Préfixe technologie** : Pour regroupement visuel
5. ✅ **Suffixe contexte** : Spécifier le composant ou préoccupation
6. ✅ **Descriptif et concis** : 2-3 mots maximum

**Anti-patterns :**

```
❌ instructions.md                    # Pas de contexte
❌ tech-tasks.md                      # Pas l'extension .instructions.md
❌ tech_tasks_best_practices.instructions.md  # Trop long, underscores
❌ Tech.Tasks.Instructions.md         # Majuscules
❌ tech.instructions.md               # Trop vague
```

## Checklist de Validation Structurelle

Avant de créer/modifier un fichier d'instructions :

### Frontmatter YAML
- [ ] Extension `.instructions.md` présente
- [ ] Bloc YAML frontmatter en début de fichier
- [ ] Délimiteurs `---` corrects (début et fin)
- [ ] Champ `description` présent, 1 ligne, descriptif
- [ ] Champ `name` présent, format `Snake_Case`
- [ ] Champ `applyTo` présent, pattern glob entre guillemets
- [ ] Pattern `applyTo` précis et testé
- [ ] Nom de fichier cohérent avec `name` et `applyTo`

### Structure du Document
- [ ] Un seul titre H1 après le frontmatter
- [ ] Section "🎯 Actions Obligatoires" en premier
- [ ] Hiérarchie des titres logique (H1 → H2 → H3)
- [ ] Organisation du général au spécifique
- [ ] Pas de H4/H5/H6 (restructurer si nécessaire)

### Modularité
- [ ] Le fichier a une responsabilité unique claire
- [ ] Le `applyTo` cible précisément le bon contexte
- [ ] Pas de duplication avec d'autres fichiers d'instructions
- [ ] Le découpage est justifié et pragmatique

## Ressources

### Documentation Officielle Microsoft
- [Custom Instructions - VS Code](https:/code.visualstudio.com/docs/copilot/customization/custom-instructions)
- [Instructions File Format](https:/code.visualstudio.com/docs/copilot/customization/custom-instructions#_instructions-file-format)

### Configuration VS Code
- Setting : `github.copilot.chat.codeGeneration.useInstructionFiles`
- Setting : `chat.instructionsFilesLocations`
- Commande : `Chat: New Instructions File`
