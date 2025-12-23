using Authorization.Domain.ValueObjects;

namespace Authorization.Application.Services.Authorization;

/// <summary>
/// Informations sur une assignation de rôle.
/// </summary>
public record RoleAssignmentInfo(
    RoleAssignmentId Id,
    RoleId RoleId,
    string RoleName,
    Scope Scope,
    DateTime AssignedAt,
    PrincipalId? AssignedBy,
    DateTime? ExpiresAt);
