// <copyright file="IIdpSyncService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Authorization.Domain.ValueObjects;

namespace Authorization.Infrastructure.Idp.Sync;

/// <summary>
/// Service de synchronisation des memberships de groupes depuis l'IDP vers OpenFGA.
/// Supporte 3 stratégies : JIT (temps réel), Batch (périodique), Webhook (événementiel).
/// </summary>
public interface IIdpSyncService
{
    /// <summary>
    /// Synchronise les groupes d'un utilisateur depuis l'IDP vers OpenFGA (JIT).
    /// Appelé au premier accès de l'utilisateur via le middleware JWT.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="userId">Identifiant de l'utilisateur dans l'IDP.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Résultat de la synchronisation.</returns>
    Task<SyncResult> SyncUserGroupsAsync(
        TenantId tenantId,
        PrincipalId userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronise tous les membres d'un groupe depuis l'IDP vers OpenFGA.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="groupId">Identifiant du groupe dans l'IDP.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Résultat de la synchronisation.</returns>
    Task<SyncResult> SyncGroupMembersAsync(
        TenantId tenantId,
        PrincipalId groupId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exécute une synchronisation complète de tous les groupes et memberships (Batch).
    /// Appelé par le job de synchronisation périodique.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Résultat global de la synchronisation.</returns>
    Task<BatchSyncResult> SyncAllAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gère un événement de modification de groupe depuis l'IDP (Webhook).
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="eventType">Type d'événement (group.member.added, group.member.removed, etc.).</param>
    /// <param name="groupId">Identifiant du groupe concerné.</param>
    /// <param name="memberId">Identifiant du membre ajouté/retiré (si applicable).</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Résultat de la synchronisation.</returns>
    Task<SyncResult> HandleGroupEventAsync(
        TenantId tenantId,
        string eventType,
        PrincipalId groupId,
        PrincipalId? memberId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Résultat d'une opération de synchronisation unitaire.
/// </summary>
/// <param name="Success">Indique si la synchronisation a réussi.</param>
/// <param name="AddedCount">Nombre de relations ajoutées.</param>
/// <param name="RemovedCount">Nombre de relations supprimées.</param>
/// <param name="ErrorMessage">Message d'erreur en cas d'échec.</param>
public record SyncResult(
    bool Success,
    int AddedCount,
    int RemovedCount,
    string? ErrorMessage = null)
{
    /// <summary>
    /// Crée un résultat de synchronisation réussie.
    /// </summary>
    public static SyncResult Successful(int added, int removed) =>
        new(true, added, removed);

    /// <summary>
    /// Crée un résultat de synchronisation en échec.
    /// </summary>
    public static SyncResult Failed(string error) =>
        new(false, 0, 0, error);
}

/// <summary>
/// Résultat d'une synchronisation batch complète.
/// </summary>
/// <param name="TotalGroups">Nombre total de groupes traités.</param>
/// <param name="TotalUsers">Nombre total d'utilisateurs traités.</param>
/// <param name="TotalRelationsAdded">Nombre total de relations ajoutées.</param>
/// <param name="TotalRelationsRemoved">Nombre total de relations supprimées.</param>
/// <param name="Errors">Liste des erreurs rencontrées.</param>
/// <param name="Duration">Durée totale de la synchronisation.</param>
public record BatchSyncResult(
    int TotalGroups,
    int TotalUsers,
    int TotalRelationsAdded,
    int TotalRelationsRemoved,
    IReadOnlyList<string> Errors,
    TimeSpan Duration)
{
    /// <summary>
    /// Indique si la synchronisation batch a réussi sans erreurs.
    /// </summary>
    public bool Success => Errors.Count == 0;
}
