using LLMProxy.Domain.Entities;

namespace LLMProxy.Infrastructure.LLMProviders.Configuration;

/// <summary>
/// Configuration d'un provider LLM individuel.
/// </summary>
public sealed class LLMProviderConfiguration
{
    /// <summary>
    /// Type du provider.
    /// </summary>
    public required ProviderType Type { get; init; }

    /// <summary>
    /// Nom unique du provider (pour référence).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Indique si le provider est activé.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Alias pour Enabled pour compatibilité.
    /// </summary>
    public bool IsEnabled => Enabled;

    /// <summary>
    /// Priorité pour le routage (plus bas = plus prioritaire).
    /// </summary>
    public int Priority { get; init; } = 100;

    /// <summary>
    /// URL de base de l'API.
    /// </summary>
    public required string BaseUrl { get; init; }

    /// <summary>
    /// Clé API ou token d'authentification.
    /// Peut être une variable d'environnement avec ${VAR_NAME}.
    /// </summary>
    public string? ApiKey { get; init; }

    /// <summary>
    /// Modèle par défaut à utiliser.
    /// </summary>
    public string? DefaultModel { get; init; }

    /// <summary>
    /// Nombre maximum de tokens par défaut.
    /// </summary>
    public int? MaxTokens { get; init; }

    /// <summary>
    /// Timeout des requêtes en secondes.
    /// </summary>
    public int TimeoutSeconds { get; init; } = 120;

    /// <summary>
    /// Nombre maximum de retries.
    /// </summary>
    public int? MaxRetries { get; init; }

    /// <summary>
    /// Configuration Azure spécifique (pour AzureOpenAI).
    /// </summary>
    public AzureConfiguration? Azure { get; init; }

    /// <summary>
    /// Configuration AWS spécifique (pour Bedrock).
    /// </summary>
    public AwsConfiguration? Aws { get; init; }

    /// <summary>
    /// Headers HTTP personnalisés.
    /// </summary>
    public Dictionary<string, string>? CustomHeaders { get; init; }

    /// <summary>
    /// Métadonnées additionnelles.
    /// </summary>
    public Dictionary<string, string>? Metadata { get; init; }
}

/// <summary>
/// Configuration spécifique à Azure OpenAI.
/// </summary>
public sealed class AzureConfiguration
{
    /// <summary>
    /// Nom du déploiement Azure.
    /// </summary>
    public required string DeploymentName { get; init; }

    /// <summary>
    /// Version de l'API Azure.
    /// </summary>
    public string ApiVersion { get; init; } = "2024-02-01";

    /// <summary>
    /// Région Azure (optionnel).
    /// </summary>
    public string? Region { get; init; }
}

/// <summary>
/// Configuration spécifique à AWS Bedrock.
/// </summary>
public sealed class AwsConfiguration
{
    /// <summary>
    /// Région AWS.
    /// </summary>
    public required string Region { get; init; }

    /// <summary>
    /// Profil AWS CLI à utiliser.
    /// </summary>
    public string? Profile { get; init; }

    /// <summary>
    /// Access Key ID (optionnel si profil utilisé).
    /// </summary>
    public string? AccessKeyId { get; init; }

    /// <summary>
    /// Secret Access Key (optionnel si profil utilisé).
    /// </summary>
    public string? SecretAccessKey { get; init; }
}
