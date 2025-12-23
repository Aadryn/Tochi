// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// LLMProxy - Extension pour l'enregistrement des services LLM Providers (Application Layer)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using LLMProxy.Application.LLMProviders.Services;
using LLMProxy.Application.LLMProviders.Services.Orchestration;
using LLMProxy.Application.LLMProviders.Services.Selection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LLMProxy.Application.Extensions;

/// <summary>
/// Extensions pour l'injection de dépendances des services LLM Providers de la couche Application.
/// Configure l'orchestrateur, le sélecteur de providers et le gestionnaire de failover.
/// </summary>
/// <remarks>
/// <para>
/// Cette classe fournit les méthodes d'extension pour configurer les services
/// de gestion des providers LLM, conformément à l'ADR-034 (Third-Party Library Encapsulation).
/// </para>
/// <para>
/// Les services configurés sont :
/// </para>
/// <list type="bullet">
/// <item><description><b>IProviderOrchestrator</b> : Point d'entrée pour l'exécution des requêtes LLM</description></item>
/// <item><description><b>IProviderSelector</b> : Sélection intelligente des providers</description></item>
/// <item><description><b>IFailoverManager</b> : Gestion du failover automatique</description></item>
/// </list>
/// </remarks>
public static class LLMProvidersApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Ajoute les services de gestion des providers LLM de la couche Application.
    /// </summary>
    /// <param name="services">Collection de services.</param>
    /// <param name="configureOptions">Configuration optionnelle du failover.</param>
    /// <returns>La collection de services pour le chaînage.</returns>
    /// <remarks>
    /// <para>
    /// Ces services sont la couche Application pour l'orchestration des providers LLM.
    /// Ils dépendent de <c>ILLMProviderClientFactory</c> qui doit être enregistré
    /// par le projet Infrastructure via <c>AddLLMProviders()</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Dans Program.cs
    /// builder.Services
    ///     .AddLLMProviders(configuration) // Infrastructure
    ///     .AddLLMProvidersApplication(); // Application
    /// </code>
    /// </example>
    public static IServiceCollection AddLLMProvidersApplication(
        this IServiceCollection services,
        Action<FailoverOptions>? configureOptions = null)
    {
        // ═══════════════════════════════════════════════════════════════
        // OPTIONS DE FAILOVER
        // ═══════════════════════════════════════════════════════════════
        var options = new FailoverOptions();
        configureOptions?.Invoke(options);
        services.TryAddSingleton(options);

        // ═══════════════════════════════════════════════════════════════
        // SERVICES DE SÉLECTION ET FAILOVER
        // ═══════════════════════════════════════════════════════════════

        // IProviderSelector : Sélection des providers par critères
        services.TryAddSingleton<IProviderSelector, ProviderSelector>();

        // IFailoverManager : Gestion du failover et blacklist
        services.TryAddSingleton<IFailoverManager, FailoverManager>();

        // IProviderMetricsService : Service optionnel de métriques
        // Non enregistré par défaut, peut être ajouté par l'utilisateur

        // ═══════════════════════════════════════════════════════════════
        // ORCHESTRATEUR (Point d'entrée principal)
        // ═══════════════════════════════════════════════════════════════

        // IProviderOrchestrator : Orchestration des requêtes LLM
        services.TryAddScoped<IProviderOrchestrator, ProviderOrchestrator>();

        return services;
    }

    /// <summary>
    /// Ajoute un service de métriques personnalisé pour les providers.
    /// </summary>
    /// <typeparam name="TMetricsService">Type du service de métriques.</typeparam>
    /// <param name="services">Collection de services.</param>
    /// <returns>La collection de services pour le chaînage.</returns>
    public static IServiceCollection AddProviderMetricsService<TMetricsService>(
        this IServiceCollection services)
        where TMetricsService : class, IProviderMetricsService
    {
        services.AddSingleton<IProviderMetricsService, TMetricsService>();
        return services;
    }
}
