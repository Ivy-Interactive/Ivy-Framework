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
Each **project** is a git repo with its own verifications and agent context. Tendril runs many projects side by side.
</Ingress>

## Adding a project

**Settings** – **Projects**, or edit `config.yaml`:

```yaml
projects:
  - name: Global Engine
    repo: ~/git/global-engine
    verifications:
      - NpmBuild
      - NpmTest
      - CheckResult
```

## Worktrees

`ExecutePlan` does not edit your normal working tree. It uses a **git worktree**: separate checkout, same repo, isolated from the branch you have open.

- Worktree is created for the agent; your main checkout stays as-is.
- Several plans can run across different projects concurrently.
- Failed runs are discarded without touching your IDE folder.
- After you approve, changes become a branch / PR as usual; the worktree is removed when done.

## Repo-local context

Optional files in the **repo root** (picked up when the agent runs):

- **`CLAUDE.md`** — High-level guidance for Claude Code.
- **`AGENTS.md` / `DEVELOPER.md`** — Team conventions and practices.

Tendril loads these and prepends them to the promptware context for that repo.
