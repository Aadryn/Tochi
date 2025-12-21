using LLMProxy.Domain.Entities;

namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Résultat de la vérification de quota (implémentation du Result Pattern)
/// Indique si la requête est autorisée et fournit les détails d'utilisation
/// </summary>
public class QuotaCheckResult
{
    /// <summary>
    /// Indique si la requête est autorisée selon les quotas disponibles
    /// </summary>
    public bool IsAllowed { get; init; }
    
    /// <summary>
    /// Détails de l'utilisation du quota (actuelle et limite)
    /// </summary>
    public QuotaUsage? Usage { get; init; }
    
    /// <summary>
    /// Raison du refus si la requête n'est pas autorisée
    /// </summary>
    public string? DenialReason { get; init; }
    
    /// <summary>
    /// Crée un résultat positif (quota disponible)
    /// </summary>
    public static QuotaCheckResult Allow(QuotaUsage usage) => new()
    {
        IsAllowed = true,
        Usage = usage
    };
    
    /// <summary>
    /// Crée un résultat négatif (quota dépassé ou non disponible)
    /// </summary>
    public static QuotaCheckResult Deny(string reason, QuotaUsage? usage = null) => new()
    {
        IsAllowed = false,
        DenialReason = reason,
        Usage = usage
    };
}
