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
/// Implémentation du client OpenAI.
/// Supporte GPT-3.5, GPT-4, embeddings et streaming.
/// </summary>
public class OpenAIProviderClient : LLMProviderClientBase
{
    /// <summary>
    /// Options JSON pour la sérialisation.
    /// </summary>
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <inheritdoc />
    public override ProviderType Type => ProviderType.OpenAI;

    /// <summary>
    /// Constructeur.
    /// </summary>
    public OpenAIProviderClient(
        HttpClient httpClient,
        LLMProviderConfiguration configuration,
        ILogger<OpenAIProviderClient> logger)
        : base(httpClient, configuration, logger)
    {
        if (string.IsNullOrEmpty(HttpClient.BaseAddress?.ToString()))
        {
            HttpClient.BaseAddress = new Uri(configuration.BaseUrl);
        }
    }

    /// <inheritdoc />
    public override async Task<ProviderCapabilities> GetCapabilitiesAsync(
        CancellationToken cancellationToken = default)
    {
        return new ProviderCapabilities
        {
            Type = ProviderType.OpenAI,
            Name = Name,
            ApiVersion = "v1",
            SupportedCapabilities = ModelCapabilities.ChatCompletion
                                    | ModelCapabilities.Streaming
                                    | ModelCapabilities.Embeddings
                                    | ModelCapabilities.FunctionCalling
                                    | ModelCapabilities.Vision
                                    | ModelCapabilities.JsonMode,
            SupportsStreaming = true,
            SupportsEmbeddings = true,
            SupportsFunctionCalling = true,
            SupportsVision = true,
            IsHealthy = await IsHealthyAsync(cancellationToken),
            LastHealthCheck = DateTimeOffset.UtcNow
        };
    }

    /// <inheritdoc />
    public override async Task<IReadOnlyList<LLMModel>> ListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await HttpClient.GetAsync("/v1/models", cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenAIModelsResponse>(
            JsonOptions, cancellationToken);

        return result?.Data?.Select(m => new LLMModel
        {
            Id = ModelIdentifier.FromValid(m.Id),
            Name = m.Id,
            OwnedBy = m.OwnedBy,
            CreatedAt = DateTimeOffset.FromUnixTimeSeconds(m.Created),
            Capabilities = InferCapabilities(m.Id)
        }).ToList() ?? new List<LLMModel>();
    }

    /// <inheritdoc />
    public override async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await HttpClient.GetAsync("/v1/models", cancellationToken);
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
        var openAIRequest = MapToOpenAIRequest(request);

        var response = await HttpClient.PostAsJsonAsync(
            "/v1/chat/completions",
            openAIRequest,
            JsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenAIChatCompletionResponse>(
            JsonOptions, cancellationToken);

        return MapToLLMResponse(result!);
    }

    /// <inheritdoc />
    protected override async IAsyncEnumerable<LLMResponse> ExecuteChatCompletionStreamAsync(
        LLMRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var openAIRequest = MapToOpenAIRequest(request) with { Stream = true };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v1/chat/completions")
        {
            Content = JsonContent.Create(openAIRequest, options: JsonOptions)
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

            var chunk = JsonSerializer.Deserialize<OpenAIStreamChunk>(json, JsonOptions);
            var delta = chunk?.Choices?.FirstOrDefault()?.Delta;
            if (chunk != null && delta?.Content != null)
            {
                yield return new LLMResponse
                {
                    Id = chunk.Id ?? string.Empty,
                    Model = ModelIdentifier.FromValid(chunk.Model ?? request.Model.Value),
                    Content = delta.Content,
                    FinishReason = MapFinishReason(chunk.Choices![0].FinishReason),
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
        var openAIRequest = new OpenAIEmbeddingRequest
        {
            Model = request.Model.Value,
            Input = request.Inputs.ToList(),
            EncodingFormat = request.Encoding == EmbeddingEncoding.Base64 ? "base64" : "float",
            Dimensions = request.Dimensions
        };

        var response = await HttpClient.PostAsJsonAsync(
            "/v1/embeddings",
            openAIRequest,
            JsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenAIEmbeddingResponse>(
            JsonOptions, cancellationToken);

        return new EmbeddingResponse
        {
            Model = ModelIdentifier.FromValid(result!.Model),
            Embeddings = result.Data.Select(e => new Embedding
            {
                Index = e.Index,
                Vector = e.Embedding.ToList()
            }).ToList(),
            Usage = TokenUsage.FromValid(result.Usage.PromptTokens, 0)
        };
    }

    #region Mapping

    private OpenAIChatCompletionRequest MapToOpenAIRequest(LLMRequest request)
    {
        return new OpenAIChatCompletionRequest
        {
            Model = request.Model.Value,
            Messages = request.Messages.Select(m => new OpenAIMessage
            {
                Role = m.Role.ToString().ToLowerInvariant(),
                Content = m.Content,
                Name = m.Name
            }).ToList(),
            Temperature = request.Temperature,
            MaxTokens = request.MaxTokens ?? Configuration.MaxTokens,
            TopP = request.TopP,
            N = request.N,
            Stop = request.StopSequences?.ToList(),
            PresencePenalty = request.PresencePenalty,
            FrequencyPenalty = request.FrequencyPenalty,
            User = request.UserId,
            Seed = request.Seed,
            ResponseFormat = request.ResponseFormat == ResponseFormat.Json
                ? new OpenAIResponseFormat { Type = "json_object" }
                : null,
            Stream = request.Stream
        };
    }

    private static LLMResponse MapToLLMResponse(OpenAIChatCompletionResponse response)
    {
        var choice = response.Choices?.FirstOrDefault();

        return new LLMResponse
        {
            Id = response.Id,
            Model = ModelIdentifier.FromValid(response.Model),
            Content = choice?.Message?.Content ?? string.Empty,
            FinishReason = MapFinishReason(choice?.FinishReason),
            Usage = TokenUsage.FromValid(
                response.Usage?.PromptTokens ?? 0,
                response.Usage?.CompletionTokens ?? 0),
            CreatedAt = DateTimeOffset.FromUnixTimeSeconds(response.Created)
        };
    }

    private static FinishReason MapFinishReason(string? reason)
    {
        return reason?.ToLowerInvariant() switch
        {
            "stop" => FinishReason.Stop,
            "length" => FinishReason.Length,
            "tool_calls" or "function_call" => FinishReason.ToolCalls,
            "content_filter" => FinishReason.ContentFilter,
            _ => FinishReason.Stop
        };
    }

    private static ModelCapabilities InferCapabilities(string modelId)
    {
        var caps = ModelCapabilities.ChatCompletion | ModelCapabilities.Streaming;

        if (modelId.Contains("gpt-4") || modelId.Contains("gpt-3.5"))
        {
            caps |= ModelCapabilities.FunctionCalling | ModelCapabilities.JsonMode;
        }

        if (modelId.Contains("vision") || modelId.Contains("gpt-4o") || modelId.Contains("gpt-4-turbo"))
        {
            caps |= ModelCapabilities.Vision;
        }

        if (modelId.Contains("embedding"))
        {
            caps = ModelCapabilities.Embeddings;
        }

        return caps;
    }

    #endregion

    #region DTOs OpenAI

    private sealed record OpenAIChatCompletionRequest
    {
        public required string Model { get; init; }
        public required List<OpenAIMessage> Messages { get; init; }
        public decimal? Temperature { get; init; }
        public int? MaxTokens { get; init; }
        public decimal? TopP { get; init; }
        public int? N { get; init; }
        public List<string>? Stop { get; init; }
        public decimal? PresencePenalty { get; init; }
        public decimal? FrequencyPenalty { get; init; }
        public string? User { get; init; }
        public int? Seed { get; init; }
        public OpenAIResponseFormat? ResponseFormat { get; init; }
        public bool Stream { get; init; }
    }

    private sealed record OpenAIMessage
    {
        public required string Role { get; init; }
        public required string Content { get; init; }
        public string? Name { get; init; }
    }

    private sealed record OpenAIResponseFormat
    {
        public required string Type { get; init; }
    }

    private sealed record OpenAIChatCompletionResponse
    {
        public required string Id { get; init; }
        public required string Model { get; init; }
        public long Created { get; init; }
        public OpenAIChoice[]? Choices { get; init; }
        public OpenAIUsage? Usage { get; init; }
    }

    private sealed record OpenAIChoice
    {
        public OpenAIMessage? Message { get; init; }
        public string? FinishReason { get; init; }
    }

    private sealed record OpenAIUsage
    {
        public int PromptTokens { get; init; }
        public int CompletionTokens { get; init; }
        public int TotalTokens { get; init; }
    }

    private sealed record OpenAIStreamChunk
    {
        public string? Id { get; init; }
        public string? Model { get; init; }
        public OpenAIStreamChoice[]? Choices { get; init; }
    }

    private sealed record OpenAIStreamChoice
    {
        public OpenAIDelta? Delta { get; init; }
        public string? FinishReason { get; init; }
    }

    private sealed record OpenAIDelta
    {
        public string? Content { get; init; }
    }

    private sealed record OpenAIModelsResponse
    {
        public List<OpenAIModelInfo>? Data { get; init; }
    }

    private sealed record OpenAIModelInfo
    {
        public required string Id { get; init; }
        public string? OwnedBy { get; init; }
        public long Created { get; init; }
    }

    private sealed record OpenAIEmbeddingRequest
    {
        public required string Model { get; init; }
        public required List<string> Input { get; init; }
        public string? EncodingFormat { get; init; }
        public int? Dimensions { get; init; }
    }

    private sealed record OpenAIEmbeddingResponse
    {
        public required string Model { get; init; }
        public required List<OpenAIEmbeddingData> Data { get; init; }
        public required OpenAIUsage Usage { get; init; }
    }

    private sealed record OpenAIEmbeddingData
    {
        public int Index { get; init; }
        public required float[] Embedding { get; init; }
    }

    #endregion
}
