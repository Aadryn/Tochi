---
description: Rédaction du contenu - ton directif, exemples concrets, et formatage
name: Copilot_Instructions_Content
applyTo: "**/.github/instructions/*.instructions.md"
---

# Rédaction du Contenu des Instructions Copilot

Guide pour rédiger des instructions claires, directives, actionnables avec exemples concrets.

## ⛔ À NE PAS FAIRE

- **N'utilise jamais** de formulations vagues ("il serait bien de", "penser à")
- **N'écris jamais** de règle sans exemple concret
- **Ne mélange jamais** plusieurs concepts dans une seule instruction
- **N'omets jamais** les exemples ✅ BON et ❌ MAUVAIS
- **N'utilise jamais** de conditionnel faible ("pourrait", "devrait peut-être")
- **Ne laisse jamais** d'instruction sans action immédiatement applicable
- **N'écris jamais** de blocs de code sans contextualiser

## ✅ À FAIRE

- **Utilise toujours** un ton directif et impératif (DOIT, TOUJOURS, JAMAIS)
- **Écris toujours** des instructions actionnables immédiatement
- **Fournis toujours** un exemple ✅ BON et un exemple ❌ MAUVAIS
- **Structure toujours** avec des listes numérotées ou à puces
- **Utilise toujours** des blocs de code avec le langage spécifié
- **Explique toujours** le POURQUOI derrière chaque règle
- **Garde toujours** chaque instruction focalisée sur un seul concept

## 🎯 Actions Obligatoires (Mandatory)

**Style de rédaction à TOUJOURS appliquer :**

1. ✅ **Ton directif et impératif** : DOIT, TOUJOURS, JAMAIS, OBLIGATOIRE
   - ✅ Bon : `"La task DOIT avoir un nom descriptif"`
   - ❌ Mauvais : `"Il serait bien d'avoir un nom descriptif"`

2. ✅ **Instructions actionnables** : Chaque règle DOIT être applicable immédiatement
   - ✅ Bon : `"Utiliser 4 espaces pour l'indentation"`
   - ❌ Mauvais : `"Penser à l'indentation"`

3. ✅ **Exemples ✅/❌ OBLIGATOIRES** : TOUJOURS montrer bon ET mauvais exemple
   ```yaml
   # ✅ Bon exemple
   correct_code: value
   
   # ❌ Mauvais exemple
   wrong_code: bad
   ```

4. ✅ **Précision absolue** : JAMAIS d'ambiguïté ni de généralisation vague
   - ✅ Bon : `"Préfixer TOUTES les variables par le nom du rôle"`
   - ❌ Mauvais : `"Utiliser des conventions de nommage cohérentes"`

5. ✅ **Contextualiser** : Expliquer le "pourquoi" en plus du "comment"

6. ✅ **Blocs de code complets** : Toujours spécifier le langage
   ````markdown
   ```yaml
   code: here
   ```
   ````

7. ✅ **Numérotation cohérente** : Règles obligatoires numérotées 1, 2, 3...

8. ✅ **Checklist finale** : Fournir une liste de validation en fin de document

## Ton et Style

### Impératif et Directif

**TOUJOURS utiliser des verbes d'action à l'impératif :**

✅ **Verbes recommandés :**
- DOIT / NE DOIT PAS
- TOUJOURS / JAMAIS
- OBLIGATOIRE / INTERDIT
- Créer, Utiliser, Définir, Valider, Vérifier
- Éviter, Privilégier, Préférer

❌ **Formulations à éviter :**
- "Il serait bien de..."
- "On pourrait..."
- "Essayer de..."
- "Penser à..."
- "Considérer..."

### Exemples Comparatifs

```markdown
# ❌ MAUVAIS - Ton suggestif
Les noms de variables devraient être descriptifs.
On pourrait utiliser des préfixes pour éviter les conflits.

# ✅ BON - Ton directif
1. ✅ **Variables descriptives OBLIGATOIRES** : Chaque variable DOIT avoir un nom explicite
   ```yaml
   # ✅ Bon
   nginx_worker_processes: 4
   
   # ❌ Mauvais
   workers: 4
   ```

2. ✅ **Préfixe OBLIGATOIRE** : TOUTES les variables DOIVENT être préfixées par le nom du rôle
```

### Pragmatisme et Actionnabilité

**Chaque instruction DOIT être immédiatement applicable.**

```markdown
# ❌ MAUVAIS - Vague et non actionnable
- Écrire du bon code
- Suivre les bonnes pratiques
- Faire attention à la qualité

# ✅ BON - Précis et actionnable
1. ✅ **Nom descriptif** : Chaque task DOIT avoir un `name` décrivant l'état souhaité
2. ✅ **Idempotence** : Exécuter la task 2 fois DOIT donner le même résultat
3. ✅ **Modules natifs** : JAMAIS shell/command si un module Ansible existe
```

### Précision vs Généralisation

```markdown
# ❌ MAUVAIS - Généralisation vague
- Utiliser une indentation cohérente
- Organiser le code proprement
- Suivre les standards

# ✅ BON - Précision absolue
- Utiliser 4 espaces pour l'indentation (JAMAIS de tabs)
- Organiser les imports par ordre alphabétique, stdlib en premier
- Suivre PEP 8 pour Python : ligne max 88 caractères (Black)
```

## Exemples Concrets

### Structure des Exemples

**TOUJOURS fournir :**
1. ✅ Exemple de ce qu'il faut faire (préfixé ✅)
2. ❌ Exemple de ce qu'il faut éviter (préfixé ❌)
3. Explication contextuelle si nécessaire

### Format Standard

````markdown
## Nom de la Règle

Explication brève de la règle et son importance.

```[language]
# ✅ Bon - Description du pourquoi
example_correct: "value"
another_field: true

# ❌ Mauvais - Description du problème
wrong_example: "bad"
```

**Justification :** Pourquoi le premier est correct et le second incorrect.
````

### Exemples par Contexte

**Infrastructure as Code :**

```yaml
# ✅ Bon - Idempotent avec module déclaratif
- name: Ensure service configuration is present
  template:
    src: service.conf.j2
    dest: /etc/service/service.conf
  notify: Restart service

# ❌ Mauvais - Non idempotent, utilise commande impérative
- name: Configure service
  shell: cat service.conf > /etc/service/service.conf
```

**Langage typé :**

```typescript
/ ✅ Bon - Types explicites et documentation
function calculateTotal(price: number, quantity: number): number {
    /**
     * Calculate total price for items.
     * @param price - Unit price per item
     * @param quantity - Number of items
     * @returns Total price (price * quantity)
     */
    return price * quantity;
}

/ ❌ Mauvais - Pas de types, pas de documentation
function calculateTotal(price, quantity) {
    return price * quantity;
}
```

**Scripts Shell :**

```bash
# ✅ Bon - Validation des arguments et gestion d'erreurs
if [[ $# -lt 1 ]]; then
    echo "Usage: $0 <filename>" >&2
    exit 2
fi

if [[ ! -f "$1" ]]; then
    echo "Error: File '$1' not found" >&2
    exit 3
fi

# ❌ Mauvais - Pas de validation, erreurs silencieuses
cat "$1"
```

## Formatage Markdown

### Titres et Hiérarchie

```markdown
# Titre Principal (H1) - Une seule fois

## Section Principale (H2)

### Sous-section (H3)

#### Point Spécifique (H4) - Rarement utilisé
```

**Règles :**
- ✅ Un seul H1 par fichier
- ✅ H2 pour sections principales : "Actions Obligatoires", "Idempotence", "Sécurité"
- ✅ H3 pour détails dans une section
- ❌ Éviter H4+ (revoir la structure si nécessaire)

### Blocs de Code

**TOUJOURS spécifier le langage :**

````markdown
```yaml
ansible_code: here
```

```python
python_code = "here"
```

```bash
#!/bin/bash
bash_code="here"
```

```powershell
$PowerShellCode = "here"
```
````

**Langages courants :**
- `yaml` / `yml` - Configuration, IaC, pipelines
- `python` / `py` - Scripts et applications
- `javascript` / `js` - Frontend et Node.js
- `typescript` / `ts` - TypeScript
- `bash` / `sh` - Scripts Shell Unix/Linux
- `powershell` / `ps1` - Scripts PowerShell
- `batch` / `bat` - Scripts Windows
- `json` - Configuration JSON
- `ini` - Fichiers de configuration INI
- `dockerfile` - Containers
- `sql` - Requêtes et schémas
- `markdown` / `md` - Documentation

### Listes et Énumérations

**Listes non ordonnées :**

```markdown
- ✅ Élément 1
- ✅ Élément 2
  - Sous-élément A
  - Sous-élément B
- ✅ Élément 3
```

**Listes ordonnées (Actions Obligatoires) :**

```markdown
1. ✅ **Règle 1** : Description précise
2. ✅ **Règle 2** : Autre règle
3. ✅ **Règle 3** : Continue...
```

**Checklists (validation) :**

```markdown
## Checklist de Validation

- [ ] Critère 1 vérifié
- [ ] Critère 2 vérifié
- [ ] Critère 3 vérifié
```

### Emphase et Mise en Forme

```markdown
**Texte en gras** - Pour mots-clés importants
*Texte en italique* - Pour emphase légère
`code inline` - Pour noms de variables, fichiers, commandes
~~Texte barré~~ - Rarement utilisé

> Citation ou note importante
> Sur plusieurs lignes si nécessaire
```

**Utilisation recommandée :**

```markdown
La variable `nginx_worker_processes` DOIT être définie dans `defaults/main.yml`.
Le module **Ansible Vault** est OBLIGATOIRE pour les secrets.
```

### Emojis pour Sections Clés

**Utiliser avec parcimonie et cohérence :**

- 🎯 **Actions Obligatoires** - Section des règles essentielles
- ✅ **Bon exemple** - Ce qu'il faut faire
- ❌ **Mauvais exemple** - Ce qu'il faut éviter
- ⚠️ **Avertissement** - Point d'attention critique
- 💡 **Conseil / Astuce** - Recommandation utile
- 📋 **Checklist** - Liste de vérification
- 🔍 **Validation** - Tests et contrôles

**Exemples d'utilisation :**

```markdown
## 🎯 Actions Obligatoires (Mandatory)

## ⚠️ Avertissements Critiques

## 💡 Conseils Avancés

## 📋 Checklist de Validation
```

## Organisation du Contenu

### Section "Actions Obligatoires"

**Format standard :**

```markdown
## 🎯 Actions Obligatoires (Mandatory)

**À TOUJOURS [faire/respecter] lors de [contexte] :**

1. ✅ **Règle 1** : Description impérative avec exemple
   ```[language]
   # ✅ Bon
   good_example: value
   
   # ❌ Mauvais
   bad_example: wrong
   ```

2. ✅ **Règle 2** : Autre règle essentielle

3. ✅ **Règle 3** : Continue...
```

**Règles pour cette section :**
- ✅ 5-10 règles maximum (les plus critiques)
- ✅ Numérotation obligatoire
- ✅ Chaque règle avec titre en gras
- ✅ Exemples de code pour règles complexes
- ✅ Ton impératif : DOIT, TOUJOURS, JAMAIS

### Sections Thématiques

**Après les Actions Obligatoires, détailler par thème :**

```markdown
## Idempotence

### Principes de Base

[Explication conceptuelle]

### Exemples Pratiques

[Code avec comparaisons ✅/❌]

### Cas Particuliers

[Situations spécifiques]

## Gestion d'Erreurs

### Block/Rescue/Always

[Exemples structurés]

### Failed_when et Changed_when

[Cas d'usage concrets]
```

### Bonnes Pratiques vs Anti-Patterns

**Organiser en deux colonnes logiques :**

```markdown
## Bonnes Pratiques

- ✅ **Pratique 1** : Description et bénéfice
- ✅ **Pratique 2** : Quand et comment l'appliquer
- ✅ **Pratique 3** : Continue...

## Anti-Patterns à Éviter

- ❌ **Anti-pattern 1** : Pourquoi c'est problématique
- ❌ **Anti-pattern 2** : Alternative recommandée
- ❌ **Anti-pattern 3** : Impact sur la maintenabilité
```

### Checklist de Validation

**TOUJOURS en fin de document :**

```markdown
## Checklist de Validation

Avant de considérer le travail terminé :

### Structure
- [ ] Frontmatter YAML présent et valide
- [ ] Titre H1 unique et descriptif
- [ ] Section Actions Obligatoires en premier

### Contenu
- [ ] Règles numérotées et clairement définies
- [ ] Exemples ✅/❌ pour chaque règle importante
- [ ] Blocs de code avec langage spécifié

### Style
- [ ] Ton directif (DOIT, TOUJOURS, JAMAIS)
- [ ] Instructions actionnables et précises
- [ ] Pas d'ambiguïté ni de généralisation
```

### Références et Documentation

**En fin de document :**

```markdown
## Ressources et Références

### Documentation Officielle
- [Lien 1 - Titre descriptif](https:/example.com/doc1)
- [Lien 2 - Titre descriptif](https:/example.com/doc2)

### Standards et Conventions
- [Standard XYZ](https:/example.com/standard)

### Outils
- [Outil de validation](https:/example.com/tool)
```

## Contexte et Justification

### Expliquer le Pourquoi

**Ne pas se limiter au "comment", expliquer le "pourquoi" :**

```markdown
# ❌ MAUVAIS - Seulement le comment
Utiliser `lineinfile` au lieu de `shell`.

# ✅ BON - Comment + pourquoi
Utiliser `lineinfile` au lieu de `shell` pour garantir l'idempotence.
Le module `lineinfile` vérifie si la ligne existe avant de l'ajouter,
tandis que `shell` ajouterait la ligne à chaque exécution.
```

### Contextualiser les Règles

```markdown
## Préfixe des Variables

### Pourquoi ?

Les variables préfixées évitent les conflits de nommage entre rôles.
Un rôle `nginx` et un rôle `apache` peuvent tous deux avoir besoin
d'une variable `port`, mais `nginx_port` et `apache_port` sont distincts.

### Comment ?

```yaml
# ✅ Bon - Préfixe du rôle
nginx_worker_processes: 4
nginx_port: 80

# ❌ Mauvais - Risque de conflit
worker_processes: 4
port: 80
```
```

## Exemples Complets

### Exemple : Règle avec Contexte

```markdown
## Nommage des Tâches de Configuration

### Règle

✅ **Chaque tâche DOIT avoir un nom décrivant l'ÉTAT souhaité, pas l'action.**

### Pourquoi ?

Les outils de configuration déclaratifs sont orientés état. Le nom de la tâche doit
refléter l'état final attendu, pas l'action effectuée pour l'atteindre.
Cela améliore la lisibilité et la compréhension de la configuration.

### Exemples

```yaml
# ✅ BON - Décrit l'état
- name: Ensure service is installed and running
  package:
    name: service
    state: present

- name: Ensure application is deployed
  copy:
    src: app.tar.gz
    dest: /opt/app/

# ❌ MAUVAIS - Décrit l'action
- name: Install service
  package:
    name: service
    state: present

- name: Copy application
  copy:
    src: app.tar.gz
    dest: /opt/app/
```

### Impact

Un nom décrivant l'état facilite :
- La compréhension de la configuration sans lire le code
- Le débogage lors d'exécutions
- La documentation automatique
```

### Exemple : Section Thématique Complète

```markdown
## Idempotence

### Définition

Une opération idempotente peut être exécutée plusieurs fois sans changer
le résultat après la première exécution réussie. L'état du système
converge vers l'état désiré, puis reste stable.

### Principe

```
Exécution 1 : État initial → État désiré (changement effectué)
Exécution 2 : État désiré → État désiré (aucun changement)
Exécution N : État désiré → État désiré (aucun changement)
```

### Approche Déclarative (Idempotente)

```yaml
# ✅ IDEMPOTENT - Approche déclarative
- name: Ensure configuration file is present
  template:
    src: config.j2
    dest: /etc/app/config.yml
  # Si le fichier existe et est identique : pas de changement
  # Si le fichier diffère : mise à jour uniquement
```

### Approche Impérative (Non Idempotente)

```yaml
# ❌ NON IDEMPOTENT - Ajoute à chaque exécution
- name: Add configuration line
  shell: echo "setting=value" >> /etc/app/config

# ✅ VERSION IDEMPOTENTE
- name: Ensure configuration line is present
  lineinfile:
    path: /etc/app/config
    regexp: "^setting="
    line: "setting=value"
  # Vérifie si la ligne existe avant de l'ajouter
```

### Contrôler le Statut de Changement

```yaml
# Opération read-only - ne doit jamais être marquée comme changement
- name: Check application status
  command: /opt/app/bin/status
  register: app_status
  changed_when: false

# Opération avec détection conditionnelle
- name: Initialize database
  command: /opt/app/bin/init-db
  register: init_result
  changed_when: "'Database initialized' in init_result.stdout"
  failed_when:
    - init_result.rc != 0
    - "'already initialized' not in init_result.stderr"
```

### Test d'Idempotence

```bash
# Exécuter 2 fois, la 2ème DOIT avoir 0 changements
./configure.sh
./configure.sh  # Doit être sans effet

# Avec framework de test
test-idempotence  # Vérifie automatiquement
```
```

## Checklist de Validation du Contenu

- [ ] Ton directif utilisé (DOIT, TOUJOURS, JAMAIS)
- [ ] Instructions actionnables et précises
- [ ] Exemples ✅ (bon) fournis pour chaque règle
- [ ] Exemples ❌ (mauvais) fournis pour contraste
- [ ] Blocs de code avec langage spécifié
- [ ] Contexte et justification ("pourquoi") expliqués
- [ ] Organisation logique (général → spécifique)
- [ ] Pas d'ambiguïté ni de généralisation vague
- [ ] Checklist finale de validation présente
- [ ] Références et liens vers documentation officielle

## Ressources

### Documentation Officielle
- [Markdown Guide](https:/www.markdownguide.org/)
- [GitHub Flavored Markdown](https:/github.github.com/gfm/)

### Style Guides
- [Microsoft Writing Style Guide](https:/learn.microsoft.com/en-us/style-guide/welcome/)
- [Google Developer Documentation Style Guide](https:/developers.google.com/style)
