using LLMProxy.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace LLMProxy.Infrastructure.PostgreSQL.Repositories;

/// <summary>
/// Classe de base abstraite pour les repositories.
/// Fournit les opérations CRUD communes pour éviter la duplication de code.
/// </summary>
/// <typeparam name="TEntity">Type de l'entité gérée par le repository (doit hériter de Entity).</typeparam>
public abstract class RepositoryBase<TEntity> where TEntity : Entity
{
    /// <summary>
    /// Contexte de base de données EF Core.
    /// </summary>
    protected readonly LLMProxyDbContext _context;

    /// <summary>
    /// DbSet pour l'entité gérée.
    /// </summary>
    protected readonly DbSet<TEntity> _dbSet;

    /// <summary>
    /// Initialise une nouvelle instance de la classe <see cref="RepositoryBase{TEntity}"/>.
    /// </summary>
    /// <param name="context">Contexte de base de données.</param>
    protected RepositoryBase(LLMProxyDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<TEntity>();
    }

    /// <summary>
    /// Récupère une entité par son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique de l'entité.</param>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération asynchrone.</param>
    /// <returns>L'entité si trouvée, sinon null.</returns>
    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync([id], cancellationToken);
    }

    /// <summary>
    /// Vérifie si une entité existe par son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique de l'entité.</param>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération asynchrone.</param>
    /// <returns>True si l'entité existe, sinon false.</returns>
    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(e => e.Id == id, cancellationToken);
    }

    /// <summary>
    /// Ajoute une nouvelle entité au contexte.
    /// </summary>
    /// <param name="entity">Entité à ajouter.</param>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération asynchrone.</param>
    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Met à jour une entité existante dans le contexte.
    /// </summary>
    /// <param name="entity">Entité à mettre à jour.</param>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération asynchrone.</param>
    public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Supprime une entité par son identifiant.
    /// Récupère d'abord l'entité puis la supprime si elle existe.
    /// </summary>
    /// <param name="id">Identifiant unique de l'entité à supprimer.</param>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération asynchrone.</param>
    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            _dbSet.Remove(entity);
        }
    }
}
