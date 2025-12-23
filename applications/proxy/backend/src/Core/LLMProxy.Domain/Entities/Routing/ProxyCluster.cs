namespace LLMProxy.Domain.Entities.Routing;

/// <summary>
/// Représente un cluster YARP (groupe de destinations backend).
/// </summary>
/// <remarks>
/// <para>
/// Un cluster regroupe une ou plusieurs destinations vers lesquelles
/// les requêtes peuvent être routées. YARP utilise le load balancing
/// pour distribuer le trafic entre les destinations actives.
/// </para>
/// <para>
/// <b>Exemple de cluster :</b>
/// <code>
/// var cluster = ProxyCluster.Create("openai-cluster", tenantId)
///     .WithLoadBalancingPolicy("RoundRobin")
///     .WithHealthCheck(TimeSpan.FromSeconds(30), "/health");
/// 
/// cluster.AddDestination("primary", "https://api.openai.com");
/// cluster.AddDestination("backup", "https://api.openai.com", health: "https://api.openai.com/v1/models");
/// </code>
/// </para>
/// </remarks>
public class ProxyCluster
{
    /// <summary>
    /// Identifiant unique de l'entité en base de données.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Identifiant du cluster utilisé par YARP.
    /// </summary>
    /// <remarks>
    /// Doit être unique. Référencé par les routes via <see cref="ProxyRoute.ClusterId"/>.
    /// </remarks>
    /// <example>"openai-cluster", "ollama-local-cluster"</example>
    public string ClusterId { get; private set; } = string.Empty;

    /// <summary>
    /// Identifiant du tenant propriétaire de ce cluster.
    /// </summary>
    /// <remarks>
    /// Null pour les clusters globaux partagés.
    /// </remarks>
    public Guid? TenantId { get; private set; }

    /// <summary>
    /// Politique de load balancing.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><c>RoundRobin</c> - Distribution circulaire (par défaut)</item>
    /// <item><c>Random</c> - Distribution aléatoire</item>
    /// <item><c>LeastRequests</c> - Vers la destination la moins chargée</item>
    /// <item><c>PowerOfTwoChoices</c> - Choix parmi 2 destinations aléatoires</item>
    /// <item><c>FirstAlphabetical</c> - Toujours la première destination</item>
    /// </list>
    /// </remarks>
    public string LoadBalancingPolicy { get; private set; } = "RoundRobin";

    /// <summary>
    /// Destinations du cluster.
    /// </summary>
    public ICollection<ClusterDestination> Destinations { get; private set; } = new List<ClusterDestination>();

    /// <summary>
    /// Configuration du health check actif.
    /// </summary>
    public HealthCheckConfig? HealthCheck { get; private set; }

    /// <summary>
    /// Configuration du client HTTP.
    /// </summary>
    public HttpClientConfig? HttpClient { get; private set; }

    /// <summary>
    /// Timeout par défaut pour les requêtes vers ce cluster.
    /// </summary>
    public TimeSpan? RequestTimeout { get; private set; }

    /// <summary>
    /// Version de session pour affinité.
    /// </summary>
    /// <remarks>
    /// Permet de router les requêtes d'un même client vers la même destination.
    /// </remarks>
    public SessionAffinityConfig? SessionAffinity { get; private set; }

    /// <summary>
    /// Métadonnées additionnelles du cluster.
    /// </summary>
    public Dictionary<string, string>? Metadata { get; private set; }

    /// <summary>
    /// Indique si le cluster est actif.
    /// </summary>
    public bool IsEnabled { get; private set; } = true;

    /// <summary>
    /// Date de création.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Date de dernière modification.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Constructeur protégé pour Entity Framework.
    /// </summary>
    protected ProxyCluster() { }

    /// <summary>
    /// Crée un nouveau cluster.
    /// </summary>
    /// <param name="clusterId">Identifiant unique du cluster.</param>
    /// <param name="tenantId">Identifiant du tenant propriétaire (optionnel).</param>
    /// <returns>Nouvelle instance de <see cref="ProxyCluster"/>.</returns>
    public static ProxyCluster Create(string clusterId, Guid? tenantId = null)
    {
        if (string.IsNullOrWhiteSpace(clusterId))
            throw new ArgumentException("L'identifiant de cluster est requis.", nameof(clusterId));

        return new ProxyCluster
        {
            Id = Guid.NewGuid(),
            ClusterId = clusterId,
            TenantId = tenantId,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Ajoute une destination au cluster.
    /// </summary>
    /// <param name="destinationId">Identifiant de la destination.</param>
    /// <param name="address">URL de base de la destination.</param>
    /// <param name="health">URL de health check (optionnel).</param>
    /// <returns>L'instance courante pour le chaînage.</returns>
    public ProxyCluster AddDestination(string destinationId, string address, string? health = null)
    {
        var destination = ClusterDestination.Create(Id, destinationId, address, health);
        Destinations.Add(destination);
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    /// <summary>
    /// Supprime une destination du cluster.
    /// </summary>
    /// <param name="destinationId">Identifiant de la destination à supprimer.</param>
    /// <returns>L'instance courante pour le chaînage.</returns>
    public ProxyCluster RemoveDestination(string destinationId)
    {
        var destination = Destinations.FirstOrDefault(d => d.DestinationId == destinationId);
        if (destination != null)
        {
            Destinations.Remove(destination);
            UpdatedAt = DateTime.UtcNow;
        }
        return this;
    }

    /// <summary>
    /// Définit la politique de load balancing.
    /// </summary>
    /// <param name="policy">Nom de la politique.</param>
    /// <returns>L'instance courante pour le chaînage.</returns>
    public ProxyCluster WithLoadBalancingPolicy(string policy)
    {
        LoadBalancingPolicy = policy;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    /// <summary>
    /// Configure le health check actif.
    /// </summary>
    /// <param name="interval">Intervalle entre les checks.</param>
    /// <param name="path">Chemin de l'endpoint de health.</param>
    /// <param name="timeout">Timeout du check.</param>
    /// <returns>L'instance courante pour le chaînage.</returns>
    public ProxyCluster WithHealthCheck(TimeSpan interval, string path, TimeSpan? timeout = null)
    {
        HealthCheck = new HealthCheckConfig
        {
            Enabled = true,
            Interval = interval,
            Path = path,
            Timeout = timeout ?? TimeSpan.FromSeconds(10)
        };
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    /// <summary>
    /// Configure le client HTTP du cluster.
    /// </summary>
    /// <param name="config">Configuration du client HTTP.</param>
    /// <returns>L'instance courante pour le chaînage.</returns>
    public ProxyCluster WithHttpClient(HttpClientConfig config)
    {
        HttpClient = config;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    /// <summary>
    /// Définit le timeout par défaut.
    /// </summary>
    /// <param name="timeout">Durée du timeout.</param>
    /// <returns>L'instance courante pour le chaînage.</returns>
    public ProxyCluster WithRequestTimeout(TimeSpan timeout)
    {
        RequestTimeout = timeout;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    /// <summary>
    /// Active ou désactive le cluster.
    /// </summary>
    /// <param name="enabled">True pour activer, false pour désactiver.</param>
    /// <returns>L'instance courante pour le chaînage.</returns>
    public ProxyCluster SetEnabled(bool enabled)
    {
        IsEnabled = enabled;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }
}

/// <summary>
/// Configuration du health check actif.
/// </summary>
public class HealthCheckConfig
{
    /// <summary>
    /// Indique si le health check est activé.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Intervalle entre les vérifications.
    /// </summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Timeout de chaque vérification.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Chemin de l'endpoint de health.
    /// </summary>
    public string Path { get; set; } = "/health";

    /// <summary>
    /// Politique de détection de pannes.
    /// </summary>
    public string Policy { get; set; } = "ConsecutiveFailures";
}

/// <summary>
/// Configuration du client HTTP pour un cluster.
/// </summary>
public class HttpClientConfig
{
    /// <summary>
    /// Accepter tout certificat SSL (développement uniquement).
    /// </summary>
    public bool DangerousAcceptAnyServerCertificate { get; set; }

    /// <summary>
    /// Nombre maximum de connexions par destination.
    /// </summary>
    public int? MaxConnectionsPerServer { get; set; }

    /// <summary>
    /// Activer la compression HTTP.
    /// </summary>
    public bool? EnableMultipleHttp2Connections { get; set; }

    /// <summary>
    /// Version HTTP à utiliser.
    /// </summary>
    public string? RequestHeaderEncoding { get; set; }
}

/// <summary>
/// Configuration de l'affinité de session.
/// </summary>
public class SessionAffinityConfig
{
    /// <summary>
    /// Indique si l'affinité est activée.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Politique d'affinité.
    /// </summary>
    public string Policy { get; set; } = "Cookie";

    /// <summary>
    /// Nom du cookie ou header d'affinité.
    /// </summary>
    public string AffinityKeyName { get; set; } = ".Yarp.Affinity";
}
