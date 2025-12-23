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

- [x] Interface ILLMProvider avec 6 méthodes (ILLMProviderClient)
- [x] 9 providers implémentés (Ollama, vLLM, OVH, Scaleway, OpenAI, AzureOpenAI, Bedrock, Anthropic, Mistral)
- [x] Normalisation request/response (LLMRequest/LLMResponse)
- [x] Streaming support (IAsyncEnumerable<LLMResponse>)
- [x] Factory pattern pour création providers (ILLMProviderClientFactory)
- [x] Health checks par provider (IsHealthyAsync)
- [x] Failover automatique (FailoverManager)
- [x] Configuration externalisée (ProviderSettings, DI extensions)
- [x] Tests unitaires services (21 tests passent)
- [ ] Tests d'intégration avec Ollama local (non requis immédiatement)
- [x] Build : 0 erreurs, 0 warnings
- [x] Documentation XML complète (français)

## RAPPORT DE COMPLÉTION

### Fichiers Créés

#### Phase 1 : Domain Layer (Abstractions)
- `src/Core/LLMProxy.Domain/LLM/LLMRequest.cs` - Requête normalisée
- `src/Core/LLMProxy.Domain/LLM/LLMResponse.cs` - Réponse normalisée  
- `src/Core/LLMProxy.Domain/LLM/LLMMessage.cs` - Message chat
- `src/Core/LLMProxy.Domain/LLM/LLMModel.cs` - Modèle disponible
- `src/Core/LLMProxy.Domain/LLM/ModelIdentifier.cs` - Value Object
- `src/Core/LLMProxy.Domain/LLM/TokenUsage.cs` - Usage tokens
- `src/Core/LLMProxy.Domain/LLM/EmbeddingRequest.cs` - Requête embeddings
- `src/Core/LLMProxy.Domain/LLM/EmbeddingResponse.cs` - Réponse embeddings
- `src/Core/LLMProxy.Domain/LLM/ILLMProviderClient.cs` - Interface commune
- `src/Core/LLMProxy.Domain/Entities/ProviderType.cs` - Enum 9 providers
- `src/Core/LLMProxy.Domain/Entities/ProviderCapabilities.cs` - Capacités provider
- `src/Core/LLMProxy.Domain/Entities/ModelCapabilities.cs` - Capacités modèle

#### Phase 2 : Infrastructure Layer (9 Providers)
- `src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/LLMProxy.Infrastructure.LLMProviders.csproj`
- **Providers Local:**
  - `Providers/Local/OllamaProviderClient.cs`
  - `Providers/Local/VLLMProviderClient.cs`
- **Providers Private Cloud:**
  - `Providers/PrivateCloud/OVHCloudProviderClient.cs`
  - `Providers/PrivateCloud/ScalewayProviderClient.cs`
- **Providers Public Cloud:**
  - `Providers/PublicCloud/OpenAIProviderClient.cs`
  - `Providers/PublicCloud/AzureOpenAIProviderClient.cs`
  - `Providers/PublicCloud/AWSBedrockProviderClient.cs`
  - `Providers/PublicCloud/AnthropicProviderClient.cs`
  - `Providers/PublicCloud/MistralProviderClient.cs`
- **Factory:**
  - `Factory/ILLMProviderClientFactory.cs`
  - `Factory/LLMProviderClientFactory.cs`
- **Configuration:**
  - `Configuration/ProviderSettings.cs`
- **Extensions:**
  - `Extensions/LLMProvidersServiceCollectionExtensions.cs`

#### Phase 3 : Application Layer (Orchestration)
- `src/Application/LLMProxy.Application/LLMProviders/Services/IProviderSelector.cs`
- `src/Application/LLMProxy.Application/LLMProviders/Services/ProviderSelector.cs`
- `src/Application/LLMProxy.Application/LLMProviders/Services/SelectionCriteria.cs`
- `src/Application/LLMProxy.Application/LLMProviders/Services/FailoverManager.cs` (IFailoverManager + FailoverResult + FailoverOptions)
- `src/Application/LLMProxy.Application/LLMProviders/Services/ProviderOrchestrator.cs` (IProviderOrchestrator + OrchestratorResult + ExecutionContext)

#### Phase 4 : DI Extensions
- `src/Application/LLMProxy.Application/Extensions/LLMProvidersApplicationServiceCollectionExtensions.cs`

#### Phase 5 : Tests
- `tests/LLMProxy.Application.Tests/LLMProviders/Services/FailoverManagerTests.cs` (11 tests)
- `tests/LLMProxy.Application.Tests/LLMProviders/Services/ProviderSelectorTests.cs` (10 tests)

### Résultats Validation

- **Build:** ✅ 0 erreurs, 0 warnings
- **Tests:** ✅ 21/21 tests passent (nouveaux tests LLM)
- **Tests globaux:** 96/96 tests Application passent

### Architecture Implémentée

```
┌─────────────────────────────────────────────────────────┐
│                    Application Layer                     │
│  ┌─────────────────┐  ┌─────────────────┐              │
│  │ProviderOrchestrator│  │ProviderSelector│              │
│  └────────┬────────┘  └────────┬────────┘              │
│           │                     │                        │
│  ┌────────▼────────┐           │                        │
│  │ FailoverManager │◄──────────┘                        │
│  └────────┬────────┘                                    │
└───────────┼─────────────────────────────────────────────┘
            │
┌───────────▼─────────────────────────────────────────────┐
│               Infrastructure Layer                       │
│  ┌────────────────────────────────────────────────┐    │
│  │           LLMProviderClientFactory              │    │
│  └───────────────────┬────────────────────────────┘    │
│                      │                                   │
│  ┌───────────────────▼────────────────────────────┐    │
│  │              9 Provider Clients                  │    │
│  │  ┌─────────┐ ┌─────────┐ ┌─────────┐          │    │
│  │  │ Ollama  │ │  vLLM   │ │  OVH    │          │    │
│  │  └─────────┘ └─────────┘ └─────────┘          │    │
│  │  ┌─────────┐ ┌─────────┐ ┌─────────┐          │    │
│  │  │Scaleway │ │ OpenAI  │ │Azure OAI│          │    │
│  │  └─────────┘ └─────────┘ └─────────┘          │    │
│  │  ┌─────────┐ ┌─────────┐ ┌─────────┐          │    │
│  │  │ Bedrock │ │Anthropic│ │ Mistral │          │    │
│  │  └─────────┘ └─────────┘ └─────────┘          │    │
│  └────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────┘
            │
┌───────────▼─────────────────────────────────────────────┐
│                    Domain Layer                          │
│  ┌───────────────────────────────────────────────────┐  │
│  │ ILLMProviderClient │ LLMRequest │ LLMResponse     │  │
│  │ ProviderType      │ TokenUsage │ ModelIdentifier  │  │
│  └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

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

## TRACKING
Début: 2025-01-07T18:50:00Z
Fin: 2025-01-08T10:15:00Z
Durée: ~15h30

**Statut: ✅ COMPLÉTÉ**
