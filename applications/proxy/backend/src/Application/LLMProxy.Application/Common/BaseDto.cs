namespace LLMProxy.Application.Common;

/// <summary>
/// Classe DTO de base
/// </summary>
public abstract class BaseDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
