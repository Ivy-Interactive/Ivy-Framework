# Ivy-Tendril Repository Ambiguity

**Created**: 2026-04-04 (Session 00550, Plan 01706)

## The Issue

There are **two separate GitHub repositories** that have caused confusion and missing changes:
- **Ivy-Interactive/Ivy-Tendril** (default branch: `master`) - legacy/deprecated repo
- **Ivy-Interactive/Ivy-Framework** (default branch: `main`) - current active repo with Tendril code at `src/tendril/Ivy.Tendril/`

Some plans were created with `repos: [D:\Repos\_Ivy\Ivy-Tendril]` (which doesn't exist locally), causing PRs to be created in the Ivy-Tendril repo on GitHub instead of Ivy-Framework.

## How It Was Discovered

User reported that 3 DashboardApp plans (01605, 01650, 01652) were created with PRs, but only 2 changes were visible:

1. **Plan 01605** - PR #238 to `Ivy-Interactive/Ivy-Tendril` repo, merged to `master` branch
   - Commit d18cf59f changes `Apps/DashboardApp.cs` (relative path in Ivy-Tendril repo)
   - This change never made it to Ivy-Framework main
   - However, a later large import commit (b10722ad) brought functionally equivalent changes

2. **Plans 01650 & 01652** - PRs to `Ivy-Interactive/Ivy-Framework`, merged to `main` branch
   - Both changes ARE visible in current code

## Current State

- **Local repos**: Only Ivy-Framework exists at `D:\Repos\_Ivy\Ivy-Framework`
- **No local Ivy-Tendril repo**: The path `D:\Repos\_Ivy\Ivy-Tendril` does not exist
- **Config.yaml is correct**: Tendril project is configured to use Ivy-Framework repo
- **Remote Ivy-Tendril repo**: Still exists on GitHub but appears deprecated

## Why This Causes Problems

1. Plans created with wrong repo path (`D:\Repos\_Ivy\Ivy-Tendril`) still succeed because:
   - MakePr can create PRs to remote repos even if they don't exist locally
   - The GitHub repo exists, so `gh pr create` works

2. Changes go to different repos:
   - Ivy-Tendril PRs merge to `master` branch
   - Ivy-Framework PRs merge to `main` branch
   - No automatic sync between the two

3. User confusion:
   - Expects all changes to appear in the running app
   - App runs from Ivy-Framework main, but some changes went to Ivy-Tendril master

## Detection

```bash
# Check which repo a commit belongs to
cd D:\Repos\_Ivy\Ivy-Framework
git log --oneline --all | grep <commit-hash>

# Check which branches contain a commit
git branch -r --contains <commit-hash>

# If it shows only master (not main), it might be from Ivy-Tendril

# Verify plan.yaml repos field
cat <PlanFolder>/plan.yaml | grep -A 1 "^repos:"
```

## Solution

1. **For existing plans**: Verify all changes are functionally present in Ivy-Framework main
2. **For future plans**: Ensure config.yaml is followed and plans don't reference Ivy-Tendril
3. **Deprecate Ivy-Tendril**: Consider archiving the Ivy-Tendril GitHub repo to prevent future confusion

## Prevention

- **MakePlan validation**: Add check to ensure repo paths in plan.yaml match project config
- **ExecutePlan validation**: Verify worktree repos exist locally before execution
- **Config.yaml compliance**: Ensure project repos are used for new plans

## Related Memory

- `branch-divergence-main-vs-master.md` - Similar symptoms but different root cause
- This issue is NOT about branch divergence within one repo
- This is about plans targeting two completely different repositories

## Success Metrics

After implementing preventions:
- Zero plans created with Ivy-Tendril repo path
- All Tendril plans target Ivy-Framework repo only
- No "missing changes" reports due to repo ambiguity
