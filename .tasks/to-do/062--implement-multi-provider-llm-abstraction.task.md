# Tâche 062 - Implémenter Abstraction Multi-Providers LLM

## PRIORITÉ
🟡 **P3 - MOYENNE** (Priorité 7/8 de la refonte)

## OBJECTIF

Créer une couche d'abstraction uniforme pour 9 providers LLM (Ollama, vLLM, OVH Cloud, Scaleway, OpenAI, Azure OpenAI, AWS Bedrock, Anthropic, Mistral AI), permettant le routage intelligent et le failover.

## CONTEXTE

### Providers Cibles
| Provider | Type | API Format | Auth |
|----------|------|------------|------|
| Ollama | Local | Ollama native | None |
| vLLM | Local | OpenAI-compatible | API Key |
| OVH Cloud AI | Private Cloud | OpenAI-compatible | Token |
| Scaleway Generative | Private Cloud | OpenAI-compatible | Secret Key |
| OpenAI | Public Cloud | OpenAI native | API Key |
| Azure OpenAI | Public Cloud | OpenAI-compatible | API Key + Endpoint |
| AWS Bedrock | Public Cloud | AWS native | IAM/SigV4 |
| Anthropic | Public Cloud | Anthropic native | API Key |
| Mistral AI | Public Cloud | OpenAI-compatible | API Key |

### Patterns d'Architecture
- **Strategy Pattern** : Un ILLMProvider par provider
- **Factory Pattern** : Création des clients selon configuration
- **Adapter Pattern** : Normalisation des réponses

## IMPLÉMENTATION

### Phase 1 : Domain Layer (Abstractions)
```
src/Core/LLMProxy.Domain/
├── Interfaces/
│   ├── ILLMProvider.cs           # Interface commune
│   └── ILLMProviderFactory.cs
├── Entities/LLM/
│   ├── LLMRequest.cs             # Requête normalisée
│   ├── LLMResponse.cs            # Réponse normalisée
│   ├── LLMMessage.cs             # Message chat
│   ├── LLMModel.cs               # Modèle disponible
│   └── ProviderCapabilities.cs   # Capacités du provider
├── ValueObjects/
│   ├── ModelIdentifier.cs        # "gpt-4", "claude-3-opus"
│   ├── ProviderType.cs           # Enum des providers
│   └── TokenUsage.cs             # Input/Output tokens
└── Events/
    ├── LLMRequestStarted.cs
    ├── LLMRequestCompleted.cs
    └── LLMRequestFailed.cs
```

```csharp
// ILLMProvider.cs
public interface ILLMProvider
{
    ProviderType Type { get; }
    string Name { get; }
    
    Task<ProviderCapabilities> GetCapabilitiesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<LLMModel>> ListModelsAsync(CancellationToken ct = default);
    Task<bool> IsHealthyAsync(CancellationToken ct = default);
    
    Task<LLMResponse> ChatCompletionAsync(LLMRequest request, CancellationToken ct = default);
    IAsyncEnumerable<LLMResponse> ChatCompletionStreamAsync(LLMRequest request, CancellationToken ct = default);
    Task<EmbeddingResponse> EmbeddingsAsync(EmbeddingRequest request, CancellationToken ct = default);
}
```

### Phase 2 : Infrastructure Layer (Implémentations)
```
src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/
├── Providers/
│   ├── Local/
│   │   ├── OllamaProvider.cs
│   │   └── VLLMProvider.cs
│   ├── PrivateCloud/
│   │   ├── OVHCloudProvider.cs
│   │   └── ScalewayProvider.cs
│   └── PublicCloud/
│       ├── OpenAIProvider.cs
│       ├── AzureOpenAIProvider.cs
│       ├── AWSBedrockProvider.cs
│       ├── AnthropicProvider.cs
│       └── MistralProvider.cs
├── Adapters/
│   ├── OpenAIFormatAdapter.cs    # Convertit vers format OpenAI
│   ├── AnthropicFormatAdapter.cs
│   └── BedrockFormatAdapter.cs
├── Clients/
│   ├── HttpClientFactory.cs
│   └── RetryPolicyHandler.cs
├── Configuration/
│   ├── ProviderConfiguration.cs
│   └── ProviderCredentials.cs
└── Factory/
    └── LLMProviderFactory.cs
```

### Phase 3 : Application Layer (Orchestration)
```
src/Application/LLMProxy.Application/
├── Features/
│   └── Providers/
│       ├── Services/
│       │   ├── ProviderOrchestrator.cs    # Routing intelligent
│       │   ├── ProviderSelector.cs        # Sélection par critères
│       │   └── FailoverManager.cs         # Gestion failover
│       └── Queries/
│           ├── GetBestProvider/
│           └── GetProviderMetrics/
```

### Phase 4 : Configuration
```json
{
  "LLMProviders": {
    "Providers": [
      {
        "Type": "OpenAI",
        "Name": "openai-primary",
        "Enabled": true,
        "Priority": 1,
        "Configuration": {
          "BaseUrl": "https://api.openai.com/v1",
          "ApiKey": "${OPENAI_API_KEY}",
          "DefaultModel": "gpt-4o",
          "MaxTokens": 4096
        }
      },
      {
        "Type": "Ollama",
        "Name": "ollama-local",
        "Enabled": true,
        "Priority": 2,
        "Configuration": {
          "BaseUrl": "http://localhost:11434",
          "DefaultModel": "llama3.1"
        }
      },
      {
        "Type": "AzureOpenAI",
        "Name": "azure-eu",
        "Enabled": true,
        "Priority": 3,
        "Configuration": {
          "Endpoint": "https://myresource.openai.azure.com/",
          "ApiKey": "${AZURE_OPENAI_KEY}",
          "DeploymentName": "gpt-4o-deployment",
          "ApiVersion": "2024-02-01"
        }
      }
    ],
    "Routing": {
      "Strategy": "PriorityWithFallback",
      "HealthCheckInterval": "00:00:30",
      "FailoverThreshold": 3
    }
  }
}
```

## CRITÈRES DE SUCCÈS

- [ ] Interface ILLMProvider avec 6 méthodes
- [ ] 9 providers implémentés
- [ ] Normalisation request/response
- [ ] Streaming support (IAsyncEnumerable)
- [ ] Factory pattern pour création providers
- [ ] Health checks par provider
- [ ] Failover automatique
- [ ] Configuration externalisée
- [ ] Tests unitaires par provider (mocks HTTP)
- [ ] Tests d'intégration avec Ollama local
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Documentation XML complète (français)

## DÉPENDANCES

- Ollama installé localement pour tests
- Credentials des providers cloud (variables env)

## ESTIMATION

**Effort** : 20h (9 providers)
**Complexité** : Haute

## RÉFÉRENCES

- [OpenAI API](https://platform.openai.com/docs/api-reference)
- [Anthropic API](https://docs.anthropic.com/claude/reference)
- [AWS Bedrock](https://docs.aws.amazon.com/bedrock/)
- [Ollama API](https://github.com/ollama/ollama/blob/main/docs/api.md)
- ADR-034 (Third-Party Library Encapsulation)
