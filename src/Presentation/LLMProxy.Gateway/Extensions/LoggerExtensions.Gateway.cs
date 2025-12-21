using Microsoft.Extensions.Logging;

namespace LLMProxy.Gateway.Extensions;

/// <summary>
/// Extensions de logging haute performance pour le module Gateway.
/// </summary>
/// <remarks>
/// Utilise LoggerMessage avec source generators pour améliorer les performances
/// de logging (3-6x plus rapide que l'interpolation de chaînes).
/// 
/// <para><strong>Bénéfices</strong> :</para>
/// <list type="bullet">
/// <item>Pas d'allocation de chaînes inutiles</item>
/// <item>Pas de boxing/unboxing des paramètres</item>
/// <item>Code généré au compile-time</item>
/// <item>Type-safety garantie</item>
/// </list>
/// 
/// <para><strong>EventIds convention</strong> :</para>
/// <list type="bullet">
/// <item>2xxx : Request/Response</item>
/// <item>3xxx : Exceptions</item>
/// <item>4xxx : Quota</item>
/// <item>5xxx : Streaming</item>
/// </list>
/// 
/// <para><strong>Exemple</strong> :</para>
/// <code>
/// // Avant (interpolation)
/// _logger.LogInformation("HTTP {Method} {Path} started", method, path);
/// 
/// // Après (LoggerMessage)
/// _logger.LogRequestStarted(method, path, requestId);
/// </code>
/// 
/// Conforme à : ADR-031 (Structured Logging - performance optimized)
/// </remarks>
public static partial class GatewayLoggerExtensions
{
    /// <summary>
    /// Enregistre le démarrage d'une requête HTTP.
    /// </summary>
    /// <param name="logger">Instance de logger.</param>
    /// <param name="method">Méthode HTTP (GET, POST, etc.).</param>
    /// <param name="path">Chemin de la requête.</param>
    /// <param name="requestId">Identifiant unique de la requête.</param>
    /// <remarks>
    /// EventId: 2001 | Level: Information
    /// Message: "HTTP {Method} {Path} started - RequestId: {RequestId}"
    /// </remarks>
    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Information,
        Message = "HTTP {Method} {Path} started - RequestId: {RequestId}")]
    public static partial void LogRequestStarted(
        this ILogger logger, string method, string path, Guid requestId);

    /// <summary>
    /// Enregistre la complétion d'une requête HTTP.
    /// </summary>
    /// <param name="logger">Instance de logger.</param>
    /// <param name="method">Méthode HTTP.</param>
    /// <param name="path">Chemin de la requête.</param>
    /// <param name="statusCode">Code de statut HTTP.</param>
    /// <param name="durationMs">Durée de traitement en millisecondes.</param>
    /// <param name="requestId">Identifiant unique de la requête.</param>
    /// <remarks>
    /// EventId: 2002 | Level: Information
    /// Message: "HTTP {Method} {Path} completed in {DurationMs}ms with status {StatusCode} - RequestId: {RequestId}"
    /// </remarks>
    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Information,
        Message = "HTTP {Method} {Path} completed in {DurationMs}ms with status {StatusCode} - RequestId: {RequestId}")]
    public static partial void LogRequestCompleted(
        this ILogger logger, string method, string path, int statusCode, double durationMs, Guid requestId);

    /// <summary>
    /// Enregistre une erreur lors du traitement d'une requête.
    /// </summary>
    /// <param name="logger">Instance de logger.</param>
    /// <param name="exception">Exception capturée.</param>
    /// <param name="method">Méthode HTTP.</param>
    /// <param name="path">Chemin de la requête.</param>
    /// <param name="requestId">Identifiant unique de la requête.</param>
    /// <remarks>
    /// EventId: 2003 | Level: Error
    /// Message: "Error in request {Method} {Path} - RequestId: {RequestId}"
    /// </remarks>
    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Error,
        Message = "Error in request {Method} {Path} - RequestId: {RequestId}")]
    public static partial void LogRequestError(
        this ILogger logger, Exception exception, string method, string path, Guid requestId);

    /// <summary>
    /// Enregistre une requête annulée par le client.
    /// </summary>
    /// <param name="logger">Instance de logger.</param>
    /// <param name="path">Chemin de la requête.</param>
    /// <remarks>
    /// EventId: 3001 | Level: Information
    /// Message: "Request cancelled by client: {Path}"
    /// </remarks>
    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Information,
        Message = "Request cancelled by client: {Path}")]
    public static partial void LogRequestCancelled(
        this ILogger logger, string path);

    /// <summary>
    /// Enregistre une tentative d'accès non autorisé.
    /// </summary>
    /// <param name="logger">Instance de logger.</param>
    /// <param name="exception">Exception capturée.</param>
    /// <param name="path">Chemin de la requête.</param>
    /// <remarks>
    /// EventId: 3002 | Level: Warning
    /// Message: "Unauthorized access attempt: {Path}"
    /// </remarks>
    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Warning,
        Message = "Unauthorized access attempt: {Path}")]
    public static partial void LogUnauthorizedAccess(
        this ILogger logger, Exception exception, string path);

    /// <summary>
    /// Enregistre un argument invalide.
    /// </summary>
    /// <param name="logger">Instance de logger.</param>
    /// <param name="exception">Exception capturée.</param>
    /// <param name="path">Chemin de la requête.</param>
    /// <remarks>
    /// EventId: 3003 | Level: Warning
    /// Message: "Invalid argument: {Path}"
    /// </remarks>
    [LoggerMessage(
        EventId = 3003,
        Level = LogLevel.Warning,
        Message = "Invalid argument: {Path}")]
    public static partial void LogInvalidArgument(
        this ILogger logger, Exception exception, string path);

    /// <summary>
    /// Enregistre une opération invalide.
    /// </summary>
    /// <param name="logger">Instance de logger.</param>
    /// <param name="exception">Exception capturée.</param>
    /// <param name="path">Chemin de la requête.</param>
    /// <remarks>
    /// EventId: 3004 | Level: Error
    /// Message: "Invalid operation: {Path}"
    /// </remarks>
    [LoggerMessage(
        EventId = 3004,
        Level = LogLevel.Error,
        Message = "Invalid operation: {Path}")]
    public static partial void LogInvalidOperation(
        this ILogger logger, Exception exception, string path);

    /// <summary>
    /// Enregistre une exception non gérée.
    /// </summary>
    /// <param name="logger">Instance de logger.</param>
    /// <param name="exception">Exception capturée.</param>
    /// <param name="path">Chemin de la requête.</param>
    /// <param name="method">Méthode HTTP.</param>
    /// <param name="statusCode">Code de statut HTTP retourné.</param>
    /// <remarks>
    /// EventId: 3005 | Level: Error
    /// Message: "Unhandled exception: {Path} | {Method} | {StatusCode}"
    /// </remarks>
    [LoggerMessage(
        EventId = 3005,
        Level = LogLevel.Error,
        Message = "Unhandled exception: {Path} | {Method} | {StatusCode}")]
    public static partial void LogUnhandledException(
        this ILogger logger, Exception exception, string path, string method, int statusCode);

    /// <summary>
    /// Enregistre un dépassement de quota.
    /// </summary>
    /// <param name="logger">Instance de logger.</param>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    /// <param name="reason">Raison du refus.</param>
    /// <remarks>
    /// EventId: 4001 | Level: Warning
    /// Message: "Quota exceeded for user {UserId}: {Reason}"
    /// </remarks>
    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Warning,
        Message = "Quota exceeded for user {UserId}: {Reason}")]
    public static partial void LogQuotaExceeded(
        this ILogger logger, Guid userId, string reason);

    /// <summary>
    /// Enregistre une erreur lors de la vérification de quota.
    /// </summary>
    /// <param name="logger">Instance de logger.</param>
    /// <param name="exception">Exception capturée.</param>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    /// <remarks>
    /// EventId: 4002 | Level: Error
    /// Message: "Error checking quota for user {UserId}"
    /// </remarks>
    [LoggerMessage(
        EventId = 4002,
        Level = LogLevel.Error,
        Message = "Error checking quota for user {UserId}")]
    public static partial void LogQuotaCheckError(
        this ILogger logger, Exception exception, Guid userId);

    /// <summary>
    /// Enregistre l'interception d'une requête streaming.
    /// </summary>
    /// <param name="logger">Instance de logger.</param>
    /// <param name="path">Chemin de la requête.</param>
    /// <remarks>
    /// EventId: 5001 | Level: Information
    /// Message: "Intercepting streaming request: {Path}"
    /// </remarks>
    [LoggerMessage(
        EventId = 5001,
        Level = LogLevel.Information,
        Message = "Intercepting streaming request: {Path}")]
    public static partial void LogStreamingRequestIntercepted(
        this ILogger logger, string path);

    /// <summary>
    /// Enregistre une erreur lors de la lecture du corps de la réponse streaming.
    /// </summary>
    /// <param name="logger">Instance de logger.</param>
    /// <param name="exception">Exception capturée.</param>
    /// <param name="contentLength">Longueur du contenu lu.</param>
    /// <remarks>
    /// EventId: 5002 | Level: Error
    /// Message: "Error reading streaming response body: {ContentLength} bytes read"
    /// </remarks>
    [LoggerMessage(
        EventId = 5002,
        Level = LogLevel.Error,
        Message = "Error reading streaming response body: {ContentLength} bytes read")]
    public static partial void LogStreamingReadError(
        this ILogger logger, Exception exception, long contentLength);

    /// <summary>
    /// Enregistre l'échec du parsing d'un chunk streaming.
    /// </summary>
    /// <param name="logger">Instance de logger.</param>
    /// <param name="exception">Exception capturée.</param>
    /// <param name="data">Données brutes du chunk.</param>
    /// <remarks>
    /// EventId: 5003 | Level: Warning
    /// Message: "Failed to parse streaming chunk: {Data}"
    /// </remarks>
    [LoggerMessage(
        EventId = 5003,
        Level = LogLevel.Warning,
        Message = "Failed to parse streaming chunk: {Data}")]
    public static partial void LogStreamingChunkParseError(
        this ILogger logger, Exception exception, string data);

    /// <summary>
    /// Enregistre une erreur lors du traitement d'un chunk streaming.
    /// </summary>
    /// <param name="logger">Instance de logger.</param>
    /// <param name="exception">Exception capturée.</param>
    /// <remarks>
    /// EventId: 5004 | Level: Error
    /// Message: "Error processing streaming chunk"
    /// </remarks>
    [LoggerMessage(
        EventId = 5004,
        Level = LogLevel.Error,
        Message = "Error processing streaming chunk")]
    public static partial void LogStreamingChunkProcessingError(
        this ILogger logger, Exception exception);

    /// <summary>
    /// Enregistre l'échec du comptage de tokens pour la réponse streaming.
    /// </summary>
    /// <param name="logger">Instance de logger.</param>
    /// <param name="exception">Exception capturée.</param>
    /// <remarks>
    /// EventId: 5005 | Level: Warning
    /// Message: "Failed to count tokens for streaming response"
    /// </remarks>
    [LoggerMessage(
        EventId = 5005,
        Level = LogLevel.Warning,
        Message = "Failed to count tokens for streaming response")]
    public static partial void LogStreamingTokenCountError(
        this ILogger logger, Exception exception);

    /// <summary>
    /// Enregistre la complétion de la réponse streaming.
    /// </summary>
    /// <param name="logger">Instance de logger.</param>
    /// <param name="requestId">Identifiant de la requête.</param>
    /// <param name="totalChunks">Nombre total de chunks traités.</param>
    /// <param name="totalTokens">Nombre total de tokens.</param>
    /// <remarks>
    /// EventId: 5006 | Level: Information
    /// Message: "Streaming response completed for request {RequestId}: {TotalChunks} chunks, {TotalTokens} tokens"
    /// </remarks>
    [LoggerMessage(
        EventId = 5006,
        Level = LogLevel.Information,
        Message = "Streaming response completed for request {RequestId}: {TotalChunks} chunks, {TotalTokens} tokens")]
    public static partial void LogStreamingCompleted(
        this ILogger logger, Guid requestId, int totalChunks, int totalTokens);

    /// <summary>
    /// Enregistre la sauvegarde réussie d'un audit log.
    /// </summary>
    /// <param name="logger">Instance de logger.</param>
    /// <param name="requestId">Identifiant de la requête.</param>
    /// <remarks>
    /// EventId: 5007 | Level: Information
    /// Message: "Audit log saved for request {RequestId}"
    /// </remarks>
    [LoggerMessage(
        EventId = 5007,
        Level = LogLevel.Information,
        Message = "Audit log saved for request {RequestId}")]
    public static partial void LogAuditLogSaved(
        this ILogger logger, Guid requestId);

    /// <summary>
    /// Enregistre l'échec de la création d'un audit log.
    /// </summary>
    /// <param name="logger">Instance de logger.</param>
    /// <param name="error">Message d'erreur.</param>
    /// <remarks>
    /// EventId: 5008 | Level: Error
    /// Message: "Failed to create audit log: {Error}"
    /// </remarks>
    [LoggerMessage(
        EventId = 5008,
        Level = LogLevel.Error,
        Message = "Failed to create audit log: {Error}")]
    public static partial void LogAuditLogCreationError(
        this ILogger logger, string error);

    /// <summary>
    /// Enregistre l'échec de la sauvegarde des métriques.
    /// </summary>
    /// <param name="logger">Instance de logger.</param>
    /// <param name="exception">Exception capturée.</param>
    /// <remarks>
    /// EventId: 5009 | Level: Error
    /// Message: "Failed to save metrics for streaming response"
    /// </remarks>
    [LoggerMessage(
        EventId = 5009,
        Level = LogLevel.Error,
        Message = "Failed to save metrics for streaming response")]
    public static partial void LogMetricsSaveError(
        this ILogger logger, Exception exception);
}
