---
mode: 'agent'
model: Claude Sonnet 4
description: 'Action'
---

Tu vas analyser en détail les projets suivants :
- GroupeAdp.GenAi.Domains.Commons
- GroupeAdp.GenAi.Domains.Commons.Abstractions
- GroupeAdp.GenAi.Domains.Commons.Abstractions.Unit.Tests
- GroupeAdp.GenAi.Domains.Commons.Unit.Tests

Pour chacun des handlers, tu dois rajouter un appel au metering, avec les métadonnées nécessaires (request, userid).
Efectue une analyse approfondie du code pour identifier les handlers qui ne sont pas compliants avec les exigences suivantes.
Tu mettras à jour en premier les tests, puis tu mettras à jour chacune handler.


La propriété IsEnabled d'une entité correspond à la suppression logique. Il faut filtrer les entités sur cette propriété.
La propriété IsActive d'une entité correspond à la validité de l'entité. 
Corriger toutes les erreurs d'encodage utf-8.

All the abstractions must be in the Abstractions project.
All the implementations must be in the Commons project.
All the documentation must be in english.

Setup activitySource for OpenTelemetry tracing in each handler. Use an helper to avoid code duplication.
Example :
using var activity = Activity.Current?.Source.StartActivity("GetCollectionByIdQueryHandler.Handle");
activity?.SetTag("collection.id", request.Id.ToString());
activity?.SetTag("operation", "get_collection_by_id");


# Mandatory
Traitre en priorité les handlers qui ne sont pas compliants avec les exigences
La couche domain ne doit jamais exposer les modèles de la couche infrastructure.
Toujours créer des domain models et faire le mapping dans les handlers.
Stocker ces différents models dans le projet Abstractions.
Procède de façon pragmatique, organisé, en suivant l'ordre alphabétique.
Tu dois aussi rationaliser le code entre les différents services pour mutualiser un maximum de code.
Zero failing test, Zero build warning, Zero build errors.