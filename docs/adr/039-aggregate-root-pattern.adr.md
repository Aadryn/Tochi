# 39. Aggregate Root Pattern

Date: 2025-12-21

## Statut

Accepté

## Contexte

Dans un domaine riche, les entités sont liées entre elles :
- **Tenant** possède des **ApiKeys**
- **Tenant** a des **Contacts**
- **Tenant** a des **Quotas**
- **ApiKey** appartient à un **Tenant**

Sans frontières claires :
- N'importe quel code peut modifier n'importe quelle entité
- Les invariants métier sont violés
- Les transactions sont incohérentes
- Le code devient un plat de spaghetti

```csharp
// ❌ SANS AGGREGATE ROOT : Accès direct à tout
public class ApiKeyService
{
    public async Task CreateApiKeyAsync(Guid tenantId, string name)
    {
        // Accès direct - Tenant pas vérifié
        var apiKey = new ApiKey
        {
            TenantId = tenantId,
            Name = name,
            Status = ApiKeyStatus.Active
        };
        
        // Qui vérifie que le Tenant est actif ?
        // Qui vérifie la limite de clés par tenant ?
        // Qui garantit la cohérence ?
        await _apiKeyRepository.AddAsync(apiKey);
    }
}
```

## Décision

**Utiliser le pattern Aggregate Root pour définir des frontières de cohérence transactionnelle et garantir les invariants métier.**

### 1. Définition de l'Aggregate

```csharp
/// <summary>
/// Un Aggregate est un cluster d'entités traitées comme une unité :
/// 
/// ┌─────────────────────────────────────────────────────────────┐
/// │                    AGGREGATE TENANT                          │
/// │                                                              │
/// │  ┌──────────────────────────────────────────────────────┐   │
/// │  │              TENANT (Aggregate Root)                  │   │
/// │  │                                                       │   │
/// │  │  • Point d'entrée UNIQUE pour modifications           │   │
/// │  │  • Garantit les invariants métier                     │   │
/// │  │  • Gère la cohérence transactionnelle                 │   │
/// │  └───────────────────────────────────────────────────────┘   │
/// │           │                    │                   │         │
/// │           ▼                    ▼                   ▼         │
/// │  ┌─────────────┐    ┌──────────────┐    ┌──────────────┐    │
/// │  │  Contact    │    │   ApiKey     │    │    Quota     │    │
/// │  │  (Entity)   │    │   (Entity)   │    │   (Entity)   │    │
/// │  └─────────────┘    └──────────────┘    └──────────────┘    │
/// │                                                              │
/// │  RÈGLES :                                                    │
/// │  • Accès externe UNIQUEMENT via Tenant                       │
/// │  • Pas de Repository pour ApiKey, Contact, Quota             │
/// │  • Une transaction = un Aggregate                            │
/// │  • Références entre Aggregates = ID seulement                │
/// └─────────────────────────────────────────────────────────────┘
/// </summary>
```

### 2. Aggregate Root : Tenant

```csharp
/// <summary>
/// Aggregate Root pour le domaine Tenant.
/// Point d'entrée unique pour toutes les modifications.
/// </summary>
public sealed class Tenant : AggregateRoot
{
    private readonly List<Contact> _contacts = new();
    private readonly List<ApiKey> _apiKeys = new();
    private readonly List<TenantQuota> _quotas = new();
    
    // Propriétés publiques en lecture seule
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public TenantStatus Status { get; private set; }
    public TenantSettings Settings { get; private set; } = TenantSettings.Default;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    
    // Collections exposées en lecture seule
    public IReadOnlyList<Contact> Contacts => _contacts.AsReadOnly();
    public IReadOnlyList<ApiKey> ApiKeys => _apiKeys.AsReadOnly();
    public IReadOnlyList<TenantQuota> Quotas => _quotas.AsReadOnly();
    
    // Invariants métier
    private const int MaxApiKeysPerTenant = 10;
    private const int MaxContactsPerTenant = 5;
    
    // Constructeur privé - Factory method obligatoire
    private Tenant() { }
    
    /// <summary>
    /// Factory method pour créer un nouveau Tenant.
    /// Garantit que l'Aggregate est dans un état valide dès la création.
    /// </summary>
    public static Result<Tenant> Create(
        string name,
        string slug,
        Contact billingContact)
    {
        // Validations
        if (string.IsNullOrWhiteSpace(name))
            return Result<Tenant>.Failure("Name is required");
        
        if (string.IsNullOrWhiteSpace(slug))
            return Result<Tenant>.Failure("Slug is required");
        
        if (!SlugValidator.IsValid(slug))
            return Result<Tenant>.Failure("Invalid slug format");
        
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Slug = slug.ToLowerInvariant(),
            Status = TenantStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        // Ajouter le contact obligatoire
        tenant._contacts.Add(billingContact);
        
        // Ajouter les quotas par défaut
        tenant._quotas.Add(TenantQuota.CreateDefault(QuotaType.MonthlyTokens));
        tenant._quotas.Add(TenantQuota.CreateDefault(QuotaType.MonthlyRequests));
        
        // Émettre l'événement de domaine
        tenant.AddDomainEvent(new TenantCreatedEvent(tenant.Id, tenant.Name));
        
        return Result<Tenant>.Success(tenant);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // MÉTHODES DE MODIFICATION (encapsulent la logique métier)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Ajoute une clé API au tenant.
    /// </summary>
    public Result<ApiKey> AddApiKey(string name, IReadOnlyList<string> scopes)
    {
        // Invariant : Tenant doit être actif
        if (Status != TenantStatus.Active)
            return Result<ApiKey>.Failure("Cannot add API key to inactive tenant");
        
        // Invariant : Limite de clés par tenant
        var activeKeys = _apiKeys.Count(k => k.Status == ApiKeyStatus.Active);
        if (activeKeys >= MaxApiKeysPerTenant)
            return Result<ApiKey>.Failure($"Maximum {MaxApiKeysPerTenant} API keys allowed");
        
        // Invariant : Nom unique dans le tenant
        if (_apiKeys.Any(k => k.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            return Result<ApiKey>.Failure("API key name already exists");
        
        // Créer la clé
        var apiKey = ApiKey.Create(Id, name, scopes);
        _apiKeys.Add(apiKey);
        
        UpdatedAt = DateTimeOffset.UtcNow;
        
        AddDomainEvent(new ApiKeyCreatedEvent(Id, apiKey.Id, name));
        
        return Result<ApiKey>.Success(apiKey);
    }
    
    /// <summary>
    /// Révoque une clé API.
    /// </summary>
    public Result RevokeApiKey(Guid apiKeyId, string reason)
    {
        var apiKey = _apiKeys.FirstOrDefault(k => k.Id == apiKeyId);
        
        if (apiKey is null)
            return Result.Failure("API key not found");
        
        if (apiKey.Status == ApiKeyStatus.Revoked)
            return Result.Failure("API key already revoked");
        
        apiKey.Revoke(reason);
        UpdatedAt = DateTimeOffset.UtcNow;
        
        AddDomainEvent(new ApiKeyRevokedEvent(Id, apiKeyId, reason));
        
        return Result.Success();
    }
    
    /// <summary>
    /// Ajoute un contact.
    /// </summary>
    public Result<Contact> AddContact(ContactType type, string email, string? name = null)
    {
        if (_contacts.Count >= MaxContactsPerTenant)
            return Result<Contact>.Failure($"Maximum {MaxContactsPerTenant} contacts allowed");
        
        // Un seul contact de chaque type
        if (_contacts.Any(c => c.Type == type))
            return Result<Contact>.Failure($"Contact of type {type} already exists");
        
        var contact = Contact.Create(type, email, name);
        _contacts.Add(contact);
        
        UpdatedAt = DateTimeOffset.UtcNow;
        
        return Result<Contact>.Success(contact);
    }
    
    /// <summary>
    /// Met à jour un contact existant.
    /// </summary>
    public Result UpdateContact(ContactType type, string email, string? name = null)
    {
        var contact = _contacts.FirstOrDefault(c => c.Type == type);
        
        if (contact is null)
            return Result.Failure($"Contact of type {type} not found");
        
        contact.Update(email, name);
        UpdatedAt = DateTimeOffset.UtcNow;
        
        return Result.Success();
    }
    
    /// <summary>
    /// Désactive le tenant.
    /// </summary>
    public Result Deactivate(string reason)
    {
        if (Status == TenantStatus.Deactivated)
            return Result.Failure("Tenant already deactivated");
        
        var previousStatus = Status;
        Status = TenantStatus.Deactivated;
        UpdatedAt = DateTimeOffset.UtcNow;
        
        // Révoquer toutes les clés API actives
        foreach (var apiKey in _apiKeys.Where(k => k.Status == ApiKeyStatus.Active))
        {
            apiKey.Revoke("Tenant deactivated");
        }
        
        AddDomainEvent(new TenantDeactivatedEvent(Id, previousStatus, reason));
        
        return Result.Success();
    }
    
    /// <summary>
    /// Réactive le tenant.
    /// </summary>
    public Result Reactivate()
    {
        if (Status != TenantStatus.Deactivated)
            return Result.Failure("Tenant is not deactivated");
        
        Status = TenantStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
        
        AddDomainEvent(new TenantReactivatedEvent(Id));
        
        return Result.Success();
    }
    
    /// <summary>
    /// Consomme du quota.
    /// </summary>
    public Result ConsumeQuota(QuotaType type, long amount)
    {
        var quota = _quotas.FirstOrDefault(q => q.Type == type);
        
        if (quota is null)
            return Result.Failure($"Quota {type} not configured");
        
        return quota.Consume(amount);
    }
    
    /// <summary>
    /// Vérifie si le quota permet la consommation.
    /// </summary>
    public bool CanConsumeQuota(QuotaType type, long amount)
    {
        var quota = _quotas.FirstOrDefault(q => q.Type == type);
        return quota?.CanConsume(amount) ?? false;
    }
}
```

### 3. Entités internes à l'Aggregate

```csharp
/// <summary>
/// Entité ApiKey - Accessible uniquement via Tenant.
/// </summary>
public sealed class ApiKey : Entity
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string KeyHash { get; private set; } = string.Empty;
    public string KeyPrefix { get; private set; } = string.Empty;
    public IReadOnlyList<string> Scopes { get; private set; } = Array.Empty<string>();
    public ApiKeyStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? RevocationReason { get; private set; }
    
    // Constructeur interne - Création via Tenant uniquement
    private ApiKey() { }
    
    internal static ApiKey Create(
        Guid tenantId,
        string name,
        IReadOnlyList<string> scopes)
    {
        var (keyHash, keyPrefix, _) = GenerateKey();
        
        return new ApiKey
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            KeyHash = keyHash,
            KeyPrefix = keyPrefix,
            Scopes = scopes,
            Status = ApiKeyStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
    
    // Modification interne uniquement
    internal void Revoke(string reason)
    {
        Status = ApiKeyStatus.Revoked;
        RevokedAt = DateTimeOffset.UtcNow;
        RevocationReason = reason;
    }
    
    internal void SetExpiration(DateTimeOffset expiresAt)
    {
        ExpiresAt = expiresAt;
    }
    
    private static (string Hash, string Prefix, string PlainKey) GenerateKey()
    {
        var plainKey = $"sk_live_{Guid.NewGuid():N}";
        var hash = BCrypt.Net.BCrypt.HashPassword(plainKey);
        var prefix = plainKey[..12];
        return (hash, prefix, plainKey);
    }
}

/// <summary>
/// Entité Contact - Accessible uniquement via Tenant.
/// </summary>
public sealed class Contact : Entity
{
    public Guid Id { get; private set; }
    public ContactType Type { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string? Name { get; private set; }
    public string? Phone { get; private set; }
    
    private Contact() { }
    
    internal static Contact Create(ContactType type, string email, string? name)
    {
        return new Contact
        {
            Id = Guid.NewGuid(),
            Type = type,
            Email = email,
            Name = name
        };
    }
    
    internal void Update(string email, string? name)
    {
        Email = email;
        Name = name;
    }
}

/// <summary>
/// Entité Quota - Accessible uniquement via Tenant.
/// </summary>
public sealed class TenantQuota : Entity
{
    public Guid Id { get; private set; }
    public QuotaType Type { get; private set; }
    public long Limit { get; private set; }
    public long CurrentUsage { get; private set; }
    public DateTimeOffset? ResetAt { get; private set; }
    
    private TenantQuota() { }
    
    internal static TenantQuota CreateDefault(QuotaType type)
    {
        var defaultLimit = type switch
        {
            QuotaType.MonthlyTokens => 1_000_000,
            QuotaType.MonthlyRequests => 10_000,
            _ => 0
        };
        
        return new TenantQuota
        {
            Id = Guid.NewGuid(),
            Type = type,
            Limit = defaultLimit,
            CurrentUsage = 0,
            ResetAt = GetNextMonthStart()
        };
    }
    
    internal bool CanConsume(long amount)
    {
        CheckAndResetIfNeeded();
        return CurrentUsage + amount <= Limit;
    }
    
    internal Result Consume(long amount)
    {
        CheckAndResetIfNeeded();
        
        if (CurrentUsage + amount > Limit)
            return Result.Failure($"Quota {Type} exceeded");
        
        CurrentUsage += amount;
        return Result.Success();
    }
    
    private void CheckAndResetIfNeeded()
    {
        if (ResetAt.HasValue && DateTimeOffset.UtcNow >= ResetAt.Value)
        {
            CurrentUsage = 0;
            ResetAt = GetNextMonthStart();
        }
    }
    
    private static DateTimeOffset GetNextMonthStart()
    {
        var now = DateTimeOffset.UtcNow;
        return new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero)
            .AddMonths(1);
    }
}
```

### 4. Repository pour l'Aggregate Root uniquement

```csharp
/// <summary>
/// Repository UNIQUEMENT pour l'Aggregate Root.
/// Pas de repository pour ApiKey, Contact, Quota.
/// </summary>
public interface ITenantRepository
{
    /// <summary>
    /// Récupère un Tenant complet avec toutes ses entités.
    /// </summary>
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default);
    
    /// <summary>
    /// Récupère un Tenant par son slug.
    /// </summary>
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default);
    
    /// <summary>
    /// Vérifie si un slug existe déjà.
    /// </summary>
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);
    
    /// <summary>
    /// Ajoute un nouveau Tenant.
    /// </summary>
    void Add(Tenant tenant);
    
    /// <summary>
    /// Met à jour un Tenant existant.
    /// L'Aggregate entier est sauvegardé (Unit of Work).
    /// </summary>
    void Update(Tenant tenant);
}

/// <summary>
/// Implémentation EF Core.
/// </summary>
public sealed class TenantRepository : ITenantRepository
{
    private readonly LlmProxyDbContext _context;
    
    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        // Charger l'Aggregate complet en une requête
        return await _context.Tenants
            .Include(t => t.Contacts)
            .Include(t => t.ApiKeys)
            .Include(t => t.Quotas)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }
    
    public void Add(Tenant tenant)
    {
        _context.Tenants.Add(tenant);
    }
    
    public void Update(Tenant tenant)
    {
        _context.Tenants.Update(tenant);
    }
}
```

### 5. Application Service utilisant l'Aggregate

```csharp
/// <summary>
/// Handler qui utilise l'Aggregate Root.
/// </summary>
public sealed class CreateApiKeyHandler : IRequestHandler<CreateApiKeyCommand, Result<ApiKeyResponse>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<Result<ApiKeyResponse>> Handle(
        CreateApiKeyCommand request,
        CancellationToken ct)
    {
        // 1. Charger l'Aggregate Root
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
        
        if (tenant is null)
            return Result<ApiKeyResponse>.Failure("Tenant not found");
        
        // 2. Utiliser la méthode de l'Aggregate (encapsule la logique)
        var result = tenant.AddApiKey(request.Name, request.Scopes);
        
        if (result.IsFailure)
            return Result<ApiKeyResponse>.Failure(result.Error);
        
        // 3. Sauvegarder l'Aggregate entier
        _tenantRepository.Update(tenant);
        await _unitOfWork.SaveChangesAsync(ct);
        
        // 4. Les Domain Events sont dispatchés par le UnitOfWork
        
        return Result<ApiKeyResponse>.Success(result.Value.ToResponse());
    }
}
```

### 6. Règles pour les références entre Aggregates

```csharp
/// <summary>
/// Références entre Aggregates : ID uniquement, jamais d'objet.
/// </summary>

// ❌ MAUVAIS : Référence directe à un autre Aggregate
public class LlmRequest
{
    public Tenant Tenant { get; set; } // Navigation vers Aggregate = BAD
}

// ✅ BON : Référence par ID uniquement
public class LlmRequest
{
    public Guid TenantId { get; set; } // ID seulement
    
    // Si besoin des données Tenant, charger séparément via son Repository
}

/// <summary>
/// Aggregate LlmRequest (séparé de Tenant).
/// </summary>
public sealed class LlmRequest : AggregateRoot
{
    public Guid Id { get; private set; }
    
    // Référence par ID - pas de navigation
    public Guid TenantId { get; private set; }
    public Guid ApiKeyId { get; private set; }
    
    public string Model { get; private set; } = string.Empty;
    public string Provider { get; private set; } = string.Empty;
    public int InputTokens { get; private set; }
    public int OutputTokens { get; private set; }
    public LlmRequestStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    
    private LlmRequest() { }
    
    public static LlmRequest Create(
        Guid tenantId,
        Guid apiKeyId,
        string model,
        string provider)
    {
        return new LlmRequest
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ApiKeyId = apiKeyId,
            Model = model,
            Provider = provider,
            Status = LlmRequestStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
    
    public void Complete(int inputTokens, int outputTokens)
    {
        InputTokens = inputTokens;
        OutputTokens = outputTokens;
        Status = LlmRequestStatus.Completed;
        
        AddDomainEvent(new LlmRequestCompletedEvent(
            Id, TenantId, InputTokens, OutputTokens));
    }
}
```

### 7. Base classes

```csharp
/// <summary>
/// Classe de base pour les Aggregate Roots.
/// </summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = new();
    
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

/// <summary>
/// Classe de base pour les entités.
/// </summary>
public abstract class Entity
{
    // Peut contenir des méthodes communes
}

/// <summary>
/// Interface pour les événements de domaine.
/// </summary>
public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
```

## Conséquences

### Positives

- **Invariants garantis** : La logique métier est encapsulée
- **Cohérence** : Transaction = 1 Aggregate
- **Testabilité** : L'Aggregate est testable isolément
- **Évolution** : La structure interne peut changer sans impacter l'extérieur

### Négatives

- **Chargement** : L'Aggregate complet est chargé à chaque fois
  - *Mitigation* : Projections read-only pour les requêtes
- **Taille** : Aggregates trop gros = problèmes de performance
  - *Mitigation* : Bien définir les frontières

### Neutres

- Pattern DDD standard
- Nécessite une réflexion sur les frontières métier

## Alternatives considérées

### Option A : Entités indépendantes

- **Description** : Chaque entité a son repository
- **Avantages** : Flexible, granulaire
- **Inconvénients** : Pas de garantie d'invariants
- **Raison du rejet** : Incohérences possibles

### Option B : Un seul gros Aggregate

- **Description** : Tout dans un Aggregate
- **Avantages** : Cohérence totale
- **Inconvénients** : Performance, concurrence
- **Raison du rejet** : Ne scale pas

## Références

- [DDD Aggregates - Martin Fowler](https://martinfowler.com/bliki/DDD_Aggregate.html)
- [Effective Aggregate Design - Vaughn Vernon](https://www.dddcommunity.org/library/vernon_2011/)
- [Domain-Driven Design - Eric Evans](https://www.domainlanguage.com/ddd/)
