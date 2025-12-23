using LLMProxy.Domain.Common;

namespace LLMProxy.Domain.Entities;

/// <summary>
/// Événement de domaine déclenché lors de la création d'une clé API.
/// </summary>
/// <param name="ApiKeyId">Identifiant de la clé API créée.</param>
/// <param name="RawKey">Clé brute (non hashée) - à afficher une seule fois.</param>
public sealed record ApiKeyCreatedEvent(Guid ApiKeyId, string RawKey) : DomainEvent;
