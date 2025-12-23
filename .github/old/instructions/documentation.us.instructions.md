---
applyTo: "documentations/functionnals/**/*.us.md,documentations/functionnals/*.us.md"
---

# Instructions pour la Rédaction des User Stories (US)

## Objectif
Une User Story (US) représente une fonctionnalité atomique et testable apportant une valeur concrète à un utilisateur. Elle constitue l'unité de travail la plus fine dans la décomposition fonctionnelle.

## Principes Directeurs
- **Toujours écrire en français** avec un ton pragmatique, descriptif, argumentatif, précis, logique et professionnel
- **Rester agnostique de la technologie** : décrire le besoin utilisateur, pas l'implémentation
- **Atomicité** : Une US = Une fonctionnalité = Un objectif testable
- **Testabilité** : Critères d'acceptation SMART (Spécifiques, Mesurables, Actionnables, Réalistes, Testables)
- **Valeur utilisateur** : Chaque US doit apporter une valeur mesurable

## Nomenclature Obligatoire
- **Format de fichier** : `US-{ID:4 digits}-{slug-en-kebab-case}.us.md`
- **Exemples** :
  - `US-0001-connexion-utilisateur.us.md`
  - `US-0002-creer-ressource-simple.us.md`
  - `US-0003-valider-unicite-email.us.md`

## Structure Obligatoire du Document

### 1. Métadonnées
```markdown
# US-{ID} : {Titre Clair et Concis de l'Action Utilisateur}

## Métadonnées
- **ID** : US-{ID}
- **Feature Parent** : [FEATURE-{ID} : {Titre}](../FEATURE-{ID}-{slug}.feature.md)
- **EPIC Parent** : [EPIC-{ID} : {Titre}](../../EPIC-{ID}-{slug}.epic.md)
- **Statut** : [DRAFT | READY | IN_PROGRESS | IN_REVIEW | DONE | DEPRECATED]
- **Priorité** : [CRITIQUE | HAUTE | MOYENNE | BASSE]
- **Estimation** : {X story points}
- **Date de création** : YYYY-MM-DD
- **Dernière mise à jour** : YYYY-MM-DD
- **Auteur** : {Nom ou rôle}
- **Version** : X.Y.Z
```

**Règles** :
- L'ID doit être unique, séquentiel et ne JAMAIS être réutilisé
- Toujours lier à la FEATURE et EPIC parents
- Le statut reflète l'avancement précis (READY = prête pour développement, IN_REVIEW = en recette)
- L'estimation en story points facilite la planification Sprint

### 2. User Story
```markdown
## User Story
**En tant que** {persona spécifique avec rôle précis},  
**Je veux** {action précise et atomique},  
**Afin de** {valeur/bénéfice immédiat et mesurable}.

### Contexte d'Usage
{Description détaillée du contexte dans lequel l'utilisateur a besoin de cette fonctionnalité : situation déclenchante, fréquence, environnement}

### Valeur Apportée
{Explication de la valeur business ou utilisateur apportée par cette story : gain de temps, réduction d'erreurs, amélioration de l'expérience}
```

**Règles** :
- Format User Story strict et obligatoire
- Le "Je veux" doit décrire UNE action atomique (pas un ensemble d'actions)
- Le "Afin de" doit exprimer une valeur mesurable ou observable
- Rester au niveau fonctionnel (pas technique)

**Exemples** :

✅ **Bon** :
```markdown
## User Story
**En tant que** Responsable RH,  
**Je veux** créer une nouvelle ressource en saisissant son nom, prénom et email,  
**Afin de** pouvoir l'enregistrer dans le système et la rendre disponible pour allocation.

### Contexte d'Usage
Lorsqu'un nouveau collaborateur rejoint l'entreprise, le Responsable RH doit l'enregistrer rapidement (< 2 minutes) dans le système avec les informations essentielles. Cette action est effectuée quotidiennement (3 à 5 fois par jour en moyenne).

### Valeur Apportée
Permet de centraliser les informations des ressources et de les rendre immédiatement disponibles pour allocation sur les projets, réduisant le délai de staffing de 2 jours à quelques heures.
```

❌ **Mauvais** :
```markdown
**En tant que** utilisateur,  
**Je veux** gérer les ressources,  
**Afin de** faire mon travail.
```
→ Trop vague, pas atomique, pas de valeur mesurable

### 3. Personas Concernés
```markdown
## Personas Concernés
| Persona | Rôle | Fréquence d'Usage | Niveau d'Expertise |
|---------|------|-------------------|-------------------|
| {Nom Persona} | {Rôle métier} | {Quotidien/Hebdomadaire/Mensuel/Ponctuel} | {Débutant/Intermédiaire/Expert} |
```

**Règles** :
- Référencer les personas définis dans `*.personna.md`
- Préciser la fréquence d'usage (impact sur l'UX)
- Indiquer le niveau d'expertise (impact sur la complexité d'utilisation)

**Exemple** :
```markdown
| Persona | Rôle | Fréquence d'Usage | Niveau d'Expertise |
|---------|------|-------------------|-------------------|
| Marie Dupont | Responsable RH | Quotidien (3-5 fois/jour) | Intermédiaire |
```

### 4. Workflow Détaillé

#### 4.1 Pré-conditions
```markdown
## Workflow Détaillé

### Pré-conditions
- {Condition 1 devant être remplie avant d'exécuter la story : ex. "Utilisateur authentifié"}
- {Condition 2 : ex. "Rôle = Responsable RH"}
- {État système nécessaire : ex. "Aucune ressource avec l'email test@example.com n'existe"}
- {Données pré-existantes requises : ex. "Référentiel de compétences chargé"}
```

**Règles** :
- Lister TOUTES les conditions préalables
- Être précis (pas de suppositions implicites)
- Inclure les conditions techniques si elles impactent le fonctionnel

#### 4.2 Scénario Principal (Happy Path)
```markdown
### Scénario Principal (Happy Path)
\`\`\`gherkin
Given je suis authentifié en tant que "Responsable RH"
  And je suis sur la page "Liste des Ressources"
  And aucune ressource avec l'email "john.doe@example.com" n'existe
When je clique sur le bouton "Nouvelle Ressource"
  And je saisis "John" dans le champ "Prénom"
  And je saisis "Doe" dans le champ "Nom"
  And je saisis "john.doe@example.com" dans le champ "Email"
  And je clique sur le bouton "Enregistrer"
Then le système crée la ressource avec un ID unique
  And le système affiche le message "Ressource créée avec succès" pendant 3 secondes
  And je suis redirigé vers la page "Liste des Ressources"
  And la ressource "John Doe" apparaît dans la liste
\`\`\`

**Description Narrative** :
1. **Étape 1 : Accès au formulaire de création**
   - Écran/Page : Liste des Ressources
   - Élément UI : Bouton "Nouvelle Ressource" (en haut à droite)
   - Action utilisateur : Clic sur le bouton
   - Résultat : Affichage du formulaire de création vierge avec champs par défaut

2. **Étape 2 : Saisie des informations**
   - Écran/Page : Formulaire Nouvelle Ressource
   - Champs affichés : Prénom*, Nom*, Email*, Téléphone, Statut
   - Données saisies : 
     - Prénom : "John" (valide)
     - Nom : "Doe" (valide)
     - Email : "john.doe@example.com" (valide)
   - Traitement : Validation en temps réel à la sortie de chaque champ
   - Validation : Format email validé ✅, Unicité email vérifiée ✅

3. **Étape 3 : Enregistrement**
   - Action utilisateur : Clic sur "Enregistrer"
   - Traitement système : 
     - Validation finale de tous les champs
     - Génération d'un ID unique (GUID)
     - Insertion en base de données
     - Commit de la transaction
   - Mise à jour : Nouvelle ressource avec Id, DateCreation = maintenant, Statut = "Active"

4. **Résultat Final : Confirmation et redirection**
   - Message de succès : "Ressource créée avec succès" (toast vert, 3 secondes)
   - Redirection : Vers la page "Liste des Ressources"
   - Données visibles : La nouvelle ressource "John Doe" apparaît en première position (tri par date de création DESC)
```

**Règles** :
- Utiliser le format Gherkin (Given/When/Then) pour la traçabilité
- Fournir une description narrative détaillée avec numérotation des étapes
- Préciser les écrans, éléments UI, données saisies
- Décrire les traitements système (ce qui se passe en arrière-plan)
- Spécifier les résultats observables et vérifiables

#### 4.3 Post-conditions
```markdown
### Post-conditions
- {État système après exécution réussie : ex. "Ressource créée avec statut 'Active'"}
- {Données créées/modifiées : ex. "Enregistrement en table Ressources avec ID unique"}
- {Notifications envoyées : ex. "Email de bienvenue envoyé au nouveau collaborateur"}
- {Logs enregistrés : ex. "Log d'audit : création ressource par {User} à {DateTime}"}
```

**Règles** :
- Documenter tous les effets de bord
- Inclure les notifications, logs, événements déclenchés
- Vérifier la cohérence avec les règles métier

### 5. Scénarios Alternatifs
```markdown
## Scénarios Alternatifs

### Scénario Alternatif 1 : Annulation de la création
\`\`\`gherkin
Given je suis sur le formulaire "Nouvelle Ressource"
  And j'ai saisi partiellement les informations
When je clique sur le bouton "Annuler"
Then le système affiche une boîte de dialogue de confirmation
  And le message affiché est "Voulez-vous vraiment annuler ? Les données non enregistrées seront perdues."
When je clique sur "Confirmer"
Then je suis redirigé vers la page "Liste des Ressources"
  And aucune ressource n'a été créée
\`\`\`

**Description** : Permet à l'utilisateur d'annuler la création en cours sans perdre la navigation. Une confirmation est demandée pour éviter les annulations accidentelles.

### Scénario Alternatif 2 : Sauvegarde avec champs optionnels vides
\`\`\`gherkin
Given je suis sur le formulaire "Nouvelle Ressource"
When je saisis uniquement les champs obligatoires (Nom, Prénom, Email)
  And je laisse le champ "Téléphone" vide
  And je clique sur "Enregistrer"
Then la ressource est créée avec succès
  And le champ "Téléphone" est NULL en base de données
  And le message "Ressource créée avec succès" s'affiche
\`\`\`

**Description** : Les champs optionnels peuvent être laissés vides sans bloquer la création.
```

**Règles** :
- Documenter AU MINIMUM 2 scénarios alternatifs
- Couvrir les chemins alternatifs courants (annulation, validation partielle, etc.)
- Utiliser Gherkin + description narrative

### 6. Cas d'Erreur et Exceptions
```markdown
## Cas d'Erreur et Exceptions

### Erreur 1 : Email invalide
\`\`\`gherkin
Given je suis sur le formulaire "Nouvelle Ressource"
When je saisis "invalid-email" dans le champ "Email"
  And je clique sur "Enregistrer"
Then le système affiche l'erreur "Format d'email invalide (ex: nom@example.com)" sous le champ "Email"
  And le champ "Email" est mis en évidence en rouge
  And la ressource n'est PAS créée
  And je reste sur le formulaire
\`\`\`

**Gestion de l'Erreur** :
- **Code d'erreur** : RES-VAL-001
- **Message utilisateur** : "Format d'email invalide. Veuillez saisir une adresse email valide (ex: nom@example.com)."
- **Message technique (logs)** : `[RES-VAL-001] Invalid email format. Input: 'invalid-email'. Pattern expected: ^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$`
- **Action corrective suggérée** : "Corrigez le format de l'email et réessayez."
- **Récupération** : L'utilisateur corrige le champ Email et peut cliquer à nouveau sur "Enregistrer".

### Erreur 2 : Email déjà existant (doublon)
\`\`\`gherkin
Given une ressource avec l'email "john.doe@example.com" existe déjà
  And je suis sur le formulaire "Nouvelle Ressource"
When je saisis "john.doe@example.com" dans le champ "Email"
  And je clique sur "Enregistrer"
Then le système affiche l'erreur "Une ressource avec cet email existe déjà" sous le champ "Email"
  And un lien "Consulter la ressource existante" est affiché
  And la ressource n'est PAS créée
\`\`\`

**Gestion de l'Erreur** :
- **Code d'erreur** : RES-VAL-002
- **Message utilisateur** : "Une ressource avec cet email existe déjà. Utilisez un autre email ou consultez la ressource existante."
- **Message technique (logs)** : `[RES-VAL-002] Duplicate email detected. Email: 'john.doe@example.com'. Existing ResourceId: '{GUID}'`
- **Action corrective suggérée** : "Utilisez un autre email ou consultez la ressource existante via le lien."
- **Récupération** : L'utilisateur peut modifier l'email ou cliquer sur le lien pour consulter la ressource existante.

### Erreur 3 : Champ obligatoire vide
\`\`\`gherkin
Given je suis sur le formulaire "Nouvelle Ressource"
When je laisse le champ "Nom" vide
  And je clique sur "Enregistrer"
Then le système affiche l'erreur "Le champ Nom est obligatoire" sous le champ "Nom"
  And le champ "Nom" est mis en évidence en rouge
  And le bouton "Enregistrer" reste actif
  And la ressource n'est PAS créée
\`\`\`

**Gestion de l'Erreur** :
- **Code d'erreur** : RES-VAL-003
- **Message utilisateur** : "Le champ Nom est obligatoire."
- **Message technique (logs)** : `[RES-VAL-003] Required field missing. Field: 'Nom'`
- **Action corrective suggérée** : "Remplissez le champ Nom."
- **Récupération** : L'utilisateur saisit le champ Nom et peut réessayer.
```

**Règles** :
- Documenter AU MINIMUM 3 cas d'erreur (validation, doublon, champ vide)
- Utiliser Gherkin pour la traçabilité
- Spécifier un code d'erreur unique pour chaque type d'erreur
- Fournir des messages utilisateur clairs, actionnables, sans jargon
- Logger les erreurs avec contexte technique pour le debugging
- Proposer des actions correctives concrètes
- Décrire comment l'utilisateur peut récupérer

### 7. Cas Limites (Edge Cases)
```markdown
## Cas Limites (Edge Cases)

### Cas Limite 1 : Email avec caractères spéciaux valides
**Scénario** : Vérifier que les emails avec caractères spéciaux valides sont acceptés  
**Données de test** : `john.doe+test@example.com`, `jean-pierre@sub.example.com`  
**Comportement attendu** : Emails acceptés comme valides

### Cas Limite 2 : Nom très long
**Scénario** : Vérifier la gestion des noms de longueur maximale  
**Données de test** : Nom de 50 caractères (limite max)  
**Comportement attendu** : Nom accepté, pas de troncature

### Cas Limite 3 : Nom avec 51 caractères (dépassement)
**Scénario** : Vérifier le rejet des noms trop longs  
**Données de test** : Nom de 51 caractères  
**Comportement attendu** : Erreur "Le nom ne peut pas dépasser 50 caractères"

### Cas Limite 4 : Caractères accentués et spéciaux dans le nom
**Scénario** : Vérifier la gestion des caractères internationaux  
**Données de test** : "François", "José", "Müller", "O'Brien"  
**Comportement attendu** : Noms acceptés correctement

### Cas Limite 5 : Soumission multiple rapide (double-clic)
**Scénario** : Vérifier la protection contre les doublons accidentels  
**Données de test** : Double-clic rapide sur "Enregistrer"  
**Comportement attendu** : Une seule ressource créée, bouton désactivé après premier clic
```

**Règles** :
- Identifier AU MINIMUM 5 cas limites
- Couvrir : valeurs min/max, null/vide, caractères spéciaux, concurrence
- Spécifier les données de test précises
- Décrire le comportement attendu exact

### 8. Règles Métier Spécifiques
```markdown
## Règles Métier Spécifiques

### Règles de Validation
| ID | Champ | Règle | Condition | Message d'Erreur |
|----|-------|-------|-----------|------------------|
| RV-001 | Prénom | Obligatoire, max 50 caractères | Champ vide ou > 50 car. | "Le prénom est obligatoire et ne peut pas dépasser 50 caractères" |
| RV-002 | Nom | Obligatoire, max 50 caractères | Champ vide ou > 50 car. | "Le nom est obligatoire et ne peut pas dépasser 50 caractères" |
| RV-003 | Email | Obligatoire, format valide | Format invalide | "Format d'email invalide (ex: nom@example.com)" |
| RV-004 | Email | Unique | Email déjà existant | "Une ressource avec cet email existe déjà" |

**Exemples de Validation** :
- ✅ **Valides** :
  - Prénom : "Jean-Pierre" (10 caractères)
  - Nom : "O'Connor" (8 caractères)
  - Email : "jp.oconnor@example.com"
  
- ❌ **Invalides** :
  - Prénom : "" (vide) → "Le prénom est obligatoire"
  - Nom : "Nom de plus de cinquante caractères qui dépasse la limite" (55 car.) → "Le nom ne peut pas dépasser 50 caractères"
  - Email : "invalid-email" → "Format d'email invalide"

### Règles de Calcul
| ID | Formule | Description | Exemple |
|----|---------|-------------|---------|
| RC-001 | `DateCreation = NOW()` | Date de création = date/heure actuelle du système | Si création le 2024-11-20 à 14:30 → DateCreation = "2024-11-20T14:30:00Z" |
| RC-002 | `Statut = "Active"` | Statut par défaut à la création | Toute nouvelle ressource a le statut "Active" |

### Règles de Cohérence
- Une ressource créée doit avoir un ID unique (GUID généré par le système)
- L'email doit être unique à l'échelle de toutes les ressources
- Si le téléphone est saisi, il doit respecter un format valide (optionnel mais validé si présent)
```

**Règles** :
- Numéroter toutes les règles (RV-XXX pour validation, RC-XXX pour calcul)
- Fournir des exemples concrets de données valides/invalides
- Spécifier les formules de calcul exactes
- Documenter les règles de cohérence entre champs

### 9. Données Manipulées
```markdown
## Données Manipulées

### Entité Principale : Ressource

| Propriété | Type | Obligatoire | Valeur par Défaut | Validation | Plage de Valeurs | Exemple Valide |
|-----------|------|-------------|-------------------|------------|------------------|----------------|
| Id | GUID | ✅ (auto) | Généré système | - | - | "3fa85f64-5717-4562-b3fc-2c963f66afa6" |
| Prénom | Texte | ✅ | - | Max 50 car., lettres + espaces | 1-50 caractères | "Jean-Pierre" |
| Nom | Texte | ✅ | - | Max 50 car., lettres + espaces | 1-50 caractères | "Dupont" |
| Email | Email | ✅ | - | Format email, unique | - | "jp.dupont@example.com" |
| Téléphone | Texte | ❌ | NULL | Format international si saisi | - | "+33 6 12 34 56 78" |
| DateCreation | DateTime | ✅ (auto) | NOW() | - | - | "2024-11-20T14:30:00Z" |
| Statut | Enum | ✅ | "Active" | Valeurs : Active, Inactive, Archivée | - | "Active" |

**Exemple Complet de Données (JSON)** :
\`\`\`json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "prenom": "Jean-Pierre",
  "nom": "Dupont",
  "email": "jp.dupont@example.com",
  "telephone": "+33 6 12 34 56 78",
  "dateCreation": "2024-11-20T14:30:00Z",
  "statut": "Active"
}
\`\`\`

### Relations avec Autres Entités
- **Aucune relation directe** pour cette US (création simple)
- Les relations avec Compétences et Allocations seront gérées dans d'autres US
```

**Règles** :
- Documenter TOUTES les propriétés manipulées
- Préciser types, contraintes, valeurs par défaut
- Fournir des exemples JSON valides
- Identifier les relations (même si non manipulées dans cette US)

### 10. Interface Utilisateur (Vue Fonctionnelle)
```markdown
## Interface Utilisateur (Vue Fonctionnelle)

### Écrans Impliqués
| Écran | Type | Objectif | Navigation |
|-------|------|----------|------------|
| Liste des Ressources | Liste | Point d'entrée | Menu principal > Ressources |
| Formulaire Nouvelle Ressource | Formulaire | Saisie des données | Liste > Bouton "Nouvelle Ressource" |

### Écran : Formulaire Nouvelle Ressource

#### Informations Affichées
| Champ Affiché | Source de Données | Format | Condition d'Affichage |
|--------------|-------------------|--------|----------------------|
| Titre | Statique | Texte : "Nouvelle Ressource" | Toujours |
| Aide contextuelle | Statique | Texte : "Saisissez les informations essentielles de la ressource" | Toujours |

#### Champs de Saisie
| Champ | Type de Contrôle | Obligatoire | Placeholder | Aide Contextuelle | Validation Temps Réel |
|-------|-----------------|-------------|-------------|-------------------|----------------------|
| Prénom | Input texte | ✅ | "ex: Jean-Pierre" | "Prénom du collaborateur (max 50 caractères)" | Oui (à la sortie du champ) |
| Nom | Input texte | ✅ | "ex: Dupont" | "Nom du collaborateur (max 50 caractères)" | Oui |
| Email | Input email | ✅ | "ex: jp.dupont@example.com" | "Adresse email professionnelle (doit être unique)" | Oui |
| Téléphone | Input texte | ❌ | "ex: +33 6 12 34 56 78" | "Téléphone professionnel (optionnel)" | Oui si saisi |

#### Actions Disponibles
| Action | Type | Libellé | Icône | Position | Raccourci Clavier | Confirmation Requise |
|--------|------|---------|-------|----------|-------------------|---------------------|
| Enregistrer | Bouton primaire | "Enregistrer" | 💾 | Bas du formulaire, droite | Ctrl+S | Non |
| Annuler | Bouton secondaire | "Annuler" | ✖️ | Bas du formulaire, gauche | Echap | Oui si données saisies |

#### Messages et Feedbacks
- **Succès** : "Ressource créée avec succès" - Toast vert en haut à droite, 3 secondes
- **Erreur de validation** : Message inline sous le champ en erreur, texte rouge
- **Erreur globale** : Banner en haut du formulaire, fond rouge clair
- **Information** : "Vérification de l'email en cours..." - Inline sous le champ Email, texte bleu
- **Avertissement** : "Vous avez des modifications non enregistrées" - Modal lors de l'annulation

#### Navigation
- **Depuis** : Liste des Ressources via bouton "Nouvelle Ressource"
- **Vers (succès)** : Liste des Ressources avec la nouvelle ressource visible
- **Vers (annulation)** : Liste des Ressources sans création
- **Annulation** : Confirmation demandée si des données ont été saisies, sinon retour direct
```

**Règles** :
- Décrire tous les éléments d'interface de manière fonctionnelle (pas de détails visuels/CSS)
- Spécifier les libellés exacts, placeholders, aides contextuelles
- Documenter les actions avec raccourcis clavier
- Préciser les types de messages et leur affichage (toast, inline, modal)
- Décrire les flux de navigation complets

### 11. Critères d'Acceptation (Testables et Mesurables)
```markdown
## Critères d'Acceptation (Testables et Mesurables)

### Critères Fonctionnels
- [ ] **AC-001** : Étant donné que je suis authentifié en tant que Responsable RH et que je suis sur la liste des ressources, quand je clique sur "Nouvelle Ressource", alors le formulaire de création s'affiche en moins de 500ms
- [ ] **AC-002** : Étant donné que je suis sur le formulaire de création, quand je saisis "John" dans Prénom, "Doe" dans Nom, "john.doe@example.com" dans Email valide et unique, et que je clique sur "Enregistrer", alors la ressource est créée et le message "Ressource créée avec succès" s'affiche pendant 3 secondes
- [ ] **AC-003** : Étant donné que je suis sur le formulaire, quand je laisse le champ "Nom" vide et que je clique sur "Enregistrer", alors l'erreur "Le nom est obligatoire" s'affiche sous le champ et la ressource n'est PAS créée
- [ ] **AC-004** : Étant donné qu'une ressource avec l'email "existing@example.com" existe déjà, quand j'essaie de créer une ressource avec le même email, alors l'erreur "Une ressource avec cet email existe déjà" s'affiche et un lien vers la ressource existante est affiché

### Critères de Validation
- [ ] **AV-001** : La validation du format email fonctionne : "invalid-email" → Erreur "Format d'email invalide"
- [ ] **AV-002** : Les données valides suivantes sont acceptées sans erreur :
  - Prénom : "Jean-Pierre" (avec tiret)
  - Nom : "O'Connor" (avec apostrophe)
  - Email : "jp.oconnor+test@sub.example.com" (avec +, sous-domaine)
- [ ] **AV-003** : Un nom de 51 caractères est rejeté avec l'erreur "Le nom ne peut pas dépasser 50 caractères"

### Critères d'Interface
- [ ] **AI-001** : Le formulaire affiche les champs : Prénom*, Nom*, Email*, Téléphone (l'astérisque indique les champs obligatoires)
- [ ] **AI-002** : Le bouton "Enregistrer" est actif dès l'affichage du formulaire
- [ ] **AI-003** : Le bouton "Enregistrer" se désactive après le premier clic pour éviter les doubles soumissions
- [ ] **AI-004** : Le message de succès s'affiche en haut à droite sous forme de toast vert pendant exactement 3 secondes
- [ ] **AI-005** : Les champs en erreur sont mis en évidence en rouge avec le message d'erreur affiché en dessous

### Critères de Performance
- [ ] **AP-001** : L'affichage du formulaire de création répond en moins de 500ms
- [ ] **AP-002** : L'enregistrement d'une ressource s'effectue en moins de 1 seconde
- [ ] **AP-003** : La validation de l'unicité de l'email s'effectue en moins de 500ms

### Critères de Sécurité
- [ ] **AS-001** : Seul un utilisateur avec le rôle "Responsable RH" peut accéder au bouton "Nouvelle Ressource"
- [ ] **AS-002** : Un utilisateur avec le rôle "Chef de Projet" ne voit PAS le bouton "Nouvelle Ressource"
- [ ] **AS-003** : La création d'une ressource est tracée dans les logs d'audit avec : Utilisateur, DateTime, Action, Données créées
```

**Règles** :
- Définir AU MINIMUM 10 critères d'acceptation
- Utiliser le format Gherkin (Étant donné / quand / alors) pour les critères fonctionnels
- Couvrir : fonctionnel, validation, interface, performance, sécurité
- Chaque critère doit être testable automatiquement ou manuellement
- Inclure des métriques précises (temps de réponse, durée d'affichage)

### 12. Tests Fonctionnels Détaillés
```markdown
## Tests Fonctionnels Détaillés

### Test 1 : Création ressource valide (Happy Path)
**Objectif** : Vérifier la création d'une ressource avec des données valides  
**Pré-requis** : 
- Utilisateur authentifié en tant que Responsable RH
- Aucune ressource avec l'email "john.doe@test.com" n'existe

**Étapes** :
1. Naviguer vers "Ressources" > "Liste des Ressources"
2. Cliquer sur "Nouvelle Ressource"
3. Saisir :
   - Prénom : "John"
   - Nom : "Doe"
   - Email : "john.doe@test.com"
   - Téléphone : "+33 6 12 34 56 78"
4. Cliquer sur "Enregistrer"

**Données de Test** :
\`\`\`json
{
  "prenom": "John",
  "nom": "Doe",
  "email": "john.doe@test.com",
  "telephone": "+33 6 12 34 56 78"
}
\`\`\`

**Résultat Attendu** : 
- Ressource créée avec ID unique
- Message "Ressource créée avec succès" affiché (toast vert, 3s)
- Redirection vers liste des ressources
- Ressource "John Doe" visible dans la liste

**Criticité** : Bloquant

### Test 2 : Email invalide
**Objectif** : Vérifier le rejet d'un email invalide  
**Pré-requis** : Formulaire affiché

**Étapes** :
1. Saisir Prénom : "John", Nom : "Doe"
2. Saisir Email : "invalid-email"
3. Cliquer sur "Enregistrer"

**Données de Test** :
\`\`\`json
{
  "email": "invalid-email"
}
\`\`\`

**Résultat Attendu** :
- Erreur affichée sous le champ Email : "Format d'email invalide (ex: nom@example.com)"
- Champ Email mis en évidence en rouge
- Ressource NON créée
- Reste sur le formulaire

**Criticité** : Bloquant

### Test 3 : Email en doublon
**Objectif** : Vérifier la détection de doublon  
**Pré-requis** : Une ressource avec l'email "existing@test.com" existe déjà

**Étapes** :
1. Saisir les données avec Email : "existing@test.com"
2. Cliquer sur "Enregistrer"

**Données de Test** :
\`\`\`json
{
  "email": "existing@test.com"
}
\`\`\`

**Résultat Attendu** :
- Erreur : "Une ressource avec cet email existe déjà"
- Lien "Consulter la ressource existante" affiché
- Ressource NON créée

**Criticité** : Bloquant

### Test 4 : Champs obligatoires vides
**Objectif** : Vérifier la validation des champs obligatoires  
**Étapes** : Laisser Nom vide, cliquer sur "Enregistrer"  
**Résultat Attendu** : Erreur "Le champ Nom est obligatoire"  
**Criticité** : Majeur

### Test 5 : Caractères spéciaux valides
**Objectif** : Vérifier l'acceptation des caractères spéciaux courants  
**Données de Test** : Nom : "O'Connor", Prénom : "Jean-Marie"  
**Résultat Attendu** : Acceptés sans erreur  
**Criticité** : Mineur

### Test 6 : Nom trop long (dépassement limite)
**Objectif** : Vérifier la validation de la longueur max  
**Données de Test** : Nom de 51 caractères  
**Résultat Attendu** : Erreur "Le nom ne peut pas dépasser 50 caractères"  
**Criticité** : Mineur

### Test 7 : Double-clic sur Enregistrer
**Objectif** : Vérifier la protection contre les doubles soumissions  
**Étapes** : Double-clic rapide sur "Enregistrer"  
**Résultat Attendu** : Une seule ressource créée, bouton désactivé après 1er clic  
**Criticité** : Majeur

### Test 8 : Annulation avec données saisies
**Objectif** : Vérifier la confirmation d'annulation  
**Étapes** : Saisir données, cliquer sur "Annuler"  
**Résultat Attendu** : Modal de confirmation affichée, aucune ressource créée après confirmation  
**Criticité** : Mineur

### Test 9 : Performance création
**Objectif** : Vérifier le temps de réponse  
**Mesure** : Temps entre clic "Enregistrer" et affichage message succès  
**Résultat Attendu** : < 1 seconde  
**Criticité** : Mineur

### Test 10 : Autorisation refusée (Chef de Projet)
**Objectif** : Vérifier le contrôle d'accès  
**Pré-requis** : Utilisateur avec rôle "Chef de Projet"  
**Résultat Attendu** : Bouton "Nouvelle Ressource" non visible  
**Criticité** : Bloquant
```

**Règles** :
- Définir AU MINIMUM 10 tests fonctionnels
- Couvrir : nominal, erreurs, cas limites, performance, sécurité
- Fournir des données de test concrètes (JSON)
- Préciser le résultat attendu de manière observable
- Prioriser : Bloquant, Majeur, Mineur

### 13. Estimation et Complexité
```markdown
## Estimation et Complexité
- **Story Points** : 3 points
- **Complexité** : Moyenne
- **Justification** : 
  - Formulaire simple avec 4 champs
  - Validation standard (format, unicité)
  - Pas d'intégration complexe
  - Mais nécessite gestion des erreurs et cas limites
```

**Règles** :
- Estimer en story points (échelle de Fibonacci : 1, 2, 3, 5, 8, 13)
- Évaluer la complexité (Simple / Moyenne / Élevée)
- Justifier l'estimation (points de complexité identifiés)

### 14. Définition of Done (DoD)
```markdown
## Définition of Done (DoD)
- [ ] Tous les critères d'acceptation sont remplis et testés
- [ ] Tous les tests fonctionnels passent (10/10)
- [ ] Les scénarios d'erreur sont couverts et testés (3/3)
- [ ] Les cas limites sont testés (5/5)
- [ ] Les performances répondent aux exigences (< 1s création, < 500ms affichage)
- [ ] Les permissions/autorisations sont validées (RH Manager ✅, Chef Projet ❌)
- [ ] Le code est reviewé et approuvé par un pair
- [ ] Les tests unitaires et d'intégration sont écrits et passent
- [ ] La documentation utilisateur est mise à jour (si nécessaire)
- [ ] La revue fonctionnelle est effectuée avec le Product Owner et validée
- [ ] Aucun bug bloquant ou majeur ouvert
- [ ] Déployé et validé en environnement de recette
```

**Règles** :
- Définir une DoD exhaustive et non ambiguë
- Inclure : tests, code review, documentation, validation métier
- Chaque case doit être cochable de manière binaire (fait / pas fait)

### 15. Dépendances
```markdown
## Dépendances

### Dépend de (Bloquants)
- **Aucune dépendance bloquante** pour cette US (création simple sans relations)

### Bloque (Bloqués)
- US-0002-ajouter-competences-ressource : Nécessite qu'une ressource existe avant de pouvoir lui ajouter des compétences
- US-0007-creer-allocation : Nécessite qu'une ressource existe avant de pouvoir l'allouer à un projet

### Dépendances Externes
- **Aucune dépendance externe** (pas d'intégration SIRH pour cette US)
```

**Règles** :
- Documenter toutes les dépendances bloquantes
- Identifier les US bloquées par celle-ci
- Lister les dépendances externes (systèmes, API, données)

### 16. Questions Ouvertes / Points à Clarifier
```markdown
## Questions Ouvertes / Points à Clarifier
- ❓ **[RÉSOLU - 2024-11-22]** Faut-il valider le format du téléphone si saisi ? → OUI, validation internationale si saisi
- ❓ Doit-on envoyer un email de bienvenue au nouveau collaborateur ? → En attente décision métier
- ⚠️ Point d'attention : Le SIRH actuel ne sera pas intégré dans cette US (import manuel prévu dans US-0015)
```

**Règles** :
- Documenter toutes les questions non résolues avec ❓
- Tracer les décisions avec date de résolution
- Marquer les points d'attention avec ⚠️
- Mettre à jour dès résolution

### 17. Historique des Modifications
```markdown
## Historique des Modifications
| Date | Version | Auteur | Modifications |
|------|---------|--------|---------------|
| 2024-11-20 | 1.0.0 | Marie Dupont | Création initiale de la US |
| 2024-11-22 | 1.1.0 | Jean Martin | Ajout validation unicité email suite à revue |
| 2024-11-23 | 1.1.1 | Marie Dupont | Clarification message d'erreur pour email invalide |
| 2024-11-25 | 2.0.0 | Marie Dupont | Ajout cas limite double-clic (breaking change DoD) |
```

**Règles** :
- Versioning sémantique (SemVer)
- Documenter toutes les modifications
- Ne JAMAIS supprimer l'historique

### 18. Références
```markdown
## Références
- **Feature Parent** : FEATURE-0002-creer-ressource
- **EPIC Parent** : EPIC-0002-gestion-ressources
- **ADR liés** : 
  - ADR-015 : Validation emails côté serveur (impact règle RV-003)
- **Personas** :
  - responsable-rh.personna.md
- **Documentation externe** :
  - RFC 5322 (format email)
```

**Règles** :
- Lier systématiquement à Feature et EPIC parents
- Référencer les ADR impactant cette US
- Lier aux personas
- Citer les standards/RFC utilisés

## Checklist de Validation

Avant de considérer une US comme complète et READY :

### Complétude
- [ ] Format User Story ("En tant que... Je veux... Afin de...") respecté
- [ ] Atomique (une seule fonctionnalité, pas un ensemble)
- [ ] Pré-conditions et post-conditions définies
- [ ] Workflow détaillé avec Gherkin + description narrative
- [ ] Scénarios : 1 nominal + 2 alternatifs + 3 erreurs minimum
- [ ] Cas limites : 5 minimum identifiés
- [ ] Règles métier spécifiques numérotées et complètes
- [ ] Données manipulées avec exemples JSON
- [ ] Interface utilisateur décrite (champs, actions, messages exacts)
- [ ] Critères d'acceptation SMART : 10 minimum
- [ ] Tests fonctionnels : 10 minimum avec données de test
- [ ] Estimation (story points) fournie et justifiée
- [ ] Dépendances identifiées

### Qualité
- [ ] Français correct, ton professionnel
- [ ] Agnostique de la technologie
- [ ] Pas de contradiction avec la feature ou l'EPIC
- [ ] Messages utilisateur exacts et actionnables
- [ ] Codes d'erreur uniques
- [ ] Liens entre documents fonctionnels
- [ ] Historique des modifications à jour

### Testabilité
- [ ] Critères d'acceptation testables (pas d'ambiguïté)
- [ ] Données de test concrètes fournies
- [ ] Résultats attendus observables et mesurables
- [ ] Cas d'erreur avec récupération documentée
- [ ] Performance mesurable (< X ms)

## Bonnes Pratiques

### ✅ À Faire
- Impliquer un utilisateur final dans la validation
- Utiliser des données réelles anonymisées
- Tester manuellement le workflow avant rédaction
- Réviser avec le Product Owner avant READY
- Spécifier les messages exacts (ne pas paraphraser)
- Documenter les décisions métier prises

### ❌ À Éviter
- US trop large (> 5 story points → découper)
- Critères d'acceptation vagues ("L'interface doit être intuitive")
- Ignorer les cas d'erreur et limites
- Oublier les autorisations
- Messages d'erreur techniques ("Error 500", "NullPointerException")
- Mélanger plusieurs fonctionnalités dans une US

## Maintenance

### Révision
- **Fréquence** : À chaque Sprint si modifications, sinon stable
- **Déclencheurs** : Bugs, feedback utilisateurs, changement de périmètre
- **Actions** : Mettre à jour scénarios, critères, tests

### Dépréciation
Si obsolète :
1. Statut = `[DEPRECATED]`
2. Ajouter note en haut :
   ```markdown
   > ⚠️ **DEPRECATED** : Cette US est obsolète.  
   > Remplacée par : US-{ID}  
   > Date : YYYY-MM-DD  
   > Raison : {Explication}
   ```
3. Ne JAMAIS supprimer
4. Mettre à jour Feature et EPIC parents

## Exemple Complet

Voir les User Stories existantes dans `documentations/functionnals/` pour des exemples concrets.