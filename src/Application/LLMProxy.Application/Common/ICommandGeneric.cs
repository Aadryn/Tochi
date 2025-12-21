using LLMProxy.Domain.Common;
using MediatR;

namespace LLMProxy.Application.Common;

/// <summary>
/// Interface de base pour les commandes CQRS avec valeur de retour.
/// </summary>
/// <typeparam name="TResponse">Type de la réponse retournée par la commande.</typeparam>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
