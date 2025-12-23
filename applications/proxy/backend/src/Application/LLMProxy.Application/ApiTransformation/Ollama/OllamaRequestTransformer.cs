using System.Text.Json;
using LLMProxy.Application.ApiTransformation.Interfaces;
using LLMProxy.Application.ApiTransformation.Ollama.Contracts;
using LLMProxy.Domain.LLM;

namespace LLMProxy.Application.ApiTransformation.Ollama;

/// <summary>
/// Transformer pour convertir les requêtes du format Ollama vers le format canonique.
/// </summary>
public sealed class OllamaRequestTransformer : IRequestTransformer
{
    /// <inheritdoc />
    public ApiFormat SupportedFormat => ApiFormat.Ollama;

    /// <inheritdoc />
    public LLMRequest TransformChatRequest(object request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var ollamaRequest = request as OllamaChatRequest
            ?? throw new ArgumentException(
                "La requête doit être de type OllamaChatRequest",
                nameof(request));

        return new LLMRequest
        {
            Model = ModelIdentifier.FromValid(ollamaRequest.Model),
            Messages = ollamaRequest.Messages
                .Select(m => new LLMMessage
                {
                    Role = ParseRole(m.Role),
                    Content = m.Content
                })
                .ToList(),
            Temperature = ollamaRequest.Options?.Temperature,
            MaxTokens = ollamaRequest.Options?.NumPredict,
            TopP = ollamaRequest.Options?.TopP,
            TopK = ollamaRequest.Options?.TopK,
            Stream = ollamaRequest.Stream,
            StopSequences = ollamaRequest.Options?.Stop,
            Seed = ollamaRequest.Options?.Seed
        };
    }

    /// <inheritdoc />
    public EmbeddingRequest TransformEmbeddingRequest(object request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var ollamaRequest = request as OllamaEmbeddingRequest
            ?? throw new ArgumentException(
                "La requête doit être de type OllamaEmbeddingRequest",
                nameof(request));

        var inputs = ExtractInputTexts(ollamaRequest.Input);

        return new EmbeddingRequest
        {
            Model = ModelIdentifier.FromValid(ollamaRequest.Model),
            Inputs = inputs
        };
    }

    /// <summary>
    /// Parse le rôle depuis la chaîne Ollama.
    /// </summary>
    private static MessageRole ParseRole(string role)
    {
        return role.ToLowerInvariant() switch
        {
            "system" => MessageRole.System,
            "user" => MessageRole.User,
            "assistant" => MessageRole.Assistant,
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
