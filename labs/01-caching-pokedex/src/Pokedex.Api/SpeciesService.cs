using System.Collections.Concurrent;
using System.Text.Json;
using StackExchange.Redis;

namespace Pokedex.Api;

// ============================================================
//  REFERENCE SOLUTION (solutions branch). Lab 1.
// ============================================================
public sealed class SpeciesService(ISpeciesRepository repo, IDatabase cache, CacheOptions options)
{
    // Negative caching: unknown ids are cached briefly so they can't bypass the cache,
    // but short enough that a newly added species shows up soon.
    const string NotFound = "__not_found__";
    static readonly TimeSpan NotFoundTtl = TimeSpan.FromSeconds(30);

    // Single-flight: one in-progress load per id, shared by every concurrent caller in this process.
    readonly ConcurrentDictionary<int, Lazy<Task<Species?>>> _inflight = new();

    public static string Key(int id) => $"species:{id}";

    public async Task<Species?> GetAsync(int id, CancellationToken ct = default)
    {
        var hit = await TryReadCacheAsync(id);
        if (hit.HasValue) return hit == NotFound ? null : JsonSerializer.Deserialize<Species>(hit.ToString());

        var load = _inflight.GetOrAdd(id, _ => new Lazy<Task<Species?>>(() => LoadAndPopulateAsync(id)));
        try
        {
            // WaitAsync: one caller cancelling must not cancel the shared load for everyone else.
            return await load.Value.WaitAsync(ct);
        }
        finally
        {
            // Only removes this exact load; a newer one for the same id is left alone.
            _inflight.TryRemove(new KeyValuePair<int, Lazy<Task<Species?>>>(id, load));
        }
    }

    public async Task UpdateAsync(Species species, CancellationToken ct = default)
    {
        // DB first, then invalidate. Invalidating first lets a concurrent reader re-cache the old row.
        // Delete rather than overwrite, so a slow reader can't write a stale value over the new one.
        await repo.UpdateAsync(species, ct);
        await cache.KeyDeleteAsync(Key(species.Id));
    }

    async Task<Species?> LoadAndPopulateAsync(int id)
    {
        var species = await repo.GetAsync(id, CancellationToken.None);
        try
        {
            await (species is null
                ? cache.StringSetAsync(Key(id), NotFound, NotFoundTtl)
                : cache.StringSetAsync(Key(id), JsonSerializer.Serialize(species), options.Ttl));
        }
        catch (RedisException) { /* fail open: serve from the DB even if the cache write fails */ }
        return species;
    }

    async Task<RedisValue> TryReadCacheAsync(int id)
    {
        // Fail open: if Redis is down, treat it as a miss. Single-flight still caps DB load per id.
        try { return await cache.StringGetAsync(Key(id)); }
        catch (RedisException) { return RedisValue.Null; }
    }
}
