// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// LLMProxy - Extension pour l'enregistrement des services Application Layer
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using FluentValidation;
using LLMProxy.Application.Common.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LLMProxy.Application.Extensions;

/// <summary>
/// Extensions pour l'injection de dépendances des services de la couche Application.
/// Configure MediatR, FluentValidation et les Pipeline Behaviors.
/// </summary>
/// <remarks>
/// <para>
/// Cette classe fournit une méthode d'extension unique pour configurer
/// tous les services de la couche Application, conformément à l'ADR-007 (Vertical Slice)
/// et l'ADR-014 (Dependency Injection).
/// </para>
/// <para>
/// Les Pipeline Behaviors sont enregistrés dans l'ordre suivant :
/// </para>
/// <list type="number">
/// <item><description><b>LoggingBehavior</b> : Log entrée/sortie de chaque requête</description></item>
/// <item><description><b>ValidationBehavior</b> : Valide la requête avant le handler</description></item>
/// <item><description><b>PerformanceBehavior</b> : Collecte les métriques de performance</description></item>
/// <item><description><b>TransactionBehavior</b> : Gère la transaction pour les Commands</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Dans Program.cs
/// builder.Services.AddApplicationServices();
/// </code>
/// </example>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Marqueur d'assembly pour la découverte automatique des handlers et validateurs.
    /// </summary>
    public sealed class ApplicationAssemblyMarker { }

    /// <summary>
    /// Ajoute tous les services de la couche Application au conteneur DI.
    /// </summary>
    /// <param name="services">Collection de services.</param>
    /// <returns>La collection de services pour le chaînage.</returns>
    /// <remarks>
    /// <para>Configure :</para>
    /// <list type="bullet">
    /// <item><description>MediatR avec découverte automatique des handlers</description></item>
    /// <item><description>FluentValidation avec découverte automatique des validateurs</description></item>
    /// <item><description>Pipeline Behaviors pour cross-cutting concerns</description></item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = typeof(ApplicationAssemblyMarker).Assembly;

        // ═══════════════════════════════════════════════════════════════
        // MEDIATR (ADR-013 CQRS)
        // ═══════════════════════════════════════════════════════════════
        services.AddMediatR(cfg =>
        {
            // Découverte automatique des handlers dans l'assembly Application
            cfg.RegisterServicesFromAssembly(assembly);

            // Pipeline Behaviors - ORDRE IMPORTANT !
            // 1. Logging : Premier pour capturer toute la durée
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

            // 2. Validation : Échouer tôt si la requête est invalide
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            // 3. Caching : Après validation, avant performance/transaction (ADR-042)
            cfg.AddOpenBehavior(typeof(CachingBehavior<,>));

            // 4. Performance : Métriques après validation (éviter les métriques sur requêtes invalides)
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

            // 5. Transaction : Dernière avant le handler pour les Commands
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

            // 6. Cache Invalidation : APRÈS Transaction pour garantir commit DB avant invalidation (ADR-042)
            cfg.AddOpenBehavior(typeof(CacheInvalidationBehavior<,>));
        });

        // ═══════════════════════════════════════════════════════════════
        // FLUENT VALIDATION (ADR-018 Guard Clauses)
        // ═══════════════════════════════════════════════════════════════
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
