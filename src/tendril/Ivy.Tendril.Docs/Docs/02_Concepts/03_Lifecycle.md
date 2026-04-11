---
icon: RefreshCw
searchHints:
  - jobs
  - execution
  - running
  - cost
  - tokens
  - monitoring
  - verification
---

# Lifecycle & Jobs

<Ingress>
Each promptware run is a **Job**: live status, cost, and telemetry for that agent.
</Ingress>

## What you see per job

- **Status** — `Pending`, `Running`, `Completed`, `Failed`, `Timeout`, `Queued`, `Stopped`, `Blocked`
- **Type** — e.g. `MakePlan`, `ExecutePlan`, `MakePr`
- **Plan** — Which plan and branch context
- **Cost** — Tokens and estimated spend
- **Duration** — Wall time
- **Output** — Streamed messages and command output

## Verification

After `ExecutePlan` / `UpdatePlan`, Tendril runs your **Verification** steps (not guesswork):

1. **Build** — e.g. `dotnet build`, `npm run typecheck`. Failures go back to the agent with logs.
2. **Format** — e.g. `dotnet format`, `prettier`.
3. **Tests** — Unit (or configured) test commands.

Too many failures vs. limits – plan **Failed**; nothing broken moves to Review by default.

## Concurrency & worktrees

Parallel job slots are configurable. Execution uses a **git worktree** (separate working tree), not your current branch—so several agents can run without fighting your editor.

## Cost

Each job appends to the plan’s `costs.csv`. The UI aggregates by project, time, and promptware type.
