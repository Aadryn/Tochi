namespace LLMProxy.Domain.Entities.Routing;

/// <summary>
/// Représente une route de proxy inverse YARP persistée en base de données.
/// </summary>
/// <remarks>
/// <para>
/// Une route définit comment les requêtes entrantes sont matchées et routées
/// vers un cluster de destinations. Cette entité permet la configuration
/// dynamique de YARP sans redémarrage de l'application.
/// </para>
/// <para>
/// <b>Exemple de route :</b>
/// <code>
/// var route = new ProxyRoute
/// {
///     Id = Guid.NewGuid(),
///     RouteId = "openai-chat",
///     ClusterId = "openai-cluster",
///     MatchPath = "/v1/chat/completions",
///     MatchMethods = new[] { "POST" },
///     TenantId = tenantId,
///     IsEnabled = true,
///     Order = 1
/// };
/// </code>
/// </para>
/// </remarks>
public class ProxyRoute
{
    /// <summary>
    /// Identifiant unique de l'entité en base de données.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Identifiant de la route utilisé par YARP.
    /// </summary>
    /// <remarks>
    /// Doit être unique dans le système. Utilisé comme clé de référence
    /// dans la configuration YARP.
    /// </remarks>
    /// <example>"openai-chat-route", "ollama-generate-route"</example>
    public string RouteId { get; private set; } = string.Empty;

    /// <summary>
    /// Identifiant du cluster vers lequel router les requêtes.
    /// </summary>
    /// <remarks>
    /// Référence un <see cref="ProxyCluster"/> existant.
    /// </remarks>
    public string ClusterId { get; private set; } = string.Empty;

    /// <summary>
    /// Pattern de chemin URL à matcher.
    /// </summary>
    /// <remarks>
    /// Supporte les wildcards YARP :
    /// <list type="bullet">
    /// <item><c>/api/v1/chat</c> - Chemin exact</item>
    /// <item><c>/api/{version}/chat</c> - Segment variable</item>
    /// <item><c>/api/{**catch-all}</c> - Capture tout le reste</item>
    /// </list>
    /// </remarks>
    /// <example>"/v1/chat/completions", "/ollama/{**catch-all}"</example>
    public string MatchPath { get; private set; } = string.Empty;

    /// <summary>
    /// Méthodes HTTP autorisées pour cette route.
    /// </summary>
    /// <remarks>
    /// Si null ou vide, toutes les méthodes sont autorisées.
    /// </remarks>
    /// <example>["GET", "POST"], ["POST"]</example>
    public string[]? MatchMethods { get; private set; }

    /// <summary>
    /// Headers requis pour matcher cette route.
    /// </summary>
    /// <remarks>
    /// Dictionnaire clé-valeur des headers requis.
    /// Valeurs supportées : valeur exacte, regex, ou liste de valeurs.
    /// </remarks>
    /// <example>{ "X-Api-Version": "2024-01" }</example>
    public Dictionary<string, string[]>? MatchHeaders { get; private set; }

    /// <summary>
    /// Hosts autorisés pour cette route.
    /// </summary>
    /// <remarks>
    /// Permet de router selon le header Host de la requête.
    /// </remarks>
    /// <example>["api.example.com", "*.example.com"]</example>
    public string[]? MatchHosts { get; private set; }

    /// <summary>
    /// Transformations à appliquer sur la requête/réponse.
    /// </summary>
    /// <remarks>
    /// Liste de transformations YARP sous forme de dictionnaires.
    /// <list type="bullet">
    /// <item><c>PathRemovePrefix</c> - Supprime un préfixe du chemin</item>
    /// <item><c>PathPrefix</c> - Ajoute un préfixe au chemin</item>
    /// <item><c>RequestHeader</c> - Ajoute/modifie un header</item>
    /// </list>
    /// </remarks>
    public List<Dictionary<string, string>>? Transforms { get; private set; }

    /// <summary>
    /// Identifiant du tenant propriétaire de cette route.
    /// </summary>
    /// <remarks>
    /// Null pour les routes globales (partagées par tous les tenants).
    /// </remarks>
    public Guid? TenantId { get; private set; }

    /// <summary>
    /// Ordre de priorité de la route.
    /// </summary>
    /// <remarks>
    /// Les routes avec un ordre plus bas sont évaluées en premier.
    /// En cas d'égalité, l'ordre alphabétique du RouteId est utilisé.
    /// </remarks>
    public int Order { get; private set; }

    /// <summary>
    /// Indique si la route est active.
    /// </summary>
    /// <remarks>
    /// Permet de désactiver temporairement une route sans la supprimer.
    /// </remarks>
    public bool IsEnabled { get; private set; } = true;

    /// <summary>
    /// Politique d'autorisation requise pour cette route.
    /// </summary>
    /// <remarks>
    /// Nom de la policy ASP.NET Core Authorization à appliquer.
    /// Null pour aucune autorisation (route publique).
    /// </remarks>
    /// <example>"RequireApiKey", "RequireAdmin"</example>
    public string? AuthorizationPolicy { get; private set; }

    /// <summary>
    /// Politique de rate limiting à appliquer.
    /// </summary>
    /// <remarks>
    /// Nom de la policy de rate limiting configurée.
    /// </remarks>
    /// <example>"PerTenantRateLimit", "GlobalRateLimit"</example>
    public string? RateLimiterPolicy { get; private set; }

    /// <summary>
    /// Timeout spécifique pour cette route.
    /// </summary>
    /// <remarks>
    /// Null pour utiliser le timeout par défaut du cluster.
    /// </remarks>
    public TimeSpan? Timeout { get; private set; }

    /// <summary>
    /// Métadonnées additionnelles de la route.
    /// </summary>
    /// <remarks>
    /// Permet de stocker des informations custom accessibles
    /// dans les middlewares via <c>HttpContext.GetRouteData()</c>.
    /// </remarks>
    public Dictionary<string, string>? Metadata { get; private set; }

    /// <summary>
    /// Date de création de la route.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Date de dernière modification.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Constructeur protégé pour Entity Framework.
    /// </summary>
    protected ProxyRoute() { }

    /// <summary>
    /// Crée une nouvelle route de proxy.
    /// </summary>
    /// <param name="routeId">Identifiant unique de la route.</param>
    /// <param name="clusterId">Identifiant du cluster cible.</param>
    /// <param name="matchPath">Pattern de chemin à matcher.</param>
    /// <param name="tenantId">Identifiant du tenant (optionnel).</param>
    /// <returns>Nouvelle instance de <see cref="ProxyRoute"/>.</returns>
    /// <exception cref="ArgumentException">Si routeId, clusterId ou matchPath est vide.</exception>
    public static ProxyRoute Create(
        string routeId,
        string clusterId,
        string matchPath,
        Guid? tenantId = null)
    {
        if (string.IsNullOrWhiteSpace(routeId))
            throw new ArgumentException("L'identifiant de route est requis.", nameof(routeId));

        if (string.IsNullOrWhiteSpace(clusterId))
            throw new ArgumentException("L'identifiant de cluster est requis.", nameof(clusterId));

        if (string.IsNullOrWhiteSpace(matchPath))
            throw new ArgumentException("Le chemin de matching est requis.", nameof(matchPath));

        return new ProxyRoute
        {
            Id = Guid.NewGuid(),
            RouteId = routeId,
            ClusterId = clusterId,
            MatchPath = matchPath,
            TenantId = tenantId,
            Order = 0,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Définit les méthodes HTTP autorisées.
    /// </summary>
    /// <param name="methods">Liste des méthodes (GET, POST, etc.).</param>
    /// <returns>L'instance courante pour le chaînage.</returns>
    public ProxyRoute WithMethods(params string[] methods)
    {
        MatchMethods = methods;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    /// <summary>
    /// Définit les transformations de la route.
    /// </summary>
    /// <param name="transforms">Liste des transformations.</param>
    /// <returns>L'instance courante pour le chaînage.</returns>
    public ProxyRoute WithTransforms(List<Dictionary<string, string>> transforms)
    {
        Transforms = transforms;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    /// <summary>
    /// Ajoute une transformation de suppression de préfixe.
    /// </summary>
    /// <param name="prefix">Préfixe à supprimer.</param>
    /// <returns>L'instance courante pour le chaînage.</returns>
    public ProxyRoute WithPathRemovePrefix(string prefix)
    {
        Transforms ??= new List<Dictionary<string, string>>();
        Transforms.Add(new Dictionary<string, string> { ["PathRemovePrefix"] = prefix });
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    /// <summary>
    /// Définit l'ordre de priorité.
    /// </summary>
    /// <param name="order">Ordre (plus bas = plus prioritaire).</param>
    /// <returns>L'instance courante pour le chaînage.</returns>
    public ProxyRoute WithOrder(int order)
    {
        Order = order;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    /// <summary>
    /// Définit la politique d'autorisation.
    /// </summary>
    /// <param name="policy">Nom de la policy.</param>
    /// <returns>L'instance courante pour le chaînage.</returns>
    public ProxyRoute WithAuthorizationPolicy(string policy)
    {
        AuthorizationPolicy = policy;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    /// <summary>
    /// Définit la politique de rate limiting.
    /// </summary>
    /// <param name="policy">Nom de la policy.</param>
    /// <returns>L'instance courante pour le chaînage.</returns>
    public ProxyRoute WithRateLimiterPolicy(string policy)
    {
        RateLimiterPolicy = policy;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    /// <summary>
    /// Active ou désactive la route.
    /// </summary>
    /// <param name="enabled">True pour activer, false pour désactiver.</param>
    /// <returns>L'instance courante pour le chaînage.</returns>
    public ProxyRoute SetEnabled(bool enabled)
    {
        IsEnabled = enabled;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    /// <summary>
    /// Ajoute des métadonnées à la route.
    /// </summary>
    /// <param name="key">Clé de la métadonnée.</param>
    /// <param name="value">Valeur de la métadonnée.</param>
    /// <returns>L'instance courante pour le chaînage.</returns>
    public ProxyRoute WithMetadata(string key, string value)
    {
        Metadata ??= new Dictionary<string, string>();
        Metadata[key] = value;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }
}
