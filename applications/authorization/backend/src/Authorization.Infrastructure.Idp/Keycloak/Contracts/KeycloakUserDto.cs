// <copyright file="KeycloakUserDto.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Authorization.Infrastructure.Idp.Models;

namespace Authorization.Infrastructure.Idp.Keycloak.Contracts;

/// <summary>
/// DTO représentant un utilisateur Keycloak.
/// </summary>
/// <remarks>
/// Correspond à la structure JSON retournée par l'API Admin Keycloak pour les utilisateurs.
/// </remarks>
internal sealed record KeycloakUserDto
{
    /// <summary>
    /// Identifiant unique de l'utilisateur dans Keycloak.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Adresse email de l'utilisateur.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Nom d'utilisateur (username).
    /// </summary>
    public string? Username { get; init; }

    /// <summary>
    /// Prénom de l'utilisateur.
    /// </summary>
    public string? FirstName { get; init; }

    /// <summary>
    /// Nom de famille de l'utilisateur.
    /// </summary>
    public string? LastName { get; init; }

    /// <summary>
    /// Indique si le compte est activé.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// Convertit ce DTO en <see cref="IdpUser"/>.
    /// </summary>
    /// <returns>Instance de <see cref="IdpUser"/> ou null si l'ID n'est pas un GUID valide.</returns>
    public IdpUser? ToIdpUser()
    {
        if (!Guid.TryParse(Id, out var objectId))
        {
            return null;
        }

        var displayName = $"{FirstName} {LastName}".Trim();
        if (string.IsNullOrEmpty(displayName))
        {
            displayName = Username ?? Email ?? Id;
        }

        return new IdpUser(
            objectId,
            Email ?? Username ?? Id,
            displayName,
            Username,
            Enabled);
    }
}
