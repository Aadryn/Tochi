---
applyTo: "documentations/functionnals/**/*.personna.md,documentations/functionnals/*.personna.md"
---

# Instructions pour la Rédaction des Personas

## Objectif
Un Persona représente un archétype d'utilisateur du système, défini par ses caractéristiques, besoins, objectifs et comportements. Les personas servent de référence pour la rédaction des EPICs, FEATUREs et User Stories.

## Principes Directeurs
- **Toujours écrire en français** avec un ton pragmatique, descriptif, précis et professionnel
- **Basé sur des utilisateurs réels** : s'appuyer sur des interviews, observations, données
- **Humaniser** : donner un nom, une photo (fictive), une personnalité
- **Actionnable** : doit aider à prendre des décisions de conception et de priorisation
- **Évolutif** : mettre à jour selon les retours terrain

## Nomenclature Obligatoire
- **Format de fichier** : `{prenom}-{nom}.personna.md` ou `{role-metier}.personna.md`
- **Exemples** :
  - `marie-dupont.personna.md`
  - `responsable-rh.personna.md`
  - `chef-de-projet.personna.md`

## Structure Obligatoire du Document

### 1. En-tête et Identité
```markdown
# Persona : {Prénom Nom} - {Rôle Métier}

## Identité
- **Nom** : {Prénom Nom (fictif mais réaliste)}
- **Âge** : {Tranche d'âge : ex. 35-45 ans}
- **Rôle** : {Intitulé du poste exact}
- **Ancienneté dans le rôle** : {X années}
- **Localisation** : {Ville/Pays ou Remote}
- **Photo** : {Description ou lien vers image fictive}
```

**Règles** :
- Utiliser un nom fictif mais réaliste (éviter les stéréotypes)
- L'âge et l'ancienneté impactent le niveau d'expertise et les attentes
- La localisation peut influencer les besoins (fuseau horaire, langue)

**Exemple** :
```markdown
# Persona : Marie Dupont - Responsable RH

## Identité
- **Nom** : Marie Dupont
- **Âge** : 38 ans
- **Rôle** : Responsable Ressources Humaines
- **Ancienneté dans le rôle** : 5 ans
- **Localisation** : Paris, France
- **Photo** : Femme professionnelle, souriante, en tenue business casual
```

### 2. Citation Représentative
```markdown
## Citation
> "{Citation qui résume la vision, frustration ou objectif principal du persona}"

*Cette citation capture l'essence des besoins et motivations du persona*
```

**Règles** :
- Courte (1-2 phrases maximum)
- Authentique (issue d'interviews réelles si possible)
- Représentative des frustrations ou objectifs

**Exemple** :
```markdown
## Citation
> "Je passe trop de temps à chercher les bonnes ressources pour mes projets. J'ai besoin d'un système qui me permette de voir rapidement qui est disponible et avec quelles compétences."

*Marie exprime son besoin d'efficacité et de visibilité sur les ressources.*
```

### 3. Contexte Professionnel
```markdown
## Contexte Professionnel

### Responsabilités Principales
- {Responsabilité 1 : description précise}
- {Responsabilité 2}
- {Responsabilité 3}

### Objectifs de Performance (KPIs)
- {KPI 1 : ex. "Réduire le temps de staffing de X%"}
- {KPI 2 : ex. "Atteindre un taux d'allocation de Y%"}
- {KPI 3}

### Journée Type
**Matin (8h-12h)** :
- {Activité 1}
- {Activité 2}

**Après-midi (14h-18h)** :
- {Activité 3}
- {Activité 4}

### Interactions Clés
| Avec Qui | Fréquence | Objectif de l'Interaction |
|----------|-----------|--------------------------|
| {Rôle/Personne} | {Quotidien/Hebdo/Mensuel} | {Objectif} |
```

**Règles** :
- Lister 3 à 5 responsabilités principales
- Identifier les KPIs mesurables du persona
- Décrire une journée type réaliste
- Cartographier les interactions avec d'autres personas/rôles

**Exemple** :
```markdown
## Contexte Professionnel

### Responsabilités Principales
- Gérer un portefeuille de 150 collaborateurs (recrutement, formation, développement)
- Assurer l'allocation optimale des ressources sur les projets
- Piloter les entretiens annuels et la gestion des carrières
- Garantir la conformité RH (RGPD, droit du travail)

### Objectifs de Performance (KPIs)
- Réduire le temps de staffing de 5 jours à 2 jours (objectif : -60%)
- Atteindre un taux d'allocation de 85% (actuellement 65%)
- Satisfaction collaborateurs > 80% (NPS)

### Journée Type
**Matin (8h-12h)** :
- Lecture et traitement des emails (demandes d'affectation, questions RH)
- Réunion hebdomadaire avec les Chefs de Projet (besoins en ressources)
- Recherche de ressources disponibles avec les compétences requises
- Mise à jour des affectations dans le système

**Après-midi (14h-18h)** :
- Entretiens individuels avec collaborateurs (2-3 par semaine)
- Validation des demandes de formation
- Reporting mensuel à la Direction
- Veille légale et réglementaire

### Interactions Clés
| Avec Qui | Fréquence | Objectif de l'Interaction |
|----------|-----------|--------------------------|
| Chefs de Projet | Quotidien | Identifier besoins en ressources, valider affectations |
| Collaborateurs | Hebdomadaire | Suivre bien-être, carrière, compétences |
| Direction | Mensuel | Reporting RH, indicateurs, stratégie |
| Service Juridique | Ponctuel | Validation conformité légale |
```

### 4. Profil Technologique
```markdown
## Profil Technologique

### Niveau de Compétence Numérique
- **Niveau global** : {Débutant / Intermédiaire / Avancé / Expert}
- **Outils bureautiques** : {Excel, Word, PowerPoint} - {Niveau}
- **Outils métier** : {Liste des outils utilisés} - {Niveau pour chacun}
- **Outils collaboratifs** : {Teams, Slack, etc.} - {Niveau}

### Équipement
- **Ordinateur** : {PC / Mac} - {Portable / Fixe}
- **Mobile** : {Smartphone (iOS/Android)} - {Fréquence d'usage professionnel}
- **Connexion** : {Bureau / Remote / Hybride}

### Habitudes Numériques
- {Habitude 1 : ex. "Consulte ses emails dès 7h30"}
- {Habitude 2 : ex. "Préfère les outils web aux applications desktop"}
- {Habitude 3 : ex. "Utilise rarement le mobile pour des tâches complexes"}

### Frustrations Technologiques
- {Frustration 1 : ex. "Perd du temps à naviguer entre 5 outils différents"}
- {Frustration 2 : ex. "Systèmes lents qui ralentissent son travail"}
- {Frustration 3}
```

**Règles** :
- Évaluer honnêtement le niveau de compétence (impact sur la complexité d'utilisation)
- Lister les outils actuellement utilisés
- Identifier les frustrations technologiques (besoins d'amélioration)

**Exemple** :
```markdown
## Profil Technologique

### Niveau de Compétence Numérique
- **Niveau global** : Intermédiaire
- **Outils bureautiques** : Excel (bon), Word (bon), PowerPoint (moyen)
- **Outils métier** : SIRH (expert), ATS de recrutement (bon)
- **Outils collaboratifs** : Teams (bon), SharePoint (moyen)

### Équipement
- **Ordinateur** : PC Portable Windows 11
- **Mobile** : iPhone 14 - Utilise principalement pour emails et appels
- **Connexion** : Bureau 3 jours/semaine, Remote 2 jours/semaine

### Habitudes Numériques
- Consulte ses emails dès 7h30 avant d'arriver au bureau
- Préfère les applications web aux logiciels installés
- Utilise Excel pour ses tableaux de bord et reporting
- Travaille sur un seul écran, jongle entre les fenêtres

### Frustrations Technologiques
- Perd du temps à naviguer entre le SIRH, les fichiers Excel et les emails
- Le SIRH actuel est lent et nécessite trop de clics pour une action simple
- Manque de visibilité consolidée sur les disponibilités des ressources
- Pas d'accès mobile aux données RH (doit attendre d'être au bureau)
```

### 5. Besoins et Objectifs
```markdown
## Besoins et Objectifs

### Besoins Fonctionnels
| Besoin | Priorité | Fréquence | Contexte |
|--------|----------|-----------|----------|
| {Besoin 1} | {Critique/Haute/Moyenne/Basse} | {Quotidien/Hebdo/Mensuel} | {Dans quel contexte ce besoin apparaît} |

### Objectifs à Court Terme (0-6 mois)
- {Objectif 1 : mesurable et daté}
- {Objectif 2}

### Objectifs à Moyen Terme (6-12 mois)
- {Objectif 1}
- {Objectif 2}

### Objectifs à Long Terme (1-3 ans)
- {Objectif 1}
- {Objectif 2}
```

**Règles** :
- Prioriser les besoins (Critique = bloquant, Haute = important, Moyenne = souhaitable, Basse = nice-to-have)
- Distinguer court/moyen/long terme
- Rendre les objectifs mesurables (SMART)

**Exemple** :
```markdown
## Besoins et Objectifs

### Besoins Fonctionnels
| Besoin | Priorité | Fréquence | Contexte |
|--------|----------|-----------|----------|
| Voir rapidement les ressources disponibles | Critique | Quotidien | Lors de demandes d'affectation par les Chefs de Projet |
| Filtrer les ressources par compétences | Critique | Quotidien | Pour trouver le bon profil pour un projet |
| Créer une nouvelle ressource rapidement | Haute | Hebdomadaire | Lors de l'arrivée d'un nouveau collaborateur |
| Consulter l'historique des affectations | Moyenne | Hebdomadaire | Pour les entretiens annuels |
| Exporter les données RH en Excel | Moyenne | Mensuel | Pour le reporting à la Direction |
| Recevoir une notification quand une ressource se libère | Basse | Quotidien | Pour anticiper les affectations |

### Objectifs à Court Terme (0-6 mois)
- Réduire le temps moyen de staffing de 5 jours à 3 jours (objectif : -40%)
- Atteindre un taux d'allocation de 75% (actuellement 65%)
- Former 100% de l'équipe RH au nouveau système

### Objectifs à Moyen Terme (6-12 mois)
- Réduire le temps de staffing à 2 jours (objectif : -60%)
- Atteindre un taux d'allocation de 85%
- Automatiser le reporting mensuel (gain de 2 jours/mois)

### Objectifs à Long Terme (1-3 ans)
- Devenir une référence en gestion des ressources dans l'entreprise
- Implémenter un système prédictif d'allocation basé sur l'IA
- Améliorer le NPS collaborateurs à 85%
```

### 6. Frustrations et Douleurs (Pain Points)
```markdown
## Frustrations et Douleurs (Pain Points)

### Frustrations Majeures
1. **{Titre de la frustration}**
   - **Description** : {Description détaillée du problème}
   - **Impact** : {Conséquence sur le travail : temps perdu, erreurs, stress}
   - **Fréquence** : {Quotidien / Hebdomadaire / Mensuel}
   - **Niveau de criticité** : {Bloquant / Majeur / Mineur}

2. **{Frustration 2}**
   - ...

### Scénario de Frustration (Story)
> {Raconter un scénario concret illustrant une frustration majeure}

**Exemple** :
> "Hier, un Chef de Projet m'a demandé une ressource Java Senior pour démarrer un projet urgent lundi. J'ai passé 3 heures à éplucher des fichiers Excel, envoyer des emails, appeler des collaborateurs... pour finalement découvrir qu'une ressource parfaite était disponible mais que je ne l'ai trouvée qu'après avoir proposé quelqu'un de moins expérimenté. Le projet a pris du retard."
```

**Règles** :
- Identifier 3 à 5 frustrations majeures
- Quantifier l'impact (temps, coût, erreurs)
- Raconter un scénario réel pour humaniser
- Prioriser par criticité

**Exemple** :
```markdown
## Frustrations et Douleurs (Pain Points)

### Frustrations Majeures

1. **Manque de visibilité sur les disponibilités en temps réel**
   - **Description** : Les informations de disponibilité sont éparpillées dans des fichiers Excel, des emails, des conversations Teams. Impossible d'avoir une vue consolidée.
   - **Impact** : Perte de temps quotidienne (2-3 heures), affectations sous-optimales, frustration des Chefs de Projet qui attendent une réponse rapide.
   - **Fréquence** : Quotidien
   - **Niveau de criticité** : Bloquant

2. **Processus de création de ressource trop long**
   - **Description** : Pour enregistrer un nouveau collaborateur, il faut saisir ses informations dans 3 outils différents (SIRH, outil d'affectation Excel, liste de diffusion email).
   - **Impact** : 20 minutes par ressource, risque d'incohérence entre les systèmes, ressources non disponibles immédiatement pour affectation.
   - **Fréquence** : Hebdomadaire (3-5 nouvelles ressources)
   - **Niveau de criticité** : Majeur

3. **Absence de filtrage par compétences**
   - **Description** : Les compétences sont mentionnées dans des champs libres non structurés. Impossible de rechercher "Java Senior" et obtenir une liste fiable.
   - **Impact** : Recherche manuelle fastidieuse, risque de passer à côté de ressources qualifiées, propositions parfois inadéquates.
   - **Fréquence** : Quotidien
   - **Niveau de criticité** : Bloquant

4. **Reporting manuel chronophage**
   - **Description** : Chaque mois, compilation manuelle de données depuis plusieurs sources pour produire le reporting Direction (taux d'allocation, compétences, turnover).
   - **Impact** : 2 jours complets par mois, risque d'erreurs, données obsolètes au moment de la présentation.
   - **Fréquence** : Mensuel
   - **Niveau de criticité** : Mineur

### Scénario de Frustration (Story)
> "La semaine dernière, un Chef de Projet m'a appelée à 16h pour me demander un développeur Full Stack JavaScript disponible dès lundi pour un projet stratégique client. J'ai ouvert mes 3 fichiers Excel 'Ressources', 'Compétences', 'Allocations', envoyé 7 emails pour confirmer les disponibilités, passé 4 coups de fil... Après 3 heures de recherche, j'ai finalement trouvé le profil parfait : Paul, qui venait justement de terminer sa mission vendredi. Mais entre-temps, j'avais déjà proposé quelqu'un de moins expérimenté au Chef de Projet, qui avait accepté par urgence. Résultat : Paul est resté sur le bench, le projet a pris du retard, et le Chef de Projet était frustré. Avec une vue centralisée et des filtres par compétences, j'aurais trouvé Paul en 30 secondes."
```

### 7. Motivations et Gains Attendus
```markdown
## Motivations et Gains Attendus

### Motivations Principales
- {Motivation 1 : ce qui pousse le persona à adopter une nouvelle solution}
- {Motivation 2}
- {Motivation 3}

### Gains Attendus (Value Proposition)
| Gain | Mesure de Succès | Impact Attendu |
|------|-----------------|----------------|
| {Gain 1} | {Comment mesurer} | {Bénéfice concret} |

### Critères de Satisfaction
- {Critère 1 : qu'est-ce qui rendrait le persona satisfait}
- {Critère 2}
- {Critère 3}
```

**Règles** :
- Identifier 3 à 5 motivations principales
- Définir des gains mesurables
- Établir des critères de satisfaction clairs

**Exemple** :
```markdown
## Motivations et Gains Attendus

### Motivations Principales
- Gagner du temps au quotidien pour se concentrer sur des tâches à plus forte valeur ajoutée (entretiens, stratégie RH)
- Améliorer la qualité des affectations (meilleur matching compétences-besoins)
- Réduire la frustration des Chefs de Projet qui attendent des réponses rapides
- Valoriser son rôle en apportant un service RH de qualité
- Faciliter le reporting et la prise de décision data-driven

### Gains Attendus (Value Proposition)
| Gain | Mesure de Succès | Impact Attendu |
|------|-----------------|----------------|
| Réduction temps de staffing | De 5 jours à 2 jours (−60%) | 15 heures/semaine libérées pour autres tâches |
| Amélioration taux d'allocation | De 65% à 85% (+20 points) | Réduction coûts bench, augmentation rentabilité |
| Satisfaction Chefs de Projet | NPS > 80% | Amélioration collaboration RH-Projets |
| Réduction temps reporting | De 2 jours à 2 heures (−90%) | Données en temps réel, décisions plus rapides |

### Critères de Satisfaction
- Trouver une ressource en moins de 5 minutes (vs 2-3 heures actuellement)
- Interface simple et intuitive (pas de formation longue nécessaire)
- Données toujours à jour et fiables
- Accès depuis n'importe où (bureau, remote, mobile)
- Reporting automatique en 1 clic
```

### 8. Scénarios d'Usage
```markdown
## Scénarios d'Usage

### Scénario 1 : {Titre du scénario}
**Contexte** : {Situation déclenchante}  
**Fréquence** : {Quotidien/Hebdomadaire/Mensuel}  
**Étapes** :
1. {Action 1}
2. {Action 2}
3. {Action 3}

**Résultat attendu** : {Ce que le persona obtient}  
**Gain par rapport à la situation actuelle** : {Amélioration mesurable}

### Scénario 2 : {Autre scénario}
...
```

**Règles** :
- Décrire 3 à 5 scénarios d'usage courants
- Être concret et réaliste
- Quantifier les gains

**Exemple** :
```markdown
## Scénarios d'Usage

### Scénario 1 : Recherche rapide d'une ressource disponible
**Contexte** : Un Chef de Projet demande un développeur Java Senior disponible pour un projet urgent  
**Fréquence** : Quotidien (3-4 fois/jour)

**Étapes** :
1. Accède à l'application Gestion des Ressources
2. Filtre par compétence "Java Senior" et disponibilité "Disponible maintenant"
3. Consulte la liste des 5 ressources matchant les critères
4. Sélectionne la ressource la plus adaptée (localisation, expérience projet similaire)
5. Crée l'allocation directement depuis l'application
6. Le Chef de Projet reçoit une notification avec les coordonnées de la ressource

**Résultat attendu** : Ressource identifiée et allouée en moins de 5 minutes  
**Gain par rapport à la situation actuelle** : 2h45 économisées par recherche, soit ~12 heures/semaine

### Scénario 2 : Création d'une nouvelle ressource lors d'une arrivée
**Contexte** : Un nouveau collaborateur rejoint l'entreprise lundi, doit être enregistré dans le système  
**Fréquence** : Hebdomadaire (3-5 nouvelles ressources)

**Étapes** :
1. Clique sur "Nouvelle Ressource"
2. Saisit Nom, Prénom, Email, Compétences principales
3. Valide en 1 clic
4. La ressource est immédiatement disponible pour allocation
5. Une notification est envoyée au manager et au collaborateur

**Résultat attendu** : Ressource créée et opérationnelle en 2 minutes  
**Gain par rapport à la situation actuelle** : 18 minutes économisées (de 20 min à 2 min), une seule saisie au lieu de 3

### Scénario 3 : Génération du reporting mensuel
**Contexte** : Fin de mois, préparation du reporting Direction  
**Fréquence** : Mensuel

**Étapes** :
1. Accède à la section "Reporting"
2. Sélectionne le mois concerné
3. Clique sur "Générer le rapport"
4. Le système produit automatiquement le rapport avec taux d'allocation, compétences, projets, turnover
5. Exporte en Excel ou PDF
6. Partage avec la Direction

**Résultat attendu** : Rapport généré en 2 heures (incluant analyse) au lieu de 2 jours  
**Gain par rapport à la situation actuelle** : 14 heures économisées par mois
```

### 9. Objections et Freins Potentiels
```markdown
## Objections et Freins Potentiels

### Objections
| Objection | Réponse / Mitigation |
|-----------|---------------------|
| {Objection 1} | {Comment lever cette objection} |

### Freins à l'Adoption
- {Frein 1 : résistance au changement, courbe d'apprentissage}
- {Frein 2}

### Conditions de Réussite
- {Condition 1 nécessaire pour que le persona adopte la solution}
- {Condition 2}
- {Condition 3}
```

**Règles** :
- Anticiper les objections et résistances
- Proposer des mitigations concrètes
- Définir les conditions de succès

**Exemple** :
```markdown
## Objections et Freins Potentiels

### Objections
| Objection | Réponse / Mitigation |
|-----------|---------------------|
| "Je n'ai pas le temps d'apprendre un nouvel outil" | Interface intuitive, formation 1h suffit, gains de temps immédiats (12h/semaine) |
| "Mes fichiers Excel fonctionnent bien" | Excel limité pour la collaboration, risque d'erreurs, pas de visibilité temps réel |
| "Et si le système tombe en panne ?" | SLA 99,9%, backup quotidiens, mode dégradé avec accès lecture seule |
| "Les données seront-elles à jour ?" | Synchronisation automatique avec le SIRH, mise à jour temps réel par les utilisateurs |

### Freins à l'Adoption
- Habitudes bien ancrées (10 ans d'utilisation d'Excel)
- Crainte de perdre le contrôle (Excel = "ma base de données")
- Courbe d'apprentissage perçue comme longue
- Besoin d'accompagnement au changement de l'équipe RH

### Conditions de Réussite
- Formation courte et efficace (1h suffit)
- Interface simple, pas de jargon technique
- Migration des données Excel existantes sans perte
- Support réactif en cas de question (chat, hotline)
- Quick wins immédiats (temps de recherche divisé par 30)
- Sponsorship de la Direction RH
```

### 10. Références
```markdown
## Références
- **EPICs associées** : EPIC-{ID}, EPIC-{ID}
- **FEATUREs associées** : FEATURE-{ID}, FEATURE-{ID}
- **User Stories associées** : US-{ID}, US-{ID}
- **Sources** : 
  - {Interview avec {Nom} le {Date}}
  - {Observation terrain le {Date}}
  - {Étude marché / Benchmark}
```

**Règles** :
- Lier aux EPICs/FEATUREs/US concernant ce persona
- Citer les sources (interviews, observations)
- Tracer la méthodologie de création du persona

## Checklist de Validation

Avant de considérer un Persona comme complet :

### Complétude
- [ ] Identité complète (nom, âge, rôle, ancienneté, localisation)
- [ ] Citation représentative
- [ ] Contexte professionnel détaillé (responsabilités, KPIs, journée type, interactions)
- [ ] Profil technologique (niveau, équipement, habitudes, frustrations)
- [ ] Besoins fonctionnels priorisés
- [ ] Objectifs court/moyen/long terme
- [ ] 3-5 frustrations majeures avec impact quantifié
- [ ] Scénario de frustration (story)
- [ ] Motivations et gains attendus
- [ ] 3-5 scénarios d'usage concrets
- [ ] Objections et freins anticipés avec réponses

### Qualité
- [ ] Basé sur des données réelles (interviews, observations)
- [ ] Humanisé (nom, personnalité, citation)
- [ ] Réaliste et crédible
- [ ] Actionnable (aide à prendre des décisions)
- [ ] Français correct, ton professionnel

### Pragmatisme
- [ ] Gains mesurables et quantifiés
- [ ] Scénarios d'usage concrets et fréquents
- [ ] Objections anticipées avec mitigations

## Bonnes Pratiques

### ✅ À Faire
- Interviewer plusieurs personnes dans le même rôle (trouver les patterns communs)
- Observer les utilisateurs en situation réelle
- Quantifier autant que possible (temps, coûts, erreurs)
- Mettre à jour régulièrement selon les retours terrain
- Partager les personas avec toute l'équipe (dev, PO, métier)
- Utiliser les personas dans les revues de spécifications

### ❌ À Éviter
- Créer un persona "moyen" qui n'existe pas réellement
- Faire des suppositions sans validation
- Multiplier les personas (5-7 maximum pour un projet)
- Oublier de mettre à jour les personas
- Créer des personas trop techniques ou trop flous

## Maintenance

### Révision
- **Fréquence** : Semestrielle ou après feedback utilisateurs significatifs
- **Déclencheurs** : Changement organisationnel, nouveaux besoins identifiés, feedback terrain
- **Actions** : Mettre à jour les frustrations, besoins, objectifs

## Exemple Complet

Voir les Personas existants dans `documentations/functionnals/` pour des exemples concrets.