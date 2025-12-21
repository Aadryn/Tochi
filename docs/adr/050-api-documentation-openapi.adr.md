# 50. API Documentation avec OpenAPI/Swagger

Date: 2025-12-21

## Statut

Accepté

## Contexte

Un proxy LLM expose des APIs REST consommées par des clients variés. Sans documentation :
- Développeurs perdus pour intégrer
- Incompréhension des formats de requête/réponse
- Pas de génération de clients automatique
- Support surchargé de questions basiques

```csharp
// ❌ SANS DOCUMENTATION : API non documentée
[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Chat(ChatRequest request)
    {
        // Quels champs sont obligatoires ?
        // Quel format pour la réponse ?
        // Quels codes d'erreur possibles ?
        return Ok(await _service.ChatAsync(request));
    }
}
```

## Décision

**Documenter exhaustivement les APIs avec OpenAPI 3.0, annotations, et exemples.**

### 1. Configuration Swagger/OpenAPI

```csharp
/// <summary>
/// Configuration OpenAPI dans Program.cs.
/// </summary>
public static class OpenApiConfiguration
{
    public static IServiceCollection AddOpenApiDocumentation(
        this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "LLM Proxy API",
                Version = "v1",
                Description = """
                    API de proxy pour les modèles de langage (LLM).
                    
                    ## Fonctionnalités
                    - Chat completions (streaming et non-streaming)
                    - Gestion des clés API
                    - Quotas et rate limiting
                    - Statistiques d'usage
                    
                    ## Authentification
                    Toutes les requêtes nécessitent un header `X-API-Key`.
                    """,
                Contact = new OpenApiContact
                {
                    Name = "Équipe LLM Proxy",
                    Email = "llm-proxy@example.com",
                    Url = new Uri("https://docs.example.com/llm-proxy")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });
            
            // Ajouter la documentation XML
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            
            // Schéma d'authentification
            options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Name = "X-API-Key",
                Description = "Clé API fournie par l'administrateur du tenant"
            });
            
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "ApiKey"
                        }
                    },
                    Array.Empty<string>()
                }
            });
            
            // Schémas personnalisés
            options.SchemaFilter<EnumSchemaFilter>();
            options.OperationFilter<ResponseHeadersFilter>();
            options.DocumentFilter<TagDescriptionsFilter>();
        });
        
        return services;
    }
    
    public static IApplicationBuilder UseOpenApiDocumentation(
        this IApplicationBuilder app,
        IWebHostEnvironment env)
    {
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "api-docs/{documentName}/openapi.json";
        });
        
        if (env.IsDevelopment())
        {
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/api-docs/v1/openapi.json", "LLM Proxy API v1");
                options.RoutePrefix = "api-docs";
                options.DocumentTitle = "LLM Proxy API Documentation";
                options.DefaultModelsExpandDepth(2);
                options.EnableDeepLinking();
                options.EnableFilter();
                options.ShowExtensions();
            });
            
            // ReDoc comme alternative
            app.UseReDoc(options =>
            {
                options.SpecUrl = "/api-docs/v1/openapi.json";
                options.RoutePrefix = "docs";
                options.DocumentTitle = "LLM Proxy API Reference";
            });
        }
        
        return app;
    }
}
```

### 2. Controller Documenté

```csharp
/// <summary>
/// API de chat completion compatible OpenAI.
/// </summary>
/// <remarks>
/// Ce controller expose les endpoints de chat completion
/// avec support du streaming via Server-Sent Events.
/// </remarks>
[ApiController]
[Route("v1/chat")]
[Produces("application/json")]
[Consumes("application/json")]
[Tags("Chat")]
public class ChatController : ControllerBase
{
    private readonly ILlmService _llmService;
    private readonly ILogger<ChatController> _logger;
    
    /// <summary>
    /// Crée une completion de chat.
    /// </summary>
    /// <param name="request">Paramètres de la requête de chat.</param>
    /// <param name="ct">Token d'annulation.</param>
    /// <returns>Réponse du modèle de langage.</returns>
    /// <response code="200">Completion générée avec succès.</response>
    /// <response code="400">Requête invalide (paramètres manquants ou incorrects).</response>
    /// <response code="401">Clé API manquante ou invalide.</response>
    /// <response code="429">Quota dépassé ou rate limit atteint.</response>
    /// <response code="502">Erreur du provider LLM.</response>
    /// <response code="503">Service temporairement indisponible.</response>
    [HttpPost("completions")]
    [ProducesResponseType(typeof(ChatCompletionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ChatCompletionResponse>> CreateChatCompletion(
        [FromBody] ChatCompletionRequest request,
        CancellationToken ct)
    {
        if (request.Stream == true)
        {
            return await StreamChatCompletion(request, ct);
        }
        
        var response = await _llmService.ChatCompletionAsync(request, ct);
        return Ok(response);
    }
    
    /// <summary>
    /// Crée une completion de chat en streaming (SSE).
    /// </summary>
    /// <remarks>
    /// Le streaming utilise Server-Sent Events.
    /// Chaque chunk contient un delta de la réponse.
    /// 
    /// Exemple de consommation:
    /// ```javascript
    /// const eventSource = new EventSource('/v1/chat/completions/stream');
    /// eventSource.onmessage = (event) => {
    ///     const data = JSON.parse(event.data);
    ///     console.log(data.choices[0].delta.content);
    /// };
    /// ```
    /// </remarks>
    [HttpPost("completions/stream")]
    [Produces("text/event-stream")]
    [ProducesResponseType(typeof(ChatCompletionChunk), StatusCodes.Status200OK)]
    public async Task StreamChatCompletion(
        [FromBody] ChatCompletionRequest request,
        CancellationToken ct)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";
        
        await foreach (var chunk in _llmService.StreamChatAsync(request, ct))
        {
            var json = JsonSerializer.Serialize(chunk);
            await Response.WriteAsync($"data: {json}\n\n", ct);
            await Response.Body.FlushAsync(ct);
        }
        
        await Response.WriteAsync("data: [DONE]\n\n", ct);
    }
}
```

### 3. DTOs avec Annotations Complètes

```csharp
/// <summary>
/// Requête de chat completion.
/// </summary>
/// <example>
/// {
///   "model": "gpt-4",
///   "messages": [
///     {"role": "system", "content": "You are a helpful assistant."},
///     {"role": "user", "content": "Hello!"}
///   ],
///   "temperature": 0.7,
///   "max_tokens": 1000
/// }
/// </example>
public sealed record ChatCompletionRequest
{
    /// <summary>
    /// Identifiant du modèle à utiliser.
    /// </summary>
    /// <example>gpt-4</example>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    [JsonPropertyName("model")]
    public required string Model { get; init; }
    
    /// <summary>
    /// Liste des messages de la conversation.
    /// </summary>
    /// <remarks>
    /// Doit contenir au moins un message.
    /// L'ordre chronologique doit être respecté.
    /// </remarks>
    [Required]
    [MinLength(1)]
    [JsonPropertyName("messages")]
    public required IReadOnlyList<ChatMessage> Messages { get; init; }
    
    /// <summary>
    /// Température de sampling (0.0 à 2.0).
    /// </summary>
    /// <remarks>
    /// - 0.0 : Réponses déterministes
    /// - 0.7 : Équilibre créativité/cohérence (défaut)
    /// - 2.0 : Réponses très créatives
    /// </remarks>
    /// <example>0.7</example>
    [Range(0.0, 2.0)]
    [JsonPropertyName("temperature")]
    public double? Temperature { get; init; } = 0.7;
    
    /// <summary>
    /// Nombre maximum de tokens à générer.
    /// </summary>
    /// <remarks>
    /// Si non spécifié, utilise le maximum du modèle.
    /// Le coût est calculé sur les tokens générés.
    /// </remarks>
    /// <example>1000</example>
    [Range(1, 128000)]
    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; init; }
    
    /// <summary>
    /// Active le streaming de la réponse.
    /// </summary>
    /// <example>false</example>
    [JsonPropertyName("stream")]
    public bool? Stream { get; init; } = false;
    
    /// <summary>
    /// Nucleus sampling (top_p).
    /// </summary>
    /// <remarks>
    /// Alternative à temperature. Ne pas utiliser les deux ensemble.
    /// </remarks>
    /// <example>1.0</example>
    [Range(0.0, 1.0)]
    [JsonPropertyName("top_p")]
    public double? TopP { get; init; } = 1.0;
    
    /// <summary>
    /// Pénalité de fréquence (-2.0 à 2.0).
    /// </summary>
    /// <remarks>
    /// Valeurs positives réduisent la répétition.
    /// </remarks>
    /// <example>0.0</example>
    [Range(-2.0, 2.0)]
    [JsonPropertyName("frequency_penalty")]
    public double? FrequencyPenalty { get; init; } = 0.0;
    
    /// <summary>
    /// Métadonnées utilisateur pour le tracking.
    /// </summary>
    /// <example>user-123</example>
    [StringLength(256)]
    [JsonPropertyName("user")]
    public string? User { get; init; }
}

/// <summary>
/// Message dans une conversation.
/// </summary>
public sealed record ChatMessage
{
    /// <summary>
    /// Rôle de l'auteur du message.
    /// </summary>
    /// <example>user</example>
    [Required]
    [JsonPropertyName("role")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required ChatRole Role { get; init; }
    
    /// <summary>
    /// Contenu textuel du message.
    /// </summary>
    /// <example>Hello, how can you help me?</example>
    [Required]
    [JsonPropertyName("content")]
    public required string Content { get; init; }
    
    /// <summary>
    /// Nom de l'auteur (optionnel).
    /// </summary>
    /// <example>John</example>
    [StringLength(64)]
    [JsonPropertyName("name")]
    public string? Name { get; init; }
}

/// <summary>
/// Rôle d'un participant à la conversation.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChatRole
{
    /// <summary>Instructions système pour le modèle.</summary>
    [Description("Instructions système pour le comportement du modèle")]
    System,
    
    /// <summary>Message de l'utilisateur.</summary>
    [Description("Message envoyé par l'utilisateur")]
    User,
    
    /// <summary>Réponse de l'assistant.</summary>
    [Description("Réponse générée par le modèle")]
    Assistant,
    
    /// <summary>Résultat d'un appel de fonction.</summary>
    [Description("Résultat retourné par un appel de fonction")]
    Function
}

/// <summary>
/// Réponse de chat completion.
/// </summary>
public sealed record ChatCompletionResponse
{
    /// <summary>
    /// Identifiant unique de la completion.
    /// </summary>
    /// <example>chatcmpl-abc123</example>
    [JsonPropertyName("id")]
    public required string Id { get; init; }
    
    /// <summary>
    /// Type d'objet retourné.
    /// </summary>
    /// <example>chat.completion</example>
    [JsonPropertyName("object")]
    public string Object { get; init; } = "chat.completion";
    
    /// <summary>
    /// Timestamp de création (Unix).
    /// </summary>
    /// <example>1703152800</example>
    [JsonPropertyName("created")]
    public long Created { get; init; }
    
    /// <summary>
    /// Modèle utilisé pour la génération.
    /// </summary>
    /// <example>gpt-4-0613</example>
    [JsonPropertyName("model")]
    public required string Model { get; init; }
    
    /// <summary>
    /// Liste des choix générés.
    /// </summary>
    [JsonPropertyName("choices")]
    public required IReadOnlyList<ChatChoice> Choices { get; init; }
    
    /// <summary>
    /// Statistiques d'usage des tokens.
    /// </summary>
    [JsonPropertyName("usage")]
    public required UsageInfo Usage { get; init; }
}

/// <summary>
/// Statistiques d'utilisation des tokens.
/// </summary>
public sealed record UsageInfo
{
    /// <summary>
    /// Nombre de tokens dans le prompt.
    /// </summary>
    /// <example>50</example>
    [JsonPropertyName("prompt_tokens")]
    public required int PromptTokens { get; init; }
    
    /// <summary>
    /// Nombre de tokens générés.
    /// </summary>
    /// <example>150</example>
    [JsonPropertyName("completion_tokens")]
    public required int CompletionTokens { get; init; }
    
    /// <summary>
    /// Total des tokens utilisés.
    /// </summary>
    /// <example>200</example>
    [JsonPropertyName("total_tokens")]
    public int TotalTokens => PromptTokens + CompletionTokens;
}
```

### 4. Réponses d'Erreur Standardisées

```csharp
/// <summary>
/// Réponse d'erreur standardisée.
/// </summary>
/// <example>
/// {
///   "error": {
///     "code": "INVALID_REQUEST",
///     "message": "Le champ 'model' est requis",
///     "details": {
///       "field": "model",
///       "reason": "required"
///     }
///   }
/// }
/// </example>
public sealed record ErrorResponse
{
    /// <summary>
    /// Détails de l'erreur.
    /// </summary>
    [JsonPropertyName("error")]
    public required ErrorDetail Error { get; init; }
}

/// <summary>
/// Détails d'une erreur.
/// </summary>
public sealed record ErrorDetail
{
    /// <summary>
    /// Code d'erreur machine-readable.
    /// </summary>
    /// <example>INVALID_REQUEST</example>
    [JsonPropertyName("code")]
    public required string Code { get; init; }
    
    /// <summary>
    /// Message d'erreur human-readable.
    /// </summary>
    /// <example>Le champ 'model' est requis</example>
    [JsonPropertyName("message")]
    public required string Message { get; init; }
    
    /// <summary>
    /// Type d'erreur.
    /// </summary>
    /// <example>invalid_request_error</example>
    [JsonPropertyName("type")]
    public string? Type { get; init; }
    
    /// <summary>
    /// Paramètre concerné par l'erreur.
    /// </summary>
    /// <example>model</example>
    [JsonPropertyName("param")]
    public string? Param { get; init; }
    
    /// <summary>
    /// Détails additionnels.
    /// </summary>
    [JsonPropertyName("details")]
    public IDictionary<string, object>? Details { get; init; }
}
```

### 5. Filtres et Extensions OpenAPI

```csharp
/// <summary>
/// Filtre pour ajouter les descriptions des tags.
/// </summary>
public sealed class TagDescriptionsFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Tags = new List<OpenApiTag>
        {
            new()
            {
                Name = "Chat",
                Description = "Endpoints de chat completion pour interagir avec les LLMs"
            },
            new()
            {
                Name = "API Keys",
                Description = "Gestion des clés API pour l'authentification"
            },
            new()
            {
                Name = "Tenants",
                Description = "Gestion des tenants (organisations)"
            },
            new()
            {
                Name = "Usage",
                Description = "Statistiques d'utilisation et quotas"
            },
            new()
            {
                Name = "Health",
                Description = "Endpoints de santé et monitoring"
            }
        };
    }
}

/// <summary>
/// Filtre pour documenter les headers de réponse.
/// </summary>
public sealed class ResponseHeadersFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Ajouter les headers de rate limiting
        operation.Responses["200"]?.Headers.Add(
            "X-RateLimit-Limit",
            new OpenApiHeader
            {
                Description = "Nombre maximum de requêtes autorisées par minute",
                Schema = new OpenApiSchema { Type = "integer" }
            });
        
        operation.Responses["200"]?.Headers.Add(
            "X-RateLimit-Remaining",
            new OpenApiHeader
            {
                Description = "Nombre de requêtes restantes",
                Schema = new OpenApiSchema { Type = "integer" }
            });
        
        operation.Responses["200"]?.Headers.Add(
            "X-RateLimit-Reset",
            new OpenApiHeader
            {
                Description = "Timestamp Unix de réinitialisation du compteur",
                Schema = new OpenApiSchema { Type = "integer" }
            });
        
        // Request ID pour le tracing
        foreach (var response in operation.Responses.Values)
        {
            response.Headers.Add(
                "X-Request-Id",
                new OpenApiHeader
                {
                    Description = "Identifiant unique de la requête pour le support",
                    Schema = new OpenApiSchema { Type = "string", Format = "uuid" }
                });
        }
    }
}

/// <summary>
/// Filtre pour les enums avec descriptions.
/// </summary>
public sealed class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum) return;
        
        var enumType = context.Type;
        var descriptions = new List<string>();
        
        foreach (var enumValue in Enum.GetValues(enumType))
        {
            var member = enumType.GetMember(enumValue.ToString()!).FirstOrDefault();
            var description = member?
                .GetCustomAttribute<DescriptionAttribute>()?
                .Description ?? enumValue.ToString();
            
            descriptions.Add($"- `{enumValue}`: {description}");
        }
        
        schema.Description = $"{schema.Description}\n\n{string.Join("\n", descriptions)}";
    }
}
```

### 6. Exemples de Requêtes/Réponses

```csharp
/// <summary>
/// Provider d'exemples pour Swagger.
/// </summary>
public sealed class ChatCompletionExamplesProvider : IExamplesProvider<ChatCompletionRequest>
{
    public IEnumerable<SwaggerExample<ChatCompletionRequest>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Simple",
            "Requête simple avec un message",
            new ChatCompletionRequest
            {
                Model = "gpt-4",
                Messages = new[]
                {
                    new ChatMessage { Role = ChatRole.User, Content = "Hello!" }
                }
            });
        
        yield return SwaggerExample.Create(
            "WithSystem",
            "Requête avec message système",
            new ChatCompletionRequest
            {
                Model = "gpt-4",
                Messages = new[]
                {
                    new ChatMessage 
                    { 
                        Role = ChatRole.System, 
                        Content = "You are a helpful coding assistant." 
                    },
                    new ChatMessage 
                    { 
                        Role = ChatRole.User, 
                        Content = "Write a hello world in Python" 
                    }
                },
                Temperature = 0.2,
                MaxTokens = 500
            });
        
        yield return SwaggerExample.Create(
            "Streaming",
            "Requête avec streaming activé",
            new ChatCompletionRequest
            {
                Model = "gpt-4",
                Messages = new[]
                {
                    new ChatMessage { Role = ChatRole.User, Content = "Tell me a story" }
                },
                Stream = true,
                MaxTokens = 2000
            });
    }
}
```

### 7. Génération de Clients

```csharp
/// <summary>
/// Configuration pour la génération de clients.
/// </summary>
// openapi-generator-config.yaml
/*
generatorName: csharp-netcore
outputDir: ./generated/csharp-client
inputSpec: ./api-docs/v1/openapi.json
additionalProperties:
  packageName: LlmProxy.Client
  targetFramework: net8.0
  nullableReferenceTypes: true
  library: httpclient
*/

// Commande de génération:
// openapi-generator generate -c openapi-generator-config.yaml

/// <summary>
/// Exemple d'utilisation du client généré.
/// </summary>
public static class ClientUsageExample
{
    public static async Task DemoAsync()
    {
        var config = new Configuration
        {
            BasePath = "https://api.llmproxy.example.com",
            DefaultHeaders = new Dictionary<string, string>
            {
                ["X-API-Key"] = "sk-your-api-key"
            }
        };
        
        var chatApi = new ChatApi(config);
        
        var request = new ChatCompletionRequest
        {
            Model = "gpt-4",
            Messages = new List<ChatMessage>
            {
                new() { Role = ChatRole.User, Content = "Hello!" }
            }
        };
        
        var response = await chatApi.CreateChatCompletionAsync(request);
        Console.WriteLine(response.Choices[0].Message.Content);
    }
}
```

## Conséquences

### Positives

- **Self-service** : Développeurs autonomes
- **Précision** : Contrats API explicites
- **Génération** : Clients automatiques
- **Testing** : UI interactive pour tests

### Négatives

- **Maintenance** : Docs à synchroniser avec code
  - *Mitigation* : Génération depuis annotations
- **Verbosité** : Beaucoup d'annotations
  - *Mitigation* : Conventions et filtres

### Neutres

- Standard industrie (OpenAPI 3.0)
- Compatible tous langages clients

## Alternatives considérées

### Option A : Documentation manuelle (Markdown)

- **Description** : Docs en fichiers .md
- **Avantages** : Flexible
- **Inconvénients** : Désynchronisation
- **Raison du rejet** : Non maintenable

### Option B : GraphQL

- **Description** : Schema auto-documenté
- **Avantages** : Typage fort, introspection
- **Inconvénients** : Changement d'architecture
- **Raison du rejet** : APIs REST existantes

## Références

- [OpenAPI Specification](https://swagger.io/specification/)
- [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- [NSwag](https://github.com/RicoSuter/NSwag)
