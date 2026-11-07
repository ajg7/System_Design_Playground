namespace Pokedex.Api;

public sealed record Species(int Id, string Name, string Type1, string? Type2, bool IsLegendary);

public sealed record CacheOptions(TimeSpan Ttl);
