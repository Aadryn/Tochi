using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;

namespace LLMProxy.Domain.LLM.Events;

/// <summary>
/// Événement déclenché quand une requête LLM est démarrée.
/// </summary>
public sealed record LLMRequestStartedEvent : DomainEvent
{
    /// <summary>
    /// Identifiant unique de la requête.
    /// </summary>
    public required string RequestId { get; init; }

    /// <summary>
    /// Type du provider ciblé.
    /// </summary>
    public required ProviderType ProviderType { get; init; }

    /// <summary>
    /// Nom du provider.
    /// </summary>
    public required string ProviderName { get; init; }

    /// <summary>
    /// Modèle utilisé.
    /// </summary>
    public required ModelIdentifier Model { get; init; }

    /// <summary>
    /// Identifiant du tenant.
    /// </summary>
    public Guid? TenantId { get; init; }

    /// <summary>
    /// Identifiant utilisateur.
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Indique si la requête est en streaming.
    /// </summary>
    public bool IsStreaming { get; init; }

    /// <summary>
    /// Nombre de messages dans la requête.
    /// </summary>
    public int MessageCount { get; init; }
}

/// <summary>
/// Événement déclenché quand une requête LLM est terminée avec succès.
/// </summary>
public sealed record LLMRequestCompletedEvent : DomainEvent
{
    /// <summary>
    /// Identifiant unique de la requête.
    /// </summary>
    public required string RequestId { get; init; }

    /// <summary>
    /// Type du provider.
    /// </summary>
    public required ProviderType ProviderType { get; init; }

    /// <summary>
    /// Nom du provider.
    /// </summary>
    public required string ProviderName { get; init; }

    /// <summary>
    /// Modèle utilisé.
    /// </summary>
    public required ModelIdentifier Model { get; init; }

    /// <summary>
    /// Utilisation des tokens.
    /// </summary>
    public required TokenUsage TokenUsage { get; init; }

    /// <summary>
    /// Durée de la requête.
    /// </summary>
    public required TimeSpan Duration { get; init; }

    /// <summary>
    /// Raison de fin de génération.
    /// </summary>
    public FinishReason FinishReason { get; init; }

    /// <summary>
    /// Identifiant du tenant.
    /// </summary>
    public Guid? TenantId { get; init; }
}

/// <summary>
/// Événement déclenché quand une requête LLM échoue.
/// </summary>
public sealed record LLMRequestFailedEvent : DomainEvent
{
    /// <summary>
    /// Identifiant unique de la requête.
    /// </summary>
    public required string RequestId { get; init; }

    /// <summary>
    /// Type du provider.
    /// </summary>
    public required ProviderType ProviderType { get; init; }

    /// <summary>
    /// Nom du provider.
    /// </summary>
    public required string ProviderName { get; init; }

    /// <summary>
    /// Modèle ciblé.
    /// </summary>
    public required ModelIdentifier Model { get; init; }

    /// <summary>
    /// Type de l'erreur.
    /// </summary>
    public required string ErrorType { get; init; }

    /// <summary>
    /// Message d'erreur.
    /// </summary>
    public required string ErrorMessage { get; init; }

    /// <summary>
    /// Code d'erreur HTTP (si applicable).
    /// </summary>
    public int? HttpStatusCode { get; init; }

    /// <summary>
    /// Durée avant l'échec.
    /// </summary>
    public TimeSpan? Duration { get; init; }

    /// <summary>
    /// Identifiant du tenant.
    /// </summary>
    public Guid? TenantId { get; init; }

    /// <summary>
    /// Indique si un failover a été tenté.
    /// </summary>
    public bool FailoverAttempted { get; init; }
}
