---
applyTo: "**/*Handler.cs,**/*Query.cs,**/*Command.cs"
---
# Command and Query Guidelines
- Declare every command and query as a `record` type.
- Add a `userId` property to every command and query record.
- Use only `userId` as a property name for the user identifier.

# Result Model Guidelines
- The result model should always following the pattern: 
  - `Data` (generic type `T`): Contains the result data when the operation is successful.
  - `IsSuccess` (bool): Indicates whether the operation was successful.
  - `ErrorMessage` (string?): Contains an error message if the operation failed.

# Handler Implementation Guidelines
- For Request and Response models, use the established naming conventions: `GetXxxQuery`, `GetXxxQueryHandler`, `CreateXxxCommand`, `CreateXxxCommandHandler`, etc. and `GetXxxQueryResult`, `CreateXxxCommandResult`, etc.
- Implements a model for Data in the result, use the `DomainModel` pattern as per the DomainModel design principles. If the model is linked to another entity add light models in the same folder of the main DomainModel.
- Emit a structured log entry when a handler starts executing.
- Emit a structured log entry when a handler finishes, including the execution outcome.
- Increment a success counter metric whenever a handler returns without error.
- Increment a failure counter metric inside the handler's error path.
- Measure handler execution time with a duration histogram metric.
- Tag every metric with the `userId` from the command or query.
- Tag every metric with the aggregate identifier when the message exposes an `entityId` or equivalent value.
- Attach correlation and trace identifiers to every emitted log entry.
- Attach correlation and trace identifiers to every telemetry event and metric.
- Enclose the handler body in a try/catch block that converts exceptions into domain-specific error results.
- Populate each error result with a localized, user-friendly message.
- Remove internal technical details such as stack traces from error results before returning them.
- Always do the CreatedByDisplayName, UpdatedByDisplayName mapping in the handler with the Graph Service withe CreatedById, UpdatedById

## Mandatory Telemetry
- Use a dedicated `IMeter` fpr the Entity, e.g. `UserFavoriteMeter` for UserFavorite handlers.
- Use a dedicated `Counter` for success and failure, e.g. `GetUserFavoritesSuccessCounter`, `GetUserFavoritesFailureCounter` for `GetUserFavoritesQueryHandler`.
- Use a dedicated `Histogram` for duration, e.g. `GetUserFavoritesDurationHistogram` for `GetUserFavoritesQueryHandler`.
- Use a dedicated `UpDownCounter` for queue length or similar metrics, e.g. `GetUserFavoritesQueueLength` for `GetUserFavoritesQueryHandler`.
- Use a dedicated `Gauge` for current values, e.g. `ActiveUserFavoritesGauge` for `GetUserFavoritesQueryHandler`.
- Use a dedicated `Asynchronous Gauge` for current values that are collected once per export, e.g. `ActiveUserFavoritesAsyncGauge` for `GetUserFavoritesQueryHandler`.
- Use a dedicated `Asynchronous Counter` for values that are collected once per export, e.g. `GetUserFavoritesAsyncCounter` for `GetUserFavoritesQueryHandler`.
- Metric must always be pertinent to the handler's purpose.
- Always tag metrics with metadata of the command or query, e.g. `userId`, `entityId`, etc.
- Always tag metrics with metadata of the handler's execution context, e.g. `correlationId`, `traceId`, etc.
- Always record metrics in both success and failure paths.
- Always record metrics in a `finally` block to ensure they are captured even if an exception occurs.
- Use `ActivitySource` to create an activity for the handler's execution.
- Add relevant tags to the activity, e.g. `userId`, `entityId`, etc.
- Ensure the activity is started at the beginning of the handler and stopped at the end.
- Use helper methods or middleware to reduce boilerplate code for telemetry.



## Mandatory for Management handlers
- Always verify that the userId has `administrator` or `approbator` role before proceeding.
- `administrator` can view and manage all entities.
- `approbator` can view and manage only collections and prompts linked to CollectionsPermission that they own (userId, or groupId they belong to).
- Restrict any other access with a `ForbiddenError` result.
