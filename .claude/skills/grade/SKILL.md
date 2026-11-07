---
name: grade
description: Grade AJ's lab quiz answers, break-it predictions, or mock-interview designs against the lab's rubric.md, then log the attempt and update PROGRESS.md. Use when AJ says "/grade", "grade lab N", "grade my quiz", "grade my design", or submits answers for a lab.
---

# Grade

Grading is the assessment half of the playground. Be a staff-level interviewer: fair,
specific, and hard to fool.

## 1. Find what's being graded
- Lab number from the args or message → `labs/NN-*/`.
- Answers: the newest `attempts/quiz-*.md` in that lab, or what AJ pasted. For a design,
  his Mermaid. For break-it, `attempts/predictions.md`.
- If there's nothing to grade yet, say so and stop.

## 2. Confidence first
Ask for a 1–5 confidence rating **before** any feedback, unless he already gave one.

## 3. Grade
- Read `rubric.md`. Score each item exactly as the rubric defines, with no curve.
- Validate Mermaid first (Mermaid Chart connector if available). Syntax is a footnote.
- Feedback order: what's specifically right → gaps, worst first → the rubric's killer
  question (or one of your own). **Make him answer the killer question before you do.**
- Always probe failure ("delete one box") and tradeoffs ("what lost, and why?").
- Never reveal rubric text verbatim for an item he missed; explain the idea.
- Don't reference the `solutions` branch.

## 4. Log it
Write `labs/NN-*/attempts/graded-<YYYY-MM-DD>-<n>.md`:

```markdown
# Lab N: <quiz|design|break-it>, attempt <n>
**Date:** <date>  **Confidence:** <1–5>  **Score:** <x>/<max>  **Calibration:** <one line>
| # | Score | Note |
|---|---|---|
**Killer question:** <q>, **his answer:** <summary>
**Retry next:** <the 1–2 ideas to re-study>
```

Then update the lab's row in `PROGRESS.md` (best score, attempt count, last calibration
gap, retry topics). Don't commit unless AJ asks.
