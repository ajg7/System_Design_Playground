# System Design Playground: Progress

**Plan:** docs/build-plan.md
**Last touched:** 2026-11-07

## Phase 1: Harness + Lab 1, Pokédex Caching  [in progress]
- [>] 1.1 The harness runs (autopilot): harness built and verified; checkpoint (AJ runs baseline, logs p50/p99) pending
- [ ] 1.2 Cache-aside makes the tests green
- [ ] 1.3 The legendary reveal stampede

## Scaffolding written by Claude
- Solution, .gitignore, Lab 1 projects, Dockerfile, compose, seed SQL, k6 scripts (1.1)
- infra/observability (Prometheus, Grafana, dashboard) (1.1)
- Lab 1 tests, README, quiz, rubric, predictions template (1.1)
- .claude/skills/grade, PROGRESS.md (1.1)

## Deviations from the plan
- None.

## Carryover
- `solutions` branch pushed with the Lab 1 reference (7/7 green). Keep it in sync with
  `git switch solutions && git merge main` whenever main gains harness changes.
- Ports bound to 127.0.0.1 after a security review; keep that for every new lab.
