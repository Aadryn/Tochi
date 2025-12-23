using LLMProxy.Domain.Entities;
using LLMProxy.Domain.LLM;
using LLMProxy.Infrastructure.LLMProviders.Configuration;
using LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.LLMProviders.Providers;

/// <summary>
/// Client générique pour les APIs OpenAI-compatibles.
/// Réutilise l'implémentation OpenAI avec des overrides mineurs.
/// Utilisé pour : vLLM, Mistral AI, OVH Cloud, Scaleway.
/// </summary>
public sealed class OpenAICompatibleProviderClient : OpenAIProviderClient
{
    private readonly ProviderType _providerType;

    /// <inheritdoc />
    public override ProviderType Type => _providerType;

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="httpClient">Client HTTP configuré.</param>
    /// <param name="configuration">Configuration du provider.</param>
    /// <param name="logger">Logger.</param>
    public OpenAICompatibleProviderClient(
        HttpClient httpClient,
        LLMProviderConfiguration configuration,
        ILogger<OpenAICompatibleProviderClient> logger)
        : base(httpClient, configuration, logger)
    {
        _providerType = configuration.Type;
    }

    /// <inheritdoc />
    public override async Task<ProviderCapabilities> GetCapabilitiesAsync(
        CancellationToken cancellationToken = default)
    {
        var baseCapabilities = await base.GetCapabilitiesAsync(cancellationToken);

        return baseCapabilities with
        {
            Type = _providerType,
            Name = Name,
            // Ajuster les capacités selon le provider
            SupportedCapabilities = GetProviderSpecificCapabilities(),
            SupportsEmbeddings = SupportsEmbeddings(),
            SupportsFunctionCalling = SupportsFunctionCalling(),
            SupportsVision = SupportsVision()
        };
    }

    /// <inheritdoc />
    public override async Task<IReadOnlyList<LLMModel>> ListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Tenter d'utiliser l'endpoint OpenAI standard
            return await base.ListModelsAsync(cancellationToken);
        }
        catch
        {
            // Fallback : retourner les modèles connus pour ce provider
            return GetKnownModels();
        }
    }

    private ModelCapabilities GetProviderSpecificCapabilities()
    {
        return _providerType switch
        {
            ProviderType.Mistral => ModelCapabilities.ChatCompletion
                                    | ModelCapabilities.Streaming
                                    | ModelCapabilities.Embeddings
                                    | ModelCapabilities.FunctionCalling
                                    | ModelCapabilities.JsonMode,

            ProviderType.VLLM => ModelCapabilities.ChatCompletion
                                 | ModelCapabilities.TextCompletion
                                 | ModelCapabilities.Streaming,

            ProviderType.OVHCloud => ModelCapabilities.ChatCompletion
                                     | ModelCapabilities.Streaming,

            ProviderType.Scaleway => ModelCapabilities.ChatCompletion
                                     | ModelCapabilities.Streaming,

            _ => ModelCapabilities.ChatCompletion | ModelCapabilities.Streaming
        };
    }

    private bool SupportsEmbeddings()
    {
        return _providerType == ProviderType.Mistral;
    }

    private bool SupportsFunctionCalling()
    {
        return _providerType == ProviderType.Mistral;
    }

    private bool SupportsVision()
    {
        return false; // La plupart des providers compatibles ne supportent pas la vision
    }

    private IReadOnlyList<LLMModel> GetKnownModels()
    {
        return _providerType switch
        {
            ProviderType.Mistral => new List<LLMModel>
            {
                CreateModel("mistral-tiny", "Mistral Tiny", 32000),
                CreateModel("mistral-small-latest", "Mistral Small", 32000),
                CreateModel("mistral-medium-latest", "Mistral Medium", 32000),
                CreateModel("mistral-large-latest", "Mistral Large", 32000),
                CreateModel("open-mistral-7b", "Open Mistral 7B", 32000),
                CreateModel("open-mixtral-8x7b", "Open Mixtral 8x7B", 32000),
                CreateModel("open-mixtral-8x22b", "Open Mixtral 8x22B", 65000),
                CreateModel("codestral-latest", "Codestral", 32000),
                CreateModel("mistral-embed", "Mistral Embed", 8192)
            },

            ProviderType.VLLM => new List<LLMModel>
            {
                // Les modèles vLLM dépendent de l'installation locale
                CreateModel(Configuration.DefaultModel ?? "default", "vLLM Model", 4096)
            },

            ProviderType.OVHCloud => new List<LLMModel>
            {
                CreateModel("Mixtral-8x22B-Instruct-v0.1", "Mixtral 8x22B", 65000),
                CreateModel("Mixtral-8x7B-Instruct-v0.1", "Mixtral 8x7B", 32000),
                CreateModel("Mistral-7B-Instruct-v0.2", "Mistral 7B", 32000),
                CreateModel("Llama-3-70B-Instruct", "Llama 3 70B", 8192),
                CreateModel("Llama-3-8B-Instruct", "Llama 3 8B", 8192)
            },

            ProviderType.Scaleway => new List<LLMModel>
            {
                CreateModel("llama-3.1-70b-instruct", "Llama 3.1 70B", 128000),
                CreateModel("llama-3.1-8b-instruct", "Llama 3.1 8B", 128000),
                CreateModel("mistral-nemo-instruct-2407", "Mistral NeMo", 128000),
                CreateModel("qwen2.5-coder-32b-instruct", "Qwen 2.5 Coder", 32000)
            },

            _ => new List<LLMModel>()
        };
    }

    private static LLMModel CreateModel(string id, string name, int contextLength)
    {
        return new LLMModel
        {
            Id = ModelIdentifier.FromValid(id),
            Name = name,
            ContextLength = contextLength,
            Capabilities = ModelCapabilities.ChatCompletion | ModelCapabilities.Streaming,
            IsAvailable = true
        };
    }
}
