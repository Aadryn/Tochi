---
applyTo: "documentations/functionnals/**/*.epic.md,documentations/functionnals/*.epic.md"
---

# Instructions pour la Rédaction des EPICs

## Objectif
Une EPIC représente un ensemble cohérent de fonctionnalités majeures alignées sur un objectif business stratégique. Elle regroupe plusieurs FEATUREs et sert de vision macro pour un domaine fonctionnel.

## Principes Directeurs
- **Toujours écrire en français** avec un ton pragmatique, descriptif, argumentatif, précis, logique et professionnel
- **Rester agnostique de la technologie** : focus sur le besoin métier, pas l'implémentation technique
- **Fournir une vision claire** : alignement avec les objectifs business et la stratégie d'entreprise
- **Être exhaustif** : documenter tous les aspects (personas, périmètre, KPIs, risques, dépendances)

## Nomenclature Obligatoire
- **Format de fichier** : `EPIC-{ID:4 digits}-{slug-en-kebab-case}.epic.md`
- **Exemples** :
  - `EPIC-0001-gestion-utilisateurs.epic.md`
  - `EPIC-0002-gestion-ressources.epic.md`
  - `EPIC-0003-gestion-allocations.epic.md`

## Structure Obligatoire du Document

### 1. Métadonnées
```markdown
# EPIC-{ID} : {Titre Clair et Concis}

## Métadonnées
- **ID** : EPIC-{ID}
- **Statut** : [DRAFT | IN_PROGRESS | DONE | DEPRECATED]
- **Priorité** : [CRITIQUE | HAUTE | MOYENNE | BASSE]
- **Date de création** : YYYY-MM-DD
- **Dernière mise à jour** : YYYY-MM-DD
- **Auteur** : {Nom ou rôle}
- **Version** : X.Y.Z (SemVer)
```

**Règles** :
- L'ID doit être unique et ne JAMAIS être réutilisé
- Le statut reflète l'avancement global de toutes les features
- La priorité est définie selon l'impact business (CRITIQUE = bloquant, HAUTE = important, MOYENNE = souhaitable, BASSE = nice-to-have)
- Incrémenter la version à chaque modification (Major.Minor.Patch)

### 2. Vision
```markdown
## Vision
**En tant que** {type d'utilisateur / persona concerné},  
**Je veux** {objectif métier de haut niveau},  
**Afin de** {valeur business / bénéfice attendu mesurable}.
```

**Règles** :
- Utiliser le format User Story pour clarifier la valeur
- Rester au niveau stratégique (pas de détails d'implémentation)
- Expliquer le POURQUOI, pas le COMMENT
- Aligner avec les objectifs business et OKRs de l'entreprise

**Exemple** :
```markdown
## Vision
**En tant que** Responsable RH,  
**Je veux** disposer d'un système centralisé de gestion des ressources,  
**Afin de** optimiser l'allocation des collaborateurs sur les projets et réduire le temps de staffing de 40%.
```

### 3. Contexte Métier
```markdown
## Contexte Métier

### Problème à Résoudre
{Description détaillée du problème métier actuel : inefficacités, coûts, risques, opportunités manquées}

### Enjeux et Opportunités
- **Enjeux** :
  - {Enjeu stratégique 1 : ex. "Améliorer la visibilité sur les compétences disponibles"}
  - {Enjeu stratégique 2 : ex. "Réduire le temps de staffing de X jours à Y jours"}
  
- **Opportunités** :
  - {Opportunité business 1 : ex. "Augmenter le taux d'allocation des ressources de X% à Y%"}
  - {Opportunité business 2 : ex. "Améliorer la satisfaction client grâce à un meilleur matching"}

### Contraintes
- **Réglementaires** : {RGPD, normes sectorielles, obligations légales}
- **Organisationnelles** : {Processus existants à respecter, changements organisationnels requis}
- **Temporelles** : {Deadlines business, contraintes de déploiement, phases de migration}
- **Techniques (si pertinent fonctionnellement)** : {Contraintes architecturales impactant le fonctionnel, volumétrie, performance}
```

**Règles** :
- Quantifier les problèmes quand possible (temps perdu, coûts, erreurs)
- Lier aux objectifs stratégiques de l'entreprise
- Identifier les contraintes réelles (pas supposées)

### 4. Personas Concernés
```markdown
## Personas Concernés
| Persona | Rôle | Besoins Principaux | Fréquence d'Usage |
|---------|------|-------------------|-------------------|
| {Nom Persona} | {Rôle métier précis} | {Besoin 1, Besoin 2, Besoin 3} | {Quotidien/Hebdomadaire/Mensuel/Ponctuel} |
```

**Règles** :
- Référencer les personas définis dans les fichiers `*.personna.md`
- Lister tous les personas concernés par cette EPIC
- Préciser leurs besoins spécifiques et leur fréquence d'utilisation
- Prioriser les personas principaux vs secondaires

**Exemple** :
```markdown
| Persona | Rôle | Besoins Principaux | Fréquence d'Usage |
|---------|------|-------------------|-------------------|
| Marie Dupont | Responsable RH | Visualiser disponibilités, Affecter ressources, Suivre allocations | Quotidien |
| Jean Martin | Chef de Projet | Consulter ressources disponibles, Demander affectations | Hebdomadaire |
| Sophie Bernard | Collaborateur | Consulter ses affectations, Mettre à jour compétences | Mensuel |
```

### 5. Périmètre Fonctionnel
```markdown
## Périmètre Fonctionnel

### Dans le Périmètre (In Scope)
- ✅ {Fonctionnalité incluse 1 : description précise}
- ✅ {Fonctionnalité incluse 2}
- ✅ {Fonctionnalité incluse 3}

### Hors Périmètre (Out of Scope)
- ❌ {Fonctionnalité explicitement exclue 1 : avec justification}
- ❌ {Fonctionnalité explicitement exclue 2}

### Dépendances
- **Dépend de** :
  - EPIC-{ID} : {Titre} - {Raison précise de la dépendance}
  
- **Bloque** :
  - EPIC-{ID} : {Titre} - {Raison du blocage : cette EPIC doit être terminée pour permettre...}
```

**Règles** :
- Être explicite sur ce qui est inclus ET exclu (éviter les ambiguïtés)
- Justifier les exclusions (pourquoi pas maintenant ? phase ultérieure ?)
- Documenter toutes les dépendances avec d'autres EPICs
- Vérifier la cohérence bidirectionnelle des dépendances

### 6. Valeur Business
```markdown
## Valeur Business

### KPIs de Succès
| KPI | Valeur Actuelle | Valeur Cible | Mesure |
|-----|----------------|--------------|--------|
| {Nom du KPI mesurable} | {Valeur baseline} | {Objectif chiffré} | {Méthode de mesure précise} |

**Exemples** :
| KPI | Valeur Actuelle | Valeur Cible | Mesure |
|-----|----------------|--------------|--------|
| Temps moyen de staffing | 5 jours | 2 jours | Durée entre demande et affectation |
| Taux d'allocation des ressources | 65% | 85% | % temps facturable / temps total |
| Satisfaction client sur le matching | 70% | 90% | Score satisfaction post-projet |

### ROI Attendu
- **Gains** : {Gains quantifiables : réduction temps, coûts évités, productivité accrue}
- **Coûts évités** : {Coûts épargnés : erreurs, retravail, gaspillage}
- **Satisfaction utilisateur** : {Amélioration attendue : NPS, CSAT, adoption}
```

**Règles** :
- Définir 3 à 5 KPIs mesurables et atteignables
- Établir une baseline (valeur actuelle) et une cible chiffrée
- Préciser comment mesurer (outils, méthode, fréquence)
- Estimer le ROI de manière réaliste

### 7. Entités Métier Principales
```markdown
## Entités Métier Principales
| Entité | Description | Propriétés Clés | Cycle de Vie |
|--------|-------------|----------------|--------------|
| {Nom Entité} | {Description métier} | {Prop1, Prop2, Prop3} | {État1 → État2 → État3} |
```

**Règles** :
- Lister toutes les entités métier principales manipulées
- Décrire leur signification métier (pas technique)
- Identifier leurs propriétés clés (pas exhaustif, juste les principales)
- Documenter leur cycle de vie (états et transitions)

**Exemple** :
```markdown
| Entité | Description | Propriétés Clés | Cycle de Vie |
|--------|-------------|----------------|--------------|
| Ressource | Collaborateur assignable à un projet | Nom, Prénom, Compétences, Disponibilité | Disponible → Allouée → En Projet → Disponible |
| Allocation | Affectation d'une ressource à un projet | Ressource, Projet, Période, Taux | Demandée → Validée → Active → Terminée |
| Projet | Initiative nécessitant des ressources | Nom, Client, Période, Budget | Planifié → En Cours → Clôturé |
```

### 8. Features Associées
```markdown
## Features Associées
| ID | Feature | Priorité | Statut | Dépendances |
|----|---------|----------|--------|-------------|
| FEATURE-{ID} | {Titre court} | {CRITIQUE/HAUTE/MOYENNE/BASSE} | {DRAFT/IN_PROGRESS/DONE} | FEATURE-{ID} (si applicable) |
```

**Règles** :
- Lister toutes les FEATUREs composant cette EPIC
- Maintenir cette liste à jour (ajouter les nouvelles features)
- Prioriser les features (ordre d'implémentation)
- Documenter les dépendances entre features

### 9. Phases de Déploiement
```markdown
## Phases de Déploiement

### Phase 1 : MVP (Minimum Viable Product)
- **Objectif** : {Objectif business de cette phase : ex. "Valider le concept avec 10 utilisateurs pilotes"}
- **Features incluses** : FEATURE-{ID}, FEATURE-{ID}, FEATURE-{ID}
- **Date cible** : {YYYY-MM-DD ou YYYY-QX}
- **Critères de succès** : 
  - {Critère mesurable 1}
  - {Critère mesurable 2}

### Phase 2 : Évolutions
- **Objectif** : {Objectif de cette phase : ex. "Déploiement à l'ensemble de l'organisation"}
- **Features incluses** : FEATURE-{ID}, FEATURE-{ID}
- **Date cible** : {Date}
- **Critères de succès** : {Critères}

### Phase N : ...
```

**Règles** :
- Définir des phases réalistes et incrémentales
- Chaque phase doit apporter une valeur utilisable
- Établir des critères de succès mesurables pour chaque phase
- Planifier de manière réaliste (ne pas sur-promettre)

### 10. Risques et Mitigations
```markdown
## Risques et Mitigations
| Risque | Probabilité | Impact | Mitigation |
|--------|-------------|--------|------------|
| {Description précise du risque} | {Haute/Moyenne/Basse} | {Haut/Moyen/Bas} | {Actions concrètes pour réduire/éviter le risque} |
```

**Règles** :
- Identifier tous les risques métier, organisationnels, techniques
- Évaluer probabilité ET impact de manière réaliste
- Proposer des actions de mitigation concrètes et actionnables
- Mettre à jour cette section au fil du temps

**Exemple** :
```markdown
| Risque | Probabilité | Impact | Mitigation |
|--------|-------------|--------|------------|
| Résistance au changement des managers | Haute | Haut | Plan de conduite du changement, formations, ambassadeurs |
| Données historiques incomplètes | Moyenne | Moyen | Phase de nettoyage de données en amont, validation manuelle |
| Intégration avec SIRH complexe | Moyenne | Haut | POC technique en amont, plan B avec saisie manuelle |
```

### 11. Hypothèses
```markdown
## Hypothèses
- {Hypothèse 1 sur laquelle repose cette EPIC : ex. "Les managers accepteront de saisir les compétences de leurs équipes"}
- {Hypothèse 2 : ex. "Les données du SIRH sont fiables et à jour"}
- {Hypothèse 3 : ex. "Les collaborateurs consulteront leurs affectations au moins une fois par mois"}
```

**Règles** :
- Lister toutes les hypothèses critiques
- Valider ces hypothèses le plus tôt possible (interviews, POC, tests)
- Mettre à jour ou invalider les hypothèses au fil du projet

### 12. Historique des Modifications
```markdown
## Historique des Modifications
| Date | Version | Auteur | Modifications |
|------|---------|--------|---------------|
| 2024-11-20 | 1.0.0 | {Auteur} | Création initiale de l'EPIC |
| 2024-11-25 | 1.1.0 | {Auteur} | Ajout de la Feature FEATURE-0005 suite à atelier utilisateurs |
| 2024-12-01 | 1.2.0 | {Auteur} | Révision du périmètre : exclusion de la fonctionnalité X (reportée en Phase 2) |
```

**Règles** :
- Documenter TOUTES les modifications avec date et justification
- Appliquer le versioning sémantique (SemVer) :
  - **Major (X.0.0)** : Changement majeur de périmètre ou de vision
  - **Minor (x.Y.0)** : Ajout/retrait de features, modification de priorités
  - **Patch (x.y.Z)** : Corrections mineures, clarifications, typos
- Ne JAMAIS supprimer l'historique

### 13. Références
```markdown
## Références
- **ADR liés** : 
  - ADR-{ID} : {Titre de la décision architecturale impactant cette EPIC}
- **Documentation externe** : 
  - {Lien vers documentation métier}
  - {Lien vers étude de marché}
  - {Lien vers benchmarks}
- **Personas** :
  - {persona-nom}.personna.md
```

**Règles** :
- Référencer tous les documents connexes
- Maintenir les liens à jour
- Faciliter la navigation entre documents

## Checklist de Validation

Avant de considérer une EPIC comme complète, vérifier :

### Complétude
- [ ] Vision claire alignée avec les objectifs business
- [ ] Tous les personas identifiés et documentés
- [ ] Périmètre (In/Out scope) explicite sans ambiguïté
- [ ] KPIs et métriques de succès définis et mesurables
- [ ] Au moins 3 features associées identifiées
- [ ] Toutes les dépendances documentées
- [ ] Risques identifiés avec mitigations
- [ ] Phases de déploiement planifiées

### Qualité
- [ ] Français correct, ton professionnel
- [ ] Agnostique de la technologie (focus métier)
- [ ] Pas de contradiction avec d'autres EPICs
- [ ] Liens entre documents fonctionnels
- [ ] Historique des modifications à jour

### Pragmatisme
- [ ] Objectifs réalistes et atteignables
- [ ] Périmètre raisonnable (pas trop large)
- [ ] Planning réaliste
- [ ] Risques identifiés de manière honnête

## Bonnes Pratiques

### ✅ À Faire
- Impliquer les parties prenantes dans la rédaction
- Valider la vision avec le sponsor business
- Réviser régulièrement (au moins trimestriellement)
- Aligner les KPIs avec les OKRs de l'entreprise
- Documenter les décisions et leurs justifications

### ❌ À Éviter
- Rédiger une EPIC trop large (> 6 mois de développement)
- Ignorer les dépendances avec d'autres EPICs
- Définir des KPIs non mesurables ("améliorer la satisfaction")
- Mélanger fonctionnel et technique
- Copier-coller sans adapter au contexte

## Maintenance

### Révision Continue
- **Fréquence** : Trimestrielle ou à chaque changement majeur
- **Déclencheurs** : Feedback utilisateurs, changement stratégique, nouvelle contrainte
- **Actions** : Mettre à jour le statut, réviser le périmètre, ajuster les KPIs

### Dépréciation
Si l'EPIC devient obsolète :
1. Mettre le statut à `[DEPRECATED]`
2. Ajouter en haut du document :
   ```markdown
   > ⚠️ **DEPRECATED** : Cette EPIC est obsolète.  
   > Remplacée par : EPIC-{ID}  
   > Date de dépréciation : YYYY-MM-DD  
   > Raison : {Explication détaillée}
   ```
3. Ne JAMAIS supprimer le fichier (traçabilité)
4. Mettre à jour l'index README.md

## Exemple Complet

Voir les EPICs existantes dans `documentations/functionnals/` pour des exemples concrets adaptés au projet.