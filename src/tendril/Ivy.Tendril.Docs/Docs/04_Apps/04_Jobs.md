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
The Jobs App provides low-level diagnostics into running agent sessions, displaying real-time command streaming, exact financial costs, and execution duration.
</Ingress>

## Overview

Whenever a Tendril component dispatches a Promptware (e.g. hitting 'Execute' in Drafts, or 'Revise' in Review), the dispatcher registers an active payload under the Jobs architecture.

The interface details:

- **Status** — Running, Completed, Failed, Pending.
- **Job Category** — Identification of the underlying Agent directive (`MakePlan`, `ExecutePlan`, `UpdatePlan`, `MakePr`).
- **Telemetry** — Current input/output Tokens measured concurrently against your Anthropic configuration.

## Telemetry Terminal 

A critical feature built into the Jobs App is the live diagnostic tail. Instead of staring at a generic progress bar, you are given a filtered Xterm terminal stream routing `stdout` native interactions back to the UI. If the executed Agent is running a failing compiler build or reading a directory log, it is printed visually allowing the developer to guarantee the system isn't stalled quietly.

## Intercept & Control

If you suspect an Agent is looping fatally or spending excessively due to bad initialization:

| Action | Description |
|--------|-------------|
| **Kill (Stop)** | Sends a termination signal halting background PowerShell Promptware processes, forcibly releasing the git worktree locks. |
| **View Trace Logs** | Exposes the preserved post-mortar textual dumps collected inside the `logs/` directory for debugging. |
| **Retry Jobs** | Convenient hook to un-stall a stuck Job transition. |
