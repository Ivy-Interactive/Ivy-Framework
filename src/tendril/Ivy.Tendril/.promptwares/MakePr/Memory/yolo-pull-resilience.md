# Yolo Pull Resilience

When applying the `yolo` prRule after merging a PR, the final step is `git pull origin <default-branch>` in the original repo. This can fail if the repo is on a different branch, has uncommitted changes, or has merge conflicts.

## Primary Approach: Use `git fetch` Instead

If the original repo is on a working branch (not the default branch) or has uncommitted changes, use `git fetch origin <default-branch>` instead of `git pull`. This safely updates the `origin/<default-branch>` ref without touching the current working tree.

**Why:** The original repo may be used for multiple concurrent plans. Pulling main into a working branch causes conflicts. Fetching updates the remote tracking branch without affecting the working tree.

## Fallback: Stash and Pull

If the repo IS on the default branch but has uncommitted changes:

1. `git stash push -m "MakePr: stashing local changes"`
2. `git pull origin <default-branch>`
3. `git stash pop`

If stash/pull/pop fails: attempt `git merge --abort` and retry.

## Final Fallback: Skip

If all approaches fail, skip the pull and log the issue. The PR workflow can complete successfully without updating the original repo — the critical operations (PR merge and branch deletion) have already succeeded.

**How to apply:**
1. Check if the original repo is on the default branch and clean
2. If yes → `git pull origin <default-branch>`
3. If on a different branch or has changes → `git fetch origin <default-branch>`
4. If fetch also fails → log warning, do not fail MakePr
