using LLMProxy.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace LLMProxy.Infrastructure.Security.Abstractions;

/// <summary>
/// Résultat de la validation d'une clé API.
/// </summary>
/// <remarks>
/// Cette classe encapsule le résultat d'une validation de clé API,
/// incluant l'état de validité et les détails d'erreur éventuels.
/// Conforme à ADR-023 (Result Pattern).
/// </remarks>
public class ApiKeyValidationResult
{
    /// <summary>
    /// Indique si la clé API est valide.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Message d'erreur en cas de validation échouée (null si valide).
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Code de statut HTTP suggéré en cas d'échec de validation.
    /// </summary>
    public int StatusCode { get; init; }

    /// <summary>
    /// Crée un résultat de validation réussi.
    /// </summary>
    /// <returns>Résultat indiquant que la clé API est valide.</returns>
    /// <example>
    /// <code>
    /// var result = ApiKeyValidationResult.Success();
    /// if (result.IsValid)
    /// {
    ///     // Clé valide, continuer le traitement
    /// }
    /// </code>
    /// </example>
    public static ApiKeyValidationResult Success() =>
        new() { IsValid = true, StatusCode = StatusCodes.Status200OK };

    /// <summary>
    /// Crée un résultat de validation échoué.
    /// </summary>
    /// <param name="errorMessage">Message décrivant la raison de l'échec de validation.</param>
    /// <param name="statusCode">Code de statut HTTP suggéré (par défaut 401 Unauthorized).</param>
    /// <returns>Résultat indiquant que la clé API est invalide.</returns>
    /// <example>
    /// <code>
    /// return ApiKeyValidationResult.Failure("API key has been revoked");
    /// // ou avec code spécifique
    /// return ApiKeyValidationResult.Failure("API key expired", 403);
    /// </code>
    /// </example>
    public static ApiKeyValidationResult Failure(string errorMessage, int statusCode = StatusCodes.Status401Unauthorized) =>
        new() { IsValid = false, ErrorMessage = errorMessage, StatusCode = statusCode };
}
