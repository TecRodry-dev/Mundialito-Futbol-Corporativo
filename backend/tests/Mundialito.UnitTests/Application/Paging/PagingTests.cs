using Mundialito.Application.Common.Paging;
using Mundialito.Domain.Common;

namespace Mundialito.UnitTests.Application.Paging;

public class PagingTests
{
    private static readonly IReadOnlyCollection<string> AllowedSortFields = ["name", "createdAt"];

    [Fact]
    public void Validate_WithDefaultValues_IsSuccessful()
    {
        var result = new PageRequest().Validate(AllowedSortFields);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Validate_WithAllowedSortByIgnoringCase_IsSuccessful()
    {
        var result = new PageRequest(SortBy: "NAME").Validate(AllowedSortFields);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData("password")]
    [InlineData("name; DROP TABLE Teams")]
    public void Validate_WithSortByOutsideWhitelist_ReturnsValidationError(string sortBy)
    {
        var result = new PageRequest(SortBy: sortBy).Validate(AllowedSortFields);

        Assert.True(result.IsFailure);
        Assert.Equal("Paging.InvalidSortBy", result.Error.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, PageRequest.MaxPageSize + 1)]
    public void Validate_WithInvalidPage_ReturnsValidationError(int pageNumber, int pageSize)
    {
        var result = new PageRequest(pageNumber, pageSize).Validate(AllowedSortFields);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void Offset_SkipsPreviousPages()
    {
        var request = new PageRequest(PageNumber: 3, PageSize: 10);

        Assert.Equal(20, request.Offset);
    }

    [Theory]
    [InlineData(50, 10, 5)]
    [InlineData(51, 10, 6)]
    [InlineData(0, 10, 0)]
    public void TotalPages_IsCalculatedFromTotalRecords(int totalRecords, int pageSize, int expectedPages)
    {
        var page = new PagedResult<string>([], 1, pageSize, totalRecords);

        Assert.Equal(expectedPages, page.TotalPages);
    }
}
