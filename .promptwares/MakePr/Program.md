# MakePr

Create GitHub pull requests from the commits in a plan's worktrees.

## Context

The firmware header contains:
- **PlanFolder** — path to the plan folder
- **ConfigPath** — absolute path to config.yaml
- **CurrentTime** — current UTC timestamp

Read the plan structure in `../.shared/Plans.md`.

## Execution Steps

### 1. Read Plan

- Read `plan.yaml` from the plan folder
- Get the list of `commits` and `repos`
- Read the latest revision for the plan title and description

### 2. For Each Worktree

Check `<PlanFolder>/worktrees/` for each repo worktree.

For each worktree:

1. Determine the GitHub remote: `git remote get-url origin` (from the original repo, not the worktree)
2. Extract `owner/repo` from the remote URL
3. Get the worktree branch name: `git rev-parse --abbrev-ref HEAD`
4. Push the branch to origin: `git push -u origin <branch>`

### 3. Create PR

For each pushed branch, create a PR using `gh`:

```bash
gh pr create --repo <owner/repo> --base <default-branch> --head <branch> --title "<title>" --body "<body>"
```

- **Base branch:** Detect with `gh repo view --json defaultBranchRef -q .defaultBranchRef.name` or check Memory for known repos
- **Title:** Plan title with plan ID, e.g. `[01111] Add --greeting flag to CLI`
- **Body:**
  - Summary from the plan's Problem + Solution sections
  - List of commits
  - Link back to the plan folder

### 4. Update plan.yaml

Append each PR URL to the `prs` list in `plan.yaml`:

```yaml
prs:
  - https://github.com/owner/repo/pull/42
```

### Rules

- One PR per repo worktree that has commits
- Skip worktrees with no commits ahead of the base branch
- Do NOT modify any source code — only push existing commits and create PRs
- Use `gh` CLI for all GitHub operations
