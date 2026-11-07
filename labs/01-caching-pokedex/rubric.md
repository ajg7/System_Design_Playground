# Lab 1 Rubric: Caching

> AJ: don't read this before attempting the quiz. It's for the grader.

Score each question 0–2. **2** = interview-ready (correct, concrete, tradeoff named).
**1** = right idea, missing the mechanism or the tradeoff. **0** = wrong or hand-wavy.
Total /16. Lab passes at 12+, with Q2 and Q6 each scoring at least 1.

**Coverage rule:** every question's *Source* must point at `concepts.md` or a lab step.
Only grade against what that source teaches; don't dock points for ideas it doesn't cover.

| # | Source | Full credit requires | Common 1-point answer |
|---|---|---|---|
| 1 | §1, M1.1 baseline | Tail latency is what users and SLOs feel; tails compound across many calls per page. In this lab: 20-connection pool + ~100ms queries → requests queue for a connection, so p99 includes wait time p50 doesn't. | "p99 is the slow ones" with no mechanism |
| 2 | §3, M1.2 | Picks delete. Constructs the overwrite race: reader reads old from DB → writer updates DB + cache → reader writes old into cache → stale until TTL. Notes delete narrows but doesn't fully remove the race, so TTL is the backstop. | Picks delete, can't construct the race |
| 3 | §3 | DB first, then delete. Walks through delete-first: a reader in the gap reloads the old row into cache. Mentions a failed delete leaves staleness until TTL (retry or change-log invalidation). | Correct order, no reader walkthrough |
| 4 | §4, M1.2 | Without negative caching, missing ids always hit the DB (cache penetration). Fix: cache a not-found marker with a short TTL. Downside: a newly created species is invisible until it expires. Bonus: random ids still get through, so validate input or use a Bloom filter. | Mentions negative caching, no downside |
| 5 | §5 | TTL is a safety net for missed invalidations; a week means a bug stays wrong for a week. No TTL is safe only with reliable invalidation on every write path *and* an eviction policy like allkeys-lru. | "Data could get stale" only |
| 6 | §6, M1.3 break-it | Explains cold key + N concurrent misses → N DB reads → pool saturation → p99 explosion, tied to their actual numbers. Names three mitigations from §6 *and* when each is the right call. | Lists mitigations without when-to-use |
| 7 | §7, M1.3 step 6 | ~one DB read per instance (~20): bounded by instance count, usually fine. If not: distributed lock/lease, pre-warm, or an L1 in-process cache in front of Redis. | Says "1" (wrong) or "20" with no next step |
| 8 | §8, M1.3 step 7 | Reports what they observed when Redis stopped (their code's actual behavior). Should fail open: treat as a miss and read the DB, *and* protect the DB from the resulting full-traffic miss storm (single-flight, concurrency limit, or circuit breaker). Names that tradeoff. | "Fall back to DB" without the DB-protection risk |

## Grader notes
- Probe Q8 even on a full-credit answer: "what protects Postgres when you fall through?"
- Killer question if total is 12+ (answerable from §1, §6–§8): *"Your fail-open code is
  live on 20 instances. Redis dies one second into the Mewtwo reveal. Walk me through what
  Postgres sees over the next few seconds, and the one change that protects it most."*
