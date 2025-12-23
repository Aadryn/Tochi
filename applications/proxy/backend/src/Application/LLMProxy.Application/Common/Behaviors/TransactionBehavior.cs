// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// LLMProxy - MediatR Pipeline Behavior pour la gestion des transactions
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using LLMProxy.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Application.Common.Behaviors;

/// <summary>
/// Pipeline Behavior MediatR pour envelopper les commandes dans une transaction.
/// Assure l'atomicité des opérations de modification de données.
/// </summary>
/// <remarks>
/// <para>
/// Ce behavior s'applique uniquement aux <b>Commands</b> (pas aux Queries) et :
/// </para>
/// <list type="bullet">
/// <item><description>Démarre une transaction avant l'exécution du handler</description></item>
/// <item><description>Commit automatiquement si le handler réussit</description></item>
/// <item><description>Rollback automatiquement en cas d'exception</description></item>
/// </list>
/// <para>
/// Les Queries sont ignorées car elles ne modifient pas les données (principe CQRS).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // La transaction est gérée automatiquement pour les commandes
/// await mediator.Send(new CreateTenantCommand { Name = "Acme" });
/// // → BEGIN TRANSACTION
/// // → INSERT INTO Tenants...
/// // → COMMIT (ou ROLLBACK si erreur)
/// </code>
/// </example>
/// <typeparam name="TRequest">Type de la requête MediatR.</typeparam>
/// <typeparam name="TResponse">Type de la réponse attendue.</typeparam>
public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="TransactionBehavior{TRequest, TResponse}"/>.
    /// </summary>
    /// <param name="unitOfWork">Unité de travail pour la gestion des transactions.</param>
    /// <param name="logger">Logger pour tracer les transactions.</param>
    public TransactionBehavior(
        IUnitOfWork unitOfWork,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Ignorer les Queries (lecture seule, pas de transaction nécessaire)
        if (!IsCommand(requestName))
        {
            return await next();
        }

        _logger.LogDebug("Début transaction pour {RequestName}", requestName);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var response = await next();

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogDebug("Transaction committée pour {RequestName}", requestName);

            return response;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            _logger.LogWarning(
                ex,
                "Transaction rollback pour {RequestName} : {ErrorMessage}",
                requestName,
                ex.Message);

            throw;
        }
    }

    /// <summary>
    /// Détermine si la requête est une Command (nécessite une transaction).
    /// </summary>
    /// <param name="requestName">Nom du type de requête.</param>
    /// <returns><c>true</c> si c'est une Command, <c>false</c> sinon.</returns>
    private static bool IsCommand(string requestName)
    {
        return requestName.EndsWith("Command", StringComparison.OrdinalIgnoreCase);
    }
}
