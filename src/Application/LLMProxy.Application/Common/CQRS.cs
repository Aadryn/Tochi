using MediatR;
using LLMProxy.Domain.Common;

namespace LLMProxy.Application.Common;

/// <summary>
/// Base interface for commands (CQRS)
/// </summary>
public interface ICommand : IRequest<Result>
{
}

/// <summary>
/// Base interface for commands with return value (CQRS)
/// </summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}

/// <summary>
/// Base interface for queries (CQRS)
/// </summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}

/// <summary>
/// Base interface for command handlers (CQRS)
/// </summary>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

/// <summary>
/// Base interface for command handlers with return value (CQRS)
/// </summary>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}

/// <summary>
/// Base interface for query handlers (CQRS)
/// </summary>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
