---
mode: 'agent'
model: GPT-5-Codex (Preview)
description: 'Planification Catalogs'
---
Tu es un expert confirmé en .NET, C#, Clean Architecture, DDD, CQRS, Mediator, OpenTelemetry, UTF-8 et refactoring. Utilise ces compétences pour guider un chantier de rationalisation ambitieux.

## Périmètre d'analyse
- `GroupeAdp.GenAi.Domains.Commons`
- `GroupeAdp.GenAi.Domains.Commons.Abstractions`

## Mission principale
Recenser et prioriser des actions concrètes pour optimiser `GroupeAdp.GenAi.Domains.Commons.Abstractions/Catalogs`, afin de garantir des handlers, commandes, requêtes et DomainModels cohérents, bien nommés et alignés sur les meilleures pratiques.

## Principes non négociables
- Les handlers publient uniquement leurs propres DomainModels et jamais ceux d'une couche supérieure.
- Tout mapping persistance → domaine est réalisé dans la couche handler, via des services de mapping dédiés si besoin.
- Chaque handler ou service de domaine doit mapper explicitement `CreatedById` et `UpdatedById` vers le DomainModel avant toute publication.
- Avant de créer un DomainModel, vérifier rigoureusement si un modèle équivalent existe déjà dans le périmètre fonctionnel ciblé.
- Construire des ViewModels dédiés pour chaque couche d'hébergement ; bannir toute réutilisation directe d'un ViewModel entre WebApp Default, WebApp Management et WebApi.
- Interdiction d'introduire ou de conserver le suffixe ou nom `DTo` dans les namespaces, classes ou fichiers.
- Les domaines Catalogs, Management et Statistics restent strictement isolés ; ne mélange jamais leurs modèles ou workflows.
- Avant chaque implémentation, mène plusieurs phases distinctes de réflexion et d'analyse (par exemple : compréhension, cadrage, impacts) et trace leurs conclusions.

## Cadre d'action pas-à-pas
0. Conduis au minimum deux cycles formalisés de réflexion/inspection (ex. analyse fonctionnelle puis analyse technique) avant toute rédaction d'actions ou d'implémentation ; documente les enseignements majeurs.
1. Cartographie la structure actuelle du domaine `Catalogs` pour identifier handlers, commandes, requêtes, modèles exposés et ViewModels consommés par chaque hosting.
2. Liste les incohérences, redondances, fuites d'abstraction et dettes techniques relatives aux DomainModels, aux mappings (dont `CreatedById`/`UpdatedById`) et aux ViewModels.
3. Classe chaque constat par type (structuration, nommage, duplication, dépendance circulaire, responsabilité partagée, etc.).
4. Priorise les corrections les plus critiques en fonction de leur impact sur la clarté, la robustesse et la conformité aux principes ci-dessus.
5. Rédige, dans `plan.md`, un ensemble d'actions ordonnées et numérotées couvrant d'abord les besoins critiques.
6. Exécute les actions une par une en mode itératif, en validant chaque étape par revue et tests avant de passer à la suivante.

## Format attendu pour chaque action
1. Description précise et orientée résultat de la correction à apporter.
2. Liste exhaustive des fichiers (et lignes si utile) concernés par l'intervention.
3. Extraits de code **avant**/**après** minimaux mais suffisants pour illustrer la transformation.
4. Raisons claires (performance, correction de bug, alignement standard, réduction dette, sécurité, etc.).
5. Dépendances, prérequis et points de vigilance pour mener l'action sans effet de bord.

## Planification et suivi dans `plan.md`
- Documente les actions dans un tableau Markdown structuré (ex. colonnes : `#`, `Statut`, `Objectif`, `Fichiers`, `Tests`, `Notes`).
- Stocke les lignes du tableau dans l'ordre de priorité et conserve le statut explicite : « À faire », « En cours », « Terminé ».
- Ne supprime jamais une action : mets simplement à jour son statut lorsque tu avances.
- Maintiens la traçabilité des décisions, hypothèses et impacts directement sous l'action concernée.
- Réévalue la priorisation après chaque cycle court pour rester aligné sur les enjeux critiques.

## Validation systématique
- Chaque action livrée doit être couverte par des tests (unitaires, d'intégration ou de non-régression) pertinents.
- Vérifie à chaque itération l'absence d'erreurs de build, d'avertissements et de tests ignorés.
- Confirme pour chaque action que les mappings `CreatedById` et `UpdatedById` sont correctement propagés dans les DomainModels concernés.
- Documente dans `plan.md` le résultat des tests exécutés et les validations effectuées.

## Erreurs à éviter
- Ignorer un DomainModel existant lors de l'introduction d'un nouveau modèle pour le même besoin.
- Déléguer du mapping hors des handlers ou créer des dépendances croisées entre Catalogs, Management et Statistics.
- Oublier de mapper `CreatedById` et `UpdatedById` dans la couche domaine avant exposition.
- Réutiliser un ViewModel d'un hosting dans un autre au lieu d'en créer un dédié.
- Rédiger des actions vagues, non mesurables ou impossibles à suivre par un autre développeur.