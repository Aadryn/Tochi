using System.Linq.Expressions;
using LLMProxy.Domain.Entities;

namespace LLMProxy.Domain.Specifications;

/// <summary>
/// Spécification pour vérifier si un tenant est éligible.
/// Conforme à ADR-028 (Specification Pattern).
/// </summary>
/// <remarks>
/// Un tenant est éligible si :
/// - Il est actif (IsActive = true)
/// - Il n'a pas été désactivé (DeactivatedAt = null)
/// 
/// Cette spécification centralise la règle métier de validation tenant,
/// éliminant la duplication trouvée dans 5+ handlers.
/// </remarks>
public sealed class TenantIsEligibleSpecification : CompositeSpecification<Tenant>
{
    /// <summary>
    /// Vérifie si le tenant est éligible (actif et non désactivé).
    /// </summary>
    /// <param name="tenant">Tenant à vérifier.</param>
    /// <returns><c>true</c> si éligible, sinon <c>false</c>.</returns>
    public override bool IsSatisfiedBy(Tenant tenant)
    {
        if (tenant is null)
            return false;

        return tenant.IsActive && tenant.DeactivatedAt is null;
    }

    /// <summary>
    /// Expression LINQ pour requêtes EF Core.
    /// </summary>
    /// <returns>Expression (t => t.IsActive && t.DeactivatedAt == null).</returns>
    public override Expression<Func<Tenant, bool>> ToExpression()
    {
        return t => t.IsActive && t.DeactivatedAt == null;
    }
}
