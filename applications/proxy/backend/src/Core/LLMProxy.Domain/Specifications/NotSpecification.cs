using System.Linq.Expressions;

namespace LLMProxy.Domain.Specifications;

/// <summary>
/// Spécification représentant l'opérateur logique NOT (négation).
/// Conforme à ADR-028 (Specification Pattern).
/// </summary>
/// <typeparam name="T">Type d'entité.</typeparam>
/// <remarks>
/// Inverse une spécification : l'entité doit NE PAS satisfaire la spécification encapsulée.
/// </remarks>
internal sealed class NotSpecification<T> : CompositeSpecification<T>
{
    private readonly ISpecification<T> _specification;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="NotSpecification{T}"/>.
    /// </summary>
    /// <param name="specification">Spécification à inverser.</param>
    public NotSpecification(ISpecification<T> specification)
    {
        _specification = specification;
    }

    /// <summary>
    /// Vérifie si l'entité NE satisfait PAS la spécification.
    /// </summary>
    /// <param name="entity">Entité à tester.</param>
    /// <returns><c>true</c> si NE satisfait PAS, sinon <c>false</c>.</returns>
    public override bool IsSatisfiedBy(T entity)
    {
        return !_specification.IsSatisfiedBy(entity);
    }

    /// <summary>
    /// Convertit en expression LINQ inversant la spécification avec NOT.
    /// </summary>
    /// <returns>Expression (NOT specification).</returns>
    public override Expression<Func<T, bool>> ToExpression()
    {
        var expression = _specification.ToExpression();
        var parameter = Expression.Parameter(typeof(T), "x");
        var body = Expression.Invoke(expression, parameter);
        var notExpression = Expression.Not(body);

        return Expression.Lambda<Func<T, bool>>(notExpression, parameter);
    }
}
