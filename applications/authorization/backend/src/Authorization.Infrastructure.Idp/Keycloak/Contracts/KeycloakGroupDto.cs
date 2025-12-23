// <copyright file="KeycloakGroupDto.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Authorization.Infrastructure.Idp.Models;

namespace Authorization.Infrastructure.Idp.Keycloak.Contracts;

/// <summary>
/// DTO représentant un groupe Keycloak.
/// </summary>
/// <remarks>
/// Correspond à la structure JSON retournée par l'API Admin Keycloak pour les groupes.
/// </remarks>
internal sealed record KeycloakGroupDto
{
    /// <summary>
    /// Identifiant unique du groupe dans Keycloak.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Nom du groupe.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Chemin hiérarchique du groupe (ex: /parent/child).
    /// </summary>
    public string? Path { get; init; }

    /// <summary>
    /// Convertit ce DTO en <see cref="IdpGroup"/>.
    /// </summary>
    /// <returns>Instance de <see cref="IdpGroup"/> ou null si l'ID n'est pas un GUID valide.</returns>
    public IdpGroup? ToIdpGroup()
    {
        if (!Guid.TryParse(Id, out var objectId))
        {
            return null;
        }

        return new IdpGroup(objectId, Name, Path);
    }
}
