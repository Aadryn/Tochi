namespace LLMProxy.Infrastructure.Security;

/// <summary>
/// Service de hachage cryptographique.
/// </summary>
/// <remarks>
/// Cette interface définit le contrat pour les services de hachage.
/// Permet l'encapsulation des bibliothèques cryptographiques tierces (ADR-034).
/// </remarks>
public interface IHashService
{
    /// <summary>
    /// Calcule le hachage SHA-256 d'une chaîne de caractères.
    /// </summary>
    /// <param name="input">Chaîne à hacher (ne peut être null ou vide).</param>
    /// <returns>
    /// Hachage SHA-256 en format hexadécimal minuscule (64 caractères).
    /// </returns>
    /// <exception cref="ArgumentNullException">Si <paramref name="input"/> est null.</exception>
    /// <exception cref="ArgumentException">Si <paramref name="input"/> est vide ou ne contient que des espaces.</exception>
    /// <example>
    /// <code>
    /// var hash = hashService.ComputeSha256Hash("sk-abc123def456");
    /// // Résultat : "a1b2c3d4e5f6..." (64 caractères hexadécimaux)
    /// </code>
    /// </example>
    string ComputeSha256Hash(string input);
}
