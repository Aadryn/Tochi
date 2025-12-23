using Authorization.Domain.Abstractions;
using Authorization.Domain.ValueObjects;

namespace Authorization.Domain.Events;

/// <summary>
/// Actions possibles lors de la synchronisation d'un principal.
/// </summary>
public enum PrincipalSyncAction
{
    /// <summary>Principal créé pour la première fois.</summary>
    Created,

    /// <summary>Principal mis à jour.</summary>
    Updated,

    /// <summary>Principal désactivé.</summary>
    Deactivated,

    /// <summary>Principal réactivé.</summary>
    Reactivated,

    /// <summary>Principal supprimé de l'IDP.</summary>
    Deleted
}

/// <summary>
/// Événement émis lors de la synchronisation d'un principal depuis l'IDP.
/// </summary>
/// <param name="PrincipalId">Identifiant du principal.</param>
/// <param name="PrincipalType">Type de principal.</param>
/// <param name="TenantId">Tenant du principal.</param>
/// <param name="Action">Action de synchronisation.</param>
public sealed record PrincipalSyncedEvent(
    PrincipalId PrincipalId,
    PrincipalType PrincipalType,
    TenantId TenantId,
    PrincipalSyncAction Action) : IDomainEvent;
