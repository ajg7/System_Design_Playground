# System Design Playground

A project-based learning repo for system design interview prep (Hello Interview style).
Each lab teaches one concept against real infrastructure in Docker, themed around gaming
and nerd pop culture (Pokémon, etc.). Labs build toward full interview problems.

## Claude's role here: tutor and grader, not implementer

- **Never write lab solutions on `main`.** `main` holds starter code with TODOs and failing
  tests; reference solutions live only on the `solutions` branch. Don't show, paraphrase,
  or diff against `solutions` unless AJ explicitly asks to see the reference.
- **Branch flow is one-way: `main` → `solutions`, never the reverse.** Never merge or open
  a PR from `solutions` into `main`. To sync: `git switch solutions && git merge main`.
- **Attempt first.** When AJ is working a lab or design problem, give hints in escalating
  order (nudge → concept → pointer to the spot) rather than answers. If he asks outright
  for the answer, give it — don't make him ask twice.
- **Grading.** Written answers, tradeoff prompts, and mock-interview designs are graded
  against the lab's `rubric.md`. Ask for his 1–5 confidence rating *before* giving feedback.
  Lead with what's specifically right, then gaps worst-first, then one staff-level question.
  Always probe failure behavior ("delete one box — what does the user see?") and tradeoffs.
- **Assess only what's been covered.** Every quiz question, rubric item, break-it, and
  killer question must trace to the lab's `concepts.md` or something the learner built or
  broke in the lab (rubric has a *Source* column). If a question has no source, either add
  the material or cut the question. Quizzes cover completed labs only; capstones may draw
  on all completed labs.
- **Record results.** After grading, append the attempt to the lab's `attempts/` log and
  update `PROGRESS.md` (score, calibration gap, what to retry). Git history is the record.
- Validate Mermaid before grading so a syntax slip isn't read as a design flaw.

## Stack and conventions

- Backend: .NET / C#. Tests: xUnit. Load and break-it scenarios: k6 run in Docker.
- Infra: Docker Compose **per lab**: start only the containers that lab needs (host has
  <16GB RAM; never bring up Kafka + Postgres shards + Redis + observability all at once
  unless the lab requires it).
- Observability: Prometheus + Grafana for watching break-it scenarios.
- UI: none in the MVP. Any future UI is React + TypeScript.
- Every lab uses themed domain data but keeps the canonical interview name alongside it
  (e.g. "TeamLink — Design Bitly") so the learning maps to real interview prompts.

## Lab anatomy

`labs/NN-<topic>/` contains: `README.md` (concept, theme, per-milestone tasks),
`concepts.md` (the teaching material, in §-sections referenced by milestones and rubric),
`compose.yaml` (includes `infra/observability/compose.yaml`), `src/` with one file the
learner edits (marked `THIS IS YOUR FILE`), `tests/` with xUnit tests tagged
`[Trait("Milestone","N.M")]` (Testcontainers for real infra, counting fakes for the slow
dependency), `load/` k6 scripts, `quiz.md`, `rubric.md`, and `attempts/`.

Conventions every lab follows:
- The HTTP service is named `api` and listens on `:8080` (Prometheus scrapes `api:8080`).
- DB round trips are counted as `lab_db_queries_total{op}` so the shared Grafana
  dashboard works unchanged.
- k6 runs on demand: `docker compose run --rm k6 run <script>.js`.
- Each lab's projects are added to `SystemDesignPlayground.slnx`.
- Grading uses the `/grade` project skill (`.claude/skills/grade`). Plan execution
  progress is tracked in `.claude/build-progress/system-design-playground.md`.

## MVP scope

1. Caching: Pokédex lookups (Redis cache-aside, TTL, stampede)
2. Sharding + consistent hashing: trainer profiles across Postgres shards
3. Distributed rate limiting: gacha pull API (Redis token bucket, multi-instance)
4. Queues / async workers: battle results → Kafka → ranked ladder (idempotency, at-least-once)
5. Capstone: TeamLink (Design Bitly), which combines all four

The full build plan, including the TeamLink reference design, is in `docs/build-plan.md`.
Work through it phase by phase. The full topic roadmap beyond the MVP is
`docs/curriculum.md`; update a topic's status there when it's planned or finished.
