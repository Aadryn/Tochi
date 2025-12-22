using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.Configuration.FeatureFlags;

/// <summary>
/// Implémentation de <see cref="IFeatureFlags"/> basée sur IConfiguration (appsettings.json, variables d'environnement).
/// </summary>
/// <remarks>
/// <para>
/// Cette implémentation lit les feature flags depuis la configuration statique :
/// </para>
/// <list type="bullet">
/// <item><description><c>appsettings.json</c> : Configuration par environnement</description></item>
/// <item><description>Variables d'environnement : Overrides runtime</description></item>
/// <item><description>Secrets utilisateur : Configuration locale développeur</description></item>
/// </list>
/// <para>
/// <strong>Avantages</strong> :
/// </para>
/// <list type="bullet">
/// <item><description>Simple et rapide (aucune dépendance externe)</description></item>
/// <item><description>Fonctionne sans Redis/DB</description></item>
/// <item><description>Configuration versionnée avec le code</description></item>
/// </list>
/// <para>
/// <strong>Limitations</strong> :
/// </para>
/// <list type="bullet">
/// <item><description>Nécessite redéploiement pour changer (pas dynamique)</description></item>
/// <item><description>Pas de scoping par tenant/user</description></item>
/// <item><description>Pas de rollout percentage</description></item>
/// </list>
/// <para>
/// <strong>Usage recommandé</strong> : Fallback quand Redis down, ou flags simples on/off.
/// </para>
/// </remarks>
/// <example>
/// Configuration dans appsettings.json :
/// <code>
/// {
///   "FeatureFlags": {
///     "llm_use_optimized_provider": true,
///     "quota_enhanced_tracking": false,
///     "ui_dark_mode": true
///   }
/// }
/// </code>
/// 
/// Utilisation :
/// <code>
/// services.AddSingleton&lt;IFeatureFlags&gt;(sp =>
/// {
///     var config = sp.GetRequiredService&lt;IConfiguration&gt;();
///     return new ConfigurationFeatureFlags(config);
/// });
/// </code>
/// </example>
public sealed class ConfigurationFeatureFlags : IFeatureFlags
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationFeatureFlags> _logger;
    private const string FeatureFlagsSection = "FeatureFlags";

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="ConfigurationFeatureFlags"/>.
    /// </summary>
    /// <param name="configuration">Configuration de l'application.</param>
    /// <param name="logger">Logger pour traçabilité des activations.</param>
    /// <exception cref="ArgumentNullException">Si <paramref name="configuration"/> est null.</exception>
    public ConfigurationFeatureFlags(
        IConfiguration configuration,
        ILogger<ConfigurationFeatureFlags> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public bool IsEnabled(string featureName)
    {
        Guard.AgainstNullOrWhiteSpace(featureName, nameof(featureName));

        var isEnabled = GetFeatureFlagValue(featureName);
        
        _logger.LogDebug(
            "Feature flag {FeatureName} evaluated from Configuration: {IsEnabled}",
            featureName,
            isEnabled);
        
        return isEnabled;
    }

    /// <inheritdoc />
    /// <remarks>
    /// <strong>Note</strong> : Cette implémentation ne supporte pas le scoping par tenant/user.
    /// Le contexte est ignoré et seule la valeur globale est retournée.
    /// Pour un scoping avancé, utiliser <see cref="Redis.FeatureFlags.RedisFeatureFlags"/>.
    /// </remarks>
    public bool IsEnabled(string featureName, FeatureContext context)
    {
        Guard.AgainstNullOrWhiteSpace(featureName, nameof(featureName));
        ArgumentNullException.ThrowIfNull(context);

        // Configuration ne supporte pas le scoping - retourner valeur globale
        var isEnabled = GetFeatureFlagValue(featureName);
        
        _logger.LogDebug(
            "Feature flag {FeatureName} evaluated from Configuration (context ignored): {IsEnabled}",
            featureName,
            isEnabled);
        
        return isEnabled;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Version synchrone enveloppée dans Task pour conformité interface.
    /// Aucune opération I/O, retour immédiat.
    /// </remarks>
    public Task<bool> IsEnabledAsync(
        string featureName, 
        FeatureContext context, 
        CancellationToken cancellationToken = default)
    {
        var isEnabled = IsEnabled(featureName, context);
        return Task.FromResult(isEnabled);
    }

    /// <summary>
    /// Lit la valeur d'un feature flag depuis la configuration.
    /// </summary>
    /// <param name="featureName">Nom du feature flag.</param>
    /// <returns><c>true</c> si activé, <c>false</c> si désactivé ou absent.</returns>
    /// <remarks>
    /// <para>
    /// Chemins de recherche (par ordre de priorité) :
    /// </para>
    /// <list type="number">
    /// <item><description>Variables d'environnement : <c>FeatureFlags__{featureName}</c></description></item>
    /// <item><description>appsettings.{Environment}.json : <c>FeatureFlags:{featureName}</c></description></item>
    /// <item><description>appsettings.json : <c>FeatureFlags:{featureName}</c></description></item>
    /// </list>
    /// <para>
    /// Si absent ou non-boolean, retourne <c>false</c> par défaut (fail-safe).
    /// </para>
    /// </remarks>
    private bool GetFeatureFlagValue(string featureName)
    {
        var configPath = $"{FeatureFlagsSection}:{featureName}";
        
        // Lire la valeur depuis la configuration
        var value = _configuration[configPath];
        
        if (string.IsNullOrWhiteSpace(value))
        {
            _logger.LogTrace(
                "Feature flag {FeatureName} not found in configuration at path {ConfigPath}, defaulting to false",
                featureName,
                configPath);
            return false;
        }

        // Parser la valeur booléenne
        if (bool.TryParse(value, out var isEnabled))
        {
            return isEnabled;
        }

        _logger.LogWarning(
            "Feature flag {FeatureName} has invalid boolean value '{Value}' at path {ConfigPath}, defaulting to false",
            featureName,
            value,
            configPath);
        
        return false;
    }
}
