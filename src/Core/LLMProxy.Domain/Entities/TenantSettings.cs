using LLMProxy.Domain.Common;

namespace LLMProxy.Domain.Entities;

/// <summary>
/// Paramètres d'un tenant (value object)
/// </summary>
public class TenantSettings : ValueObject
{
    public int MaxUsers { get; private set; }
    public int MaxProviders { get; private set; }
    public bool EnableAuditLogging { get; private set; }
    public int AuditRetentionDays { get; private set; }
    public bool EnableResponseCache { get; private set; }

    private TenantSettings() { }

    public TenantSettings(
        int maxUsers,
        int maxProviders,
        bool enableAuditLogging,
        int auditRetentionDays,
        bool enableResponseCache)
    {
        MaxUsers = maxUsers;
        MaxProviders = maxProviders;
        EnableAuditLogging = enableAuditLogging;
        AuditRetentionDays = auditRetentionDays;
        EnableResponseCache = enableResponseCache;
    }

    public static TenantSettings Default() => new(
        maxUsers: 100,
        maxProviders: 10,
        enableAuditLogging: true,
        auditRetentionDays: 90,
        enableResponseCache: true
    );

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return MaxUsers;
        yield return MaxProviders;
        yield return EnableAuditLogging;
        yield return AuditRetentionDays;
        yield return EnableResponseCache;
    }
}
