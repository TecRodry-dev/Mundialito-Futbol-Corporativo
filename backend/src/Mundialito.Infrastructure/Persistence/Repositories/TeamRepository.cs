using Microsoft.EntityFrameworkCore;
using Mundialito.Application.Abstractions.Persistence;
using Mundialito.Domain.Teams;

namespace Mundialito.Infrastructure.Persistence.Repositories;

internal sealed class TeamRepository(MundialitoDbContext dbContext) : ITeamRepository
{
    public Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Teams.FirstOrDefaultAsync(team => team.Id == id, cancellationToken);

    public Task<Team?> GetByIdWithPlayersAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Teams
            .Include(team => team.Players)
            .FirstOrDefaultAsync(team => team.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Teams.AnyAsync(team => team.Id == id, cancellationToken);

    public Task<bool> NameExistsAsync(string name, Guid? excludingTeamId, CancellationToken cancellationToken) =>
        dbContext.Teams.AnyAsync(
            team => team.Name == name && (excludingTeamId == null || team.Id != excludingTeamId),
            cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, Guid>> GetTeamIdsByPlayerIdsAsync(
        IReadOnlyCollection<Guid> playerIds,
        CancellationToken cancellationToken)
    {
        if (playerIds.Count == 0)
        {
            return new Dictionary<Guid, Guid>();
        }

        return await dbContext.Players
            .AsNoTracking()
            .Where(player => playerIds.Contains(player.Id))
            .Select(player => new { player.Id, player.TeamId })
            .ToDictionaryAsync(player => player.Id, player => player.TeamId, cancellationToken);
    }

    public void Add(Team team) => dbContext.Teams.Add(team);

    public void Remove(Team team) => dbContext.Teams.Remove(team);
}
