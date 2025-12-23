// <copyright file="IExpirationStore.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Authorization.Domain.Entities;

namespace Authorization.Domain.Interfaces;

/// <summary>
/// Interface pour le stockage des dates d'expiration des assignations.
/// </summary>
/// <remarks>
/// <para>
/// Cette interface permet de gérer les assignations temporaires de rôles.
/// Les implémentations peuvent utiliser PostgreSQL, Redis, ou un autre stockage.
/// </para>
/// <para>
/// Les dates d'expiration sont stockées séparément des tuples OpenFGA
/// car OpenFGA ne supporte pas nativement les expirations de tuples.
/// </para>
/// </remarks>
public interface IExpirationStore
{
    /// <summary>
    /// Crée une nouvelle entrée d'expiration.
    /// </summary>
    /// <param name="request">Données de l'expiration à créer.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>L'expiration créée avec son ID.</returns>
    Task<AssignmentExpiration> CreateAsync(
        CreateAssignmentExpirationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère toutes les assignations expirées à une date donnée.
    /// </summary>
    /// <param name="asOf">Date de référence (typiquement UtcNow).</param>
    /// <param name="limit">Nombre maximum d'entrées à retourner.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des assignations expirées.</returns>
    Task<IReadOnlyList<AssignmentExpiration>> GetExpiredAsync(
        DateTimeOffset asOf,
        int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Supprime une entrée d'expiration par son ID.
    /// </summary>
    /// <param name="id">ID de l'entrée à supprimer.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>True si l'entrée a été supprimée.</returns>
    Task<bool> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Supprime une entrée d'expiration par les critères de l'assignation.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="principalId">Identifiant du principal.</param>
    /// <param name="role">Nom du rôle.</param>
    /// <param name="scope">Scope de l'assignation.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>True si l'entrée a été supprimée.</returns>
    Task<bool> DeleteByAssignmentAsync(
        string tenantId,
        Guid principalId,
        string role,
        string scope,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Vérifie si une assignation a une expiration configurée.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="principalId">Identifiant du principal.</param>
    /// <param name="role">Nom du rôle.</param>
    /// <param name="scope">Scope de l'assignation.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>L'expiration si elle existe, null sinon.</returns>
    Task<AssignmentExpiration?> GetByAssignmentAsync(
        string tenantId,
        Guid principalId,
        string role,
        string scope,
        CancellationToken cancellationToken = default);
}
