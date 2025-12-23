using Authorization.Domain.ValueObjects;

namespace Authorization.Domain.Exceptions;

/// <summary>
/// Exception levée lorsqu'un principal n'est pas trouvé.
/// </summary>
public sealed class PrincipalNotFoundException : AuthorizationDomainException
{
    /// <inheritdoc />
    public override string ErrorCode => "PRINCIPAL_NOT_FOUND";

    /// <summary>
    /// Identifiant du principal recherché.
    /// </summary>
    public PrincipalId PrincipalId { get; }

    /// <summary>
    /// Tenant dans lequel la recherche a été effectuée.
    /// </summary>
    public TenantId TenantId { get; }

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="principalId">Identifiant du principal.</param>
    /// <param name="tenantId">Identifiant du tenant.</param>
    public PrincipalNotFoundException(PrincipalId principalId, TenantId tenantId)
        : base($"Le principal '{principalId}' n'existe pas dans le tenant '{tenantId}'.")
    {
        PrincipalId = principalId;
        TenantId = tenantId;
    }
}

/// <summary>
/// Exception levée lorsqu'un rôle n'est pas trouvé.
/// </summary>
public sealed class RoleNotFoundException : AuthorizationDomainException
{
    /// <inheritdoc />
    public override string ErrorCode => "ROLE_NOT_FOUND";

    /// <summary>
    /// Identifiant du rôle recherché.
    /// </summary>
    public RoleId RoleId { get; }

    /// <summary>
    /// Tenant dans lequel la recherche a été effectuée.
    /// </summary>
    public TenantId TenantId { get; }

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="roleId">Identifiant du rôle.</param>
    /// <param name="tenantId">Identifiant du tenant.</param>
    public RoleNotFoundException(RoleId roleId, TenantId tenantId)
        : base($"Le rôle '{roleId}' n'existe pas dans le tenant '{tenantId}'.")
    {
        RoleId = roleId;
        TenantId = tenantId;
    }
}

/// <summary>
/// Exception levée lorsqu'une assignation de rôle n'est pas trouvée.
/// </summary>
public sealed class RoleAssignmentNotFoundException : AuthorizationDomainException
{
    /// <inheritdoc />
    public override string ErrorCode => "ROLE_ASSIGNMENT_NOT_FOUND";

    /// <summary>
    /// Identifiant de l'assignation recherchée.
    /// </summary>
    public RoleAssignmentId AssignmentId { get; }

    /// <summary>
    /// Tenant dans lequel la recherche a été effectuée.
    /// </summary>
    public TenantId TenantId { get; }

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="assignmentId">Identifiant de l'assignation.</param>
    /// <param name="tenantId">Identifiant du tenant.</param>
    public RoleAssignmentNotFoundException(RoleAssignmentId assignmentId, TenantId tenantId)
        : base($"L'assignation de rôle '{assignmentId}' n'existe pas dans le tenant '{tenantId}'.")
    {
        AssignmentId = assignmentId;
        TenantId = tenantId;
    }
}

/// <summary>
/// Exception levée lorsqu'un tenant n'est pas trouvé.
/// </summary>
public sealed class TenantNotFoundException : AuthorizationDomainException
{
    /// <inheritdoc />
    public override string ErrorCode => "TENANT_NOT_FOUND";

    /// <summary>
    /// Identifiant du tenant recherché.
    /// </summary>
    public TenantId TenantId { get; }

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    public TenantNotFoundException(TenantId tenantId)
        : base($"Le tenant '{tenantId}' n'existe pas.")
    {
        TenantId = tenantId;
    }
}
