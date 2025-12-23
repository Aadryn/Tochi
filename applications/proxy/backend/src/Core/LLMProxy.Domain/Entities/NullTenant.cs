namespace LLMProxy.Domain.Entities;

/// <summary>
/// Null Object représentant un tenant par défaut pour les utilisateurs non assignés.
/// Conforme à ADR-026 (Null Object Pattern).
/// </summary>
/// <remarks>
/// Ce tenant fictif retourne toujours des résultats d'échec pour éviter les NullReferenceException.
/// Utilisé lorsque aucun tenant réel n'est trouvé au lieu de retourner <c>null</c>.
/// Pattern Singleton : une seule instance partagée via <see cref="Instance"/>.
/// </remarks>
public sealed class NullTenant : Tenant
{
    /// <summary>
    /// Instance singleton du tenant par défaut.
    /// </summary>
    /// <remarks>
    /// Utiliser cette instance au lieu de créer de nouvelles instances.
    /// Économise la mémoire et garantit un comportement cohérent.
    /// </remarks>
    public static NullTenant Instance { get; } = new NullTenant();

    /// <summary>
    /// Constructeur privé pour empêcher l'instanciation externe.
    /// </summary>
    /// <remarks>
    /// Conforme au pattern Singleton : seule l'instance statique <see cref="Instance"/> peut être utilisée.
    /// </remarks>
    private NullTenant()
        : base(
            id: Guid.Empty,
            name: "Default Tenant",
            slug: "default")
    {
    }

    /// <summary>
    /// Le tenant par défaut est toujours inactif.
    /// </summary>
    /// <remarks>
    /// Permet de détecter facilement un tenant fictif dans le code appelant.
    /// Les validations peuvent vérifier <c>!tenant.IsActive</c> au lieu de <c>tenant == null</c>.
    /// </remarks>
    public override bool IsActive => false;
}
