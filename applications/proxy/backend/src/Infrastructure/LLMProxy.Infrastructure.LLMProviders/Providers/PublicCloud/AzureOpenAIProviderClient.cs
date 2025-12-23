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
/// Implémentation du client Azure OpenAI.
/// Utilise les endpoints Azure avec déploiements personnalisés.
/// </summary>
public sealed class AzureOpenAIProviderClient : LLMProviderClientBase
{
    private readonly AzureConfiguration _azureConfig;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <inheritdoc />
    public override ProviderType Type => ProviderType.AzureOpenAI;

    /// <summary>
    /// Constructeur.
    /// </summary>
    public AzureOpenAIProviderClient(
        HttpClient httpClient,
        LLMProviderConfiguration configuration,
        ILogger<AzureOpenAIProviderClient> logger)
        : base(httpClient, configuration, logger)
    {
        _azureConfig = configuration.Azure
            ?? throw new ArgumentException("Configuration Azure requise pour Azure OpenAI");

        if (string.IsNullOrEmpty(HttpClient.BaseAddress?.ToString()))
        {
            HttpClient.BaseAddress = new Uri(configuration.BaseUrl);
        }
    }

    /// <inheritdoc />
    protected override void SetAuthorizationHeader(string apiKey)
    {
        // Azure utilise api-key au lieu de Bearer
        HttpClient.DefaultRequestHeaders.Add("api-key", apiKey);
    }

    /// <inheritdoc />
    public override async Task<ProviderCapabilities> GetCapabilitiesAsync(
        CancellationToken cancellationToken = default)
    {
        return new ProviderCapabilities
        {
            Type = ProviderType.AzureOpenAI,
            Name = Name,
            ApiVersion = _azureConfig.ApiVersion,
            SupportedCapabilities = ModelCapabilities.ChatCompletion
                                    | ModelCapabilities.Streaming
                                    | ModelCapabilities.Embeddings
                                    | ModelCapabilities.FunctionCalling
                                    | ModelCapabilities.JsonMode,
            SupportsStreaming = true,
            SupportsEmbeddings = true,
            SupportsFunctionCalling = true,
            SupportsVision = true,
            IsHealthy = await IsHealthyAsync(cancellationToken),
            LastHealthCheck = DateTimeOffset.UtcNow,
            Regions = _azureConfig.Region != null ? new[] { _azureConfig.Region } : null
        };
    }

    /// <inheritdoc />
    public override Task<IReadOnlyList<LLMModel>> ListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        // Azure n'a pas d'endpoint pour lister les modèles
        // Les modèles sont des déploiements configurés dans le portail Azure
        var models = new List<LLMModel>
        {
            new()
            {
                Id = ModelIdentifier.FromValid(_azureConfig.DeploymentName),
                Name = _azureConfig.DeploymentName,
                OwnedBy = "Azure OpenAI",
                Capabilities = ModelCapabilities.ChatCompletion
                               | ModelCapabilities.Streaming
                               | ModelCapabilities.FunctionCalling,
                IsAvailable = true,
                Metadata = new Dictionary<string, string>
                {
                    ["deployment"] = _azureConfig.DeploymentName,
                    ["api_version"] = _azureConfig.ApiVersion
                }
            }
        };

        return Task.FromResult<IReadOnlyList<LLMModel>>(models);
    }

    /// <inheritdoc />
    public override async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Tenter une requête minimale pour vérifier le déploiement
            var uri = $"/openai/deployments/{_azureConfig.DeploymentName}/chat/completions" +
                      $"?api-version={_azureConfig.ApiVersion}";

            var request = new AzureChatRequest
            {
                Messages = new List<AzureMessage>
                {
                    new() { Role = "user", Content = "test" }
                },
                MaxTokens = 1
            };

            var response = await HttpClient.PostAsJsonAsync(uri, request, JsonOptions, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    protected override async Task<LLMResponse> ExecuteChatCompletionAsync(
        LLMRequest request,
        CancellationToken cancellationToken)
    {
        var uri = $"/openai/deployments/{_azureConfig.DeploymentName}/chat/completions" +
                  $"?api-version={_azureConfig.ApiVersion}";

        var azureRequest = MapToAzureRequest(request);

        var response = await HttpClient.PostAsJsonAsync(
            uri,
            azureRequest,
            JsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AzureChatResponse>(
            JsonOptions, cancellationToken);

        return MapToLLMResponse(result!, request.Model);
    }

    /// <inheritdoc />
    protected override async IAsyncEnumerable<LLMResponse> ExecuteChatCompletionStreamAsync(
        LLMRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var uri = $"/openai/deployments/{_azureConfig.DeploymentName}/chat/completions" +
                  $"?api-version={_azureConfig.ApiVersion}";

        var azureRequest = MapToAzureRequest(request) with { Stream = true };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = JsonContent.Create(azureRequest, options: JsonOptions)
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

            var chunk = JsonSerializer.Deserialize<AzureStreamChunk>(json, JsonOptions);
            if (chunk?.Choices?.Length > 0)
            {
                var content = chunk.Choices[0].Delta?.Content;
                if (!string.IsNullOrEmpty(content))
                {
                    yield return new LLMResponse
                    {
                        Id = chunk.Id ?? string.Empty,
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
        // Pour Azure, il faut un déploiement d'embeddings séparé
        // On utilise le modèle spécifié ou un déploiement par défaut
        var deploymentName = request.Model.Value;

        var uri = $"/openai/deployments/{deploymentName}/embeddings" +
                  $"?api-version={_azureConfig.ApiVersion}";

        var azureRequest = new AzureEmbeddingRequest
        {
            Input = request.Inputs.ToList()
        };

        var response = await HttpClient.PostAsJsonAsync(
            uri,
            azureRequest,
            JsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AzureEmbeddingResponse>(
            JsonOptions, cancellationToken);

        return new EmbeddingResponse
        {
            Model = request.Model,
            Embeddings = result!.Data.Select(e => new Embedding
            {
                Index = e.Index,
                Vector = e.Embedding.ToList()
            }).ToList(),
            Usage = TokenUsage.FromValid(result.Usage.PromptTokens, 0)
        };
    }

    #region Mapping

    private AzureChatRequest MapToAzureRequest(LLMRequest request)
    {
        return new AzureChatRequest
        {
            Messages = request.Messages.Select(m => new AzureMessage
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
                ? new AzureResponseFormat { Type = "json_object" }
                : null,
            Stream = request.Stream
        };
    }

    private static LLMResponse MapToLLMResponse(
        AzureChatResponse response,
        ModelIdentifier requestModel)
    {
        var choice = response.Choices?.FirstOrDefault();

        return new LLMResponse
        {
            Id = response.Id,
            Model = requestModel,
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

    #endregion

    #region DTOs Azure

    private sealed record AzureChatRequest
    {
        public required List<AzureMessage> Messages { get; init; }
        public decimal? Temperature { get; init; }
        public int? MaxTokens { get; init; }
        public decimal? TopP { get; init; }
        public int? N { get; init; }
        public List<string>? Stop { get; init; }
        public decimal? PresencePenalty { get; init; }
        public decimal? FrequencyPenalty { get; init; }
        public string? User { get; init; }
        public int? Seed { get; init; }
        public AzureResponseFormat? ResponseFormat { get; init; }
        public bool Stream { get; init; }
    }

    private sealed record AzureMessage
    {
        public required string Role { get; init; }
        public required string Content { get; init; }
        public string? Name { get; init; }
    }

    private sealed record AzureResponseFormat
    {
        public required string Type { get; init; }
    }

    private sealed record AzureChatResponse
    {
        public required string Id { get; init; }
        public long Created { get; init; }
        public AzureChoice[]? Choices { get; init; }
        public AzureUsage? Usage { get; init; }
    }

    private sealed record AzureChoice
    {
        public AzureMessage? Message { get; init; }
        public string? FinishReason { get; init; }
    }

    private sealed record AzureUsage
    {
        public int PromptTokens { get; init; }
        public int CompletionTokens { get; init; }
    }

    private sealed record AzureStreamChunk
    {
        public string? Id { get; init; }
        public AzureStreamChoice[]? Choices { get; init; }
    }

    private sealed record AzureStreamChoice
    {
        public AzureDelta? Delta { get; init; }
        public string? FinishReason { get; init; }
    }

    private sealed record AzureDelta
    {
        public string? Content { get; init; }
    }

    private sealed record AzureEmbeddingRequest
    {
        public required List<string> Input { get; init; }
    }

    private sealed record AzureEmbeddingResponse
    {
        public required List<AzureEmbeddingData> Data { get; init; }
        public required AzureUsage Usage { get; init; }
    }

    private sealed record AzureEmbeddingData
    {
        public int Index { get; init; }
        public required float[] Embedding { get; init; }
    }

    #endregion
}
