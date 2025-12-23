using Authorization.Domain.Abstractions;
using Authorization.Domain.ValueObjects;

namespace Authorization.Domain.Events;

/// <summary>
/// Événement émis lors d'une vérification de permission.
/// </summary>
/// <param name="PrincipalId">Principal effectuant la vérification.</param>
/// <param name="Permission">Permission vérifiée.</param>
/// <param name="Scope">Scope de la vérification.</param>
/// <param name="TenantId">Tenant.</param>
/// <param name="IsAllowed">Résultat de la vérification.</param>
/// <param name="Duration">Durée de la vérification.</param>
/// <param name="CacheHit">Indique si le résultat provient du cache.</param>
public sealed record PermissionCheckedEvent(
    PrincipalId PrincipalId,
    Permission Permission,
    Scope Scope,
    TenantId TenantId,
    bool IsAllowed,
    TimeSpan Duration,
    bool CacheHit) : IDomainEvent;
