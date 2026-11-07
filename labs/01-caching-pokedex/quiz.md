# Lab 1 Quiz: Caching

Answer in `attempts/quiz-<n>.md` without looking at the rubric. Write the way you'd say it
in an interview: short, concrete, with the tradeoff named. Then run `/grade lab 1`.

1. **Measure first.** Your baseline p50 is fine but p99 is ugly. Why is p99 the number an
   interviewer cares about, and what in *this* lab's setup makes p99 diverge from p50?

2. **Invalidate vs. update.** On `UpdateAsync`, you can delete the cache key or write the
   new value into it. Which did you choose? Describe the specific race condition the other
   choice opens, with two concurrent requests, step by step.

3. **Order of operations.** You update the DB and invalidate the cache. Does it matter which
   happens first? Walk through what a concurrent reader sees in each ordering.

4. **Missing species.** What does your cache do for an id that doesn't exist? What attack or
   failure does that choice expose the database to, and what's the fix's own downside?

5. **TTL.** A species entry almost never changes. Why not set the TTL to a week? What would
   you need to be true to safely set *no* TTL at all?

6. **Stampede.** Explain what happened in your 1.3 prediction vs. actual. Name three
   mitigations for a stampede and when each one is the right call.

7. **Your coalescing is per-process.** In production there are 20 API instances. How many
   DB reads does the legendary reveal cause now, and what would you do if that's still too
   many?

8. **Delete one box.** Redis goes down mid-reveal. What does a trainer see with your code,
   and what *should* they see? Fail open or fail closed?
