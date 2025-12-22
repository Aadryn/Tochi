using LLMProxy.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.PostgreSQL.Repositories;

/// <summary>
/// Classe de base abstraite pour les repositories avec support du Result Pattern (ADR-023).
/// Fournit les opérations CRUD communes avec gestion des erreurs explicite.
/// </summary>
/// <typeparam name="TEntity">Type de l'entité gérée par le repository (doit hériter de <see cref="Entity"/>).</typeparam>
/// <remarks>
/// <para>
/// Cette classe implémente les opérations CRUD standard avec gestion des erreurs via <see cref="Result{T}"/>,
/// conformément à ADR-023 Result Pattern. Toutes les exceptions sont capturées et converties en erreurs typées.
/// </para>
/// <para>
/// <strong>Gestion des transactions :</strong>
/// Les méthodes Add/Update/Delete ne persistent PAS automatiquement les changements.
/// Il faut appeler <see cref="IUnitOfWork.SaveChangesAsync"/> pour valider les modifications.
/// </para>
/// <para>
/// <strong>Logging :</strong>
/// Toutes les erreurs sont loggées avec contexte complet pour faciliter le diagnostic.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Repository spécifique héritant de RepositoryBase
/// public class TenantRepository : RepositoryBase&lt;Tenant&gt;, ITenantRepository
/// {
///     public TenantRepository(LLMProxyDbContext context, ILogger&lt;TenantRepository&gt; logger) 
///         : base(context, logger) { }
///     
///     // Méthodes spécifiques à Tenant
///     public async Task&lt;Result&lt;Tenant&gt;&gt; GetBySlugAsync(string slug, CancellationToken ct)
///     {
///         try
///         {
///             var tenant = await DbSet.FirstOrDefaultAsync(t => t.Slug == slug, ct);
///             return tenant is null 
///                 ? Error.Tenant.NotFound(Guid.Empty)
///                 : Result&lt;Tenant&gt;.Success(tenant);
///         }
///         catch (Exception ex)
///         {
///             Logger.LogError(ex, "Erreur lors de la recherche du tenant par slug {Slug}", slug);
///             return Error.Database.AccessError("GetBySlugAsync", ex.Message);
///         }
///     }
/// }
/// </code>
/// </example>
public abstract class RepositoryBase<TEntity> where TEntity : Entity
{
    /// <summary>
    /// Contexte de base de données EF Core.
    /// </summary>
    protected readonly LLMProxyDbContext Context;

    /// <summary>
    /// DbSet pour l'entité gérée.
    /// </summary>
    protected readonly DbSet<TEntity> DbSet;

    /// <summary>
    /// Logger pour enregistrer les erreurs et événements.
    /// </summary>
    protected readonly ILogger Logger;

    /// <summary>
    /// Nom du type d'entité (utilisé pour les messages d'erreur).
    /// </summary>
    protected readonly string EntityTypeName;

    /// <summary>
    /// Initialise une nouvelle instance de la classe <see cref="RepositoryBase{TEntity}"/>.
    /// </summary>
    /// <param name="context">Contexte de base de données EF Core.</param>
    /// <param name="logger">Logger pour enregistrer les opérations et erreurs.</param>
    /// <exception cref="ArgumentNullException">Si context ou logger est null.</exception>
    protected RepositoryBase(LLMProxyDbContext context, ILogger logger)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        DbSet = context.Set<TEntity>();
        EntityTypeName = typeof(TEntity).Name;
    }

    /// <summary>
    /// Récupère une entité par son identifiant unique.
    /// </summary>
    /// <param name="id">Identifiant unique de l'entité (GUID).</param>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération asynchrone.</param>
    /// <returns>
    /// <see cref="Result{T}"/> contenant l'entité si trouvée, ou une erreur.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Utilise <c>FindAsync</c> qui recherche d'abord en cache puis en base de données.
    /// </para>
    /// <para>
    /// <strong>Erreurs possibles :</strong>
    /// <list type="bullet">
    /// <item><description><c>EntityNotFound</c> : Aucune entité avec cet ID</description></item>
    /// <item><description><c>Database.AccessError</c> : Erreur d'accès à la base de données</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public virtual async Task<Result<TEntity>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await DbSet.FindAsync([id], cancellationToken);
            
            if (entity is null)
            {
                Logger.LogWarning("{EntityType} avec ID {Id} non trouvé", EntityTypeName, id);
                return Error.Database.EntityNotFound(EntityTypeName, id);
            }

            return Result<TEntity>.Success(entity);
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation("Opération GetByIdAsync annulée pour {EntityType} {Id}", EntityTypeName, id);
            throw; // Laisser passer les annulations
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erreur lors de la récupération de {EntityType} avec ID {Id}", EntityTypeName, id);
            return Error.Database.AccessError($"GetByIdAsync({EntityTypeName})", ex.Message);
        }
    }

    /// <summary>
    /// Récupère toutes les entités du repository.
    /// </summary>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération asynchrone.</param>
    /// <returns>
    /// <see cref="Result{T}"/> contenant la collection d'entités (peut être vide).
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>⚠️ Performance :</strong> Cette méthode charge TOUTES les entités en mémoire.
    /// Pour de grandes tables, préférer des méthodes avec pagination ou filtrage.
    /// </para>
    /// </remarks>
    public virtual async Task<Result<IReadOnlyList<TEntity>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await DbSet.ToListAsync(cancellationToken);
            
            Logger.LogDebug("Récupéré {Count} entités {EntityType}", entities.Count, EntityTypeName);
            
            return Result<IReadOnlyList<TEntity>>.Success(entities.AsReadOnly());
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation("Opération GetAllAsync annulée pour {EntityType}", EntityTypeName);
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erreur lors de la récupération de toutes les entités {EntityType}", EntityTypeName);
            return Error.Database.AccessError($"GetAllAsync({EntityTypeName})", ex.Message);
        }
    }

    /// <summary>
    /// Vérifie si une entité existe par son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique de l'entité.</param>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération asynchrone.</param>
    /// <returns>
    /// <see cref="Result{T}"/> contenant <c>true</c> si l'entité existe, <c>false</c> sinon.
    /// </returns>
    /// <remarks>
    /// Cette méthode utilise <c>AnyAsync</c> qui est optimisée pour vérifier l'existence
    /// sans charger l'entité complète.
    /// </remarks>
    public virtual async Task<Result<bool>> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await DbSet.AnyAsync(e => e.Id == id, cancellationToken);
            
            Logger.LogDebug("{EntityType} avec ID {Id} existe: {Exists}", EntityTypeName, id, exists);
            
            return Result<bool>.Success(exists);
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation("Opération ExistsAsync annulée pour {EntityType} {Id}", EntityTypeName, id);
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erreur lors de la vérification d'existence de {EntityType} avec ID {Id}", EntityTypeName, id);
            return Error.Database.AccessError($"ExistsAsync({EntityTypeName})", ex.Message);
        }
    }

    /// <summary>
    /// Ajoute une nouvelle entité au repository.
    /// </summary>
    /// <param name="entity">Entité à ajouter.</param>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération asynchrone.</param>
    /// <returns>
    /// <see cref="Result"/> indiquant le succès ou l'échec.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>⚠️ Important :</strong> Cette méthode n'appelle PAS SaveChangesAsync().
    /// Les changements doivent être persistés explicitement via IUnitOfWork.
    /// </para>
    /// <para>
    /// <strong>Validation :</strong> Vérifie que l'entité n'existe pas déjà (par ID).
    /// </para>
    /// </remarks>
    public virtual async Task<Result> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
        {
            Logger.LogWarning("Tentative d'ajout d'une entité {EntityType} null", EntityTypeName);
            return Error.Validation.Required(EntityTypeName);
        }

        try
        {
            // Vérifier si l'entité existe déjà
            var existsResult = await ExistsAsync(entity.Id, cancellationToken);
            if (existsResult.IsFailure)
            {
                return existsResult.Error;
            }

            if (existsResult.Value)
            {
                Logger.LogWarning("Tentative d'ajout d'une entité {EntityType} existante (ID: {Id})", EntityTypeName, entity.Id);
                return Error.Database.EntityAlreadyExists(EntityTypeName, entity.Id);
            }

            await DbSet.AddAsync(entity, cancellationToken);
            
            Logger.LogInformation("Entité {EntityType} ajoutée (ID: {Id}) - En attente de SaveChanges", EntityTypeName, entity.Id);
            
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation("Opération AddAsync annulée pour {EntityType}", EntityTypeName);
            throw;
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError(ex, "Erreur de mise à jour lors de l'ajout de {EntityType} (ID: {Id})", EntityTypeName, entity.Id);
            
            // Détection des violations de contraintes spécifiques
            if (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true)
            {
                return Error.Database.UniqueConstraintViolation(ex.InnerException.Message);
            }
            
            return Error.Database.AccessError($"AddAsync({EntityTypeName})", ex.Message);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erreur lors de l'ajout de {EntityType} (ID: {Id})", EntityTypeName, entity.Id);
            return Error.Database.AccessError($"AddAsync({EntityTypeName})", ex.Message);
        }
    }

    /// <summary>
    /// Met à jour une entité existante dans le repository.
    /// </summary>
    /// <param name="entity">Entité à mettre à jour.</param>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération asynchrone.</param>
    /// <returns>
    /// <see cref="Result"/> indiquant le succès ou l'échec.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>⚠️ Important :</strong> Cette méthode n'appelle PAS SaveChangesAsync().
    /// Les changements doivent être persistés explicitement via IUnitOfWork.
    /// </para>
    /// <para>
    /// <strong>Validation :</strong> Vérifie que l'entité existe avant de la mettre à jour.
    /// </para>
    /// </remarks>
    public virtual async Task<Result> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
        {
            Logger.LogWarning("Tentative de mise à jour d'une entité {EntityType} null", EntityTypeName);
            return Error.Validation.Required(EntityTypeName);
        }

        try
        {
            // Vérifier si l'entité existe
            var existsResult = await ExistsAsync(entity.Id, cancellationToken);
            if (existsResult.IsFailure)
            {
                return existsResult.Error;
            }

            if (!existsResult.Value)
            {
                Logger.LogWarning("Tentative de mise à jour d'une entité {EntityType} inexistante (ID: {Id})", EntityTypeName, entity.Id);
                return Error.Database.EntityNotFound(EntityTypeName, entity.Id);
            }

            DbSet.Update(entity);
            
            Logger.LogInformation("Entité {EntityType} mise à jour (ID: {Id}) - En attente de SaveChanges", EntityTypeName, entity.Id);
            
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation("Opération UpdateAsync annulée pour {EntityType} {Id}", EntityTypeName, entity.Id);
            throw;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Logger.LogWarning(ex, "Conflit de concurrence lors de la mise à jour de {EntityType} (ID: {Id})", EntityTypeName, entity.Id);
            return Error.Database.ConcurrencyConflict(EntityTypeName);
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError(ex, "Erreur de mise à jour lors de l'update de {EntityType} (ID: {Id})", EntityTypeName, entity.Id);
            return Error.Database.AccessError($"UpdateAsync({EntityTypeName})", ex.Message);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erreur lors de la mise à jour de {EntityType} (ID: {Id})", EntityTypeName, entity.Id);
            return Error.Database.AccessError($"UpdateAsync({EntityTypeName})", ex.Message);
        }
    }

    /// <summary>
    /// Supprime une entité par son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique de l'entité à supprimer.</param>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération asynchrone.</param>
    /// <returns>
    /// <see cref="Result"/> indiquant le succès ou l'échec.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>⚠️ Important :</strong> Cette méthode n'appelle PAS SaveChangesAsync().
    /// Les changements doivent être persistés explicitement via IUnitOfWork.
    /// </para>
    /// <para>
    /// <strong>Hard Delete :</strong> Cette méthode effectue une suppression physique.
    /// Pour un Soft Delete, utiliser UpdateAsync avec une propriété IsDeleted.
    /// </para>
    /// </remarks>
    public virtual async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entityResult = await GetByIdAsync(id, cancellationToken);
            if (entityResult.IsFailure)
            {
                return entityResult.Error;
            }

            DbSet.Remove(entityResult.Value);
            
            Logger.LogInformation("Entité {EntityType} supprimée (ID: {Id}) - En attente de SaveChanges", EntityTypeName, id);
            
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation("Opération DeleteAsync annulée pour {EntityType} {Id}", EntityTypeName, id);
            throw;
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError(ex, "Erreur lors de la suppression de {EntityType} (ID: {Id})", EntityTypeName, id);
            
            // Détection des violations de clés étrangères
            if (ex.InnerException?.Message.Contains("foreign key", StringComparison.OrdinalIgnoreCase) == true ||
                ex.InnerException?.Message.Contains("reference", StringComparison.OrdinalIgnoreCase) == true)
            {
                return Error.Database.ForeignKeyViolation(ex.InnerException.Message);
            }
            
            return Error.Database.AccessError($"DeleteAsync({EntityTypeName})", ex.Message);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erreur lors de la suppression de {EntityType} (ID: {Id})", EntityTypeName, id);
            return Error.Database.AccessError($"DeleteAsync({EntityTypeName})", ex.Message);
        }
    }
}
