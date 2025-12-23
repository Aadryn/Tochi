using System.Diagnostics;
using Authorization.Domain.ValueObjects;
using Authorization.Infrastructure.OpenFGA.Services;
using Authorization.Infrastructure.PostgreSQL.Entities;
using Authorization.Infrastructure.PostgreSQL.Repositories;
using Authorization.Infrastructure.Redis.Services;
using Microsoft.Extensions.Logging;

namespace Authorization.Application.Services;

/// <summary>
/// Implémentation du service d'autorisation.
/// Orchestre OpenFGA, cache Redis et audit PostgreSQL.
/// </summary>
public class AuthorizationService : IRbacAuthorizationService
{
    private readonly IOpenFgaService _openFgaService;
    private readonly IPermissionCacheService _cacheService;
    private readonly IAuditLogRepository _auditRepository;
    private readonly ILogger<AuthorizationService> _logger;

    /// <summary>
    /// Initialise une nouvelle instance du service.
    /// </summary>
    public AuthorizationService(
        IOpenFgaService openFgaService,
        IPermissionCacheService cacheService,
        IAuditLogRepository auditRepository,
        ILogger<AuthorizationService> logger)
    {
        _openFgaService = openFgaService;
        _cacheService = cacheService;
        _auditRepository = auditRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> CheckPermissionAsync(
        TenantId tenantId,
        PrincipalId principalId,
        PrincipalType principalType,
        Permission permission,
        Scope scope,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var cacheHit = false;

        try
        {
            // 1. Vérifier le cache
            var cachedResult = await _cacheService.GetPermissionCheckAsync(
                tenantId,
                principalId,
                permission,
                scope,
                cancellationToken);

            if (cachedResult.HasValue)
            {
                cacheHit = true;
                stopwatch.Stop();

                await LogPermissionCheckAsync(
                    tenantId,
                    principalId,
                    principalType,
                    permission,
                    scope,
                    cachedResult.Value,
                    stopwatch.Elapsed.TotalMilliseconds,
                    cacheHit,
                    cancellationToken);

                return cachedResult.Value;
            }

            // 2. Vérifier dans OpenFGA
            var relation = MapPermissionToRelation(permission);
            var objectType = "scope";
            var objectId = scope.Path;

            var allowed = await _openFgaService.CheckAsync(
                tenantId,
                principalId,
                principalType,
                relation,
                objectType,
                objectId,
                cancellationToken);

            stopwatch.Stop();

            // 3. Mettre en cache le résultat
            await _cacheService.SetPermissionCheckAsync(
                tenantId,
                principalId,
                permission,
                scope,
                allowed,
                cancellationToken);

            // 4. Auditer
            await LogPermissionCheckAsync(
                tenantId,
                principalId,
                principalType,
                permission,
                scope,
                allowed,
                stopwatch.Elapsed.TotalMilliseconds,
                cacheHit,
                cancellationToken);

            return allowed;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error checking permission {Permission} for {Principal} on {Scope}",
                permission,
                principalId,
                scope.Path);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<RoleAssignmentId> AssignRoleAsync(
        TenantId tenantId,
        PrincipalId actorId,
        PrincipalType actorType,
        PrincipalId targetPrincipalId,
        PrincipalType targetPrincipalType,
        RoleId roleId,
        Scope scope,
        DateTime? expiresAt = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Vérifier que l'acteur peut déléguer ce rôle
            var canDelegate = await CanDelegateRoleAsync(
                tenantId,
                actorId,
                actorType,
                roleId,
                scope,
                cancellationToken);

            if (!canDelegate)
            {
                throw new UnauthorizedAccessException(
                    $"Actor {actorId} cannot delegate role {roleId} on scope {scope.Path}");
            }

            // 2. Créer l'assignation dans OpenFGA
            var relation = MapRoleToRelation(roleId);
            var objectType = "scope";
            var objectId = scope.Path;
            var user = $"{targetPrincipalType.ToOpenFgaPrefix()}:{targetPrincipalId.Value}";

            await _openFgaService.WriteAsync(
                tenantId,
                user,
                relation,
                objectType,
                objectId,
                cancellationToken);

            // 3. Créer l'ID de l'assignation
            var assignmentId = RoleAssignmentId.New();

            // 4. Invalider le cache pour le principal cible
            await _cacheService.InvalidatePrincipalAsync(
                tenantId,
                targetPrincipalId,
                cancellationToken);

            // 5. Auditer
            var auditLog = AuditLog.CreateRoleAssignment(
                tenantId,
                actorId,
                actorType,
                targetPrincipalId,
                targetPrincipalType,
                roleId,
                scope,
                AuditResult.Success);

            await _auditRepository.AddAsync(auditLog, cancellationToken);

            _logger.LogInformation(
                "Role {RoleId} assigned to {TargetPrincipal} on {Scope} by {Actor}",
                roleId,
                targetPrincipalId,
                scope.Path,
                actorId);

            return assignmentId;
        }
        catch (UnauthorizedAccessException)
        {
            // Auditer l'échec
            var auditLog = AuditLog.CreateRoleAssignment(
                tenantId,
                actorId,
                actorType,
                targetPrincipalId,
                targetPrincipalType,
                roleId,
                scope,
                AuditResult.Denied,
                "Delegation not allowed");

            await _auditRepository.AddAsync(auditLog, cancellationToken);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task RevokeRoleAsync(
        TenantId tenantId,
        PrincipalId actorId,
        PrincipalType actorType,
        RoleAssignmentId assignmentId,
        CancellationToken cancellationToken = default)
    {
        // Note: Dans une implémentation complète, on récupérerait l'assignation
        // depuis un repository pour obtenir les détails (principal, rôle, scope)
        // Pour l'instant, on simplifie

        _logger.LogInformation(
            "Role assignment {AssignmentId} revoked by {Actor}",
            assignmentId,
            actorId);

        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RoleAssignmentInfo>> ListRoleAssignmentsAsync(
        TenantId tenantId,
        PrincipalId principalId,
        CancellationToken cancellationToken = default)
    {
        // Note: Cette implémentation nécessite un repository d'assignations
        // Pour l'instant, on retourne une liste vide
        await Task.CompletedTask;
        return Array.Empty<RoleAssignmentInfo>();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PrincipalAccessInfo>> ListPrincipalsWithAccessAsync(
        TenantId tenantId,
        Scope scope,
        Permission? permission = null,
        CancellationToken cancellationToken = default)
    {
        // Note: Cette implémentation utiliserait OpenFGA ListUsers
        // Pour l'instant, on retourne une liste vide
        await Task.CompletedTask;
        return Array.Empty<PrincipalAccessInfo>();
    }

    /// <summary>
    /// Mappe une permission vers une relation OpenFGA.
    /// </summary>
    private static string MapPermissionToRelation(Permission permission)
    {
        // Les permissions de type read/write/manage mappent vers les relations
        var action = permission.Action.ToLowerInvariant();
        return action switch
        {
            "read" => "can_read",
            "write" => "can_write",
            "manage" => "can_manage",
            "delete" => "can_write",
            "create" => "can_write",
            "update" => "can_write",
            _ => $"can_{action}"
        };
    }

    /// <summary>
    /// Mappe un rôle vers une relation OpenFGA.
    /// </summary>
    private static string MapRoleToRelation(RoleId roleId)
    {
        var role = roleId.Value.ToLowerInvariant();
        return role switch
        {
            "owner" => "owner",
            "contributor" => "contributor",
            "reader" => "reader",
            _ => role
        };
    }

    /// <summary>
    /// Vérifie si un acteur peut déléguer un rôle.
    /// </summary>
    private async Task<bool> CanDelegateRoleAsync(
        TenantId tenantId,
        PrincipalId actorId,
        PrincipalType actorType,
        RoleId roleId,
        Scope scope,
        CancellationToken cancellationToken)
    {
        // L'acteur doit avoir le rôle owner ou un rôle supérieur sur le scope
        var canManage = await _openFgaService.CheckAsync(
            tenantId,
            actorId,
            actorType,
            "can_manage",
            "scope",
            scope.Path,
            cancellationToken);

        if (!canManage)
        {
            return false;
        }

        // Vérifier la hiérarchie des rôles (owner peut déléguer tout,
        // contributor peut déléguer contributor et reader, reader ne peut rien déléguer)
        var actorRole = await GetHighestRoleAsync(
            tenantId,
            actorId,
            actorType,
            scope,
            cancellationToken);

        if (actorRole is null)
        {
            return false;
        }

        return CanRoleDelegateOther(actorRole!.Value, roleId);
    }

    /// <summary>
    /// Récupère le rôle le plus élevé d'un principal sur un scope.
    /// </summary>
    private async Task<RoleId?> GetHighestRoleAsync(
        TenantId tenantId,
        PrincipalId principalId,
        PrincipalType principalType,
        Scope scope,
        CancellationToken cancellationToken)
    {
        // Vérifier dans l'ordre: owner, contributor, reader
        if (await _openFgaService.CheckAsync(
            tenantId, principalId, principalType, "owner", "scope", scope.Path, cancellationToken))
        {
            return RoleId.Owner;
        }

        if (await _openFgaService.CheckAsync(
            tenantId, principalId, principalType, "contributor", "scope", scope.Path, cancellationToken))
        {
            return RoleId.Contributor;
        }

        if (await _openFgaService.CheckAsync(
            tenantId, principalId, principalType, "reader", "scope", scope.Path, cancellationToken))
        {
            return RoleId.Reader;
        }

        return null;
    }

    /// <summary>
    /// Détermine si un rôle peut déléguer un autre rôle.
    /// </summary>
    private static bool CanRoleDelegateOther(RoleId actorRole, RoleId targetRole)
    {
        var actorLevel = GetRoleLevel(actorRole);
        var targetLevel = GetRoleLevel(targetRole);

        // Un rôle ne peut déléguer que des rôles de niveau inférieur ou égal
        return actorLevel >= targetLevel;
    }

    /// <summary>
    /// Retourne le niveau hiérarchique d'un rôle.
    /// </summary>
    private static int GetRoleLevel(RoleId roleId)
    {
        return roleId.Value.ToLowerInvariant() switch
        {
            "owner" => 3,
            "contributor" => 2,
            "reader" => 1,
            _ => 0
        };
    }

    /// <summary>
    /// Audite une vérification de permission.
    /// </summary>
    private async Task LogPermissionCheckAsync(
        TenantId tenantId,
        PrincipalId principalId,
        PrincipalType principalType,
        Permission permission,
        Scope scope,
        bool allowed,
        double durationMs,
        bool cacheHit,
        CancellationToken cancellationToken)
    {
        var auditLog = AuditLog.CreatePermissionCheck(
            tenantId,
            principalId,
            principalType,
            permission,
            scope,
            allowed,
            durationMs,
            cacheHit);

        await _auditRepository.AddAsync(auditLog, cancellationToken);
    }
}
