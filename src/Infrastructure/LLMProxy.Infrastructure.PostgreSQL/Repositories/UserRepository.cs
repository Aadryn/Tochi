using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Extensions;
using LLMProxy.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.PostgreSQL.Repositories;

/// <summary>
/// Implémentation du repository pour l'agrégat User avec support Result Pattern.
/// </summary>
internal class UserRepository : RepositoryBase<User>, IUserRepository
{
    public UserRepository(LLMProxyDbContext context, ILogger<UserRepository> logger) 
        : base(context, logger)
    {
    }

    /// <summary>
    /// Récupère un utilisateur par ID avec ses ApiKeys et QuotaLimits.
    /// </summary>
    public override async Task<Result<User>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await DbSet
                .Include(u => u.ApiKeys)
                .Include(u => u.QuotaLimits)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

            if (user is null)
            {
                Logger.LogWarning("User avec ID {Id} non trouvé", id);
                return Error.User.NotFound(id);
            }

            return Result<User>.Success(user);
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation("Opération GetByIdAsync annulée pour User {Id}", id);
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erreur lors de la récupération de User avec ID {Id}", id);
            return Error.Database.AccessError("GetByIdAsync(User)", ex.Message);
        }
    }

    /// <summary>
    /// Récupère un utilisateur par email dans un tenant spécifique.
    /// </summary>
    public async Task<Result<User>> GetByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedEmail = email.NormalizeEmail();
            var user = await DbSet.FirstOrDefaultAsync(
                u => u.TenantId == tenantId && u.Email == normalizedEmail, 
                cancellationToken);

            if (user is null)
            {
                Logger.LogWarning("User avec email '{Email}' non trouvé dans tenant {TenantId}", email, tenantId);
                return Error.User.NotFound(Guid.Empty);
            }

            return Result<User>.Success(user);
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation("Opération GetByEmailAsync annulée");
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erreur lors de la récupération de User par email '{Email}'", email);
            return Error.Database.AccessError("GetByEmailAsync", ex.Message);
        }
    }

    /// <summary>
    /// Récupère tous les utilisateurs d'un tenant.
    /// </summary>
    public async Task<Result<IReadOnlyList<User>>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await DbSet
                .Where(u => u.TenantId == tenantId)
                .OrderBy(u => u.Name)
                .ToListAsync(cancellationToken);

            Logger.LogDebug("Récupéré {Count} utilisateurs pour tenant {TenantId}", users.Count, tenantId);

            return Result<IReadOnlyList<User>>.Success(users.AsReadOnly());
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation("Opération GetByTenantIdAsync annulée");
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erreur lors de la récupération des users du tenant {TenantId}", tenantId);
            return Error.Database.AccessError("GetByTenantIdAsync", ex.Message);
        }
    }

    /// <summary>
    /// Vérifie si un email existe dans un tenant.
    /// </summary>
    public async Task<Result<bool>> EmailExistsAsync(Guid tenantId, string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedEmail = email.NormalizeEmail();
            var exists = await DbSet.AnyAsync(
                u => u.TenantId == tenantId && u.Email == normalizedEmail, 
                cancellationToken);

            Logger.LogDebug("Email '{Email}' existe dans tenant {TenantId}: {Exists}", email, tenantId, exists);

            return Result<bool>.Success(exists);
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation("Opération EmailExistsAsync annulée");
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erreur lors de la vérification d'existence de l'email '{Email}'", email);
            return Error.Database.AccessError("EmailExistsAsync", ex.Message);
        }
    }
}
