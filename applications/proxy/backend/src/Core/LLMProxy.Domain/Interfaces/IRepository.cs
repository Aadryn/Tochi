using LLMProxy.Domain.Common;

namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Interface générique de base pour tous les repositories.
/// Définit les opérations CRUD communes avec gestion des erreurs via Result Pattern (ADR-023).
/// </summary>
/// <typeparam name="TEntity">Type de l'entité gérée (doit hériter de <see cref="Entity"/>).</typeparam>
/// <remarks>
/// <para>
/// Cette interface générique sert de contrat de base pour tous les repositories spécifiques.
/// Elle garantit une API cohérente pour les opérations CRUD standard.
/// </para>
/// <para>
/// <strong>Utilisation du Result Pattern :</strong>
/// Toutes les méthodes retournent <see cref="Result{T}"/> au lieu de lever des exceptions,
/// conformément à ADR-023. Cela permet une gestion d'erreurs explicite et testable.
/// </para>
/// <para>
/// <strong>Repositories spécifiques :</strong>
/// Les repositories métier (ITenantRepository, IUserRepository, etc.) doivent hériter de cette interface
/// et ajouter leurs méthodes spécifiques (ex: GetByEmailAsync, GetBySlugAsync).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Repository spécifique héritant de IRepository
/// public interface ITenantRepository : IRepository&lt;Tenant&gt;
/// {
///     Task&lt;Result&lt;Tenant&gt;&gt; GetBySlugAsync(string slug, CancellationToken ct = default);
///     Task&lt;Result&lt;bool&gt;&gt; SlugExistsAsync(string slug, CancellationToken ct = default);
/// }
/// </code>
/// </example>
public interface IRepository<TEntity> where TEntity : Entity
{
    /// <summary>
    /// Récupère une entité par son identifiant unique.
    /// </summary>
    /// <param name="id">Identifiant unique de l'entité (GUID).</param>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération asynchrone.</param>
    /// <returns>
    /// <see cref="Result{T}"/> contenant l'entité si trouvée.
    /// En cas d'échec, retourne une erreur appropriée :
    /// <list type="bullet">
    /// <item><description><c>Error.NotFound</c> : L'entité n'existe pas</description></item>
    /// <item><description><c>Error.Database</c> : Erreur d'accès à la base de données</description></item>
    /// </list>
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await repository.GetByIdAsync(tenantId);
    /// 
    /// if (result.IsSuccess)
    /// {
    ///     var tenant = result.Value;
    ///     // Traiter l'entité
    /// }
    /// else
    /// {
    ///     _logger.LogWarning("Tenant non trouvé: {Error}", result.Error.Message);
    /// }
    /// </code>
    /// </example>
    Task<Result<TEntity>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère toutes les entités du repository.
    /// </summary>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération asynchrone.</param>
    /// <returns>
    /// <see cref="Result{T}"/> contenant la collection d'entités (peut être vide).
    /// En cas d'échec, retourne une erreur <c>Error.Database</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>⚠️ Performance :</strong> Cette méthode charge TOUTES les entités en mémoire.
    /// Pour de grandes collections, préférer des méthodes avec pagination ou filtrage.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = await repository.GetAllAsync();
    /// 
    /// if (result.IsSuccess)
    /// {
    ///     var entities = result.Value; // IReadOnlyList&lt;TEntity&gt;
    ///     foreach (var entity in entities)
    ///     {
    ///         // Traiter chaque entité
    ///     }
    /// }
    /// </code>
    /// </example>
    Task<Result<IReadOnlyList<TEntity>>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Vérifie si une entité existe par son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique de l'entité.</param>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération asynchrone.</param>
    /// <returns>
    /// <see cref="Result{T}"/> contenant <c>true</c> si l'entité existe, <c>false</c> sinon.
    /// En cas d'échec, retourne une erreur <c>Error.Database</c>.
    /// </returns>
    /// <remarks>
    /// Cette méthode est optimisée pour vérifier l'existence sans charger l'entité complète.
    /// </remarks>
    /// <example>
    /// <code>
    /// var existsResult = await repository.ExistsAsync(tenantId);
    /// 
    /// if (existsResult.IsSuccess &amp;&amp; existsResult.Value)
    /// {
    ///     // L'entité existe
    /// }
    /// else if (existsResult.IsSuccess &amp;&amp; !existsResult.Value)
    /// {
    ///     // L'entité n'existe pas
    /// }
    /// else
    /// {
    ///     // Erreur d'accès à la base de données
    ///     _logger.LogError("Erreur lors de la vérification: {Error}", existsResult.Error.Message);
    /// }
    /// </code>
    /// </example>
    Task<Result<bool>> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ajoute une nouvelle entité au repository.
    /// </summary>
    /// <param name="entity">Entité à ajouter (ne doit pas déjà exister).</param>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération asynchrone.</param>
    /// <returns>
    /// <see cref="Result"/> indiquant le succès ou l'échec de l'opération.
    /// En cas d'échec, retourne une erreur appropriée :
    /// <list type="bullet">
    /// <item><description><c>Error.AlreadyExists</c> : Une entité avec cet ID existe déjà</description></item>
    /// <item><description><c>Error.Validation</c> : L'entité est invalide</description></item>
    /// <item><description><c>Error.Database</c> : Erreur d'accès à la base de données</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>⚠️ Important :</strong> Cette méthode n'appelle PAS automatiquement SaveChangesAsync().
    /// Il faut utiliser IUnitOfWork.SaveChangesAsync() pour persister les changements.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var tenant = Tenant.Create("Acme Corp", "admin@acme.com");
    /// var addResult = await repository.AddAsync(tenant);
    /// 
    /// if (addResult.IsSuccess)
    /// {
    ///     await unitOfWork.SaveChangesAsync(); // Persister les changements
    /// }
    /// else
    /// {
    ///     _logger.LogError("Échec ajout tenant: {Error}", addResult.Error.Message);
    /// }
    /// </code>
    /// </example>
    Task<Result> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Met à jour une entité existante dans le repository.
    /// </summary>
    /// <param name="entity">Entité à mettre à jour (doit exister).</param>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération asynchrone.</param>
    /// <returns>
    /// <see cref="Result"/> indiquant le succès ou l'échec de l'opération.
    /// En cas d'échec, retourne une erreur appropriée :
    /// <list type="bullet">
    /// <item><description><c>Error.NotFound</c> : L'entité n'existe pas</description></item>
    /// <item><description><c>Error.Concurrency</c> : Conflit de concurrence (version obsolète)</description></item>
    /// <item><description><c>Error.Database</c> : Erreur d'accès à la base de données</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>⚠️ Important :</strong> Cette méthode n'appelle PAS automatiquement SaveChangesAsync().
    /// Il faut utiliser IUnitOfWork.SaveChangesAsync() pour persister les changements.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var tenantResult = await repository.GetByIdAsync(tenantId);
    /// if (tenantResult.IsFailure) return tenantResult.Error;
    /// 
    /// var tenant = tenantResult.Value;
    /// tenant.UpdateSettings(newSettings);
    /// 
    /// var updateResult = await repository.UpdateAsync(tenant);
    /// if (updateResult.IsSuccess)
    /// {
    ///     await unitOfWork.SaveChangesAsync(); // Persister les changements
    /// }
    /// </code>
    /// </example>
    Task<Result> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Supprime une entité par son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique de l'entité à supprimer.</param>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération asynchrone.</param>
    /// <returns>
    /// <see cref="Result"/> indiquant le succès ou l'échec de l'opération.
    /// En cas d'échec, retourne une erreur appropriée :
    /// <list type="bullet">
    /// <item><description><c>Error.NotFound</c> : L'entité n'existe pas</description></item>
    /// <item><description><c>Error.Constraint</c> : Violation de contrainte (clé étrangère)</description></item>
    /// <item><description><c>Error.Database</c> : Erreur d'accès à la base de données</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>⚠️ Important :</strong> Cette méthode n'appelle PAS automatiquement SaveChangesAsync().
    /// Il faut utiliser IUnitOfWork.SaveChangesAsync() pour persister les changements.
    /// </para>
    /// <para>
    /// <strong>Soft Delete vs Hard Delete :</strong>
    /// Si l'entité supporte le Soft Delete (propriété IsDeleted), préférer marquer l'entité
    /// comme supprimée plutôt que de la supprimer physiquement.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var deleteResult = await repository.DeleteAsync(tenantId);
    /// 
    /// if (deleteResult.IsSuccess)
    /// {
    ///     await unitOfWork.SaveChangesAsync(); // Persister la suppression
    /// }
    /// else if (deleteResult.Error.Code == "Constraint.ForeignKey")
    /// {
    ///     _logger.LogWarning("Impossible de supprimer: entité référencée");
    /// }
    /// </code>
    /// </example>
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
