namespace LLMProxy.Infrastructure.Security.Providers;

/// <summary>
/// Fournisseur de secrets basé sur une base de données avec chiffrement AES-256.
/// </summary>
/// <remarks>
/// Nécessite une table dédiée au stockage des secrets (ex: dbo.Secrets).
/// Les secrets sont stockés chiffrés via SecretEncryptor avant insertion.
/// Implémentation à compléter avec contexte EF Core et table Secrets.
/// </remarks>
internal class EncryptedDatabaseSecretProvider : ISecretProvider
{
    /// <summary>
    /// Récupère un secret chiffré depuis la base de données.
    /// </summary>
    /// <param name="secretName">Nom du secret à récupérer.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Valeur du secret (déchiffrée).</returns>
    /// <exception cref="NotImplementedException">Encrypted database storage non implémenté.</exception>
    public Task<string?> GetSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        // TODO: Implémenter avec EF Core
        // var encryptedSecret = await _context.Secrets.FirstOrDefaultAsync(s => s.Name == secretName, cancellationToken);
        // if (encryptedSecret == null) return null;
        // return _encryptor.Decrypt(encryptedSecret.EncryptedValue);
        
        throw new NotImplementedException(
            "Encrypted database storage not yet implemented. Create Secrets table and DbContext."
        );
    }

    /// <summary>
    /// Définit ou met à jour un secret chiffré dans la base de données.
    /// </summary>
    /// <param name="secretName">Nom du secret à créer ou mettre à jour.</param>
    /// <param name="secretValue">Valeur du secret (sera chiffré avant stockage).</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Tâche asynchrone.</returns>
    /// <exception cref="NotImplementedException">Encrypted database storage non implémenté.</exception>
    public Task SetSecretAsync(string secretName, string secretValue, CancellationToken cancellationToken = default)
    {
        // TODO: Implémenter avec EF Core
        // var encryptedValue = _encryptor.Encrypt(secretValue);
        // var secret = await _context.Secrets.FirstOrDefaultAsync(s => s.Name == secretName, cancellationToken);
        // if (secret != null) secret.EncryptedValue = encryptedValue;
        // else _context.Secrets.Add(new Secret { Name = secretName, EncryptedValue = encryptedValue });
        // await _context.SaveChangesAsync(cancellationToken);
        
        throw new NotImplementedException("Encrypted database storage not yet implemented.");
    }

    /// <summary>
    /// Supprime un secret de la base de données.
    /// </summary>
    /// <param name="secretName">Nom du secret à supprimer.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Tâche asynchrone.</returns>
    /// <exception cref="NotImplementedException">Encrypted database storage non implémenté.</exception>
    public Task DeleteSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        // TODO: Implémenter avec EF Core
        // var secret = await _context.Secrets.FirstOrDefaultAsync(s => s.Name == secretName, cancellationToken);
        // if (secret != null) _context.Secrets.Remove(secret);
        // await _context.SaveChangesAsync(cancellationToken);
        
        throw new NotImplementedException("Encrypted database storage not yet implemented.");
    }
}
