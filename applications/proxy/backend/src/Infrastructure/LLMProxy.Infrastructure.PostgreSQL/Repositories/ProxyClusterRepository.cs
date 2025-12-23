using LLMProxy.Domain.Entities.Routing;
using LLMProxy.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.PostgreSQL.Repositories;

/// <summary>
/// Implémentation PostgreSQL du repository pour les clusters YARP.
/// </summary>
/// <remarks>
/// <para>
/// Ce repository gère la persistance des clusters du reverse proxy.
/// Un cluster représente un groupe de destinations backend vers lesquelles
/// le trafic peut être dirigé selon une politique de load balancing.
/// </para>
/// <para>
/// <strong>Relations :</strong>
/// <list type="bullet">
/// <item>Un cluster contient une collection de <see cref="ClusterDestination"/></item>
/// <item>Plusieurs routes peuvent référencer le même cluster</item>
/// </list>
/// </para>
/// <para>
/// <strong>Schéma :</strong> Utilise le schéma <c>routing</c> pour isoler les données YARP.
/// </para>
/// </remarks>
public class ProxyClusterRepository : IProxyClusterRepository
{
    private readonly LLMProxyDbContext _context;
    private readonly ILogger<ProxyClusterRepository> _logger;
    private readonly DbSet<ProxyCluster> _dbSet;

    /// <summary>
    /// Initialise une nouvelle instance du repository de clusters.
    /// </summary>
    /// <param name="context">Contexte de base de données EF Core.</param>
    /// <param name="logger">Logger pour les opérations et erreurs.</param>
    public ProxyClusterRepository(LLMProxyDbContext context, ILogger<ProxyClusterRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbSet = context.Set<ProxyCluster>();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProxyCluster>> GetAllActiveWithDestinationsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var clusters = await _dbSet
                .Include(c => c.Destinations.Where(d => d.IsEnabled))
                .Where(c => c.IsEnabled)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return clusters;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des clusters actifs avec destinations");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProxyCluster>> GetByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var clusters = await _dbSet
                .Include(c => c.Destinations)
                .Where(c => c.TenantId == tenantId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return clusters;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des clusters pour le tenant {TenantId}", tenantId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ProxyCluster?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du cluster {ClusterId}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ProxyCluster?> GetByClusterIdAsync(
        string clusterId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClusterId == clusterId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du cluster par ClusterId {ClusterId}", clusterId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ProxyCluster?> GetByIdWithDestinationsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Include(c => c.Destinations)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du cluster {ClusterId} avec destinations", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task AddAsync(
        ProxyCluster cluster,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbSet.AddAsync(cluster, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'ajout du cluster {ClusterId}", cluster.ClusterId);
            throw;
        }
    }

    /// <inheritdoc />
    public Task UpdateAsync(
        ProxyCluster cluster,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _dbSet.Update(cluster);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour du cluster {ClusterId}", cluster.ClusterId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var cluster = await _dbSet
                .Include(c => c.Destinations)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (cluster is not null)
            {
                // Supprimer les destinations associées
                _context.Set<ClusterDestination>().RemoveRange(cluster.Destinations);
                _dbSet.Remove(cluster);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du cluster {ClusterId}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(
        string clusterId,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbSet.Where(c => c.ClusterId == clusterId);

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la vérification de l'existence du cluster {ClusterId}", clusterId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsReferencedByRoutesAsync(
        string clusterId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Set<ProxyRoute>()
                .AnyAsync(r => r.ClusterId == clusterId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la vérification des références au cluster {ClusterId}", clusterId);
            throw;
        }
    }
}
