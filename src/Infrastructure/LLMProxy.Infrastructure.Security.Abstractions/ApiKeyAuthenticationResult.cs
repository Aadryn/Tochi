using LLMProxy.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace LLMProxy.Infrastructure.Security.Abstractions;

/// <summary>
/// Résultat de l'authentification par clé API.
/// </summary>
/// <remarks>
/// Cette classe encapsule le résultat d'une tentative d'authentification,
/// incluant l'état de succès/échec, les entités associées, et les détails d'erreur.
/// Conforme à ADR-023 (Result Pattern).
/// </remarks>
public class ApiKeyAuthenticationResult
{
    /// <summary>
    /// Indique si l'authentification a réussi.
    /// </summary>
    public bool IsAuthenticated { get; init; }

    /// <summary>
    /// Clé API validée (null si échec).
    /// </summary>
    public ApiKey? ApiKey { get; init; }

    /// <summary>
    /// Utilisateur associé à la clé API (null si échec).
    /// </summary>
    public User? User { get; init; }

    /// <summary>
    /// Message d'erreur en cas d'échec (null si succès).
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Code de statut HTTP associé au résultat.
    /// </summary>
    public int StatusCode { get; init; }

    /// <summary>
    /// Crée un résultat d'authentification réussi.
    /// </summary>
    /// <param name="apiKey">Clé API authentifiée.</param>
    /// <param name="user">Utilisateur associé.</param>
    /// <returns>Résultat indiquant le succès de l'authentification.</returns>
    public static ApiKeyAuthenticationResult Success(ApiKey apiKey, User user) =>
        new() { IsAuthenticated = true, ApiKey = apiKey, User = user, StatusCode = StatusCodes.Status200OK };

    /// <summary>
    /// Crée un résultat d'authentification échoué.
    /// </summary>
    /// <param name="errorMessage">Message décrivant la raison de l'échec.</param>
    /// <param name="statusCode">Code de statut HTTP (401 par défaut).</param>
    /// <returns>Résultat indiquant l'échec de l'authentification.</returns>
    public static ApiKeyAuthenticationResult Failure(string errorMessage, int statusCode = StatusCodes.Status401Unauthorized) =>
        new() { IsAuthenticated = false, ErrorMessage = errorMessage, StatusCode = statusCode };
}
