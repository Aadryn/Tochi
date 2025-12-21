using LLMProxy.Domain.Common;
using System.Diagnostics;

namespace LLMProxy.Domain.Entities;

/// <summary>
/// Représente un utilisateur au sein d'un tenant avec capacités d'authentification
/// </summary>
public class User : Entity
{
    public Guid TenantId { get; private set; }
    public string Email { get; private set; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; }
    public UserRole Role { get; private set; }
    
    // Navigation
    public Tenant Tenant { get; private set; } = null!;
    
    private readonly List<ApiKey> _apiKeys = new();
    public IReadOnlyCollection<ApiKey> ApiKeys => _apiKeys.AsReadOnly();
    
    private readonly List<QuotaLimit> _quotaLimits = new();
    public IReadOnlyCollection<QuotaLimit> QuotaLimits => _quotaLimits.AsReadOnly();

    private User() 
    {
        Email = string.Empty;
        Name = string.Empty;
    } // EF Core

    private User(Guid tenantId, string email, string name, UserRole role)
    {
        TenantId = tenantId;
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Role = role;
        IsActive = true;
        
        // Invariants : L'utilisateur doit avoir un email, nom et tenant valides après construction
        Debug.Assert(TenantId != Guid.Empty, "User must have a valid TenantId after construction");
        Debug.Assert(!string.IsNullOrWhiteSpace(Email), "User email must not be null or whitespace after construction");
        Debug.Assert(!string.IsNullOrWhiteSpace(Name), "User name must not be null or whitespace after construction");
        Debug.Assert(IsActive, "User must be active after construction");
        Debug.Assert(CreatedAt != default, "CreatedAt must be set after construction");
    }

    public static Result<User> Create(Guid tenantId, string email, string name, UserRole role)
    {
        if (tenantId == Guid.Empty)
            return Result.Failure<User>("Invalid tenant ID.");

        try
        {
            Guard.AgainstNullOrWhiteSpace(email, nameof(email), "Email cannot be empty.");
            if (!IsValidEmail(email))
                return Result.Failure<User>("Invalid email address.");

            Guard.AgainstNullOrWhiteSpace(name, nameof(name), "User name cannot be empty.");
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<User>(ex.Message);
        }

        var user = new User(tenantId, email.ToLowerInvariant(), name, role);
        
        return Result.Success(user);
    }

    public Result<ApiKey> CreateApiKey(string name, DateTime? expiresAt = null)
    {
        if (!IsActive)
            return Result.Failure<ApiKey>("Cannot create API key for inactive user.");

        var apiKeyResult = ApiKey.Create(Id, TenantId, name, expiresAt);
        if (apiKeyResult.IsFailure)
            return Result.Failure<ApiKey>(apiKeyResult.Error!);

        _apiKeys.Add(apiKeyResult.Value);
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Success(apiKeyResult.Value);
    }

    public Result SetQuotaLimit(QuotaType quotaType, long limit, QuotaPeriod period)
    {
        var existingQuota = _quotaLimits.FirstOrDefault(q => q.QuotaType == quotaType && q.Period == period);
        
        if (existingQuota != null)
        {
            var updateResult = existingQuota.UpdateLimit(limit);
            if (updateResult.IsFailure)
                return updateResult;
        }
        else
        {
            var quotaResult = QuotaLimit.Create(Id, TenantId, quotaType, limit, period);
            if (quotaResult.IsFailure)
                return Result.Failure(quotaResult.Error!);
            
            _quotaLimits.Add(quotaResult.Value);
        }

        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure("User is already inactive.");

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        
        // Revoke all active API keys
        foreach (var apiKey in _apiKeys.Where(k => k.IsActive))
        {
            apiKey.Revoke();
        }
        
        return Result.Success();
    }

    public Result Activate()
    {
        if (IsActive)
            return Result.Failure("User is already active.");

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Success();
    }

    public void UpdateName(string name)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name), "User name cannot be empty.");

        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateRole(UserRole role)
    {
        Role = role;
        UpdatedAt = DateTime.UtcNow;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
