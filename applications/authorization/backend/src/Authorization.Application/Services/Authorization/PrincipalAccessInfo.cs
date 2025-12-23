using Authorization.Domain.ValueObjects;

namespace Authorization.Application.Services.Authorization;

/// <summary>
/// Informations sur l'accès d'un principal.
/// </summary>
public record PrincipalAccessInfo(
    PrincipalId PrincipalId,
    PrincipalType PrincipalType,
    string DisplayName,
    RoleId RoleId,
    string RoleName,
    Scope Scope);
