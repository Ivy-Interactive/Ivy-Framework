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
- Read the project's `color` from config.yaml for Slack notification formatting
- Check for `<PlanFolder>/.custom-pr-options.yaml`. If present, read it and use its flags to conditionally skip steps below. The file has these fields (all default to `true` if missing):
  - `approve` — if `false`, skip approval and merge entirely (regardless of prRule)
  - `merge` — if `false`, skip merge and delete-branch
  - `deleteBranch` — if `false`, skip branch deletion after merge
  - `includeArtifacts` — if `false`, skip step 2.5 (artifact upload)
  - `submitToSlack` — if `false`, skip step 6 (Slack notification)
  - `assignee` — if non-empty, add `--assignee <assignee>` to the `gh pr create` command
  - `comment` — if non-empty, after creating the PR run `gh pr comment <number> --repo <owner/repo> --body "<comment>"`
- After all steps complete, delete `.custom-pr-options.yaml` so it doesn't affect future runs

### 2. For Each Worktree

Check `<PlanFolder>/worktrees/` for each repo worktree.

For each worktree:

1. `git remote get-url origin` (from the worktree) to get the GitHub remote
2. Extract `owner/repo` from the remote URL
3. `git rev-parse --abbrev-ref HEAD` to get the branch name
4. `git push -u origin <branch>`

### 2.5. Upload Artifacts

**Skip this step if custom PR options has `includeArtifacts: false`.**

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
- **Assignee:** If custom PR options has a non-empty `assignee`, add `--assignee <assignee>` to the `gh pr create` command.
- **Comment:** If custom PR options has a non-empty `comment`, after creating the PR run: `gh pr comment <pr-number> --repo <owner/repo> --body "<comment>"`

### 4. Apply PR Rule

**!MANDATORY** — look up the `prRule` for this repo in config.yaml under the project's repos list.

**Custom PR overrides:** If custom PR options exist:
- If `approve: false`, skip this entire step (treat as `default` rule regardless of config)
- If `merge: false`, skip the merge command (but PR is still created)
- If `deleteBranch: false`, remove `--delete-branch` from the merge command

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

**!MANDATORY** — this step must always run, even if there are no screenshots. **Exception:** Skip if custom PR options has `submitToSlack: false`.

**Check for screenshot URL:** Extract the first image URL from `$artifactMarkdown` (from step 2.5) by matching the pattern `![...](url)`.

**If a screenshot URL exists** — use `notify slack done-by-niels --json` with Block Kit JSON:

```bash
notify slack done-by-niels --json '{"blocks":[{"type":"section","text":{"type":"mrkdwn","text":"*Title:* <plan-title>\n*Project:* <color-emoji> <project>\n*PR:* <pr-link>"},"accessory":{"type":"image","image_url":"<screenshot-url>","alt_text":"screenshot"}}]}'
```

**If no screenshot URL** — fall back to plain text:

```bash
notify slack done-by-niels --message "*Title:* <plan-title>
*Project:* <color-emoji> <project>
*PR:* <pr-link>"
```

For both variants:
- Replace `<plan-title>` with the plan title
- Replace `<project>` with the project from plan.yaml
- Replace `<pr-link>` with `<url|owner/repo#number>` for each PR
- Replace `<screenshot-url>` with the extracted URL (Block Kit variant only)
- Replace `<color-emoji>` with a Slack emoji based on the project's `color` from config.yaml:
  - Blue → `:large_blue_circle:`
  - Purple → `:purple_circle:`
  - Teal → `:large_green_circle:`
  - Amber → `:large_yellow_circle:`
  - Emerald → `:large_green_circle:`
  - Sky → `:large_blue_circle:`
  - Slate → `:white_circle:`
  - Red → `:red_circle:`
  - If no color is set, omit the emoji

### Rules

- **ALL 7 steps are mandatory** (including 2.5) — do not stop after creating the PR
- One PR per repo worktree that has commits
- Skip worktrees with no commits ahead of the base branch
- Use `gh` CLI for all GitHub operations
- NEVER embed images via GitHub branch URLs (`github.com/blob/<branch>/...`) — these 404 after branch deletion. All screenshots/images in PR bodies must use storage URLs from Upload-Artifacts.ps1.
