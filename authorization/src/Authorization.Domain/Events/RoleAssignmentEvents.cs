using Authorization.Domain.Abstractions;
using Authorization.Domain.ValueObjects;

namespace Authorization.Domain.Events;

/// <summary>
/// Événement émis lors de la création d'une assignation de rôle.
/// </summary>
/// <param name="AssignmentId">Identifiant de l'assignation.</param>
/// <param name="PrincipalId">Principal recevant le rôle.</param>
/// <param name="PrincipalType">Type de principal.</param>
/// <param name="RoleId">Rôle assigné.</param>
/// <param name="Scope">Scope de l'assignation.</param>
/// <param name="TenantId">Tenant.</param>
/// <param name="ExpiresAt">Date d'expiration optionnelle.</param>
public sealed record RoleAssignmentCreatedEvent(
    RoleAssignmentId AssignmentId,
    PrincipalId PrincipalId,
    PrincipalType PrincipalType,
    RoleId RoleId,
    Scope Scope,
    TenantId TenantId,
    DateTimeOffset? ExpiresAt) : IDomainEvent;

/// <summary>
/// Événement émis lors de la révocation d'une assignation de rôle.
/// </summary>
/// <param name="AssignmentId">Identifiant de l'assignation.</param>
/// <param name="PrincipalId">Principal concerné.</param>
/// <param name="RoleId">Rôle révoqué.</param>
/// <param name="Scope">Scope de l'assignation.</param>
/// <param name="TenantId">Tenant.</param>
/// <param name="RevokedBy">Principal ayant révoqué.</param>
/// <param name="Reason">Raison de la révocation.</param>
public sealed record RoleAssignmentRevokedEvent(
    RoleAssignmentId AssignmentId,
    PrincipalId PrincipalId,
    RoleId RoleId,
    Scope Scope,
    TenantId TenantId,
    PrincipalId RevokedBy,
    string? Reason) : IDomainEvent;

/// <summary>
/// Événement émis lors de la mise à jour de la date d'expiration.
/// </summary>
/// <param name="AssignmentId">Identifiant de l'assignation.</param>
/// <param name="TenantId">Tenant.</param>
/// <param name="OldExpiresAt">Ancienne date d'expiration.</param>
/// <param name="NewExpiresAt">Nouvelle date d'expiration.</param>
public sealed record RoleAssignmentExpirationUpdatedEvent(
    RoleAssignmentId AssignmentId,
    TenantId TenantId,
    DateTimeOffset? OldExpiresAt,
    DateTimeOffset? NewExpiresAt) : IDomainEvent;

/// <summary>
/// Événement émis lorsqu'une assignation de rôle expire automatiquement.
/// </summary>
/// <param name="AssignmentId">Identifiant de l'assignation.</param>
/// <param name="PrincipalId">Principal concerné.</param>
/// <param name="RoleId">Rôle expiré.</param>
/// <param name="Scope">Scope de l'assignation.</param>
/// <param name="TenantId">Tenant.</param>
public sealed record RoleAssignmentExpiredEvent(
    RoleAssignmentId AssignmentId,
    PrincipalId PrincipalId,
    RoleId RoleId,
    Scope Scope,
    TenantId TenantId) : IDomainEvent;
