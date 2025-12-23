using Authorization.Domain.ValueObjects;

namespace Authorization.Domain.Exceptions;

/// <summary>
/// Exception levée lorsqu'une assignation de rôle identique existe déjà.
/// </summary>
public sealed class DuplicateRoleAssignmentException : AuthorizationDomainException
{
    /// <inheritdoc />
    public override string ErrorCode => "DUPLICATE_ROLE_ASSIGNMENT";

    /// <summary>
    /// Principal concerné.
    /// </summary>
    public PrincipalId PrincipalId { get; }

    /// <summary>
    /// Rôle concerné.
    /// </summary>
    public RoleId RoleId { get; }

    /// <summary>
    /// Scope de l'assignation.
    /// </summary>
    public Scope Scope { get; }

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="principalId">Identifiant du principal.</param>
    /// <param name="roleId">Identifiant du rôle.</param>
    /// <param name="scope">Scope de l'assignation.</param>
    public DuplicateRoleAssignmentException(PrincipalId principalId, RoleId roleId, Scope scope)
        : base($"Une assignation active du rôle '{roleId}' au principal '{principalId}' sur le scope '{scope}' existe déjà.")
    {
        PrincipalId = principalId;
        RoleId = roleId;
        Scope = scope;
    }
}

/// <summary>
/// Exception levée lorsqu'un rôle avec ce nom existe déjà.
/// </summary>
public sealed class DuplicateRoleNameException : AuthorizationDomainException
{
    /// <inheritdoc />
    public override string ErrorCode => "DUPLICATE_ROLE_NAME";

    /// <summary>
    /// Nom du rôle en doublon.
    /// </summary>
    public string RoleName { get; }

    /// <summary>
    /// Tenant concerné.
    /// </summary>
    public TenantId TenantId { get; }

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="roleName">Nom du rôle.</param>
    /// <param name="tenantId">Identifiant du tenant.</param>
    public DuplicateRoleNameException(string roleName, TenantId tenantId)
        : base($"Un rôle nommé '{roleName}' existe déjà dans le tenant '{tenantId}'.")
    {
        RoleName = roleName;
        TenantId = tenantId;
    }
}
