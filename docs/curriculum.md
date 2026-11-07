# Curriculum Roadmap

The full topic map for the playground. The MVP (marked **MVP**) is planned in detail in
`build-plan.md`; everything else gets planned in batches after the lab format is proven.
Every lab keeps its canonical interview name next to the themed one.

Status: `MVP` = in build plan · `next` = candidate for the next batch · `later` = parked

## Core concepts
| Topic | Themed lab | Status |
|---|---|---|
| Caching | Pokédex lookups: cache-aside, TTL, stampede | MVP |
| Sharding + consistent hashing | Trainer profiles across Postgres shards | MVP |
| Replication + read-your-writes | Guild roster: edit, then read a stale replica | next |
| CAP / consistency models | Card marketplace during a network partition | next |
| Database indexing | Match-history queries, with and without indexes; read query plans | next |
| Data modeling (SQL vs NoSQL) | Raid inventory: Postgres vs Cassandra/DynamoDB Local | later |
| API design + networking | Game lobby: REST vs gRPC vs WebSocket; pagination | later |
| Numbers to know | Napkin-math drills (quiz only, no code) | next |

## Patterns
| Topic | Themed lab | Status |
|---|---|---|
| Rate limiting | Gacha pull API: distributed token bucket | MVP |
| Queues / async workers | Battle results → Kafka → ranked ladder | MVP |
| Real-time updates | Live tournament bracket: SSE/WebSocket + Redis pub/sub | next |
| Dealing with contention | Limited-drop card sale: locks vs optimistic concurrency | next |
| Multi-step processes / sagas | Player-to-player trade: escrow, compensation, retries | later |
| Scaling reads | Game wiki: read replicas + CDN pattern | later |
| Scaling writes | In-game event telemetry spike: batching, write-behind | later |
| Large blobs | Speedrun video upload: presigned URLs, chunking (MinIO) | later |
| Long-running tasks | Replay rendering: job queue, status polling, timeouts | later |
| Search | Fuzzy card search (Elasticsearch) | later |
| Leader election / coordination | Raid-boss scheduler: exactly one spawner | later |

## Capstone problems
| Problem | Themed as | Reuses | Status |
|---|---|---|---|
| Design Bitly | TeamLink: shareable team-build links | cache, shard, rate limit, queue | MVP |
| Leaderboard / Top-K | Global ranked ladder | queue, cache, sorted sets | next |
| Ticketmaster | Tournament ticket drop | contention, rate limit, queue | next |
| WhatsApp | Guild chat | real-time, replication, queue | later |
| News feed | Trainer activity feed | fan-out, cache, shard | later |
| YouTube | Speedrun VOD hosting | blobs, long-running tasks, CDN | later |
| Dropbox | Save-file sync across devices | blobs, contention, real-time | later |
| Ad-click aggregator | Stream click aggregation | queue, scaling writes | later |

## Constraints
- Host has <16GB RAM: heavy stream processors (Flink) are replaced by plain .NET
  consumers; labs start only the containers they need.
