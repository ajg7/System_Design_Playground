# Lab 1 Rubric: Caching

> AJ: don't read this before attempting the quiz. It's for the grader.

Score each question 0–2. **2** = interview-ready (correct, concrete, tradeoff named).
**1** = right idea, missing the mechanism or the tradeoff. **0** = wrong or hand-wavy.
Total /16. Lab passes at 12+, with Q2 and Q6 each scoring at least 1.

| # | Full credit requires | Common 1-point answer |
|---|---|---|
| 1 | Tail latency is what users and SLOs feel; fan-out makes tails compound. In this lab: 20-connection pool + 100ms queries → requests queue for connections, so p99 includes wait time p50 doesn't. | "p99 is the slow ones" with no mechanism |
| 2 | Picks invalidate (delete). Race for update-in-place: A reads old from DB, B writes new to DB + cache, A writes stale old value into cache → stale until TTL. Delete makes the next reader reload. Names that delete still has a small race (read-before-delete repopulates stale) bounded by TTL. | Picks delete, can't construct the race |
| 3 | DB first, then invalidate. Invalidate-first lets a reader reload the *old* DB row into cache between delete and DB write. Mentions retry/at-least-once on the delete, or delayed double-delete. | Correct order, no reader walkthrough |
| 4 | Without negative caching, every miss on a nonexistent id hits the DB, so attackers can bypass the cache. Fix: cache a sentinel "not found" with a short TTL. Downside: a species created later stays invisible until that TTL expires. | Mentions negative caching, no downside |
| 5 | TTL is a safety net for invalidation bugs and missed deletes; a week of stale data on a bug is the cost. No TTL is safe only if every write path reliably invalidates (single writer, or change-data-capture) and memory eviction policy is set (e.g. allkeys-lru). | "Data could get stale" only |
| 6 | Explains cold key + N concurrent misses → N DB reads, pool saturation, p99 explosion. Three of: request coalescing/single-flight (per-process, simplest), distributed lock on the key (cross-instance, adds latency + lock failure modes), early/probabilistic refresh before expiry (hot keys stay warm), jittered TTLs (avoid synchronized mass expiry), pre-warming before a known event. Matches each to a situation. | Lists mitigations without when-to-use |
| 7 | ~20 reads (one per instance): bounded by instance count, usually fine. If not: distributed lock / lease on the key, pre-warm the key before the reveal, or a local L1 in front of Redis. Connects to TeamLink's hot-key problem. | Says "1" (wrong) or "20" without a next step |
| 8 | With naive code, Redis exceptions bubble up and the endpoint 500s even though the DB is fine. Should: catch cache failures and fall through to DB (fail open), *with* a circuit breaker or concurrency limit so the DB isn't stampeded by 100% miss traffic. Names that tradeoff. | "Fall back to DB" without the stampede risk |

## Grader notes
- Probe Q8 even on a full-credit answer: "what protects Postgres when you fall through?"
- Killer question if the total is 12+: *"The species table is now 500GB and hot keys
  change hourly. Does cache-aside still work, and what would you change first?"*
