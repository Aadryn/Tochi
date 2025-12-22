using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;

namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Repository pour l'agrégat User (Port dans l'architecture hexagonale).
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Récupère un utilisateur par son email dans un tenant spécifique.
    /// </summary>
    Task<Result<User>> GetByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère tous les utilisateurs d'un tenant.
    /// </summary>
    Task<Result<IReadOnlyList<User>>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Vérifie si un email existe dans un tenant.
    /// </summary>
    Task<Result<bool>> EmailExistsAsync(Guid tenantId, string email, CancellationToken cancellationToken = default);
}
