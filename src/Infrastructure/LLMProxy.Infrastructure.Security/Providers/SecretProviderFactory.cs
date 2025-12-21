using Microsoft.Extensions.Configuration;

namespace LLMProxy.Infrastructure.Security.Providers;

/// <summary>
/// Factory pour créer le provider de secrets approprié selon la configuration (ADR-014 Dependency Injection).
/// </summary>
/// <remarks>
/// Encapsule la logique de sélection du provider (Strategy Pattern).
/// Lit la configuration "SecretProvider:Type" pour déterminer le provider à instancier.
/// </remarks>
internal class SecretProviderFactory
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="SecretProviderFactory"/>.
    /// </summary>
    /// <param name="configuration">Configuration de l'application.</param>
    public SecretProviderFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Crée le provider de secrets approprié selon la configuration.
    /// </summary>
    /// <returns>Instance de <see cref="ISecretProvider"/> configurée.</returns>
    /// <exception cref="NotSupportedException">Si le type de provider n'est pas supporté.</exception>
    public ISecretProvider CreateProvider()
    {
        var providerType = Enum.Parse<SecretProviderType>(
            _configuration["SecretProvider:Type"] ?? "EnvironmentVariable",
            ignoreCase: true
        );

        return providerType switch
        {
            SecretProviderType.EnvironmentVariable => new EnvironmentVariableSecretProvider(_configuration),
            SecretProviderType.AzureKeyVault => new AzureKeyVaultSecretProvider(),
            SecretProviderType.HashiCorpVault => new HashiCorpVaultSecretProvider(),
            SecretProviderType.EncryptedDatabase => new EncryptedDatabaseSecretProvider(),
            _ => throw new NotSupportedException($"Secret provider type {providerType} is not supported")
        };
    }
}
