using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.LLM;
using LLMProxy.Infrastructure.LLMProviders.Configuration;
using LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Anthropic.Contracts;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud;

/// <summary>
/// Implémentation du client Anthropic Claude.
/// Supporte Claude 3 (Opus, Sonnet, Haiku) et streaming.
/// </summary>
public sealed class AnthropicProviderClient : LLMProviderClientBase
{
    private const string AnthropicApiVersion = "2023-06-01";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <inheritdoc />
    public override ProviderType Type => ProviderType.Anthropic;

    /// <summary>
    /// Constructeur.
    /// </summary>
    public AnthropicProviderClient(
        HttpClient httpClient,
        LLMProviderConfiguration configuration,
        ILogger<AnthropicProviderClient> logger)
        : base(httpClient, configuration, logger)
    {
        if (string.IsNullOrEmpty(HttpClient.BaseAddress?.ToString()))
        {
            HttpClient.BaseAddress = new Uri(configuration.BaseUrl);
        }

        // Anthropic requiert un header spécifique de version
        HttpClient.DefaultRequestHeaders.Add("anthropic-version", AnthropicApiVersion);
    }

    /// <inheritdoc />
    protected override void SetAuthorizationHeader(string apiKey)
    {
        // Anthropic utilise x-api-key au lieu de Bearer token
        HttpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
    }

    /// <inheritdoc />
    public override async Task<ProviderCapabilities> GetCapabilitiesAsync(
        CancellationToken cancellationToken = default)
    {
        return new ProviderCapabilities
        {
            Type = ProviderType.Anthropic,
            Name = Name,
            ApiVersion = AnthropicApiVersion,
            SupportedCapabilities = ModelCapabilities.ChatCompletion
                                    | ModelCapabilities.Streaming
                                    | ModelCapabilities.Vision
                                    | ModelCapabilities.FunctionCalling,
            SupportsStreaming = true,
            SupportsEmbeddings = false, // Anthropic ne supporte pas les embeddings
            SupportsFunctionCalling = true,
            SupportsVision = true,
            IsHealthy = await IsHealthyAsync(cancellationToken),
            LastHealthCheck = DateTimeOffset.UtcNow,
            MaxTokensPerRequest = 4096
        };
    }

    /// <inheritdoc />
    public override Task<IReadOnlyList<LLMModel>> ListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        // Anthropic n'a pas d'endpoint pour lister les modèles
        // On retourne la liste connue
        var models = new List<LLMModel>
        {
            CreateModel("claude-3-opus-20240229", "Claude 3 Opus", 200000),
            CreateModel("claude-3-sonnet-20240229", "Claude 3 Sonnet", 200000),
            CreateModel("claude-3-haiku-20240307", "Claude 3 Haiku", 200000),
            CreateModel("claude-3-5-sonnet-20241022", "Claude 3.5 Sonnet", 200000),
            CreateModel("claude-2.1", "Claude 2.1", 200000),
            CreateModel("claude-2.0", "Claude 2.0", 100000),
            CreateModel("claude-instant-1.2", "Claude Instant 1.2", 100000)
        };

        return Task.FromResult<IReadOnlyList<LLMModel>>(models);
    }

    private static LLMModel CreateModel(string id, string name, int contextLength)
    {
        return new LLMModel
        {
            Id = ModelIdentifier.FromValid(id),
            Name = name,
            OwnedBy = "Anthropic",
            ContextLength = contextLength,
            Capabilities = ModelCapabilities.ChatCompletion
                           | ModelCapabilities.Streaming
                           | ModelCapabilities.Vision
                           | ModelCapabilities.FunctionCalling,
            IsAvailable = true
        };
    }

    /// <inheritdoc />
    public override async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Envoyer une requête minimale pour vérifier la connectivité
            var request = new AnthropicMessagesRequest
            {
                Model = "claude-3-haiku-20240307",
                MaxTokens = 1,
                Messages = new List<AnthropicMessage>
                {
                    new() { Role = "user", Content = "Hi" }
                }
            };

            var response = await HttpClient.PostAsJsonAsync(
                "/v1/messages",
                request,
                JsonOptions,
                cancellationToken);

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
        var anthropicRequest = MapToAnthropicRequest(request);

        var response = await HttpClient.PostAsJsonAsync(
            "/v1/messages",
            anthropicRequest,
            JsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AnthropicMessagesResponse>(
            JsonOptions, cancellationToken);

        return MapToLLMResponse(result!, request.Model);
    }

    /// <inheritdoc />
    protected override async IAsyncEnumerable<LLMResponse> ExecuteChatCompletionStreamAsync(
        LLMRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var anthropicRequest = MapToAnthropicRequest(request) with { Stream = true };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v1/messages")
        {
            Content = JsonContent.Create(anthropicRequest, options: JsonOptions)
        };

        var response = await HttpClient.SendAsync(
            httpRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        string? messageId = null;

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrEmpty(line))
                continue;

            if (line.StartsWith("event: "))
                continue;

            if (!line.StartsWith("data: "))
                continue;

            var json = line["data: ".Length..];
            var evt = JsonSerializer.Deserialize<AnthropicStreamEvent>(json, JsonOptions);

            if (evt == null)
                continue;

            switch (evt.Type)
            {
                case "message_start":
                    messageId = evt.Message?.Id;
                    break;

                case "content_block_delta" when evt.Delta?.Text != null:
                    yield return new LLMResponse
                    {
                        Id = messageId ?? string.Empty,
                        Model = request.Model,
                        Content = evt.Delta.Text,
                        CreatedAt = DateTimeOffset.UtcNow
                    };
                    break;

                case "message_stop":
                    yield break;
            }
        }
    }

    /// <inheritdoc />
    protected override Task<EmbeddingResponse> ExecuteEmbeddingsAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken)
    {
        throw new NotSupportedException(
            "Anthropic ne supporte pas la génération d'embeddings. " +
            "Utilisez un autre provider comme OpenAI ou Cohere.");
    }

    #region Mapping

    private AnthropicMessagesRequest MapToAnthropicRequest(LLMRequest request)
    {
        var messages = new List<AnthropicMessage>();
        string? systemPrompt = null;

        foreach (var msg in request.Messages)
        {
            if (msg.Role == MessageRole.System)
            {
                systemPrompt = msg.Content;
            }
            else
            {
                messages.Add(new AnthropicMessage
                {
                    Role = msg.Role == MessageRole.User ? "user" : "assistant",
                    Content = msg.Content
                });
            }
        }

        return new AnthropicMessagesRequest
        {
            Model = request.Model.Value,
            MaxTokens = request.MaxTokens ?? Configuration.MaxTokens ?? 4096,
            Messages = messages,
            System = systemPrompt,
            Temperature = request.Temperature,
            TopP = request.TopP,
            StopSequences = request.StopSequences?.ToList(),
            Metadata = request.UserId != null
                ? new AnthropicMetadata { UserId = request.UserId }
                : null
        };
    }

    private static LLMResponse MapToLLMResponse(
        AnthropicMessagesResponse response,
        ModelIdentifier requestModel)
    {
        var content = response.Content
            ?.FirstOrDefault(c => c.Type == "text")
            ?.Text ?? string.Empty;

        return new LLMResponse
        {
            Id = response.Id,
            Model = ModelIdentifier.FromValid(response.Model ?? requestModel.Value),
            Content = content,
            FinishReason = MapFinishReason(response.StopReason),
            Usage = TokenUsage.FromValid(
                response.Usage?.InputTokens ?? 0,
                response.Usage?.OutputTokens ?? 0),
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    private static FinishReason MapFinishReason(string? reason)
    {
        return reason?.ToLowerInvariant() switch
        {
            "end_turn" => FinishReason.Stop,
            "max_tokens" => FinishReason.Length,
            "stop_sequence" => FinishReason.Stop,
            "tool_use" => FinishReason.ToolCalls,
            _ => FinishReason.Stop
        };
    }

    #endregion

}
