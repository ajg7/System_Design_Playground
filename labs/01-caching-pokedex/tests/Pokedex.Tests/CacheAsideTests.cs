using Pokedex.Api;

namespace Pokedex.Tests;

[Collection("redis")]
[Trait("Milestone", "1.2")]
public sealed class CacheAsideTests(RedisFixture redis)
{
    static readonly TimeSpan Slow = TimeSpan.FromMilliseconds(20);

    async Task<(SpeciesService svc, CountingRepository repo, StackExchange.Redis.IDatabase db)> Setup(TimeSpan ttl)
    {
        var db = await redis.FreshDatabaseAsync();
        var repo = new CountingRepository(Slow);
        return (new SpeciesService(repo, db, new CacheOptions(ttl)), repo, db);
    }

    [Fact]
    public async Task Second_read_is_served_from_cache()
    {
        var (svc, repo, _) = await Setup(TimeSpan.FromMinutes(1));

        var first = await svc.GetAsync(25);
        var second = await svc.GetAsync(25);

        Assert.Equal("Pikachu", first?.Name);
        Assert.Equal(first, second);
        Assert.Equal(1, repo.Reads);
    }

    [Fact]
    public async Task Cached_entry_carries_the_configured_ttl()
    {
        var ttl = TimeSpan.FromMinutes(1);
        var (svc, _, db) = await Setup(ttl);

        await svc.GetAsync(25);
        var remaining = await db.KeyTimeToLiveAsync(SpeciesService.Key(25));

        Assert.NotNull(remaining);
        Assert.InRange(remaining.Value, TimeSpan.Zero, ttl);
    }

    [Fact]
    public async Task Entry_expires_and_is_reloaded()
    {
        var (svc, repo, _) = await Setup(TimeSpan.FromSeconds(1));

        await svc.GetAsync(25);
        await svc.GetAsync(25);
        Assert.Equal(1, repo.Reads); // still fresh: served from cache

        await Task.Delay(TimeSpan.FromSeconds(1.5));
        await svc.GetAsync(25);
        Assert.Equal(2, repo.Reads); // expired: reloaded
    }

    // Regression guard: this one is green before you start. Keep it green once caching exists.
    [Fact]
    public async Task Update_is_visible_on_the_next_read()
    {
        var (svc, _, _) = await Setup(TimeSpan.FromMinutes(1));

        await svc.GetAsync(25);
        await svc.UpdateAsync(new Species(25, "Pikachu (Gigantamax)", "Electric", null, false));
        var after = await svc.GetAsync(25);

        Assert.Equal("Pikachu (Gigantamax)", after?.Name);
    }

    // Regression guard as well: unknown ids must still come back as null.
    [Fact]
    public async Task Unknown_species_returns_null()
    {
        var (svc, _, _) = await Setup(TimeSpan.FromMinutes(1));

        Assert.Null(await svc.GetAsync(9999));
    }
}
