using Mundialito.Domain.Players;

namespace Mundialito.Api.Contracts;

public sealed record AddPlayerRequest(string FullName, int ShirtNumber, PlayerPosition Position);
