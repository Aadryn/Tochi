// <copyright file="KeycloakClientDto.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Authorization.Infrastructure.Idp.Keycloak.Contracts;

/// <summary>
/// DTO représentant un client Keycloak.
/// </summary>
/// <remarks>
/// Correspond à la structure JSON retournée par l'API Admin Keycloak pour les clients.
/// Utilisé notamment pour les service accounts.
/// </remarks>
internal sealed record KeycloakClientDto
{
    /// <summary>
    /// Identifiant unique du client dans Keycloak.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Client ID (identifiant public du client).
    /// </summary>
    public string ClientId { get; init; } = string.Empty;

    /// <summary>
    /// Nom descriptif du client.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Indique si les service accounts sont activés pour ce client.
    /// </summary>
    public bool? ServiceAccountsEnabled { get; init; }
}
