using LLMProxy.Domain.Common;

namespace LLMProxy.Domain.Entities;

/// <summary>
/// Événement de domaine déclenché lors de la création d'un tenant.
/// </summary>
/// <param name="TenantId">Identifiant du tenant créé.</param>
/// <param name="TenantName">Nom du tenant.</param>
public sealed record TenantCreatedEvent(Guid TenantId, string TenantName) : DomainEvent;
