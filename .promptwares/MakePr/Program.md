# MakePr

Create GitHub pull requests from the commits in a plan's worktrees.

## Context

The firmware header contains:
- **PlanFolder** — path to the plan folder
- **ConfigPath** — absolute path to config.yaml
- **CurrentTime** — current UTC timestamp

Read the plan structure in `../.shared/Plans.md`.
Read `config.yaml` (from `ConfigPath`) for project repos and their `prRule` setting.

## PR Rules

Each repo in config.yaml has a `prRule`:
- **`default`** — Create the PR and stop
- **`yolo`** — Create PR, auto-approve with `--admin`, merge, delete remote branch, then pull the default branch into the original local repo so the code is available locally

## Execution Steps

### 1. Read Plan

- Read `plan.yaml` from the plan folder (project, commits, repos)
- Read the latest revision for the plan title and description
- Read config.yaml to find the `prRule` for each repo

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

- **Base branch:** Detect with `gh repo view --json defaultBranchRef -q .defaultBranchRef.name`
- **Title:** Plan title with plan ID, e.g. `[01111] Add --greeting flag to CLI`
- **Body:**
  - Summary from the plan's Problem + Solution sections
  - List of commits
  - **Screenshots:** Check `<PlanFolder>/artifacts/screenshots/` for images. If found, upload 1-2 of the most descriptive ones and embed with `![screenshot](url)`
  - **Video:** Check `<PlanFolder>/artifacts/videos/` for recordings. If found, upload and embed or link

### 4. Apply PR Rule

Look up the `prRule` for this repo from config.yaml.

**If `yolo`:**
```bash
gh pr merge <pr-number> --repo <owner/repo> --merge --delete-branch --admin
cd <original-repo-path>
git pull origin <default-branch>
```

This auto-merges the PR, deletes the remote branch, and pulls the merged code into the local repo.

**If `default`:** Do nothing — PR stays open for manual review.

### 5. Update plan.yaml

Append each PR URL to the `prs` list in `plan.yaml`:

```yaml
prs:
  - https://github.com/owner/repo/pull/42
```

### 6. Notify Slack

Post a notification to the `done-by-niels` Slack channel using Block Kit formatting:

```bash
notify slack done-by-niels --json '{"blocks":[{"type":"section","text":{"type":"mrkdwn","text":"*Title:* <plan-title>\n*Project:* <project>\n*PR:* <pr-links>"}}]}'
```

- **Title:** Plan title
- **Project:** From plan.yaml
- **PR:** One line per PR, formatted as `<url|owner/repo#number>`

If screenshots or videos exist in `<PlanFolder>/artifacts/`, upload them and include image URLs in the Slack message blocks.

### Rules

- One PR per repo worktree that has commits
- Skip worktrees with no commits ahead of the base branch
- Do NOT modify any source code — only push existing commits and create PRs
- Use `gh` CLI for all GitHub operations
