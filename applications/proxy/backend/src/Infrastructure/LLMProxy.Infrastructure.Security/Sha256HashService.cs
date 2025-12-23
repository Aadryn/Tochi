using System.Security.Cryptography;
using System.Text;
using LLMProxy.Infrastructure.Security.Abstractions;

namespace LLMProxy.Infrastructure.Security;

/// <summary>
/// Implémentation du service de hachage utilisant l'algorithme SHA-256.
/// </summary>
/// <remarks>
/// Cette classe fournit une implémentation thread-safe du hachage SHA-256.
/// Conforme à ADR-027 (Defensive Programming - Cryptographie) et ADR-034 (Encapsulation).
/// </remarks>
public class Sha256HashService : IHashService
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
    /// <remarks>
    /// La méthode utilise l'encodage UTF-8 pour convertir la chaîne en bytes.
    /// Le résultat est retourné en hexadécimal minuscule pour cohérence avec la base de données.
    /// L'instance SHA256 est correctement disposée via 'using' (ADR-027).
    /// </remarks>
    /// <example>
    /// <code>
    /// var hashService = new Sha256HashService();
    /// var hash = hashService.ComputeSha256Hash("sk-abc123def456");
    /// // Résultat : "a1b2c3d4e5f6..." (64 caractères hexadécimaux en minuscules)
    /// </code>
    /// </example>
    public string ComputeSha256Hash(string input)
    {
        Guard.AgainstNullOrWhiteSpace(input, nameof(input));

        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
