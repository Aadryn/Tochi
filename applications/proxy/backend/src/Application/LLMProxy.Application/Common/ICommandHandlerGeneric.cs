using LLMProxy.Domain.Common;
using MediatR;

namespace LLMProxy.Application.Common;

/// <summary>
/// Interface de base pour les gestionnaires de commandes CQRS avec valeur de retour.
/// </summary>
/// <typeparam name="TCommand">Type de la commande à traiter.</typeparam>
/// <typeparam name="TResponse">Type de la réponse retournée.</typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
