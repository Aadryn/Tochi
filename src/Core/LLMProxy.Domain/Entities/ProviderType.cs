namespace LLMProxy.Domain.Entities;

/// <summary>
/// Types de fournisseurs LLM supportés
/// </summary>
public enum ProviderType
{
    OpenAI = 0,
    AzureOpenAI = 1,
    Ollama = 2,
    Anthropic = 3,
    Mistral = 4,
    GoogleGemini = 5,
    Cohere = 6,
    HuggingFace = 7,
    Custom = 99
}
