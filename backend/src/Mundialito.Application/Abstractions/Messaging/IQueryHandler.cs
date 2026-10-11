using Mundialito.Domain.Common;

namespace Mundialito.Application.Abstractions.Messaging;

/// <summary>
/// Ejecuta una consulta de solo lectura.
/// </summary>
public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}
