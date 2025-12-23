using LLMProxy.Domain.LLM;
using Microsoft.AspNetCore.Http;

namespace LLMProxy.Application.ApiTransformation;

/// <summary>
/// Service de détection automatique du format API basé sur le chemin de requête et les headers.
/// </summary>
public sealed class ApiFormatDetector : IApiFormatDetector
{
    private const string OpenAIPathPrefix = "/v1/";
    private const string OllamaPathPrefix = "/api/";
    private const string FormatHeaderName = "X-Api-Format";

    /// <inheritdoc />
    public ApiFormat DetectFormat(HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 1. Vérifier le header explicite en priorité
        if (TryDetectFromHeader(request, out var format))
        {
            return format;
        }

        // 2. Détecter à partir du chemin
        return DetectFromPath(request.Path.Value ?? string.Empty);
    }

    /// <inheritdoc />
    public ApiFormat DetectFromPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return ApiFormat.OpenAI; // Format par défaut
        }

        // Normaliser le chemin
        var normalizedPath = path.ToLowerInvariant();

        // Routes OpenAI
        if (normalizedPath.StartsWith(OpenAIPathPrefix, StringComparison.Ordinal))
        {
            return ApiFormat.OpenAI;
        }

        // Routes Ollama
        if (normalizedPath.StartsWith(OllamaPathPrefix, StringComparison.Ordinal))
        {
            return ApiFormat.Ollama;
        }

        // Par défaut, utiliser OpenAI (le format le plus courant)
        return ApiFormat.OpenAI;
    }

    /// <inheritdoc />
    public bool TryDetectFromHeader(HttpRequest request, out ApiFormat format)
    {
        ArgumentNullException.ThrowIfNull(request);

        format = ApiFormat.OpenAI;

        if (!request.Headers.TryGetValue(FormatHeaderName, out var headerValue))
        {
            return false;
        }

        var value = headerValue.FirstOrDefault()?.ToLowerInvariant();

        return value switch
        {
            "openai" => SetFormat(out format, ApiFormat.OpenAI),
            "ollama" => SetFormat(out format, ApiFormat.Ollama),
            _ => false
        };
    }

    /// <summary>
    /// Méthode utilitaire pour assigner le format et retourner true.
    /// </summary>
    private static bool SetFormat(out ApiFormat format, ApiFormat value)
    {
        format = value;
        return true;
    }
}
