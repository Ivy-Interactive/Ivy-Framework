---
name: Worktree Cleanup Windows
description: Handle Windows file locking issues during worktree cleanup after yolo merges
type: feedback
---

On Windows, `git worktree remove --force` can fail with various errors when trying to delete worktree directories:
- **"Permission denied"** — Windows file handles may still be open (VS Code, file indexers, antivirus)
- **"Result too large"** — Directory tree is too large or path is too long for git's internal deletion logic
- These issues commonly occur with `node_modules/` or `bin/Debug/` directories containing many nested files
- The `--force` flag doesn't bypass OS-level file locks or size limitations

**Why:** This is a known Windows limitation with file locking, not a bug in the MakePr workflow. The worktree cleanup is an optimization to reclaim disk space - it's not critical to the PR workflow success.

**How to apply:**
1. Attempt `git worktree remove --force` as normal
2. If it fails (permission denied, result too large, or any other error), **immediately use PowerShell removal** as the primary fallback
3. If PowerShell removal also fails, log a warning but do NOT fail the overall MakePr execution
4. The locked worktree directory can be manually cleaned up later or on next system restart

**Fallback strategy:** When `git worktree remove --force` fails, use PowerShell's `Remove-Item -Recurse -Force` which is more effective on Windows than bash `rm -rf`:
```powershell
pwsh -NoProfile -Command "Remove-Item -Path '<worktrees-dir>' -Recurse -Force -ErrorAction SilentlyContinue"
```
This bypasses git's internal checks and handles Windows long paths better than git worktree remove.

The Program.md already states: "If cleanup fails (e.g. locked files on Windows), log a warning but do not fail the overall MakePr execution."
