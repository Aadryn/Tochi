using System.ComponentModel.DataAnnotations;

namespace LLMProxy.Admin.API.Configuration;

/// <summary>
/// Options de configuration pour le rate limiting de l'API d'administration.
/// Conforme à ADR-041 (Rate Limiting et Throttling).
/// </summary>
/// <remarks>
/// Admin.API a besoin de rate limiting simplifié (pas de tenants ni users).
/// Protection principale contre DDoS et abus par IP.
/// </remarks>
public sealed class RateLimitingOptions
{
    /// <summary>
    /// Configuration du rate limit global (protection infrastructure).
    /// Limite le nombre total de requêtes simultanées vers l'API.
    /// </summary>
    [Required]
    public FixedWindowOptions Global { get; set; } = new();

    /// <summary>
    /// Configuration du rate limit par adresse IP (protection DDoS).
    /// Empêche un client unique de saturer le service.
    /// </summary>
    [Required]
    public FixedWindowOptions PerIp { get; set; } = new();

    /// <summary>
    /// Configuration du limiteur de concurrence (connexions simultanées).
    /// Protège les ressources serveur contre la surcharge.
    /// </summary>
    [Required]
    public ConcurrencyOptions Concurrency { get; set; } = new();
}

/// <summary>
/// Configuration pour stratégie Fixed Window.
/// Fenêtre temporelle fixe avec reset périodique.
/// </summary>
/// <remarks>
/// Algorithme simple et efficace pour limiter le débit moyen.
/// Exemple : 100 requêtes par minute → reset toutes les minutes.
/// </remarks>
public sealed class FixedWindowOptions
{
    /// <summary>
    /// Nombre maximum de requêtes autorisées par fenêtre.
    /// </summary>
    /// <example>100 pour autoriser 100 requêtes par fenêtre.</example>
    [Range(1, int.MaxValue)]
    public int PermitLimit { get; set; }

    /// <summary>
    /// Durée de la fenêtre temporelle.
    /// </summary>
    /// <remarks>
    /// Format TimeSpan : "hh:mm:ss" (ex: "00:01:00" pour 1 minute).
    /// Une fenêtre plus courte offre une protection plus réactive.
    /// Une fenêtre plus longue lisse mieux le trafic.
    /// </remarks>
    /// <example>TimeSpan.FromMinutes(1) pour fenêtre de 1 minute.</example>
    [Required]
    public TimeSpan Window { get; set; }
}

/// <summary>
/// Configuration pour limiteur de concurrence.
/// Limite le nombre de requêtes traitées simultanément.
/// </summary>
/// <remarks>
/// Protège le serveur contre épuisement de threads et mémoire.
/// À distinguer du rate limiting temporel (fenêtre temporelle).
/// </remarks>
public sealed class ConcurrencyOptions
{
    /// <summary>
    /// Nombre maximum de requêtes simultanées autorisées.
    /// </summary>
    /// <remarks>
    /// Ajuster selon capacité serveur (CPU, RAM, connexions DB).
    /// Trop bas : sous-utilisation. Trop haut : risque saturation.
    /// </remarks>
    /// <example>50 pour autoriser 50 requêtes simultanées max.</example>
    [Range(1, int.MaxValue)]
    public int PermitLimit { get; set; }

    /// <summary>
    /// Nombre de requêtes en file d'attente quand limite atteinte.
    /// </summary>
    /// <remarks>
    /// 0 = rejet immédiat (recommandé pour Admin API).
    /// > 0 = mise en file (risque timeouts).
    /// </remarks>
    /// <example>0 pour rejeter immédiatement si limite atteinte.</example>
    [Range(0, int.MaxValue)]
    public int QueueLimit { get; set; } = 0;
}
