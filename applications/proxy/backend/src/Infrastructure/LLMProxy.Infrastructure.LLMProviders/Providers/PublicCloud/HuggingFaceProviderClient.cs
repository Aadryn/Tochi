using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.LLM;
using LLMProxy.Infrastructure.LLMProviders.Configuration;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud;

/// <summary>
/// Implémentation du client HuggingFace Inference API.
/// Supporte les modèles hébergés sur l'infrastructure HuggingFace.
/// </summary>
public sealed class HuggingFaceProviderClient : LLMProviderClientBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <inheritdoc />
    public override ProviderType Type => ProviderType.HuggingFace;

    /// <summary>
    /// Constructeur.
    /// </summary>
    public HuggingFaceProviderClient(
        HttpClient httpClient,
        LLMProviderConfiguration configuration,
        ILogger<HuggingFaceProviderClient> logger)
        : base(httpClient, configuration, logger)
    {
        if (string.IsNullOrEmpty(HttpClient.BaseAddress?.ToString()))
        {
            HttpClient.BaseAddress = new Uri(
                configuration.BaseUrl ?? "https://api-inference.huggingface.co");
        }
    }

    /// <inheritdoc />
    protected override void SetAuthorizationHeader(string apiKey)
    {
        HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    /// <inheritdoc />
    public override Task<ProviderCapabilities> GetCapabilitiesAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ProviderCapabilities
        {
            Type = ProviderType.HuggingFace,
            Name = Name,
            SupportedCapabilities = ModelCapabilities.ChatCompletion
                                    | ModelCapabilities.Streaming
                                    | ModelCapabilities.Embeddings,
            SupportsStreaming = true,
            SupportsEmbeddings = true,
            SupportsFunctionCalling = false,
            SupportsVision = true, // Certains modèles
            IsHealthy = true,
            LastHealthCheck = DateTimeOffset.UtcNow
        });
    }

    /// <inheritdoc />
    public override Task<IReadOnlyList<LLMModel>> ListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        // Modèles populaires disponibles via Inference API
        var models = new List<LLMModel>
        {
            new()
            {
                Id = ModelIdentifier.FromValid("meta-llama/Llama-3.3-70B-Instruct"),
                Name = "Llama 3.3 70B Instruct",
                OwnedBy = "Meta",
                Capabilities = ModelCapabilities.ChatCompletion | ModelCapabilities.Streaming,
                IsAvailable = true,
                ContextLength = 128000
            },
            new()
            {
                Id = ModelIdentifier.FromValid("mistralai/Mixtral-8x7B-Instruct-v0.1"),
                Name = "Mixtral 8x7B Instruct",
                OwnedBy = "Mistral AI",
                Capabilities = ModelCapabilities.ChatCompletion | ModelCapabilities.Streaming,
                IsAvailable = true,
                ContextLength = 32768
            },
            new()
            {
                Id = ModelIdentifier.FromValid("Qwen/Qwen2.5-72B-Instruct"),
                Name = "Qwen 2.5 72B Instruct",
                OwnedBy = "Alibaba",
                Capabilities = ModelCapabilities.ChatCompletion | ModelCapabilities.Streaming,
                IsAvailable = true,
                ContextLength = 128000
            },
            new()
            {
                Id = ModelIdentifier.FromValid("microsoft/Phi-3-medium-128k-instruct"),
                Name = "Phi-3 Medium 128K",
                OwnedBy = "Microsoft",
                Capabilities = ModelCapabilities.ChatCompletion | ModelCapabilities.Streaming,
                IsAvailable = true,
                ContextLength = 128000
            },
            new()
            {
                Id = ModelIdentifier.FromValid("sentence-transformers/all-MiniLM-L6-v2"),
                Name = "All-MiniLM-L6-v2",
                OwnedBy = "Sentence Transformers",
                Capabilities = ModelCapabilities.Embeddings,
                IsAvailable = true,
                OutputDimension = 384
            },
            new()
            {
                Id = ModelIdentifier.FromValid("BAAI/bge-large-en-v1.5"),
                Name = "BGE Large EN v1.5",
                OwnedBy = "BAAI",
                Capabilities = ModelCapabilities.Embeddings,
                IsAvailable = true,
                OutputDimension = 1024
            }
        };

        return Task.FromResult<IReadOnlyList<LLMModel>>(models);
    }

    /// <inheritdoc />
    public override async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Vérifier l'API status
            var response = await HttpClient.GetAsync(
                "/status", cancellationToken);
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
        var model = request.Model.Value;

        // HuggingFace Inference API supporte le format Messages API
        var uri = $"/models/{model}/v1/chat/completions";

        var hfRequest = MapToHuggingFaceRequest(request);

        var response = await HttpClient.PostAsJsonAsync(uri, hfRequest, JsonOptions, cancellationToken);

        // Vérifier si le modèle est en train de charger
        if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
        {
            var error = await response.Content.ReadFromJsonAsync<HuggingFaceError>(
                JsonOptions, cancellationToken);

            if (error?.Error?.Contains("loading") == true)
            {
                // Le modèle est en train de charger, attendre et réessayer
                Logger.LogInformation("Modèle HuggingFace en cours de chargement, attente...");
                await Task.Delay(5000, cancellationToken);
                response = await HttpClient.PostAsJsonAsync(uri, hfRequest, JsonOptions, cancellationToken);
            }
        }

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<HuggingFaceChatResponse>(
            JsonOptions, cancellationToken);

        return MapToLLMResponse(result!, request.Model);
    }

    /// <inheritdoc />
    protected override async IAsyncEnumerable<LLMResponse> ExecuteChatCompletionStreamAsync(
        LLMRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var model = request.Model.Value;
        var uri = $"/models/{model}/v1/chat/completions";

        var hfRequest = MapToHuggingFaceRequest(request) with { Stream = true };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = JsonContent.Create(hfRequest, options: JsonOptions)
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
            if (string.IsNullOrEmpty(line) || !line.StartsWith("data: "))
                continue;

            var json = line["data: ".Length..];
            if (json == "[DONE]")
                break;

            var chunk = JsonSerializer.Deserialize<HuggingFaceStreamChunk>(json, JsonOptions);
            if (chunk?.Choices?.Length > 0)
            {
                var content = chunk.Choices[0].Delta?.Content;
                if (!string.IsNullOrEmpty(content))
                {
                    yield return new LLMResponse
                    {
                        Id = chunk.Id ?? Guid.NewGuid().ToString(),
                        Model = request.Model,
                        Content = content,
                        FinishReason = MapFinishReason(chunk.Choices[0].FinishReason),
                        CreatedAt = DateTimeOffset.UtcNow
                    };
                }
            }
        }
    }

    /// <inheritdoc />
    protected override async Task<EmbeddingResponse> ExecuteEmbeddingsAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken)
    {
        var model = request.Model.Value;
        var uri = $"/pipeline/feature-extraction/{model}";

        var embeddings = new List<Embedding>();

        // HuggingFace traite les embeddings en batch
        var hfRequest = new HuggingFaceEmbeddingRequest
        {
            Inputs = request.Inputs.ToList(),
            Options = new HuggingFaceOptions
            {
                WaitForModel = true
            }
        };

        var response = await HttpClient.PostAsJsonAsync(uri, hfRequest, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<float[][]>(JsonOptions, cancellationToken);

        if (result != null)
        {
            for (var i = 0; i < result.Length; i++)
            {
                embeddings.Add(new Embedding
                {
                    Index = i,
                    Vector = result[i].ToList()
                });
            }
        }

        return new EmbeddingResponse
        {
            Model = request.Model,
            Embeddings = embeddings,
            Usage = TokenUsage.FromValid(request.Inputs.Count * 100, 0) // Estimation
        };
    }

    #region Mapping

    private HuggingFaceChatRequest MapToHuggingFaceRequest(LLMRequest request)
    {
        return new HuggingFaceChatRequest
        {
            Model = request.Model.Value,
            Messages = request.Messages.Select(m => new HuggingFaceMessage
            {
                Role = m.Role.ToString().ToLowerInvariant(),
                Content = m.Content
            }).ToList(),
            Temperature = (float?)(request.Temperature),
            MaxTokens = request.MaxTokens ?? Configuration.MaxTokens,
            TopP = (float?)(request.TopP),
            Stop = request.StopSequences?.ToList(),
            RepetitionPenalty = request.FrequencyPenalty.HasValue
                ? 1.0f + (float)request.FrequencyPenalty.Value
                : null
        };
    }

    private static LLMResponse MapToLLMResponse(
        HuggingFaceChatResponse response,
        ModelIdentifier requestModel)
    {
        var choice = response.Choices?.FirstOrDefault();

        return new LLMResponse
        {
            Id = response.Id ?? Guid.NewGuid().ToString(),
            Model = requestModel,
            Content = choice?.Message?.Content ?? string.Empty,
            FinishReason = MapFinishReason(choice?.FinishReason),
            Usage = response.Usage != null
                ? TokenUsage.FromValid(
                    response.Usage.PromptTokens ?? 0,
                    response.Usage.CompletionTokens ?? 0)
                : TokenUsage.Empty,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    private static FinishReason MapFinishReason(string? reason)
    {
        return reason?.ToLowerInvariant() switch
        {
            "stop" => FinishReason.Stop,
            "length" => FinishReason.Length,
            "eos_token" => FinishReason.Stop,
            _ => FinishReason.Stop
        };
    }

    #endregion

    #region DTOs HuggingFace

    private sealed record HuggingFaceChatRequest
    {
        public required string Model { get; init; }
        public required List<HuggingFaceMessage> Messages { get; init; }
        public float? Temperature { get; init; }
        public int? MaxTokens { get; init; }
        public float? TopP { get; init; }
        public List<string>? Stop { get; init; }
        public float? RepetitionPenalty { get; init; }
        public bool Stream { get; init; }
    }

    private sealed record HuggingFaceMessage
    {
        public required string Role { get; init; }
        public required string Content { get; init; }
    }

    private sealed record HuggingFaceChatResponse
    {
        public string? Id { get; init; }
        public HuggingFaceChoice[]? Choices { get; init; }
        public HuggingFaceUsage? Usage { get; init; }
    }

    private sealed record HuggingFaceChoice
    {
        public HuggingFaceMessage? Message { get; init; }
        public string? FinishReason { get; init; }
    }

    private sealed record HuggingFaceUsage
    {
        public int? PromptTokens { get; init; }
        public int? CompletionTokens { get; init; }
    }

    private sealed record HuggingFaceStreamChunk
    {
        public string? Id { get; init; }
        public HuggingFaceStreamChoice[]? Choices { get; init; }
    }

    private sealed record HuggingFaceStreamChoice
    {
        public HuggingFaceDelta? Delta { get; init; }
        public string? FinishReason { get; init; }
    }

    private sealed record HuggingFaceDelta
    {
        public string? Content { get; init; }
    }

    private sealed record HuggingFaceEmbeddingRequest
    {
        public required List<string> Inputs { get; init; }
        public HuggingFaceOptions? Options { get; init; }
    }

    private sealed record HuggingFaceOptions
    {
        public bool WaitForModel { get; init; }
    }

    private sealed record HuggingFaceError
    {
        public string? Error { get; init; }
        public float? EstimatedTime { get; init; }
    }

    #endregion
}
