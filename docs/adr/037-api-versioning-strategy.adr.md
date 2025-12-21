# 37. API Versioning Strategy

Date: 2025-12-21

## Statut

Accepté

## Contexte

Les APIs évoluent avec le temps et les changements peuvent casser les clients existants :
- **Breaking changes** : Suppression de champs, changement de types
- **Nouvelles fonctionnalités** : Nouveaux endpoints, nouveaux champs
- **Corrections** : Changements de comportement
- **Dépréciation** : Suppression planifiée de fonctionnalités

Sans stratégie de versioning :
- Les clients cassent à chaque mise à jour
- Impossible de faire évoluer l'API
- Pas de période de transition
- Support difficile (quelle version utilise le client ?)

```csharp
// ❌ SANS VERSIONING : Breaking change direct
// Avant
public record TenantResponse(Guid Id, string Name, string Email);

// Après - BREAKING : Email supprimé, Contacts ajouté
public record TenantResponse(Guid Id, string Name, List<Contact> Contacts);
// Tous les clients qui utilisaient Email sont cassés !
```

## Décision

**Utiliser le versioning d'API par URL avec support de versions multiples et politique de dépréciation claire.**

### 1. Stratégie de versioning par URL

```csharp
/// <summary>
/// Versioning par URL : /api/v1/tenants, /api/v2/tenants
/// 
/// AVANTAGES :
/// - Explicite et visible
/// - Facile à router
/// - Cache-friendly (URLs différentes)
/// - Simple à comprendre pour les clients
/// 
/// ALTERNATIVES REJETÉES :
/// - Header (Accept-Version) : Moins visible, cache complexe
/// - Query string (?version=1) : Pollue les URLs
/// - Media type (Accept: application/vnd.api.v1+json) : Complexe
/// </summary>
```

### 2. Configuration ASP.NET Core

```csharp
/// <summary>
/// Configuration du versioning API.
/// </summary>
public static class ApiVersioningExtensions
{
    public static IServiceCollection AddLlmProxyApiVersioning(
        this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            // Version par défaut si non spécifiée
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            
            // Reporter la version dans les headers de réponse
            options.ReportApiVersions = true;
            
            // Lire la version depuis l'URL
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            // Format: 'v'major[.minor]
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
        
        return services;
    }
}

// Program.cs
builder.Services.AddLlmProxyApiVersioning();

// Swagger avec versions multiples
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LLM Proxy API",
        Version = "v1",
        Description = "Version stable de l'API LLM Proxy"
    });
    
    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "LLM Proxy API",
        Version = "v2",
        Description = "Nouvelle version avec support streaming amélioré"
    });
});

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "LLM Proxy API v1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "LLM Proxy API v2");
});
```

### 3. Controllers versionnés

```csharp
/// <summary>
/// Controller v1 - Version stable.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/tenants")]
public sealed class TenantsV1Controller : ControllerBase
{
    private readonly ISender _mediator;
    
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TenantResponseV1), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTenant(
        Guid id, 
        CancellationToken ct)
    {
        var query = new GetTenantQuery(id);
        var result = await _mediator.Send(query, ct);
        
        return result.Match<IActionResult>(
            tenant => Ok(tenant.ToV1Response()),
            () => NotFound());
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(TenantResponseV1), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateTenant(
        [FromBody] CreateTenantRequestV1 request,
        CancellationToken ct)
    {
        var command = request.ToCommand();
        var result = await _mediator.Send(command, ct);
        
        return result.Match<IActionResult>(
            tenant => CreatedAtAction(
                nameof(GetTenant),
                new { id = tenant.Id },
                tenant.ToV1Response()),
            error => BadRequest(error));
    }
}

/// <summary>
/// Controller v2 - Nouvelle version avec breaking changes.
/// </summary>
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/tenants")]
public sealed class TenantsV2Controller : ControllerBase
{
    private readonly ISender _mediator;
    
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TenantResponseV2), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTenant(
        Guid id,
        CancellationToken ct)
    {
        var query = new GetTenantQuery(id);
        var result = await _mediator.Send(query, ct);
        
        // V2 : Réponse enrichie avec contacts
        return result.Match<IActionResult>(
            tenant => Ok(tenant.ToV2Response()),
            () => NotFound());
    }
    
    // V2 : Nouvel endpoint pour le streaming
    [HttpPost("completions/stream")]
    [ProducesResponseType(typeof(StreamingCompletionResponse), StatusCodes.Status200OK)]
    public async Task StreamCompletion(
        [FromBody] CompletionRequestV2 request,
        CancellationToken ct)
    {
        Response.ContentType = "text/event-stream";
        
        await foreach (var chunk in _completionService.StreamAsync(request, ct))
        {
            await Response.WriteAsync($"data: {JsonSerializer.Serialize(chunk)}\n\n", ct);
            await Response.Body.FlushAsync(ct);
        }
    }
}
```

### 4. DTOs versionnés

```csharp
// ═══════════════════════════════════════════════════════════════
// DTOs V1 - Structure originale
// ═══════════════════════════════════════════════════════════════

namespace LLMProxy.Api.Contracts.V1;

/// <summary>
/// Réponse tenant v1.
/// </summary>
public sealed record TenantResponseV1
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public required string Status { get; init; }
    
    // V1 : Email unique (sera déprécié en V2)
    public required string Email { get; init; }
    
    public required DateTime CreatedAt { get; init; }
}

/// <summary>
/// Requête de création v1.
/// </summary>
public sealed record CreateTenantRequestV1
{
    [Required]
    [StringLength(255)]
    public required string Name { get; init; }
    
    [Required]
    [EmailAddress]
    public required string Email { get; init; }
}

// ═══════════════════════════════════════════════════════════════
// DTOs V2 - Structure enrichie (breaking changes)
// ═══════════════════════════════════════════════════════════════

namespace LLMProxy.Api.Contracts.V2;

/// <summary>
/// Réponse tenant v2 - Email remplacé par Contacts.
/// </summary>
public sealed record TenantResponseV2
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public required string Status { get; init; }
    
    // V2 : Liste de contacts au lieu d'un email unique
    public required IReadOnlyList<ContactResponse> Contacts { get; init; }
    
    // V2 : Nouvelles propriétés
    public required QuotaStatusResponse QuotaStatus { get; init; }
    public required TenantSettingsResponse Settings { get; init; }
    
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset? UpdatedAt { get; init; }
}

public sealed record ContactResponse
{
    public required string Type { get; init; } // billing, technical, admin
    public required string Email { get; init; }
    public string? Name { get; init; }
    public string? Phone { get; init; }
}

/// <summary>
/// Requête de création v2 - Avec contacts multiples.
/// </summary>
public sealed record CreateTenantRequestV2
{
    [Required]
    [StringLength(255)]
    public required string Name { get; init; }
    
    [Required]
    [MinLength(1)]
    public required IReadOnlyList<CreateContactRequest> Contacts { get; init; }
    
    public TenantSettingsRequest? Settings { get; init; }
}
```

### 5. Mappers entre versions

```csharp
/// <summary>
/// Extensions de mapping entre versions.
/// </summary>
public static class TenantMappingExtensions
{
    /// <summary>
    /// Mappe vers la réponse V1.
    /// </summary>
    public static TenantResponseV1 ToV1Response(this Tenant tenant)
    {
        return new TenantResponseV1
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Slug = tenant.Slug,
            Status = tenant.Status.ToString(),
            // V1 : Prendre le premier email contact ou vide
            Email = tenant.Contacts
                .FirstOrDefault(c => c.Type == ContactType.Admin)?.Email 
                ?? string.Empty,
            CreatedAt = tenant.CreatedAt.DateTime
        };
    }
    
    /// <summary>
    /// Mappe vers la réponse V2.
    /// </summary>
    public static TenantResponseV2 ToV2Response(this Tenant tenant)
    {
        return new TenantResponseV2
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Slug = tenant.Slug,
            Status = tenant.Status.ToString(),
            Contacts = tenant.Contacts.Select(c => new ContactResponse
            {
                Type = c.Type.ToString().ToLower(),
                Email = c.Email,
                Name = c.Name,
                Phone = c.Phone
            }).ToList(),
            QuotaStatus = tenant.QuotaStatus.ToResponse(),
            Settings = tenant.Settings.ToResponse(),
            CreatedAt = tenant.CreatedAt,
            UpdatedAt = tenant.UpdatedAt
        };
    }
}
```

### 6. Politique de dépréciation

```csharp
/// <summary>
/// Attribute pour marquer une version comme dépréciée.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ApiDeprecatedAttribute : Attribute
{
    public string Message { get; }
    public string SunsetDate { get; }
    public string? SuccessorVersion { get; }
    
    public ApiDeprecatedAttribute(
        string message,
        string sunsetDate,
        string? successorVersion = null)
    {
        Message = message;
        SunsetDate = sunsetDate;
        SuccessorVersion = successorVersion;
    }
}

/// <summary>
/// Middleware qui ajoute les headers de dépréciation.
/// </summary>
public sealed class DeprecationHeaderMiddleware
{
    private readonly RequestDelegate _next;
    
    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);
        
        var endpoint = context.GetEndpoint();
        var deprecatedAttr = endpoint?.Metadata.GetMetadata<ApiDeprecatedAttribute>();
        
        if (deprecatedAttr is not null)
        {
            // Headers standards de dépréciation
            context.Response.Headers.Append(
                "Deprecation", 
                "true");
            
            context.Response.Headers.Append(
                "Sunset", 
                deprecatedAttr.SunsetDate);
            
            context.Response.Headers.Append(
                "X-Deprecation-Message", 
                deprecatedAttr.Message);
            
            if (deprecatedAttr.SuccessorVersion is not null)
            {
                context.Response.Headers.Append(
                    "Link",
                    $"</api/{deprecatedAttr.SuccessorVersion}>; rel=\"successor-version\"");
            }
        }
    }
}

// Utilisation sur un controller
[ApiController]
[ApiVersion("1.0", Deprecated = true)]
[Route("api/v{version:apiVersion}/tenants")]
[ApiDeprecated(
    "V1 will be sunset on 2025-06-01. Please migrate to V2.",
    "2025-06-01",
    "v2")]
public sealed class TenantsV1Controller : ControllerBase
{
    // ...
}
```

### 7. Changelog et documentation

```csharp
/// <summary>
/// Documentation des changements entre versions.
/// </summary>

/*
═══════════════════════════════════════════════════════════════
API CHANGELOG
═══════════════════════════════════════════════════════════════

## v2.0.0 (2025-03-01)

### Breaking Changes
- `GET /tenants/{id}`: Le champ `email` a été remplacé par `contacts[]`
- `POST /tenants`: Le champ `email` a été remplacé par `contacts[]`
- Les timestamps utilisent maintenant `DateTimeOffset` au lieu de `DateTime`

### New Features
- `POST /tenants/{id}/completions/stream`: Nouveau endpoint de streaming SSE
- Ajout de `quotaStatus` dans les réponses tenant
- Ajout de `settings` dans les réponses tenant

### Deprecations
- V1 sera sunset le 2025-06-01

## v1.0.0 (2024-01-01)

### Initial Release
- CRUD tenants
- CRUD API keys
- LLM completions (non-streaming)

═══════════════════════════════════════════════════════════════
*/

/// <summary>
/// Endpoint pour récupérer le changelog.
/// </summary>
[ApiController]
[Route("api")]
public sealed class ApiInfoController : ControllerBase
{
    [HttpGet("versions")]
    [ProducesResponseType(typeof(ApiVersionsResponse), StatusCodes.Status200OK)]
    public IActionResult GetVersions()
    {
        return Ok(new ApiVersionsResponse
        {
            Versions = new[]
            {
                new ApiVersionInfo
                {
                    Version = "v1",
                    Status = "deprecated",
                    SunsetDate = new DateOnly(2025, 6, 1),
                    DocumentationUrl = "/swagger/v1/swagger.json"
                },
                new ApiVersionInfo
                {
                    Version = "v2",
                    Status = "current",
                    SunsetDate = null,
                    DocumentationUrl = "/swagger/v2/swagger.json"
                }
            },
            CurrentVersion = "v2",
            ChangelogUrl = "/api/changelog"
        });
    }
}
```

### 8. Tests de non-régression

```csharp
/// <summary>
/// Tests de compatibilité entre versions.
/// </summary>
public sealed class ApiVersionCompatibilityTests
{
    private readonly HttpClient _client;
    
    [Fact]
    public async Task V1_Endpoint_Should_Still_Work_When_V2_Exists()
    {
        // Arrange
        var tenantId = await CreateTestTenantAsync();
        
        // Act - V1 doit toujours fonctionner
        var responseV1 = await _client.GetAsync($"/api/v1/tenants/{tenantId}");
        
        // Assert
        responseV1.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var contentV1 = await responseV1.Content
            .ReadFromJsonAsync<TenantResponseV1>();
        
        contentV1.Should().NotBeNull();
        contentV1!.Email.Should().NotBeNullOrEmpty(); // V1 a toujours Email
    }
    
    [Fact]
    public async Task V1_Response_Should_Be_Backwards_Compatible()
    {
        // Arrange - Client avec ancien schema V1
        var oldSchemaJson = """
            {
                "id": "00000000-0000-0000-0000-000000000000",
                "name": "string",
                "slug": "string",
                "status": "string",
                "email": "string",
                "createdAt": "2024-01-01T00:00:00Z"
            }
            """;
        
        var oldSchema = JsonDocument.Parse(oldSchemaJson);
        
        // Act
        var response = await _client.GetAsync("/api/v1/tenants/some-id");
        var content = await response.Content.ReadAsStringAsync();
        var actualJson = JsonDocument.Parse(content);
        
        // Assert - Tous les champs attendus sont présents
        foreach (var expectedProp in oldSchema.RootElement.EnumerateObject())
        {
            actualJson.RootElement.TryGetProperty(expectedProp.Name, out _)
                .Should().BeTrue($"V1 response should contain '{expectedProp.Name}'");
        }
    }
    
    [Fact]
    public async Task V1_Should_Return_Deprecation_Headers()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/tenants");
        
        // Assert
        response.Headers.Should().ContainKey("Deprecation");
        response.Headers.Should().ContainKey("Sunset");
        
        var sunset = response.Headers.GetValues("Sunset").First();
        DateOnly.Parse(sunset).Should().Be(new DateOnly(2025, 6, 1));
    }
}
```

### 9. Migration guide pour les clients

```markdown
# Migration Guide: V1 → V2

## Breaking Changes

### 1. Tenant Email → Contacts

**V1 (deprecated):**
```json
{
  "id": "...",
  "name": "Acme Corp",
  "email": "admin@acme.com"
}
```

**V2:**
```json
{
  "id": "...",
  "name": "Acme Corp",
  "contacts": [
    { "type": "admin", "email": "admin@acme.com" },
    { "type": "billing", "email": "billing@acme.com" }
  ]
}
```

**Migration:** Remplacer `response.email` par `response.contacts.find(c => c.type === 'admin')?.email`

### 2. DateTime → DateTimeOffset

**V1:** `"createdAt": "2024-01-01T00:00:00"`
**V2:** `"createdAt": "2024-01-01T00:00:00+00:00"`

**Migration:** Parser avec support timezone

### 3. Nouveaux endpoints

- `POST /api/v2/tenants/{id}/completions/stream` - Streaming SSE
```

## Conséquences

### Positives

- **Compatibilité** : Clients existants non impactés
- **Évolution** : API peut évoluer librement
- **Transition** : Période de migration pour les clients
- **Documentation** : Versions clairement documentées

### Négatives

- **Maintenance** : Plusieurs versions à maintenir
  - *Mitigation* : Limiter à 2 versions actives max
- **Complexité** : Code dupliqué entre versions
  - *Mitigation* : Handlers partagés, seuls DTOs versionnés

### Neutres

- Standard de l'industrie pour les APIs publiques
- Swagger supporte nativement le multi-version

## Alternatives considérées

### Option A : Versioning par header

- **Description** : `Accept-Version: 1.0`
- **Avantages** : URLs propres
- **Inconvénients** : Moins visible, cache complexe
- **Raison du rejet** : Moins explicite pour les développeurs

### Option B : Pas de versioning (évolution additive)

- **Description** : Uniquement ajouts, jamais de suppressions
- **Avantages** : Simple, une seule version
- **Inconvénients** : Accumulation de dette, renommages impossibles
- **Raison du rejet** : Limitant sur le long terme

## Références

- [API Versioning Best Practices](https://www.troyhunt.com/your-api-versioning-is-wrong-which-is/)
- [Microsoft REST API Guidelines](https://github.com/microsoft/api-guidelines)
- [Asp.Versioning](https://github.com/dotnet/aspnet-api-versioning)
- [Sunset Header RFC](https://datatracker.ietf.org/doc/html/rfc8594)
