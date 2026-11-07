using StackExchange.Redis;

namespace Pokedex.Api;

// ============================================================
//  THIS IS YOUR FILE. Lab 1 is implemented here.
//  Run the milestone's tests:  dotnet test --filter Milestone=1.2
// ============================================================
public sealed class SpeciesService(ISpeciesRepository repo, IDatabase cache, CacheOptions options)
{
    public static string Key(int id) => $"species:{id}";

    public async Task<Species?> GetAsync(int id, CancellationToken ct = default)
    {
        // TODO(1.2): cache-aside.
        //   Look in Redis first. On a miss, load from the repo, then populate the cache
        //   with options.Ttl. Decide what to do when the species doesn't exist.
        //
        // TODO(1.3): coalesce concurrent misses.
        //   When many requests miss on the same id at the same moment, only ONE should
        //   reach the repo; the rest wait for its result.
        return await repo.GetAsync(id, ct);
    }

    public async Task UpdateAsync(Species species, CancellationToken ct = default)
    {
        await repo.UpdateAsync(species, ct);
        // TODO(1.2): make sure the next read can't return the old version.
        //   Invalidate or update in place? Be ready to defend your choice (see quiz.md).
    }
}
