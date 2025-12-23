// <copyright file="AssignmentExpirationEntity.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Authorization.Infrastructure.PostgreSQL.Entities;

/// <summary>
/// Entité EF Core pour le stockage des expirations d'assignation.
/// </summary>
public sealed class AssignmentExpirationEntity
{
    /// <summary>
    /// Identifiant unique auto-incrémenté.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Identifiant du tenant.
    /// </summary>
    public required string TenantId { get; set; }

    /// <summary>
    /// Identifiant du principal (user, group, service account).
    /// </summary>
    public Guid PrincipalId { get; set; }

    /// <summary>
    /// Type de principal : User, Group, ServiceAccount.
    /// </summary>
    public required string PrincipalType { get; set; }

    /// <summary>
    /// Nom du rôle assigné.
    /// </summary>
    public required string Role { get; set; }

    /// <summary>
    /// Scope de l'assignation (format URI).
    /// </summary>
    public required string Scope { get; set; }

    /// <summary>
    /// Date et heure d'expiration (UTC).
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    /// Date et heure de création (UTC).
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
