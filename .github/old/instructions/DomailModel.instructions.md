---
applyTo: "**/*DomainModel.cs"
---
## DomainModel design principles

Ces consignes cadrent la structure des DomainModels côté lecture afin d'assurer une expérience homogène entre les handlers, les services de mapping et les différentes surfaces (WebApi, WebApp).
- Always use meaningful names for DomainModels that reflect their purpose in the consuming view.
- Use the DomainModel to represent the read-only view returned by the feature's CQRS query.
- Return the DomainModel instance unchanged from CQRS query handlers.
- Create a new DomainModel instance in each CQRS command handler instead of mutating an existing instance.
- If the model is linked to another entity add related XxxLinkedDomainModels in the same folder of the main XxxDomainModel.
- Use a dedicated MapperService to build the DomainModel from persistence models and other data sources.
- Always map DisplayName properties (CreatedByDisplayName, UpdatedByDisplayName) in the MapperService using the Graph Service with CreatedById and UpdatedById.
- Remove all business logic from the DomainModel.
- Declare every DomainModel as a `record` type.
- Place every DomainModel file in the `Models` folder next to its handler.
- Name every DomainModel file and type with PascalCase ending in `DomainModel`.
- Associate every DomainModel with a MapperService located in the same feature scope.
- Make the MapperService apply default values when building the DomainModel.
- Make the MapperService enforce deterministic ordering of every collection property.
- Make the MapperService filter out inactive or disabled items before constructing the DomainModel.
- Unit-test every public method of the MapperService.
- Keep the DomainModel free of persistence attributes and persistence model references.
- Expose only the properties required by the consuming view.
- Declare every required property as a non-nullable type.
- Annotate every optional property with a nullable reference type.
- Document the default value for every optional property in XML comments.
- Expose every collection property through an immutable interface such as `IReadOnlyList<T>` or `ImmutableArray<T>`.
- Initialize every collection property with an empty instance.
- Prevent circular references between DomainModels.
- Select property types that serialize without custom converters.
- Store timestamps in `CreatedAt` and `UpdatedAt` as UTC values.
- Expose `CreatedById` and `UpdatedById` properties. uniquement en output.
- Expose `createdByDisplayName` and `updatedByDisplayName` when display names are available. Uniquement en output.
- remove audit properties prefixed with `author`.
- remove audit properties prefixed with `userPrincipalName`.
- Write integration tests for every handler that returns a DomainModel.
- Add proprerties with DomainModel for linked entities as needed by the consuming view. Use existing domain models when possible.