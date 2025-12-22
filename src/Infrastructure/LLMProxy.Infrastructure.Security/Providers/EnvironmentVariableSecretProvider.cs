using Microsoft.Extensions.Configuration;

using LLMProxy.Infrastructure.Security.Abstractions.Providers;

namespace LLMProxy.Infrastructure.Security.Providers;

/// <summary>
/// Fournisseur de secrets basé sur les variables d'environnement et la configuration (appsettings.json).
/// </summary>
/// <remarks>
/// Stratégie de récupération : variables d'environnement système prioritaires, puis fallback sur configuration.
/// </remarks>
internal class EnvironmentVariableSecretProvider : ISecretProvider
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="EnvironmentVariableSecretProvider"/>.
    /// </summary>
    /// <param name="configuration">Configuration de l'application (appsettings.json).</param>
    public EnvironmentVariableSecretProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Récupère un secret depuis les variables d'environnement ou la configuration.
    /// </summary>
    /// <param name="secretName">Nom du secret à récupérer.</param>
    /// <param name="cancellationToken">Jeton d'annulation (non utilisé pour cette stratégie synchrone).</param>
    /// <returns>Valeur du secret ou null si introuvable.</returns>
    public Task<string?> GetSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        var value = Environment.GetEnvironmentVariable(secretName) ?? _configuration[secretName];
        return Task.FromResult(value);
    }

    /// <summary>
    /// Définit une variable d'environnement système (uniquement pour le processus en cours).
    /// </summary>
    /// <param name="secretName">Nom du secret à définir.</param>
    /// <param name="secretValue">Valeur du secret.</param>
    /// <param name="cancellationToken">Jeton d'annulation (non utilisé pour cette stratégie synchrone).</param>
    /// <returns>Tâche asynchrone.</returns>
    public Task SetSecretAsync(string secretName, string secretValue, CancellationToken cancellationToken = default)
    {
        Environment.SetEnvironmentVariable(secretName, secretValue);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Supprime une variable d'environnement système (uniquement pour le processus en cours).
    /// </summary>
    /// <param name="secretName">Nom du secret à supprimer.</param>
    /// <param name="cancellationToken">Jeton d'annulation (non utilisé pour cette stratégie synchrone).</param>
    /// <returns>Tâche asynchrone.</returns>
    public Task DeleteSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        Environment.SetEnvironmentVariable(secretName, null);
        return Task.CompletedTask;
    }
}
