using LLMProxy.Application.Common;
using LLMProxy.Application.Common.Interfaces;

namespace LLMProxy.Application.LLMProviders.Queries;

/// <summary>
/// Requête pour récupérer un fournisseur LLM par son identifiant.
/// </summary>
/// <remarks>
/// Cette Query est mise en cache avec une durée de vie de 60 minutes (ADR-042).
/// Raison : Les providers (OpenAI, Anthropic, etc.) changent très rarement (configuration quasi-statique).
/// </remarks>
public record GetProviderByIdQuery : IQuery<LLMProviderDto>, ICachedQuery<LLMProviderDto>
{
    public Guid ProviderId { get; init; }

    /// <summary>
    /// Durée de vie du cache : 60 minutes (données très stables).
    /// </summary>
    public TimeSpan? CacheExpiration => TimeSpan.FromMinutes(60);
}
