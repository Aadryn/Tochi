using LLMProxy.Domain.Interfaces;
using LLMProxy.Domain.LLM;
using LLMProxy.Infrastructure.LLMProviders.Configuration;
using LLMProxy.Infrastructure.LLMProviders.Resilience;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

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
    /// Ajoute les services de providers LLM multi-providers au conteneur de DI.
    /// </summary>
    /// <param name="services">Collection de services.</param>
    /// <param name="configuration">Configuration de l'application.</param>
    /// <returns>La collection de services pour chaînage.</returns>
    public static IServiceCollection AddLLMProviders(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Enregistrer la configuration
        services.Configure<LLMProvidersOptions>(configuration.GetSection("LLMProviders"));

        var options = new LLMProvidersOptions();
        configuration.GetSection("LLMProviders").Bind(options);

        // Configurer les HttpClients pour chaque provider
        if (options.Providers != null)
        {
            foreach (var provider in options.Providers.Where(p => p.IsEnabled))
            {
                ConfigureHttpClientForProvider(services, provider);
            }
        }

        // Enregistrer la factory
        services.AddSingleton<ILLMProviderClientFactory, LLMProviderClientFactory>();

        return services;
    }

    /// <summary>
    /// Ajoute les services de providers LLM avec configuration manuelle.
    /// </summary>
    /// <param name="services">Collection de services.</param>
    /// <param name="configure">Action de configuration.</param>
    /// <returns>La collection de services pour chaînage.</returns>
    public static IServiceCollection AddLLMProviders(
        this IServiceCollection services,
        Action<LLMProvidersOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new LLMProvidersOptions();
        configure(options);

        services.Configure<LLMProvidersOptions>(opt =>
        {
            opt.Providers = options.Providers;
            opt.Routing = options.Routing;
            opt.DefaultTimeoutSeconds = options.DefaultTimeoutSeconds;
            opt.DefaultMaxRetries = options.DefaultMaxRetries;
        });

        // Configurer les HttpClients
        if (options.Providers != null)
        {
            foreach (var provider in options.Providers.Where(p => p.IsEnabled))
            {
                ConfigureHttpClientForProvider(services, provider);
            }
        }

        // Enregistrer la factory
        services.AddSingleton<ILLMProviderClientFactory, LLMProviderClientFactory>();

        return services;
    }

    /// <summary>
    /// Configure un HttpClient pour un provider spécifique.
    /// </summary>
    private static void ConfigureHttpClientForProvider(
        IServiceCollection services,
        LLMProviderConfiguration provider)
    {
        var builder = services.AddHttpClient(provider.Name, client =>
        {
            if (!string.IsNullOrEmpty(provider.BaseUrl))
            {
                client.BaseAddress = new Uri(provider.BaseUrl);
            }

            // Timeouts
            var timeout = provider.TimeoutSeconds > 0 ? provider.TimeoutSeconds : 120;
            client.Timeout = TimeSpan.FromSeconds(timeout);

            // Headers communs
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "LLMProxy/1.0");
        });

        // Ajouter les politiques de résilience Polly
        builder
            .AddPolicyHandler(GetRetryPolicy(provider))
            .AddPolicyHandler(GetCircuitBreakerPolicy());
    }

    /// <summary>
    /// Crée une politique de retry avec exponential backoff.
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(LLMProviderConfiguration provider)
    {
        var maxRetries = provider.MaxRetries ?? 3;

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                maxRetries,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    /// <summary>
    /// Crée une politique de circuit breaker.
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30));
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

