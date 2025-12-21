using LLMProxy.Application.Common;

namespace LLMProxy.Application.Users.Commands;

/// <summary>
/// Commande pour mettre à jour un utilisateur existant.
/// </summary>
public record UpdateUserCommand : ICommand<UserDto>
{
    public Guid UserId { get; set; }
    public string Name { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}
