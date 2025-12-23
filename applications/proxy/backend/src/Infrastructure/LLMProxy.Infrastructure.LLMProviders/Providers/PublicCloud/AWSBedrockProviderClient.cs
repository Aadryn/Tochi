using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.LLM;
using LLMProxy.Infrastructure.LLMProviders.Configuration;
using Microsoft.Extensions.Logging;
using DomainTokenUsage = LLMProxy.Domain.LLM.TokenUsage;

namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud;

/// <summary>
/// Implémentation du client AWS Bedrock.
/// Utilise le SDK AWS pour l'authentification SigV4.
/// </summary>
public sealed class AWSBedrockProviderClient : LLMProviderClientBase
{
    private readonly AmazonBedrockRuntimeClient _bedrockClient;
    private readonly AwsConfiguration _awsConfig;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <inheritdoc />
    public override ProviderType Type => ProviderType.AWSBedrock;

    /// <summary>
    /// Constructeur.
    /// </summary>
    public AWSBedrockProviderClient(
        HttpClient httpClient,
        LLMProviderConfiguration configuration,
        ILogger<AWSBedrockProviderClient> logger)
        : base(httpClient, configuration, logger)
    {
        _awsConfig = configuration.Aws
            ?? throw new ArgumentException("Configuration AWS requise pour Bedrock");

        var region = RegionEndpoint.GetBySystemName(_awsConfig.Region);

        // Créer le client Bedrock avec les credentials AWS
        if (!string.IsNullOrEmpty(_awsConfig.AccessKeyId) && !string.IsNullOrEmpty(_awsConfig.SecretAccessKey))
        {
            _bedrockClient = new AmazonBedrockRuntimeClient(
                _awsConfig.AccessKeyId,
                _awsConfig.SecretAccessKey,
                region);
        }
        else
        {
            // Utiliser les credentials par défaut (profil, variables d'environnement, etc.)
            _bedrockClient = new AmazonBedrockRuntimeClient(region);
        }
    }

    /// <inheritdoc />
    protected override void SetAuthorizationHeader(string apiKey)
    {
        // AWS utilise SigV4, pas de header d'autorisation classique
        // L'authentification est gérée par le SDK AWS
    }

    /// <inheritdoc />
    public override Task<ProviderCapabilities> GetCapabilitiesAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ProviderCapabilities
        {
            Type = ProviderType.AWSBedrock,
            Name = Name,
            SupportedCapabilities = ModelCapabilities.ChatCompletion
                                    | ModelCapabilities.Streaming
                                    | ModelCapabilities.Embeddings,
            SupportsStreaming = true,
            SupportsEmbeddings = true,
            SupportsFunctionCalling = false, // Via tools beta uniquement
            SupportsVision = true, // Claude 3 sur Bedrock
            IsHealthy = true, // AWS est généralement disponible
            LastHealthCheck = DateTimeOffset.UtcNow,
            Regions = new[] { _awsConfig.Region }
        });
    }

    /// <inheritdoc />
    public override Task<IReadOnlyList<LLMModel>> ListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        // Liste des modèles Bedrock supportés
        var models = new List<LLMModel>
        {
            new()
            {
                Id = ModelIdentifier.FromValid("anthropic.claude-3-5-sonnet-20241022-v2:0"),
                Name = "Claude 3.5 Sonnet v2",
                OwnedBy = "Anthropic",
                Capabilities = ModelCapabilities.ChatCompletion
                               | ModelCapabilities.Streaming
                               | ModelCapabilities.Vision,
                IsAvailable = true,
                ContextLength = 200000
            },
            new()
            {
                Id = ModelIdentifier.FromValid("anthropic.claude-3-haiku-20240307-v1:0"),
                Name = "Claude 3 Haiku",
                OwnedBy = "Anthropic",
                Capabilities = ModelCapabilities.ChatCompletion
                               | ModelCapabilities.Streaming
                               | ModelCapabilities.Vision,
                IsAvailable = true,
                ContextLength = 200000
            },
            new()
            {
                Id = ModelIdentifier.FromValid("amazon.titan-text-premier-v1:0"),
                Name = "Titan Text Premier",
                OwnedBy = "Amazon",
                Capabilities = ModelCapabilities.ChatCompletion | ModelCapabilities.Streaming,
                IsAvailable = true
            },
            new()
            {
                Id = ModelIdentifier.FromValid("amazon.titan-embed-text-v2:0"),
                Name = "Titan Embeddings V2",
                OwnedBy = "Amazon",
                Capabilities = ModelCapabilities.Embeddings,
                IsAvailable = true
            },
            new()
            {
                Id = ModelIdentifier.FromValid("meta.llama3-1-70b-instruct-v1:0"),
                Name = "Llama 3.1 70B Instruct",
                OwnedBy = "Meta",
                Capabilities = ModelCapabilities.ChatCompletion | ModelCapabilities.Streaming,
                IsAvailable = true
            },
            new()
            {
                Id = ModelIdentifier.FromValid("mistral.mistral-large-2407-v1:0"),
                Name = "Mistral Large 2407",
                OwnedBy = "Mistral AI",
                Capabilities = ModelCapabilities.ChatCompletion | ModelCapabilities.Streaming,
                IsAvailable = true
            }
        };

        return Task.FromResult<IReadOnlyList<LLMModel>>(models);
    }

    /// <inheritdoc />
    public override async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Utiliser le modèle le plus léger pour vérifier la connectivité
            var request = new InvokeModelRequest
            {
                ModelId = "anthropic.claude-3-haiku-20240307-v1:0",
                ContentType = "application/json",
                Accept = "application/json",
                Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new
                {
                    anthropic_version = "bedrock-2023-05-31",
                    max_tokens = 1,
                    messages = new[]
                    {
                        new { role = "user", content = "test" }
                    }
                }, JsonOptions)))
            };

            await _bedrockClient.InvokeModelAsync(request, cancellationToken);
            return true;
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
        var modelId = request.Model.Value;

        // Déterminer le format du payload selon le provider du modèle
        var bodyJson = BuildRequestBody(modelId, request);

        var invokeRequest = new InvokeModelRequest
        {
            ModelId = modelId,
            ContentType = "application/json",
            Accept = "application/json",
            Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyJson))
        };

        var response = await _bedrockClient.InvokeModelAsync(invokeRequest, cancellationToken);

        using var reader = new StreamReader(response.Body);
        var responseJson = await reader.ReadToEndAsync(cancellationToken);

        return ParseResponse(modelId, responseJson, request.Model);
    }

    /// <inheritdoc />
    protected override async IAsyncEnumerable<LLMResponse> ExecuteChatCompletionStreamAsync(
        LLMRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var modelId = request.Model.Value;
        var bodyJson = BuildRequestBody(modelId, request);

        var streamRequest = new InvokeModelWithResponseStreamRequest
        {
            ModelId = modelId,
            ContentType = "application/json",
            Accept = "application/json",
            Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyJson))
        };

        var response = await _bedrockClient.InvokeModelWithResponseStreamAsync(
            streamRequest, cancellationToken);

        // ResponseStream contient des événements qu'on itère séquentiellement
        foreach (var payloadPart in response.Body)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (payloadPart is PayloadPart chunk)
            {
                var json = Encoding.UTF8.GetString(chunk.Bytes.ToArray());
                var partialResponse = ParseStreamChunk(modelId, json, request.Model);
                if (partialResponse != null)
                {
                    yield return partialResponse;
                }
            }
        }
    }

    /// <inheritdoc />
    protected override async Task<EmbeddingResponse> ExecuteEmbeddingsAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken)
    {
        var modelId = request.Model.Value;
        var embeddings = new List<Embedding>();

        // Bedrock traite les embeddings un par un
        for (var i = 0; i < request.Inputs.Count; i++)
        {
            var bodyJson = JsonSerializer.Serialize(new
            {
                inputText = request.Inputs[i]
            }, JsonOptions);

            var invokeRequest = new InvokeModelRequest
            {
                ModelId = modelId,
                ContentType = "application/json",
                Accept = "application/json",
                Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyJson))
            };

            var response = await _bedrockClient.InvokeModelAsync(invokeRequest, cancellationToken);

            using var reader = new StreamReader(response.Body);
            var responseJson = await reader.ReadToEndAsync(cancellationToken);

            var result = JsonSerializer.Deserialize<BedrockEmbeddingResponse>(responseJson, JsonOptions);

            embeddings.Add(new Embedding
            {
                Index = i,
                Vector = result!.Embedding.ToList()
            });
        }

        return new EmbeddingResponse
        {
            Model = request.Model,
            Embeddings = embeddings,
            Usage = DomainTokenUsage.FromValid(request.Inputs.Count * 100, 0) // Estimation
        };
    }

    #region Builders par provider

    private string BuildRequestBody(string modelId, LLMRequest request)
    {
        // Format Claude/Anthropic sur Bedrock
        if (modelId.StartsWith("anthropic.", StringComparison.OrdinalIgnoreCase))
        {
            return JsonSerializer.Serialize(new
            {
                anthropic_version = "bedrock-2023-05-31",
                max_tokens = request.MaxTokens ?? Configuration.MaxTokens ?? 4096,
                temperature = request.Temperature,
                top_p = request.TopP,
                messages = request.Messages.Select(m => new
                {
                    role = m.Role.ToString().ToLowerInvariant(),
                    content = m.Content
                }).ToArray()
            }, JsonOptions);
        }

        // Format Amazon Titan
        if (modelId.StartsWith("amazon.titan", StringComparison.OrdinalIgnoreCase))
        {
            return JsonSerializer.Serialize(new
            {
                inputText = BuildPromptFromMessages(request.Messages),
                textGenerationConfig = new
                {
                    maxTokenCount = request.MaxTokens ?? Configuration.MaxTokens ?? 4096,
                    temperature = (float)(request.Temperature ?? 0.7m),
                    topP = (float)(request.TopP ?? 1.0m)
                }
            }, JsonOptions);
        }

        // Format Meta Llama
        if (modelId.StartsWith("meta.llama", StringComparison.OrdinalIgnoreCase))
        {
            return JsonSerializer.Serialize(new
            {
                prompt = BuildPromptFromMessages(request.Messages),
                max_gen_len = request.MaxTokens ?? Configuration.MaxTokens ?? 4096,
                temperature = (float)(request.Temperature ?? 0.7m),
                top_p = (float)(request.TopP ?? 1.0m)
            }, JsonOptions);
        }

        // Format Mistral sur Bedrock
        if (modelId.StartsWith("mistral.", StringComparison.OrdinalIgnoreCase))
        {
            return JsonSerializer.Serialize(new
            {
                prompt = BuildPromptFromMessages(request.Messages, "[INST]", "[/INST]"),
                max_tokens = request.MaxTokens ?? Configuration.MaxTokens ?? 4096,
                temperature = (float)(request.Temperature ?? 0.7m),
                top_p = (float)(request.TopP ?? 1.0m)
            }, JsonOptions);
        }

        throw new NotSupportedException($"Modèle Bedrock non supporté: {modelId}");
    }

    private static string BuildPromptFromMessages(
        IEnumerable<LLMMessage> messages,
        string? userPrefix = null,
        string? userSuffix = null)
    {
        var sb = new StringBuilder();

        foreach (var message in messages)
        {
            switch (message.Role)
            {
                case MessageRole.System:
                    sb.AppendLine(message.Content);
                    sb.AppendLine();
                    break;
                case MessageRole.User:
                    if (userPrefix != null)
                        sb.Append(userPrefix);
                    sb.Append(message.Content);
                    if (userSuffix != null)
                        sb.Append(userSuffix);
                    sb.AppendLine();
                    break;
                case MessageRole.Assistant:
                    sb.AppendLine(message.Content);
                    break;
            }
        }

        return sb.ToString();
    }

    #endregion

    #region Parsers par provider

    private LLMResponse ParseResponse(string modelId, string responseJson, ModelIdentifier requestModel)
    {
        // Format Claude/Anthropic
        if (modelId.StartsWith("anthropic.", StringComparison.OrdinalIgnoreCase))
        {
            var response = JsonSerializer.Deserialize<AnthropicBedrockResponse>(responseJson, JsonOptions);
            return new LLMResponse
            {
                Id = response!.Id ?? Guid.NewGuid().ToString(),
                Model = requestModel,
                Content = response.Content?.FirstOrDefault()?.Text ?? string.Empty,
                FinishReason = MapAnthropicStopReason(response.StopReason),
                Usage = DomainTokenUsage.FromValid(
                    response.Usage?.InputTokens ?? 0,
                    response.Usage?.OutputTokens ?? 0),
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        // Format Amazon Titan
        if (modelId.StartsWith("amazon.titan", StringComparison.OrdinalIgnoreCase))
        {
            var response = JsonSerializer.Deserialize<TitanResponse>(responseJson, JsonOptions);
            return new LLMResponse
            {
                Id = Guid.NewGuid().ToString(),
                Model = requestModel,
                Content = response!.Results?.FirstOrDefault()?.OutputText ?? string.Empty,
                FinishReason = FinishReason.Stop,
                Usage = DomainTokenUsage.FromValid(
                    response.InputTextTokenCount ?? 0,
                    response.Results?.FirstOrDefault()?.TokenCount ?? 0),
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        // Format Meta Llama
        if (modelId.StartsWith("meta.llama", StringComparison.OrdinalIgnoreCase))
        {
            var response = JsonSerializer.Deserialize<LlamaResponse>(responseJson, JsonOptions);
            return new LLMResponse
            {
                Id = Guid.NewGuid().ToString(),
                Model = requestModel,
                Content = response!.Generation ?? string.Empty,
                FinishReason = MapLlamaStopReason(response.StopReason),
                Usage = DomainTokenUsage.FromValid(
                    response.PromptTokenCount ?? 0,
                    response.GenerationTokenCount ?? 0),
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        // Format Mistral
        if (modelId.StartsWith("mistral.", StringComparison.OrdinalIgnoreCase))
        {
            var response = JsonSerializer.Deserialize<MistralBedrockResponse>(responseJson, JsonOptions);
            return new LLMResponse
            {
                Id = Guid.NewGuid().ToString(),
                Model = requestModel,
                Content = response!.Outputs?.FirstOrDefault()?.Text ?? string.Empty,
                FinishReason = MapMistralStopReason(response.Outputs?.FirstOrDefault()?.StopReason),
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        throw new NotSupportedException($"Modèle Bedrock non supporté: {modelId}");
    }

    private LLMResponse? ParseStreamChunk(string modelId, string json, ModelIdentifier requestModel)
    {
        // Anthropic streaming
        if (modelId.StartsWith("anthropic.", StringComparison.OrdinalIgnoreCase))
        {
            var chunk = JsonSerializer.Deserialize<AnthropicStreamChunk>(json, JsonOptions);
            if (chunk?.Type == "content_block_delta")
            {
                return new LLMResponse
                {
                    Id = Guid.NewGuid().ToString(),
                    Model = requestModel,
                    Content = chunk.Delta?.Text ?? string.Empty,
                    FinishReason = FinishReason.Stop,
                    CreatedAt = DateTimeOffset.UtcNow
                };
            }
        }

        // Autres modèles - simplifié
        var genericChunk = JsonSerializer.Deserialize<Dictionary<string, object>>(json, JsonOptions);
        if (genericChunk != null)
        {
            var content = genericChunk.TryGetValue("generation", out var gen)
                ? gen?.ToString()
                : genericChunk.TryGetValue("outputText", out var output)
                    ? output?.ToString()
                    : null;

            if (!string.IsNullOrEmpty(content))
            {
                return new LLMResponse
                {
                    Id = Guid.NewGuid().ToString(),
                    Model = requestModel,
                    Content = content,
                    FinishReason = FinishReason.Stop,
                    CreatedAt = DateTimeOffset.UtcNow
                };
            }
        }

        return null;
    }

    private static FinishReason MapAnthropicStopReason(string? reason)
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

    private static FinishReason MapLlamaStopReason(string? reason)
    {
        return reason?.ToLowerInvariant() switch
        {
            "stop" => FinishReason.Stop,
            "length" => FinishReason.Length,
            _ => FinishReason.Stop
        };
    }

    private static FinishReason MapMistralStopReason(string? reason)
    {
        return reason?.ToLowerInvariant() switch
        {
            "stop" => FinishReason.Stop,
            "length" => FinishReason.Length,
            "end_turn" => FinishReason.Stop,
            _ => FinishReason.Stop
        };
    }

    #endregion

    #region DTOs Bedrock

    private sealed record AnthropicBedrockResponse
    {
        public string? Id { get; init; }
        public List<ContentBlock>? Content { get; init; }
        public string? StopReason { get; init; }
        public AnthropicUsage? Usage { get; init; }
    }

    private sealed record ContentBlock
    {
        public string? Type { get; init; }
        public string? Text { get; init; }
    }

    private sealed record AnthropicUsage
    {
        public int InputTokens { get; init; }
        public int OutputTokens { get; init; }
    }

    private sealed record AnthropicStreamChunk
    {
        public string? Type { get; init; }
        public AnthropicDelta? Delta { get; init; }
    }

    private sealed record AnthropicDelta
    {
        public string? Type { get; init; }
        public string? Text { get; init; }
    }

    private sealed record TitanResponse
    {
        public int? InputTextTokenCount { get; init; }
        public List<TitanResult>? Results { get; init; }
    }

    private sealed record TitanResult
    {
        public string? OutputText { get; init; }
        public int TokenCount { get; init; }
    }

    private sealed record LlamaResponse
    {
        public string? Generation { get; init; }
        public int? PromptTokenCount { get; init; }
        public int? GenerationTokenCount { get; init; }
        public string? StopReason { get; init; }
    }

    private sealed record MistralBedrockResponse
    {
        public List<MistralOutput>? Outputs { get; init; }
    }

    private sealed record MistralOutput
    {
        public string? Text { get; init; }
        public string? StopReason { get; init; }
    }

    private sealed record BedrockEmbeddingResponse
    {
        public required float[] Embedding { get; init; }
    }

    #endregion

    /// <inheritdoc />
    public override void Dispose()
    {
        _bedrockClient.Dispose();
        base.Dispose();
    }
}
