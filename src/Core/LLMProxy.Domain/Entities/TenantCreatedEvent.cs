using LLMProxy.Domain.Common;

namespace LLMProxy.Domain.Entities;

/// <summary>
/// Événement de domaine déclenché lors de la création d'un tenant
/// </summary>
public record TenantCreatedEvent(Guid TenantId, string TenantName) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
