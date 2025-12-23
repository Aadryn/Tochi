using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.LLM;
using LLMProxy.Infrastructure.LLMProviders.Configuration;
using Microsoft.Extensions.Logging;
using LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.GoogleGemini.Contracts;

namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud;

/// <summary>
/// Implémentation du client Google Gemini (Generative AI).
/// Utilise l'API native Google AI avec authentification API key.
/// </summary>
public sealed class GoogleGeminiProviderClient : LLMProviderClientBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <inheritdoc />
    public override ProviderType Type => ProviderType.GoogleGemini;

    /// <summary>
    /// Constructeur.
    /// </summary>
    public GoogleGeminiProviderClient(
        HttpClient httpClient,
        LLMProviderConfiguration configuration,
        ILogger<GoogleGeminiProviderClient> logger)
        : base(httpClient, configuration, logger)
    {
        // L'API Gemini utilise le paramètre URL pour l'authentification
        if (string.IsNullOrEmpty(HttpClient.BaseAddress?.ToString()))
        {
            HttpClient.BaseAddress = new Uri(
                configuration.BaseUrl ?? "https://generativelanguage.googleapis.com/v1beta");
        }
    }

    /// <inheritdoc />
    protected override void SetAuthorizationHeader(string apiKey)
    {
        // Google Gemini utilise le paramètre URL key= au lieu d'un header
        // Le header est inutile mais on peut mettre un custom header si besoin
    }

    /// <inheritdoc />
    public override Task<ProviderCapabilities> GetCapabilitiesAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ProviderCapabilities
        {
            Type = ProviderType.GoogleGemini,
            Name = Name,
            ApiVersion = "v1beta",
            SupportedCapabilities = ModelCapabilities.ChatCompletion
                                    | ModelCapabilities.Streaming
                                    | ModelCapabilities.Embeddings
                                    | ModelCapabilities.FunctionCalling
                                    | ModelCapabilities.Vision,
            SupportsStreaming = true,
            SupportsEmbeddings = true,
            SupportsFunctionCalling = true,
            SupportsVision = true,
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
                Id = ModelIdentifier.FromValid("gemini-2.0-flash"),
                Name = "Gemini 2.0 Flash",
                OwnedBy = "Google",
                Capabilities = ModelCapabilities.ChatCompletion
                               | ModelCapabilities.Streaming
                               | ModelCapabilities.FunctionCalling
                               | ModelCapabilities.Vision,
                IsAvailable = true,
                ContextLength = 1000000
            },
            new()
            {
                Id = ModelIdentifier.FromValid("gemini-1.5-pro"),
                Name = "Gemini 1.5 Pro",
                OwnedBy = "Google",
                Capabilities = ModelCapabilities.ChatCompletion
                               | ModelCapabilities.Streaming
                               | ModelCapabilities.FunctionCalling
                               | ModelCapabilities.Vision,
                IsAvailable = true,
                ContextLength = 2000000
            },
            new()
            {
                Id = ModelIdentifier.FromValid("gemini-1.5-flash"),
                Name = "Gemini 1.5 Flash",
                OwnedBy = "Google",
                Capabilities = ModelCapabilities.ChatCompletion
                               | ModelCapabilities.Streaming
                               | ModelCapabilities.FunctionCalling
                               | ModelCapabilities.Vision,
                IsAvailable = true,
                ContextLength = 1000000
            },
            new()
            {
                Id = ModelIdentifier.FromValid("text-embedding-004"),
                Name = "Gemini Text Embedding 004",
                OwnedBy = "Google",
                Capabilities = ModelCapabilities.Embeddings,
                IsAvailable = true,
                OutputDimension = 768
            }
        };

        return Task.FromResult<IReadOnlyList<LLMModel>>(models);
    }

    /// <inheritdoc />
    public override async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var uri = $"/models?key={Configuration.ApiKey}";
            var response = await HttpClient.GetAsync(uri, cancellationToken);
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
        var uri = $"/models/{model}:generateContent?key={Configuration.ApiKey}";

        var geminiRequest = MapToGeminiRequest(request);

        var response = await HttpClient.PostAsJsonAsync(uri, geminiRequest, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GeminiResponse>(JsonOptions, cancellationToken);

        return MapToLLMResponse(result!, request.Model);
    }

    /// <inheritdoc />
    protected override async IAsyncEnumerable<LLMResponse> ExecuteChatCompletionStreamAsync(
        LLMRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var model = request.Model.Value;
        var uri = $"/models/{model}:streamGenerateContent?key={Configuration.ApiKey}&alt=sse";

        var geminiRequest = MapToGeminiRequest(request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = JsonContent.Create(geminiRequest, options: JsonOptions)
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

            var chunk = JsonSerializer.Deserialize<GeminiResponse>(json, JsonOptions);
            if (chunk?.Candidates?.Length > 0)
            {
                var content = chunk.Candidates[0].Content?.Parts?.FirstOrDefault()?.Text;
                if (!string.IsNullOrEmpty(content))
                {
                    yield return new LLMResponse
                    {
                        Id = Guid.NewGuid().ToString(),
                        Model = request.Model,
                        Content = content,
                        FinishReason = MapFinishReason(chunk.Candidates[0].FinishReason),
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
        var embeddings = new List<Embedding>();

        // Gemini peut traiter les embeddings en batch
        var uri = $"/models/{model}:batchEmbedContents?key={Configuration.ApiKey}";

        var geminiRequest = new GeminiBatchEmbeddingRequest
        {
            Requests = request.Inputs.Select(input => new GeminiEmbedRequest
            {
                Model = $"models/{model}",
                Content = new GeminiContent
                {
                    Parts = new List<GeminiPart>
                    {
                        new() { Text = input }
                    }
                }
            }).ToList()
        };

        var response = await HttpClient.PostAsJsonAsync(uri, geminiRequest, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GeminiBatchEmbeddingResponse>(
            JsonOptions, cancellationToken);

        for (var i = 0; i < result!.Embeddings.Count; i++)
        {
            embeddings.Add(new Embedding
            {
                Index = i,
                Vector = result.Embeddings[i].Values.ToList()
            });
        }

        return new EmbeddingResponse
        {
            Model = request.Model,
            Embeddings = embeddings,
            Usage = TokenUsage.FromValid(request.Inputs.Count * 100, 0) // Estimation
        };
    }

    #region Mapping

    private GeminiRequest MapToGeminiRequest(LLMRequest request)
    {
        var contents = new List<GeminiContent>();
        var systemInstruction = (GeminiContent?)null;

        foreach (var message in request.Messages)
        {
            if (message.Role == MessageRole.System)
            {
                // Gemini utilise systemInstruction séparé
                systemInstruction = new GeminiContent
                {
                    Parts = new List<GeminiPart>
                    {
                        new() { Text = message.Content }
                    }
                };
            }
            else
            {
                contents.Add(new GeminiContent
                {
                    Role = MapRole(message.Role),
                    Parts = new List<GeminiPart>
                    {
                        new() { Text = message.Content }
                    }
                });
            }
        }

        return new GeminiRequest
        {
            Contents = contents,
            SystemInstruction = systemInstruction,
            GenerationConfig = new GeminiGenerationConfig
            {
                Temperature = (float?)(request.Temperature),
                MaxOutputTokens = request.MaxTokens ?? Configuration.MaxTokens,
                TopP = (float?)(request.TopP),
                TopK = request.TopK,
                StopSequences = request.StopSequences?.ToList(),
                ResponseMimeType = request.ResponseFormat == ResponseFormat.Json
                    ? "application/json"
                    : null
            },
            SafetySettings = new List<GeminiSafetySetting>
            {
                new() { Category = "HARM_CATEGORY_HARASSMENT", Threshold = "BLOCK_NONE" },
                new() { Category = "HARM_CATEGORY_HATE_SPEECH", Threshold = "BLOCK_NONE" },
                new() { Category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", Threshold = "BLOCK_NONE" },
                new() { Category = "HARM_CATEGORY_DANGEROUS_CONTENT", Threshold = "BLOCK_NONE" }
            }
        };
    }

    private static string MapRole(MessageRole role)
    {
        return role switch
        {
            MessageRole.User => "user",
            MessageRole.Assistant => "model",
            _ => "user"
        };
    }

    private static LLMResponse MapToLLMResponse(GeminiResponse response, ModelIdentifier requestModel)
    {
        var candidate = response.Candidates?.FirstOrDefault();
        var content = candidate?.Content?.Parts?.FirstOrDefault()?.Text ?? string.Empty;

        return new LLMResponse
        {
            Id = Guid.NewGuid().ToString(),
            Model = requestModel,
            Content = content,
            FinishReason = MapFinishReason(candidate?.FinishReason),
            Usage = response.UsageMetadata != null
                ? TokenUsage.FromValid(
                    response.UsageMetadata.PromptTokenCount ?? 0,
                    response.UsageMetadata.CandidatesTokenCount ?? 0)
                : TokenUsage.Empty,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    private static FinishReason MapFinishReason(string? reason)
    {
        return reason?.ToUpperInvariant() switch
        {
            "STOP" => FinishReason.Stop,
            "MAX_TOKENS" => FinishReason.Length,
            "SAFETY" => FinishReason.ContentFilter,
            "RECITATION" => FinishReason.ContentFilter,
            _ => FinishReason.Stop
        };
    }

    #endregion

}
