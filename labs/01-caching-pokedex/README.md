# Lab 1: Pokédex Caching

**Interview concept:** Caching: cache-aside, TTL, invalidation, cache stampede.
**Shows up in:** almost every design (Bitly redirects, news feed, Ticketmaster event pages).

## The situation
The Pokédex species database is slow: every lookup costs ~100ms, and it only has 20
connections. Trainers browse constantly, and when a new legendary is revealed, everyone
opens the same page at the same instant. Your job: put Redis in front of it without
ever serving a wrong entry, and without the reveal melting the database.

```
trainer ──GET /species/150──▶ api ──▶ SpeciesService (YOUR CODE) ──▶ Redis
                                                    └──miss──▶ Postgres (slow, 20 conns)
```

## Setup
```powershell
docker compose up -d --build       # Postgres, Redis, API, Prometheus, Grafana
curl localhost:8080/species/150    # Mewtwo, ~100ms+
```
- Grafana: http://localhost:3000 → **Lab overview** (req/s, p50/p99, DB round trips)
- Raw counters: `curl localhost:8080/metrics | Select-String lab_db_queries_total`
- After editing code: `docker compose up -d --build api`
- Done for the day: `docker compose down`

The only file you edit is `src/Pokedex.Api/SpeciesService.cs`.

---

## Milestone 1.1: Measure first
Before touching any code, run the baseline and **write down p50 and p99** in
`attempts/predictions.md`. Every later claim ("the cache made it faster") is measured
against this number.
```powershell
docker compose run --rm k6 run baseline.js
```

## Milestone 1.2: Cache-aside
Implement the `TODO(1.2)` parts of `SpeciesService`.
```powershell
dotnet test --filter Milestone=1.2
```
- Read path: Redis first; on miss, load from the repo and populate with the TTL.
- Write path: the next read after an update must never return the old version.
- Decide what happens for a species that doesn't exist (and be ready to defend it).
- Re-run `baseline.js` and compare to your 1.1 numbers.

Two tests are green from the start (`Update_is_visible...`, `Unknown_species...`).
They're regression guards: caching makes them easy to break.

## Milestone 1.3: The legendary reveal (break-it)
1. Flush the cache: `docker compose exec redis redis-cli flushall`
2. Note the current `lab_db_queries_total{op="read"}`.
3. **Predict** how many DB reads 500 simultaneous requests for Mewtwo will cause with your
   1.2 code, and roughly what p99 will be. Write it in `attempts/predictions.md` *before*
   running.
4. `docker compose run --rm k6 run stampede.js`, then diff the counter. Explain the gap.
5. Implement `TODO(1.3)` (coalesce concurrent misses) until `dotnet test --filter Milestone=1.3` is green.
6. Repeat steps 1–4. Write down what changed, and what *still* isn't solved (hint: how
   many API instances are there in production?).

## Assessment
- Tests: `dotnet test --filter Milestone=1.2` and `1.3`
- Break-it: predictions vs. actuals in `attempts/predictions.md`
- Quiz: answer `quiz.md` in `attempts/`, then ask Claude to grade it (`/grade lab 1`).
  Graded against `rubric.md`; don't read the rubric first.
