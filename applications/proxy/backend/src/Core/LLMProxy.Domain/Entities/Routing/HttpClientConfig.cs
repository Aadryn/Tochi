namespace LLMProxy.Domain.Entities.Routing;

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
