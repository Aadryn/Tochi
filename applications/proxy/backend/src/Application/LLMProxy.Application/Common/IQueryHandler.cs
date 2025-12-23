using LLMProxy.Domain.Common;
using MediatR;

namespace LLMProxy.Application.Common;

/// <summary>
/// Interface de base pour les gestionnaires de requêtes CQRS.
/// </summary>
/// <typeparam name="TQuery">Type de la requête à traiter.</typeparam>
/// <typeparam name="TResponse">Type de la réponse retournée.</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
