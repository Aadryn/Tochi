namespace LLMProxy.Infrastructure.Authorization.Abstractions;

/// <summary>
/// Interface du service d'autorisation ReBAC (Relationship-Based Access Control).
/// </summary>
/// <remarks>
/// <para>
/// Ce service implémente le pattern ReBAC basé sur OpenFGA, où les autorisations
/// sont déterminées par les relations entre utilisateurs et objets.
/// </para>
/// <para>
/// Voir ADR-055 pour les détails de l'architecture d'autorisation.
/// </para>
/// </remarks>
public interface IAuthorizationService
{
    /// <summary>
    /// Vérifie si un utilisateur a une relation spécifique avec un objet.
    /// </summary>
    /// <param name="request">Requête d'autorisation contenant utilisateur, relation et objet.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Résultat de la vérification d'autorisation.</returns>
    /// <example>
    /// <code>
    /// var request = new AuthorizationRequest(
    ///     userId: "user@example.com",
    ///     relation: "can_view",
    ///     objectType: "tenant",
    ///     objectId: "tenant-123"
    /// );
    /// var result = await authService.CheckAsync(request, cancellationToken);
    /// if (result.IsAllowed)
    /// {
    ///     // L'utilisateur peut accéder au tenant
    /// }
    /// </code>
    /// </example>
    Task<AuthorizationResult> CheckAsync(AuthorizationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Vérifie si un utilisateur a une relation spécifique avec un objet.
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    /// <param name="relation">Relation à vérifier.</param>
    /// <param name="objectType">Type de l'objet.</param>
    /// <param name="objectId">Identifiant de l'objet.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Résultat de la vérification d'autorisation.</returns>
    Task<AuthorizationResult> CheckAsync(
        string userId,
        string relation,
        string objectType,
        string objectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ajoute une relation entre un utilisateur et un objet.
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    /// <param name="relation">Relation à créer.</param>
    /// <param name="objectType">Type de l'objet.</param>
    /// <param name="objectId">Identifiant de l'objet.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Tâche représentant l'opération asynchrone.</returns>
    /// <example>
    /// <code>
    /// // Accorder le rôle d'opérateur sur un tenant
    /// await authService.AddRelationAsync(
    ///     "user@example.com",
    ///     "operator",
    ///     "tenant",
    ///     "tenant-123",
    ///     cancellationToken
    /// );
    /// </code>
    /// </example>
    Task AddRelationAsync(
        string userId,
        string relation,
        string objectType,
        string objectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Supprime une relation entre un utilisateur et un objet.
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    /// <param name="relation">Relation à supprimer.</param>
    /// <param name="objectType">Type de l'objet.</param>
    /// <param name="objectId">Identifiant de l'objet.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Tâche représentant l'opération asynchrone.</returns>
    Task RemoveRelationAsync(
        string userId,
        string relation,
        string objectType,
        string objectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Liste tous les objets d'un type donné auxquels un utilisateur a accès via une relation.
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    /// <param name="relation">Relation à rechercher.</param>
    /// <param name="objectType">Type d'objets à lister.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des identifiants d'objets accessibles.</returns>
    /// <example>
    /// <code>
    /// // Lister tous les tenants que l'utilisateur peut voir
    /// var tenantIds = await authService.ListObjectsAsync(
    ///     "user@example.com",
    ///     "can_view",
    ///     "tenant",
    ///     cancellationToken
    /// );
    /// </code>
    /// </example>
    Task<IReadOnlyList<string>> ListObjectsAsync(
        string userId,
        string relation,
        string objectType,
        CancellationToken cancellationToken = default);
}
