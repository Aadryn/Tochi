using LLMProxy.Application.Common;

namespace LLMProxy.Application.ApiKeys.Queries;

/// <summary>
/// Requête pour récupérer toutes les clés API d'un utilisateur
/// </summary>
public record GetApiKeysByUserIdQuery : IQuery<IEnumerable<ApiKeyDto>>
{
    public Guid UserId { get; init; }
}
