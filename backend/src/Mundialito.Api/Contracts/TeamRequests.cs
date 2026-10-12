namespace Mundialito.Api.Contracts;

public sealed record CreateTeamRequest(string Name, string Coach);

/// <summary>PUT: se envían todos los campos.</summary>
public sealed record UpdateTeamRequest(string Name, string Coach);

/// <summary>PATCH: solo los campos que se quieren cambiar; los omitidos se mantienen.</summary>
public sealed record PatchTeamRequest(string? Name, string? Coach);
