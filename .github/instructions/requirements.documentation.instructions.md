---
description: Requirements fonctionnels - création, structure, format et gestion du cycle de vie
name: Requirements_Documentation
applyTo: "**/docs/requirements/**/*.requirements.md"
---

# Requirements Fonctionnels - Guide Expert

Guide complet pour créer, structurer et gérer les documents de requirements fonctionnels (spécifications métier).

## ⛔ À NE PAS FAIRE

- **N'écris jamais** de détails techniques dans un requirement (architecture, patterns, BDD)
- **Ne mélange jamais** fonctionnel et technique dans le même document
- **N'utilise jamais** de jargon technique incompréhensible par le métier
- **N'omets jamais** les critères d'acceptation mesurables
- **Ne crée jamais** de requirement sans identifiant unique (`REQ-XXX`)
- **N'oublie jamais** de lier les requirements aux ADR techniques associés
- **Ne duplique jamais** des requirements existants
- **N'écris jamais** de requirement vague ou non testable

## ✅ À FAIRE

- **Rédige toujours** du point de vue utilisateur/métier
- **Utilise toujours** le format Given-When-Then pour les critères d'acceptation
- **Inclus toujours** des seuils quantifiés et mesurables
- **Organise toujours** par domaine métier (`docs/requirements/{domaine}/`)
- **Documente toujours** les règles métier avec des exemples concrets
- **Référence toujours** les ADR techniques liés
- **Valide toujours** avec le Product Owner avant de finaliser
- **Mets à jour toujours** le statut du requirement (Draft → Validé → Implémenté)

## 🎯 Actions Obligatoires (Mandatory)

### Séparation Fonctionnel/Technique

**RÈGLE ABSOLUE : Les requirements DOIVENT être UNIQUEMENT fonctionnels.**

**TOUJOURS dans `*.requirements.md` (fonctionnel) :**
- ✅ Besoins utilisateur et cas d'usage métier
- ✅ Règles métier et processus
- ✅ Critères d'acceptation fonctionnels
- ✅ Workflows et parcours utilisateur
- ✅ Contraintes métier et organisationnelles
- ✅ Données métier manipulées (sans structure technique)

**JAMAIS dans `*.requirements.md` (technique) :**
- ❌ Choix d'architecture ou de technologies
- ❌ Patterns techniques (CQRS, DDD, etc.)
- ❌ Décisions d'implémentation
- ❌ Structure de base de données
- ❌ APIs et contrats techniques
- ❌ Configuration infrastructure

**Les aspects techniques DOIVENT être dans `docs/adr/*.adr.md`**

### Format et Structure

**Structure de fichier OBLIGATOIRE :**

```markdown
# REQ-[Numéro]. [Titre court et descriptif du requirement]

Date: YYYY-MM-DD

## Statut

[Draft | Validé | Implémenté | Obsolète | Remplacé par REQ-XXX]

## Contexte Métier

[Description du besoin métier, contexte utilisateur, problème à résoudre]

## Objectifs Métier

[Liste des objectifs métier que ce requirement doit atteindre]

- Objectif 1
- Objectif 2
- Objectif 3

## Acteurs Concernés

[Liste des rôles utilisateur/acteurs impliqués]

- **Acteur 1** : Description du rôle
- **Acteur 2** : Description du rôle

## Exigences Fonctionnelles

### EF-[ID].1 - [Titre de l'exigence]
- **Description** : [Description détaillée de l'exigence fonctionnelle]
- **Priorité** : [Critique | Haute | Moyenne | Basse]
- **Justification priorité** : [Pourquoi cette priorité - impact métier, urgence, dépendances]
- **Critères de validation** : 
  - Critère mesurable 1 (avec seuil quantifié)
  - Critère mesurable 2 (avec seuil quantifié)
- **Exemples concrets** :
  - **Exemple 1** : [Scénario réel avec données concrètes]
  - **Exemple 2** : [Autre scénario avec données concrètes]
- **Anti-exemples** : [Ce qui NE doit PAS se produire]
  - Anti-exemple 1 : [Comportement à éviter]
- **Seuils quantifiés** :
  - Volume minimum : [ex: 10 éléments]
  - Volume maximum : [ex: 1000 éléments]
  - Temps de réponse : [ex: < 2 secondes]

### EF-[ID].2 - [Titre de l'exigence]
[...]

## Règles Métier

### RG-[ID].1 - [Titre de la règle]
- **Description** : [Énoncé clair et non ambigu de la règle métier]
- **Conditions** : [Quand cette règle s'applique - conditions EXACTES]
- **Actions** : [Que doit-il se passer - comportement PRÉCIS attendu]
- **Formule/Algorithme métier** : [Si calcul, formule exacte avec exemple]
  - Exemple : `Total = (Quantité × Prix) - Remise`
  - Avec : Quantité=5, Prix=10€, Remise=5€ → Total=45€
- **Exceptions à la règle** : [Cas où la règle ne s'applique PAS]
  - Exception 1 : [Condition d'exception]
- **Origine** : [Réglementation, processus métier, décision business]
- **Vérifiable par** : [Comment tester cette règle - test manuel, automatisé, audit]

### RG-[ID].2 - [Titre de la règle]
[...]

## Scénarios d'Usage

### Scénario 1: [Nom du scénario]
**Acteur** : [Qui effectue l'action]

**Objectif** : [Ce que l'acteur veut accomplir]

**Préconditions** :
- Condition 1
- Condition 2

**Flux nominal** :
1. L'acteur fait action 1
2. Le système répond par...
3. L'acteur fait action 2
4. ...

**Flux alternatifs** :
- **3a.** Si condition alternative, alors...

**Postconditions** :
- État final 1
- État final 2

**Critères d'acceptation** :
- [ ] **CAC-1** : Critère 1 vérifié (DOIT être testable)
- [ ] **CAC-2** : Critère 2 vérifié (DOIT être testable)

**Données de test** :
- **Jeu de données 1** : [Données d'entrée concrètes]
  - Résultat attendu : [Sortie exacte attendue]
- **Jeu de données 2** : [Données limites]
  - Résultat attendu : [Sortie exacte attendue]

### Scénario 2: [Nom du scénario]
[...]

## Critères d'Acceptation Globaux

[Liste exhaustive et systématique des critères d'acceptation couvrant TOUS les aspects du requirement]

**RÈGLES D'ÉCRITURE DES CRITÈRES :**
- ✅ **Spécifique** : Décrire EXACTEMENT ce qui doit être vérifié
- ✅ **Mesurable** : Inclure des seuils quantifiés (nombres, temps, pourcentages)
- ✅ **Testable** : Doit pouvoir être vérifié par un test manuel ou automatisé
- ✅ **Non ambigu** : Une seule interprétation possible
- ✅ **Indépendant** : Chaque critère est autonome
- ✅ **Format Given-When-Then** : Utiliser "Étant donné... quand... alors..."

**EXEMPLE DE CRITÈRE BIEN RÉDIGÉ :**
- ❌ Mauvais : "Le système doit être rapide"
- ✅ Bon : "Étant donné une liste de 100 prompts, quand l'utilisateur accède à 'Mes Favoris', alors la liste s'affiche en moins de 1 seconde (mesurée par Performance API)"

### Critères Fonctionnels

**Cas Nominaux (Happy Path) :**
- [ ] **CFN-1** : [Critère cas nominal 1 - vérifiable et mesurable]
- [ ] **CFN-2** : [Critère cas nominal 2 - vérifiable et mesurable]
- [ ] **CFN-3** : [Critère cas nominal 3 - vérifiable et mesurable]

**Cas aux Limites (Edge Cases) :**
- [ ] **CFL-1** : [Critère limite 1 - ex: liste vide, valeur nulle, maximum atteint]
- [ ] **CFL-2** : [Critère limite 2 - ex: premier élément, dernier élément]
- [ ] **CFL-3** : [Critère limite 3 - ex: valeur minimum, valeur maximum]

**Cas d'Exception (Error Cases) :**
- [ ] **CFE-1** : [Critère exception 1 - gestion erreur réseau]
- [ ] **CFE-2** : [Critère exception 2 - données invalides]
- [ ] **CFE-3** : [Critère exception 3 - timeout, indisponibilité]

### Critères Non Fonctionnels

**Performance :**
- [ ] **CNF-P1** : [Critère performance 1 - temps de réponse < X secondes]
- [ ] **CNF-P2** : [Critère performance 2 - débit minimum Y transactions/seconde]

**Sécurité :**
- [ ] **CNF-S1** : [Critère sécurité 1 - authentification requise]
- [ ] **CNF-S2** : [Critère sécurité 2 - autorisation par rôle]
- [ ] **CNF-S3** : [Critère sécurité 3 - audit trail complet]

**Utilisabilité :**
- [ ] **CNF-U1** : [Critère utilisabilité 1 - accessible au clavier]
- [ ] **CNF-U2** : [Critère utilisabilité 2 - messages d'erreur explicites]
- [ ] **CNF-U3** : [Critère utilisabilité 3 - feedback visuel immédiat]

**Compatibilité :**
- [ ] **CNF-C1** : [Critère compatibilité 1 - navigateurs supportés avec versions]
  - Chrome ≥ 90, Firefox ≥ 88, Safari ≥ 14, Edge ≥ 90
- [ ] **CNF-C2** : [Critère compatibilité 2 - appareils mobiles]
  - iOS ≥ 14, Android ≥ 11, résolutions 360×640 à 1920×1080
- [ ] **CNF-C3** : [Critère compatibilité 3 - versions antérieures]

**Fiabilité :**
- [ ] **CNF-F1** : [Critère fiabilité 1 - taux d'erreur < X%]
- [ ] **CNF-F2** : [Critère fiabilité 2 - récupération automatique après erreur]

**Maintenabilité (vue métier) :**
- [ ] **CNF-M1** : Les messages d'erreur sont compréhensibles par l'utilisateur final
- [ ] **CNF-M2** : Les logs permettent d'identifier la cause d'un problème utilisateur

**Observabilité (vue métier) :**
- [ ] **CNF-O1** : Les administrateurs peuvent suivre l'utilisation en temps réel
- [ ] **CNF-O2** : Les métriques métier sont disponibles pour analyse

### Critères de Conformité

**Réglementaire :**
- [ ] **CCR-1** : [Conformité RGPD - anonymisation données]
- [ ] **CCR-2** : [Conformité accessibilité - WCAG 2.1 niveau AA]
- [ ] **CCR-3** : [Conformité réglementation métier spécifique]

**Standards Internes :**
- [ ] **CCS-1** : [Respect charte graphique]
- [ ] **CCS-2** : [Respect conventions de nommage]
- [ ] **CCS-3** : [Documentation utilisateur fournie]

## Exigences Non Fonctionnelles

### ENF-[ID].1 - Performance
- [Exigence de performance métier, ex: "Temps de réponse ressenti < 3 secondes"]

### ENF-[ID].2 - Utilisabilité
- [Exigence d'utilisabilité, ex: "Interface accessible aux personnes malvoyantes"]

### ENF-[ID].3 - Disponibilité
- [Exigence de disponibilité métier, ex: "Service accessible 24/7"]

## Contraintes Métier

[Liste des contraintes imposées par le métier, la réglementation, l'organisation]

- **Contrainte 1** : Description
- **Contrainte 2** : Description

## Données Métier

[Description des données métier manipulées, SANS structure technique]

### Entité Métier 1
- **Description** : [Ce que représente cette entité pour le métier]
- **Informations** : [Liste des informations métier, pas de types techniques]
  - Information 1
  - Information 2
- **Règles de validation métier** :
  - Information 1 : [Contraintes métier, ex: "obligatoire", "unique", "format attendu"]
  - Information 2 : [Contraintes métier]
- **Cycle de vie** : [États possibles et transitions métier]

### Entité Métier 2
[...]

## Glossaire Local

[Définition des termes métier spécifiques à ce requirement]

### Terme Métier 1
- **Définition** : [Définition précise dans le contexte de ce requirement]
- **Synonymes** : [Autres termes utilisés pour le même concept]
- **Exemples** : [Exemples concrets d'utilisation]

### Terme Métier 2
- **Définition** : [Définition précise]
- **À ne pas confondre avec** : [Clarification de termes proches]
- **Exemples** : [Exemples concrets]

## Relations avec Autres Requirements

[Relations et dépendances avec d'autres requirements du projet]

### Dépendances (Prérequis)

[Requirements qui DOIVENT être implémentés AVANT celui-ci]

- **REQ-XXX** - [Titre] : [Explication de la dépendance]
- **REQ-YYY** - [Titre] : [Explication de la dépendance]

### Impacte (Successeurs)

[Requirements qui dépendent de celui-ci, qui ne peuvent être implémentés qu'après]

- **REQ-ZZZ** - [Titre] : [Explication de l'impact]

### Complète

[Requirements que celui-ci complète ou enrichit]

- **REQ-AAA** - [Titre] : [Explication de comment il complète]

### Amende

[Requirements que celui-ci modifie, corrige ou améliore]

- **REQ-BBB** - [Titre] : [Explication de l'amendement]
- **Changements apportés** : [Description précise]

### Désavoue

[Requirements que celui-ci remplace ou rend obsolète]

- **REQ-CCC** - [Titre] : [Explication du désaveu]
- **Raison** : [Pourquoi l'ancien requirement n'est plus valide]

### Incompatible Avec

[Requirements avec lesquels celui-ci est en conflit (à résoudre)]

- **REQ-DDD** - [Titre] : [Nature du conflit]
- **Résolution proposée** : [Comment lever le conflit]

### En Conflit Avec (à résoudre)

[Conflits identifiés nécessitant arbitrage métier]

- **REQ-EEE** - [Titre] : [Description du conflit]
- **Impact** : [Conséquences si non résolu]
- **Arbitrage nécessaire** : [Qui doit décider]

## Dépendances Métier Externes

[Dépendances vers processus métier externes, systèmes tiers, réglementations]

- **Processus externe** : [Nom du processus] - [Description]
- **Système tiers** : [Nom du système] - [Nature de la dépendance]
- **Réglementation** : [Nom] - [Obligation]

## Critères de Succès Métier

[Comment mesurer le succès métier de ce requirement - KPI et métriques]

### Métriques d'Adoption
- [ ] **MA-1** : [Métrique adoption 1 - ex: 60% utilisateurs utilisent la fonctionnalité]
- [ ] **MA-2** : [Métrique adoption 2 - ex: 100 utilisations/jour]

### Métriques de Performance Métier
- [ ] **MP-1** : [Métrique performance 1 - ex: réduction temps de 90s à 10s]
- [ ] **MP-2** : [Métrique performance 2 - ex: augmentation productivité 30%]

### Métriques de Satisfaction
- [ ] **MS-1** : [Métrique satisfaction 1 - ex: score satisfaction ≥ 4/5]
- [ ] **MS-2** : [Métrique satisfaction 2 - ex: taux de recommandation ≥ 80%]

### Métriques de Qualité
- [ ] **MQ-1** : [Métrique qualité 1 - ex: taux d'erreur < 1%]
- [ ] **MQ-2** : [Métrique qualité 2 - ex: 0 bugs critiques en production]

### Métriques Business
- [ ] **MB-1** : [Métrique business 1 - ex: ROI positif dans 6 mois]
- [ ] **MB-2** : [Métrique business 2 - ex: réduction coût support 40%]

## Matrice de Couverture

[Vérification que TOUS les objectifs métier sont couverts par les exigences]

| Objectif Métier | Exigences Couvrant | Scénarios Validant | Critères d'Acceptation |
|-----------------|--------------------|--------------------|------------------------|
| [Objectif 1]    | EF-001.1, EF-001.3 | Scénario 1, 3      | CFN-1, CFN-2, CFL-1    |
| [Objectif 2]    | EF-001.2           | Scénario 2         | CFN-3, CFE-1           |

**OBLIGATION** : Chaque objectif métier DOIT être couvert par au moins une exigence, un scénario et des critères.

## Acceptation par Persona

[Validation du requirement du point de vue de chaque acteur concerné]

### [Acteur 1 - Nom du Rôle]
- **Bénéfices attendus** : [Ce que cet acteur gagne]
- **Exigences prioritaires pour lui** : EF-001.1, EF-001.3
- **Validation** : [Comment cet acteur valide que le requirement répond à son besoin]
- **Critères de satisfaction** : [KPI spécifiques à cet acteur]

### [Acteur 2 - Nom du Rôle]
- **Bénéfices attendus** : [Ce que cet acteur gagne]
- **Exigences prioritaires pour lui** : EF-001.2
- **Validation** : [Comment cet acteur valide]
- **Critères de satisfaction** : [KPI spécifiques]

## Analyse de Cohérence

[Vérification systématique de non-duplication et non-contradiction]

### Vérification de Duplication
- [ ] **VD-1** : Aucun requirement existant ne couvre déjà ce besoin
- [ ] **VD-2** : Aucun chevauchement fonctionnel avec REQ-XXX identifié
- [ ] **VD-3** : Périmètre clairement distinct des autres requirements du domaine

### Vérification de Cohérence
- [ ] **VC-1** : Aucune contradiction avec REQ-XXX (vérifié)
- [ ] **VC-2** : Terminologie cohérente avec glossaire métier
- [ ] **VC-3** : Règles métier cohérentes avec requirements existants
- [ ] **VC-4** : Données métier cohérentes avec modèle de domaine

### Points de Vigilance
- **Risque de duplication** : [Aucun | Description du risque]
- **Risque de contradiction** : [Aucun | Description du risque]
- **Dépendances circulaires** : [Aucune | Description et résolution]

## Risques et Mitigations

[Identification proactive des risques métier liés au requirement]

### Risque Métier 1 : [Titre du risque]
- **Description** : [Nature du risque]
- **Probabilité** : [Élevée | Moyenne | Faible]
- **Impact** : [Critique | Majeur | Mineur]
- **Déclencheurs** : [Ce qui pourrait causer ce risque]
- **Mitigation** : [Actions préventives pour réduire le risque]
- **Plan de contingence** : [Actions si le risque se matérialise]

### Risque Métier 2 : [Titre du risque]
- **Description** : [Nature du risque]
- **Probabilité** : [Élevée | Moyenne | Faible]
- **Impact** : [Critique | Majeur | Mineur]
- **Déclencheurs** : [Ce qui pourrait causer ce risque]
- **Mitigation** : [Actions préventives]
- **Plan de contingence** : [Actions correctives]

**Exemples de risques métier :**
- Adoption insuffisante par les utilisateurs
- Mauvaise compréhension des besoins réels
- Processus métier non respecté en pratique
- Données métier incomplètes ou incorrectes
- Résistance au changement organisationnel

## Traçabilité

### Origine du Besoin
- **Source** : [Demande utilisateur | Étude marché | Réglementation | Initiative interne]
- **Demandeur** : [Nom/Rôle]
- **Date de la demande** : [YYYY-MM-DD]
- **Référence** : [Ticket JIRA, email, document source]

### Historique des Modifications

| Date       | Version | Auteur      | Modification                                |
|------------|---------|-------------|---------------------------------------------|
| 2025-12-06 | 1.0     | [Nom]       | Création initiale                           |
| 2025-12-10 | 1.1     | [Nom]       | Ajout scénario alternatif suite review      |

### Implémentation

- **Sprint/Release** : [Sprint 5 / Release 2.3.0]
- **Équipe responsable** : [Nom de l'équipe]
- **Estimation** : [Story points / Jours-homme]
- **Priorité backlog** : [Critique | Haute | Moyenne | Basse]
- **Dépendances techniques** : [Référence aux ADR associés]

## Définition de "Fini" (Definition of Done)

[Checklist exhaustive pour considérer ce requirement comme COMPLÈTEMENT implémenté]

### Développement
- [ ] Toutes les exigences fonctionnelles (EF-XXX) sont implémentées
- [ ] Toutes les règles métier (RG-XXX) sont codées et testées
- [ ] Code reviewé et approuvé par au moins 2 développeurs
- [ ] Aucune dette technique intentionnelle non documentée

### Tests
- [ ] Tous les critères d'acceptation (CFN, CFL, CFE) validés par tests automatisés
- [ ] Tests d'intégration passent à 100%
- [ ] Tests de non-régression passent à 100%
- [ ] Tests de performance métier validés (seuils respectés)
- [ ] Tests d'accessibilité (WCAG) passent si applicable

### Documentation
- [ ] Documentation utilisateur créée/mise à jour
- [ ] Documentation technique (ADR) créée si décisions architecturales
- [ ] Guide d'utilisation disponible pour chaque acteur concerné
- [ ] Messages d'aide contextuels intégrés dans l'interface

### Validation Métier
- [ ] Démo effectuée au Product Owner
- [ ] Validation par chaque persona concerné
- [ ] Critères de succès métier mesurables (KPI en place)
- [ ] Feedback utilisateur final collecté (UAT passé)

### Déploiement
- [ ] Déployé en environnement de recette
- [ ] Déployé en production
- [ ] Monitoring et alertes configurés
- [ ] Rollback plan testé et documenté

### Formation et Communication
- [ ] Utilisateurs finaux formés
- [ ] Support/Helpdesk informé et formé
- [ ] Communication interne envoyée (changelog)
- [ ] Communication externe si applicable

## Références

- [Lien vers documentation métier]
- [Lien vers processus métier existant]
- [Lien vers réglementation applicable]
- [ADR-XXX: Décisions techniques associées]
- [Glossaire métier - Termes utilisés]
- [Modèle de domaine - Entités concernées]
```

### Nommage des Fichiers

**Convention de nommage OBLIGATOIRE** : `NNN-titre-en-kebab-case.requirements.md`

- `NNN` : Numéro séquentiel avec padding de zéros (001, 002, 003, ...)
- Titre en kebab-case (minuscules, mots séparés par tirets)
- Extension `.requirements.md` OBLIGATOIRE

**Exemples valides :**
```
001-gestion-utilisateurs.requirements.md
002-processus-validation.requirements.md
003-tableau-de-bord-statistiques.requirements.md
015-module-facturation.requirements.md
```

**Exemples INVALIDES :**
```
❌ req-001.md (extension incorrecte)
❌ 1-gestion-users.requirements.md (numérotation sans padding)
❌ 001-Gestion_Utilisateurs.requirements.md (PascalCase, underscore)
❌ gestion-utilisateurs.requirements.md (pas de numéro)
```

### Emplacement des Fichiers

**Tous les requirements DOIVENT être stockés dans** : `docs/requirements/`

**Structure du répertoire OBLIGATOIRE par domaines/sous-domaines :**
```
docs/
└── requirements/
    ├── README.md                                    # Index global et documentation
    │
    ├── authentication/                              # Domaine : Authentification
    │   ├── README.md                               # Index du domaine
    │   ├── 001-user-login.requirements.md
    │   └── 002-password-reset.requirements.md
    │
    ├── prompts/                                     # Domaine : Gestion des prompts
    │   ├── README.md                               # Index du domaine
    │   ├── library/                                # Sous-domaine
    │   │   ├── 010-prompt-library.requirements.md
    │   │   └── 011-prompt-search.requirements.md
    │   ├── favorites/                              # Sous-domaine
    │   │   └── 020-favorite-prompts.requirements.md
    │   └── sharing/                                # Sous-domaine
    │       └── 030-prompt-sharing.requirements.md
    │
    ├── analytics/                                   # Domaine : Analytique
    │   ├── README.md                               # Index du domaine
    │   ├── statistics/                             # Sous-domaine
    │   │   └── 040-usage-statistics.requirements.md
    │   └── reporting/                              # Sous-domaine
    │       └── 041-export-reports.requirements.md
    │
    └── administration/                              # Domaine : Administration
        ├── README.md                               # Index du domaine
        └── 050-user-management.requirements.md
```

**RÈGLES d'organisation :**

1. **Domaines métier** : Premier niveau de dossiers représentant les grands domaines fonctionnels
2. **Sous-domaines** : Second niveau pour organiser les sous-fonctionnalités complexes
3. **Granularité** : Maximum 2 niveaux de profondeur (domaine/sous-domaine)
4. **README.md** : Chaque domaine DOIT avoir son README.md listant les requirements du domaine
5. **Numérotation globale** : Les numéros de requirements sont uniques dans TOUT le projet (001, 002, 003...)
6. **Cohérence** : Tous les requirements d'un même domaine doivent avoir une cohérence fonctionnelle

**Exemples de domaines typiques :**
- `authentication/` - Authentification, autorisation, gestion des sessions
- `prompts/` - Gestion des prompts IA (bibliothèque, favoris, partage)
- `analytics/` - Statistiques, rapports, tableaux de bord
- `administration/` - Gestion utilisateurs, configuration, paramétrage
- `notifications/` - Alertes, emails, notifications push
- `integration/` - APIs externes, webhooks, connecteurs

## 📝 Contenu des Sections

### Section "Contexte Métier"

**DOIT contenir :**
- Description du besoin métier à l'origine du requirement
- Contexte utilisateur et problématique métier
- Situation actuelle (si applicable)
- Pourquoi ce requirement est nécessaire d'un point de vue métier

**Exemple :**
```markdown
## Contexte Métier

L'équipe marketing souhaite analyser l'utilisation des prompts IA par les utilisateurs pour :
- Identifier les prompts les plus populaires
- Comprendre les besoins récurrents des utilisateurs
- Améliorer la bibliothèque de prompts proposée

Actuellement, aucun outil ne permet de suivre l'utilisation des prompts.
Les décisions d'amélioration sont prises de manière intuitive sans données factuelles.

Ce requirement vise à fournir un tableau de bord permettant de visualiser
les statistiques d'utilisation et de prendre des décisions data-driven.
```

**Caractéristiques :**
- ✅ Vocabulaire métier (pas de jargon technique)
- ✅ Centré sur les besoins utilisateur
- ✅ Justification métier claire
- ❌ Pas de mention de technologies

### Section "Objectifs Métier"

**DOIT contenir :**
- Liste des objectifs métier à atteindre
- Bénéfices attendus pour l'utilisateur final
- Valeur ajoutée pour l'organisation

**Exemple :**
```markdown
## Objectifs Métier

- Permettre à l'équipe marketing d'identifier les 10 prompts les plus utilisés
- Réduire le temps de décision pour l'amélioration de la bibliothèque de prompts
- Augmenter la satisfaction utilisateur en proposant des prompts pertinents
- Mesurer l'adoption de nouvelles fonctionnalités de prompts
```

### Section "Acteurs Concernés"

**DOIT contenir :**
- Liste exhaustive des rôles utilisateur impliqués
- Description claire de chaque rôle
- Responsabilités de chaque acteur dans le contexte du requirement

**Exemple :**
```markdown
## Acteurs Concernés

- **Administrateur Marketing** : Consulte les statistiques globales, exporte les rapports, identifie les tendances
- **Utilisateur Final** : Utilise les prompts, ses actions sont comptabilisées dans les statistiques (anonymement)
- **Manager** : Valide les décisions d'amélioration basées sur les statistiques
```

### Section "Exigences Fonctionnelles"

**DOIT contenir :**
- Liste numérotée des exigences fonctionnelles
- Chaque exigence avec titre, description, priorité et critères de validation
- Exigences mesurables et vérifiables

**Format OBLIGATOIRE pour chaque exigence :**
```markdown
### EF-[ID].X - [Titre court]
- **Description** : [Description détaillée du comportement attendu]
- **Priorité** : [Critique | Haute | Moyenne | Basse]
- **Critères de validation** : 
  - Critère mesurable 1
  - Critère mesurable 2
```

**Exemple :**
```markdown
## Exigences Fonctionnelles

### EF-001.1 - Affichage du top 10 des prompts
- **Description** : Le système doit afficher la liste des 10 prompts les plus utilisés sur les 30 derniers jours, classés par nombre d'utilisations décroissant.
- **Priorité** : Haute
- **Critères de validation** :
  - La liste contient exactement 10 prompts (ou moins si moins de 10 prompts utilisés)
  - Le classement est correct (nombre d'utilisations vérifié manuellement)
  - La période de 30 jours est respectée
  - Les données sont rafraîchies toutes les heures

### EF-001.2 - Filtrage par période
- **Description** : L'utilisateur doit pouvoir sélectionner la période d'analyse : 7 jours, 30 jours, 90 jours, ou période personnalisée.
- **Priorité** : Moyenne
- **Critères de validation** :
  - Les 3 périodes prédéfinies sont disponibles
  - La période personnalisée accepte des dates valides
  - Les statistiques sont recalculées correctement selon la période choisie
```

### Section "Règles Métier"

**DOIT contenir :**
- Règles métier explicites et non ambiguës
- Conditions d'application de chaque règle
- Actions à effectuer quand la règle s'applique

**Format OBLIGATOIRE pour chaque règle :**
```markdown
### RG-[ID].X - [Titre de la règle]
- **Description** : [Énoncé de la règle]
- **Conditions** : [Quand la règle s'applique]
- **Actions** : [Ce qui doit se passer]
```

**Exemple :**
```markdown
## Règles Métier

### RG-001.1 - Anonymisation des données utilisateur
- **Description** : Les statistiques d'utilisation ne doivent jamais révéler l'identité des utilisateurs individuels
- **Conditions** : Toujours, pour toute consultation de statistiques
- **Actions** : 
  - Afficher uniquement les compteurs agrégés
  - Ne jamais afficher de nom, email ou identifiant utilisateur
  - Masquer les statistiques si moins de 5 utilisateurs concernés (risque de ré-identification)

### RG-001.2 - Exclusion des tests internes
- **Description** : Les utilisations de prompts par les comptes de test ne doivent pas être comptabilisées
- **Conditions** : Si l'utilisateur a le rôle "Testeur" ou "Admin"
- **Actions** : 
  - Ne pas incrémenter les compteurs d'utilisation
  - Afficher un badge "Mode Test" pour ces comptes
```

### Section "Scénarios d'Usage"

**DOIT contenir :**
- Scénarios complets d'utilisation du point de vue métier
- Flux nominal (cas nominal)
- Flux alternatifs (cas d'erreur, cas particuliers)
- Critères d'acceptation pour chaque scénario

**Format OBLIGATOIRE :**
```markdown
### Scénario X: [Nom du scénario]
**Acteur** : [Qui]
**Objectif** : [Quoi]

**Préconditions** :
- Condition 1

**Flux nominal** :
1. L'acteur fait...
2. Le système...

**Flux alternatifs** :
- **Xa.** Si..., alors...

**Postconditions** :
- État final

**Critères d'acceptation** :
- [ ] Critère 1
```

**Exemple :**
```markdown
## Scénarios d'Usage

### Scénario 1: Consultation des statistiques globales
**Acteur** : Administrateur Marketing

**Objectif** : Consulter les prompts les plus utilisés pour identifier les tendances

**Préconditions** :
- L'administrateur est authentifié
- Des données statistiques existent (au moins 7 jours d'historique)

**Flux nominal** :
1. L'administrateur accède au tableau de bord statistiques
2. Le système affiche le top 10 des prompts sur 30 jours par défaut
3. L'administrateur sélectionne la période "7 jours"
4. Le système recalcule et affiche le top 10 sur 7 jours
5. L'administrateur exporte les données en CSV
6. Le système génère et télécharge le fichier CSV

**Flux alternatifs** :
- **2a.** Si aucune donnée disponible, afficher un message "Aucune statistique disponible pour la période sélectionnée"
- **5a.** Si l'export échoue, afficher un message d'erreur et proposer de réessayer

**Postconditions** :
- Les statistiques sont affichées correctement
- Le fichier CSV est téléchargé (cas nominal)

**Critères d'acceptation** :
- [ ] Les 10 prompts les plus utilisés sont affichés
- [ ] Le changement de période fonctionne en moins de 2 secondes
- [ ] L'export CSV contient toutes les colonnes attendues
- [ ] Un message clair s'affiche en cas d'absence de données
```

### Section "Exigences Non Fonctionnelles"

**DOIT contenir UNIQUEMENT des exigences métier :**
- Performance ressentie par l'utilisateur (pas de détails techniques)
- Utilisabilité et accessibilité
- Disponibilité du point de vue métier
- Conformité réglementaire (RGPD, etc.)

**❌ NE PAS inclure :**
- Architecture technique
- Technologies à utiliser
- Scalabilité infrastructure

**Exemple :**
```markdown
## Exigences Non Fonctionnelles

### ENF-001.1 - Performance ressentie
- L'affichage du tableau de bord doit être perçu comme instantané (< 3 secondes)
- Le changement de période doit être fluide (< 2 secondes)

### ENF-001.2 - Accessibilité
- L'interface doit être utilisable au clavier uniquement
- Les graphiques doivent avoir des alternatives textuelles pour les lecteurs d'écran
- Le contraste des couleurs doit respecter les normes WCAG 2.1 niveau AA

### ENF-001.3 - Disponibilité
- Le tableau de bord doit être accessible 24/7 aux horaires de bureau
- Une maintenance planifiée peut être effectuée les weekends (notification préalable)

### ENF-001.4 - Conformité RGPD
- Les données statistiques doivent être anonymisées
- Aucune donnée personnelle ne doit être exportable
- Les utilisateurs doivent pouvoir demander l'exclusion de leurs données des statistiques
```

### Section "Contraintes Métier"

**DOIT contenir :**
- Contraintes imposées par l'organisation, le métier, la réglementation
- Limitations métier à respecter
- Processus obligatoires à suivre

**Exemple :**
```markdown
## Contraintes Métier

- **Validation marketing** : Toute nouvelle métrique doit être validée par le responsable marketing avant affichage
- **Confidentialité** : Les statistiques ne peuvent être consultées que par les rôles "Admin" et "Manager"
- **Audit** : Toute consultation de statistiques doit être tracée pour audit interne
- **Rétention des données** : Les statistiques doivent être conservées pendant 2 ans minimum (obligation légale)
- **Export limité** : Les exports CSV sont limités à 1000 lignes maximum pour éviter les abus
```

### Section "Données Métier"

**DOIT contenir description métier des données, PAS structure technique :**
- Entités métier manipulées
- Informations métier (sans types techniques)
- Relations métier entre entités

**❌ NE PAS inclure :**
- Types de données techniques (int, string, DateTime)
- Noms de tables ou colonnes
- Structure de base de données

**Exemple :**
```markdown
## Données Métier

### Prompt
- **Description** : Un modèle de question ou instruction pré-rédigé pour l'IA
- **Informations métier** :
  - Titre du prompt
  - Catégorie (technique, marketing, RH, etc.)
  - Créateur du prompt
  - Date de création
  - Nombre d'utilisations total
  - Statut (actif, archivé)

### Statistique d'utilisation
- **Description** : Agrégation des utilisations d'un prompt sur une période
- **Informations métier** :
  - Prompt concerné
  - Période (jour, semaine, mois)
  - Nombre d'utilisations
  - Nombre d'utilisateurs uniques (anonymisé)
  - Score de satisfaction moyen (si disponible)
```

### Section "Dépendances Métier"

**DOIT contenir :**
- Dépendances vers autres requirements
- Liens avec processus métier existants
- Prérequis métier

**Exemple :**
```markdown
## Dépendances Métier

- **REQ-005** : Gestion des prompts favoris (les prompts favoris doivent être identifiables dans les statistiques)
- **REQ-012** : Système d'authentification (nécessaire pour l'accès au tableau de bord)
- **Processus externe** : Validation marketing mensuelle des KPIs (les métriques affichées doivent être approuvées)
```

### Section "Critères de Succès"

**DOIT contenir :**
- Critères mesurables de succès métier
- Indicateurs de performance métier (KPI)
- Seuils de validation

**Exemple :**
```markdown
## Critères de Succès

- [ ] L'équipe marketing utilise le tableau de bord au moins 1 fois par semaine
- [ ] 90% des décisions d'amélioration de prompts sont basées sur les statistiques
- [ ] Le temps de décision pour améliorer la bibliothèque est réduit de 50%
- [ ] Le taux de satisfaction utilisateur sur les prompts proposés augmente de 20%
- [ ] 100% des acteurs concernés sont formés à l'utilisation du tableau de bord
```

### Section "Références"

**DOIT contenir :**
- Liens vers documentation métier
- Processus métier existants
- Réglementation applicable
- ADR techniques associés (séparation fonctionnel/technique)

**Exemple :**
```markdown
## Références

- [Guide utilisateur - Prompts IA](lien-vers-doc-utilisateur)
- [Processus de validation marketing](lien-vers-processus)
- [RGPD - Anonymisation des données](lien-vers-reglementation)
- [ADR-023: Architecture du module statistiques](../adr/023-module-statistiques-architecture.adr.md) (décisions techniques)
- [ADR-024: Choix base de données temporelles](../adr/024-timeseries-database.adr.md) (décisions techniques)
```

## 🔄 Cycle de Vie des Requirements

### Statuts Possibles

**UTILISER UNIQUEMENT ces statuts :**

- **Draft** : Requirement en cours de rédaction, pas encore validé
- **Validé** : Requirement approuvé par le métier, prêt pour implémentation
- **Implémenté** : Requirement développé et déployé en production
- **Obsolète** : Requirement devenu obsolète sans remplacement
- **Remplacé par REQ-XXX** : Requirement remplacé par un nouveau

### Principe d'Immutabilité Partielle

**RÈGLE** : Un requirement validé ou implémenté NE DOIT PAS être modifié dans son fond.

**Actions autorisées :**
- ✅ Changer le statut (Draft → Validé → Implémenté)
- ✅ Ajouter des clarifications mineures (section "Notes" ou "Précisions")
- ✅ Corriger des fautes de frappe
- ✅ Ajouter des références complémentaires

**Actions INTERDITES :**
- ❌ Modifier les exigences fonctionnelles
- ❌ Changer les règles métier
- ❌ Supprimer des scénarios d'usage
- ❌ Modifier les critères de succès

**Pour faire évoluer un requirement :**
1. Créer un NOUVEAU requirement avec le numéro suivant
2. Référencer le requirement précédent dans le contexte
3. Marquer l'ancien comme "Remplacé par REQ-XXX"
4. Mettre à jour l'index dans `docs/requirements/README.md`

**Exemple d'obsolescence :**
```markdown
# REQ-005. Gestion des prompts favoris (v1)

Date: 2025-06-15

## Statut

~~Validé~~ **Remplacé par REQ-025** (2025-12-06)

**Raison du remplacement** : Nouvelle approche avec système de collections suite aux retours utilisateurs demandant plus de flexibilité dans l'organisation des prompts.

[... reste du requirement inchangé ...]
```

## 📋 Processus de Création d'un Requirement

### Étape 1: Identifier un Besoin Métier

**Créer un requirement lorsque :**
- ✅ Un nouveau besoin métier est identifié
- ✅ Un processus métier doit être digitalisé
- ✅ Une nouvelle fonctionnalité est demandée par les utilisateurs
- ✅ Une amélioration significative est nécessaire
- ✅ Une contrainte réglementaire impose un changement

**NE PAS créer de requirement pour :**
- ❌ Corrections de bugs (créer une issue)
- ❌ Améliorations techniques sans impact métier visible
- ❌ Décisions d'architecture (créer un ADR)
- ❌ Refactoring technique (créer une tâche technique)

### Étape 2: Identifier le Domaine/Sous-domaine

**Analyser le besoin et identifier le domaine fonctionnel approprié :**

```powershell
# Lister les domaines existants
Get-ChildItem docs\requirements\ -Directory

# Analyser les requirements du domaine cible
Get-ChildItem docs\requirements\prompts\**\*.requirements.md | Sort-Object Name

# Si nouveau domaine nécessaire, le créer avec son README.md
New-Item docs\requirements\nouveau-domaine -ItemType Directory
New-Item docs\requirements\nouveau-domaine\README.md -ItemType File
```

**RÈGLES de choix du domaine :**
1. Utiliser un domaine existant si cohérence fonctionnelle
2. Créer un nouveau domaine si nouveau périmètre métier distinct
3. Limiter à 2 niveaux : domaine/sous-domaine (pas plus profond)
4. Nommer les domaines en kebab-case (minuscules, tirets)

### Étape 3: Obtenir le Numéro Séquentiel Global

**IMPORTANT** : La numérotation est GLOBALE sur tout le projet, pas par domaine.

```powershell
# Lister TOUS les requirements de TOUS les domaines pour identifier le prochain numéro
Get-ChildItem docs\requirements\**\*.requirements.md -Recurse | 
    ForEach-Object { 
        if ($_.Name -match '^(\d+)-') { [int]$matches[1] }
    } | 
    Sort-Object | 
    Select-Object -Last 1

# Résultat: 047 → prochain numéro = 048
$nextNumber = "{0:D3}" -f 48
Write-Host "Prochain numéro global: $nextNumber"
```

### Étape 4: Vérifier Absence de Duplication

**OBLIGATOIRE avant de créer le requirement :**

```powershell
# Rechercher requirements similaires par mots-clés
$keywords = @("prompt", "favori", "bookmark")
Get-ChildItem docs\requirements\**\*.requirements.md -Recurse | 
    ForEach-Object {
        $content = Get-Content $_.FullName -Raw
        foreach ($keyword in $keywords) {
            if ($content -match $keyword) {
                Write-Host "ATTENTION: $($_.Name) contient '$keyword'" -ForegroundColor Yellow
                Write-Host "  Chemin: $($_.FullName)" -ForegroundColor Gray
            }
        }
    }

# Lire les requirements potentiellement similaires
# Vérifier qu'il n'y a pas de chevauchement fonctionnel
# Si duplication détectée → ARRÊTER et clarifier le besoin
```

### Étape 5: Créer le Fichier dans le Domaine Approprié

```powershell
# Créer le fichier dans le bon domaine/sous-domaine
$domain = "prompts"
$subdomain = "favorites"
$number = "048"
$title = "favorite-collections"

$filePath = "docs\requirements\$domain\$subdomain\$number-$title.requirements.md"
New-Item $filePath -ItemType File

Write-Host "✅ Fichier créé: $filePath" -ForegroundColor Green
```

### Étape 6: Rédiger le Contenu

1. **Commencer par le contexte métier** : Décrire le besoin objectivement
2. **Définir les objectifs métier** : Qu'est-ce qui doit être atteint ?
3. **Lister les acteurs** : Qui est concerné ?
4. **Détailler les exigences** : Fonctionnelles, règles métier, scénarios
5. **Ajouter les contraintes** : Métier, réglementaires, organisationnelles
6. **Définir relations avec autres requirements** : Dépendances, amendements, désaveux
7. **Rédiger critères d'acceptation complets** : Cas nominaux, limites, exceptions
8. **Vérifier cohérence** : Pas de duplication, pas de contradiction
9. **Définir les critères de succès métier** : KPI et métriques mesurables

**Principes de rédaction :**
- ✅ **Vocabulaire métier** : Termes compréhensibles par les utilisateurs finaux
- ✅ **Pas de jargon technique** : Éviter les termes d'implémentation
- ✅ **Complétude** : Toutes les informations nécessaires à la compréhension
- ✅ **Mesurabilité** : Critères vérifiables et testables
- ❌ **Éviter les détails techniques** : Pas de mention de technologies, frameworks, architectures

### Étape 7: Mettre à Jour les Index

**Mettre à jour DEUX index :**

1. **Index global** `docs/requirements/README.md` :

```markdown
| [048](prompts/favorites/048-favorite-collections.requirements.md) | Collections de favoris | Draft | 2025-12-06 | Prompts |
```

2. **Index du domaine** `docs/requirements/prompts/README.md` :

```markdown
### Favorites

| ID  | Titre | Statut | Date |
|-----|-------|--------|------|
| [020](favorites/020-favorite-prompts.requirements.md) | Prompts favoris | Implémenté | 2025-11-15 |
| [048](favorites/048-favorite-collections.requirements.md) | Collections de favoris | Draft | 2025-12-06 |
```

**TOUJOURS maintenir les index triés par numéro croissant.**

### Étape 8: Vérification Finale de Cohérence

**OBLIGATOIRE avant validation métier :**

```powershell
# Script de vérification de cohérence
$newReqFile = "docs\requirements\prompts\favorites\048-favorite-collections.requirements.md"
$newContent = Get-Content $newReqFile -Raw

# 1. Vérifier références valides
$references = [regex]::Matches($newContent, 'REQ-(\d+)')
foreach ($ref in $references) {
    $reqNum = $ref.Groups[1].Value
    $exists = Get-ChildItem docs\requirements\**\$reqNum-*.requirements.md -Recurse -ErrorAction SilentlyContinue
    if (-not $exists) {
        Write-Host "❌ ERREUR: REQ-$reqNum référencé n'existe pas" -ForegroundColor Red
    }
}

# 2. Vérifier termes du glossaire
$glossary = Get-Content docs\glossary.md -Raw -ErrorAction SilentlyContinue
if ($glossary) {
    # Extraire termes métier et vérifier définitions cohérentes
}

# 3. Vérifier contradictions potentielles
$allRequirements = Get-ChildItem docs\requirements\**\*.requirements.md -Recurse
foreach ($req in $allRequirements) {
    if ($req.FullName -ne $newReqFile) {
        # Comparer règles métier, données métier, etc.
        # Alerter si contradictions potentielles
    }
}

Write-Host "✅ Vérification de cohérence terminée" -ForegroundColor Green
```

### Étape 9: Validation Métier

- Soumettre le requirement au Product Owner / Métier
- Organiser une review avec les parties prenantes
- Ajuster selon les retours (tant que statut = "Draft")
- Changer le statut à "Validé" une fois approuvé

### Étape 10: Implémentation

1. Créer les ADR techniques nécessaires (architecture, choix technologiques)
2. Créer les tâches techniques d'implémentation
3. Développer et tester
4. Déployer en production
5. Changer le statut à "Implémenté"

## 🔗 Relation Requirements ↔ ADR

### Principe de Séparation

**RÈGLE ABSOLUE :**
- `*.requirements.md` = **QUOI** (besoin métier, fonctionnalité attendue)
- `*.adr.md` = **COMMENT** (décision technique d'implémentation)

### Workflow Recommandé

```
1. Besoin métier identifié
   ↓
2. Création REQ-XXX.requirements.md (QUOI métier)
   ↓
3. Validation métier
   ↓
4. Analyse technique
   ↓
5. Création ADR-YYY.adr.md (COMMENT technique)
   ↓
6. Implémentation
   ↓
7. Déploiement
   ↓
8. Mise à jour statuts (REQ: Implémenté, ADR: Accepté)
```

### Références Croisées

**Dans un requirement, référencer les ADR associés :**
```markdown
## Références

- [ADR-023: Architecture du module statistiques](../adr/023-module-statistiques-architecture.adr.md)
- [ADR-024: Choix base de données temporelles](../adr/024-timeseries-database.adr.md)
```

**Dans un ADR, référencer le requirement source :**
```markdown
## Contexte

Ce choix architectural découle du besoin métier décrit dans [REQ-001: Tableau de bord statistiques](../requirements/001-tableau-de-bord-statistiques.requirements.md).

[...]
```

### Exemples de Séparation

**Besoin métier → Requirement :**
```markdown
# REQ-001. Tableau de bord statistiques des prompts

## Exigences Fonctionnelles

### EF-001.1 - Affichage du top 10 des prompts
- **Description** : Afficher les 10 prompts les plus utilisés sur les 30 derniers jours
- **Priorité** : Haute
```

**Décision technique → ADR :**
```markdown
# ADR-023. Architecture du module statistiques

## Décision

Utiliser une architecture CQRS avec projections read-optimized pour les statistiques.

- Command : Enregistrement des utilisations de prompts
- Query : Lecture des statistiques agrégées depuis projections
- Event Sourcing : Historique complet des utilisations
```

## ✅ Checklist de Validation

**AVANT de considérer un requirement comme terminé :**

### Structure et Formatage
- [ ] Extension `.requirements.md` utilisée
- [ ] Numérotation correcte (NNN avec padding de zéros, unique dans tout le projet)
- [ ] Fichier placé dans domaine/sous-domaine approprié (`docs/requirements/{domaine}/{sous-domaine}/`)
- [ ] Domaine cohérent avec le contenu fonctionnel
- [ ] Maximum 2 niveaux de profondeur (domaine/sous-domaine)
- [ ] Frontmatter présent avec titre et date
- [ ] Statut défini (Draft, Validé, Implémenté, Obsolète, Remplacé)
- [ ] Syntaxe Markdown valide

### Contenu Fonctionnel
- [ ] Section "Contexte Métier" : Besoin métier clairement décrit
- [ ] Section "Objectifs Métier" : Objectifs listés et mesurables
- [ ] Section "Acteurs Concernés" : Tous les rôles identifiés
- [ ] Section "Exigences Fonctionnelles" : Au moins 3-5 exigences détaillées
- [ ] Section "Règles Métier" : Règles explicites et non ambiguës
- [ ] Section "Scénarios d'Usage" : Au moins 2-3 scénarios complets
- [ ] Section "Critères d'Acceptation Globaux" : **OBLIGATOIRE**
  - [ ] Cas nominaux (happy path) couverts exhaustivement
  - [ ] Cas aux limites (edge cases) identifiés et testables
  - [ ] Cas d'exception (error cases) documentés avec gestion attendue
  - [ ] Critères non fonctionnels (performance, sécurité, utilisabilité)
  - [ ] Critères de conformité (réglementaire, standards internes)
- [ ] Section "Exigences Non Fonctionnelles" : Performance, accessibilité, etc. (vue métier)
- [ ] Section "Contraintes Métier" : Contraintes organisationnelles identifiées
- [ ] Section "Données Métier" : Entités métier décrites (sans détails techniques)
- [ ] Section "Relations avec Autres Requirements" : **OBLIGATOIRE**
  - [ ] Dépendances (prérequis) identifiées
  - [ ] Successeurs (impacte) listés
  - [ ] Amendements documentés si applicable
  - [ ] Désaveux explicités si remplacement
  - [ ] Conflits potentiels identifiés
- [ ] Section "Analyse de Cohérence" : **OBLIGATOIRE**
  - [ ] Vérification de non-duplication effectuée
  - [ ] Vérification de non-contradiction effectuée
  - [ ] Terminologie cohérente avec glossaire
  - [ ] Aucune dépendance circulaire
- [ ] Section "Critères de Succès Métier" : KPI et métriques mesurables définis

### Qualité du Contenu
- [ ] Vocabulaire métier utilisé (pas de jargon technique)
- [ ] AUCUNE décision technique (architecture, techno, patterns)
- [ ] **Critères d'acceptation COMPLETS, REPRÉSENTATIFS et PERTINENTS**
  - [ ] TOUS les cas nominaux couverts
  - [ ] TOUS les cas aux limites identifiés
  - [ ] TOUTES les exceptions documentées
  - [ ] Critères vérifiables et testables
  - [ ] Critères mesurables avec seuils définis
- [ ] Scénarios complets (flux nominal + alternatifs + exceptions)
- [ ] Règles métier non ambiguës et vérifiables
- [ ] Contraintes RGPD/réglementaires identifiées si applicable
- [ ] **Aucune duplication avec requirements existants (vérifié)**
- [ ] **Aucune contradiction avec requirements existants (vérifié)**
- [ ] **Relations clairement définies (dépendances, amendements, désaveux)**

### Références et Index
- [ ] Références vers ADR techniques associés (séparation fonctionnel/technique)
- [ ] Références vers documentation métier pertinente
- [ ] **Toutes les références REQ-XXX sont valides** (requirements existent)
- [ ] Glossaire métier référencé pour termes techniques
- [ ] Traçabilité complète (origine, historique, implémentation)
- [ ] **Index global mis à jour** dans `docs/requirements/README.md`
- [ ] **Index du domaine mis à jour** dans `docs/requirements/{domaine}/README.md`

### Validation Métier
- [ ] Requirement reviewé par le Product Owner
- [ ] Requirement validé par les parties prenantes métier
- [ ] Critères de succès approuvés par le métier

## 💡 Exemples Complets

### Exemple 1 : Requirement Simple et Clair

```markdown
# REQ-003. Gestion des prompts favoris

Date: 2025-12-06

## Statut

Validé

## Contexte Métier

Les utilisateurs de la plateforme IA utilisent fréquemment les mêmes prompts pour leurs tâches quotidiennes.

Actuellement, ils doivent :
- Rechercher le prompt dans la bibliothèque complète à chaque utilisation
- Mémoriser les titres exacts des prompts qu'ils utilisent souvent
- Parcourir de longues listes pour retrouver leurs prompts préférés

Cette situation entraîne :
- Perte de temps (30 secondes à 2 minutes par recherche)
- Frustration utilisateur
- Sous-utilisation de la bibliothèque de prompts

Ce requirement vise à permettre aux utilisateurs de marquer leurs prompts favoris pour un accès rapide.

## Objectifs Métier

- Réduire le temps de recherche d'un prompt fréquemment utilisé de 90 secondes à 5 secondes
- Améliorer la satisfaction utilisateur sur la fonctionnalité de prompts
- Augmenter l'utilisation de la bibliothèque de prompts de 30%
- Faciliter la découverte de nouveaux prompts similaires aux favoris

## Acteurs Concernés

- **Utilisateur Final** : Marque des prompts comme favoris, accède rapidement à ses favoris
- **Administrateur** : Consulte les prompts les plus mis en favoris pour identifier les besoins

## Exigences Fonctionnelles

### EF-003.1 - Marquer un prompt comme favori
- **Description** : L'utilisateur doit pouvoir marquer n'importe quel prompt de la bibliothèque comme favori d'un simple clic
- **Priorité** : Critique
- **Critères de validation** :
  - Icône "étoile" visible sur chaque prompt de la bibliothèque
  - Clic sur l'étoile marque/démarque le favori instantanément
  - État favori visible immédiatement (étoile pleine)
  - État persisté après rechargement de la page

### EF-003.2 - Afficher la liste des favoris
- **Description** : L'utilisateur doit avoir accès à une vue dédiée listant tous ses prompts favoris
- **Priorité** : Critique
- **Critères de validation** :
  - Vue "Mes Favoris" accessible depuis le menu principal
  - Liste affiche tous les prompts marqués comme favoris
  - Ordre de tri par date d'ajout aux favoris (plus récent en premier)
  - Possibilité de retirer un favori directement depuis cette vue

### EF-003.3 - Recherche dans les favoris
- **Description** : L'utilisateur doit pouvoir rechercher un prompt parmi ses favoris par titre ou catégorie
- **Priorité** : Moyenne
- **Critères de validation** :
  - Champ de recherche présent en haut de la liste des favoris
  - Recherche instantanée (sans clic sur "Rechercher")
  - Recherche insensible à la casse
  - Recherche sur titre et catégorie du prompt

### EF-003.4 - Limitation du nombre de favoris
- **Description** : Un utilisateur peut marquer jusqu'à 50 prompts comme favoris maximum
- **Priorité** : Basse
- **Critères de validation** :
  - Message d'information si limite atteinte
  - Proposition de retirer un ancien favori pour en ajouter un nouveau
  - Compteur visible "X/50 favoris" dans la vue "Mes Favoris"

## Règles Métier

### RG-003.1 - Favoris personnels uniquement
- **Description** : Les favoris d'un utilisateur sont strictement personnels et ne sont pas visibles par les autres utilisateurs
- **Conditions** : Toujours
- **Actions** : 
  - Chaque utilisateur a sa propre liste de favoris
  - Pas de partage de favoris entre utilisateurs
  - Pas d'affichage public des favoris

### RG-003.2 - Favoris disponibles sur tous les appareils
- **Description** : Les favoris d'un utilisateur sont synchronisés entre tous ses appareils
- **Conditions** : Si l'utilisateur se connecte depuis plusieurs appareils
- **Actions** : 
  - Marquer un favori sur appareil A le rend visible sur appareil B
  - Synchronisation automatique à la connexion
  - Pas de décalage entre appareils

### RG-003.3 - Conservation des favoris si prompt archivé
- **Description** : Si un prompt marqué comme favori est archivé par l'administrateur, il reste accessible dans les favoris de l'utilisateur
- **Conditions** : Si un administrateur archive un prompt
- **Actions** : 
  - Le prompt reste visible dans "Mes Favoris" de l'utilisateur
  - Un badge "Archivé" est affiché sur le prompt
  - L'utilisateur peut toujours l'utiliser
  - L'utilisateur peut choisir de le retirer de ses favoris

## Scénarios d'Usage

### Scénario 1: Marquer un prompt comme favori
**Acteur** : Utilisateur Final

**Objectif** : Sauvegarder un prompt pour un accès rapide ultérieur

**Préconditions** :
- L'utilisateur est authentifié
- L'utilisateur consulte la bibliothèque de prompts
- Le prompt n'est pas déjà en favori

**Flux nominal** :
1. L'utilisateur parcourt la bibliothèque de prompts
2. L'utilisateur trouve un prompt intéressant
3. L'utilisateur clique sur l'icône "étoile" à côté du prompt
4. Le système marque le prompt comme favori
5. L'étoile devient pleine pour indiquer l'état favori
6. Une notification subtile confirme "Ajouté aux favoris"

**Flux alternatifs** :
- **3a.** Si la limite de 50 favoris est atteinte :
  1. Le système affiche un message "Limite de 50 favoris atteinte"
  2. Le système propose de retirer un ancien favori
  3. L'utilisateur peut retirer un favori puis recommencer

**Postconditions** :
- Le prompt est marqué comme favori
- Le prompt apparaît dans la vue "Mes Favoris"
- L'état est persisté en base de données

**Critères d'acceptation** :
- [ ] L'icône étoile change d'état visuellement
- [ ] Le favori apparaît immédiatement dans "Mes Favoris"
- [ ] Le favori est toujours présent après rechargement de la page
- [ ] La notification de confirmation est affichée

### Scénario 2: Accéder rapidement à un prompt favori
**Acteur** : Utilisateur Final

**Objectif** : Utiliser un prompt fréquemment utilisé sans le rechercher

**Préconditions** :
- L'utilisateur est authentifié
- L'utilisateur a au moins un prompt en favori

**Flux nominal** :
1. L'utilisateur clique sur "Mes Favoris" dans le menu
2. Le système affiche la liste des prompts favoris (triés par date)
3. L'utilisateur identifie visuellement le prompt recherché
4. L'utilisateur clique sur le prompt
5. Le système ouvre le prompt pour utilisation

**Flux alternatifs** :
- **2a.** Si aucun favori : afficher "Vous n'avez pas encore de prompts favoris. Explorez la bibliothèque !"
- **3a.** Si nombreux favoris, utiliser la recherche pour filtrer

**Postconditions** :
- Le prompt est prêt à l'emploi
- L'utilisateur peut l'utiliser immédiatement

**Critères d'acceptation** :
- [ ] Temps d'accès au prompt < 5 secondes
- [ ] Liste des favoris chargée en < 1 seconde
- [ ] Message clair si aucun favori

## Exigences Non Fonctionnelles

### ENF-003.1 - Performance
- Marquer/démarquer un favori doit être instantané (< 500ms ressenti)
- Affichage de la liste des favoris en moins de 1 seconde
- Recherche dans les favoris instantanée (< 300ms)

### ENF-003.2 - Utilisabilité
- Icône "étoile" universellement reconnaissable
- État favori visible sans ambiguïté (étoile pleine vs vide)
- Pas de confirmation nécessaire pour marquer un favori (action réversible)
- Recherche dans favoris accessible au clavier (Tab + Entrée)

### ENF-003.3 - Disponibilité
- Fonctionnalité disponible 24/7
- Synchronisation des favoris entre appareils en temps réel (< 5 secondes)

## Contraintes Métier

- **Limite de 50 favoris** : Pour éviter que les favoris deviennent une deuxième bibliothèque encombrée
- **Pas de partage** : Contrainte de confidentialité, les favoris sont strictement personnels
- **Conservation si archivé** : Les utilisateurs doivent pouvoir continuer à utiliser leurs prompts favoris même si l'admin les archive

## Données Métier

### Prompt Favori
- **Description** : Association entre un utilisateur et un prompt qu'il a marqué comme favori
- **Informations métier** :
  - Utilisateur propriétaire
  - Prompt concerné
  - Date d'ajout aux favoris
  - Position dans la liste (ordre personnalisé, optionnel pour version future)

### Utilisateur
- **Description** : Personne utilisant la plateforme
- **Informations métier** (liées aux favoris) :
  - Identifiant unique
  - Liste de prompts favoris (max 50)
  - Date de dernière modification des favoris

## Dépendances Métier

- **REQ-001** : Gestion de la bibliothèque de prompts (les prompts doivent exister pour être mis en favoris)
- **REQ-002** : Système d'authentification (nécessaire pour identifier l'utilisateur et ses favoris)

## Critères de Succès

- [ ] 60% des utilisateurs actifs utilisent la fonctionnalité favoris dans le premier mois
- [ ] Temps moyen de recherche d'un prompt passe de 90s à < 10s pour les utilisateurs utilisant les favoris
- [ ] Satisfaction utilisateur sur la fonctionnalité ≥ 4/5 (sondage post-déploiement)
- [ ] Taux d'utilisation de la bibliothèque de prompts augmente de 30%
- [ ] 0 bugs critiques rapportés dans les 2 semaines suivant le déploiement

## Références

- [Guide utilisateur - Bibliothèque de prompts](lien-doc)
- [Étude UX - Comportements utilisateurs](lien-etude)
- [ADR-025: Architecture module favoris](../adr/025-module-favoris-architecture.adr.md) (décisions techniques)
```

## 🎓 Bonnes Pratiques

### Pour la Rédaction

1. **Utiliser le vocabulaire métier** : Termes compris par les utilisateurs finaux, cohérents avec le glossaire
2. **Éviter le jargon technique** : Pas de mention de frameworks, architectures, technologies
3. **Être précis et mesurable** : Critères vérifiables et testables avec seuils définis
4. **Penser à l'utilisateur final** : Rédiger du point de vue métier, pas développeur
5. **Documenter TOUS les cas exhaustivement** :
   - Flux nominal (happy path) complet
   - Tous les flux alternatifs identifiés
   - Tous les cas aux limites (edge cases)
   - Toutes les exceptions et gestions d'erreur
6. **Organiser par domaine cohérent** : Placer le requirement dans le bon domaine/sous-domaine métier
7. **Définir relations explicitement** : Dépendances, amendements, désaveux clairement documentés
8. **Vérifier cohérence systématiquement** : Pas de duplication, pas de contradiction avant de finaliser

### Pour la Maintenance

1. **Ne PAS modifier un requirement validé/implémenté** : Créer un nouveau requirement
2. **Maintenir les index à jour** : 
   - Index global `docs/requirements/README.md`
   - Index de chaque domaine `docs/requirements/{domaine}/README.md`
3. **Vérifier cohérence lors de modifications** : 
   - Pas de nouvelles contradictions introduites
   - Relations mises à jour (dépendances, amendements)
   - Numérotation globale respectée
4. **Créer des ADR pour les décisions techniques** : Séparer fonctionnel (REQ) et technique (ADR)
5. **Archiver les requirements obsolètes** : Garder la trace, ne pas supprimer
6. **Réorganiser domaines si nécessaire** : Cohérence métier prime sur stabilité des chemins
7. **Documenter amendements/désaveux** : Tracer l'évolution des requirements dans les relations

### Pour la Validation Métier

1. **Impliquer le Product Owner** : Validation obligatoire avant passage à "Validé"
2. **Organiser des reviews** : Avec toutes les parties prenantes
3. **Vérifier la complétude exhaustive** : 
   - Tous les scénarios nominaux couverts
   - Tous les cas aux limites identifiés
   - Toutes les exceptions documentées
   - Toutes les règles métier explicites
4. **Valider les critères d'acceptation** : 
   - Complets (couvrent 100% du requirement)
   - Représentatifs (cas réels du métier)
   - Pertinents (testables et mesurables)
   - Réalistes (atteignables techniquement)
5. **Valider les critères de succès métier** : KPI réalistes et mesurables
6. **Vérifier absence de duplication** : Pas de chevauchement avec requirements existants
7. **Vérifier cohérence globale** : Pas de contradiction avec le corpus existant
8. **Valider les relations** : Dépendances, amendements, désaveux justifiés

### Pour l'Implémentation

1. **Créer les ADR nécessaires** : Décisions techniques documentées séparément
2. **Référencer le requirement dans le code** : Commentaires avec REQ-XXX
3. **Créer des tests basés sur les critères d'acceptation** : Tests automatisés alignés sur le requirement
4. **Mettre à jour le statut** : "Implémenté" une fois en production

## Format de Rédaction : Langage Directif et Actionnable

**RÈGLE ABSOLUE** : Utiliser un langage DIRECTIF avec verbes à l'impératif ou indicatif présent.

### Verbes Recommandés (Directifs)

**Pour les exigences :**
- ✅ **DOIT** / **DOIT ÊTRE** : Exigence obligatoire non négociable
- ✅ **DEVRAIT** / **DEVRAIT ÊTRE** : Exigence fortement recommandée
- ✅ **PEUT** / **PEUT ÊTRE** : Exigence optionnelle
- ✅ **NE DOIT PAS** : Interdiction explicite

**Exemples :**
- ✅ "Le système DOIT afficher la liste en moins de 2 secondes"
- ✅ "L'utilisateur NE DOIT PAS pouvoir supprimer un prompt partagé"
- ✅ "L'application DEVRAIT envoyer une notification de confirmation"

### Verbes d'Action pour Scénarios

- **Consulter, Afficher, Visualiser** : Lecture d'information
- **Créer, Ajouter, Saisir** : Création de données
- **Modifier, Mettre à jour, Éditer** : Modification de données
- **Supprimer, Retirer, Effacer** : Suppression de données
- **Rechercher, Filtrer, Trier** : Recherche et organisation
- **Valider, Approuver, Rejeter** : Workflow de validation
- **Exporter, Importer, Télécharger** : Échange de données

### Verbes à ÉVITER (Ambigus)

- ❌ "Permettre" → ✅ "L'utilisateur PEUT"
- ❌ "Gérer" → ✅ "Créer, Modifier, Supprimer"
- ❌ "Il faut" → ✅ "Le système DOIT"
- ❌ "Il serait bien de" → ✅ "DEVRAIT" ou supprimer si non essentiel
- ❌ "Pouvoir" → ✅ "PEUT"

### Structure de Phrase Directive

**Format : [Sujet] + [VERBE DIRECTIF] + [Action précise] + [Seuil quantifié si applicable]**

**Exemples :**
- ✅ "Le système DOIT enregistrer chaque action utilisateur dans les logs d'audit"
- ✅ "L'administrateur PEUT exporter les statistiques au format CSV"
- ✅ "L'application NE DOIT PAS stocker les mots de passe en clair"
- ✅ "Le tableau de bord DOIT se rafraîchir automatiquement toutes les 5 minutes"

## Exemples Comparatifs : Mauvais vs Bon

### ❌ Requirement MAL RÉDIGÉ (Vague, Non Actionnable)

```markdown
## Exigences Fonctionnelles

### EF-001.1 - Affichage des favoris
- **Description** : L'utilisateur doit pouvoir voir ses prompts favoris
- **Priorité** : Haute

## Règles Métier

### RG-001.1 - Limitation
- Il faut limiter le nombre de favoris

## Critères d'Acceptation
- [ ] Les favoris s'affichent correctement
- [ ] Le système est rapide
```

**Problèmes :**
- ❌ "pouvoir voir" : vague, pas de détails
- ❌ "Il faut limiter" : pas directif, seuil manquant
- ❌ "correctement" : non mesurable
- ❌ "rapide" : non quantifié

### ✅ Requirement BIEN RÉDIGÉ (Précis, Spécifique, Directif, Actionnable)

```markdown
## Exigences Fonctionnelles

### EF-001.1 - Affichage de la liste des prompts favoris
- **Description** : L'application DOIT afficher la liste complète des prompts marqués comme favoris par l'utilisateur connecté, triée par date d'ajout décroissante (plus récent en premier)
- **Priorité** : Critique
- **Justification priorité** : Fonctionnalité centrale du module favoris, bloquante pour 60% des utilisateurs quotidiens
- **Critères de validation** :
  - La liste contient TOUS les prompts marqués comme favoris (aucun oublié)
  - L'ordre de tri est décroissant par date d'ajout (vérifié avec 3 prompts de dates différentes)
- **Exemples concrets** :
  - **Exemple 1** : Utilisateur avec 5 favoris ajoutés dans l'ordre A(01/12), B(03/12), C(02/12), D(05/12), E(04/12) → Affichage : D, E, B, C, A
  - **Exemple 2** : Utilisateur sans aucun favori → Message "Aucun prompt favori. Explorez la bibliothèque !"
- **Seuils quantifiés** :
  - Volume minimum : 0 éléments (liste vide gérée)
  - Volume maximum : 50 éléments (limite métier)
  - Temps de réponse : < 1 seconde (mesure P95)

## Règles Métier

### RG-001.1 - Limitation du nombre de favoris par utilisateur
- **Description** : Un utilisateur NE DOIT PAS pouvoir marquer plus de 50 prompts comme favoris simultanément
- **Conditions** : Toujours, pour tous les utilisateurs (aucune exception)
- **Actions** : 
  - Si l'utilisateur tente de marquer un 51ème favori, le système DOIT afficher le message : "Limite de 50 favoris atteinte. Retirez un favori pour en ajouter un nouveau."
  - Le système NE DOIT PAS ajouter le nouveau favori
  - Le compteur "X/50 favoris" DOIT rester à 50/50
- **Formule/Algorithme métier** : `NombreFavoris ≤ 50`
- **Origine** : Décision product owner (limiter l'encombrement, focus sur l'essentiel)
- **Vérifiable par** : Test automatisé avec utilisateur ayant 50 favoris tentant d'en ajouter un 51ème

## Critères d'Acceptation Globaux

### Critères Fonctionnels

**Cas Nominaux (Happy Path) :**
- [ ] **CFN-1** : Étant donné un utilisateur avec 5 prompts favoris, quand il accède à "Mes Favoris", alors les 5 prompts s'affichent triés par date décroissante en moins de 1 seconde
- [ ] **CFN-2** : Étant donné un utilisateur avec 50 prompts favoris (limite atteinte), quand il affiche la liste, alors le compteur affiche "50/50 favoris" et tous les prompts sont visibles

**Cas aux Limites (Edge Cases) :**
- [ ] **CFL-1** : Étant donné un utilisateur sans aucun favori (0), quand il accède à "Mes Favoris", alors le message "Aucun prompt favori. Explorez la bibliothèque !" s'affiche (pas de liste vide)
- [ ] **CFL-2** : Étant donné un utilisateur avec 50 favoris (limite), quand il tente de marquer un 51ème prompt, alors un message d'erreur explicite s'affiche et l'action est refusée

**Cas d'Exception (Error Cases) :**
- [ ] **CFE-1** : Étant donné une erreur réseau lors du chargement, quand le timeout de 5 secondes est atteint, alors le message "Impossible de charger vos favoris. Vérifiez votre connexion." s'affiche avec bouton "Réessayer"
```

**Améliorations :**
- ✅ Verbes directifs : "DOIT", "NE DOIT PAS"
- ✅ Seuils quantifiés : 50 favoris, < 1 seconde, P95
- ✅ Exemples concrets avec données réelles
- ✅ Critères testables au format Given-When-Then
- ✅ Messages d'erreur exacts (pas "un message d'erreur")
- ✅ Comportements précis à chaque étape

## 🔗 Références

- [User Story Mapping - Jeff Patton](https://www.jpattonassociates.com/user-story-mapping/)
- [Writing Effective Use Cases - Alistair Cockburn](https://www.amazon.com/Writing-Effective-Cases-Alistair-Cockburn/dp/0201702258)
- [Specification by Example - Gojko Adzic](https://www.amazon.com/Specification-Example-Successful-Deliver-Software/dp/1617290084)
- [IEEE 29148 - Requirements Engineering Standard](https://standards.ieee.org/standard/29148-2018.html)
- [BABOK Guide - Business Analysis Body of Knowledge](https://www.iiba.org/business-analysis-certifications/babok/)
- [RFC 2119 - Key words for use in RFCs (MUST, SHOULD, MAY)](https://www.ietf.org/rfc/rfc2119.txt)
- [Gherkin Language - Given-When-Then](https://cucumber.io/docs/gherkin/reference/)
- Copilot instructions: `.github/copilot-instructions.md` (section Documentation Fonctionnelle)
