using LLMProxy.Application.BackgroundServices;
using LLMProxy.Application.Common.EventPublishing;
using LLMProxy.Application.Configuration;
using LLMProxy.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Application.Extensions;

/// <summary>
/// Extensions pour configurer l'Outbox Pattern dans le conteneur DI.
/// </summary>
/// <remarks>
/// Conforme à l'ADR-040 Outbox Pattern.
/// </remarks>
public static class OutboxServiceCollectionExtensions
{
    /// <summary>
    /// Ajoute les services de l'Outbox Pattern au conteneur DI.
    /// </summary>
    /// <param name="services">Collection de services.</param>
    /// <param name="configure">Action de configuration des options (optionnel).</param>
    /// <returns>Collection de services pour chaînage.</returns>
    /// <example>
    /// <code>
    /// services.AddOutboxPattern(options =>
    /// {
    ///     options.PollingInterval = TimeSpan.FromSeconds(10);
    ///     options.BatchSize = 50;
    ///     options.MaxRetries = 5;
    ///     options.RetentionPeriod = TimeSpan.FromDays(30);
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddOutboxPattern(
        this IServiceCollection services,
        Action<OutboxOptions>? configure = null)
    {
        // Configurer les options
        var options = new OutboxOptions();
        configure?.Invoke(options);
        options.Validate(); // Valider la configuration

        services.AddSingleton(options);

        // Publisher d'événements (MediatR par défaut)
        services.AddScoped<IEventPublisher, MediatREventPublisher>();

        // OutboxProcessor (traitement principal des messages)
        services.AddHostedService(sp =>
            new OutboxProcessor(
                sp,
                sp.GetRequiredService<ILogger<OutboxProcessor>>(),
                options.PollingInterval,
                options.BatchSize));

        // OutboxCleanupService (nettoyage des messages traités)
        if (options.EnableCleanup)
        {
            services.AddHostedService(sp =>
                new OutboxCleanupService(
                    sp,
                    sp.GetRequiredService<ILogger<OutboxCleanupService>>(),
                    options.RetentionPeriod,
                    options.CleanupInterval));
        }

        // OutboxDeadLetterService (gestion des messages en échec)
        if (options.EnableDeadLetter)
        {
            services.AddHostedService(sp =>
                new OutboxDeadLetterService(
                    sp,
                    sp.GetRequiredService<ILogger<OutboxDeadLetterService>>(),
                    options.MaxRetries,
                    options.DeadLetterCheckInterval));
        }

        return services;
    }
}
