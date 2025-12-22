namespace LLMProxy.Domain.Entities.Routing;

/// <summary>
/// Représente une destination (backend) au sein d'un cluster YARP.
/// </summary>
/// <remarks>
/// <para>
/// Une destination est un serveur backend vers lequel les requêtes peuvent
/// être routées. Un cluster peut contenir plusieurs destinations pour
/// le load balancing et la haute disponibilité.
/// </para>
/// <para>
/// <b>Exemple :</b>
/// <code>
/// var destination = ClusterDestination.Create(
///     clusterId: cluster.Id,
///     destinationId: "openai-primary",
///     address: "https://api.openai.com",
///     health: "https://api.openai.com/v1/models"
/// );
/// </code>
/// </para>
/// </remarks>
public class ClusterDestination
{
    /// <summary>
    /// Identifiant unique de l'entité en base de données.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Identifiant du cluster parent.
    /// </summary>
    public Guid ClusterId { get; private set; }

    /// <summary>
    /// Cluster parent (navigation EF).
    /// </summary>
    public ProxyCluster? Cluster { get; private set; }

    /// <summary>
    /// Identifiant de la destination au sein du cluster.
    /// </summary>
    /// <remarks>
    /// Doit être unique au sein du cluster.
    /// </remarks>
    /// <example>"primary", "secondary", "eu-west-1"</example>
    public string DestinationId { get; private set; } = string.Empty;

    /// <summary>
    /// URL de base de la destination.
    /// </summary>
    /// <remarks>
    /// Inclut le schéma, l'hôte et le port. Le chemin de la requête
    /// sera ajouté à cette URL de base.
    /// </remarks>
    /// <example>"https://api.openai.com", "http://localhost:11434"</example>
    public string Address { get; private set; } = string.Empty;

    /// <summary>
    /// URL de l'endpoint de health check.
    /// </summary>
    /// <remarks>
    /// Si différent de l'adresse principale. Utilisé pour les health checks
    /// actifs configurés au niveau du cluster.
    /// </remarks>
    public string? Health { get; private set; }

    /// <summary>
    /// Métadonnées de la destination.
    /// </summary>
    /// <remarks>
    /// Permet de stocker des informations additionnelles comme :
    /// <list type="bullet">
    /// <item>Région géographique</item>
    /// <item>Poids pour weighted load balancing</item>
    /// <item>Tags pour filtrage</item>
    /// </list>
    /// </remarks>
    public Dictionary<string, string>? Metadata { get; private set; }

    /// <summary>
    /// Indique si la destination est active.
    /// </summary>
    /// <remarks>
    /// Permet de désactiver temporairement une destination (maintenance)
    /// sans la supprimer du cluster.
    /// </remarks>
    public bool IsEnabled { get; private set; } = true;

    /// <summary>
    /// Poids pour le load balancing pondéré.
    /// </summary>
    /// <remarks>
    /// Utilisé avec la politique <c>Weighted</c>. Plus le poids est élevé,
    /// plus la destination recevra de requêtes.
    /// </remarks>
    public int Weight { get; private set; } = 1;

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
    protected ClusterDestination() { }

    /// <summary>
    /// Crée une nouvelle destination.
    /// </summary>
    /// <param name="clusterId">Identifiant du cluster parent.</param>
    /// <param name="destinationId">Identifiant de la destination.</param>
    /// <param name="address">URL de base.</param>
    /// <param name="health">URL de health check (optionnel).</param>
    /// <returns>Nouvelle instance de <see cref="ClusterDestination"/>.</returns>
    public static ClusterDestination Create(
        Guid clusterId,
        string destinationId,
        string address,
        string? health = null)
    {
        if (string.IsNullOrWhiteSpace(destinationId))
            throw new ArgumentException("L'identifiant de destination est requis.", nameof(destinationId));

        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("L'adresse est requise.", nameof(address));

        if (!Uri.TryCreate(address, UriKind.Absolute, out _))
            throw new ArgumentException("L'adresse doit être une URL valide.", nameof(address));

        return new ClusterDestination
        {
            Id = Guid.NewGuid(),
            ClusterId = clusterId,
            DestinationId = destinationId,
            Address = address,
            Health = health,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Met à jour l'adresse de la destination.
    /// </summary>
    /// <param name="address">Nouvelle adresse.</param>
    /// <returns>L'instance courante pour le chaînage.</returns>
    public ClusterDestination WithAddress(string address)
    {
        if (!Uri.TryCreate(address, UriKind.Absolute, out _))
            throw new ArgumentException("L'adresse doit être une URL valide.", nameof(address));

        Address = address;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    /// <summary>
    /// Définit l'URL de health check.
    /// </summary>
    /// <param name="health">URL de health check.</param>
    /// <returns>L'instance courante pour le chaînage.</returns>
    public ClusterDestination WithHealth(string health)
    {
        Health = health;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    /// <summary>
    /// Définit le poids pour le load balancing.
    /// </summary>
    /// <param name="weight">Poids (1-100).</param>
    /// <returns>L'instance courante pour le chaînage.</returns>
    public ClusterDestination WithWeight(int weight)
    {
        if (weight < 1 || weight > 100)
            throw new ArgumentOutOfRangeException(nameof(weight), "Le poids doit être entre 1 et 100.");

        Weight = weight;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    /// <summary>
    /// Active ou désactive la destination.
    /// </summary>
    /// <param name="enabled">True pour activer, false pour désactiver.</param>
    /// <returns>L'instance courante pour le chaînage.</returns>
    public ClusterDestination SetEnabled(bool enabled)
    {
        IsEnabled = enabled;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    /// <summary>
    /// Ajoute une métadonnée.
    /// </summary>
    /// <param name="key">Clé de la métadonnée.</param>
    /// <param name="value">Valeur de la métadonnée.</param>
    /// <returns>L'instance courante pour le chaînage.</returns>
    public ClusterDestination WithMetadata(string key, string value)
    {
        Metadata ??= new Dictionary<string, string>();
        Metadata[key] = value;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }
}
