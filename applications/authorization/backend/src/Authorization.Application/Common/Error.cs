namespace Authorization.Application.Common;

/// <summary>
/// Représente une erreur applicative.
/// </summary>
/// <param name="Code">Code d'erreur.</param>
/// <param name="Message">Message d'erreur.</param>
/// <remarks>
/// Fournit des méthodes factory pour créer des erreurs courantes
/// avec des codes standardisés.
/// </remarks>
public record Error(string Code, string Message)
{
    /// <summary>
    /// Crée une erreur de validation.
    /// </summary>
    /// <param name="message">Message décrivant l'erreur de validation.</param>
    /// <returns>Erreur avec code VALIDATION_ERROR.</returns>
    public static Error Validation(string message) => new("VALIDATION_ERROR", message);

    /// <summary>
    /// Crée une erreur de ressource non trouvée.
    /// </summary>
    /// <param name="resource">Type de ressource recherchée.</param>
    /// <param name="id">Identifiant de la ressource.</param>
    /// <returns>Erreur avec code NOT_FOUND.</returns>
    public static Error NotFound(string resource, string id) =>
        new("NOT_FOUND", $"{resource} with id '{id}' was not found.");

    /// <summary>
    /// Crée une erreur d'accès non autorisé.
    /// </summary>
    /// <param name="message">Message personnalisé (optionnel).</param>
    /// <returns>Erreur avec code UNAUTHORIZED.</returns>
    public static Error Unauthorized(string message = "Access denied.") =>
        new("UNAUTHORIZED", message);

    /// <summary>
    /// Crée une erreur d'opération interdite.
    /// </summary>
    /// <param name="message">Message décrivant la restriction.</param>
    /// <returns>Erreur avec code FORBIDDEN.</returns>
    public static Error Forbidden(string message) => new("FORBIDDEN", message);

    /// <summary>
    /// Crée une erreur de conflit (ressource existe déjà).
    /// </summary>
    /// <param name="message">Message décrivant le conflit.</param>
    /// <returns>Erreur avec code CONFLICT.</returns>
    public static Error Conflict(string message) => new("CONFLICT", message);

    /// <summary>
    /// Crée une erreur interne.
    /// </summary>
    /// <param name="message">Message décrivant l'erreur.</param>
    /// <returns>Erreur avec code INTERNAL_ERROR.</returns>
    public static Error Internal(string message) => new("INTERNAL_ERROR", message);
}
