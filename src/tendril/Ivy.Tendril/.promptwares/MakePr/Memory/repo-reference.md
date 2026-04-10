# Repository Reference

## Repositories

| Repo | Remote URL | Default Branch | Local Path |
|------|------------|----------------|------------|
| Ivy-Interactive/Ivy-Framework | `https://github.com/Ivy-Interactive/Ivy-Framework.git` | `main` | `D:\Repos\_Ivy\Ivy-Framework` |
| Ivy-Interactive/Ivy-Tendril | ARCHIVED (read-only) | N/A | `D:\Repos\_Ivy\Ivy-Tendril` (legacy name, points to Ivy-Framework) |

**All PRs (both Framework and Tendril) target the `main` branch in Ivy-Interactive/Ivy-Framework.**

## Archived Repository: Ivy-Tendril

Ivy-Tendril was previously a standalone repository at `https://github.com/Ivy-Interactive/Ivy-Tendril.git` but has been **archived** and migrated into the Ivy-Framework monorepo.

- **Migration date:** Prior to 2026-04-03
- **Migration commit:** b10722ad (merged Tendril into Ivy-Framework/main)
- **New location:** `src/tendril/Ivy.Tendril/` within Ivy-Framework

## Handling Old Worktrees with Diverged History

Old plan worktrees created before the migration may be based on the obsolete "master" branch history. These branches have no common ancestor with "main".

**Detection:** Check the worktree's remote: `git remote get-url origin`. If it points to `Ivy-Interactive/Ivy-Tendril`, the worktree is outdated.

**Solution:**
1. Create a fresh branch from current `origin/main` in the original repo
2. Cherry-pick just the relevant commits from the old worktree
3. Push the new branch and create PR from that
4. Use `Ivy-Interactive/Ivy-Framework` as the repo for all PR operations
