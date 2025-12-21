namespace LLMProxy.Domain.Extensions;

/// <summary>
/// Extensions pour la normalisation des chaînes de caractères.
/// </summary>
/// <remarks>
/// Centralise la logique de normalisation pour garantir la cohérence
/// des comparaisons et stockages dans toute l'application.
/// Conforme à ADR-003 (DRY).
/// </remarks>
public static class StringNormalizationExtensions
{
    /// <summary>
    /// Normalise une adresse email en la convertissant en minuscules (invariant culture).
    /// </summary>
    /// <param name="email">Email à normaliser.</param>
    /// <returns>Email normalisé en minuscules.</returns>
    /// <example>
    /// <code>
    /// var normalized = "User@Example.COM".NormalizeEmail(); // returns "user@example.com"
    /// </code>
    /// </example>
    public static string NormalizeEmail(this string email)
    {
        return email.ToLowerInvariant();
    }

    /// <summary>
    /// Normalise un slug en le convertissant en minuscules (invariant culture).
    /// </summary>
    /// <param name="slug">Slug à normaliser.</param>
    /// <returns>Slug normalisé en minuscules.</returns>
    /// <example>
    /// <code>
    /// var normalized = "My-Company".NormalizeSlug(); // returns "my-company"
    /// </code>
    /// </example>
    public static string NormalizeSlug(this string slug)
    {
        return slug.ToLowerInvariant();
    }
}
