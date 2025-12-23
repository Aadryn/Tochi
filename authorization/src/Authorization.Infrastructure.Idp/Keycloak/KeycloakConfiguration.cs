// <copyright file="KeycloakConfiguration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Authorization.Infrastructure.Idp.Keycloak;

/// <summary>
/// Configuration pour la connexion à un serveur Keycloak.
/// </summary>
public sealed class KeycloakConfiguration
{
    /// <summary>
    /// Section de configuration dans appsettings.json.
    /// </summary>
    public const string SectionName = "Idp:Keycloak";

    /// <summary>
    /// Obtient ou définit l'URL de base du serveur Keycloak.
    /// </summary>
    /// <example>https://keycloak.example.com</example>
    public required string BaseUrl { get; init; }

    /// <summary>
    /// Obtient ou définit le nom du realm Keycloak.
    /// </summary>
    /// <example>llmproxy</example>
    public required string Realm { get; init; }

    /// <summary>
    /// Obtient ou définit le client ID pour l'authentification admin.
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// Obtient ou définit le client secret pour l'authentification admin.
    /// </summary>
    public required string ClientSecret { get; init; }

    /// <summary>
    /// Obtient l'URL complète de l'API Admin Keycloak pour le realm.
    /// </summary>
    public string AdminApiUrl => $"{BaseUrl.TrimEnd('/')}/admin/realms/{Realm}";

    /// <summary>
    /// Obtient l'URL du endpoint token pour obtenir un access token.
    /// </summary>
    public string TokenUrl => $"{BaseUrl.TrimEnd('/')}/realms/{Realm}/protocol/openid-connect/token";
}
