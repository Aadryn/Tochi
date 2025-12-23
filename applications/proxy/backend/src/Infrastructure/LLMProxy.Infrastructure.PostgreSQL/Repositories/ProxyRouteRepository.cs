using LLMProxy.Domain.Entities.Routing;
using LLMProxy.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.PostgreSQL.Repositories;

/// <summary>
/// Implémentation PostgreSQL du repository pour les routes YARP.
/// </summary>
/// <remarks>
/// <para>
/// Ce repository gère la persistance des routes du reverse proxy.
/// Les routes définissent les règles de correspondance (path, méthodes, headers)
/// pour diriger le trafic vers les clusters appropriés.
/// </para>
/// <para>
/// <strong>Schéma :</strong> Utilise le schéma <c>routing</c> pour isoler les données YARP.
/// </para>
/// <para>
/// <strong>Performance :</strong> Les requêtes incluent des optimisations :
/// <list type="bullet">
/// <item>Index sur <c>tenant_id</c> pour les requêtes multi-tenant</item>
/// <item>Index sur <c>route_id</c> pour les lookups par identifiant métier</item>
/// <item>Index sur <c>is_enabled</c> pour filtrer les routes actives</item>
/// </list>
/// </para>
/// </remarks>
public class ProxyRouteRepository : IProxyRouteRepository
{
    private readonly LLMProxyDbContext _context;
    private readonly ILogger<ProxyRouteRepository> _logger;
    private readonly DbSet<ProxyRoute> _dbSet;

    /// <summary>
    /// Initialise une nouvelle instance du repository de routes.
    /// </summary>
    /// <param name="context">Contexte de base de données EF Core.</param>
    /// <param name="logger">Logger pour les opérations et erreurs.</param>
    public ProxyRouteRepository(LLMProxyDbContext context, ILogger<ProxyRouteRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbSet = context.Set<ProxyRoute>();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProxyRoute>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var routes = await _dbSet
                .Where(r => r.IsEnabled)
                .OrderBy(r => r.Order)
                .ThenBy(r => r.RouteId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return routes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des routes actives");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProxyRoute>> GetByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var routes = await _dbSet
                .Where(r => r.TenantId == tenantId)
                .OrderBy(r => r.Order)
                .ThenBy(r => r.RouteId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return routes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des routes pour le tenant {TenantId}", tenantId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ProxyRoute?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de la route {RouteId}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ProxyRoute?> GetByRouteIdAsync(
        string routeId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RouteId == routeId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de la route par RouteId {RouteId}", routeId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task AddAsync(
        ProxyRoute route,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbSet.AddAsync(route, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'ajout de la route {RouteId}", route.RouteId);
            throw;
        }
    }

    /// <inheritdoc />
    public Task UpdateAsync(
        ProxyRoute route,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _dbSet.Update(route);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour de la route {RouteId}", route.RouteId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var route = await _dbSet.FindAsync([id], cancellationToken);
            if (route is not null)
            {
                _dbSet.Remove(route);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de la route {RouteId}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(
        string routeId,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbSet.Where(r => r.RouteId == routeId);

            if (excludeId.HasValue)
            {
                query = query.Where(r => r.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la vérification de l'existence de la route {RouteId}", routeId);
            throw;
        }
    }
}
