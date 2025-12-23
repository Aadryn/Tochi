// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// LLMProxy - MediatR Pipeline Behavior pour la validation FluentValidation
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using FluentValidation;
using MediatR;

namespace LLMProxy.Application.Common.Behaviors;

/// <summary>
/// Pipeline Behavior MediatR pour valider automatiquement les requêtes via FluentValidation.
/// Exécute tous les validateurs enregistrés avant le handler.
/// </summary>
/// <remarks>
/// <para>
/// Ce behavior intercepte toutes les requêtes MediatR et :
/// </para>
/// <list type="bullet">
/// <item><description>Récupère tous les validateurs FluentValidation pour le type de requête</description></item>
/// <item><description>Exécute la validation de manière asynchrone</description></item>
/// <item><description>Agrège toutes les erreurs de validation</description></item>
/// <item><description>Lance une <see cref="ValidationException"/> si des erreurs sont trouvées</description></item>
/// </list>
/// <para>
/// Si aucun validateur n'est enregistré pour une requête, celle-ci passe directement au handler.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Exemple de validateur pour une commande
/// public class CreateTenantValidator : AbstractValidator&lt;CreateTenantCommand&gt;
/// {
///     public CreateTenantValidator()
///     {
///         RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
///         RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9-]+$");
///     }
/// }
/// 
/// // La validation s'exécute automatiquement via le pipeline
/// var result = await mediator.Send(new CreateTenantCommand { Name = "" });
/// // → ValidationException levée automatiquement
/// </code>
/// </example>
/// <typeparam name="TRequest">Type de la requête MediatR.</typeparam>
/// <typeparam name="TResponse">Type de la réponse attendue.</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="ValidationBehavior{TRequest, TResponse}"/>.
    /// </summary>
    /// <param name="validators">Collection de validateurs FluentValidation pour ce type de requête.</param>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    /// <inheritdoc/>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Pas de validateurs = passer directement au handler
        if (!_validators.Any())
        {
            return await next();
        }

        // Créer le contexte de validation
        var context = new ValidationContext<TRequest>(request);

        // Exécuter tous les validateurs en parallèle
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Collecter toutes les erreurs
        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(error => error is not null)
            .ToList();

        // Si des erreurs, lever une exception
        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}
