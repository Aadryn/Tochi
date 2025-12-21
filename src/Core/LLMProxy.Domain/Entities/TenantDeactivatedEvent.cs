using LLMProxy.Domain.Common;

namespace LLMProxy.Domain.Entities;

/// <summary>
/// Événement de domaine déclenché lors de la désactivation d'un tenant
/// </summary>
public record TenantDeactivatedEvent(Guid TenantId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
