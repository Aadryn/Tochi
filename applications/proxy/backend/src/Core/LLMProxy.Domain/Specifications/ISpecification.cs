using System.Linq.Expressions;

namespace LLMProxy.Domain.Specifications;

/// <summary>
/// Spécification pour encapsuler une règle métier réutilisable.
/// Conforme à ADR-028 (Specification Pattern).
/// </summary>
/// <typeparam name="T">Type d'entité sur laquelle la spécification s'applique.</typeparam>
/// <remarks>
/// Permet de centraliser les règles métier complexes et de les combiner avec des opérateurs logiques.
/// Support à la fois l'évaluation in-memory (<see cref="IsSatisfiedBy"/>) et les requêtes EF Core (<see cref="ToExpression"/>).
/// </remarks>
public interface ISpecification<T>
{
    /// <summary>
    /// Vérifie si une entité satisfait la spécification (évaluation in-memory).
    /// </summary>
    /// <param name="entity">Entité à tester.</param>
    /// <returns><c>true</c> si l'entité satisfait la règle métier, sinon <c>false</c>.</returns>
    /// <remarks>
    /// Utilisé pour valider des entités déjà chargées en mémoire.
    /// Pour les requêtes EF Core, utiliser <see cref="ToExpression"/> à la place.
    /// </remarks>
    bool IsSatisfiedBy(T entity);

    /// <summary>
    /// Convertit la spécification en expression LINQ pour requêtes EF Core.
    /// </summary>
    /// <returns>Expression LINQ représentant la règle métier.</returns>
    /// <remarks>
    /// Permet d'utiliser la spécification dans des requêtes EF Core :
    /// <code>
    /// var spec = new TenantIsEligibleSpecification();
    /// var eligibleTenants = await context.Tenants
    ///     .Where(spec.ToExpression())
    ///     .ToListAsync();
    /// </code>
    /// </remarks>
    Expression<Func<T, bool>> ToExpression();
}
