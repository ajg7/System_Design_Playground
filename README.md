# System Design Playground

Learn system design by building, breaking, and defending it. Each lab is one interview
concept against real infrastructure in Docker, themed on gaming and pop culture, and
assessed four ways: failing tests, a break-it scenario you predict before running, a
graded quiz, and (for capstones) a mock interview.

## Requirements
.NET 10 SDK and Docker Desktop. Each lab uses a few hundred MB; run one lab at a time.

## Layout
```
labs/NN-<topic>/        one lab: README, compose.yaml, src/, tests/, load/, quiz.md, rubric.md, attempts/
infra/observability/    Prometheus + Grafana, included by every lab's compose file
docs/build-plan.md      phases, milestones, checkpoints, TeamLink reference design
docs/curriculum.md      every topic, MVP and beyond
PROGRESS.md             scores and calibration, updated by /grade
```

## Working a lab
1. `cd labs/01-caching-pokedex` and follow its README.
2. Make the tests green: `dotnet test --filter Milestone=1.2`
3. Predict, then run the break-it. Log both in `attempts/predictions.md`.
4. Answer `quiz.md` in `attempts/`, then run `/grade lab 1` in Claude Code.

Reference solutions live on the `solutions` branch. Look only after you're done.
