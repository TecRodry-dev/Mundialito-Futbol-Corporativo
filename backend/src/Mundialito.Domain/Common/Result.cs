namespace Mundialito.Domain.Common;

/// <summary>
/// Resultado de una operación sin valor de retorno: éxito, o fallo con su <see cref="Error"/>.
/// Reemplaza a las excepciones en los casos de negocio esperados.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

/// <summary>
/// Resultado de una operación que, si sale bien, devuelve un valor.
/// </summary>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Valor de la operación; solo existe si fue exitosa.
    /// Leerlo en un fallo es un error de programación (no un caso de negocio),
    /// por eso aquí sí se lanza una excepción.
    /// </summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("No se puede leer el valor de un resultado fallido.");
}
