using Mundialito.Domain.Common;

namespace Mundialito.Application.Abstractions.Messaging;

/// <summary>
/// Ejecuta un comando que no devuelve valor.
/// </summary>
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    Task<Result> Handle(TCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// Ejecuta un comando que devuelve un valor si sale bien.
/// </summary>
public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
}
