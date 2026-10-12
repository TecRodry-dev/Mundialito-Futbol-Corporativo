using System.Diagnostics;
using Mundialito.Application.Abstractions.Messaging;
using Mundialito.Domain.Common;

namespace Mundialito.Api.Logging;

/// <summary>
/// Envuelven a cada handler para loggear de forma estructurada: qué se ejecutó, cuánto tardó y cómo terminó.
/// Un fallo de negocio se loggea como Warning (no es un error del sistema). No se loggea el payload.
/// </summary>
internal sealed class LoggingCommandHandler<TCommand>(
    ICommandHandler<TCommand> inner,
    ILogger<LoggingCommandHandler<TCommand>> logger)
    : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = await inner.Handle(command, cancellationToken);
        HandlerLog.Write(logger, "Command", typeof(TCommand).Name, result, stopwatch.ElapsedMilliseconds);

        return result;
    }
}

internal sealed class LoggingCommandHandler<TCommand, TResponse>(
    ICommandHandler<TCommand, TResponse> inner,
    ILogger<LoggingCommandHandler<TCommand, TResponse>> logger)
    : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = await inner.Handle(command, cancellationToken);
        HandlerLog.Write(logger, "Command", typeof(TCommand).Name, result, stopwatch.ElapsedMilliseconds);

        return result;
    }
}

internal sealed class LoggingQueryHandler<TQuery, TResponse>(
    IQueryHandler<TQuery, TResponse> inner,
    ILogger<LoggingQueryHandler<TQuery, TResponse>> logger)
    : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = await inner.Handle(query, cancellationToken);
        HandlerLog.Write(logger, "Query", typeof(TQuery).Name, result, stopwatch.ElapsedMilliseconds);

        return result;
    }
}

internal static class HandlerLog
{
    public static void Write(ILogger logger, string kind, string name, Result result, long elapsedMs)
    {
        if (result.IsSuccess)
        {
            logger.LogInformation(
                "{HandlerKind} {HandlerName} completado en {ElapsedMs} ms",
                kind, name, elapsedMs);
            return;
        }

        logger.LogWarning(
            "{HandlerKind} {HandlerName} rechazado con {ErrorCode} ({ErrorType}) en {ElapsedMs} ms: {ErrorMessage}",
            kind, name, result.Error.Code, result.Error.Type, elapsedMs, result.Error.Message);
    }
}
