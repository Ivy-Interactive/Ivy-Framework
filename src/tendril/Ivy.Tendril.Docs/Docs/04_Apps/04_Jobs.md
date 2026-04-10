---
searchHints:
  - jobs
  - running
  - execution
  - agents
  - status
icon: Activity
---

# Jobs

<Ingress>
Running and past promptware runs: status, cost, duration, and live output.
</Ingress>

## Overview

Any dispatched promptware (Execute from Drafts, Revise from Review, …) shows up as a job with:

- **Status** — `Running`, `Completed`, `Failed`, `Pending`, …
- **Type** — e.g. `MakePlan`, `ExecutePlan`, `UpdatePlan`, `MakePr`
- **Tokens** — Usage vs. your provider quota

## Live output

Built-in terminal shows **stdout/stderr** from the agent (builds, logs, errors)—not only a spinner.

## Controls

| Action | Effect |
|--------|--------|
| **Stop** | End the run; release worktree locks. |
| **Logs** | Open `logs/` for that plan. |
| **Retry** | Re-run when a transition is stuck. |
