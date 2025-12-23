namespace LLMProxy.Application.Common;

/// <summary>
/// DTO représentant les paramètres d'un tenant
/// </summary>
public record TenantSettingsDto
{
    public int MaxUsers { get; init; }
    public int MaxProviders { get; init; }
    public bool EnableAuditLogging { get; init; }
    public int AuditRetentionDays { get; init; }
    public bool EnableResponseCache { get; init; }
}
