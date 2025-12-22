using System.Linq.Expressions;

namespace LLMProxy.Domain.Specifications;

/// <summary>
/// Spécification représentant l'opérateur logique AND entre deux spécifications.
/// Conforme à ADR-028 (Specification Pattern).
/// </summary>
/// <typeparam name="T">Type d'entité.</typeparam>
/// <remarks>
/// Combine deux spécifications : l'entité doit satisfaire BOTH left ET right.
/// </remarks>
internal sealed class AndSpecification<T> : CompositeSpecification<T>
{
    private readonly ISpecification<T> _left;
    private readonly ISpecification<T> _right;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="AndSpecification{T}"/>.
    /// </summary>
    /// <param name="left">Spécification de gauche.</param>
    /// <param name="right">Spécification de droite.</param>
    public AndSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        _left = left;
        _right = right;
    }

    /// <summary>
    /// Vérifie si l'entité satisfait les deux spécifications.
    /// </summary>
    /// <param name="entity">Entité à tester.</param>
    /// <returns><c>true</c> si satisfait les deux, sinon <c>false</c>.</returns>
    public override bool IsSatisfiedBy(T entity)
    {
        return _left.IsSatisfiedBy(entity) && _right.IsSatisfiedBy(entity);
    }

    /// <summary>
    /// Convertit en expression LINQ combinant les deux spécifications avec AND.
    /// </summary>
    /// <returns>Expression (left AND right).</returns>
    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpression = _left.ToExpression();
        var rightExpression = _right.ToExpression();

        var parameter = Expression.Parameter(typeof(T), "x");
        var leftBody = Expression.Invoke(leftExpression, parameter);
        var rightBody = Expression.Invoke(rightExpression, parameter);
        var andExpression = Expression.AndAlso(leftBody, rightBody);

        return Expression.Lambda<Func<T, bool>>(andExpression, parameter);
    }
}
