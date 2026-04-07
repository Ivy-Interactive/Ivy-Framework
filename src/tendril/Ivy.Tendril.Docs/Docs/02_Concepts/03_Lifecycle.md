---
icon: RefreshCw
searchHints:
  - jobs
  - execution
  - running
  - cost
  - tokens
  - monitoring
---

# Jobs

<Ingress>
Jobs are the execution units in Tendril. Every time a promptware runs, it creates a job that tracks progress, cost, and output.
</Ingress>

## Job Tracking

Each job captures:

- **Status** — Pending, Running, Completed, Failed, or Timed Out
- **Type** — Which promptware is running (MakePlan, ExecutePlan, etc.)
- **Plan** — The plan this job belongs to
- **Cost** — Token usage and estimated cost
- **Duration** — How long the job has been running
- **Output** — Live status messages from the agent

## Job Concurrency

Tendril supports configurable concurrent job limits. Multiple plans can be executed in parallel, each in its own isolated git worktree. The concurrency limit is set in `config.yaml`.

## Cost Tracking

Every job logs its token usage to a `costs.csv` file in the plan folder. The dashboard aggregates these costs across all plans, giving you visibility into your AI spending by project, time period, and promptware type.
