using System.Linq.Expressions;

namespace LLMProxy.Domain.Specifications;

/// <summary>
/// Classe de base pour spécifications composables avec opérateurs logiques.
/// Conforme à ADR-028 (Specification Pattern).
/// </summary>
/// <typeparam name="T">Type d'entité.</typeparam>
/// <remarks>
/// Fournit les opérateurs And, Or, Not pour combiner des spécifications.
/// Les classes dérivées doivent implémenter <see cref="ToExpression"/>.
/// </remarks>
public abstract class CompositeSpecification<T> : ISpecification<T>
{
    /// <summary>
    /// Vérifie si une entité satisfait la spécification.
    /// </summary>
    /// <param name="entity">Entité à tester.</param>
    /// <returns><c>true</c> si satisfait, sinon <c>false</c>.</returns>
    /// <remarks>
    /// Implémentation par défaut utilisant <see cref="ToExpression"/>.
    /// Peut être surchargée pour optimiser l'évaluation in-memory.
    /// </remarks>
    public virtual bool IsSatisfiedBy(T entity)
    {
        var predicate = ToExpression().Compile();
        return predicate(entity);
    }

    /// <summary>
    /// Convertit la spécification en expression LINQ.
    /// </summary>
    /// <returns>Expression LINQ représentant la règle métier.</returns>
    /// <remarks>
    /// Doit être implémentée par les classes dérivées.
    /// </remarks>
    public abstract Expression<Func<T, bool>> ToExpression();

    /// <summary>
    /// Combine deux spécifications avec l'opérateur logique AND.
    /// </summary>
    /// <param name="other">Autre spécification à combiner.</param>
    /// <returns>Nouvelle spécification représentant (this AND other).</returns>
    /// <remarks>
    /// Exemple :
    /// <code>
    /// var spec = new TenantIsActiveSpecification()
    ///     .And(new TenantHasQuotaSpecification());
    /// </code>
    /// </remarks>
    public ISpecification<T> And(ISpecification<T> other)
    {
        return new AndSpecification<T>(this, other);
    }

    /// <summary>
    /// Combine deux spécifications avec l'opérateur logique OR.
    /// </summary>
    /// <param name="other">Autre spécification à combiner.</param>
    /// <returns>Nouvelle spécification représentant (this OR other).</returns>
    /// <remarks>
    /// Exemple :
    /// <code>
    /// var spec = new TenantIsActiveSpecification()
    ///     .Or(new TenantIsPremiumSpecification());
    /// </code>
    /// </remarks>
    public ISpecification<T> Or(ISpecification<T> other)
    {
        return new OrSpecification<T>(this, other);
    }

    /// <summary>
    /// Inverse la spécification avec l'opérateur logique NOT.
    /// </summary>
    /// <returns>Nouvelle spécification représentant (NOT this).</returns>
    /// <remarks>
    /// Exemple :
    /// <code>
    /// var spec = new TenantIsActiveSpecification().Not();
    /// // Retourne les tenants inactifs
    /// </code>
    /// </remarks>
    public ISpecification<T> Not()
    {
        return new NotSpecification<T>(this);
    }
}
