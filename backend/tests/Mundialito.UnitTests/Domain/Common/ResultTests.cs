using Mundialito.Domain.Common;

namespace Mundialito.UnitTests.Domain.Common;

public class ResultTests
{
    [Fact]
    public void Success_IsSuccessfulAndHasNoError()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_IsFailedAndKeepsMessageCodeAndType()
    {
        var error = Error.Conflict("El partido ya tiene un resultado registrado.", "Match.AlreadyPlayed");

        var result = Result.Failure(error);

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal("El partido ya tiene un resultado registrado.", result.Error.Message);
        Assert.Equal("Match.AlreadyPlayed", result.Error.Code);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public void Failure_AllowsErrorWithoutCode()
    {
        var result = Result.Failure(Error.Validation("El nombre del equipo es obligatorio."));

        Assert.True(result.IsFailure);
        Assert.Null(result.Error.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void SuccessWithValue_ExposesValue()
    {
        var teamId = Guid.NewGuid();

        var result = Result.Success(teamId);

        Assert.True(result.IsSuccess);
        Assert.Equal(teamId, result.Value);
    }

    [Fact]
    public void FailureWithValue_DoesNotAllowReadingValue()
    {
        var result = Result.Failure<Guid>(Error.NotFound("El equipo no existe.", "Team.NotFound"));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void ErrorFactories_AssignExpectedType()
    {
        Assert.Equal(ErrorType.Validation, Error.Validation("x").Type);
        Assert.Equal(ErrorType.NotFound, Error.NotFound("x").Type);
        Assert.Equal(ErrorType.Conflict, Error.Conflict("x").Type);
    }
}
