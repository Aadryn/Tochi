using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;

namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Repository interface for LLMProvider aggregate (Port)
/// </summary>
public interface ILLMProviderRepository
{
    Task<LLMProvider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<LLMProvider>> GetByTenantIdAsync(Guid tenantId, bool onlyActive = true, CancellationToken cancellationToken = default);
    Task<IEnumerable<LLMProvider>> GetByRoutingStrategyAsync(Guid tenantId, RoutingMethod method, string value, CancellationToken cancellationToken = default);
    Task AddAsync(LLMProvider provider, CancellationToken cancellationToken = default);
    Task UpdateAsync(LLMProvider provider, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
