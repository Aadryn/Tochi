// <copyright file="AssignmentExpiration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Authorization.Domain.Entities;

/// <summary>
/// Représente une assignation de rôle avec une date d'expiration.
/// </summary>
/// <remarks>
/// <para>
/// Cette entité est utilisée pour tracker les assignations temporaires
/// de rôles qui doivent être supprimées automatiquement après expiration.
/// </para>
/// <para>
/// Exemples de cas d'usage :
/// </para>
/// <list type="bullet">
/// <item>Accès temporaire à une ressource pour un prestataire</item>
/// <item>Délégation de droits pendant les vacances</item>
/// <item>Essai limité dans le temps</item>
/// </list>
/// </remarks>
/// <param name="Id">Identifiant unique de l'enregistrement.</param>
/// <param name="TenantId">Identifiant du tenant.</param>
/// <param name="PrincipalId">Identifiant du principal (user, group, service account).</param>
/// <param name="PrincipalType">Type de principal : User, Group, ServiceAccount.</param>
/// <param name="Role">Nom du rôle assigné.</param>
/// <param name="Scope">Scope de l'assignation (format URI).</param>
/// <param name="ExpiresAt">Date et heure d'expiration (UTC).</param>
/// <param name="CreatedAt">Date et heure de création (UTC).</param>
public sealed record AssignmentExpiration(
    long Id,
    string TenantId,
    Guid PrincipalId,
    string PrincipalType,
    string Role,
    string Scope,
    DateTimeOffset ExpiresAt,
    DateTimeOffset CreatedAt)
{
    /// <summary>
    /// Vérifie si l'assignation est expirée.
    /// </summary>
    /// <param name="asOf">Date de référence pour la comparaison.</param>
    /// <returns>True si l'assignation est expirée.</returns>
    public bool IsExpired(DateTimeOffset asOf) => ExpiresAt <= asOf;

    /// <summary>
    /// Vérifie si l'assignation est expirée à l'instant présent.
    /// </summary>
    public bool IsExpired() => IsExpired(DateTimeOffset.UtcNow);

    /// <summary>
    /// Crée une nouvelle assignation expirée pour test.
    /// </summary>
    internal static AssignmentExpiration CreateForTest(
        long id,
        string tenantId,
        Guid principalId,
        string principalType,
        string role,
        string scope,
        DateTimeOffset expiresAt)
    {
        return new AssignmentExpiration(
            id,
            tenantId,
            principalId,
            principalType,
            role,
            scope,
            expiresAt,
            DateTimeOffset.UtcNow);
    }
}

/// <summary>
/// Requête pour créer une nouvelle expiration d'assignation.
/// </summary>
/// <param name="TenantId">Identifiant du tenant.</param>
/// <param name="PrincipalId">Identifiant du principal.</param>
/// <param name="PrincipalType">Type de principal.</param>
/// <param name="Role">Nom du rôle.</param>
/// <param name="Scope">Scope de l'assignation.</param>
/// <param name="ExpiresAt">Date d'expiration.</param>
public sealed record CreateAssignmentExpirationRequest(
    string TenantId,
    Guid PrincipalId,
    string PrincipalType,
    string Role,
    string Scope,
    DateTimeOffset ExpiresAt);
