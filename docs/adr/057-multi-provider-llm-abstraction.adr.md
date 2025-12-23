# ADR-057. Abstraction Multi-Providers LLM

Date: 2025-12-22
Statut: **Proposé**

## Contexte

LLMProxy doit supporter 9 providers LLM hétérogènes :
- **Locaux** : Ollama, vLLM
- **Clouds Privés** : OVH Cloud AI, Scaleway Generative
- **Clouds Publics** : OpenAI, Azure OpenAI, AWS Bedrock, Anthropic, Mistral AI

Chaque provider a son propre format d'API, authentification, et capacités.

## Décision

Implémenter une **couche d'abstraction** basée sur :
1. **Strategy Pattern** : Un `ILLMProvider` par provider
2. **Factory Pattern** : Création dynamique des clients
3. **Adapter Pattern** : Normalisation request/response

### Interface Commune

```csharp
public interface ILLMProvider
{
    ProviderType Type { get; }
    string Name { get; }
    
    // Capacités et santé
    Task<ProviderCapabilities> GetCapabilitiesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<LLMModel>> ListModelsAsync(CancellationToken ct = default);
    Task<bool> IsHealthyAsync(CancellationToken ct = default);
    
    // Opérations LLM
    Task<LLMResponse> ChatCompletionAsync(LLMRequest request, CancellationToken ct = default);
    IAsyncEnumerable<LLMResponse> ChatCompletionStreamAsync(LLMRequest request, CancellationToken ct = default);
    Task<EmbeddingResponse> EmbeddingsAsync(EmbeddingRequest request, CancellationToken ct = default);
}
```

### Modèles Canoniques

```csharp
// Request normalisée
public record LLMRequest
{
    public required string Model { get; init; }
    public required IReadOnlyList<LLMMessage> Messages { get; init; }
    public float Temperature { get; init; } = 0.7f;
    public int MaxTokens { get; init; } = 1000;
    public bool Stream { get; init; } = false;
}

// Response normalisée
public record LLMResponse
{
    public required string Id { get; init; }
    public required string Model { get; init; }
    public required IReadOnlyList<Choice> Choices { get; init; }
    public required TokenUsage Usage { get; init; }
    public required TimeSpan Latency { get; init; }
}
```

### Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                       Gateway                                │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────┐│
│  │              ProviderOrchestrator                        ││
│  │  - Routing intelligent (priority, load, latency)        ││
│  │  - Failover automatique                                  ││
│  │  - Circuit breaker per provider                          ││
│  └─────────────────────────────────────────────────────────┘│
│                           │                                  │
│                           ▼                                  │
│  ┌─────────────────────────────────────────────────────────┐│
│  │              ILLMProviderFactory                         ││
│  │  - Création providers selon configuration                ││
│  │  - Cache des instances (singleton per config)            ││
│  └─────────────────────────────────────────────────────────┘│
│              │         │         │         │                 │
│              ▼         ▼         ▼         ▼                 │
│  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐            │
│  │ OpenAI  │ │ Ollama  │ │Anthropic│ │ Bedrock │  ...       │
│  │Provider │ │Provider │ │Provider │ │Provider │            │
│  └─────────┘ └─────────┘ └─────────┘ └─────────┘            │
└─────────────────────────────────────────────────────────────┘
```

### Mapping Formats

| Provider | Format API | Conversion |
|----------|------------|------------|
| OpenAI | OpenAI native | Direct |
| Azure OpenAI | OpenAI-compatible | Endpoint + API version |
| Ollama | Ollama native | Adapter |
| vLLM | OpenAI-compatible | Direct |
| Anthropic | Anthropic native | Adapter |
| AWS Bedrock | AWS native | SigV4 + Adapter |
| Mistral AI | OpenAI-compatible | Direct |
| OVH Cloud | OpenAI-compatible | Token auth |
| Scaleway | OpenAI-compatible | Secret key auth |

## Conséquences

### Positives
- Interface uniforme pour tous les providers
- Ajout de nouveaux providers sans modifier le code existant
- Failover et load balancing transparents
- Testabilité (mock ILLMProvider)
- Métriques uniformes par provider

### Négatives
- Abstraction = perte de features spécifiques (function calling avancé, etc.)
- Maintenance de 9 implémentations
- Gestion des versions API de chaque provider
- Latence additionnelle (transformation)

## Alternatives Considérées

### Alternative 1 : LiteLLM (Python)
- Avantages : 100+ providers, bien maintenu
- Inconvénients : Python, microservice supplémentaire
- Raison du rejet : Complexité déploiement, latence inter-process

### Alternative 2 : Semantic Kernel (Microsoft)
- Avantages : .NET natif, AI-focused
- Inconvénients : Opinionated, plus que juste LLM proxy
- Raison du rejet : Surcoût d'abstraction pour notre use case simple

### Alternative 3 : YARP direct (sans abstraction)
- Avantages : Simple, juste proxy HTTP
- Inconvénients : Pas de normalisation, pas de métriques LLM
- Raison du rejet : Perte de valeur ajoutée (tokens, coûts, etc.)

## Références

- [OpenAI API Reference](https://platform.openai.com/docs/api-reference)
- [Anthropic API Reference](https://docs.anthropic.com/claude/reference)
- [AWS Bedrock Documentation](https://docs.aws.amazon.com/bedrock/)
- ADR-034 (Third-Party Library Encapsulation)
