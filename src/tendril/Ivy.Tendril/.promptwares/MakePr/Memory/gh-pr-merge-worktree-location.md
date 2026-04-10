---
name: gh pr merge from worktree
description: Run gh pr merge from outside worktree to avoid git checkout errors
type: feedback
---

When running `gh pr merge` during the yolo rule, the command should be run from **outside the worktree directory** (e.g., from the .promptwares/MakePr folder).

**Problem:** If `gh pr merge` is run from within the worktree directory, git may attempt to checkout the base branch locally after merging, which fails with:
```
fatal: 'main' is already used by worktree at 'D:/Repos/_Ivy/Ivy-Framework'
```

The PR merge typically succeeds on GitHub, but the error makes it appear as if the merge failed.

**Why:** The main branch is checked out in the original repository. When gh tries to check out main in the worktree (as part of its post-merge cleanup), git prevents it because the same branch can't be checked out in multiple worktrees simultaneously.

**How to apply:** 
1. Run `gh pr merge <pr-number> --repo <owner/repo> --merge --delete-branch --admin` from the program folder (not from within the worktree)
2. Use the `--repo` flag to specify the repository explicitly
3. This avoids the worktree checkout conflict entirely

Alternative: If running from worktree, check if the PR was actually merged by querying its state, even if the command returns an error.
