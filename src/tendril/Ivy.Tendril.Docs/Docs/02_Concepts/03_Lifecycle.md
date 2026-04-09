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
Jobs represent the tangible, running execution units in Tendril's continuous lifecycle. Every time a promptware runs, a real-time Job is spawned to monitor the agent state, cost, and telemetry.
</Ingress>

## Job Tracking

Each job provides high-fidelity insight into the running Promptware:

- **Status** — `Pending`, `Running`, `Completed`, `Failed`, `Timeout`, `Queued`, `Stopped`, or `Blocked`.
- **Type** — Which promptware job is executing (`MakePlan`, `ExecutePlan`, `MakePr`, etc.).
- **Associated Plan** — A link backing the job to its architectural Plan and branch context.
- **Cost Analytics** — Real-time tracking of token inputs, outputs, and financial estimated costs.
- **Duration** — Active running time.
- **Output** — Live status messages and command telemetry streaming natively from the agent constraints.

## Verification Pipeline

Tendril enforces a strict **Verification** pipeline at the end of each `ExecutePlan` or `UpdatePlan` job. Instead of hoping the AI wrote good code, Tendril *tests* it.

1. **Build Rules**: Tendril runs the configured build commands (e.g., `dotnet build` or `npm run typecheck`). If the build fails, the agent is fed the compiler logs and instructed to loop and fix the compilation errors.
2. **Formatting Rules**: Tendril runs auto-formatters (e.g., `dotnet format` or `prettier`).
3. **Test Rules**: Tendril runs unit testing commands.

If Verification persistently fails beyond the configured recursion limits, the Plan transitions to the **Failed** state, preventing broken code from advancing to Review.

## Job Concurrency and Isolation

Tendril is designed for massive multiprocessing. It supports configurable parallel execution channels. 

When a Plan is executed, Tendril intelligently checks out a **Git Worktree** (a secondary isolated working directory linked to your repo) instead of modifying your active branch. This means you can have 3, 5, or 10 agents writing code in the background simultaneously without creating file-locks or disrupting the code you are currently editing in your editor.

## Cost Tracking

Every job logs its usage payload directly to a `costs.csv` file embedded securely in the Plan folder. Tendril’s dashboard automatically aggregates these logs, allowing you to slice your AI spend by project, by time-period, and by promptware type.
