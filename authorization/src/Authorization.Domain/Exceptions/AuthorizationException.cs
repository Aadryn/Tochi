using Authorization.Domain.ValueObjects;

namespace Authorization.Domain.Exceptions;

/// <summary>
/// Exception levée lorsqu'un utilisateur tente une action non autorisée.
/// </summary>
public sealed class UnauthorizedAccessException : AuthorizationDomainException
{
    /// <inheritdoc />
    public override string ErrorCode => "UNAUTHORIZED_ACCESS";

    /// <summary>
    /// Principal ayant tenté l'action.
    /// </summary>
    public PrincipalId PrincipalId { get; }

    /// <summary>
    /// Permission requise.
    /// </summary>
    public Permission Permission { get; }

    /// <summary>
    /// Scope sur lequel l'action était tentée.
    /// </summary>
    public Scope Scope { get; }

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="principalId">Identifiant du principal.</param>
    /// <param name="permission">Permission manquante.</param>
    /// <param name="scope">Scope concerné.</param>
    public UnauthorizedAccessException(PrincipalId principalId, Permission permission, Scope scope)
        : base($"Le principal '{principalId}' n'a pas la permission '{permission}' sur le scope '{scope}'.")
    {
        PrincipalId = principalId;
        Permission = permission;
        Scope = scope;
    }
}

/// <summary>
/// Exception levée lorsqu'une délégation de rôle est interdite.
/// </summary>
public sealed class DelegationNotAllowedException : AuthorizationDomainException
{
    /// <inheritdoc />
    public override string ErrorCode => "DELEGATION_NOT_ALLOWED";

    /// <summary>
    /// Principal tentant la délégation.
    /// </summary>
    public PrincipalId DelegatorId { get; }

    /// <summary>
    /// Rôle du délégateur.
    /// </summary>
    public RoleId DelegatorRole { get; }

    /// <summary>
    /// Rôle qu'il tentait de déléguer.
    /// </summary>
    public RoleId TargetRole { get; }

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="delegatorId">Identifiant du délégateur.</param>
    /// <param name="delegatorRole">Rôle du délégateur.</param>
    /// <param name="targetRole">Rôle cible de la délégation.</param>
    public DelegationNotAllowedException(
        PrincipalId delegatorId,
        RoleId delegatorRole,
        RoleId targetRole)
        : base($"Le principal '{delegatorId}' avec le rôle '{delegatorRole}' ne peut pas déléguer le rôle '{targetRole}'. " +
               "La délégation hiérarchique ne permet de déléguer qu'un rôle de niveau inférieur ou égal.")
    {
        DelegatorId = delegatorId;
        DelegatorRole = delegatorRole;
        TargetRole = targetRole;
    }
}

/// <summary>
/// Exception levée lorsqu'une modification d'un rôle de base est tentée.
/// </summary>
public sealed class BuiltInRoleModificationException : AuthorizationDomainException
{
    /// <inheritdoc />
    public override string ErrorCode => "BUILTIN_ROLE_MODIFICATION";

    /// <summary>
    /// Rôle de base qu'on tentait de modifier.
    /// </summary>
    public RoleId RoleId { get; }

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="roleId">Identifiant du rôle.</param>
    public BuiltInRoleModificationException(RoleId roleId)
        : base($"Le rôle de base '{roleId}' ne peut pas être modifié.")
    {
        RoleId = roleId;
    }
}
