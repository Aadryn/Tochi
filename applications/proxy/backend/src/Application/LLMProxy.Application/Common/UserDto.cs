namespace LLMProxy.Application.Common;

/// <summary>
/// DTO représentant un utilisateur
/// </summary>
public record UserDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
