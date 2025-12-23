---
applyTo: "documentations/functionnals/**/*.md,documentations/functionnals/*.md"
---

# Instructions Globales pour la Documentation Fonctionnelle

## Objectif
La documentation fonctionnelle constitue la référence complète des besoins métier du projet. Elle doit être exhaustive, pragmatique, actionnable et maintenue à jour tout au long du cycle de vie du projet.

## Principes Fondamentaux

### 1. Langue et Ton
- **Toujours écrire en français** avec un vocabulaire précis et professionnel
- **Ton pragmatique** : focus sur l'actionnable, pas de théorie abstraite
- **Ton descriptif** : décrire avec précision les besoins, workflows, règles métier
- **Ton argumentatif** : justifier les choix fonctionnels, expliquer le pourquoi
- **Ton précis** : éviter les ambiguïtés, fournir des exemples concrets
- **Ton logique** : structuration claire, enchaînement cohérent des idées
- **Ton professionnel** : formel mais accessible, pas de jargon inutile

### 2. Agnosticisme Technologique
- **Rester agnostique de la technologie** : décrire le QUOI (besoin métier), pas le COMMENT (implémentation technique)
- **Focus métier** : partir du problème business, pas de la solution technique
- **Exceptions** : mentionner les contraintes techniques uniquement si elles impactent le fonctionnel (performance, volumétrie, intégrations)

**Exemples** :

❌ **Mauvais (trop technique)** :
> "Le système doit utiliser React pour afficher une table paginée avec MudDataGrid et stocker les données en SQL Server avec Entity Framework"

✅ **Bon (fonctionnel)** :
> "Le système doit afficher la liste des ressources avec pagination (25 éléments par page par défaut), filtrage par statut et compétences, et tri par nom ou date de création"

### 3. Organisation Hiérarchique

#### Structure Obligatoire
```
documentations/functionnals/
├── README.md                                    # Index et navigation
├── {persona1}.personna.md                       # Personas
├── {persona2}.personna.md
├── EPIC-0001-{nom}/                             # Dossier par EPIC
│   ├── EPIC-0001-{nom}.epic.md                  # Document EPIC
│   ├── FEATURE-0001-{nom}/                      # Dossier par FEATURE
│   │   ├── FEATURE-0001-{nom}.feature.md        # Document FEATURE
│   │   ├── US-0001-{nom}.us.md                  # User Stories
│   │   ├── US-0002-{nom}.us.md
│   │   └── US-0003-{nom}.us.md
│   └── FEATURE-0002-{nom}/
│       ├── FEATURE-0002-{nom}.feature.md
│       └── ...
└── EPIC-0002-{nom}/
    └── ...
```

#### Nomenclature Stricte
- **EPIC** : `EPIC-{ID:4 digits}-{slug-en-kebab-case}.epic.md`
- **FEATURE** : `FEATURE-{ID:4 digits}-{slug-en-kebab-case}.feature.md`
- **USER STORY** : `US-{ID:4 digits}-{slug-en-kebab-case}.us.md`
- **PERSONA** : `{prenom-nom}.personna.md` ou `{role-metier}.personna.md`

**Règles d'ID** :
- IDs uniques, séquentiels, ne JAMAIS réutiliser
- Pérennité : ne JAMAIS supprimer un fichier (marquer [DEPRECATED] si obsolète)

### 4. Modularité et Découpage

#### Diviser par Domaines, Concerns, Subconcerns
- **Domaine** = Epic : ex. "Gestion des Ressources", "Gestion des Allocations"
- **Concern** = Feature : ex. "Créer une Ressource", "Consulter les Allocations"
- **Subconcern** = User Story : ex. "Créer ressource simple", "Valider unicité email"

#### Principe de Responsabilité Unique
- **Une EPIC** = un domaine fonctionnel cohérent
- **Une FEATURE** = une fonctionnalité déployable apportant une valeur
- **Une USER STORY** = une action atomique et testable

#### Éviter la Duplication
- Lire systématiquement l'ensemble des fichiers `documentations/functionnals/**/*.md` avant de rédiger
- Identifier les duplications potentielles (règles métier, entités, workflows)
- Référencer plutôt que dupliquer : utiliser des liens entre documents
- Mutualiser les définitions (glossaire, entités communes)

**Exemple de Référencement** :
```markdown
## Règles Métier
Voir les règles de validation de l'email dans [US-0001-creer-ressource-simple](../US-0001-creer-ressource-simple.us.md#règles-métier-spécifiques)
```

### 5. Cohérence et Consistance

#### Cohérence Terminologique
- Utiliser un vocabulaire métier homogène dans tous les documents
- Créer et maintenir un glossaire métier (dans README.md)
- Éviter les synonymes pour un même concept

**Exemple** :
- ✅ Toujours utiliser "Ressource" (pas "Collaborateur", "Employé", "Consultant")
- ✅ Toujours utiliser "Allocation" (pas "Affectation", "Assignment")

#### Cohérence des Entités
- Définir les entités une seule fois (dans l'EPIC ou une feature de référence)
- Maintenir la cohérence des propriétés à travers tous les documents
- Documenter les relations entre entités

#### Cohérence des Messages
- Spécifier les messages utilisateur une seule fois
- Référencer les messages plutôt que les redéfinir
- Utiliser des codes d'erreur uniques et traçables

### 6. Types de Documents et Leurs Rôles

#### Personas (*.personna.md)
**Rôle** : Définir les archétypes d'utilisateurs  
**Contenu** : Identité, contexte professionnel, besoins, frustrations, objectifs, scénarios d'usage  
**Instructions détaillées** : Voir `documentation.personna.instructions.md`

#### EPICs (*.epic.md)
**Rôle** : Vision macro d'un domaine fonctionnel  
**Contenu** : Vision, contexte métier, personas, périmètre, KPIs, valeur business, features associées, phases de déploiement  
**Instructions détaillées** : Voir `documentation.epic.instructions.md`

#### FEATUREs (*.feature.md)
**Rôle** : Fonctionnalité précise et déployable  
**Contenu** : Description, workflows, scénarios (nominal/alternatif/erreur), règles métier, données, actions, autorisations, performances, user stories  
**Instructions détaillées** : Voir `documentation.feature.instructions.md`

#### USER STORies (*.us.md)
**Rôle** : Action atomique et testable  
**Contenu** : User story format, workflow détaillé, scénarios (nominal/alternatif/erreur/limites), règles métier, données, interface, critères d'acceptation, tests  
**Instructions détaillées** : Voir `documentation.us.instructions.md`

## Workflow de Rédaction

### Phase 1 : Analyse et Préparation
1. **Lire exhaustivement** tous les fichiers existants dans `documentations/functionnals/`
2. **Identifier** :
   - Les EPICs, FEATUREs, US existantes
   - Les IDs déjà utilisés (pour garantir l'unicité)
   - Les incohérences ou duplications potentielles
   - Les dépendances entre fonctionnalités
3. **Cartographier** les personas existants et leurs besoins

### Phase 2 : Rédaction
1. **Commencer par les Personas** : définir les utilisateurs cibles avant les fonctionnalités
2. **Puis les EPICs** : définir la vision macro de chaque domaine
3. **Ensuite les FEATUREs** : découper chaque EPIC en fonctionnalités déployables
4. **Enfin les User Stories** : découper chaque FEATURE en actions atomiques

### Phase 3 : Validation et Cohérence
1. **Vérifier la cohérence** :
   - Terminologie homogène
   - Entités définies de manière cohérente
   - Messages utilisateur cohérents
   - Dépendances bidirectionnelles (A dépend de B ⇔ B bloque A)
2. **Éliminer les duplications** :
   - Règles métier dupliquées → Centraliser et référencer
   - Définitions d'entités multiples → Une seule définition de référence
3. **Valider avec les parties prenantes** :
   - Revue avec le Product Owner
   - Validation avec les utilisateurs finaux (personas)
   - Approbation par le sponsor business

### Phase 4 : Maintenance Continue
1. **Mettre à jour régulièrement** :
   - Après chaque Sprint si modifications
   - Après feedback utilisateurs
   - Lors de changements de périmètre
2. **Maintenir l'index** (README.md) à jour
3. **Versionner les documents** (SemVer : Major.Minor.Patch)

## Règles de Qualité

### Complétude
- [ ] Chaque EPIC a au moins 3 FEATUREs
- [ ] Chaque FEATURE a au moins 3 User Stories
- [ ] Chaque US a au moins 10 critères d'acceptation
- [ ] Scénarios nominaux, alternatifs, erreurs et limites couverts
- [ ] Règles métier explicites et numérotées
- [ ] Données détaillées avec exemples JSON
- [ ] Autorisations et sécurité documentées

### Testabilité
- [ ] Critères d'acceptation SMART (Spécifiques, Mesurables, Actionnables, Réalistes, Testables)
- [ ] Tests fonctionnels avec données de test concrètes
- [ ] Résultats attendus observables et vérifiables
- [ ] Cas d'erreur avec codes et messages exacts

### Pragmatisme
- [ ] Objectifs réalistes et atteignables
- [ ] Périmètre raisonnable
- [ ] Gains mesurables et quantifiés
- [ ] Risques identifiés avec mitigations

## Index et Navigation (README.md)

### Structure du README.md
```markdown
# Index des Spécifications Fonctionnelles

## Vue d'Ensemble
Ce dossier contient l'ensemble des spécifications fonctionnelles du projet {Nom Projet}, organisées selon une structure hiérarchique : EPIC → FEATURE → USER STORY.

## Glossaire Métier
| Terme | Définition | Synonymes | Exemple |
|-------|------------|-----------|---------|
| Ressource | Collaborateur assignable à un projet | Consultant, Collaborateur | Jean Dupont, Développeur Java Senior |
| Allocation | Affectation d'une ressource à un projet sur une période | Affectation, Assignment | Marie Dupont allouée 50% sur Projet X du 01/01 au 31/03 |

## Personas
| Persona | Rôle | Fichier |
|---------|------|---------|
| Marie Dupont | Responsable RH | [marie-dupont.personna.md](marie-dupont.personna.md) |
| Jean Martin | Chef de Projet | [jean-martin.personna.md](jean-martin.personna.md) |

## EPICs
| ID | Titre | Priorité | Statut | Features | US | Dernière MàJ |
|----|-------|----------|--------|----------|----|--------------| 
| EPIC-0001 | Gestion des Utilisateurs | HAUTE | IN_PROGRESS | 3 | 8 | 2024-11-20 |
| EPIC-0002 | Gestion des Ressources | CRITIQUE | IN_PROGRESS | 5 | 15 | 2024-11-21 |

## Navigation par Statut
### DRAFT (En cours d'élaboration)
- EPIC-0003 : Gestion des Projets

### IN_PROGRESS (En cours de développement)
- EPIC-0001 : Gestion des Utilisateurs
- EPIC-0002 : Gestion des Ressources

### DONE (Terminées)
- Aucune pour le moment

## Graphe de Dépendances
\`\`\`mermaid
graph LR
    EPIC-0001[EPIC-0001: Utilisateurs] --> EPIC-0002[EPIC-0002: Ressources]
    EPIC-0002 --> EPIC-0004[EPIC-0004: Allocations]
    EPIC-0003[EPIC-0003: Projets] --> EPIC-0004
\`\`\`

## Règles de Gestion
### Création
1. Toujours créer une nouvelle EPIC/FEATURE/US plutôt que modifier une existante
2. Incrémenter les IDs de manière séquentielle
3. Respecter la nomenclature stricte
4. Utiliser les templates définis dans `.github/instructions/`

### Modification
1. Ne JAMAIS supprimer un fichier de spécification
2. Marquer [DEPRECATED] les spécifications obsolètes
3. Documenter les modifications dans la section "Historique"
4. Incrémenter la version (SemVer)

### Dépréciation
1. Mettre le statut à [DEPRECATED]
2. Ajouter une note en haut du fichier avec raison et remplacement
3. Ne PAS supprimer le fichier (traçabilité)
4. Mettre à jour cet index

## Statistiques
- **EPICs** : {X} (Y DONE, Z IN_PROGRESS, W DRAFT)
- **FEATUREs** : {X} (Y DONE, Z IN_PROGRESS, W DRAFT)
- **User Stories** : {X} (Y DONE, Z IN_PROGRESS, W DRAFT)
- **Personas** : {X}
- **Taux de complétion** : {X%}

## Historique des Modifications
| Date | Auteur | Modifications |
|------|--------|---------------|
| 2024-11-20 | Marie Dupont | Création de la structure initiale |
| 2024-11-21 | Jean Martin | Ajout EPIC-0002 et 5 features |
```

## Bonnes Pratiques Globales

### ✅ À Faire
- Impliquer les parties prenantes dès le début
- Valider chaque niveau (EPIC → FEATURE → US) avant de passer au suivant
- Utiliser des exemples de données réelles (anonymisées)
- Réviser régulièrement avec les utilisateurs finaux
- Maintenir la cohérence terminologique via un glossaire
- Documenter les décisions métier et leurs justifications
- Versionner systématiquement (SemVer)
- Tracer toutes les modifications dans l'historique

### ❌ À Éviter
- Rédiger des spécifications trop techniques (mélanger fonctionnel et technique)
- Dupliquer des informations entre documents (préférer les références)
- Utiliser des termes ambigus ou du jargon inaccessible
- Oublier les autorisations et la sécurité
- Ignorer les cas d'erreur et les cas limites
- Spécifier des messages d'erreur vagues ("Erreur", "Données invalides")
- Supprimer des fichiers obsolètes (toujours marquer [DEPRECATED])
- Créer trop de personas (5-7 maximum)

## Critères de Validation Globale

### Cohérence
- [ ] Vocabulaire métier homogène (glossaire respecté)
- [ ] Entités définies de manière cohérente dans tous les documents
- [ ] Messages utilisateur cohérents
- [ ] Dépendances bidirectionnelles (A dépend de B ⇔ B bloque A)
- [ ] Pas de contradiction entre spécifications

### Traçabilité
- [ ] Chaque FEATURE est rattachée à un EPIC
- [ ] Chaque USER STORY est rattachée à une FEATURE
- [ ] IDs uniques et séquentiels
- [ ] Liens entre documents fonctionnels
- [ ] Références vers ADR techniques si pertinent

### Qualité
- [ ] Français correct, ton professionnel
- [ ] Agnostique de la technologie
- [ ] Exhaustivité : tous les aspects fonctionnels couverts
- [ ] Testabilité : critères d'acceptation SMART, tests détaillés
- [ ] Pragmatisme : objectifs réalistes, gains mesurables

## Outils et Automatisation (Optionnels)

### Scripts de Validation
Créer des scripts pour :
- Vérifier l'unicité des IDs
- Détecter les liens cassés entre documents
- Valider la présence des sections obligatoires
- Vérifier la cohérence des dépendances
- Générer des rapports de couverture

### Génération Automatique
- Index README.md mis à jour automatiquement
- Graphes de dépendances (Mermaid)
- Statistiques de complétion
- Exports (Confluence, PDF, Excel)

## Références Croisées

Pour chaque type de document, référez-vous aux instructions détaillées :
- **Personas** : `.github/instructions/documentation.personna.instructions.md`
- **EPICs** : `.github/instructions/documentation.epic.instructions.md`
- **FEATUREs** : `.github/instructions/documentation.feature.instructions.md`
- **User Stories** : `.github/instructions/documentation.us.instructions.md`

## Contact et Support

Pour toute question sur les spécifications fonctionnelles :
- **Product Owner** : {Nom} - {Email}
- **Business Analyst** : {Nom} - {Email}
- **Documentation** : `.github/instructions/`