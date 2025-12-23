// <copyright file="AuthorizationClientConfiguration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LLMProxy.Infrastructure.Authorization.Configuration;

/// <summary>
/// Configuration pour le client HTTP de l'application Authorization externe.
/// </summary>
/// <remarks>
/// <para>
/// Cette configuration permet de spécifier les paramètres de connexion
/// vers l'application Authorization autonome (port 5100).
/// </para>
/// <para>
/// L'application Authorization encapsule OpenFGA et expose une API REST
/// conforme à l'ADR-060 (Azure RBAC style authorization).
/// </para>
/// </remarks>
public class AuthorizationClientConfiguration
{
    /// <summary>
    /// Section de configuration dans appsettings.json.
    /// </summary>
    public const string SectionName = "AuthorizationClient";

    /// <summary>
    /// URL de base de l'application Authorization.
    /// </summary>
    /// <example>http://authorization-api:5100</example>
    public string BaseUrl { get; set; } = "http://localhost:5100";

    /// <summary>
    /// Timeout pour les requêtes HTTP (en secondes).
    /// </summary>
    public int TimeoutSeconds { get; set; } = 5;

    /// <summary>
    /// Nombre de tentatives en cas d'erreur transitoire.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Délai entre les tentatives en cas de retry (en millisecondes).
    /// </summary>
    public int RetryDelayMilliseconds { get; set; } = 200;

    /// <summary>
    /// Durée de cache pour les résultats de vérification (en secondes).
    /// 0 = pas de cache.
    /// </summary>
    public int CacheDurationSeconds { get; set; } = 60;

    /// <summary>
    /// Active ou désactive le service d'autorisation.
    /// Si désactivé, toutes les vérifications retournent "Allowed".
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Clé API pour l'authentification auprès de l'application Authorization.
    /// Optionnel si l'application Authorization accepte les appels internes.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Valide la configuration.
    /// </summary>
    /// <exception cref="InvalidOperationException">Si la configuration est invalide.</exception>
    public void Validate()
    {
        if (Enabled && string.IsNullOrWhiteSpace(BaseUrl))
        {
            throw new InvalidOperationException("BaseUrl est requis quand le service Authorization est activé.");
        }

        if (TimeoutSeconds <= 0)
        {
            throw new InvalidOperationException("TimeoutSeconds doit être positif.");
        }

        if (MaxRetries < 0)
        {
            throw new InvalidOperationException("MaxRetries ne peut pas être négatif.");
        }
    }
}
