# MakePr

Create GitHub pull requests and apply PR rules.

**!CRITICAL: ALL steps are mandatory. Do not skip PR rule application.**

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
- **Check for custom options:** If `<PlanFolder>/.custom-pr-options.yaml` exists, read it. The file contains:
  ```yaml
  approve: true/false
  merge: true/false
  deleteBranch: true/false
  includeArtifacts: true/false
  assignee: "username"
  comment: "Review comment text"
  ```
  These flags override the default behavior in subsequent steps. If the file does not exist, all flags default to the behavior defined by the repo's `prRule`. **Delete the file after reading** so it doesn't affect future runs.

### 2. For Each Worktree

Check `<PlanFolder>/worktrees/` for each repo worktree.

For each worktree:

1. `git remote get-url origin` (from the worktree) to get the GitHub remote
2. Extract `owner/repo` from the remote URL
3. `git rev-parse --abbrev-ref HEAD` to get the branch name
4. `git push -u origin <branch>`

### 2.5. Upload Artifacts

**If custom options exist and `includeArtifacts` is `false`, skip this step entirely** (set `$artifactMarkdown` to empty).

Otherwise, run the `Upload-Artifacts.ps1` tool to upload screenshots and videos from `<PlanFolder>/artifacts/` to Azure storage:

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
- **Assignee (custom options):** If custom options exist and `assignee` is non-empty, add `--assignee <assignee>` to the `gh pr create` command.

### 3.5. Add PR Comment (custom options)

If custom options exist and `comment` is non-empty, after creating each PR run:

```bash
gh pr comment <pr-number> --repo <owner/repo> --body "<comment>"
```

If no custom options or `comment` is empty, skip this step.

### 4. Apply PR Rule

**!MANDATORY** — look up the `prRule` for this repo in config.yaml under the project's repos list.

**Custom options override:** If custom options exist, the flags override the yolo behavior:
- If `approve` is `false`: skip the entire merge step (treat as `default` rule regardless of prRule)
- If `approve` is `true` but `merge` is `false`: approve the PR with `gh pr review <pr-number> --repo <owner/repo> --approve` but do not merge
- If `merge` is `true` but `deleteBranch` is `false`: merge without `--delete-branch` flag
- If all flags are `true`: behave exactly like `yolo`

**If `yolo` (and no custom options overriding):**
```bash
gh pr merge <pr-number> --repo <owner/repo> --merge --delete-branch --admin
cd <original-repo-path>
git pull origin <default-branch>
```

**If `default`:** PR stays open for manual review.

### 5. Clean Up Worktrees

After successful `yolo` merges (or custom options with `merge: true`), clean up the worktrees to reclaim disk space:

For each repo where the PR was merged:

```bash
cd <original-repo-path>
git worktree remove "<PlanFolder>/worktrees/<repo-folder-name>" --force
git branch -D "plan-<planId>-<repo-folder-name>" 2>/dev/null
```

If **all** worktrees were cleaned up, remove the now-empty `worktrees/` directory:

```bash
rm -rf "<PlanFolder>/worktrees"
```

**Skip cleanup** for repos using the `default` PR rule (or custom options with `merge: false`) — the worktree is still needed for potential review revisions.

If cleanup fails (e.g. locked files on Windows), log a warning but do not fail the overall MakePr execution.

### 6. Update plan.yaml

Append each PR URL to the `prs` list in `plan.yaml`.

### Rules

- **ALL 7 steps are mandatory** (including 2.5) — do not stop after creating the PR
- One PR per repo worktree that has commits
- Skip worktrees with no commits ahead of the base branch
- Use `gh` CLI for all GitHub operations
- NEVER embed images via GitHub branch URLs (`github.com/blob/<branch>/...`) — these 404 after branch deletion. All screenshots/images in PR bodies must use storage URLs from Upload-Artifacts.ps1.
