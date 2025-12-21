using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.Security;

/// <summary>
/// Extensions de logging haute performance pour le module Security.
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
/// <item>1xxx : Security/Authentication</item>
/// </list>
/// 
/// <para><strong>Exemple</strong> :</para>
/// <code>
/// // Avant (interpolation)
/// _logger.LogWarning("Revoked API key used: {KeyId}", apiKey.Id);
/// 
/// // Après (LoggerMessage)
/// _logger.LogRevokedKeyUsage(apiKey.Id);
/// </code>
/// 
/// Conforme à : ADR-031 (Structured Logging - performance optimized)
/// </remarks>
public static partial class SecurityLoggerExtensions
{
    /// <summary>
    /// Enregistre l'utilisation d'une clé API révoquée.
    /// </summary>
    /// <param name="logger">Instance de logger.</param>
    /// <param name="keyId">Identifiant de la clé révoquée.</param>
    /// <remarks>
    /// EventId: 1001 | Level: Warning
    /// Message: "Revoked API key used: {KeyId}"
    /// </remarks>
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Warning,
        Message = "Revoked API key used: {KeyId}")]
    public static partial void LogRevokedKeyUsage(
        this ILogger logger, Guid keyId);

    /// <summary>
    /// Enregistre l'utilisation d'une clé API expirée.
    /// </summary>
    /// <param name="logger">Instance de logger.</param>
    /// <param name="keyId">Identifiant de la clé expirée.</param>
    /// <remarks>
    /// EventId: 1002 | Level: Warning
    /// Message: "Expired API key used: {KeyId}"
    /// </remarks>
    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Warning,
        Message = "Expired API key used: {KeyId}")]
    public static partial void LogExpiredKeyUsage(
        this ILogger logger, Guid keyId);

    /// <summary>
    /// Enregistre une tentative d'utilisation de clé API par un utilisateur inactif.
    /// </summary>
    /// <param name="logger">Instance de logger.</param>
    /// <param name="userId">Identifiant de l'utilisateur inactif.</param>
    /// <remarks>
    /// EventId: 1003 | Level: Warning
    /// Message: "Inactive user for API key: {UserId}"
    /// </remarks>
    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Warning,
        Message = "Inactive user for API key: {UserId}")]
    public static partial void LogInactiveUser(
        this ILogger logger, Guid userId);

    /// <summary>
    /// Enregistre une requête sans clé API.
    /// </summary>
    /// <param name="logger">Instance de logger.</param>
    /// <param name="path">Chemin de la requête HTTP.</param>
    /// <remarks>
    /// EventId: 1004 | Level: Warning
    /// Message: "Request without API key: {Path}"
    /// </remarks>
    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Warning,
        Message = "Request without API key: {Path}")]
    public static partial void LogRequestWithoutApiKey(
        this ILogger logger, string path);
}
