using Authorization.Domain.Abstractions;
using Authorization.Domain.Events;
using Authorization.Domain.ValueObjects;

namespace Authorization.Domain.Entities;

/// <summary>
/// Représente une assignation de rôle à un principal sur un scope spécifique.
/// </summary>
/// <remarks>
/// <para>
/// Une RoleAssignment lie un principal (utilisateur, groupe ou service account) à un rôle
/// sur un scope spécifique (ex: une organisation, un tenant).
/// </para>
/// <para>
/// Caractéristiques principales :
/// - Multi-rôles par scope : Un principal peut avoir plusieurs rôles sur le même scope
/// - Expiration optionnelle : Les assignations peuvent avoir une date d'expiration
/// - Délégation : Les assignations indiquent qui a effectué la délégation
/// - Audit : Toutes les modifications génèrent des événements de domaine
/// </para>
/// </remarks>
public sealed class RoleAssignment : Entity<RoleAssignmentId>
{
    /// <summary>
    /// Identifiant du principal qui reçoit le rôle.
    /// </summary>
    public PrincipalId PrincipalId { get; private set; }

    /// <summary>
    /// Type du principal.
    /// </summary>
    public PrincipalType PrincipalType { get; private set; }

    /// <summary>
    /// Identifiant du rôle assigné.
    /// </summary>
    public RoleId RoleId { get; private set; }

    /// <summary>
    /// Scope sur lequel le rôle est assigné.
    /// </summary>
    public Scope Scope { get; private set; }

    /// <summary>
    /// Tenant dans lequel cette assignation existe.
    /// </summary>
    public TenantId TenantId { get; private set; }

    /// <summary>
    /// Principal ayant créé cette assignation (pour la traçabilité).
    /// </summary>
    public PrincipalId? AssignedBy { get; private set; }

    /// <summary>
    /// Date d'expiration de l'assignation (null = pas d'expiration).
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; private set; }

    /// <summary>
    /// Date de création de l'assignation.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Indique si l'assignation a été révoquée.
    /// </summary>
    public bool IsRevoked { get; private set; }

    /// <summary>
    /// Date de révocation (si révoquée).
    /// </summary>
    public DateTimeOffset? RevokedAt { get; private set; }

    /// <summary>
    /// Principal ayant révoqué l'assignation.
    /// </summary>
    public PrincipalId? RevokedBy { get; private set; }

    /// <summary>
    /// Raison de la révocation.
    /// </summary>
    public string? RevocationReason { get; private set; }

    /// <summary>
    /// Condition optionnelle pour l'assignation (ex: restriction horaire).
    /// </summary>
    public string? Condition { get; private set; }

    /// <summary>
    /// Indique si l'assignation est actuellement active.
    /// </summary>
    public bool IsActive =>
        !IsRevoked &&
        (ExpiresAt is null || ExpiresAt > DateTimeOffset.UtcNow);

    private RoleAssignment()
    {
        // Constructeur pour EF Core
        Scope = null!;
    }

    private RoleAssignment(
        RoleAssignmentId id,
        PrincipalId principalId,
        PrincipalType principalType,
        RoleId roleId,
        Scope scope,
        TenantId tenantId,
        PrincipalId? assignedBy,
        DateTimeOffset? expiresAt,
        string? condition)
    {
        Id = id;
        PrincipalId = principalId;
        PrincipalType = principalType;
        RoleId = roleId;
        Scope = scope ?? throw new ArgumentNullException(nameof(scope));
        TenantId = tenantId;
        AssignedBy = assignedBy;
        ExpiresAt = expiresAt;
        Condition = condition;
        CreatedAt = DateTimeOffset.UtcNow;
        IsRevoked = false;
    }

    /// <summary>
    /// Crée une nouvelle assignation de rôle.
    /// </summary>
    /// <param name="principalId">Identifiant du principal.</param>
    /// <param name="principalType">Type de principal.</param>
    /// <param name="roleId">Identifiant du rôle à assigner.</param>
    /// <param name="scope">Scope de l'assignation.</param>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="assignedBy">Principal effectuant l'assignation.</param>
    /// <param name="expiresAt">Date d'expiration optionnelle.</param>
    /// <param name="condition">Condition optionnelle.</param>
    /// <returns>Nouvelle assignation de rôle.</returns>
    public static RoleAssignment Create(
        PrincipalId principalId,
        PrincipalType principalType,
        RoleId roleId,
        Scope scope,
        TenantId tenantId,
        PrincipalId? assignedBy = null,
        DateTimeOffset? expiresAt = null,
        string? condition = null)
    {
        var assignment = new RoleAssignment(
            RoleAssignmentId.New(),
            principalId,
            principalType,
            roleId,
            scope,
            tenantId,
            assignedBy,
            expiresAt,
            condition);

        assignment.AddDomainEvent(new RoleAssignmentCreatedEvent(
            assignment.Id,
            assignment.PrincipalId,
            assignment.PrincipalType,
            assignment.RoleId,
            assignment.Scope,
            assignment.TenantId,
            assignment.ExpiresAt));

        return assignment;
    }

    /// <summary>
    /// Révoque l'assignation de rôle.
    /// </summary>
    /// <param name="revokedBy">Principal effectuant la révocation.</param>
    /// <param name="reason">Raison de la révocation.</param>
    /// <exception cref="InvalidOperationException">Si l'assignation est déjà révoquée.</exception>
    public void Revoke(PrincipalId revokedBy, string? reason = null)
    {
        if (IsRevoked)
        {
            throw new InvalidOperationException("Cette assignation de rôle a déjà été révoquée.");
        }

        IsRevoked = true;
        RevokedAt = DateTimeOffset.UtcNow;
        RevokedBy = revokedBy;
        RevocationReason = reason;

        AddDomainEvent(new RoleAssignmentRevokedEvent(
            Id,
            PrincipalId,
            RoleId,
            Scope,
            TenantId,
            revokedBy,
            reason));
    }

    /// <summary>
    /// Met à jour la date d'expiration de l'assignation.
    /// </summary>
    /// <param name="newExpiresAt">Nouvelle date d'expiration (null pour supprimer l'expiration).</param>
    /// <exception cref="InvalidOperationException">Si l'assignation est révoquée.</exception>
    public void UpdateExpiration(DateTimeOffset? newExpiresAt)
    {
        if (IsRevoked)
        {
            throw new InvalidOperationException("Impossible de modifier une assignation révoquée.");
        }

        var oldExpiresAt = ExpiresAt;
        ExpiresAt = newExpiresAt;

        AddDomainEvent(new RoleAssignmentExpirationUpdatedEvent(
            Id, TenantId, oldExpiresAt, newExpiresAt));
    }

    /// <summary>
    /// Marque l'assignation comme expirée (appelée par le job de nettoyage).
    /// </summary>
    public void MarkAsExpired()
    {
        if (IsRevoked)
        {
            return;
        }

        IsRevoked = true;
        RevokedAt = DateTimeOffset.UtcNow;
        RevocationReason = "Expiration automatique";

        AddDomainEvent(new RoleAssignmentExpiredEvent(
            Id, PrincipalId, RoleId, Scope, TenantId));
    }

    /// <summary>
    /// Retourne la représentation OpenFGA de l'assignation.
    /// </summary>
    /// <returns>Tuple OpenFGA au format standard.</returns>
    public (string User, string Relation, string Object) ToOpenFgaTuple()
    {
        var user = PrincipalId.ToOpenFgaFormat(PrincipalType);
        var relation = RoleId.Value;
        var @object = Scope.ToOpenFgaFormat();

        return (user, relation, @object);
    }
}
