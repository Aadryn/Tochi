using LLMProxy.Domain.Entities;

namespace LLMProxy.Infrastructure.Redis;

/// <summary>
/// Modèle de cache interne pour les limites de quota stockées dans Redis.
/// </summary>
/// <remarks>
/// Structure sérialisée en JSON et stockée dans Redis avec une clé dédiée par utilisateur et type de quota.
/// Permet de valider les consommations sans requêter la base de données.
/// </remarks>
internal class QuotaLimitCache
{
    /// <summary>
    /// Obtient ou définit l'identifiant de l'utilisateur.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Obtient ou définit le type de quota concerné.
    /// </summary>
    public QuotaType QuotaType { get; set; }

    /// <summary>
    /// Obtient ou définit la période de la fenêtre de quota.
    /// </summary>
    public QuotaPeriod Period { get; set; }

    /// <summary>
    /// Obtient ou définit la valeur maximale autorisée pour ce quota.
    /// </summary>
    public long MaxValue { get; set; }
}
