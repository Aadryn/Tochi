using LLMProxy.Domain.Common;
using MediatR;

namespace LLMProxy.Application.Common;

/// <summary>
/// Interface de base pour les gestionnaires de commandes CQRS sans valeur de retour.
/// </summary>
/// <typeparam name="TCommand">Type de la commande à traiter.</typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}
