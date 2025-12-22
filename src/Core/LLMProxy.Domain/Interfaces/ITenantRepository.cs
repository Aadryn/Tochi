using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;

namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Repository pour l'agrégat Tenant (Port dans l'architecture hexagonale).
/// </summary>
/// <remarks>
/// <para>
/// Ce repository gère la persistance des tenants avec support du Result Pattern (ADR-023).
/// Toutes les méthodes retournent <see cref="Result{T}"/> pour une gestion d'erreurs explicite.
/// </para>
/// <para>
/// <strong>Méthodes spécifiques métier :</strong>
/// Hérite de <see cref="IRepository{Tenant}"/> et ajoute des méthodes spécifiques au domaine Tenant
/// (recherche par slug, vérification d'unicité, etc.).
/// </para>
/// </remarks>
public interface ITenantRepository : IRepository<Tenant>
{
    /// <summary>
    /// Récupère un tenant par son slug unique.
    /// </summary>
    /// <param name="slug">Slug du tenant (identifiant URL-safe).</param>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération asynchrone.</param>
    /// <returns>
    /// <see cref="Result{T}"/> contenant le tenant si trouvé.
    /// Retourne <c>Error.Tenant.NotFound</c> si aucun tenant ne correspond.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await repository.GetBySlugAsync("acme-corp");
    /// if (result.IsSuccess)
    /// {
    ///     var tenant = result.Value;
    ///     // Utiliser le tenant
    /// }
    /// </code>
    /// </example>
    Task<Result<Tenant>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère tous les tenants avec filtrage optionnel par statut actif/inactif.
    /// </summary>
    /// <param name="includeInactive">Si <c>true</c>, inclut les tenants inactifs. Par défaut <c>false</c>.</param>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération asynchrone.</param>
    /// <returns>
    /// <see cref="Result{T}"/> contenant la collection de tenants filtrés.
    /// </returns>
    /// <example>
    /// <code>
    /// // Récupérer uniquement les tenants actifs
    /// var activeTenantsResult = await repository.GetAllAsync(includeInactive: false);
    /// 
    /// // Récupérer tous les tenants
    /// var allTenantsResult = await repository.GetAllAsync(includeInactive: true);
    /// </code>
    /// </example>
    Task<Result<IReadOnlyList<Tenant>>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Vérifie si un slug est déjà utilisé par un tenant existant.
    /// </summary>
    /// <param name="slug">Slug à vérifier.</param>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération asynchrone.</param>
    /// <returns>
    /// <see cref="Result{T}"/> contenant <c>true</c> si le slug existe, <c>false</c> sinon.
    /// </returns>
    /// <remarks>
    /// Utilisé pour valider l'unicité du slug avant la création ou mise à jour d'un tenant.
    /// </remarks>
    /// <example>
    /// <code>
    /// var slugExistsResult = await repository.SlugExistsAsync("acme-corp");
    /// 
    /// if (slugExistsResult.IsSuccess &amp;&amp; slugExistsResult.Value)
    /// {
    ///     return Error.Tenant.SlugAlreadyExists("acme-corp");
    /// }
    /// </code>
    /// </example>
    Task<Result<bool>> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
}
