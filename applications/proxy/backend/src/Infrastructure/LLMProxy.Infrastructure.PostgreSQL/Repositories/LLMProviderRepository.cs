using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.PostgreSQL.Repositories;

/// <summary>
/// Implémentation du repository pour l'agrégat LLMProvider avec support Result Pattern.
/// </summary>
internal class LLMProviderRepository : RepositoryBase<LLMProvider>, ILLMProviderRepository
{
    public LLMProviderRepository(LLMProxyDbContext context, ILogger<LLMProviderRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<Result<IReadOnlyList<LLMProvider>>> GetByTenantIdAsync(Guid tenantId, bool onlyActive = true, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = DbSet.Where(p => p.TenantId == tenantId);
            if (onlyActive) query = query.Where(p => p.IsActive);
            var providers = await query.OrderBy(p => p.Priority).ThenBy(p => p.Name).ToListAsync(cancellationToken);
            Logger.LogDebug("Récupéré {Count} LLMProviders pour tenant {TenantId} (onlyActive={OnlyActive})", providers.Count, tenantId, onlyActive);
            return Result<IReadOnlyList<LLMProvider>>.Success(providers.AsReadOnly());
        }
        catch (OperationCanceledException) { Logger.LogInformation("Opération GetByTenantIdAsync annulée"); throw; }
        catch (Exception ex) { Logger.LogError(ex, "Erreur lors de la récupération des LLMProviders du tenant {TenantId}", tenantId); return Error.Database.AccessError("GetByTenantIdAsync", ex.Message); }
    }

    public async Task<Result<IReadOnlyList<LLMProvider>>> GetByRoutingStrategyAsync(Guid tenantId, RoutingMethod method, string value, CancellationToken cancellationToken = default)
    {
        try
        {
            var providers = await DbSet.Where(p => p.TenantId == tenantId && p.IsActive).ToListAsync(cancellationToken);
            Logger.LogDebug("Récupéré {Count} LLMProviders actifs pour tenant {TenantId} avec stratégie {Method}", providers.Count, tenantId, method);
            return Result<IReadOnlyList<LLMProvider>>.Success(providers.AsReadOnly());
        }
        catch (OperationCanceledException) { Logger.LogInformation("Opération GetByRoutingStrategyAsync annulée"); throw; }
        catch (Exception ex) { Logger.LogError(ex, "Erreur lors de la récupération des LLMProviders par stratégie pour tenant {TenantId}", tenantId); return Error.Database.AccessError("GetByRoutingStrategyAsync", ex.Message); }
    }
}
