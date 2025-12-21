using LLMProxy.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace LLMProxy.Infrastructure.Security;

/// <summary>
/// Multi-environment secret management service
/// Supports: Environment Variables, Azure KeyVault, HashiCorp Vault, Encrypted DB Storage
/// </summary>
public class SecretService : ISecretService
{
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, string> _cachedSecrets = new();
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    private readonly SecretProviderType _providerType;

    // Encryption key for DB-stored secrets (should be loaded from secure location in production)
    private static readonly byte[] _encryptionKey = Encoding.UTF8.GetBytes("CHANGE_THIS_32_BYTE_KEY_IN_PROD!"); // 32 bytes for AES-256

    public SecretService(IConfiguration configuration)
    {
        _configuration = configuration;
        _providerType = Enum.Parse<SecretProviderType>(
            configuration["SecretProvider:Type"] ?? "EnvironmentVariable",
            ignoreCase: true
        );
    }

    public async Task<string?> GetSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        // Check cache first
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

        // Retrieve from provider
        var secret = _providerType switch
        {
            SecretProviderType.EnvironmentVariable => GetFromEnvironment(secretName),
            SecretProviderType.AzureKeyVault => await GetFromAzureKeyVaultAsync(secretName, cancellationToken),
            SecretProviderType.HashiCorpVault => await GetFromHashiCorpVaultAsync(secretName, cancellationToken),
            SecretProviderType.EncryptedDatabase => await GetFromDatabaseAsync(secretName, cancellationToken),
            _ => throw new NotSupportedException($"Secret provider type {_providerType} is not supported")
        };

        // Cache the secret
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

    public async Task SetSecretAsync(string secretName, string secretValue, CancellationToken cancellationToken = default)
    {
        switch (_providerType)
        {
            case SecretProviderType.EnvironmentVariable:
                Environment.SetEnvironmentVariable(secretName, secretValue);
                break;

            case SecretProviderType.AzureKeyVault:
                await SetToAzureKeyVaultAsync(secretName, secretValue, cancellationToken);
                break;

            case SecretProviderType.HashiCorpVault:
                await SetToHashiCorpVaultAsync(secretName, secretValue, cancellationToken);
                break;

            case SecretProviderType.EncryptedDatabase:
                await SetToDatabaseAsync(secretName, secretValue, cancellationToken);
                break;

            default:
                throw new NotSupportedException($"Secret provider type {_providerType} is not supported");
        }

        // Update cache
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

    public async Task<bool> DeleteSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        try
        {
            switch (_providerType)
            {
                case SecretProviderType.EnvironmentVariable:
                    Environment.SetEnvironmentVariable(secretName, null);
                    break;

                case SecretProviderType.AzureKeyVault:
                    await DeleteFromAzureKeyVaultAsync(secretName, cancellationToken);
                    break;

                case SecretProviderType.HashiCorpVault:
                    await DeleteFromHashiCorpVaultAsync(secretName, cancellationToken);
                    break;

                case SecretProviderType.EncryptedDatabase:
                    await DeleteFromDatabaseAsync(secretName, cancellationToken);
                    break;

                default:
                    throw new NotSupportedException($"Secret provider type {_providerType} is not supported");
            }

            // Remove from cache
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

    public string EncryptSecret(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.GenerateIV();

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var msEncrypt = new MemoryStream();
        
        // Write IV first
        msEncrypt.Write(aes.IV, 0, aes.IV.Length);
        
        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(plainText);
        }

        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    public string DecryptSecret(string cipherText)
    {
        var fullCipher = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _encryptionKey;

        // Extract IV from the beginning
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

    private string? GetFromEnvironment(string secretName)
    {
        return Environment.GetEnvironmentVariable(secretName) 
            ?? _configuration[secretName];
    }

    private async Task<string?> GetFromAzureKeyVaultAsync(string secretName, CancellationToken cancellationToken)
    {
        // TODO: Implement Azure KeyVault integration
        // Use Azure.Security.KeyVault.Secrets package
        // var client = new SecretClient(new Uri(vaultUri), new DefaultAzureCredential());
        // var secret = await client.GetSecretAsync(secretName, cancellationToken: cancellationToken);
        // return secret.Value.Value;
        
        await Task.CompletedTask;
        throw new NotImplementedException("Azure KeyVault integration not yet implemented. Install Azure.Security.KeyVault.Secrets package.");
    }

    private async Task SetToAzureKeyVaultAsync(string secretName, string secretValue, CancellationToken cancellationToken)
    {
        // TODO: Implement Azure KeyVault integration
        await Task.CompletedTask;
        throw new NotImplementedException("Azure KeyVault integration not yet implemented.");
    }

    private async Task DeleteFromAzureKeyVaultAsync(string secretName, CancellationToken cancellationToken)
    {
        // TODO: Implement Azure KeyVault integration
        await Task.CompletedTask;
        throw new NotImplementedException("Azure KeyVault integration not yet implemented.");
    }

    private async Task<string?> GetFromHashiCorpVaultAsync(string secretName, CancellationToken cancellationToken)
    {
        // TODO: Implement HashiCorp Vault integration
        // Use VaultSharp package
        await Task.CompletedTask;
        throw new NotImplementedException("HashiCorp Vault integration not yet implemented. Install VaultSharp package.");
    }

    private async Task SetToHashiCorpVaultAsync(string secretName, string secretValue, CancellationToken cancellationToken)
    {
        // TODO: Implement HashiCorp Vault integration
        await Task.CompletedTask;
        throw new NotImplementedException("HashiCorp Vault integration not yet implemented.");
    }

    private async Task DeleteFromHashiCorpVaultAsync(string secretName, CancellationToken cancellationToken)
    {
        // TODO: Implement HashiCorp Vault integration
        await Task.CompletedTask;
        throw new NotImplementedException("HashiCorp Vault integration not yet implemented.");
    }

    private async Task<string?> GetFromDatabaseAsync(string secretName, CancellationToken cancellationToken)
    {
        // TODO: Implement database storage with encryption
        // Store in dedicated secrets table with encryption at rest
        await Task.CompletedTask;
        throw new NotImplementedException("Encrypted database storage not yet implemented.");
    }

    private async Task SetToDatabaseAsync(string secretName, string secretValue, CancellationToken cancellationToken)
    {
        // TODO: Implement database storage with encryption
        await Task.CompletedTask;
        throw new NotImplementedException("Encrypted database storage not yet implemented.");
    }

    private async Task DeleteFromDatabaseAsync(string secretName, CancellationToken cancellationToken)
    {
        // TODO: Implement database storage with encryption
        await Task.CompletedTask;
        throw new NotImplementedException("Encrypted database storage not yet implemented.");
    }

    private enum SecretProviderType
    {
        EnvironmentVariable,
        AzureKeyVault,
        HashiCorpVault,
        EncryptedDatabase
    }
}
