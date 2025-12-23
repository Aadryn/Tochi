using Authorization.Domain.ValueObjects;

namespace Authorization.Infrastructure.PostgreSQL.Entities;

/// <summary>
/// Entité d'audit pour tracer toutes les opérations d'autorisation.
/// Stockée en PostgreSQL pour requêtes complexes et rétention longue durée.
/// </summary>
public class AuditLog
{
    /// <summary>
    /// Identifiant unique de l'entrée d'audit.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identifiant du tenant.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Type d'opération effectuée.
    /// </summary>
    public AuditOperationType OperationType { get; set; }

    /// <summary>
    /// Type de ressource concernée (permission, role, assignment, etc.).
    /// </summary>
    public string ResourceType { get; set; } = string.Empty;

    /// <summary>
    /// Identifiant de la ressource concernée.
    /// </summary>
    public string ResourceId { get; set; } = string.Empty;

    /// <summary>
    /// Identifiant du principal qui effectue l'opération (acteur).
    /// </summary>
    public string ActorId { get; set; } = string.Empty;

    /// <summary>
    /// Type du principal acteur.
    /// </summary>
    public string ActorType { get; set; } = string.Empty;

    /// <summary>
    /// Identifiant du principal cible (pour les opérations sur les assignations).
    /// </summary>
    public string? TargetPrincipalId { get; set; }

    /// <summary>
    /// Type du principal cible.
    /// </summary>
    public string? TargetPrincipalType { get; set; }

    /// <summary>
    /// Scope concerné par l'opération.
    /// </summary>
    public string? Scope { get; set; }

    /// <summary>
    /// Permission vérifiée (pour les opérations de check).
    /// </summary>
    public string? Permission { get; set; }

    /// <summary>
    /// Rôle concerné (pour les opérations d'assignation).
    /// </summary>
    public string? RoleId { get; set; }

    /// <summary>
    /// Résultat de l'opération.
    /// </summary>
    public AuditResult Result { get; set; }

    /// <summary>
    /// Message d'erreur si l'opération a échoué.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Durée de l'opération en millisecondes.
    /// </summary>
    public double DurationMs { get; set; }

    /// <summary>
    /// Indique si le résultat provenait du cache.
    /// </summary>
    public bool CacheHit { get; set; }

    /// <summary>
    /// Adresse IP de l'appelant.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User-Agent de l'appelant.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Identifiant de corrélation pour le suivi des requêtes.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Données supplémentaires au format JSON.
    /// </summary>
    public string? AdditionalData { get; set; }

    /// <summary>
    /// Date et heure de l'opération (UTC).
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Crée une entrée d'audit pour une vérification de permission.
    /// </summary>
    public static AuditLog CreatePermissionCheck(
        TenantId tenantId,
        PrincipalId actorId,
        PrincipalType actorType,
        Permission permission,
        Scope scope,
        bool allowed,
        double durationMs,
        bool cacheHit,
        string? correlationId = null)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            OperationType = AuditOperationType.PermissionCheck,
            ResourceType = "permission",
            ResourceId = permission.ToString(),
            ActorId = actorId.Value.ToString(),
            ActorType = actorType.ToString(),
            Scope = scope.Path,
            Permission = permission.ToString(),
            Result = allowed ? AuditResult.Success : AuditResult.Denied,
            DurationMs = durationMs,
            CacheHit = cacheHit,
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Crée une entrée d'audit pour une assignation de rôle.
    /// </summary>
    public static AuditLog CreateRoleAssignment(
        TenantId tenantId,
        PrincipalId actorId,
        PrincipalType actorType,
        PrincipalId targetPrincipalId,
        PrincipalType targetPrincipalType,
        RoleId roleId,
        Scope scope,
        AuditResult result,
        string? errorMessage = null,
        string? correlationId = null)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            OperationType = AuditOperationType.RoleAssigned,
            ResourceType = "role_assignment",
            ResourceId = Guid.NewGuid().ToString(),
            ActorId = actorId.Value.ToString(),
            ActorType = actorType.ToString(),
            TargetPrincipalId = targetPrincipalId.Value.ToString(),
            TargetPrincipalType = targetPrincipalType.ToString(),
            Scope = scope.Path,
            RoleId = roleId.Value,
            Result = result,
            ErrorMessage = errorMessage,
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Crée une entrée d'audit pour une révocation de rôle.
    /// </summary>
    public static AuditLog CreateRoleRevocation(
        TenantId tenantId,
        PrincipalId actorId,
        PrincipalType actorType,
        PrincipalId targetPrincipalId,
        PrincipalType targetPrincipalType,
        RoleId roleId,
        Scope scope,
        string assignmentId,
        AuditResult result,
        string? errorMessage = null,
        string? correlationId = null)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            OperationType = AuditOperationType.RoleRevoked,
            ResourceType = "role_assignment",
            ResourceId = assignmentId,
            ActorId = actorId.Value.ToString(),
            ActorType = actorType.ToString(),
            TargetPrincipalId = targetPrincipalId.Value.ToString(),
            TargetPrincipalType = targetPrincipalType.ToString(),
            Scope = scope.Path,
            RoleId = roleId.Value,
            Result = result,
            ErrorMessage = errorMessage,
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Types d'opérations d'audit.
/// </summary>
public enum AuditOperationType
{
    /// <summary>
    /// Vérification de permission.
    /// </summary>
    PermissionCheck,

    /// <summary>
    /// Assignation de rôle.
    /// </summary>
    RoleAssigned,

    /// <summary>
    /// Révocation de rôle.
    /// </summary>
    RoleRevoked,

    /// <summary>
    /// Création de rôle personnalisé.
    /// </summary>
    RoleCreated,

    /// <summary>
    /// Modification de rôle personnalisé.
    /// </summary>
    RoleUpdated,

    /// <summary>
    /// Suppression de rôle personnalisé.
    /// </summary>
    RoleDeleted,

    /// <summary>
    /// Synchronisation de principal depuis l'IDP.
    /// </summary>
    PrincipalSynced,

    /// <summary>
    /// Création de scope.
    /// </summary>
    ScopeCreated,

    /// <summary>
    /// Suppression de scope.
    /// </summary>
    ScopeDeleted
}

/// <summary>
/// Résultats possibles d'une opération d'audit.
/// </summary>
public enum AuditResult
{
    /// <summary>
    /// Opération réussie.
    /// </summary>
    Success,

    /// <summary>
    /// Permission refusée.
    /// </summary>
    Denied,

    /// <summary>
    /// Échec technique.
    /// </summary>
    Error,

    /// <summary>
    /// Ressource non trouvée.
    /// </summary>
    NotFound,

    /// <summary>
    /// Validation échouée.
    /// </summary>
    ValidationFailed
}
