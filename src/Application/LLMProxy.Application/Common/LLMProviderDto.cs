namespace LLMProxy.Application.Common;

/// <summary>
/// DTO représentant un fournisseur LLM
/// </summary>
public record LLMProviderDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ProviderType { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = string.Empty;
    public string RoutingStrategy { get; init; } = string.Empty;
    public int Priority { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
