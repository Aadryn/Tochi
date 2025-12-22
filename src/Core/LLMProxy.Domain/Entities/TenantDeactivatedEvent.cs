using LLMProxy.Domain.Common;

namespace LLMProxy.Domain.Entities;

/// <summary>
/// Événement de domaine déclenché lors de la désactivation d'un tenant.
/// </summary>
/// <param name="TenantId">Identifiant du tenant désactivé.</param>
public sealed record TenantDeactivatedEvent(Guid TenantId) : DomainEvent;
