---
icon: Terminal
searchHints:
  - promptware
  - agent
  - claude
  - prompt
  - tools
---

# Promptwares

<Ingress>
Promptwares are structured agent programs that power each stage of the plan lifecycle. Each promptware is a self-contained prompt with tools, memory, and lifecycle hooks.
</Ingress>

## What is a Promptware?

A promptware is a folder containing:

- **Program.md** — The main prompt that instructs the Claude agent
- **Tools/** — PowerShell scripts that the agent can invoke as tools
- **Memory/** — Persistent memory files that accumulate learnings across sessions

Promptwares run via the Claude CLI and are the building blocks of Tendril's automation.

## Core Promptwares

| Promptware | Purpose |
|-----------|---------|
| **MakePlan** | Drafts a plan from a description or GitHub issue |
| **ExpandPlan** | Adds detail to an existing plan or splits it into smaller plans |
| **ExecutePlan** | Implements the plan in an isolated git worktree |
| **MakePr** | Creates a GitHub pull request from the completed worktree |
| **ReviewTests** | Runs automated verification checks |

## How Promptwares Run

When Tendril triggers a promptware:

1. The `Program.md` is loaded as the system prompt
2. Tools from the `Tools/` folder are made available to the agent
3. Memory files provide context from previous sessions
4. The agent executes autonomously, using tools as needed
5. Results are captured in the plan's logs and artifacts

## Lifecycle Hooks

Promptwares support lifecycle hooks that run at specific points:

- **Pre-execution** — Setup steps before the agent starts
- **Post-execution** — Cleanup and result processing after completion
- **On-error** — Error handling and recovery

Hooks are configured in `TENDRIL_HOME/Hooks/` and can be customized per project.
