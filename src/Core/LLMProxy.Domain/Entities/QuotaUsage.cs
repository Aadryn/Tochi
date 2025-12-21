namespace LLMProxy.Domain.Entities;

/// <summary>
/// Représente l'utilisation actuelle d'un quota (stocké dans Redis pour la performance)
/// </summary>
public class QuotaUsage
{
    public Guid UserId { get; set; }
    public QuotaType QuotaType { get; set; }
    public long CurrentUsage { get; set; }
    public long Limit { get; set; }
    public DateTime WindowStart { get; set; }
    public DateTime WindowEnd { get; set; }

    public bool IsExceeded => CurrentUsage >= Limit;
    public long Remaining => Math.Max(0, Limit - CurrentUsage);
    public double PercentageUsed => Limit > 0 ? (double)CurrentUsage / Limit * 100 : 0;
}
