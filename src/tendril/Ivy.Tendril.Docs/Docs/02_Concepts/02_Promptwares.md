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
Promptwares are structured agent programs that power each stage of the plan lifecycle. Each promptware acts as a specialized AI worker with its own prompt, toolset, persistent memory, and lifecycle hooks.
</Ingress>

## What is a Promptware?

A promptware is simply a folder inside `TENDRIL_HOME/Promptwares/` defining the behavior of an autonomous agent:

- **Program.md** — The main system prompt that instructs the agent on its objective and constraints.
- **Tools/** — Custom executable scripts (e.g., PowerShell) that the agent can seamlessly invoke as tools.
- **Memory/** — Persistent memory files that accumulate shared learnings and user preferences across multiple sessions.

Tendril dispatches these Promptwares via the underlying AI provider (like Claude Code) to achieve reliable, specialized automation.

## Core Promptware Executables (Job Types)

Tendril's architecture depends on different agents doing different tasks, rather than one agent trying to do everything:

| Job Type | Objective |
|-----------|---------|
| **MakePlan** | Drafts a detailed architectural/execution plan from a brief description or GitHub issue. |
| **ExecutePlan** | Checks out a git worktree, writes code, runs builds, and implements the drafted solution. |
| **UpdatePlan** | A constrained execution agent that specifically iterates on existing, already-built code based on review feedback. |
| **ExpandPlan** | Adds detail to an existing thin plan, expanding brief thoughts into full technical specifications. |
| **SplitPlan** | Intelligently breaks down a massive architectural plan into 3-5 sub-plans for parallel execution. |
| **MakePr** | Analyzes the git diff of a completed worktree and creates an informative GitHub pull request via `gh`. |
| **CreateIssue** | Synchronizes a logged or failing plan state to a GitHub issue for human triage. |

## Execution Flow

When Tendril triggers a promptware job:

1. **Context Assembly**: The `Program.md` is loaded. Project knowledge (from `config.yaml` context files) is attached.
2. **Tool Bindings**: Tools from the `Tools/` folder are read via the MCP/Tool protocol and made available to the agent.
3. **Execution**: The agent runs autonomously in the background, executing commands in an isolated state.
4. **Capture**: Output is streamed to `logs/`, tokens are measured, and costs are written to `costs.csv`.

## Lifecycle Hooks

Promptwares support customized PowerShell lifecycle hooks that run predictably:

- **Pre-execution** (`pre_execute.ps1`) — Setup steps, cloning repos, or bootstrapping environments.
- **Post-execution** (`post_execute.ps1`) — Cleanup, database resetting, or result processing after the agent is finished.
- **On-error** (`on_error.ps1`) — Error handling, notifications, and telemetry.

Hooks are configured locally and can be customized heavily per project.
