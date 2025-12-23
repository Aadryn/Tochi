using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.CircuitBreaker;

namespace LLMProxy.Infrastructure.LLMProviders.Resilience;

/// <summary>
/// Extensions pour configurer les politiques de résilience (Circuit Breaker + Retry) avec Polly.
/// Conforme à ADR-032 (Circuit Breaker) et ADR-033 (Retry Pattern).
/// </summary>
public static partial class HttpClientResilienceExtensions
{
    /// <summary>
    /// Ajoute les politiques de résilience (Circuit Breaker + Retry + Timeout) à un HttpClient.
    /// </summary>
    /// <param name="builder">Builder HttpClient à configurer.</param>
    /// <param name="providerName">Nom du provider LLM (pour logs et isolation).</param>
    /// <param name="circuitBreakerOptions">Options circuit breaker.</param>
    /// <param name="retryOptions">Options retry policy.</param>
    /// <param name="logger">Logger pour traçabilité.</param>
    public static void AddResiliencePolicies(
        this IHttpClientBuilder builder,
        string providerName,
        Configuration.CircuitBreakerOptions circuitBreakerOptions,
        Configuration.RetryPolicyOptions retryOptions,
        ILogger logger)
    {
        // Polly v8 - Standard HttpClient Resilience
        builder.AddStandardResilienceHandler(config =>
        {
            // ═══ CIRCUIT BREAKER (ADR-032) ═══
            config.CircuitBreaker.FailureRatio = circuitBreakerOptions.FailureThreshold;
            config.CircuitBreaker.MinimumThroughput = circuitBreakerOptions.MinimumThroughput;
            config.CircuitBreaker.SamplingDuration = circuitBreakerOptions.SamplingDuration;
            config.CircuitBreaker.BreakDuration = circuitBreakerOptions.DurationOfBreak;
            
            config.CircuitBreaker.OnOpened = args =>
            {
                LogCircuitBreakerOpened(logger, providerName, circuitBreakerOptions.DurationOfBreak.TotalSeconds);
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

            // ═══ RETRY WITH EXPONENTIAL BACKOFF (ADR-033) ═══
            config.Retry.MaxRetryAttempts = retryOptions.MaxRetryAttempts;
            config.Retry.Delay = retryOptions.InitialDelay;
            config.Retry.BackoffType = retryOptions.BackoffType == "Exponential" 
                ? Polly.DelayBackoffType.Exponential 
                : Polly.DelayBackoffType.Constant;
            config.Retry.UseJitter = retryOptions.UseJitter;

            // Retry uniquement sur erreurs transitoires (429, 503, 408, HttpRequestException, TaskCanceledException)
            // ShouldHandle est déjà configuré par défaut pour gérer les erreurs transitoires

            config.Retry.OnRetry = args =>
            {
                var failureReason = args.Outcome.Exception?.Message 
                    ?? args.Outcome.Result?.StatusCode.ToString() 
                    ?? "Unknown";
                    
                LogRetryAttempt(
                    logger,
                    providerName,
                    args.AttemptNumber,
                    retryOptions.MaxRetryAttempts,
                    failureReason);
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

    [LoggerMessage(
        EventId = 5010,
        Level = LogLevel.Information,
        Message = "Retry attempt {AttemptNumber}/{MaxAttempts} for provider {ProviderName} - Reason: {FailureReason}")]
    private static partial void LogRetryAttempt(
        ILogger logger,
        string providerName,
        int attemptNumber,
        int maxAttempts,
        string failureReason);
}
