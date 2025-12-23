using LLMProxy.Domain.Common;
using MediatR;

namespace LLMProxy.Application.Common;

/// <summary>
/// Interface de base pour les requêtes CQRS (lecture seule).
/// </summary>
/// <typeparam name="TResponse">Type de la réponse retournée par la requête.</typeparam>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
