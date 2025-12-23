namespace LLMProxy.Application.Common;

/// <summary>
/// DTO représentant une clé API
/// </summary>
public record ApiKeyDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string KeyPrefix { get; init; } = string.Empty;
    public bool IsRevoked { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public DateTime? LastUsedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
