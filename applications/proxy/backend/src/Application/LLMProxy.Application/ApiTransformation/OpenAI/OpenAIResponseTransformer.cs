using System.Text.Json;
using LLMProxy.Application.ApiTransformation.Interfaces;
using LLMProxy.Application.ApiTransformation.OpenAI.Contracts;
using LLMProxy.Domain.LLM;

namespace LLMProxy.Application.ApiTransformation.OpenAI;

/// <summary>
/// Transformer pour convertir les réponses canoniques vers le format OpenAI.
/// </summary>
public sealed class OpenAIResponseTransformer : IResponseTransformer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <inheritdoc />
    public ApiFormat SupportedFormat => ApiFormat.OpenAI;

    /// <inheritdoc />
    public object TransformChatResponse(LLMResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return new OpenAIChatResponse
        {
            Id = response.Id,
            Model = response.Model.Value,
            Created = response.CreatedAt.ToUnixTimeSeconds(),
            Choices = new[]
            {
                new OpenAIChatChoice
                {
                    Index = 0,
                    Message = new OpenAIMessage
                    {
                        Role = "assistant",
                        Content = response.Content
                    },
                    FinishReason = MapFinishReason(response.FinishReason)
                }
            },
            Usage = new OpenAIUsage
            {
                PromptTokens = response.Usage.InputTokens,
                CompletionTokens = response.Usage.OutputTokens,
                TotalTokens = response.Usage.TotalTokens
            }
        };
    }

    /// <inheritdoc />
    public string TransformStreamChunk(LLMResponse chunk)
    {
        ArgumentNullException.ThrowIfNull(chunk);

        var streamChunk = new OpenAIStreamChunk
        {
            Id = chunk.Id,
            Model = chunk.Model.Value,
            Created = chunk.CreatedAt.ToUnixTimeSeconds(),
            Choices = new[]
            {
                new OpenAIStreamChoice
                {
                    Index = chunk.StreamIndex ?? 0,
                    Delta = new OpenAIDelta
                    {
                        Role = chunk.StreamIndex == 0 ? "assistant" : null,
                        Content = chunk.Content
                    },
                    FinishReason = chunk.FinishReason != FinishReason.Stop || string.IsNullOrEmpty(chunk.Content)
                        ? MapFinishReason(chunk.FinishReason)
                        : null
                }
            }
        };

        return JsonSerializer.Serialize(streamChunk, JsonOptions);
    }

    /// <inheritdoc />
    public object TransformEmbeddingResponse(EmbeddingResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return new OpenAIEmbeddingResponse
        {
            Model = response.Model.Value,
            Data = response.Embeddings
                .Select(e => new OpenAIEmbeddingData
                {
                    Index = e.Index,
                    Embedding = e.Vector.ToArray()
                })
                .ToList(),
            Usage = new OpenAIEmbeddingUsage
            {
                PromptTokens = response.Usage.InputTokens,
                TotalTokens = response.Usage.TotalTokens
            }
        };
    }

    /// <inheritdoc />
    public object TransformModelsResponse(IReadOnlyList<LLMModel> models)
    {
        ArgumentNullException.ThrowIfNull(models);

        return new OpenAIModelsResponse
        {
            Data = models
                .Select(m => new OpenAIModelInfo
                {
                    Id = m.Id.Value,
                    OwnedBy = m.OwnedBy ?? "unknown",
                    Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                })
                .ToList()
        };
    }

    /// <summary>
    /// Mappe la raison de fin vers le format OpenAI.
    /// </summary>
    private static string? MapFinishReason(FinishReason reason)
    {
        return reason switch
        {
            FinishReason.Stop => "stop",
            FinishReason.Length => "length",
            FinishReason.ToolCalls => "tool_calls",
            FinishReason.ContentFilter => "content_filter",
            _ => null
        };
    }
}
