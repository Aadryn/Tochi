# Capitalisation de Connaissances - Obligation Morale de Documentation

**Applies To:** Tous les fichiers et activités du projet  
**Audience:** Agent IA, développeurs, architectes  
**Dernière mise à jour:** 2025-12-06

## PRINCIPE FONDAMENTAL

**OBLIGATION MORALE ABSOLUE** : Chaque leçon apprise, chaque connaissance acquise, chaque problème résolu DOIT être capitalisé formellement pour le bénéfice à long terme du projet et de l'équipe.

**La connaissance non documentée est une connaissance perdue.**

## QUAND CAPITALISER

### Situations Déclenchant IMPÉRATIVEMENT une Documentation

Documenter **IMMÉDIATEMENT** dans les cas suivants :

#### 1. Découverte Technique (→ ADR)
- **Pattern efficace** découvert ou validé
- **Contrainte système** identifiée (librairie, framework, runtime)
- **Solution technique** à un problème récurrent
- **Limite technique** découverte (performance, compatibilité)
- **Configuration spécifique** nécessaire pour faire fonctionner un composant
- **Workaround technique** mis en place

**Exemples concrets :**
- "MudDataGrid nécessite `@ref` pour appels impératifs"
- "Culture.Designer.cs auto-généré par MSBuild target PublicResXFileCodeGenerator"
- "bUnit nécessite `Services.AddLocalization()` pour tester composants avec `IStringLocalizer`"

#### 2. Règle Métier Identifiée (→ Requirements)
- **Comportement attendu** clarifié par utilisateur
- **Contrainte métier** découverte pendant le développement
- **Règle de gestion** identifiée implicitement
- **Validation métier** requise
- **Workflow métier** précisé
- **Cas limite métier** identifié

**Exemples concrets :**
- "Les prompts favoris doivent être accessibles en lecture seule si l'utilisateur n'est pas propriétaire"
- "La pagination doit conserver l'état de tri lors du changement de page"
- "Les statistiques d'utilisation sont calculées sur les 30 derniers jours par défaut"

#### 3. Bonne Pratique Validée (→ `.github/instructions/*.instructions.md`)
- **Convention de code** validée par l'équipe
- **Pattern d'architecture** récurrent dans le projet
- **Procédure de test** standardisée
- **Règle de nommage** établie
- **Structure de fichier** normalisée
- **Processus de développement** optimisé

**Exemples concrets :**
- "Tests bUnit : toujours créer TestContext dans `using` pour libération ressources"
- "Composants Razor : séparer code-behind (`.razor.cs`) du markup (`.razor`)"
- "Tests unitaires : un fichier `*Tests.cs` par classe testée avec nom identique"

#### 4. Problème Résolu avec Difficulté (→ ADR ou Requirements selon nature)
- **Bug subtil** identifié et corrigé
- **Erreur récurrente** avec solution non évidente
- **Dépannage complexe** ayant nécessité investigation
- **Régression évitée** grâce à une décision
- **Incident de parcours** avec apprentissage significatif

**Exemples concrets :**
- "Suppression accidentelle Culture.Designer.cs → récupération via script PowerShell"
- "Tests flaky à cause de l'ordre d'exécution → isolation complète requise"
- "Build warnings liés à nullable reference types → activer `<Nullable>enable</Nullable>`"

#### 5. Clarification Utilisateur (→ Requirements si métier, ADR si technique)
- **Spécification ambiguë** clarifiée
- **Comportement attendu** précisé
- **Périmètre fonctionnel** défini
- **Contrainte non documentée** révélée
- **Décision d'arbitrage** prise avec utilisateur

#### 6. Hypothèse Validée ou Invalidée (→ ADR ou Requirements)
- **Supposition technique** confirmée ou infirmée
- **Hypothèse métier** validée par tests ou utilisateur
- **Alternative évaluée** et rejetée (documenter POURQUOI)

## COMMENT CAPITALISER

### Choix du Support de Documentation

| Type de Connaissance | Support | Emplacement | Format |
|----------------------|---------|-------------|--------|
| **Décision architecturale** | ADR | `docs/adr/` | `{id}-{titre}.adr.md` |
| **Besoin métier/fonctionnel** | Requirements | `docs/requirements/{domaine}/` | `{id}-{titre}.requirements.md` |
| **Convention de code/process** | Instructions | `.github/instructions/` | `{scope}.{topic}.instructions.md` |
| **Leçon projet-spécifique** | ADR ou Instructions | Selon nature | Format approprié |

### Processus de Capitalisation (5 Étapes)

#### Étape 1 : IDENTIFIER
Dès qu'une information utile est découverte, **NOTER IMMÉDIATEMENT** :
- Nature de l'information (technique, métier, processus)
- Contexte de découverte (tâche, problème résolu, clarification)
- Impact potentiel sur le projet

#### Étape 2 : CLASSIFIER
Déterminer le type de documentation approprié :
- **Technique + Impact architecture** → ADR
- **Métier + Exigence fonctionnelle** → Requirements
- **Processus + Convention** → Instructions

#### Étape 3 : RÉDIGER
Créer le document avec :
- **Titre explicite** : Ce qu'on apprend doit être clair dans le titre
- **Contexte complet** : Situation ayant mené à cette connaissance
- **Décision/Règle/Pattern** : La connaissance elle-même, formalisée
- **Justification** : Pourquoi cette approche (alternatives considérées)
- **Conséquences** : Impact sur le projet (positif/négatif)
- **Références** : Liens vers code, discussions, documentation externe

#### Étape 4 : VALIDER
Avant de finaliser :
- [ ] Le titre est-il explicite et recherchable ?
- [ ] Le contexte est-il suffisant pour comprendre POURQUOI ?
- [ ] La décision/règle est-elle claire et actionnable ?
- [ ] Les alternatives sont-elles documentées ?
- [ ] Le document est-il cohérent avec corpus existant (pas de duplication ni contradiction) ?

#### Étape 5 : INTÉGRER
- Commiter le document avec message explicite
- Vérifier qu'il apparaît dans l'inventaire des instructions (pour `.instructions.md`)
- Si nécessaire, créer lien depuis documents connexes

## TEMPLATES RAPIDES

### ADR Minimal (Leçon Technique)
```markdown
# {ID}. {Leçon Apprise - Titre Court}

Date: {YYYY-MM-DD}
Statut: Accepté

## Contexte

[Situation ayant mené à cette découverte]

## Décision

[Ce qu'on a appris et décidé d'appliquer]

## Conséquences

### Positives
- [Bénéfice direct]

### Négatives
- [Compromis ou limitation]

## Références

- [Lien code/doc/discussion]
```

### Requirements Minimal (Règle Métier)
```markdown
# REQ-{id}. {Règle Métier Identifiée}

Date: YYYY-MM-DD

## Statut

Validé

## Contexte Métier

[Situation ayant révélé cette règle]

## Règles Métier

### RG-{id}.1 - {Titre}
- **Description** : [Règle claire et non ambiguë]
- **Justification** : [Pourquoi cette règle existe]

## Critères d'Acceptation

- [ ] [Critère testable 1]
- [ ] [Critère testable 2]
```

### Instruction Minimale (Convention/Processus)
```markdown
# {Scope}.{Topic} - {Titre Court}

**Applies To:** {Pattern de fichiers}  
**Dernière mise à jour:** {YYYY-MM-DD}

## Règle

[Énoncé clair de la convention/pratique]

## Justification

[Pourquoi cette règle (problème résolu, cohérence, qualité)]

## Exemples

### ✅ BON
\`\`\`
[Exemple correct]
\`\`\`

### ❌ MAUVAIS
\`\`\`
[Anti-pattern]
\`\`\`

## Références

- [Lien vers doc/code]
```

## EXEMPLES DE CAPITALISATION

### Exemple 1 : Leçon Technique Après Incident

**Situation** : Suppression accidentelle de `Culture.Designer.cs` lors du dépannage de ressources localisées.

**Action** : Créer ADR `023-resource-file-regeneration-strategy.adr.md`

**Contenu clé** :
- **Contexte** : Fichier `.Designer.cs` supprimé, 370 erreurs
- **Décision** : Ne jamais modifier manuellement `.Designer.cs`, utiliser targets MSBuild
- **Solution** : Script PowerShell de récupération (parse `.resx` → génère `.cs`)
- **Prévention** : Ajouter `.Designer.cs` au `.gitignore` si auto-généré

### Exemple 2 : Pattern de Test Validé

**Situation** : Tous les tests bUnit utilisent le même pattern de création `TestContext`.

**Action** : Créer instruction `bunit.testing.patterns.instructions.md`

**Contenu clé** :
- **Pattern** : `using var ctx = new TestContext(); ctx.Services.AddLocalization();`
- **Justification** : Isolation tests + libération ressources + cohérence
- **Anti-pattern** : `TestContext` réutilisé entre tests (state leak)

### Exemple 3 : Règle Métier Clarifiée

**Situation** : Utilisateur précise que les statistiques sont calculées sur période glissante de 30 jours.

**Action** : Créer/mettre à jour Requirements `040-usage-statistics.requirements.md`

**Contenu clé** :
- **Règle** : Période par défaut = 30 jours glissants depuis aujourd'hui
- **Justification** : Pertinence données récentes vs. historique complet
- **Critère** : `DateFin = DateTime.Today; DateDébut = DateFin.AddDays(-30);`

## ANTI-PATTERNS À ÉVITER

### ❌ NE PAS FAIRE

1. **Documenter "plus tard"** : La connaissance sera oubliée ou déformée
2. **Documentation verbale uniquement** : Les discussions sont volatiles
3. **Duplication d'information** : Vérifier corpus existant avant création
4. **Documentation vague** : "Il faut faire attention" n'est pas actionnable
5. **Ignorer les petites leçons** : L'accumulation de détails fait la différence
6. **Documentation sans contexte** : Expliquer POURQUOI, pas seulement QUOI

### ✅ FAIRE

1. **Documenter IMMÉDIATEMENT** après la découverte
2. **Être SPÉCIFIQUE et ACTIONNABLE** : Exemples concrets, critères mesurables
3. **CONTEXTUALISER** : Expliquer le problème résolu ou le besoin adressé
4. **RELIER** : Liens vers code, autres docs, discussions
5. **MAINTENIR** : Mettre à jour si connaissance évolue (statut "Remplacé par XXX")

## MÉTRIQUES DE CAPITALISATION

Pour mesurer l'efficacité de la capitalisation :

### Indicateurs de Qualité
- **Nombre de leçons capitalisées par tâche** : Objectif ≥ 1 par tâche non triviale
- **Taux de réutilisation ADR/Requirements** : Références dans commits/PRs
- **Réduction des incidents récurrents** : Moins de "déjà vu" non documenté
- **Temps de résolution diminué** : Problèmes déjà documentés résolus plus vite

### Processus de Revue
Lors de la complétion d'une tâche, **CHECKLIST OBLIGATOIRE** :
- [ ] Ai-je appris quelque chose de nouveau durant cette tâche ?
- [ ] Ai-je résolu un problème non trivial ?
- [ ] Ai-je reçu une clarification importante de l'utilisateur ?
- [ ] Ai-je identifié un pattern récurrent ?
- [ ] Ai-je créé un ADR/Requirements/Instruction pour capitaliser ?

**Si réponse "Oui" à une des 4 premières questions ET "Non" à la dernière → BLOQUER la tâche jusqu'à documentation créée.**

## RESPONSABILITÉ ET ENGAGEMENT

En tant qu'agent IA ou développeur sur ce projet, je m'engage à :

1. **Reconnaître** que la capitalisation est un investissement, pas une perte de temps
2. **Prioriser** la documentation des leçons apprises au même niveau que le code
3. **Traiter** chaque leçon non capitalisée comme une dette technique
4. **Contribuer** activement au corpus de connaissances du projet
5. **Maintenir** les documents existants quand une nouvelle leçon les complète ou contredit

**La qualité du projet se mesure autant à son code qu'à sa documentation de connaissances.**

---

**Références :**
- `.github/instructions/adr.documentation.instructions.md` - Format ADR
- `.github/instructions/requirements.documentation.instructions.md` - Format Requirements
- `.github/copilot-instructions.md` - Workflow général
