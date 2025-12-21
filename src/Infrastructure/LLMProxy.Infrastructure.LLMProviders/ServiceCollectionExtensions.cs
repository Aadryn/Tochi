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
    /// Enregistre l'infrastructure LLM providers avec circuit breaker.
    /// Conforme à ADR-032 (Circuit Breaker Pattern).
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

        // HttpClient factory avec circuit breaker par provider
        ConfigureHttpClientsWithCircuitBreaker(services, circuitBreakerOptions);

        return services;
    }

    /// <summary>
    /// Configure les HttpClient pour chaque provider LLM avec circuit breaker isolé.
    /// </summary>
    private static void ConfigureHttpClientsWithCircuitBreaker(
        IServiceCollection services,
        CircuitBreakerOptions options)
    {
        // OpenAI provider avec circuit breaker
        services.AddHttpClient("OpenAI", client =>
        {
            client.BaseAddress = new Uri("https://api.openai.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddCircuitBreakerPolicy(
            "OpenAI",
            options,
            services.BuildServiceProvider().GetRequiredService<ILogger<object>>());

        // Anthropic provider avec circuit breaker isolé
        services.AddHttpClient("Anthropic", client =>
        {
            client.BaseAddress = new Uri("https://api.anthropic.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddCircuitBreakerPolicy(
            "Anthropic",
            options,
            services.BuildServiceProvider().GetRequiredService<ILogger<object>>());

        // Ollama provider local avec circuit breaker
        services.AddHttpClient("Ollama", client =>
        {
            client.BaseAddress = new Uri("http://localhost:11434/");
            client.Timeout = TimeSpan.FromSeconds(60);
        })
        .AddCircuitBreakerPolicy(
            "Ollama",
            options,
            services.BuildServiceProvider().GetRequiredService<ILogger<object>>());
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

