using Mundialito.Application.Abstractions.Messaging;
using Mundialito.Domain.Common;
using Mundialito.Domain.Matches;

namespace Mundialito.Application.Matches;

public sealed record GetMatchByIdQuery(Guid Id) : IQuery<MatchDetailResponse>;

public sealed class GetMatchByIdQueryHandler(IMatchReadService matchReadService)
    : IQueryHandler<GetMatchByIdQuery, MatchDetailResponse>
{
    public async Task<Result<MatchDetailResponse>> Handle(GetMatchByIdQuery query, CancellationToken cancellationToken)
    {
        var match = await matchReadService.GetByIdAsync(query.Id, cancellationToken);

        return match is null
            ? Result.Failure<MatchDetailResponse>(MatchErrors.NotFound)
            : Result.Success(match);
    }
}
