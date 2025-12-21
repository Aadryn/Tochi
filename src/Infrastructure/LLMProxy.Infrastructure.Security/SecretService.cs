using LLMProxy.Domain.Interfaces;
using LLMProxy.Infrastructure.Security.Providers;
using Microsoft.Extensions.Configuration;

namespace LLMProxy.Infrastructure.Security;

/// <summary>
/// Service orchestrateur de gestion des secrets (ADR-005 SRP, Strategy Pattern).
/// </summary>
/// <remarks>
/// Délègue la récupération, le stockage et la suppression de secrets au provider configuré
/// (EnvironmentVariable, AzureKeyVault, HashiCorpVault, EncryptedDatabase).
/// Implémente un cache en mémoire pour optimiser les accès répétés.
/// </remarks>
public class SecretService : ISecretService
{
    private readonly ISecretProvider _provider;
    private readonly Dictionary<string, string> _cachedSecrets = new();
    private readonly SemaphoreSlim _cacheLock = new(1, 1);

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="SecretService"/>.
    /// </summary>
    /// <param name="configuration">Configuration de l'application pour sélection du provider.</param>
    public SecretService(IConfiguration configuration)
    {
        var factory = new SecretProviderFactory(configuration);
        _provider = factory.CreateProvider();
    }

    /// <summary>
    /// Récupère un secret depuis le provider configuré avec cache.
    /// </summary>
    /// <param name="secretName">Nom du secret à récupérer.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Valeur du secret ou null si introuvable.</returns>
    public async Task<string?> GetSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        // Vérifier cache
        await _cacheLock.WaitAsync(cancellationToken);
        try
        {
            if (_cachedSecrets.TryGetValue(secretName, out var cachedValue))
            {
                return cachedValue;
            }
        }
        finally
        {
            _cacheLock.Release();
        }

        // Récupérer depuis le provider
        var secret = await _provider.GetSecretAsync(secretName, cancellationToken);

        // Mettre en cache
        if (!string.IsNullOrEmpty(secret))
        {
            await _cacheLock.WaitAsync(cancellationToken);
            try
            {
                _cachedSecrets[secretName] = secret;
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        return secret;
    }

    /// <summary>
    /// Définit un secret dans le provider configuré et met à jour le cache.
    /// </summary>
    /// <param name="secretName">Nom du secret à créer ou mettre à jour.</param>
    /// <param name="secretValue">Valeur du secret.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Tâche asynchrone.</returns>
    public async Task SetSecretAsync(string secretName, string secretValue, CancellationToken cancellationToken = default)
    {
        // Définir dans le provider
        await _provider.SetSecretAsync(secretName, secretValue, cancellationToken);

        // Mettre à jour le cache
        await _cacheLock.WaitAsync(cancellationToken);
        try
        {
            _cachedSecrets[secretName] = secretValue;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    /// <summary>
    /// Supprime un secret du provider configuré et du cache.
    /// </summary>
    /// <param name="secretName">Nom du secret à supprimer.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>True si suppression réussie, false sinon.</returns>
    public async Task<bool> DeleteSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        try
        {
            // Supprimer du provider
            await _provider.DeleteSecretAsync(secretName, cancellationToken);

            // Supprimer du cache
            await _cacheLock.WaitAsync(cancellationToken);
            try
            {
                _cachedSecrets.Remove(secretName);
            }
            finally
            {
                _cacheLock.Release();
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}
