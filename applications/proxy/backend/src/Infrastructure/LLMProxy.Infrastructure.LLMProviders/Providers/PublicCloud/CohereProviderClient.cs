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
/// Implémentation du client Cohere.
/// Utilise l'API native Cohere pour chat, génération et embeddings.
/// </summary>
public sealed class CohereProviderClient : LLMProviderClientBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <inheritdoc />
    public override ProviderType Type => ProviderType.Cohere;

    /// <summary>
    /// Constructeur.
    /// </summary>
    public CohereProviderClient(
        HttpClient httpClient,
        LLMProviderConfiguration configuration,
        ILogger<CohereProviderClient> logger)
        : base(httpClient, configuration, logger)
    {
        if (string.IsNullOrEmpty(HttpClient.BaseAddress?.ToString()))
        {
            HttpClient.BaseAddress = new Uri(configuration.BaseUrl ?? "https://api.cohere.ai/v2");
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
            Type = ProviderType.Cohere,
            Name = Name,
            ApiVersion = "v2",
            SupportedCapabilities = ModelCapabilities.ChatCompletion
                                    | ModelCapabilities.Streaming
                                    | ModelCapabilities.Embeddings
                                    | ModelCapabilities.FunctionCalling,
            SupportsStreaming = true,
            SupportsEmbeddings = true,
            SupportsFunctionCalling = true,
            SupportsVision = false,
            IsHealthy = true,
            LastHealthCheck = DateTimeOffset.UtcNow
        });
    }

    /// <inheritdoc />
    public override Task<IReadOnlyList<LLMModel>> ListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = new List<LLMModel>
        {
            new()
            {
                Id = ModelIdentifier.FromValid("command-r-plus-08-2024"),
                Name = "Command R+ (08-2024)",
                OwnedBy = "Cohere",
                Capabilities = ModelCapabilities.ChatCompletion
                               | ModelCapabilities.Streaming
                               | ModelCapabilities.FunctionCalling,
                IsAvailable = true,
                ContextLength = 128000
            },
            new()
            {
                Id = ModelIdentifier.FromValid("command-r-08-2024"),
                Name = "Command R (08-2024)",
                OwnedBy = "Cohere",
                Capabilities = ModelCapabilities.ChatCompletion
                               | ModelCapabilities.Streaming
                               | ModelCapabilities.FunctionCalling,
                IsAvailable = true,
                ContextLength = 128000
            },
            new()
            {
                Id = ModelIdentifier.FromValid("command-light"),
                Name = "Command Light",
                OwnedBy = "Cohere",
                Capabilities = ModelCapabilities.ChatCompletion | ModelCapabilities.Streaming,
                IsAvailable = true
            },
            new()
            {
                Id = ModelIdentifier.FromValid("embed-english-v3.0"),
                Name = "Embed English v3.0",
                OwnedBy = "Cohere",
                Capabilities = ModelCapabilities.Embeddings,
                IsAvailable = true,
                OutputDimension = 1024
            },
            new()
            {
                Id = ModelIdentifier.FromValid("embed-multilingual-v3.0"),
                Name = "Embed Multilingual v3.0",
                OwnedBy = "Cohere",
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
            var response = await HttpClient.GetAsync("/check-api-key", cancellationToken);
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
        var cohereRequest = MapToCohereRequest(request);

        var response = await HttpClient.PostAsJsonAsync("/chat", cohereRequest, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CohereChatResponse>(
            JsonOptions, cancellationToken);

        return MapToLLMResponse(result!, request.Model);
    }

    /// <inheritdoc />
    protected override async IAsyncEnumerable<LLMResponse> ExecuteChatCompletionStreamAsync(
        LLMRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var cohereRequest = MapToCohereRequest(request) with { Stream = true };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/chat")
        {
            Content = JsonContent.Create(cohereRequest, options: JsonOptions)
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

            // Cohere utilise un format JSON par ligne
            var chunk = JsonSerializer.Deserialize<CohereStreamEvent>(line, JsonOptions);

            if (chunk?.Type == "content-delta")
            {
                var content = chunk.Delta?.Message?.Content?.Text;
                if (!string.IsNullOrEmpty(content))
                {
                    yield return new LLMResponse
                    {
                        Id = Guid.NewGuid().ToString(),
                        Model = request.Model,
                        Content = content,
                        FinishReason = FinishReason.Stop,
                        CreatedAt = DateTimeOffset.UtcNow
                    };
                }
            }
            else if (chunk?.Type == "message-end")
            {
                yield return new LLMResponse
                {
                    Id = chunk.Response?.Id ?? Guid.NewGuid().ToString(),
                    Model = request.Model,
                    Content = string.Empty,
                    FinishReason = MapFinishReason(chunk.Response?.FinishReason),
                    Usage = chunk.Response?.Meta?.Tokens != null
                        ? TokenUsage.FromValid(
                            chunk.Response.Meta.Tokens.InputTokens ?? 0,
                            chunk.Response.Meta.Tokens.OutputTokens ?? 0)
                        : TokenUsage.Empty,
                    CreatedAt = DateTimeOffset.UtcNow
                };
            }
        }
    }

    /// <inheritdoc />
    protected override async Task<EmbeddingResponse> ExecuteEmbeddingsAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken)
    {
        var cohereRequest = new CohereEmbedRequest
        {
            Model = request.Model.Value,
            Texts = request.Inputs.ToList(),
            InputType = "search_document",
            EmbeddingTypes = new[] { "float" }
        };

        var response = await HttpClient.PostAsJsonAsync("/embed", cohereRequest, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CohereEmbedResponse>(
            JsonOptions, cancellationToken);

        var embeddings = new List<Embedding>();
        if (result?.Embeddings?.Float != null)
        {
            for (var i = 0; i < result.Embeddings.Float.Length; i++)
            {
                embeddings.Add(new Embedding
                {
                    Index = i,
                    Vector = result.Embeddings.Float[i].ToList()
                });
            }
        }

        return new EmbeddingResponse
        {
            Model = request.Model,
            Embeddings = embeddings,
            Usage = result?.Meta?.BilledUnits != null
                ? TokenUsage.FromValid(result.Meta.BilledUnits.InputTokens ?? 0, 0)
                : TokenUsage.Empty
        };
    }

    #region Mapping

    private CohereChatRequest MapToCohereRequest(LLMRequest request)
    {
        var messages = new List<CohereMessage>();
        string? preamble = null;

        foreach (var message in request.Messages)
        {
            if (message.Role == MessageRole.System)
            {
                // Cohere utilise preamble pour les instructions système
                preamble = message.Content;
            }
            else
            {
                messages.Add(new CohereMessage
                {
                    Role = MapRole(message.Role),
                    Content = message.Content
                });
            }
        }

        return new CohereChatRequest
        {
            Model = request.Model.Value,
            Messages = messages,
            Preamble = preamble,
            Temperature = (float?)(request.Temperature),
            MaxTokens = request.MaxTokens ?? Configuration.MaxTokens,
            P = (float?)(request.TopP),
            K = request.TopK,
            StopSequences = request.StopSequences?.ToList(),
            FrequencyPenalty = (float?)(request.FrequencyPenalty),
            PresencePenalty = (float?)(request.PresencePenalty)
        };
    }

    private static string MapRole(MessageRole role)
    {
        return role switch
        {
            MessageRole.User => "user",
            MessageRole.Assistant => "assistant",
            MessageRole.Tool => "tool",
            _ => "user"
        };
    }

    private static LLMResponse MapToLLMResponse(CohereChatResponse response, ModelIdentifier requestModel)
    {
        return new LLMResponse
        {
            Id = response.Id,
            Model = requestModel,
            Content = response.Message?.Content?.FirstOrDefault()?.Text ?? string.Empty,
            FinishReason = MapFinishReason(response.FinishReason),
            Usage = response.Usage != null
                ? TokenUsage.FromValid(
                    response.Usage.Tokens?.InputTokens ?? 0,
                    response.Usage.Tokens?.OutputTokens ?? 0)
                : TokenUsage.Empty,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    private static FinishReason MapFinishReason(string? reason)
    {
        return reason?.ToUpperInvariant() switch
        {
            "COMPLETE" => FinishReason.Stop,
            "MAX_TOKENS" => FinishReason.Length,
            "STOP_SEQUENCE" => FinishReason.Stop,
            "TOOL_CALL" => FinishReason.ToolCalls,
            "ERROR" => FinishReason.Stop,
            _ => FinishReason.Stop
        };
    }

    #endregion

    #region DTOs Cohere

    private sealed record CohereChatRequest
    {
        public required string Model { get; init; }
        public required List<CohereMessage> Messages { get; init; }
        public string? Preamble { get; init; }
        public float? Temperature { get; init; }
        public int? MaxTokens { get; init; }
        public float? P { get; init; }
        public int? K { get; init; }
        public List<string>? StopSequences { get; init; }
        public float? FrequencyPenalty { get; init; }
        public float? PresencePenalty { get; init; }
        public bool Stream { get; init; }
    }

    private sealed record CohereMessage
    {
        public required string Role { get; init; }
        public required string Content { get; init; }
    }

    private sealed record CohereChatResponse
    {
        public required string Id { get; init; }
        public CohereMessageContent? Message { get; init; }
        public string? FinishReason { get; init; }
        public CohereUsage? Usage { get; init; }
    }

    private sealed record CohereMessageContent
    {
        public List<CohereContentBlock>? Content { get; init; }
    }

    private sealed record CohereContentBlock
    {
        public string? Type { get; init; }
        public string? Text { get; init; }
    }

    private sealed record CohereUsage
    {
        public CohereTokens? Tokens { get; init; }
    }

    private sealed record CohereTokens
    {
        public int? InputTokens { get; init; }
        public int? OutputTokens { get; init; }
    }

    private sealed record CohereStreamEvent
    {
        public string? Type { get; init; }
        public CohereDelta? Delta { get; init; }
        public CohereStreamResponse? Response { get; init; }
    }

    private sealed record CohereDelta
    {
        public CohereMessageDelta? Message { get; init; }
    }

    private sealed record CohereMessageDelta
    {
        public CohereContentDelta? Content { get; init; }
    }

    private sealed record CohereContentDelta
    {
        public string? Text { get; init; }
    }

    private sealed record CohereStreamResponse
    {
        public string? Id { get; init; }
        public string? FinishReason { get; init; }
        public CohereMeta? Meta { get; init; }
    }

    private sealed record CohereMeta
    {
        public CohereTokens? Tokens { get; init; }
    }

    private sealed record CohereEmbedRequest
    {
        public required string Model { get; init; }
        public required List<string> Texts { get; init; }
        public required string InputType { get; init; }
        public required string[] EmbeddingTypes { get; init; }
    }

    private sealed record CohereEmbedResponse
    {
        public CohereEmbeddingResult? Embeddings { get; init; }
        public CohereEmbedMeta? Meta { get; init; }
    }

    private sealed record CohereEmbeddingResult
    {
        public float[][]? Float { get; init; }
    }

    private sealed record CohereEmbedMeta
    {
        public CohereBilledUnits? BilledUnits { get; init; }
    }

    private sealed record CohereBilledUnits
    {
        public int? InputTokens { get; init; }
    }

    #endregion
}
