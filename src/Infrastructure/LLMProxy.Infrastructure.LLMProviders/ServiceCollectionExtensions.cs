using LLMProxy.Domain.Interfaces;
using LLMProxy.Infrastructure.LLMProviders.Configuration;
using LLMProxy.Infrastructure.LLMProviders.Resilience;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.LLMProviders;

/// <summary>
/// Extensions pour enregistrer les services de providers LLM avec résilience.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Enregistre l'infrastructure LLM providers avec circuit breaker et retry.
    /// Conforme à ADR-032 (Circuit Breaker) et ADR-033 (Retry Pattern).
    /// </summary>
    /// <param name="services">Collection de services.</param>
    /// <param name="configuration">Configuration application.</param>
    /// <returns>Collection de services enrichie.</returns>
    public static IServiceCollection AddLLMProviderInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Token counting service
        services.AddSingleton<ITokenCounterService, TokenCounterService>();

        // Circuit Breaker configuration
        var circuitBreakerOptions = configuration
            .GetSection("CircuitBreaker")
            .Get<CircuitBreakerOptions>() ?? new CircuitBreakerOptions();
        
        services.AddSingleton(circuitBreakerOptions);

        // Retry Policy configuration
        var retryOptions = configuration
            .GetSection("RetryPolicy")
            .Get<RetryPolicyOptions>() ?? new RetryPolicyOptions();
        
        services.AddSingleton(retryOptions);

        // HttpClient factory avec résilience complète par provider
        ConfigureHttpClientsWithResilience(services, circuitBreakerOptions, retryOptions);

        return services;
    }

    /// <summary>
    /// Configure les HttpClient pour chaque provider LLM avec résilience complète (circuit breaker + retry).
    /// </summary>
    private static void ConfigureHttpClientsWithResilience(
        IServiceCollection services,
        CircuitBreakerOptions circuitBreakerOptions,
        RetryPolicyOptions retryOptions)
    {
        var logger = services.BuildServiceProvider().GetRequiredService<ILogger<object>>();

        // OpenAI provider avec résilience complète
        services.AddHttpClient("OpenAI", client =>
        {
            client.BaseAddress = new Uri("https://api.openai.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddResiliencePolicies("OpenAI", circuitBreakerOptions, retryOptions, logger);

        // Anthropic provider avec résilience complète
        services.AddHttpClient("Anthropic", client =>
        {
            client.BaseAddress = new Uri("https://api.anthropic.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddResiliencePolicies("Anthropic", circuitBreakerOptions, retryOptions, logger);

        // Ollama provider local avec résilience complète
        services.AddHttpClient("Ollama", client =>
        {
            client.BaseAddress = new Uri("http://localhost:11434/");
            client.Timeout = TimeSpan.FromSeconds(60);
        })
        .AddResiliencePolicies("Ollama", circuitBreakerOptions, retryOptions, logger);
    }

    /// <summary>
    /// Méthode legacy sans configuration - pour compatibilité.
    /// </summary>
    public static IServiceCollection AddLLMProviderInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ITokenCounterService, TokenCounterService>();
        return services;
    }
}

