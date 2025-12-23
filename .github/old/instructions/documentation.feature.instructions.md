---
applyTo: "documentations/functionnals/**/*.feature.md,documentations/functionnals/*.feature.md"
---

# Instructions pour la Rédaction des FEATUREs

## Objectif
Une FEATURE représente une fonctionnalité précise et cohérente au sein d'une EPIC. Elle regroupe plusieurs User Stories et constitue un ensemble déployable apportant une valeur utilisateur concrète.

## Principes Directeurs
- **Toujours écrire en français** avec un ton pragmatique, descriptif, argumentatif, précis, logique et professionnel
- **Rester agnostique de la technologie** : décrire le QUOI fonctionnel, pas le COMMENT technique
- **Fournir des workflows détaillés** : scénarios nominaux, alternatifs et d'erreur
- **Être exhaustif** : règles métier, données, actions, autorisations, performances

## Nomenclature Obligatoire
- **Format de fichier** : `FEATURE-{ID:4 digits}-{slug-en-kebab-case}.feature.md`
- **Exemples** :
  - `FEATURE-0001-authentification.feature.md`
  - `FEATURE-0002-creer-ressource.feature.md`
  - `FEATURE-0003-consulter-allocations.feature.md`

## Structure Obligatoire du Document

### 1. Métadonnées
```markdown
# FEATURE-{ID} : {Titre Clair et Concis}

## Métadonnées
- **ID** : FEATURE-{ID}
- **EPIC Parent** : [EPIC-{ID} : {Titre}](../EPIC-{ID}-{slug}.epic.md)
- **Statut** : [DRAFT | IN_PROGRESS | DONE | DEPRECATED]
- **Priorité** : [CRITIQUE | HAUTE | MOYENNE | BASSE]
- **Date de création** : YYYY-MM-DD
- **Dernière mise à jour** : YYYY-MM-DD
- **Auteur** : {Nom ou rôle}
- **Version** : X.Y.Z
```

**Règles** :
- L'ID doit être unique, séquentiel et ne JAMAIS être réutilisé
- Toujours lier à l'EPIC parent avec un lien fonctionnel
- Le statut reflète l'avancement de toutes les US de cette feature
- Versioning sémantique appliqué

### 2. Description Fonctionnelle
```markdown
## Description Fonctionnelle
**En tant que** {utilisateur/rôle spécifique},  
**Je veux** {fonctionnalité précise},  
**Afin de** {bénéfice direct et mesurable}.

### Contexte
{Explication détaillée du besoin fonctionnel, du contexte d'usage, des situations déclenchantes}

### Objectifs
- {Objectif fonctionnel 1 : résultat attendu précis}
- {Objectif fonctionnel 2}
- {Objectif fonctionnel 3}
```

**Règles** :
- Format User Story obligatoire pour clarifier la valeur
- Le contexte explique POURQUOI cette feature est nécessaire
- Les objectifs sont mesurables et vérifiables
- Rester au niveau fonctionnel (pas d'implémentation technique)

**Exemple** :
```markdown
## Description Fonctionnelle
**En tant que** Responsable RH,  
**Je veux** créer une nouvelle ressource dans le système,  
**Afin de** pouvoir l'allouer à des projets et suivre sa disponibilité.

### Contexte
Lorsqu'un nouveau collaborateur rejoint l'entreprise ou qu'un collaborateur existant devient éligible à l'allocation sur projets, le Responsable RH doit l'enregistrer dans le système avec ses informations principales et ses compétences.

### Objectifs
- Permettre la saisie rapide des informations essentielles d'une ressource (< 2 minutes)
- Garantir l'unicité des ressources (pas de doublons)
- Valider la qualité des données saisies (format email, numéro de téléphone)
```

### 3. Personas Principaux
```markdown
## Personas Principaux
| Persona | Rôle | Actions Principales |
|---------|------|---------------------|
| {Nom Persona} | {Rôle métier} | {Action 1, Action 2, Action 3} |
```

**Règles** :
- Référencer les personas définis dans les fichiers `*.personna.md`
- Lister les actions principales que chaque persona effectuera
- Prioriser les personas (primaire vs secondaire)

### 4. Workflow Principal (Happy Path)
```markdown
## Workflow Principal (Happy Path)

\`\`\`mermaid
graph TD
    A[Démarrage : Utilisateur sur page X] --> B[Action 1 : Clic sur bouton Y]
    B --> C{Condition ?}
    C -->|Oui| D[Action 2 : Affichage formulaire]
    C -->|Non| E[Action 3 : Message d'erreur]
    D --> F[Action 4 : Saisie données]
    F --> G[Action 5 : Validation et enregistrement]
    G --> H[Fin : Confirmation et redirection]
\`\`\`

### Description Textuelle du Workflow
1. **Étape 1 : Accès à la fonctionnalité**
   - Pré-conditions : {Utilisateur connecté avec rôle X, sur la page Y}
   - Actions utilisateur : {Clic sur "Nouvelle Ressource"}
   - Résultat attendu : {Affichage du formulaire de création}
   - Post-conditions : {Formulaire vierge affiché avec champs par défaut}

2. **Étape 2 : Saisie des informations**
   - Pré-conditions : {Formulaire affiché}
   - Actions utilisateur : {Saisie Nom, Prénom, Email, etc.}
   - Résultat attendu : {Validation en temps réel des champs}
   - Post-conditions : {Données saisies temporairement en mémoire}

3. **Étape 3 : Validation et enregistrement**
   - Pré-conditions : {Tous les champs obligatoires remplis et valides}
   - Actions utilisateur : {Clic sur "Enregistrer"}
   - Résultat attendu : {Ressource créée, message de succès affiché}
   - Post-conditions : {Ressource persistée en base, redirection vers liste}
```

**Règles** :
- Utiliser Mermaid pour la visualisation (optionnel mais recommandé)
- Décrire chaque étape avec pré/post-conditions
- Être précis sur les actions utilisateur et les réactions système
- Documenter l'état du système à chaque étape

### 5. Scénarios
```markdown
## Scénarios

### Scénario 1 : Nominal (Happy Path)
**Contexte** : Responsable RH authentifié, aucune ressource avec l'email "john.doe@example.com" n'existe  
**Actions** :
1. L'utilisateur navigue vers "Ressources" > "Nouvelle Ressource"
2. Le système affiche le formulaire de création vierge
3. L'utilisateur saisit :
   - Prénom : "John"
   - Nom : "Doe"
   - Email : "john.doe@example.com"
   - Téléphone : "+33 6 12 34 56 78"
4. Le système valide les champs en temps réel (✅ tous valides)
5. L'utilisateur clique sur "Enregistrer"
6. Le système crée la ressource et affiche le message : "Ressource créée avec succès"
7. Le système redirige vers la liste des ressources

**Résultat attendu** : Ressource créée avec ID unique, visible dans la liste

### Scénario 2 : Alternatif - Annulation
**Contexte** : Utilisateur en cours de saisie du formulaire  
**Actions** :
1. L'utilisateur saisit partiellement le formulaire
2. L'utilisateur clique sur "Annuler"
3. Le système affiche une confirmation : "Voulez-vous vraiment annuler ? Les données non enregistrées seront perdues."
4. L'utilisateur confirme
5. Le système redirige vers la liste sans créer de ressource

**Résultat attendu** : Aucune ressource créée, retour à la liste

### Scénario 3 : Cas d'Erreur - Email déjà existant
**Contexte** : Une ressource avec l'email "john.doe@example.com" existe déjà  
**Actions** :
1. L'utilisateur saisit les informations avec l'email "john.doe@example.com"
2. L'utilisateur clique sur "Enregistrer"
3. Le système détecte le doublon

**Erreur déclenchée** : Violation de contrainte d'unicité  
**Message affiché** : "Une ressource avec cet email existe déjà. Veuillez utiliser un email différent ou consulter la ressource existante."  
**Action de récupération** : L'utilisateur peut corriger l'email ou annuler. Un lien vers la ressource existante est affiché.
```

**Règles** :
- Documenter AU MINIMUM : 1 scénario nominal + 2 alternatifs + 2 erreurs
- Utiliser des données d'exemple concrètes
- Spécifier les messages exacts affichés à l'utilisateur
- Décrire les actions de récupération possibles

### 6. Règles Métier
```markdown
## Règles Métier

### Règles de Validation
| ID | Règle | Condition | Message d'Erreur |
|----|-------|-----------|------------------|
| R-001 | L'email est obligatoire | Champ Email vide | "L'email est obligatoire" |
| R-002 | L'email doit être valide | Format email invalide | "Format d'email invalide (ex: nom@example.com)" |
| R-003 | L'email doit être unique | Email déjà existant en base | "Une ressource avec cet email existe déjà" |

### Règles de Calcul
| ID | Formule | Description | Exemple |
|----|---------|-------------|---------|
| C-001 | `TauxDisponibilite = (JoursDisponibles / JoursOuvrables) * 100` | Calcul du taux de disponibilité d'une ressource | Si 15 jours disponibles sur 20 jours ouvrables → 75% |

### Règles de Cohérence
- Une ressource ne peut pas avoir deux allocations qui se chevauchent sur la même période
- Le taux d'allocation cumulé d'une ressource ne peut pas dépasser 100% sur une période donnée
- Une compétence associée à une ressource doit exister dans le référentiel de compétences

### Règles Temporelles
- Une allocation ne peut pas avoir une date de fin antérieure à la date de début
- Une ressource créée aujourd'hui a une date de création = date du jour
- L'historique des modifications est conservé pendant 5 ans minimum
```

**Règles** :
- Identifier TOUTES les règles métier (validation, calcul, cohérence, temporelles)
- Numéroter les règles pour faciliter la traçabilité
- Spécifier les messages d'erreur exacts
- Fournir des exemples concrets pour les calculs

### 7. Données Manipulées
```markdown
## Données Manipulées

### Entités Principales
| Entité | Opérations | Champs Affichés | Champs Modifiables |
|--------|-----------|----------------|-------------------|
| Ressource | Création, Lecture | Prénom, Nom, Email, Téléphone, Statut | Prénom, Nom, Email, Téléphone |

### Propriétés Détaillées
#### Entité : Ressource
| Propriété | Type | Obligatoire | Valeur par Défaut | Validation | Exemple |
|-----------|------|-------------|-------------------|------------|---------|
| Id | GUID | Oui (auto) | Généré par système | - | "3fa85f64-5717-4562-b3fc-2c963f66afa6" |
| Prénom | Texte | Oui | - | Max 50 caractères, lettres et espaces uniquement | "Jean-Pierre" |
| Nom | Texte | Oui | - | Max 50 caractères, lettres et espaces uniquement | "Dupont" |
| Email | Email | Oui | - | Format email valide, unique | "jp.dupont@example.com" |
| Téléphone | Texte | Non | - | Format international ou national | "+33 6 12 34 56 78" |
| DateCreation | Date/Heure | Oui (auto) | Date/Heure actuelle | - | "2024-11-20T14:30:00Z" |
| Statut | Enum | Oui | "Active" | Valeurs : Active, Inactive, Archivée | "Active" |

### Relations entre Entités
\`\`\`mermaid
erDiagram
    RESSOURCE ||--o{ ALLOCATION : "est_allouée_via"
    RESSOURCE ||--o{ COMPETENCE : "possède"
    PROJET ||--o{ ALLOCATION : "nécessite"
\`\`\`
```

**Règles** :
- Lister toutes les entités manipulées par la feature
- Détailler les propriétés avec types, contraintes, exemples
- Documenter les relations (cardinalités)
- Fournir des exemples de données valides

### 8. Actions Utilisateur
```markdown
## Actions Utilisateur

### Actions Principales
| Action | Déclenchement | Pré-conditions | Résultat | Permissions Requises |
|--------|--------------|----------------|----------|---------------------|
| Créer Ressource | Clic sur "Nouvelle Ressource" | Utilisateur authentifié | Formulaire affiché | Role: RH Manager |
| Enregistrer | Clic sur "Enregistrer" | Formulaire valide | Ressource créée | Role: RH Manager |
| Annuler | Clic sur "Annuler" | En cours de saisie | Retour liste | Tous |

### Navigation
- **Depuis** : Liste des Ressources
- **Vers** : Formulaire Nouvelle Ressource → Liste des Ressources (après création)
- **Déclencheur** : Bouton "Nouvelle Ressource"
- **Contexte transmis** : Aucun (formulaire vierge)
```

**Règles** :
- Documenter toutes les actions disponibles
- Spécifier les déclencheurs (boutons, liens, événements)
- Définir les permissions requises pour chaque action
- Décrire les flux de navigation

### 9. Feedback Utilisateur
```markdown
## Feedback Utilisateur

### Messages de Succès
| Action | Message | Durée d'Affichage |
|--------|---------|-------------------|
| Création ressource | "Ressource créée avec succès" | 3 secondes (toast) |
| Modification ressource | "Ressource mise à jour" | 3 secondes |

### Messages d'Erreur
| Erreur | Message | Actions Correctives Suggérées |
|--------|---------|------------------------------|
| Email invalide | "Format d'email invalide (ex: nom@example.com)" | Corriger le format de l'email |
| Email existant | "Une ressource avec cet email existe déjà" | Utiliser un autre email ou consulter la ressource existante |
| Champ obligatoire vide | "Le champ {NomChamp} est obligatoire" | Remplir le champ |

### Messages d'Information
| Contexte | Message | Type |
|----------|---------|------|
| Formulaire modifié non enregistré | "Vous avez des modifications non enregistrées" | Warning |
| Validation en cours | "Vérification de l'email en cours..." | Info |
```

**Règles** :
- Spécifier les messages exacts (pas de paraphrase)
- Indiquer le type d'affichage (toast, modal, inline)
- Proposer des actions correctives pour les erreurs
- Messages clairs, actionnables, sans jargon technique

### 10. Filtres, Recherche et Tri
```markdown
## Filtres, Recherche et Tri

### Filtres Disponibles
| Filtre | Type | Valeurs Possibles | Comportement |
|--------|------|-------------------|--------------|
| Statut | Select | Active, Inactive, Archivée | Filtre unique (ET) |
| Compétences | Multi-select | Liste des compétences du référentiel | Filtre multiple (OU) |
| Date de création | Plage de dates | Date début, Date fin | Filtre par période (ET) |

### Recherche
- **Champs recherchés** : Prénom, Nom, Email
- **Type de recherche** : Contient (insensible à la casse)
- **Sensibilité à la casse** : Non
- **Comportement** : Recherche déclenchée après 3 caractères saisis ou appui sur Entrée

### Tri
- **Colonnes triables** : Nom, Prénom, Email, Date de création, Statut
- **Tri par défaut** : Nom (ASC)
- **Tri multiple** : Non
```

**Règles** :
- Documenter tous les filtres disponibles avec leurs valeurs possibles
- Préciser le comportement (ET/OU, cumul des filtres)
- Définir la logique de recherche (champs, type, sensibilité)
- Spécifier le tri par défaut et les colonnes triables

### 11. Pagination
```markdown
## Pagination
- **Éléments par page** : 25 (par défaut)
- **Options** : 10, 25, 50, 100
- **Navigation** : Première, Précédente, Suivante, Dernière + Numéros de pages
- **Affichage** : "Page 2 sur 10 - Total : 237 ressources"
- **Persistance** : Le choix du nombre d'éléments par page est mémorisé dans la session
```

**Règles** :
- Définir le nombre d'éléments par défaut
- Proposer des options adaptées à la volumétrie
- Spécifier le type de navigation et l'affichage

### 12. Autorisations et Sécurité
```markdown
## Autorisations et Sécurité

### Contrôle d'Accès
| Rôle | Lecture | Création | Modification | Suppression | Conditions Supplémentaires |
|------|---------|----------|--------------|-------------|---------------------------|
| RH Manager | ✅ | ✅ | ✅ | ✅ | Toutes les ressources |
| Chef de Projet | ✅ | ❌ | ❌ | ❌ | Uniquement ressources de ses projets |
| Collaborateur | ✅ | ❌ | ❌ | ❌ | Uniquement sa propre fiche |

### Visibilité des Données
- Un RH Manager voit toutes les ressources de l'organisation
- Un Chef de Projet ne voit que les ressources allouées à ses projets
- Un Collaborateur ne voit que sa propre fiche ressource

### Données Sensibles
- **Champs sensibles** : Email, Téléphone, Date de naissance
- **Protection requise** : Accès tracé dans les logs d'audit
- **Logs d'accès** : Toute consultation/modification d'une ressource est loguée (Qui, Quand, Quoi)
```

**Règles** :
- Définir précisément qui peut faire quoi (matrice CRUD)
- Documenter les règles de visibilité des données
- Identifier les données sensibles et leur protection

### 13. Intégrations
```markdown
## Intégrations

### Avec d'Autres Features
| Feature | Type d'Interaction | Données Échangées |
|---------|-------------------|-------------------|
| FEATURE-0004-gerer-allocations | Lecture | ID Ressource, Nom, Disponibilité |
| FEATURE-0005-gerer-competences | Lecture/Écriture | ID Ressource, Liste Compétences |

### Avec Systèmes Externes (si applicable)
| Système | Type | Protocole | Données Échangées | Fréquence |
|---------|------|-----------|-------------------|-----------|
| SIRH Entreprise | Import | API REST | Collaborateurs (Nom, Prénom, Email, Service) | Nuit (batch 2h00) |
```

**Règles** :
- Documenter toutes les interactions avec d'autres features
- Identifier les systèmes externes et le mode d'intégration
- Préciser les données échangées et la fréquence

### 14. Notifications
```markdown
## Notifications
| Événement | Destinataire | Canal | Contenu |
|-----------|--------------|-------|---------|
| Création ressource | RH Manager créateur | In-app | "Nouvelle ressource {Nom} {Prénom} créée" |
| Modification ressource | RH Manager | Email | "La ressource {Nom} {Prénom} a été modifiée par {Utilisateur}" |
```

**Règles** :
- Lister tous les événements déclenchant des notifications
- Spécifier les destinataires, canaux et contenu exact

### 15. Performances Attendues
```markdown
## Performances Attendues
| Opération | Temps de Réponse Maximum | Volumétrie Maximale |
|-----------|-------------------------|---------------------|
| Affichage formulaire création | 500 ms | - |
| Enregistrement ressource | 1 seconde | - |
| Affichage liste ressources (paginée) | 2 secondes | 10 000 ressources |
| Recherche | 1 seconde | 10 000 ressources |
```

**Règles** :
- Définir des temps de réponse réalistes et mesurables
- Spécifier la volumétrie testée
- Ces critères serviront aux tests de performance

### 16. User Stories Associées
```markdown
## User Stories Associées
| ID | User Story | Priorité | Statut | Estimation |
|----|-----------|----------|--------|------------|
| US-0001 | Créer une ressource simple | CRITIQUE | DONE | 3 points |
| US-0002 | Ajouter compétences à une ressource | HAUTE | IN_PROGRESS | 5 points |
| US-0003 | Valider unicité email | HAUTE | DONE | 2 points |
```

**Règles** :
- Lister toutes les US composant cette feature
- Maintenir à jour les statuts
- Prioriser les US

### 17. Dépendances
```markdown
## Dépendances

- **Dépend de** :
  - FEATURE-0010-gerer-referentiel-competences : Le référentiel de compétences doit exister avant de pouvoir associer des compétences aux ressources
  
- **Bloque** :
  - FEATURE-0004-gerer-allocations : Les allocations nécessitent l'existence de ressources
```

**Règles** :
- Documenter toutes les dépendances bloquantes
- Expliquer pourquoi la dépendance existe
- Vérifier la cohérence bidirectionnelle

### 18. Données de Référence
```markdown
## Données de Référence

### Listes de Référence (Enums/Lookup Tables)
| Nom Liste | Valeurs | Valeur par Défaut | Modifiable |
|-----------|---------|-------------------|------------|
| StatutRessource | Active, Inactive, Archivée | Active | Non (enum) |
| TypeContrat | CDI, CDD, Freelance, Stagiaire | CDI | Oui (table de référence) |

### Données à Pré-charger (Seed Data)
\`\`\`json
{
  "StatutRessource": [
    { "code": "ACTIVE", "libelle": "Active" },
    { "code": "INACTIVE", "libelle": "Inactive" },
    { "code": "ARCHIVED", "libelle": "Archivée" }
  ]
}
\`\`\`
```

**Règles** :
- Lister toutes les listes de valeurs (enums, référentiels)
- Fournir les données de seed nécessaires au fonctionnement
- Préciser si modifiable par les utilisateurs ou non

### 19. Critères d'Acceptation (Feature Level)
```markdown
## Critères d'Acceptation (Feature Level)
- [ ] Un RH Manager peut créer une ressource avec les champs obligatoires (Nom, Prénom, Email)
- [ ] Le système valide le format de l'email et affiche une erreur si invalide
- [ ] Le système vérifie l'unicité de l'email et empêche les doublons
- [ ] Un message de succès s'affiche après création réussie
- [ ] La ressource créée apparaît immédiatement dans la liste des ressources
- [ ] Un Chef de Projet ne peut PAS créer de ressource
- [ ] Le temps de réponse pour créer une ressource est < 1 seconde
```

**Règles** :
- Définir 5 à 10 critères d'acceptation testables
- Couvrir les scénarios nominaux, validations et autorisations
- Inclure des critères de performance si pertinent

### 20. Tests Fonctionnels Clés
```markdown
## Tests Fonctionnels Clés
| Scénario de Test | Données de Test | Résultat Attendu | Criticité |
|-----------------|----------------|------------------|-----------|
| Création ressource valide | Prénom: "John", Nom: "Doe", Email: "john.doe@test.com" | Ressource créée, message de succès | Haute |
| Email invalide | Email: "invalid-email" | Erreur: "Format d'email invalide" | Haute |
| Email en doublon | Email existant: "existing@test.com" | Erreur: "Email déjà utilisé" | Haute |
| Champs obligatoires vides | Nom vide | Erreur: "Le champ Nom est obligatoire" | Haute |
| Autorisation insuffisante | Utilisateur: Chef de Projet | Accès refusé, bouton "Créer" non visible | Moyenne |
```

**Règles** :
- Définir les tests clés couvrant les scénarios critiques
- Fournir des données de test concrètes
- Prioriser les tests (Haute, Moyenne, Basse)

### 21. Questions Ouvertes / Décisions en Attente
```markdown
## Questions Ouvertes / Décisions en Attente
- ❓ Faut-il permettre l'import massif de ressources depuis un fichier Excel ? → Décision en attente, à valider avec le métier
- ❓ Quelle est la durée de conservation des ressources archivées ? → Règle légale à confirmer avec le service juridique
- ⚠️ Point d'attention : Le SIRH actuel ne fournit pas toutes les compétences, nécessitera une saisie manuelle initiale
```

**Règles** :
- Documenter toutes les questions non résolues
- Tracer les décisions en attente avec la date et le responsable
- Marquer les points d'attention (⚠️)
- Mettre à jour dès résolution

### 22. Historique des Modifications
```markdown
## Historique des Modifications
| Date | Version | Auteur | Modifications |
|------|---------|--------|---------------|
| 2024-11-20 | 1.0.0 | Marie Dupont | Création initiale de la feature |
| 2024-11-22 | 1.1.0 | Jean Martin | Ajout de la règle d'unicité de l'email suite à revue |
| 2024-11-25 | 1.2.0 | Marie Dupont | Ajout de la US-0003 (validation unicité) |
```

**Règles** :
- Documenter toutes les modifications avec date, version, auteur et description
- Versioning sémantique (SemVer)
- Ne JAMAIS supprimer l'historique

### 23. Références
```markdown
## Références
- **EPIC Parent** : EPIC-0002-gestion-ressources
- **ADR liés** : 
  - ADR-015 : Validation des emails côté serveur
- **Personas** :
  - responsable-rh.personna.md
  - chef-de-projet.personna.md
```

**Règles** :
- Lier systématiquement à l'EPIC parent
- Référencer les ADR techniques impactant le fonctionnel
- Lier aux personas utilisés

## Checklist de Validation

Avant de considérer une FEATURE comme complète :

### Complétude
- [ ] Description fonctionnelle claire (format User Story)
- [ ] Workflow principal documenté avec diagramme
- [ ] Scénarios : au moins 1 nominal + 2 alternatifs + 2 erreurs
- [ ] Règles métier explicites et numérotées
- [ ] Données manipulées détaillées (entités, propriétés, relations)
- [ ] Actions utilisateur listées avec permissions
- [ ] Feedback utilisateur (messages exacts) spécifiés
- [ ] Filtres/recherche/tri/pagination définis si applicable
- [ ] Autorisations et sécurité documentées
- [ ] Performances attendues spécifiées
- [ ] Au moins 3 User Stories associées
- [ ] Critères d'acceptation testables (5 minimum)

### Qualité
- [ ] Français correct, ton professionnel
- [ ] Agnostique de la technologie (focus métier)
- [ ] Pas de contradiction avec d'autres features ou l'EPIC
- [ ] Liens entre documents fonctionnels
- [ ] Historique des modifications à jour

### Pragmatisme
- [ ] Objectifs réalistes et atteignables
- [ ] Périmètre raisonnable (feature déployable indépendamment)
- [ ] Scénarios d'erreur couverts
- [ ] Cas limites identifiés

## Bonnes Pratiques

### ✅ À Faire
- Impliquer un utilisateur final dans la rédaction
- Utiliser des exemples de données réelles (anonymisées)
- Valider les workflows avec des captures d'écran ou maquettes (optionnel)
- Réviser après chaque Sprint si modifications
- Documenter les décisions métier et leurs justifications

### ❌ À Éviter
- Mélanger plusieurs fonctionnalités dans une seule feature
- Ignorer les scénarios d'erreur
- Spécifier des messages d'erreur vagues ("Erreur", "Données invalides")
- Oublier les autorisations et la sécurité
- Copier-coller sans adapter au contexte

## Maintenance

### Révision Continue
- **Fréquence** : À chaque Sprint ou modification de périmètre
- **Déclencheurs** : Feedback utilisateurs, bugs identifiés, évolution des besoins
- **Actions** : Mettre à jour les scénarios, règles métier, critères d'acceptation

### Dépréciation
Si la feature devient obsolète :
1. Mettre le statut à `[DEPRECATED]`
2. Ajouter en haut :
   ```markdown
   > ⚠️ **DEPRECATED** : Cette feature est obsolète.  
   > Remplacée par : FEATURE-{ID}  
   > Date de dépréciation : YYYY-MM-DD  
   > Raison : {Explication}
   ```
3. Ne JAMAIS supprimer le fichier
4. Mettre à jour l'EPIC parent et l'index

## Exemple Complet

Voir les FEATUREs existantes dans `documentations/functionnals/` pour des exemples concrets adaptés au projet.