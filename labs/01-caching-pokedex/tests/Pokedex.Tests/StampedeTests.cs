using Pokedex.Api;

namespace Pokedex.Tests;

[Collection("redis")]
[Trait("Milestone", "1.3")]
public sealed class StampedeTests(RedisFixture redis)
{
    [Fact]
    public async Task Concurrent_cold_reads_for_one_species_hit_the_db_once()
    {
        var db = await redis.FreshDatabaseAsync();
        var repo = new CountingRepository(TimeSpan.FromMilliseconds(200));
        var svc = new SpeciesService(repo, db, new CacheOptions(TimeSpan.FromMinutes(1)));

        // The legendary reveal: 200 trainers look up Mewtwo at the same instant, cache cold.
        var results = await Task.WhenAll(Enumerable.Range(0, 200).Select(_ => Task.Run(() => svc.GetAsync(150))));

        Assert.All(results, s => Assert.Equal("Mewtwo", s?.Name));
        Assert.Equal(1, repo.Reads);
    }

    [Fact]
    public async Task Coalescing_does_not_merge_different_species()
    {
        var db = await redis.FreshDatabaseAsync();
        var repo = new CountingRepository(TimeSpan.FromMilliseconds(200));
        var svc = new SpeciesService(repo, db, new CacheOptions(TimeSpan.FromMinutes(1)));

        var results = await Task.WhenAll(svc.GetAsync(25), svc.GetAsync(150));

        Assert.Equal("Pikachu", results[0]?.Name);
        Assert.Equal("Mewtwo", results[1]?.Name);
        Assert.Equal(2, repo.Reads);
    }
}
