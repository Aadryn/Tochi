// <copyright file="TokenResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace Authorization.Infrastructure.Idp.Keycloak.Contracts;

/// <summary>
/// Réponse du endpoint de token Keycloak.
/// </summary>
/// <remarks>
/// Contient le token d'accès JWT et sa durée de validité.
/// </remarks>
internal sealed record TokenResponse
{
    /// <summary>
    /// Token d'accès JWT.
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// Durée de validité du token en secondes.
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }
}
