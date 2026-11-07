using Npgsql;
using Prometheus;

namespace Pokedex.Api;

public interface ISpeciesRepository
{
    Task<Species?> GetAsync(int id, CancellationToken ct = default);
    Task UpdateAsync(Species species, CancellationToken ct = default);
}

// The "slow Pokédex database". Not part of the lab: you shouldn't need to edit this file.
public sealed class PostgresSpeciesRepository(NpgsqlDataSource db) : ISpeciesRepository
{
    // Shared convention across labs: every lab counts its DB round trips under this name.
    static readonly Counter Queries = Metrics.CreateCounter("lab_db_queries_total", "DB round trips", "op");

    public async Task<Species?> GetAsync(int id, CancellationToken ct = default)
    {
        Queries.WithLabels("read").Inc();
        // pg_sleep makes the database itself slow, so a stampede really holds connections.
        await using var cmd = db.CreateCommand(
            "SELECT id, name, type1, type2, is_legendary FROM species CROSS JOIN pg_sleep(0.1) WHERE id = $1");
        cmd.Parameters.AddWithValue(id);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        if (!await r.ReadAsync(ct)) return null;
        return new Species(r.GetInt32(0), r.GetString(1), r.GetString(2),
            r.IsDBNull(3) ? null : r.GetString(3), r.GetBoolean(4));
    }

    public async Task UpdateAsync(Species s, CancellationToken ct = default)
    {
        Queries.WithLabels("write").Inc();
        await using var cmd = db.CreateCommand(
            "UPDATE species SET name = $2, type1 = $3, type2 = $4, is_legendary = $5 WHERE id = $1");
        cmd.Parameters.AddWithValue(s.Id);
        cmd.Parameters.AddWithValue(s.Name);
        cmd.Parameters.AddWithValue(s.Type1);
        cmd.Parameters.AddWithValue((object?)s.Type2 ?? DBNull.Value);
        cmd.Parameters.AddWithValue(s.IsLegendary);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
