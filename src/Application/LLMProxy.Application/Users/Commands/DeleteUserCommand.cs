using LLMProxy.Application.Common;

namespace LLMProxy.Application.Users.Commands;

/// <summary>
/// Commande pour supprimer un utilisateur.
/// </summary>
public record DeleteUserCommand : ICommand
{
    public Guid UserId { get; init; }
}
