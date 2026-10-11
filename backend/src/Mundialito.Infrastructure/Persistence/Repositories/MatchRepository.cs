using Microsoft.EntityFrameworkCore;
using Mundialito.Application.Abstractions.Persistence;
using Mundialito.Domain.Matches;

namespace Mundialito.Infrastructure.Persistence.Repositories;

internal sealed class MatchRepository(MundialitoDbContext dbContext) : IMatchRepository
{
    public Task<Match?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Matches
            .Include(match => match.Goals)
            .FirstOrDefaultAsync(match => match.Id == id, cancellationToken);

    public Task<bool> AnyForTeamAsync(Guid teamId, CancellationToken cancellationToken) =>
        dbContext.Matches.AnyAsync(
            match => match.HomeTeamId == teamId || match.AwayTeamId == teamId,
            cancellationToken);

    public void Add(Match match) => dbContext.Matches.Add(match);
}
