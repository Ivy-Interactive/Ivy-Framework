---
searchHints:
  - overview
  - what-is
  - tendril
  - agent
  - orchestration
  - architecture
---

# Welcome to Tendril

<Ingress>
Tendril is an AI orchestration app on the Ivy stack: a cross-platform UI plus autonomous agents for real software workflows—not a black box.
</Ingress>

You see each stage of the work. Tasks are **Plans**; orchestrated **Promptwares** (Claude-based agents) generate code, verify it, and open PRs—without hiding what ran.

<Embed Url="https://www.youtube.com/watch?v=PLACEHOLDER"/>

## The Concept

**Plans** are structured units of work (bugfix, refactor, feature). Tendril moves them through a defined lifecycle using isolated, single-purpose agents called **Promptwares**.

## Key Features

- **Plan lifecycle** — Draft – execution – review – PR.
- **Multi-project** — Several repos, per-project verification rules.
- **Jobs** — Status, tokens, cost.
- **Promptwares** — e.g. `MakePlan`, `ExecutePlan`, `ExpandPlan`, `MakePr`.
- **Git worktrees** — Agent work stays off your main branch.
- **Terminal & file viewer** — Embedded terminal (Claude Code under the hood) and fast local file access.
- **Verification** — Hook your build, test, and format checks.

## The Tendril Loop

1. **`MakePlan`** — Prompt or issue – implementation plan.
2. **`ExpandPlan`** — Split large work into smaller chunks (optional).
3. **`ExecutePlan`** — Worktree, implement, build, test, iterate until verifications pass.
4. **`Review`** — You approve or send feedback for another pass.
5. **`MakePr`** — Approved work – GitHub PR.

That loop turns the assistant from autocomplete into something you can ship with.

## Quick Install

```bash
curl -sSf https://raw.githubusercontent.com/Ivy-Interactive/Ivy-Framework/main/src/tendril/install.sh | sh
```
