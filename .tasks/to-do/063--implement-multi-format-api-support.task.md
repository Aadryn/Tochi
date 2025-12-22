# Tâche 063 - Implémenter Support Multi-Format API (OpenAI + Ollama)

## PRIORITÉ
🟡 **P3 - MOYENNE** (Priorité 8/8 de la refonte)

## OBJECTIF

Implémenter un système de transformation de requêtes permettant aux clients d'utiliser soit le format API OpenAI, soit le format API Ollama, avec conversion transparente vers le format interne et routage vers n'importe quel provider backend.

## CONTEXTE

### Formats API Supportés en Entrée
1. **Format OpenAI** : Standard de facto, utilisé par la plupart des clients
2. **Format Ollama** : Populaire pour les déploiements locaux

### Principe
```
Client (OpenAI format) → Gateway → Normalisation → Provider (any format)
Client (Ollama format) → Gateway → Normalisation → Provider (any format)
```

### Endpoints Exposés
```
# OpenAI-compatible endpoints
POST /v1/chat/completions
POST /v1/completions
POST /v1/embeddings
GET  /v1/models

# Ollama-compatible endpoints
POST /api/chat
POST /api/generate
POST /api/embeddings
GET  /api/tags
```

## IMPLÉMENTATION

### Phase 1 : Domain Layer (Modèles Canoniques)
```
src/Core/LLMProxy.Domain/
├── Models/
│   ├── Canonical/
│   │   ├── CanonicalChatRequest.cs
│   │   ├── CanonicalChatResponse.cs
│   │   ├── CanonicalEmbeddingRequest.cs
│   │   └── CanonicalEmbeddingResponse.cs
│   └── Formats/
│       ├── ApiFormat.cs          # Enum: OpenAI, Ollama
│       └── ContentType.cs        # Text, Image, Audio
```

### Phase 2 : Application Layer (Transformers)
```
src/Application/LLMProxy.Application/
├── Features/
│   └── ApiTransformation/
│       ├── Interfaces/
│       │   ├── IRequestTransformer.cs
│       │   └── IResponseTransformer.cs
│       ├── Transformers/
│       │   ├── OpenAI/
│       │   │   ├── OpenAIRequestTransformer.cs
│       │   │   └── OpenAIResponseTransformer.cs
│       │   └── Ollama/
│       │       ├── OllamaRequestTransformer.cs
│       │       └── OllamaResponseTransformer.cs
│       ├── Factory/
│       │   └── TransformerFactory.cs
│       └── Services/
│           └── ApiFormatDetector.cs   # Détection auto du format
```

### Phase 3 : Format OpenAI (Référence)
```csharp
// OpenAI Chat Request
{
  "model": "gpt-4o",
  "messages": [
    {"role": "system", "content": "You are a helpful assistant."},
    {"role": "user", "content": "Hello!"}
  ],
  "temperature": 0.7,
  "max_tokens": 1000,
  "stream": false
}

// OpenAI Chat Response
{
  "id": "chatcmpl-xxx",
  "object": "chat.completion",
  "created": 1234567890,
  "model": "gpt-4o",
  "choices": [{
    "index": 0,
    "message": {"role": "assistant", "content": "Hello! How can I help?"},
    "finish_reason": "stop"
  }],
  "usage": {
    "prompt_tokens": 20,
    "completion_tokens": 10,
    "total_tokens": 30
  }
}
```

### Phase 4 : Format Ollama (Transformation)
```csharp
// Ollama Chat Request (entrée)
{
  "model": "llama3.1",
  "messages": [
    {"role": "user", "content": "Hello!"}
  ],
  "stream": false,
  "options": {
    "temperature": 0.7,
    "num_predict": 1000
  }
}

// Transformation vers Canonical
public class OllamaRequestTransformer : IRequestTransformer
{
    public CanonicalChatRequest Transform(JsonDocument ollamaRequest)
    {
        return new CanonicalChatRequest
        {
            Model = ollamaRequest.RootElement.GetProperty("model").GetString(),
            Messages = TransformMessages(ollamaRequest),
            Temperature = GetNestedOption<float>("temperature", 0.7f),
            MaxTokens = GetNestedOption<int>("num_predict", 1000),
            Stream = ollamaRequest.RootElement.GetProperty("stream").GetBoolean()
        };
    }
}
```

### Phase 5 : Gateway Controllers
```
src/Presentation/LLMProxy.Gateway/
├── Controllers/
│   ├── V1/
│   │   ├── ChatCompletionsController.cs     # OpenAI format
│   │   ├── CompletionsController.cs
│   │   ├── EmbeddingsController.cs
│   │   └── ModelsController.cs
│   └── Ollama/
│       ├── OllamaChatController.cs          # Ollama format
│       ├── OllamaGenerateController.cs
│       ├── OllamaEmbeddingsController.cs
│       └── OllamaTagsController.cs
├── Middleware/
│   └── ApiFormatDetectionMiddleware.cs
```

### Phase 6 : Streaming Support
```csharp
// SSE streaming pour OpenAI format
app.MapPost("/v1/chat/completions", async (HttpContext ctx, ...) =>
{
    if (request.Stream)
    {
        ctx.Response.ContentType = "text/event-stream";
        await foreach (var chunk in provider.ChatCompletionStreamAsync(request))
        {
            var openAiChunk = transformer.TransformToOpenAI(chunk);
            await ctx.Response.WriteAsync($"data: {JsonSerializer.Serialize(openAiChunk)}\n\n");
        }
        await ctx.Response.WriteAsync("data: [DONE]\n\n");
    }
});
```

## CRITÈRES DE SUCCÈS

- [ ] CanonicalChatRequest/Response créés
- [ ] OpenAI transformer implémenté (request + response)
- [ ] Ollama transformer implémenté (request + response)
- [ ] 4 endpoints OpenAI fonctionnels
- [ ] 4 endpoints Ollama fonctionnels
- [ ] Streaming SSE pour les deux formats
- [ ] Détection automatique du format
- [ ] Tests unitaires transformers
- [ ] Tests d'intégration endpoints
- [ ] Documentation OpenAPI/Swagger
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Documentation XML complète (français)

## DÉPENDANCES

- Tâche 062 (Multi-Provider LLM) pour le backend

## ESTIMATION

**Effort** : 10h
**Complexité** : Moyenne

## RÉFÉRENCES

- [OpenAI API Reference](https://platform.openai.com/docs/api-reference/chat)
- [Ollama API Reference](https://github.com/ollama/ollama/blob/main/docs/api.md)
- ADR-037 (API Versioning Strategy)
