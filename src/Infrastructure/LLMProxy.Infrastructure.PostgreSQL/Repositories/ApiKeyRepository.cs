using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.PostgreSQL.Repositories;

/// <summary>
/// Implémentation du repository pour l'agrégat ApiKey avec support Result Pattern.
/// </summary>
internal class ApiKeyRepository : RepositoryBase<ApiKey>, IApiKeyRepository
{
    public ApiKeyRepository(LLMProxyDbContext context, ILogger<ApiKeyRepository> logger) 
        : base(context, logger)
    {
    }

    public override async Task<Result<ApiKey>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var apiKey = await DbSet.Include(k => k.User).FirstOrDefaultAsync(k => k.Id == id, cancellationToken);
            if (apiKey is null)
            {
                Logger.LogWarning("ApiKey avec ID {Id} non trouvée", id);
                return Error.ApiKey.NotFound(id);
            }
            return Result<ApiKey>.Success(apiKey);
        }
        catch (OperationCanceledException) { Logger.LogInformation("Opération GetByIdAsync annulée pour ApiKey {Id}", id); throw; }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erreur lors de la récupération de ApiKey avec ID {Id}", id);
            return Error.Database.AccessError("GetByIdAsync(ApiKey)", ex.Message);
        }
    }

    public async Task<Result<ApiKey>> GetByKeyHashAsync(string keyHash, CancellationToken cancellationToken = default)
    {
        try
        {
            var apiKey = await DbSet.Include(k => k.User).FirstOrDefaultAsync(k => k.KeyHash == keyHash, cancellationToken);
            if (apiKey is null) { Logger.LogWarning("ApiKey avec hash non trouvée"); return Error.ApiKey.InvalidHash; }
            return Result<ApiKey>.Success(apiKey);
        }
        catch (OperationCanceledException) { Logger.LogInformation("Opération GetByKeyHashAsync annulée"); throw; }
        catch (Exception ex) { Logger.LogError(ex, "Erreur lors de la récupération de ApiKey par hash"); return Error.Database.AccessError("GetByKeyHashAsync", ex.Message); }
    }

    public async Task<Result<ApiKey>> GetByKeyPrefixAsync(string keyPrefix, CancellationToken cancellationToken = default)
    {
        try
        {
            var apiKey = await DbSet.Include(k => k.User).FirstOrDefaultAsync(k => k.KeyPrefix == keyPrefix, cancellationToken);
            if (apiKey is null) { Logger.LogWarning("ApiKey avec préfixe '{Prefix}' non trouvée", keyPrefix); return Error.ApiKey.InvalidPrefix(keyPrefix); }
            return Result<ApiKey>.Success(apiKey);
        }
        catch (OperationCanceledException) { Logger.LogInformation("Opération GetByKeyPrefixAsync annulée"); throw; }
        catch (Exception ex) { Logger.LogError(ex, "Erreur lors de la récupération de ApiKey par préfixe '{Prefix}'", keyPrefix); return Error.Database.AccessError("GetByKeyPrefixAsync", ex.Message); }
    }

    public async Task<Result<IReadOnlyList<ApiKey>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var apiKeys = await DbSet.Where(k => k.UserId == userId).OrderByDescending(k => k.CreatedAt).ToListAsync(cancellationToken);
            Logger.LogDebug("Récupéré {Count} ApiKeys pour user {UserId}", apiKeys.Count, userId);
            return Result<IReadOnlyList<ApiKey>>.Success(apiKeys.AsReadOnly());
        }
        catch (OperationCanceledException) { Logger.LogInformation("Opération GetByUserIdAsync annulée"); throw; }
        catch (Exception ex) { Logger.LogError(ex, "Erreur lors de la récupération des ApiKeys de l'utilisateur {UserId}", userId); return Error.Database.AccessError("GetByUserIdAsync", ex.Message); }
    }

    public async Task<Result<IReadOnlyList<ApiKey>>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var apiKeys = await DbSet.Where(k => k.TenantId == tenantId).OrderByDescending(k => k.CreatedAt).ToListAsync(cancellationToken);
            Logger.LogDebug("Récupéré {Count} ApiKeys pour tenant {TenantId}", apiKeys.Count, tenantId);
            return Result<IReadOnlyList<ApiKey>>.Success(apiKeys.AsReadOnly());
        }
        catch (OperationCanceledException) { Logger.LogInformation("Opération GetByTenantIdAsync annulée"); throw; }
        catch (Exception ex) { Logger.LogError(ex, "Erreur lors de la récupération des ApiKeys du tenant {TenantId}", tenantId); return Error.Database.AccessError("GetByTenantIdAsync", ex.Message); }
    }
}
