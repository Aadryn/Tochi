using System.Security.Cryptography;
using System.Text;

namespace LLMProxy.Infrastructure.Security.Encryption;

/// <summary>
/// Service de chiffrement/déchiffrement AES-256 pour les secrets (ADR-005 SRP).
/// </summary>
/// <remarks>
/// Responsabilité unique : chiffrer et déchiffrer des données sensibles avec AES-256.
/// Clé de chiffrement chargée depuis une source sécurisée en production (ex: Azure KeyVault).
/// </remarks>
public class SecretEncryptor
{
    // TODO: Charger depuis Azure KeyVault ou variable d'environnement en production
    private static readonly byte[] _encryptionKey = Encoding.UTF8.GetBytes("CHANGE_THIS_32_BYTE_KEY_IN_PROD!");

    /// <summary>
    /// Chiffre un texte clair avec AES-256.
    /// </summary>
    /// <param name="plainText">Texte à chiffrer.</param>
    /// <returns>Texte chiffré encodé en Base64.</returns>
    /// <exception cref="ArgumentNullException">Si plainText est null.</exception>
    public string Encrypt(string plainText)
    {
        if (plainText == null)
            throw new ArgumentNullException(nameof(plainText));

        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.GenerateIV();

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var msEncrypt = new MemoryStream();

        // Écrire IV en premier (nécessaire pour le déchiffrement)
        msEncrypt.Write(aes.IV, 0, aes.IV.Length);

        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(plainText);
        }

        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    /// <summary>
    /// Déchiffre un texte chiffré avec AES-256.
    /// </summary>
    /// <param name="cipherText">Texte chiffré encodé en Base64.</param>
    /// <returns>Texte clair déchiffré.</returns>
    /// <exception cref="ArgumentNullException">Si cipherText est null.</exception>
    /// <exception cref="CryptographicException">Si le déchiffrement échoue (clé incorrecte ou données corrompues).</exception>
    public string Decrypt(string cipherText)
    {
        if (cipherText == null)
            throw new ArgumentNullException(nameof(cipherText));

        var fullCipher = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _encryptionKey;

        // Extraire IV du début du message chiffré
        var iv = new byte[aes.IV.Length];
        var cipher = new byte[fullCipher.Length - iv.Length];

        Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

        aes.IV = iv;

        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var msDecrypt = new MemoryStream(cipher);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);

        return srDecrypt.ReadToEnd();
    }
}
