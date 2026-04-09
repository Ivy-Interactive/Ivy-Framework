---
icon: FolderGit
searchHints:
  - project
  - repo
  - repository
  - multi-project
  - isolation
  - worktree
---

# Project Setup

<Ingress>
Tendril natively manages multiple software projects. Each Project maps entirely to a dedicated git repository and enforces its own strict validation and agent behavior settings.
</Ingress>

## Registering a Project

Add a new project in the **Setup App** -> **Projects** tab, or modify `config.yaml`:

```yaml
projects:
  - name: Global Engine
    repo: ~/git/global-engine
    verifications:
      - NpmBuild
      - NpmTest
      - CheckResult
```

## Worktree Isolation

When an execution phase begins (`ExecutePlan`), Tendril fundamentally protects your code. Instead of mutating your active repository checkout, Tendril generates an isolated, headless **Git Worktree**:

1. Tendril clones your repository locally via standard git worktree hooks.
2. An isolated environment is passed to the Claude Agent.
3. Your main branch remains completely untouched while agents build code.
4. Multiple different Plans can execute across different projects simultaneously cleanly.
5. Failed agent executions are safely wiped off disk without polluting your IDE workspace.

When you approve a Plan, the worktree is transformed into a clean git branch and pushed, then deleted safely.

## Project Context Overrides

Every project comes with distinct coding conventions. Tendril allows overriding behavior on a per-project basis. Drop context files directly into the root of your target repository:

- `CLAUDE.md`: Broad system guidelines picked up automatically by Claude Code.
- `AGENTS.md` / `DEVELOPER.md`: Domain-specific coding practices.

When initializing the Promptware environment, Tendril scans for these files and prefixes the agent execution with your project-specific standards.
