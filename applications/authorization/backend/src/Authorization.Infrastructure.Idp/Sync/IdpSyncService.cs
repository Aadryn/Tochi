// <copyright file="IdpSyncService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Diagnostics;
using Authorization.Application.Services;
using Authorization.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Idp.Sync;

/// <summary>
/// Implémentation du service de synchronisation IDP → OpenFGA.
/// Synchronise les memberships de groupes depuis l'Identity Provider.
/// </summary>
public sealed class IdpSyncService : IIdpSyncService
{
    private readonly IIdpClient _idpClient;
    private readonly IRbacAuthorizationService _authorizationService;
    private readonly ILogger<IdpSyncService> _logger;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="IdpSyncService"/>.
    /// </summary>
    /// <param name="idpClient">Client IDP pour récupérer les données.</param>
    /// <param name="authorizationService">Service d'autorisation pour gérer les memberships.</param>
    /// <param name="logger">Logger pour le diagnostic.</param>
    public IdpSyncService(
        IIdpClient idpClient,
        IRbacAuthorizationService authorizationService,
        ILogger<IdpSyncService> logger)
    {
        _idpClient = idpClient ?? throw new ArgumentNullException(nameof(idpClient));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<SyncResult> SyncUserGroupsAsync(
        TenantId tenantId,
        PrincipalId userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Synchronisation JIT des groupes pour l'utilisateur {UserId} dans le tenant {TenantId}",
            userId.Value,
            tenantId.Value);

        try
        {
            // 1. Récupérer les groupes de l'utilisateur depuis l'IDP
            var idpGroups = await _idpClient.GetUserGroupsAsync(userId.Value.ToString(), cancellationToken);

            if (idpGroups.Count == 0)
            {
                _logger.LogDebug("Aucun groupe trouvé pour l'utilisateur {UserId}", userId.Value);
                return SyncResult.Successful(0, 0);
            }

            // 2. Récupérer les memberships actuels dans OpenFGA
            var currentMemberships = await _authorizationService.GetGroupMembershipsAsync(
                tenantId,
                userId,
                cancellationToken);

            var currentGroupIds = currentMemberships
                .Select(m => m.Value)
                .ToHashSet();

            var idpGroupIds = idpGroups
                .Select(g => g.ObjectId)
                .ToHashSet();

            // 3. Calculer les différences
            var toAdd = idpGroupIds.Except(currentGroupIds).ToList();
            var toRemove = currentGroupIds.Except(idpGroupIds).ToList();

            // 4. Appliquer les changements
            foreach (var groupId in toAdd)
            {
                await _authorizationService.AddGroupMemberAsync(
                    tenantId,
                    PrincipalId.Create(groupId),
                    userId,
                    PrincipalType.User,
                    cancellationToken);
            }

            foreach (var groupId in toRemove)
            {
                await _authorizationService.RemoveGroupMemberAsync(
                    tenantId,
                    PrincipalId.Create(groupId),
                    userId,
                    cancellationToken);
            }

            _logger.LogInformation(
                "Synchronisation JIT terminée pour {UserId}: +{Added} -{Removed} groupes",
                userId.Value,
                toAdd.Count,
                toRemove.Count);

            return SyncResult.Successful(toAdd.Count, toRemove.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la synchronisation JIT pour {UserId}", userId.Value);
            return SyncResult.Failed(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<SyncResult> SyncGroupMembersAsync(
        TenantId tenantId,
        PrincipalId groupId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Synchronisation des membres du groupe {GroupId} dans le tenant {TenantId}",
            groupId.Value,
            tenantId.Value);

        try
        {
            // 1. Récupérer les membres du groupe depuis l'IDP
            var idpMembers = await _idpClient.GetGroupMembersAsync(groupId.Value.ToString(), cancellationToken);

            // 2. Récupérer les membres actuels dans OpenFGA
            var currentMembers = await _authorizationService.GetGroupMembersAsync(
                tenantId,
                groupId,
                cancellationToken);

            var currentMemberIds = currentMembers
                .Select(m => m.Value)
                .ToHashSet();

            var idpMemberIds = idpMembers
                .Select(m => m.ObjectId)
                .ToHashSet();

            // 3. Calculer les différences
            var toAdd = idpMemberIds.Except(currentMemberIds).ToList();
            var toRemove = currentMemberIds.Except(idpMemberIds).ToList();

            // 4. Appliquer les changements
            foreach (var memberId in toAdd)
            {
                var member = idpMembers.First(m => m.ObjectId == memberId);
                var principalType = member.PrincipalType switch
                {
                    "User" => PrincipalType.User,
                    "Group" => PrincipalType.Group,
                    "ServiceAccount" => PrincipalType.ServiceAccount,
                    _ => PrincipalType.User
                };

                await _authorizationService.AddGroupMemberAsync(
                    tenantId,
                    groupId,
                    PrincipalId.Create(memberId),
                    principalType,
                    cancellationToken);
            }

            foreach (var memberId in toRemove)
            {
                await _authorizationService.RemoveGroupMemberAsync(
                    tenantId,
                    groupId,
                    PrincipalId.Create(memberId),
                    cancellationToken);
            }

            _logger.LogInformation(
                "Synchronisation groupe {GroupId} terminée: +{Added} -{Removed} membres",
                groupId.Value,
                toAdd.Count,
                toRemove.Count);

            return SyncResult.Successful(toAdd.Count, toRemove.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la synchronisation du groupe {GroupId}", groupId.Value);
            return SyncResult.Failed(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<BatchSyncResult> SyncAllAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var errors = new List<string>();
        var totalGroups = 0;
        var totalUsers = 0;
        var totalAdded = 0;
        var totalRemoved = 0;

        _logger.LogInformation(
            "Démarrage synchronisation batch pour le tenant {TenantId}",
            tenantId.Value);

        try
        {
            // Synchroniser tous les groupes
            await foreach (var group in _idpClient.ListGroupsAsync(cancellationToken))
            {
                totalGroups++;

                var syncResult = await SyncGroupMembersAsync(
                    tenantId,
                    PrincipalId.Create(group.ObjectId),
                    cancellationToken);

                if (syncResult.Success)
                {
                    totalAdded += syncResult.AddedCount;
                    totalRemoved += syncResult.RemovedCount;
                }
                else if (syncResult.ErrorMessage != null)
                {
                    errors.Add($"Groupe {group.Name}: {syncResult.ErrorMessage}");
                }
            }

            // Compter les utilisateurs synchronisés
            await foreach (var user in _idpClient.ListUsersAsync(cancellationToken))
            {
                totalUsers++;

                // Optionnel: synchroniser les groupes de chaque utilisateur
                // (peut être désactivé pour performances)
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur critique lors de la synchronisation batch");
            errors.Add($"Erreur critique: {ex.Message}");
        }

        stopwatch.Stop();

        var result = new BatchSyncResult(
            totalGroups,
            totalUsers,
            totalAdded,
            totalRemoved,
            errors,
            stopwatch.Elapsed);

        _logger.LogInformation(
            "Synchronisation batch terminée en {Duration}: {Groups} groupes, {Users} utilisateurs, +{Added} -{Removed}, {Errors} erreurs",
            result.Duration,
            result.TotalGroups,
            result.TotalUsers,
            result.TotalRelationsAdded,
            result.TotalRelationsRemoved,
            result.Errors.Count);

        return result;
    }

    /// <inheritdoc/>
    public async Task<SyncResult> HandleGroupEventAsync(
        TenantId tenantId,
        string eventType,
        PrincipalId groupId,
        PrincipalId? memberId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Traitement événement IDP: {EventType} pour groupe {GroupId}",
            eventType,
            groupId.Value);

        try
        {
            return eventType.ToLowerInvariant() switch
            {
                "group.member.added" when memberId.HasValue =>
                    await HandleMemberAddedAsync(tenantId, groupId, memberId.Value, cancellationToken),

                "group.member.removed" when memberId.HasValue =>
                    await HandleMemberRemovedAsync(tenantId, groupId, memberId.Value, cancellationToken),

                "group.deleted" =>
                    await HandleGroupDeletedAsync(tenantId, groupId, cancellationToken),

                "group.updated" or "group.members.changed" =>
                    await SyncGroupMembersAsync(tenantId, groupId, cancellationToken),

                _ => SyncResult.Successful(0, 0) // Événement ignoré
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du traitement de l'événement {EventType}", eventType);
            return SyncResult.Failed(ex.Message);
        }
    }

    private async Task<SyncResult> HandleMemberAddedAsync(
        TenantId tenantId,
        PrincipalId groupId,
        PrincipalId memberId,
        CancellationToken cancellationToken)
    {
        // Récupérer le type du membre depuis l'IDP
        var user = await _idpClient.GetUserByIdAsync(memberId.Value.ToString(), cancellationToken);
        var principalType = user != null ? PrincipalType.User : PrincipalType.Group;

        await _authorizationService.AddGroupMemberAsync(
            tenantId,
            groupId,
            memberId,
            principalType,
            cancellationToken);

        return SyncResult.Successful(1, 0);
    }

    private async Task<SyncResult> HandleMemberRemovedAsync(
        TenantId tenantId,
        PrincipalId groupId,
        PrincipalId memberId,
        CancellationToken cancellationToken)
    {
        await _authorizationService.RemoveGroupMemberAsync(
            tenantId,
            groupId,
            memberId,
            cancellationToken);

        return SyncResult.Successful(0, 1);
    }

    private async Task<SyncResult> HandleGroupDeletedAsync(
        TenantId tenantId,
        PrincipalId groupId,
        CancellationToken cancellationToken)
    {
        // Récupérer tous les membres actuels et les supprimer
        var members = await _authorizationService.GetGroupMembersAsync(
            tenantId,
            groupId,
            cancellationToken);

        foreach (var memberId in members)
        {
            await _authorizationService.RemoveGroupMemberAsync(
                tenantId,
                groupId,
                memberId,
                cancellationToken);
        }

        return SyncResult.Successful(0, members.Count);
    }
}
