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
Promptwares are the agents behind each plan stage: each has its own prompt, tools, memory, and hooks.
</Ingress>

Promptwares is a folder under `TENDRIL_HOME/Promptwares/`:

- **Program.md** — System prompt (goal and rules).
- **Tools/** — Scripts (e.g. PowerShell) the agent calls as tools.
- **Memory/** — Long-lived notes across runs.

Tendril runs them through the configured AI stack (e.g. Claude Code) for focused automation.

## Core jobs

| Job | Role |
|-----|------|
| **MakePlan** | Plan from a short brief or GitHub issue. |
| **ExecutePlan** | Worktree, implement, build, test. |
| **UpdatePlan** | Change existing code from review feedback. |
| **ExpandPlan** | Flesh out a thin plan. |
| **SplitPlan** | Split a large plan into sub-plans. |
| **MakePr** | GitHub PR from the worktree diff (`gh`). |
| **CreateIssue** | Push plan failure/state to GitHub for triage. |

## Execution flow

1. **Context** — Load `Program.md`; attach project context from `config.yaml`.
2. **Tools** — Expose `Tools/` via the tool protocol.
3. **Run** — Agent runs in the background with isolated state.
4. **Capture** — Stream to `logs/`; tokens and cost – `costs.csv`.

## Hooks (PowerShell)

- **pre_execute.ps1** — Setup, clone, env.
- **post_execute.ps1** — Cleanup, post-process.
- **on_error.ps1** — Errors, notify, telemetry.

Per-project customization is normal.
