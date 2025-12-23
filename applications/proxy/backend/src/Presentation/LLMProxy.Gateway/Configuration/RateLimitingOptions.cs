using System.ComponentModel.DataAnnotations;

namespace LLMProxy.Gateway.Configuration;

/// <summary>
/// Options de configuration pour le rate limiting multi-niveau.
/// Conforme à ADR-041 (Rate Limiting et Throttling).
/// </summary>
public sealed class RateLimitingOptions
{
    /// <summary>
    /// Configuration du rate limit global (protection infrastructure).
    /// </summary>
    [Required]
    public FixedWindowOptions Global { get; set; } = new();

    /// <summary>
    /// Configuration du rate limit par tenant (équité multi-tenant).
    /// </summary>
    [Required]
    public FixedWindowOptions PerTenant { get; set; } = new();

    /// <summary>
    /// Configuration du rate limit par utilisateur (abus individuel).
    /// </summary>
    [Required]
    public SlidingWindowOptions PerUser { get; set; } = new();

    /// <summary>
    /// Configuration du rate limit par adresse IP (protection DDoS).
    /// </summary>
    [Required]
    public FixedWindowOptions PerIp { get; set; } = new();

    /// <summary>
    /// Configuration du limiteur de concurrence (connexions simultanées).
    /// </summary>
    [Required]
    public ConcurrencyOptions Concurrency { get; set; } = new();
}

/// <summary>
/// Configuration pour stratégie Fixed Window.
/// Fenêtre temporelle fixe avec reset périodique.
/// </summary>
public sealed class FixedWindowOptions
{
    /// <summary>
    /// Nombre maximum de requêtes autorisées par fenêtre.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PermitLimit { get; set; }

    /// <summary>
    /// Durée de la fenêtre temporelle.
    /// Format: "hh:mm:ss" (ex: "00:01:00" pour 1 minute).
    /// </summary>
    [Required]
    public TimeSpan Window { get; set; }
}

/// <summary>
/// Configuration pour stratégie Sliding Window.
/// Fenêtre glissante pour lisser le trafic et éviter les bursts.
/// </summary>
public sealed class SlidingWindowOptions
{
    /// <summary>
    /// Nombre maximum de requêtes autorisées par fenêtre glissante.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PermitLimit { get; set; }

    /// <summary>
    /// Durée de la fenêtre glissante.
    /// </summary>
    [Required]
    public TimeSpan Window { get; set; }

    /// <summary>
    /// Nombre de segments par fenêtre (plus élevé = plus précis).
    /// Recommandé: 6-10 segments.
    /// </summary>
    [Range(2, 100)]
    public int SegmentsPerWindow { get; set; } = 6;
}

/// <summary>
/// Configuration pour limiteur de concurrence.
/// Limite le nombre de requêtes traitées simultanément.
/// </summary>
public sealed class ConcurrencyOptions
{
    /// <summary>
    /// Nombre maximum de requêtes simultanées autorisées.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PermitLimit { get; set; }

    /// <summary>
    /// Taille de la file d'attente pour requêtes en excès.
    /// 0 = rejet immédiat sans file d'attente.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int QueueLimit { get; set; } = 0;
}
