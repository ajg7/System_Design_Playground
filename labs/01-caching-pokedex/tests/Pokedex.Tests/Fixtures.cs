using System.Collections.Concurrent;
using Pokedex.Api;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace Pokedex.Tests;

// One real Redis (via Docker) for the whole test run.
public sealed class RedisFixture : IAsyncLifetime
{
    readonly RedisContainer _container = new RedisBuilder("redis:7-alpine").Build();
    public ConnectionMultiplexer Mux { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        Mux = await ConnectionMultiplexer.ConnectAsync(_container.GetConnectionString() + ",allowAdmin=true");
    }

    public async Task DisposeAsync()
    {
        await Mux.DisposeAsync();
        await _container.DisposeAsync();
    }

    public async Task<IDatabase> FreshDatabaseAsync()
    {
        await Mux.GetServers()[0].FlushDatabaseAsync();
        return Mux.GetDatabase();
    }
}

[CollectionDefinition("redis")]
public sealed class RedisCollection : ICollectionFixture<RedisFixture>;

// Stands in for the slow Pokédex DB and counts every round trip.
public sealed class CountingRepository(TimeSpan latency) : ISpeciesRepository
{
    readonly ConcurrentDictionary<int, Species> _rows = new()
    {
        [25] = new(25, "Pikachu", "Electric", null, false),
        [150] = new(150, "Mewtwo", "Psychic", null, true),
    };
    int _reads;
    public int Reads => Volatile.Read(ref _reads);

    public async Task<Species?> GetAsync(int id, CancellationToken ct = default)
    {
        Interlocked.Increment(ref _reads);
        await Task.Delay(latency, ct);
        return _rows.GetValueOrDefault(id);
    }

    public Task UpdateAsync(Species s, CancellationToken ct = default)
    {
        _rows[s.Id] = s;
        return Task.CompletedTask;
    }
}
