# MakePr

**Note:** This promptware is stack-agnostic. Stack-specific operations (build, format, test) are defined in `config.yaml` under `verifications`. Examples in this document use multiple tech stacks for illustration.

Create GitHub pull requests and apply PR rules.

**!CRITICAL: ALL steps are mandatory. Do not skip PR rule application.**

## Context

The firmware header contains:
- **PlanFolder** — path to the plan folder
- **CurrentTime** — current UTC timestamp

Read the plan structure in `../.shared/Plans.md`.
Use the `Get-ConfigYaml` helper from Utils.ps1 to read project configuration (project repos and their `prRule` setting) with caching.

## PR Rules (from config.yaml per repo)

- **`default`** — Create the PR and stop
- **`yolo`** — Create PR → auto-merge with `--admin` → delete remote branch → pull default branch into the original local repo

## Execution Steps

### 0. Check Plan State

Before processing, read `plan.yaml` and check the `state` field:
- If `state: Completed`, the plan was already processed. Exit early with a message indicating the plan is already completed and showing the existing PR URLs from the `prs` list.
- Otherwise, proceed with step 1.

### 1. Read Plan

- Read `plan.yaml` from the plan folder (project, commits, repos)
- Read the latest revision for the plan title and description
- Use `Get-ConfigYaml` to find the `prRule` for each repo
- **Check for custom options:** If `<PlanFolder>/.custom-pr-options.yaml` exists, read it. The file contains:
  ```yaml
  merge: true/false
  deleteBranch: true/false
  includeArtifacts: true/false
  assignee: "username"
  comment: "Review comment text"
  ```
  These flags override the default behavior in subsequent steps. If the file does not exist, all flags default to the behavior defined by the repo's `prRule`. **Delete the file after reading** so it doesn't affect future runs.

### 2. For Each Worktree

Check `<PlanFolder>/worktrees/` for each repo worktree.

> **Worktree already removed:** If the worktrees/ directory is empty (worktree was already cleaned up), fall back to `plan.yaml` to get the repo path and branch name (format: `tendril/<planId>-<SafeTitle>`, where SafeTitle is extracted from the plan folder name: e.g. `03158-ChangeBranchNaming` → `ChangeBranchNaming`). The commit objects may still exist in the original repo's object store. Use `git cat-file -t <sha>` to verify, then create or force-update the local branch: `git branch -f <branch-name> <sha>` (use `-f` because the branch may already exist from a WIP auto-commit) and push from the original repo path.
>
> **Commit lost (object GC'd):** If `git cat-file -t <sha>` fails, the commit was garbage-collected after worktree removal. In this case: (1) check if the change is already on main, (2) if not, recreate the change from the plan revision — create a new branch from main, apply the changes as described in the revision, commit with the standard `[<planId>] <title>` message, and push. Update `plan.yaml` commits list with the new commit hash.

For each worktree:

1. `git remote get-url origin` (from the worktree) to get the GitHub remote
2. Extract `owner/repo` from the remote URL
3. `git rev-parse --abbrev-ref HEAD` to get the branch name
4. `git push -u origin <branch>`

> **Stale remote tracking refs warning:** A ref appearing in `git branch -a` as `remotes/origin/<branch>` does NOT guarantee the branch exists on GitHub. Always verify with `gh api repos/<owner>/<repo>/branches/<branch>` or `git ls-remote origin <branch>` before assuming the push succeeded.
>
> **Push rejected (non-fast-forward) with diverged history:** If `git push` fails with non-fast-forward and the remote branch contains commits from a different plan (plan ID reuse or prior aborted execution), **force-push** with `git push -f -u origin <branch>`. This is safe because the plan branch is private to this plan's execution and any diverged remote state is stale.

### 2.5. Upload Artifacts

**If custom options exist and `includeArtifacts` is `false`, skip this step entirely** (set `$artifactMarkdown` to empty).

Otherwise, run the `Upload-Artifacts.ps1` tool to upload screenshots and videos from `<PlanFolder>/artifacts/` to Azure storage:

```powershell
$artifactMarkdown = pwsh -NoProfile -File Promptwares/MakePr/Tools/Upload-Artifacts.ps1 -PlanFolder <PlanFolder>
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

- **Base branch:** `gh repo view <owner/repo> --json defaultBranchRef -q .defaultBranchRef.name`
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
- If `merge` is `false`: skip the entire merge step (treat as `default` rule regardless of prRule)
- If `merge` is `true` but `deleteBranch` is `false`: merge without `--delete-branch` flag
- If `merge` and `deleteBranch` are both `true`: behave exactly like `yolo`

**Merge conflict handling (applies to ALL merge paths below):**

Before calling `gh pr merge`, check for merge conflicts:

```bash
# Poll mergeability (GitHub computes it asynchronously)
for i in $(seq 1 6); do
  MERGEABLE=$(gh pr view <pr-number> --repo <owner/repo> --json mergeable -q '.mergeable')
  if [[ "$MERGEABLE" != "UNKNOWN" ]]; then break; fi
  sleep 5
done
```

| Mergeable status | Action |
|---|---|
| `MERGEABLE` | Proceed with merge |
| `CONFLICTING` | **Resolve conflicts** (see below), then retry |
| `UNKNOWN` (after 30s timeout) | Fail conservatively |

#### Conflict Resolution

When the PR status is `CONFLICTING`, resolve the conflict locally before retrying:

1. **Locate the worktree** for this repo. If the worktree still exists in `<PlanFolder>/worktrees/<repo-folder-name>`, use it. If the worktree was already removed, use the original repo path — create or force-update the local branch first: `git branch -f <branch-name> <sha>` and `git checkout <branch-name>`.

2. **Read the plan revision** to understand the intent of the plan's changes (what matters, what can be safely adapted).

3. **Merge the base branch** into the feature branch:
   ```bash
   cd <worktree-or-repo-path>
   git fetch origin <default-branch>
   git merge origin/<default-branch>
   ```

4. **Resolve conflicts**: Read each conflicted file (`git diff --name-only --diff-filter=U`), understand both sides, and resolve using the Edit tool. Prioritize:
   - Keep the plan's intentional changes
   - Accept base branch changes for unrelated code
   - When both sides changed the same lines, merge intelligently based on the plan's intent

5. **Commit the merge**:
   ```bash
   git add -A
   git commit -m "[<planId>] Resolve merge conflicts with <default-branch>"
   ```

6. **Quick build check** (if build-critical files were involved in conflicts):
   ```bash
   # Run your project's build command from config.yaml verifications
   # Examples:
   # - .NET: dotnet build --warnaserror
   # - JavaScript: npm run build
   # - Go: go build ./...
   ```
   If the build fails, fix the issue and amend the merge commit.

7. **Push** the resolved branch:
   ```bash
   git push origin <branch>
   ```

8. **Re-check mergeability** (poll up to 30s again). If now `MERGEABLE`, proceed with the merge. If still `CONFLICTING` after resolution, **fail with a detailed error** explaining which files could not be resolved.

**Important:** Only attempt conflict resolution **once**. If the second mergeability check still shows CONFLICTING, fail the execution — infinite retry loops waste tokens and time.

**If `yolo` (and no custom options overriding):**
```bash
gh pr merge <pr-number> --repo <owner/repo> --merge --delete-branch --admin
cd <original-repo-path>
git pull origin <default-branch>
```

> **Note:** If `--merge` fails with "Merge commits are not allowed", retry with `--squash` instead.


**If `default`:** PR stays open for manual review.

### 5. Clean Up Worktrees

After successful `yolo` merges (or custom options with `merge: true`), clean up the worktrees to reclaim disk space:

For each repo where the PR was merged:

```bash
cd <original-repo-path>
PLAN_FOLDER_NAME=$(basename "<PlanFolder>")
PLAN_ID=$(echo "$PLAN_FOLDER_NAME" | grep -oP '^\d+')
SAFE_TITLE=$(echo "$PLAN_FOLDER_NAME" | sed 's/^[0-9]\+-//')
BRANCH_NAME="tendril/$PLAN_ID-$SAFE_TITLE"
git worktree remove "<PlanFolder>/worktrees/<repo-folder-name>" --force
git branch -D "$BRANCH_NAME" 2>/dev/null
```

If **all** worktrees were cleaned up, remove the now-empty `worktrees/` directory:

```bash
rm -rf "<PlanFolder>/worktrees"
```

**Skip cleanup** for repos using the `default` PR rule (or custom options with `merge: false`) — the worktree is still needed for potential review revisions.

If cleanup fails (e.g. locked files on Windows), log a warning but do not fail the overall MakePr execution.

### 6. Update plan.yaml

Append each PR URL to the `prs` list in `plan.yaml`.

**Update state to Completed:** If ALL repos in the plan used the `yolo` prRule (or custom options with `merge: true`) and ALL PRs were successfully merged, update the `state` field from `Building` to `Completed`. This marks the plan as fully processed.

If ANY repo used the `default` prRule (or custom options with `merge: false`), do NOT update the state — the plan remains open for manual review and potential revisions.

> If merge conflict resolution was performed (Step 4), the resolution commit hash should already be on the pushed branch. No additional plan.yaml update needed beyond the PR URL.

### Rules

- **ALL 7 steps are mandatory** (including 2.5) — do not stop after creating the PR
- One PR per repo worktree that has commits
- Skip worktrees with no commits ahead of the base branch
- Use `gh` CLI for all GitHub operations
- NEVER embed images via GitHub branch URLs (`github.com/blob/<branch>/...`) — these 404 after branch deletion. All screenshots/images in PR bodies must use storage URLs from Upload-Artifacts.ps1.
