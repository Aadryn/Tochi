using System.Text.Json;
using LLMProxy.Application.ApiTransformation.Interfaces;
using LLMProxy.Application.ApiTransformation.OpenAI.Contracts;
using LLMProxy.Domain.LLM;

namespace LLMProxy.Application.ApiTransformation.OpenAI;

/// <summary>
/// Transformer pour convertir les requêtes du format OpenAI vers le format canonique.
/// </summary>
public sealed class OpenAIRequestTransformer : IRequestTransformer
{
    /// <inheritdoc />
    public ApiFormat SupportedFormat => ApiFormat.OpenAI;

    /// <inheritdoc />
    public LLMRequest TransformChatRequest(object request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var openAiRequest = request as OpenAIChatRequest
            ?? throw new ArgumentException(
                "La requête doit être de type OpenAIChatRequest",
                nameof(request));

        return new LLMRequest
        {
            Model = ModelIdentifier.FromValid(openAiRequest.Model),
            Messages = openAiRequest.Messages
                .Select(m => new LLMMessage
                {
                    Role = ParseRole(m.Role),
                    Content = m.Content
                })
                .ToList(),
            Temperature = openAiRequest.Temperature,
            MaxTokens = openAiRequest.MaxTokens,
            TopP = openAiRequest.TopP,
            N = openAiRequest.N,
            Stream = openAiRequest.Stream,
            StopSequences = openAiRequest.Stop,
            PresencePenalty = openAiRequest.PresencePenalty,
            FrequencyPenalty = openAiRequest.FrequencyPenalty,
            UserId = openAiRequest.User,
            Seed = openAiRequest.Seed
        };
    }

    /// <inheritdoc />
    public EmbeddingRequest TransformEmbeddingRequest(object request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var openAiRequest = request as OpenAIEmbeddingRequest
            ?? throw new ArgumentException(
                "La requête doit être de type OpenAIEmbeddingRequest",
                nameof(request));

        var inputs = ExtractInputTexts(openAiRequest.Input);

        return new EmbeddingRequest
        {
            Model = ModelIdentifier.FromValid(openAiRequest.Model),
            Inputs = inputs,
            Dimensions = openAiRequest.Dimensions
        };
    }

    /// <summary>
    /// Parse le rôle depuis la chaîne OpenAI.
    /// </summary>
    private static MessageRole ParseRole(string role)
    {
        return role.ToLowerInvariant() switch
        {
            "system" => MessageRole.System,
            "user" => MessageRole.User,
            "assistant" => MessageRole.Assistant,
            "function" => MessageRole.Function,
            "tool" => MessageRole.Tool,
            _ => MessageRole.User
        };
    }

    /// <summary>
    /// Extrait les textes d'entrée (peut être une chaîne ou une liste).
    /// </summary>
    private static IReadOnlyList<string> ExtractInputTexts(object input)
    {
        if (input is string text)
        {
            return new[] { text };
        }

        if (input is JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                return new[] { element.GetString()! };
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                return element.EnumerateArray()
                    .Select(e => e.GetString()!)
                    .ToList();
            }
        }

        if (input is IEnumerable<string> list)
        {
            return list.ToList();
        }

        throw new ArgumentException(
            "L'entrée doit être une chaîne ou une liste de chaînes",
            nameof(input));
    }
}
