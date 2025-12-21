using LLMProxy.Domain.Common;

namespace LLMProxy.Domain.Entities;

/// <summary>
/// Événement de domaine déclenché lors de la création d'une clé API
/// </summary>
public record ApiKeyCreatedEvent(Guid ApiKeyId, string RawKey) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
