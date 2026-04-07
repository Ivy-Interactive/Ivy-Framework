---
icon: FolderGit
searchHints:
  - project
  - repo
  - repository
  - multi-project
---

# Project Setup

<Ingress>
Tendril supports managing multiple projects simultaneously. Each project maps to a git repository and has its own verification and configuration settings.
</Ingress>

## Adding a Project

Add a new project entry to the `projects` array in `config.yaml`:

```yaml
projects:
  - name: My App
    repo: D:\Repos\MyApp
    verifications:
      - DotnetBuild
      - DotnetFormat
      - CheckResult
```

## Project Isolation

When a plan is executed, Tendril creates a **git worktree** for the target project. This provides complete isolation:

- The main branch remains untouched during execution
- Multiple plans can execute in parallel on different worktrees
- Failed executions don't affect your working copy

Worktrees are created under the plan's folder at `worktrees/` and are cleaned up after the PR is merged or the plan is discarded.

## Per-Project Verifications

Each project can have its own set of verifications. After an agent completes execution, Tendril runs the configured verifications in order. If any verification fails, the plan is flagged for review.

## Context Files

Projects can include context files (like `CLAUDE.md` or `DEVELOPER.md`) that are automatically included in the agent's prompt. These files provide project-specific instructions and conventions that guide the agent's behavior.
