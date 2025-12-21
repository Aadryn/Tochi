using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;

namespace LLMProxy.Infrastructure.LLMProviders.Resilience;

/// <summary>
/// Extensions pour configurer le pattern Circuit Breaker avec Polly sur HttpClient.
/// Conforme à ADR-032 (Circuit Breaker Pattern).
/// </summary>
public static partial class HttpClientCircuitBreakerExtensions
{
    /// <summary>
    /// Ajoute une politique de Circuit Breaker à un HttpClient.
    /// Le circuit s'ouvre après un seuil d'échecs, bloque les requêtes pendant une durée,
    /// puis teste la récupération avant de se refermer.
    /// </summary>
    /// <param name="builder">Builder HttpClient à configurer.</param>
    /// <param name="providerName">Nom du provider LLM (pour logs et isolation).</param>
    /// <param name="options">Options de configuration du circuit breaker.</param>
    /// <param name="logger">Logger pour traçabilité des changements d'état.</param>
    public static void AddCircuitBreakerPolicy(
        this IHttpClientBuilder builder,
        string providerName,
        Configuration.CircuitBreakerOptions options,
        ILogger logger)
    {
        // Polly v8 - Standard HttpClient Resilience
        builder.AddStandardResilienceHandler(config =>
        {
            // Circuit Breaker configuration
            config.CircuitBreaker.FailureRatio = options.FailureThreshold;
            config.CircuitBreaker.MinimumThroughput = options.MinimumThroughput;
            config.CircuitBreaker.SamplingDuration = options.SamplingDuration;
            config.CircuitBreaker.BreakDuration = options.DurationOfBreak;
            
            // Désactiver le retry (géré séparément si nécessaire)
            config.Retry.MaxRetryAttempts = 0;
            
            // Événements pour logs structurés
            config.CircuitBreaker.OnOpened = args =>
            {
                LogCircuitBreakerOpened(
                    logger,
                    providerName,
                    options.DurationOfBreak.TotalSeconds);
                return ValueTask.CompletedTask;
            };
            
            config.CircuitBreaker.OnClosed = args =>
            {
                LogCircuitBreakerClosed(logger, providerName);
                return ValueTask.CompletedTask;
            };
            
            config.CircuitBreaker.OnHalfOpened = args =>
            {
                LogCircuitBreakerHalfOpened(logger, providerName);
                return ValueTask.CompletedTask;
            };
        });
    }

    [LoggerMessage(
        EventId = 5001,
        Level = LogLevel.Warning,
        Message = "Circuit breaker OPENED for provider {ProviderName} - Blocking requests for {DurationSeconds}s")]
    private static partial void LogCircuitBreakerOpened(
        ILogger logger,
        string providerName,
        double durationSeconds);

    [LoggerMessage(
        EventId = 5002,
        Level = LogLevel.Information,
        Message = "Circuit breaker CLOSED for provider {ProviderName} - Resuming normal operation")]
    private static partial void LogCircuitBreakerClosed(
        ILogger logger,
        string providerName);

    [LoggerMessage(
        EventId = 5003,
        Level = LogLevel.Information,
        Message = "Circuit breaker HALF-OPENED for provider {ProviderName} - Testing recovery with limited requests")]
    private static partial void LogCircuitBreakerHalfOpened(
        ILogger logger,
        string providerName);
}
