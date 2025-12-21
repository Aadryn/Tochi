using LLMProxy.Application.Common;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;

namespace LLMProxy.Application.Users.Commands;

/// <summary>
/// Commande de création d'un nouvel utilisateur.
/// </summary>
/// <remarks>
/// Permet de créer un utilisateur dans un tenant spécifique avec un email, un nom et un rôle.
/// </remarks>
public record CreateUserCommand : ICommand<UserDto>
{
    /// <summary>
    /// Obtient ou initialise l'identifiant unique du tenant auquel l'utilisateur appartient.
    /// </summary>
    public Guid TenantId { get; init; }

    /// <summary>
    /// Obtient ou initialise l'adresse email de l'utilisateur (doit être unique par tenant).
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Obtient ou initialise le nom complet de l'utilisateur.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Obtient ou initialise le rôle de l'utilisateur (User, Admin, TenantAdmin).
    /// </summary>
    public UserRole Role { get; init; } = UserRole.User;
}
