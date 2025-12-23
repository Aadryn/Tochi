using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Interfaces;
using SharpToken;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LLMProxy.Infrastructure.LLMProviders;

/// <summary>
/// Token counting service using SharpToken for local estimation and response parsing
/// </summary>
public partial class TokenCounterService : ITokenCounterService
{
    private readonly Dictionary<string, GptEncoding> _encodingCache = new();
    private readonly SemaphoreSlim _cacheLock = new(1, 1);

    /// <summary>
    /// Estime le nombre de tokens dans un texte pour un modèle donné.
    /// </summary>
    /// <param name="text">Texte à analyser.</param>
    /// <param name="model">Nom du modèle pour déterminer l'encodage.</param>
    /// <returns>Nombre estimé de tokens.</returns>
    public int EstimateTokens(string text, string model)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        var encoding = GetEncodingForModel(model);
        return encoding.Encode(text).Count;
    }

    /// <summary>
    /// Estime le nombre de tokens dans une requête sérialisée.
    /// </summary>
    /// <param name="request">Objet de requête à sérialiser.</param>
    /// <param name="modelName">Nom du modèle pour déterminer l'encodage.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Nombre estimé de tokens.</returns>
    public async Task<int> EstimateTokensAsync(object request, string modelName, CancellationToken cancellationToken = default)
    {
        // Serialize request to JSON and count tokens
        var json = JsonSerializer.Serialize(request);
        return await EstimateTokensAsync(json, modelName, cancellationToken);
    }

    /// <summary>
    /// Parse les tokens d'entrée et de sortie depuis une réponse JSON.
    /// </summary>
    /// <param name="responseBody">Corps de la réponse JSON.</param>
    /// <param name="providerType">Type de provider pour adapter le parsing.</param>
    /// <returns>Tuple (inputTokens, outputTokens).</returns>
    public (long inputTokens, long outputTokens) ParseTokensFromResponse(string responseBody, ProviderType providerType)
    {
        try
        {
            using var jsonDoc = JsonDocument.Parse(responseBody);
            
            if (TryGetTokensFromUsage(jsonDoc, out var tokens))
            {
                return tokens;
            }

            if (TryEstimateFromContent(jsonDoc, out var estimatedTokens))
            {
                return estimatedTokens;
            }
        }
        catch (JsonException)
        {
            return EstimateFromRawText(responseBody);
        }

        return (0, 0);
    }

    private bool TryGetTokensFromUsage(JsonDocument jsonDoc, out (long input, long output) tokens)
    {
        tokens = (0, 0);
        
        if (!jsonDoc.RootElement.TryGetProperty("usage", out var usage))
        {
            return false;
        }

        var promptTokens = usage.TryGetProperty("prompt_tokens", out var pt) ? pt.GetInt64() : 0;
        var completionTokens = usage.TryGetProperty("completion_tokens", out var ct) ? ct.GetInt64() : 0;
        tokens = (promptTokens, completionTokens);
        
        return true;
    }

    private bool TryEstimateFromContent(JsonDocument jsonDoc, out (long input, long output) tokens)
    {
        tokens = (0, 0);
        
        if (!jsonDoc.RootElement.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
        {
            return false;
        }

        var firstChoice = choices[0];
        if (!firstChoice.TryGetProperty("message", out var message) && 
            !firstChoice.TryGetProperty("delta", out message))
        {
            return false;
        }

        if (!message.TryGetProperty("content", out var content))
        {
            return false;
        }

        var text = content.GetString() ?? string.Empty;
        var estimatedTokens = text.Length / 4;
        tokens = (0, estimatedTokens);
        
        return true;
    }

    private (long input, long output) EstimateFromRawText(string text)
    {
        var tokens = text.Length / 4;
        return (0, tokens);
    }

    /// <summary>
    /// Parse les tokens depuis un chunk de streaming SSE.
    /// </summary>
    /// <param name="chunk">Chunk SSE à parser.</param>
    /// <param name="modelName">Nom du modèle pour estimer les tokens.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Tuple (promptTokens, completionTokens).</returns>
    public async Task<(int promptTokens, int completionTokens)> ParseTokensFromStreamChunkAsync(
        string chunk, 
        string modelName, 
        CancellationToken cancellationToken = default)
    {
        // Parse SSE format: data: {...}
        var dataMatch = DataLineRegex().Match(chunk);
        if (!dataMatch.Success)
        {
            return (0, 0);
        }

        var jsonPart = dataMatch.Groups[1].Value.Trim();
        
        if (jsonPart == "[DONE]")
        {
            return (0, 0);
        }

        try
        {
            var jsonDoc = JsonDocument.Parse(jsonPart);
            
            // Check for usage in streaming response (final chunk)
            if (jsonDoc.RootElement.TryGetProperty("usage", out var usage))
            {
                var promptTokens = usage.TryGetProperty("prompt_tokens", out var pt) ? pt.GetInt32() : 0;
                var completionTokens = usage.TryGetProperty("completion_tokens", out var ct) ? ct.GetInt32() : 0;
                return (promptTokens, completionTokens);
            }

            // Estimate from delta content
            if (jsonDoc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var delta = choices[0];
                if (delta.TryGetProperty("delta", out var deltaContent))
                {
                    if (deltaContent.TryGetProperty("content", out var content))
                    {
                        var text = content.GetString() ?? string.Empty;
                        if (!string.IsNullOrEmpty(text))
                        {
                            var tokens = await EstimateTokensAsync(text, modelName, cancellationToken);
                            return (0, tokens);
                        }
                    }
                }
            }
        }
        catch (JsonException)
        {
            // Invalid JSON in chunk, ignore
        }

        return (0, 0);
    }

    /// <summary>
    /// Récupère le nom de l'encodage tiktoken approprié pour un modèle.
    /// </summary>
    /// <param name="modelName">Nom du modèle.</param>
    /// <returns>Nom de l'encodage (ex: cl100k_base, p50k_base).</returns>
    public string GetEncodingNameForModel(string modelName)
    {
        // Normalize model name
        var normalizedModel = modelName.ToLowerInvariant();

        // GPT-4 and newer models
        if (normalizedModel.Contains("gpt-4") || normalizedModel.Contains("gpt-4o"))
        {
            return "cl100k_base";
        }

        // GPT-3.5-turbo and text-embedding-ada-002
        if (normalizedModel.Contains("gpt-3.5-turbo") || 
            normalizedModel.Contains("text-embedding-ada-002") ||
            normalizedModel.Contains("text-embedding-3"))
        {
            return "cl100k_base";
        }

        // Older GPT-3 models
        if (normalizedModel.Contains("davinci") || 
            normalizedModel.Contains("curie") || 
            normalizedModel.Contains("babbage") || 
            normalizedModel.Contains("ada"))
        {
            return "p50k_base";
        }

        // Code models
        if (normalizedModel.Contains("code"))
        {
            return "p50k_base";
        }

        // Anthropic Claude
        if (normalizedModel.Contains("claude"))
        {
            return "cl100k_base"; // Use OpenAI encoding as approximation
        }

        // Mistral
        if (normalizedModel.Contains("mistral"))
        {
            return "cl100k_base"; // Use OpenAI encoding as approximation
        }

        // Ollama models - use cl100k_base as default
        if (normalizedModel.Contains("llama") || 
            normalizedModel.Contains("mixtral") || 
            normalizedModel.Contains("gemma"))
        {
            return "cl100k_base";
        }

        // Default to most common modern encoding
        return "cl100k_base";
    }

    private GptEncoding GetEncodingForModel(string modelName)
    {
        var encodingName = GetEncodingNameForModel(modelName);

        _cacheLock.Wait();
        try
        {
            if (!_encodingCache.TryGetValue(encodingName, out var encoding))
            {
                encoding = GptEncoding.GetEncoding(encodingName);
                _encodingCache[encodingName] = encoding;
            }

            return encoding;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    [GeneratedRegex(@"data:\s*(.+)")]
    private static partial Regex DataLineRegex();
}
