using LLMProxy.Application.ApiTransformation.Interfaces;
using LLMProxy.Application.ApiTransformation.Ollama;
using LLMProxy.Application.ApiTransformation.OpenAI;
using LLMProxy.Domain.LLM;

namespace LLMProxy.Application.ApiTransformation;

/// <summary>
/// Factory pour obtenir les transformers appropriés selon le format API demandé.
/// </summary>
/// <remarks>
/// Cette factory centralise la création des transformers et permet une résolution
/// efficace basée sur l'énumération <see cref="ApiFormat"/>.
/// </remarks>
public sealed class TransformerFactory : ITransformerFactory
{
    private readonly Dictionary<ApiFormat, IRequestTransformer> _requestTransformers;
    private readonly Dictionary<ApiFormat, IResponseTransformer> _responseTransformers;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="TransformerFactory"/>.
    /// </summary>
    public TransformerFactory()
    {
        _requestTransformers = new Dictionary<ApiFormat, IRequestTransformer>
        {
            [ApiFormat.OpenAI] = new OpenAIRequestTransformer(),
            [ApiFormat.Ollama] = new OllamaRequestTransformer()
        };

        _responseTransformers = new Dictionary<ApiFormat, IResponseTransformer>
        {
            [ApiFormat.OpenAI] = new OpenAIResponseTransformer(),
            [ApiFormat.Ollama] = new OllamaResponseTransformer()
        };
    }

    /// <summary>
    /// Initialise une nouvelle instance avec des transformers personnalisés.
    /// </summary>
    /// <param name="requestTransformers">Collection de transformers de requêtes.</param>
    /// <param name="responseTransformers">Collection de transformers de réponses.</param>
    public TransformerFactory(
        IEnumerable<IRequestTransformer> requestTransformers,
        IEnumerable<IResponseTransformer> responseTransformers)
    {
        ArgumentNullException.ThrowIfNull(requestTransformers);
        ArgumentNullException.ThrowIfNull(responseTransformers);

        _requestTransformers = requestTransformers
            .ToDictionary(t => t.SupportedFormat);

        _responseTransformers = responseTransformers
            .ToDictionary(t => t.SupportedFormat);
    }

    /// <inheritdoc />
    public IRequestTransformer GetRequestTransformer(ApiFormat format)
    {
        if (!_requestTransformers.TryGetValue(format, out var transformer))
        {
            throw new NotSupportedException(
                $"Aucun transformer de requête disponible pour le format '{format}'.");
        }

        return transformer;
    }

    /// <inheritdoc />
    public IResponseTransformer GetResponseTransformer(ApiFormat format)
    {
        if (!_responseTransformers.TryGetValue(format, out var transformer))
        {
            throw new NotSupportedException(
                $"Aucun transformer de réponse disponible pour le format '{format}'.");
        }

        return transformer;
    }

    /// <inheritdoc />
    public bool IsFormatSupported(ApiFormat format)
    {
        return _requestTransformers.ContainsKey(format)
            && _responseTransformers.ContainsKey(format);
    }

    /// <inheritdoc />
    public IEnumerable<ApiFormat> GetSupportedFormats()
    {
        return _requestTransformers.Keys.Intersect(_responseTransformers.Keys);
    }
}
