using System.Diagnostics;
using System.Runtime.CompilerServices;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.LLM;
using LLMProxy.Infrastructure.LLMProviders.Configuration;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.LLMProviders.Providers;

/// <summary>
/// Classe de base abstraite pour les implémentations de providers LLM.
/// Fournit la gestion commune du logging, des erreurs et des métriques.
/// </summary>
public abstract class LLMProviderClientBase : ILLMProviderClient, IDisposable
{
    /// <summary>
    /// Client HTTP pour les requêtes API.
    /// </summary>
    protected readonly HttpClient HttpClient;

    /// <summary>
    /// Configuration du provider.
    /// </summary>
    protected readonly LLMProviderConfiguration Configuration;

    /// <summary>
    /// Logger pour ce provider.
    /// </summary>
    protected readonly ILogger Logger;

    private bool _disposed;

    /// <summary>
    /// Type du provider.
    /// </summary>
    public abstract ProviderType Type { get; }

    /// <summary>
    /// Nom d'affichage du provider.
    /// </summary>
    public string Name => Configuration.Name;

    /// <summary>
    /// Constructeur de base.
    /// </summary>
    /// <param name="httpClient">Client HTTP configuré.</param>
    /// <param name="configuration">Configuration du provider.</param>
    /// <param name="logger">Logger.</param>
    protected LLMProviderClientBase(
        HttpClient httpClient,
        LLMProviderConfiguration configuration,
        ILogger logger)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ConfigureHttpClient();
    }

    /// <summary>
    /// Configure le client HTTP avec les headers et le timeout.
    /// </summary>
    protected virtual void ConfigureHttpClient()
    {
        HttpClient.Timeout = TimeSpan.FromSeconds(Configuration.TimeoutSeconds);

        if (!string.IsNullOrEmpty(Configuration.ApiKey))
        {
            var apiKey = ResolveApiKey(Configuration.ApiKey);
            SetAuthorizationHeader(apiKey);
        }

        if (Configuration.CustomHeaders != null)
        {
            foreach (var header in Configuration.CustomHeaders)
            {
                HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
    }

    /// <summary>
    /// Définit le header d'autorisation. Peut être surchargé par provider.
    /// </summary>
    /// <param name="apiKey">Clé API résolue.</param>
    protected virtual void SetAuthorizationHeader(string apiKey)
    {
        HttpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
    }

    /// <summary>
    /// Résout la clé API depuis une variable d'environnement si nécessaire.
    /// </summary>
    /// <param name="apiKeyValue">Valeur de la clé (peut être ${VAR_NAME}).</param>
    /// <returns>Clé API résolue.</returns>
    protected static string ResolveApiKey(string apiKeyValue)
    {
        if (string.IsNullOrEmpty(apiKeyValue))
            return string.Empty;

        if (apiKeyValue.StartsWith("${") && apiKeyValue.EndsWith("}"))
        {
            var envVarName = apiKeyValue[2..^1];
            return Environment.GetEnvironmentVariable(envVarName) ?? string.Empty;
        }

        return apiKeyValue;
    }

    /// <inheritdoc />
    public abstract Task<ProviderCapabilities> GetCapabilitiesAsync(
        CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract Task<IReadOnlyList<LLMModel>> ListModelsAsync(
        CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract Task<bool> IsHealthyAsync(
        CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public async Task<LLMResponse> ChatCompletionAsync(
        LLMRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        Logger.LogDebug(
            "Début requête ChatCompletion vers {Provider} avec modèle {Model}",
            Name,
            request.Model.Value);

        try
        {
            var response = await ExecuteChatCompletionAsync(request, cancellationToken);
            stopwatch.Stop();

            Logger.LogInformation(
                "Requête ChatCompletion terminée vers {Provider} en {Duration}ms. Tokens: {InputTokens}/{OutputTokens}",
                Name,
                stopwatch.ElapsedMilliseconds,
                response.Usage.InputTokens,
                response.Usage.OutputTokens);

            return response with
            {
                ProviderName = Name,
                Duration = stopwatch.Elapsed
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Logger.LogError(
                ex,
                "Erreur requête ChatCompletion vers {Provider} après {Duration}ms",
                Name,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Exécute la requête de chat completion spécifique au provider.
    /// </summary>
    protected abstract Task<LLMResponse> ExecuteChatCompletionAsync(
        LLMRequest request,
        CancellationToken cancellationToken);

    /// <inheritdoc />
    public async IAsyncEnumerable<LLMResponse> ChatCompletionStreamAsync(
        LLMRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var chunkIndex = 0;

        Logger.LogDebug(
            "Début requête ChatCompletion (stream) vers {Provider} avec modèle {Model}",
            Name,
            request.Model.Value);

        await foreach (var chunk in ExecuteChatCompletionStreamAsync(request, cancellationToken))
        {
            yield return chunk with
            {
                ProviderName = Name,
                IsStreamChunk = true,
                StreamIndex = chunkIndex++
            };
        }

        stopwatch.Stop();
        Logger.LogInformation(
            "Streaming terminé depuis {Provider} en {Duration}ms. {ChunkCount} chunks",
            Name,
            stopwatch.ElapsedMilliseconds,
            chunkIndex);
    }

    /// <summary>
    /// Exécute le streaming de chat completion spécifique au provider.
    /// </summary>
    protected abstract IAsyncEnumerable<LLMResponse> ExecuteChatCompletionStreamAsync(
        LLMRequest request,
        CancellationToken cancellationToken);

    /// <inheritdoc />
    public async Task<EmbeddingResponse> EmbeddingsAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        Logger.LogDebug(
            "Début requête Embeddings vers {Provider} avec modèle {Model}. {Count} inputs",
            Name,
            request.Model.Value,
            request.Inputs.Count);

        try
        {
            var response = await ExecuteEmbeddingsAsync(request, cancellationToken);
            stopwatch.Stop();

            Logger.LogInformation(
                "Requête Embeddings terminée vers {Provider} en {Duration}ms. {Count} embeddings générés",
                Name,
                stopwatch.ElapsedMilliseconds,
                response.Embeddings.Count);

            return response with
            {
                ProviderName = Name,
                Duration = stopwatch.Elapsed
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Logger.LogError(
                ex,
                "Erreur requête Embeddings vers {Provider} après {Duration}ms",
                Name,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Exécute la requête d'embeddings spécifique au provider.
    /// </summary>
    protected abstract Task<EmbeddingResponse> ExecuteEmbeddingsAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Obtient le modèle à utiliser (celui de la requête ou le défaut).
    /// </summary>
    protected ModelIdentifier GetModelOrDefault(ModelIdentifier? requestModel)
    {
        if (requestModel != null)
            return requestModel;

        if (!string.IsNullOrEmpty(Configuration.DefaultModel))
            return ModelIdentifier.FromValid(Configuration.DefaultModel);

        throw new InvalidOperationException(
            $"Aucun modèle spécifié et pas de modèle par défaut configuré pour {Name}");
    }

    /// <summary>
    /// Libère les ressources.
    /// </summary>
    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Libère les ressources.
    /// </summary>
    /// <param name="disposing">True si appelé depuis Dispose.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // Le HttpClient est géré par le HttpClientFactory, ne pas le disposer
        }

        _disposed = true;
    }
}
