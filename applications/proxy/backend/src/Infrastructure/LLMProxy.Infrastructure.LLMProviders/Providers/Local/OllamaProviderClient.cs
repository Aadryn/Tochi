using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.LLM;
using LLMProxy.Infrastructure.LLMProviders.Configuration;
using LLMProxy.Infrastructure.LLMProviders.Providers.Local.Ollama.Contracts;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.LLMProviders.Providers.Local;

/// <summary>
/// Implémentation du client Ollama.
/// Supporte les modèles locaux (Llama, Mistral, etc.) avec streaming.
/// </summary>
public sealed class OllamaProviderClient : LLMProviderClientBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <inheritdoc />
    public override ProviderType Type => ProviderType.Ollama;

    /// <summary>
    /// Constructeur.
    /// </summary>
    public OllamaProviderClient(
        HttpClient httpClient,
        LLMProviderConfiguration configuration,
        ILogger<OllamaProviderClient> logger)
        : base(httpClient, configuration, logger)
    {
        if (string.IsNullOrEmpty(HttpClient.BaseAddress?.ToString()))
        {
            HttpClient.BaseAddress = new Uri(configuration.BaseUrl);
        }
    }

    /// <inheritdoc />
    protected override void SetAuthorizationHeader(string apiKey)
    {
        // Ollama n'utilise généralement pas d'authentification
        // mais peut utiliser un header personnalisé si configuré
        if (!string.IsNullOrEmpty(apiKey))
        {
            HttpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        }
    }

    /// <inheritdoc />
    public override async Task<ProviderCapabilities> GetCapabilitiesAsync(
        CancellationToken cancellationToken = default)
    {
        return new ProviderCapabilities
        {
            Type = ProviderType.Ollama,
            Name = Name,
            ApiVersion = "local",
            SupportedCapabilities = ModelCapabilities.ChatCompletion
                                    | ModelCapabilities.TextCompletion
                                    | ModelCapabilities.Streaming
                                    | ModelCapabilities.Embeddings,
            SupportsStreaming = true,
            SupportsEmbeddings = true,
            SupportsFunctionCalling = false,
            SupportsVision = true, // Certains modèles (llava, etc.)
            IsHealthy = await IsHealthyAsync(cancellationToken),
            LastHealthCheck = DateTimeOffset.UtcNow
        };
    }

    /// <inheritdoc />
    public override async Task<IReadOnlyList<LLMModel>> ListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await HttpClient.GetAsync("/api/tags", cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaModelsResponse>(
            JsonOptions, cancellationToken);

        return result?.Models?.Select(m => new LLMModel
        {
            Id = ModelIdentifier.FromValid(m.Name),
            Name = m.Name,
            OwnedBy = "local",
            ContextLength = m.Details?.ParameterSize != null
                ? InferContextLength(m.Details.ParameterSize)
                : null,
            Capabilities = ModelCapabilities.ChatCompletion
                           | ModelCapabilities.TextCompletion
                           | ModelCapabilities.Streaming
                           | (m.Name.Contains("embed") ? ModelCapabilities.Embeddings : 0)
                           | (m.Name.Contains("llava") || m.Name.Contains("vision")
                               ? ModelCapabilities.Vision
                               : 0),
            IsAvailable = true,
            Metadata = new Dictionary<string, string>
            {
                ["size"] = m.Size.ToString(),
                ["digest"] = m.Digest ?? string.Empty,
                ["modified_at"] = m.ModifiedAt?.ToString("o") ?? string.Empty
            }
        }).ToList() ?? new List<LLMModel>();
    }

    /// <inheritdoc />
    public override async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await HttpClient.GetAsync("/api/tags", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Health check failed for provider {ProviderName}", Name);
            return false;
        }
    }

    /// <inheritdoc />
    protected override async Task<LLMResponse> ExecuteChatCompletionAsync(
        LLMRequest request,
        CancellationToken cancellationToken)
    {
        var ollamaRequest = MapToOllamaRequest(request);

        var response = await HttpClient.PostAsJsonAsync(
            "/api/chat",
            ollamaRequest,
            JsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(
            JsonOptions, cancellationToken);

        return MapToLLMResponse(result!, request.Model);
    }

    /// <inheritdoc />
    protected override async IAsyncEnumerable<LLMResponse> ExecuteChatCompletionStreamAsync(
        LLMRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var ollamaRequest = MapToOllamaRequest(request) with { Stream = true };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/chat")
        {
            Content = JsonContent.Create(ollamaRequest, options: JsonOptions)
        };

        var response = await HttpClient.SendAsync(
            httpRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrEmpty(line))
                continue;

            var chunk = JsonSerializer.Deserialize<OllamaStreamChunk>(line, JsonOptions);
            if (chunk == null)
                continue;

            yield return new LLMResponse
            {
                Id = Guid.NewGuid().ToString("N"),
                Model = ModelIdentifier.FromValid(chunk.Model ?? request.Model.Value),
                Content = chunk.Message?.Content ?? string.Empty,
                FinishReason = chunk.Done ? FinishReason.Stop : FinishReason.Stop,
                Usage = chunk.Done && chunk.PromptEvalCount.HasValue
                    ? TokenUsage.FromValid(chunk.PromptEvalCount.Value, chunk.EvalCount ?? 0)
                    : TokenUsage.Empty,
                CreatedAt = DateTimeOffset.UtcNow
            };

            if (chunk.Done)
                yield break;
        }
    }

    /// <inheritdoc />
    protected override async Task<EmbeddingResponse> ExecuteEmbeddingsAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken)
    {
        var embeddings = new List<Embedding>();
        var totalTokens = 0;

        for (int i = 0; i < request.Inputs.Count; i++)
        {
            var ollamaRequest = new OllamaEmbeddingRequest
            {
                Model = request.Model.Value,
                Prompt = request.Inputs[i]
            };

            var response = await HttpClient.PostAsJsonAsync(
                "/api/embeddings",
                ollamaRequest,
                JsonOptions,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>(
                JsonOptions, cancellationToken);

            if (result?.Embedding != null)
            {
                embeddings.Add(new Embedding
                {
                    Index = i,
                    Vector = result.Embedding.ToList(),
                    Text = request.Inputs[i]
                });
            }

            // Ollama ne retourne pas de token count, on estime
            totalTokens += request.Inputs[i].Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        }

        return new EmbeddingResponse
        {
            Model = request.Model,
            Embeddings = embeddings,
            Usage = TokenUsage.FromValid(totalTokens, 0)
        };
    }

    #region Mapping

    private OllamaChatRequest MapToOllamaRequest(LLMRequest request)
    {
        return new OllamaChatRequest
        {
            Model = request.Model.Value,
            Messages = request.Messages.Select(m => new OllamaMessage
            {
                Role = m.Role.ToString().ToLowerInvariant(),
                Content = m.Content
            }).ToList(),
            Options = new OllamaOptions
            {
                Temperature = request.Temperature,
                TopP = request.TopP,
                NumPredict = request.MaxTokens ?? Configuration.MaxTokens,
                Stop = request.StopSequences?.ToList(),
                Seed = request.Seed
            },
            Stream = false
        };
    }

    private static LLMResponse MapToLLMResponse(
        OllamaChatResponse response,
        ModelIdentifier requestModel)
    {
        return new LLMResponse
        {
            Id = Guid.NewGuid().ToString("N"),
            Model = ModelIdentifier.FromValid(response.Model ?? requestModel.Value),
            Content = response.Message?.Content ?? string.Empty,
            FinishReason = response.Done ? FinishReason.Stop : FinishReason.Stop,
            Usage = TokenUsage.FromValid(
                response.PromptEvalCount ?? 0,
                response.EvalCount ?? 0),
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    private static int InferContextLength(string parameterSize)
    {
        // Estimation basique basée sur la taille du modèle
        if (parameterSize.Contains("70B", StringComparison.OrdinalIgnoreCase))
            return 8192;
        if (parameterSize.Contains("13B", StringComparison.OrdinalIgnoreCase))
            return 4096;
        if (parameterSize.Contains("7B", StringComparison.OrdinalIgnoreCase))
            return 4096;
        return 2048;
    }

    #endregion

}
