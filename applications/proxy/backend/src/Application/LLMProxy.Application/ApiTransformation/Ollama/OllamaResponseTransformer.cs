using System.Text.Json;
using LLMProxy.Application.ApiTransformation.Interfaces;
using LLMProxy.Application.ApiTransformation.Ollama.Contracts;
using LLMProxy.Domain.LLM;

namespace LLMProxy.Application.ApiTransformation.Ollama;

/// <summary>
/// Transformer pour convertir les réponses canoniques vers le format Ollama.
/// </summary>
public sealed class OllamaResponseTransformer : IResponseTransformer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <inheritdoc />
    public ApiFormat SupportedFormat => ApiFormat.Ollama;

    /// <inheritdoc />
    public object TransformChatResponse(LLMResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return new OllamaChatResponse
        {
            Model = response.Model.Value,
            CreatedAt = response.CreatedAt.ToString("o"),
            Message = new OllamaMessage
            {
                Role = "assistant",
                Content = response.Content
            },
            Done = true,
            DoneReason = MapFinishReason(response.FinishReason),
            TotalDuration = response.Duration.HasValue
                ? (long)(response.Duration.Value.TotalNanoseconds)
                : null,
            PromptEvalCount = response.Usage.InputTokens,
            EvalCount = response.Usage.OutputTokens
        };
    }

    /// <inheritdoc />
    public string TransformStreamChunk(LLMResponse chunk)
    {
        ArgumentNullException.ThrowIfNull(chunk);

        var isLast = chunk.FinishReason != FinishReason.Stop || string.IsNullOrEmpty(chunk.Content);

        var response = new OllamaChatResponse
        {
            Model = chunk.Model.Value,
            CreatedAt = chunk.CreatedAt.ToString("o"),
            Message = new OllamaMessage
            {
                Role = "assistant",
                Content = chunk.Content
            },
            Done = isLast,
            DoneReason = isLast ? MapFinishReason(chunk.FinishReason) : null,
            PromptEvalCount = isLast ? chunk.Usage.InputTokens : null,
            EvalCount = isLast ? chunk.Usage.OutputTokens : null
        };

        return JsonSerializer.Serialize(response, JsonOptions);
    }

    /// <inheritdoc />
    public object TransformEmbeddingResponse(EmbeddingResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return new OllamaEmbeddingResponse
        {
            Model = response.Model.Value,
            Embeddings = response.Embeddings
                .Select(e => e.Vector.ToArray())
                .ToList(),
            PromptEvalCount = response.Usage.InputTokens
        };
    }

    /// <inheritdoc />
    public object TransformModelsResponse(IReadOnlyList<LLMModel> models)
    {
        ArgumentNullException.ThrowIfNull(models);

        return new OllamaTagsResponse
        {
            Models = models
                .Select(m => new OllamaModelInfo
                {
                    Name = m.Id.Value,
                    Model = m.Id.Value,
                    ModifiedAt = DateTimeOffset.UtcNow.ToString("o"),
                    Size = 0,
                    Digest = "sha256:" + Guid.NewGuid().ToString("N")[..12],
                    Details = new OllamaModelDetails
                    {
                        Family = m.OwnedBy,
                        ParameterSize = "unknown"
                    }
                })
                .ToList()
        };
    }

    /// <summary>
    /// Mappe la raison de fin vers le format Ollama.
    /// </summary>
    private static string? MapFinishReason(FinishReason reason)
    {
        return reason switch
        {
            FinishReason.Stop => "stop",
            FinishReason.Length => "length",
            _ => "stop"
        };
    }
}
