using LLMProxy.Application.Common;

namespace LLMProxy.Application.Users.Queries;

/// <summary>
/// Requête pour récupérer un utilisateur par son identifiant
/// </summary>
public record GetUserByIdQuery : IQuery<UserDto>
{
    public Guid UserId { get; init; }
}
