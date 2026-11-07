using Npgsql;
using Pokedex.Api;
using Prometheus;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

builder.Services.AddSingleton(NpgsqlDataSource.Create(cfg.GetConnectionString("Postgres")!));
builder.Services.AddSingleton<ISpeciesRepository, PostgresSpeciesRepository>();
builder.Services.AddSingleton(ConnectionMultiplexer.Connect(cfg.GetConnectionString("Redis")!).GetDatabase());
builder.Services.AddSingleton(new CacheOptions(TimeSpan.FromSeconds(cfg.GetValue("Cache:TtlSeconds", 60))));
builder.Services.AddSingleton<SpeciesService>();

var app = builder.Build();
app.UseHttpMetrics();

app.MapGet("/species/{id:int}", async (int id, SpeciesService svc, CancellationToken ct) =>
    await svc.GetAsync(id, ct) is { } s ? Results.Ok(s) : Results.NotFound());

app.MapPut("/species/{id:int}", async (int id, Species body, SpeciesService svc, CancellationToken ct) =>
{
    await svc.UpdateAsync(body with { Id = id }, ct);
    return Results.NoContent();
});

app.MapGet("/health", () => "ok");
app.MapMetrics();
app.Run();

public partial class Program;
