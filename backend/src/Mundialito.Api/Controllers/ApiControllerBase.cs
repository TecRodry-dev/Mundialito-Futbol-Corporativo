using Microsoft.AspNetCore.Mvc;
using Mundialito.Api.Extensions;
using Mundialito.Domain.Common;

namespace Mundialito.Api.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Convierte un error de negocio en 400 / 404 / 409 con ProblemDetails.</summary>
    protected IActionResult ErrorResult(Error error) => error.ToProblemResult(HttpContext);

    /// <summary>200 con el valor si salió bien; si no, el error traducido a HTTP.</summary>
    protected IActionResult OkOrError<T>(Result<T> result) =>
        result.IsSuccess ? Ok(result.Value) : ErrorResult(result.Error);
}
