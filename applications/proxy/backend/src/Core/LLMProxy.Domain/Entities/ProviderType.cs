namespace LLMProxy.Domain.Entities;

/// <summary>
/// Types de fournisseurs LLM supportés par le proxy.
/// Chaque type correspond à une implémentation spécifique
/// dans le projet Infrastructure.LLMProviders.
/// </summary>
public enum ProviderType
{
    /// <summary>
    /// OpenAI API native (GPT-3.5, GPT-4, etc.).
    /// Format API : OpenAI standard.
    /// Authentification : API Key.
    /// </summary>
    OpenAI = 0,

    /// <summary>
    /// Azure OpenAI Service.
    /// Format API : OpenAI-compatible avec endpoint Azure.
    /// Authentification : API Key + Resource Endpoint.
    /// </summary>
    AzureOpenAI = 1,

    /// <summary>
    /// Ollama local (Llama, Mistral, etc.).
    /// Format API : Ollama native / OpenAI-compatible.
    /// Authentification : Aucune (local).
    /// </summary>
    Ollama = 2,

    /// <summary>
    /// Anthropic Claude (Claude 3, Claude 2, etc.).
    /// Format API : Anthropic native.
    /// Authentification : API Key.
    /// </summary>
    Anthropic = 3,

    /// <summary>
    /// Mistral AI (Mistral, Mixtral, etc.).
    /// Format API : OpenAI-compatible.
    /// Authentification : API Key.
    /// </summary>
    Mistral = 4,

    /// <summary>
    /// Google Gemini (Gemini Pro, Ultra, etc.).
    /// Format API : Google AI native.
    /// Authentification : API Key.
    /// </summary>
    GoogleGemini = 5,

    /// <summary>
    /// Cohere (Command, Embed, etc.).
    /// Format API : Cohere native.
    /// Authentification : API Key.
    /// </summary>
    Cohere = 6,

    /// <summary>
    /// HuggingFace Inference API.
    /// Format API : HuggingFace native.
    /// Authentification : Bearer Token.
    /// </summary>
    HuggingFace = 7,

    /// <summary>
    /// vLLM Server (optimisé pour le serving).
    /// Format API : OpenAI-compatible.
    /// Authentification : API Key (optionnel).
    /// </summary>
    VLLM = 8,

    /// <summary>
    /// OVH Cloud AI Endpoints.
    /// Format API : OpenAI-compatible.
    /// Authentification : Token OVH.
    /// </summary>
    OVHCloud = 9,

    /// <summary>
    /// Scaleway Generative AI.
    /// Format API : OpenAI-compatible.
    /// Authentification : Secret Key.
    /// </summary>
    Scaleway = 10,

    /// <summary>
    /// AWS Bedrock (Claude, Llama, etc.).
    /// Format API : AWS native (SigV4).
    /// Authentification : IAM Credentials.
    /// </summary>
    AWSBedrock = 11,

    /// <summary>
    /// Provider personnalisé avec configuration manuelle.
    /// Format API : Configurable.
    /// Authentification : Configurable.
    /// </summary>
    Custom = 99
}
