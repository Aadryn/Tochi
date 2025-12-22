using LLMProxy.Domain.Common;
using System.Diagnostics;

namespace LLMProxy.Domain.Entities;

/// <summary>
/// Représente un tenant dans le système multi-tenant avec isolation complète
/// </summary>
public class Tenant : Entity
{
    public string Name { get; private set; }
    public string Slug { get; private set; } // URL-friendly identifier
    public virtual bool IsActive { get; private set; }
    public DateTime? DeactivatedAt { get; private set; }
    
    // Settings
    public TenantSettings Settings { get; private set; }
    
    // Navigation properties
    private readonly List<User> _users = new();
    public IReadOnlyCollection<User> Users => _users.AsReadOnly();
    
    private readonly List<LLMProvider> _providers = new();
    public IReadOnlyCollection<LLMProvider> Providers => _providers.AsReadOnly();

    private Tenant() 
    { 
        Name = string.Empty;
        Slug = string.Empty;
        Settings = null!;
    } // EF Core

    /// <summary>
    /// Constructeur protégé permettant l'héritage par <see cref="NullTenant"/>.
    /// </summary>
    /// <param name="id">Identifiant unique du tenant.</param>
    /// <param name="name">Nom du tenant.</param>
    /// <param name="slug">Slug URL-friendly du tenant.</param>
    protected Tenant(Guid id, string name, string slug)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Slug = slug ?? throw new ArgumentNullException(nameof(slug));
        Settings = TenantSettings.Default();
        IsActive = true;
    }

    private Tenant(string name, string slug, TenantSettings settings)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Slug = slug ?? throw new ArgumentNullException(nameof(slug));
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        IsActive = true;
        
        // Invariants : Le tenant doit avoir un nom et un slug valides après construction
        Debug.Assert(!string.IsNullOrWhiteSpace(Name), "Tenant name must not be null or whitespace after construction");
        Debug.Assert(!string.IsNullOrWhiteSpace(Slug), "Tenant slug must not be null or whitespace after construction");
        Debug.Assert(IsActive, "Tenant must be active after construction");
        Debug.Assert(CreatedAt != default, "CreatedAt must be set after construction");
    }

    public static Result<Tenant> Create(string name, string slug, TenantSettings? settings = null)
    {
        try
        {
            Guard.AgainstNullOrWhiteSpace(name, nameof(name), "Tenant name cannot be empty.");
            Guard.AgainstNullOrWhiteSpace(slug, nameof(slug), "Slug cannot be empty.");
            if (!IsValidSlug(slug))
                return new Error("Validation.Slug.InvalidFormat", "Invalid tenant slug. Use only lowercase letters, numbers, and hyphens.");
        }
        catch (ArgumentException)
        {
            return Error.Validation.Required(nameof(name));
        }

        var tenantSettings = settings ?? TenantSettings.Default();
        var tenant = new Tenant(name, slug, tenantSettings);
        
        AddDomainEvent(tenant, new TenantCreatedEvent(tenant.Id, tenant.Name));
        
        return tenant;
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return new Error("Tenant.AlreadyDeactivated", "Tenant is already deactivated.");

        IsActive = false;
        DeactivatedAt = DateTime.UtcNow;
        MarkAsModified();
        
        AddDomainEvent(this, new TenantDeactivatedEvent(Id));
        
        // Post-condition : Le tenant doit être inactif après désactivation
        Debug.Assert(!IsActive, "Tenant must be inactive after deactivation");
        Debug.Assert(DeactivatedAt.HasValue, "DeactivatedAt must be set after deactivation");
        
        return Result.Success();
    }

    public Result Activate()
    {
        if (IsActive)
            return new Error("Tenant.AlreadyActive", "Tenant is already active.");

        IsActive = true;
        DeactivatedAt = null;
        MarkAsModified();
        
        // Post-condition : Le tenant doit être actif après activation
        Debug.Assert(IsActive, "Tenant must be active after activation");
        Debug.Assert(!DeactivatedAt.HasValue, "DeactivatedAt must be null after activation");
        
        return Result.Success();
    }

    public Result UpdateSettings(TenantSettings settings)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        MarkAsModified();
        
        return Result.Success();
    }

    private static bool IsValidSlug(string slug)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(slug, @"^[a-z0-9-]+$");
    }

    private static void AddDomainEvent(Tenant tenant, IDomainEvent domainEvent)
    {
        tenant.AddDomainEvent(domainEvent);
    }
}
