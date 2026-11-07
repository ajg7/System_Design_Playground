# Lab 1 Concepts: Caching

Read each section when its milestone tells you to. Everything the quiz asks is here or in
something you build or break in the lab.

---

## §1 Measuring latency: p50 vs p99  *(Milestone 1.1)*

- **p50** (median): half of requests are faster than this. It's the "typical" experience.
- **p99**: 1 in 100 requests is slower than this. It's the experience of your unluckiest users,
  and at scale "1 in 100" is thousands of people per minute.
- Interviewers and SLOs care about **tail latency** (p99, p999) because one page load often
  makes many requests. If a page makes 20 calls, most page loads include at least one
  p99-or-worse call. Tails compound.

**Why p99 diverges from p50 here:** the database has a pool of only **20 connections**, and
each query holds one for ~100ms. While fewer than 20 requests are in flight, each one just
pays the query time. Once more than 20 arrive together, the rest **queue for a connection**,
so their latency is query time *plus* waiting time. p50 mostly sees the first case; p99 sees
the queue. This is the general shape of tail latency: **contention for a limited resource.**

---

## §2 Cache-aside  *(Milestone 1.2)*

Also called **lazy loading**. The *application* manages the cache; the cache knows nothing
about the database.

**Read path**
1. Look up the key in the cache.
2. **Hit** → return it.
3. **Miss** → read from the database, write the result into the cache with a TTL, return it.

**Write path**
1. Write to the database (the source of truth).
2. Make sure the cache can't keep serving the old value (see §3).

Why it's the default pattern: only data that's actually requested gets cached, and if the
cache is empty or lost, the system still works (just slower). The cost: the first read
of every key is a miss, and *your code* owns every consistency bug.

Other patterns, for vocabulary: **read-through** (the cache itself loads from the DB),
**write-through** (writes go to cache and DB together), **write-behind** (writes go to the
cache and are flushed to the DB later; fast, but risks losing data).

---

## §3 Invalidation: delete vs. update, and ordering  *(Milestone 1.2)*

On a write you can either **delete** the cache key (the next read reloads it) or
**overwrite** it with the new value. Deleting is the usual choice. Here's why.

**The overwrite race.** Two requests, A (a reader) and B (a writer):
1. A misses the cache and reads Pikachu from the DB: **old** version.
2. B updates Pikachu in the DB to the **new** version.
3. B writes the **new** version into the cache.
4. A, which was slow, now writes the **old** version it read in step 1 into the cache.

The cache now holds stale data, and nothing will fix it until the TTL expires. With
**delete**, step 3 removes the key instead. The next reader goes to the DB and gets the new
version. (Delete still has a narrower version of this race, where A's write lands just after
B's delete. That's one reason TTLs exist; see §5.)

**Ordering: DB first, then delete.**
- **Delete first, then write DB:** between the delete and the DB write, a reader can miss,
  read the *old* row from the DB, and put it back in the cache. Your delete just got undone.
- **Write DB first, then delete:** a reader in the gap gets the old cached value briefly,
  then the delete lands and the next read is fresh. The staleness window is tiny and bounded.

If the delete itself fails (network blip), the cache stays stale until TTL. Real systems
retry the delete or listen to the DB's change log to invalidate.

---

## §4 Missing keys and negative caching  *(Milestone 1.2)*

If the cache only stores things that exist, every request for a **nonexistent** id is a
miss that goes to the database, every time. That's **cache penetration**: anyone can
bypass the cache entirely by requesting `/species/99999`, `/species/99998`, ... and the DB
takes all of it.

**Negative caching:** cache a "not found" marker for the missing id, with a *short* TTL.
- Benefit: repeated lookups for missing ids are absorbed by the cache.
- Downside: if that species gets created, it stays invisible until the marker expires.
  That's why the TTL is short.
- Doesn't help against *random* ids (each is new). For that, you'd validate input
  (ids must be in a known range) or use a Bloom filter of existing keys.

---

## §5 TTLs  *(Milestone 1.2)*

A TTL (time to live) makes a key expire automatically.

- It's a **safety net**: if an invalidation was missed (bug, failed delete, a write path
  someone forgot about), the stale value fixes itself when the TTL expires.
- **Long TTL** = fewer misses, but a missed invalidation stays wrong longer.
  **Short TTL** = fresher, but more DB load.
- **No TTL at all** is only safe if (a) every write path reliably invalidates, e.g. a single
  writer, or invalidation driven by the DB's change log, *and* (b) Redis has an
  **eviction policy** (like `allkeys-lru`: drop least-recently-used keys when memory is
  full), otherwise memory fills up forever.
- Many keys created at the same moment with the same TTL also *expire* at the same moment.
  Adding random **jitter** to TTLs spreads that out (see §6).

---

## §6 Cache stampede  *(Milestone 1.3)*

Also called **thundering herd** or **dogpile**. A popular key is missing (cold cache, or it
just expired), and many requests arrive at once. *Every one* of them misses, and *every one*
goes to the database for the same row. The DB gets N copies of one query at the exact moment
it's busiest, the connection pool saturates (§1), and latency explodes for everyone, not
just the hot key.

Caches fail hardest exactly when they matter most: launches, reveals, viral moments.

**Mitigations, and when to use each:**
| Mitigation | How | Use when |
|---|---|---|
| **Request coalescing / single-flight** | Concurrent misses for the same key in one process share a single DB load; the rest wait for its result | Almost always. Simplest, no extra infrastructure. Works per process. |
| **Distributed lock / lease** | First instance to miss takes a lock in Redis and loads; other instances wait or serve stale | Many instances, and even one-load-per-instance is too much |
| **Early / probabilistic refresh** | Refresh a hot key shortly *before* it expires, so it never goes cold | Known-hot keys with expensive loads |
| **TTL jitter** | Randomize TTLs (e.g. 60s ± 10%) | Many keys were cached together and would expire together |
| **Pre-warming** | Load the key into the cache before the event | The spike is predictable (scheduled reveal, launch) |

---

## §7 Coalescing across many instances  *(Milestone 1.3)*

Single-flight lives in one process's memory. In production there are many API instances
behind a load balancer, and each has its own in-flight table. So for one cold key:

- Without coalescing: up to **one DB read per request**.
- With per-process coalescing: up to **one DB read per instance** (20 instances → ~20 reads).

That's usually fine, since it's bounded by instance count rather than traffic. If it isn't,
use a distributed lock or pre-warm the key (§6). Another option is a small **in-process
(L1) cache** in front of Redis (L2), so hot keys are served from memory without a network
hop. The TeamLink capstone needs exactly this.

---

## §8 When the cache fails  *(Milestone 1.3)*

The cache is a dependency, so it can go down. Two choices:

- **Fail closed:** cache errors become user errors (500s), even though the DB is fine.
  This is what happens by default if your code doesn't catch cache exceptions.
- **Fail open:** treat a cache error as a miss and read from the DB.

Fail open is usually right for a read cache, **but** it means 100% of traffic suddenly hits
the DB: a stampede across *every* key at once. So failing open needs protection for the DB:
single-flight (§6), a **concurrency limit** on DB calls, or a **circuit breaker** (after N
cache failures, stop trying the cache for a while and shed or queue load instead of hammering
both).
