# MakePr

Create GitHub pull requests, apply PR rules, and notify Slack.

**!CRITICAL: ALL steps are mandatory. Do not skip Slack notification or PR rule application.**

## Context

The firmware header contains:
- **PlanFolder** — path to the plan folder
- **ConfigPath** — absolute path to config.yaml
- **CurrentTime** — current UTC timestamp

Read the plan structure in `../.shared/Plans.md`.
Read `config.yaml` (from `ConfigPath`) for project repos and their `prRule` setting.

## PR Rules (from config.yaml per repo)

- **`default`** — Create the PR and stop
- **`yolo`** — Create PR → auto-merge with `--admin` → delete remote branch → pull default branch into the original local repo

## Execution Steps

### 1. Read Plan

- Read `plan.yaml` from the plan folder (project, commits, repos)
- Read the latest revision for the plan title and description
- Read config.yaml to find the `prRule` for each repo

### 2. For Each Worktree

Check `<PlanFolder>/worktrees/` for each repo worktree.

For each worktree:

1. `git remote get-url origin` (from the worktree) to get the GitHub remote
2. Extract `owner/repo` from the remote URL
3. `git rev-parse --abbrev-ref HEAD` to get the branch name
4. `git push -u origin <branch>`

### 2.5. Upload Artifacts

Run the `Upload-Artifacts.ps1` tool to upload screenshots and videos from `<PlanFolder>/artifacts/` to Azure storage:

```powershell
$artifactMarkdown = pwsh -NoProfile -File .promptwares/MakePr/Tools/Upload-Artifacts.ps1 -PlanFolder <PlanFolder>
```

Capture the returned markdown. If non-empty, it will be appended to the PR body under an `## Artifacts` heading in the next step.

### 3. Create PR

For each pushed branch:

```bash
gh pr create --repo <owner/repo> --base <default-branch> --head <branch> --title "<title>" --body "$(cat <<'EOF'
<body content>
EOF
)"
```

- **Base branch:** `gh repo view --repo <owner/repo> --json defaultBranchRef -q .defaultBranchRef.name`
- **Title:** `[<planId>] <plan title>`
- **Body:** If `<PlanFolder>/artifacts/summary.md` exists, use its content as the PR body (followed by list of commits). Otherwise, fall back to summary from Problem + Solution sections. If `$artifactMarkdown` from step 2.5 is non-empty, append it under an `## Artifacts` heading after the commits list.

### 4. Apply PR Rule

**!MANDATORY** — look up the `prRule` for this repo in config.yaml under the project's repos list.

**If `yolo`:**
```bash
gh pr merge <pr-number> --repo <owner/repo> --merge --delete-branch --admin
cd <original-repo-path>
git pull origin <default-branch>
```

**If `default`:** PR stays open for manual review.

### 5. Update plan.yaml

Append each PR URL to the `prs` list in `plan.yaml`.

### 6. Notify Slack

**!MANDATORY** — this step must always run, even if there are no screenshots.

```bash
notify slack done-by-niels --message "*Title:* <plan-title>
*Project:* <project>
*PR:* <pr-link>"
```

- Replace `<plan-title>` with the plan title
- Replace `<project>` with the project from plan.yaml
- Replace `<pr-link>` with `<url|owner/repo#number>` for each PR

Note: The `notify slack` command uses `--message` (not `--json`). Use Slack mrkdwn formatting in the message string.

### Rules

- **ALL 7 steps are mandatory** (including 2.5) — do not stop after creating the PR
- One PR per repo worktree that has commits
- Skip worktrees with no commits ahead of the base branch
- Use `gh` CLI for all GitHub operations
