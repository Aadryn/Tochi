
## Mandatory Naming Conventions
- Tous les modèles de la couche domaine doivent respecter la nomenclature `*DomainModel`.
- Tous les modèles de la couche hosting/application doivent respecter la nomenclature `*ViewModel`.
- Tous les modèles de la couche persistance doivent respecter la nomenclature `*PersistenceModel`.
- Il doit y avoir une ségrégation stricte entre les modèles de chaque couche.
- Il doit y avoir une ségrégation stricte entre les modèles Catalog, Management, et Statistics de la couche domaine.
- Tous les handlers de requêtes doivent respecter la nomenclature `*QueryHandler`.
- Tous les handlers de commandes doivent respecter la nomenclature `*CommandHandler`.
- Tous les handlers d'événements doivent respecter la nomenclature `*EventHandler`.
- Tous les services doivent respecter la nomenclature `*Service`.

## Prompts
- Un prompt est public si il appartient à une collection publique. Sinon il est privé.
- Un utilisateur peut créer uniquement des prompts privés.
- Un utilisateur peut soumettre un prompt à une collection publique via une demande d'inclusion.
- Une fois le prompt approuvé par un administrateur ou un approbateur, il devient public. Et l'utilisateur ne peut plus le modifier.
- Un utilisateur peut créer, modifier et supprimer ses propres prompts privés.
- Un administrateur peut créer, modifier et supprimer n'importe quel prompt.
- Un prompt peut avoir plusieurs tags.

## Collections
- Une collection est publique si elle est marquée comme telle. Sinon elle est privée.
- Un utilisateur peut créer uniquement des collections privées.
- Seul un administrateur peut créer des collections publiques.
- Un collection hérite des tags de ses prompts.
- Un collection peut avoir plusieurs tags.

## Tags
- Un tag est par defaut en mode IsApproved = false.
- Un administrateur peut approuver un tag en le passant en mode IsApproved = true.
- Un utilisateur peut créer des tags non approuvés.
- Un administrateur peut créer des tags approuvés.
- Un utilisateur peut utiliser uniquement des tags approuvés pour ses prompts. Ou des tags qu'il a créés lui-même.
- Un administrateur peut utiliser n'importe quel tag pour n'importe quel prompt.
- Un utilisateur peut créer, modifier et supprimer ses propres tags non approuvés.
- Un administrateur peut créer, modifier et supprimer n'importe quel tag.


## Segregation of Handlers usages.
Faire une segrégation complete dans l'utilisation des handlers. L'ensemble des composants du projet :
GroupeAdp.GenAi.Hostings.WebApp.Default.Endpoint ne doit utiliser que des handlers du namespace GroupeAdp.GenAi.Domains.Commons\Catalogs
GroupeAdp.GenAi.Hostings.WebApp.Management.Endpoint ne doit utiliser que des handlers du namespace GroupeAdp.GenAi.Domains.Commons\Management
Ne jamais mélanger les handlers entre les deux projets.
S'assurer que les handlers n'utilisent pas des modèles d'autres namespaces. (Catalog, Management)
Les handlers du namespaces Statisitics peuvent être utilisés par les deux projets.


## Cycle TDD Structuré
- Tu appliques strictement le cycle RED → GREEN → REFACTOR.
	- **Red** : tu dérives un test par comportement demandé, tu le fais échouer pour la bonne raison et tu références l'issue concernée.
	- **Green** : tu implémentes le minimum pour passer le test en restant dans le périmètre de l'issue.
	- **Refactor** : tu élimines la duplication, tu appliques les principes SOLID et tu maintiens les tests au vert.
- Tu étends la couverture de tests dès qu'un nouvel edge case est identifié.

## Standards d'Ingénierie
- Tu appliques KISS, YAGNI, DRY et SOLID à chaque modification et tu justifies explicitement toute entorse.
- Tu sépares strictement catalog, management et statistics; aucune fuite de modèle entre couches n'est tolérée.
- Tu assures la sécurité de base : entrées validées, secrets protégés, dépendances vérifiées.

# Management Hosting & Domaines [Mandatory]
- Seul les utilisateurs avec le role "administrator" ou "approbator" peuvent accéder à l'application de management.
- Un utilisateur avec le role "approbator", peuvent uniquement gérer les prompts associés à des collections pour lesquelles il est désigné comme approbateur a travers CollectionPermissions.


# Peristence models [Mandatory]
- Supprime le champ IsPinned des modèles de persistance.
- Supprime le champ Metadata des modèles de persistance.
- Supprime le champ IsFavorite dans le modèle Collection de persistance.
- Supprime le Status de type PromptStatus dans le modèle Prompt de persistance.
- Adapte toutes les couches (domain, hosting, services, handlers, tests) en conséquence.
- Adapte les tests en conséquence.

# Hosting Layers [Mandatory]
- Ne jamais faire des chargements massifs de données dans les composants d'interface utilisateur.
- Toujours utiliser la pagination pour charger les données dans les composants d'interface utilisateur.	
- Toujours utiliser des filtres pour limiter le nombre de données chargées dans les composants d'interface utilisateur.
- Utiliser les command handlers et query handlers pour charger les données depuis la couche domaine dans les composants d'interface utilisateur.
- Utiliser des ViewModels dans les composants d'interface utilisateur.
- Ne jamais utiliser des DomainModels dans les composants d'interface utilisateur.


# Domain Layers [Mandatory]
- Utiliser des IQueryable dans les repositories pour permettre la pagination et le filtrage des données au niveau de la base de données.
- Mutualiser et haroniser les handlers pour éviter la duplication de code.
- Utiliser des specifications pour encapsuler la logique de filtrage et de recherche dans les handlers.
- Utiliser des DomainModels dans la couche domaine.
- Ne jamais utiliser des PersistenceModels ou des ViewModels dans la couche domaine.

# Peristence Layers [Mandatory]
- Les services doivent exposer des méthodes asynchrones pour toutes les opérations de lecture et d'écriture.
- Utiliser des transactions pour les opérations qui impliquent plusieurs étapes d'écriture dans la base de données.
- Utiliser des index sur les colonnes fréquemment utilisées pour les filtres et les tris.

# Documentation [Mandatory]
- Ecrit les spécifications techniques et fonctionnelles pour chaque nouvelle fonctionnalité avant de commencer le développement.
- Mets à jour la documentation existante pour refléter les changements apportés au code.
- Utiliser un pattern given-when-then pour documenter les comportements attendus dans les tests. Toujours en français dans le texte. Les given-when-then doivent être en français.
## Processus Opérationnel
- Tu établis un plan atomique avant chaque action et tu le matérialises dans une todo list en Markdown.
- Tu tiens la todo list à jour : un seul item `in-progress`, tu coches immédiatement les tâches terminées et tu ajoutes les découvertes utiles.
- Tu annonces chaque appel d'outil par une phrase courte qui précise l'intention avant l'exécution.

## Recherche et Informations
- Tu consommes chaque URL fournie avec `fetch_webpage` et tu suis les liens pertinents jusqu'à disposer d'une vision complète.
- Tu consignes tout blocage (captcha, accès refusé) et tu proposes une alternative de recherche vérifiable.
- Tu bases chaque décision sur des sources datées, que tu cites dans le récapitulatif final.

## Cycle TDD Structuré
- Tu appliques strictement le cycle RED → GREEN → REFACTOR.
	- **Red** : tu dérives un test par comportement demandé, tu le fais échouer pour la bonne raison et tu références l'issue concernée.
	- **Green** : tu implémentes le minimum pour passer le test en restant dans le périmètre de l'issue.
	- **Refactor** : tu élimines la duplication, tu appliques les principes SOLID et tu maintiens les tests au vert.
- Tu étends la couverture de tests dès qu'un nouvel edge case est identifié.

## Standards d'Ingénierie
- Tu appliques KISS, YAGNI, DRY et SOLID à chaque modification et tu justifies explicitement toute entorse.
- Tu sépares strictement catalog, management et statistics; aucune fuite de modèle entre couches n'est tolérée.
- Tu assures la sécurité de base : entrées validées, secrets protégés, dépendances vérifiées.

## Communication et Validation
- Tu restes factuel, direct et concis; tu évites toute duplication ou digression.
- Tu exécutes les builds, linters et tests pertinents après chaque changement significatif et tu en diffuses les résultats.
- Tu finalises chaque livraison par un résumé structuré : actions menées, sources utilisées, risques restants et prochaines étapes éventuelles.

