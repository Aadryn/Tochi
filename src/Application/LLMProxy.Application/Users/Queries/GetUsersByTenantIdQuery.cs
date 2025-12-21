using LLMProxy.Application.Common;

namespace LLMProxy.Application.Users.Queries;

/// <summary>
/// Requête pour récupérer tous les utilisateurs d'un tenant
/// </summary>
public record GetUsersByTenantIdQuery : IQuery<IEnumerable<UserDto>>
{
    public Guid TenantId { get; init; }
}
