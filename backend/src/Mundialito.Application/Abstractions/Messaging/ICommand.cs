namespace Mundialito.Application.Abstractions.Messaging;

/// <summary>
/// Comando (escritura) que no devuelve valor. Se ejecuta con EF Core y se confirma con el Unit of Work.
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Comando (escritura) que devuelve un valor si sale bien (ej. el Id creado).
/// </summary>
public interface ICommand<TResponse>
{
}
