using LLMProxy.Domain.Common;
using MediatR;

namespace LLMProxy.Application.Common;

/// <summary>
/// Interface de base pour les commandes CQRS sans valeur de retour.
/// </summary>
public interface ICommand : IRequest<Result>
{
}
