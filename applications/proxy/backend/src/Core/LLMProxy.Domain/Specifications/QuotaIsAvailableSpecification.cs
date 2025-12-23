using System.Linq.Expressions;
using LLMProxy.Domain.Entities;

namespace LLMProxy.Domain.Specifications;

/// <summary>
/// Spécification pour vérifier si un quota est disponible (non dépassé).
/// Conforme à ADR-028 (Specification Pattern).
/// </summary>
/// <remarks>
/// Un quota est disponible si :
/// - L'usage actuel + la quantité demandée <= limite du quota
/// - Le quota n'est pas déjà dépassé
/// 
/// Cette spécification centralise la règle métier de validation de quota,
/// éliminant la duplication trouvée dans 3+ handlers.
/// </remarks>
public sealed class QuotaIsAvailableSpecification : CompositeSpecification<QuotaUsage>
{
    private readonly long _requestedAmount;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="QuotaIsAvailableSpecification"/>.
    /// </summary>
    /// <param name="requestedAmount">Quantité de ressource demandée (tokens, requêtes).</param>
    public QuotaIsAvailableSpecification(long requestedAmount)
    {
        if (requestedAmount <= 0)
            throw new ArgumentException("La quantité demandée doit être positive.", nameof(requestedAmount));

        _requestedAmount = requestedAmount;
    }

    /// <summary>
    /// Vérifie si le quota permet la consommation demandée.
    /// </summary>
    /// <param name="quota">Usage actuel du quota.</param>
    /// <returns><c>true</c> si suffisant, sinon <c>false</c>.</returns>
    public override bool IsSatisfiedBy(QuotaUsage quota)
    {
        if (quota is null)
            return false;

        return (quota.CurrentUsage + _requestedAmount) <= quota.Limit;
    }

    /// <summary>
    /// Expression LINQ pour requêtes EF Core.
    /// </summary>
    /// <returns>Expression (q => (q.CurrentUsage + requestedAmount) &lt;= q.Limit).</returns>
    /// <remarks>
    /// Note : Capture de la variable locale _requestedAmount dans l'expression.
    /// EF Core peut traduire cette expression en SQL pour filtrage côté base de données.
    /// </remarks>
    public override Expression<Func<QuotaUsage, bool>> ToExpression()
    {
        var requested = _requestedAmount; // Capture pour expression
        return q => (q.CurrentUsage + requested) <= q.Limit;
    }
}
