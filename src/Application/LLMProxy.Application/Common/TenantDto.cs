namespace LLMProxy.Application.Common;

/// <summary>
/// DTO représentant un tenant
/// </summary>
public record TenantDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public TenantSettingsDto Settings { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
