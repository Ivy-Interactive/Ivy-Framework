# ExecutePlan

**Note:** This promptware is stack-agnostic. Stack-specific operations (build, format, test) are defined in `config.yaml` under `verifications`. Examples in this document use multiple tech stacks for illustration.

Execute an approved plan in isolated git worktrees.

## Context

The firmware header contains:

- **Args** / **PlanFolder** — path to the plan folder
- **CurrentTime** — current UTC timestamp
- **Note** (optional) — Additional instructions from the reviewer. If present, follow these instructions in addition to the plan.

Read the plan structure in `../.shared/Plans.md`.
Use the `Get-ConfigYaml` helper from Utils.ps1 to read project configuration (project repos and context) with caching.

The launcher script sets the working directory to the project's primary repo.

**Note:** Plans are often executed multiple times. For example, a reviewer may not be satisfied with the first execution and sends the plan back to Draft with comments (via UpdatePlan). When re-executing, the worktree branch from the previous run may already exist — handle this gracefully (delete old worktree first, or create with a new branch suffix). Check for existing artifacts and verification reports from prior runs.

**Resume-vs-redo on re-execution:** Before deleting anything, run an integrity check on the prior run. If `plan.yaml` has commits populated and all verifications `Pass`, every `Pass` verification has a report, `artifacts/summary.md` exists, the worktree is clean with HEAD matching the last recorded commit, and the expected code changes are present in the files — then **resume** (log it and exit successfully) rather than redoing work. Redoing creates new commit hashes and breaks downstream MakePr references. Only fall back to the full re-execution flow if any of those checks fail.

## Time Budget Awareness

**You have a 30-minute hard timeout.** Plan your time carefully:

1. **Spend at most 10 minutes reading/understanding the codebase**, then start implementing. If you haven't started writing code by the 10-minute mark, simplify your approach.

2. **Prefer implementing incrementally** (write code, build, fix errors) over exhaustive upfront research. You can read more code as needed during implementation.

3. **If the plan involves unfamiliar patterns, look at ONE good example and follow it** — don't survey every usage in the codebase. Find a single clear reference and proceed.

Focus on making progress, not achieving perfect understanding. A working implementation with minor imperfections beats a timeout with no code written.

## Execution Steps

### 1. Read Plan

- Read `plan.yaml` from the plan folder (project, repos, title)
- Read the latest revision from `revisions/` (highest numbered .md file)
- Extract the plan ID from the folder name (e.g. `01105` from `01105-TestPlan`)

### 1.5. Verify Dependencies

If `plan.yaml` has a `dependsOn` list, for each entry:

1. Locate the dependency plan folder in the plans directory
2. Verify the dependency plan's state is `Completed`
3. Verify all PRs listed in the dependency's `plan.yaml` are actually merged on GitHub:

   ```bash
   gh pr view <pr-url> --json state -q .state
   # Must return "MERGED"
   ```

4. If any dependency is unmet (not completed or PRs not merged), **fail immediately** with a clear message explaining which dependency isn't ready and why.

**Note:** The JobService also performs this check before launching ExecutePlan, but this step acts as a safety net in case the dependency state changed between job launch and execution.

### 1.6. Validate Worktree Isolation

Before creating worktrees, verify the execution environment is safe:

1. **Check each repo is not itself a worktree** — If `<repo-path>/.git` is a file containing `gitdir:`, the repo is a worktree. Fail with error:
   > ERROR: Repository at <repo-path> is itself a worktree. ExecutePlan cannot create worktrees inside worktrees. Update config.yaml to use the main repo path.

2. **Check Plans directory is not inside a worktree** — If `$TENDRIL_HOME` or its parent contains a worktree `.git` file, fail with error:
   > ERROR: TENDRIL_HOME is inside a git worktree. Move your Tendril installation or change the Plans directory.

```bash
# For each repo in plan.yaml repos (or project repos if empty):
cd <repo-path>

# Check if current directory is a worktree
if [ -f .git ] && grep -q "gitdir:" .git; then
    echo "ERROR: Repository at <repo-path> is itself a worktree."
    echo "ExecutePlan cannot create worktrees inside worktrees."
    echo "Check that config.yaml repo paths point to main repositories, not worktrees."
    exit 1
fi

# Check if Plans directory would be created inside a worktree
PLANS_DIR_PARENT=$(dirname "$TENDRIL_HOME")
cd "$PLANS_DIR_PARENT"
if git rev-parse --is-inside-work-tree 2>/dev/null && [ -f "$PLANS_DIR_PARENT/.git" ]; then
    if grep -q "gitdir:" "$PLANS_DIR_PARENT/.git"; then
        echo "ERROR: TENDRIL_HOME ($TENDRIL_HOME) is inside a git worktree."
        echo "Plans and their worktrees cannot be created inside worktrees."
        echo "Move your Tendril installation outside the worktree or use a different Plans directory."
        exit 1
    fi
fi
```

This prevents recursive worktree scenarios that would corrupt git state and cause massive repo bloat.

### 1.7. Validate Code State

After reading the plan revision, scan it for code validation markers to detect stale plans (where the described code has already been changed by another plan).

1. **Extract validation blocks** — Parse the plan revision for sections containing:
   - Headers matching `**Current implementation**`, `**Current implementation in <file>**`, or `**Old implementation**`
   - Fenced code blocks (` ```language ... ``` `) immediately following these headers
   - Associated file paths (markdown links with `file:///` or inline text like `Utils.ps1:217`)

2. **Validate code exists** — For each validation block found:
   - Extract the file path from the context (header text or preceding paragraph)
   - Convert `file:///` URLs to local paths if needed
   - If a line range is specified (e.g., `:217-242`), read those specific lines
   - Otherwise, read the entire file and search for the code snippet (normalize whitespace when comparing — ignore leading/trailing blank lines and trailing spaces)
   - **Exact match** → validation passes, proceed
   - **Not found** → validation fails, the code may have already changed
   - **File not found** → validation fails, the file may have been deleted/moved

3. **Decision logic:**
   - **If no validation blocks found** → Skip validation, proceed to worktree creation (backward compatible)
   - **If all validation blocks pass** → Proceed to worktree creation
   - **If any validation fails** → Fail the plan immediately with a detailed report

4. **Write validation report** — Create `<PlanFolder>/verification/PreExecution.md`:

```markdown
# PreExecution

- **Date:** <CurrentTime>
- **Result:** Pass / Fail / Skipped
- **Blocks Found:** <number>

## Validation Blocks

### Block 1: <file path>
- **Status:** Pass / Fail
- **Expected:** (first 5 lines of expected code)
- **Actual:** (first 5 lines of actual code, or "File not found")

## Recommendation (on failure)

- Review the plan against the current codebase
- Check if this work was already completed by another plan
- Update the plan via UpdatePlan or mark as Skipped
```

**Note:** This step runs against the original repo (before worktrees are created), since it validates whether the plan's assumptions about the codebase are still accurate.

5. **Self-flagged redundancy check** — In addition to code block validation, scan the plan revision for markers where the plan itself admits it is already done:
   - A `<details><summary>Still relevant?</summary>` block whose body starts with `No.`
   - Phrases like *"Already applied"*, *"This plan is redundant"*, *"This plan is superseded"*, or *"previously attempted … was merged to main via PR #NNNN"* in the `## Problem` or `## Solution` sections.

   If any marker is found, verify the claim: run `gh pr view <cited PR> --json state,mergeCommit` (must be `MERGED`), confirm the cited commit is in `git log origin/<default-branch>`, and byte-compare the plan's proposed code against the current file contents. If all three checks pass, write `verification/PreExecution.md` with `Result: Fail`, write `artifacts/summary.md` documenting the no-op, set every `plan.yaml` verification to `Skipped`, and fail the plan **without creating a worktree** — running verifications on unchanged code wastes the time budget and produces a 0-commit PR that MakePr cannot process.

### 1.8. Auto-Commit Uncommitted Changes

Before creating worktrees, check each repo for uncommitted changes and automatically commit them. This prevents silent data loss when worktrees are created from `origin/<default-branch>` and later merged back.

For each repo listed in `plan.yaml` `repos` (or the project's repos from `config.yaml` if empty):

```bash
cd <repo-path>

if [[ -n $(git status --porcelain) ]]; then
  echo "Found uncommitted changes in $(pwd), checking for conflicts with recent commits..."
  
  STALE_FILES=()
  
  # Get list of dirty tracked files (modified/deleted/staged, not untracked)
  for file in $(git diff --name-only HEAD; git diff --cached --name-only) | sort -u; do
    # Check if this file was touched in last 5 commits
    RECENT_COMMIT=$(git log --oneline -1 -5 -- "$file" 2>/dev/null)
    if [[ -n "$RECENT_COMMIT" ]]; then
      COMMIT_HASH=$(echo "$RECENT_COMMIT" | awk '{print $1}')
      
      # Get the file content from before that commit
      PARENT_CONTENT=$(git show "${COMMIT_HASH}^:$file" 2>/dev/null)
      WORKING_CONTENT=$(cat "$file" 2>/dev/null)
      
      if [[ "$PARENT_CONTENT" == "$WORKING_CONTENT" ]]; then
        echo "WARNING: Stale file '$file' matches pre-commit state of $RECENT_COMMIT"
        echo "  Discarding stale version — keeping committed (HEAD) version."
        STALE_FILES+=("$file")
      fi
    fi
  done
  
  # Auto-resolve: discard stale files by restoring HEAD versions
  if [[ ${#STALE_FILES[@]} -gt 0 ]]; then
    echo "Auto-resolving ${#STALE_FILES[@]} stale file(s)..."
    for stale in "${STALE_FILES[@]}"; do
      git checkout HEAD -- "$stale"
      echo "  Restored: $stale"
    done
  fi
  
  # After resolving stale files, check if there are still changes to commit
  if [[ -n $(git status --porcelain) ]]; then
    git add -A
    git reset -- '*.bak_*' 2>/dev/null || true
    git commit -m "WIP: Auto-commit before plan execution [$(date -u +%Y-%m-%dT%H:%M:%SZ)]"
    git push origin $(git branch --show-current)
    echo "Changes committed and pushed successfully"
  else
    echo "All dirty files were stale — nothing to commit after cleanup."
  fi
fi
```

**Rationale:**
- Worktrees branch from `origin/<default-branch>` (Step 2), so unpushed local changes won't be in the worktree base
- When the PR merges and MakePr pulls main back, `git pull` would overwrite any uncommitted local changes
- Auto-committing and pushing ensures all local work is preserved and visible to worktrees
- The `WIP:` prefix makes auto-commits easily identifiable for later cleanup (squash/amend)
- **Revert detection with auto-resolve:** Before committing, each dirty tracked file — whether unstaged (`git diff --name-only HEAD`) or staged (`git diff --cached --name-only`) — is checked against the last 5 commits. If the working tree version matches the file's state *before* a recent commit (i.e., it's stale), the file is automatically restored to its HEAD version via `git checkout HEAD -- <file>`. This prevents silent reverts while keeping the process fully autonomous. Any remaining non-stale dirty files are committed normally.
- **Backup file exclusion:** After staging all changes with `git add -A`, the command `git reset -- '*.bak_*'` explicitly unstages any files matching the backup pattern. This prevents temporary backup files (created by FileHelper.ReadAllText's defensive copy mechanism in plan 03055) from being committed to version control. Backup files serve only as local recovery points and should not pollute the repository history.

**Note:** This step runs in the original repo directories, before worktree creation.

### 2. Create Worktrees

For each repo listed in `plan.yaml` `repos` (or the project's repos from `config.yaml` if empty):

1. Fetch latest from remote: `git fetch origin`
2. Detect the default branch: `git symbolic-ref refs/remotes/origin/HEAD | sed 's|refs/remotes/origin/||'` (usually `master` or `main`)
3. If the worktree or branch already exists from a prior execution, remove it first. A prior run may have left a **stale directory** (the filesystem tree still exists but git no longer tracks it as a worktree — there's no `.git` file at the worktree root). In that case `git worktree remove` will fail with "is not a working tree"; you must also `rm -rf` the directory. **Do all three unconditionally** so the next `git worktree add` starts from a clean slate:

```bash
PLAN_FOLDER_NAME=$(basename "<PlanFolder>")
PLAN_ID=$(echo "$PLAN_FOLDER_NAME" | grep -oP '^\d+')
SAFE_TITLE=$(echo "$PLAN_FOLDER_NAME" | sed 's/^[0-9]\+-//')
BRANCH_NAME="tendril/$PLAN_ID-$SAFE_TITLE"

git worktree remove "<PlanFolder>/worktrees/<repo-folder-name>" --force 2>/dev/null
git branch -D "$BRANCH_NAME" 2>/dev/null
rm -rf "<PlanFolder>/worktrees/<repo-folder-name>"
```

**Note on stale directories:** If a stale worktree directory exists and you run `git -C <stale-dir> status`, git silently walks up the parent chain and reports the state of the main repo — making it look like the "worktree" is simply on `main`. Do not trust that output. Before assuming a prior worktree is intact, verify with `git -C <main-repo> worktree list | grep <path>` or check that `<worktree-path>/.git` exists.

1. Create worktree branching from the remote default branch:

```bash
cd <original-repo-path>
git fetch origin
PLAN_FOLDER_NAME=$(basename "<PlanFolder>")
PLAN_ID=$(echo "$PLAN_FOLDER_NAME" | grep -oP '^\d+')
SAFE_TITLE=$(echo "$PLAN_FOLDER_NAME" | sed 's/^[0-9]\+-//')
BRANCH_NAME="tendril/$PLAN_ID-$SAFE_TITLE"
git worktree add "<PlanFolder>/worktrees/<repo-folder-name>" -b "$BRANCH_NAME" "origin/<default-branch>"
```

Example:

```bash
cd <RepoPath>
git fetch origin
git worktree add "<PlanFolder>/worktrees/<RepoName>" -b "tendril/<PlanId>-<SafeTitle>" origin/master
```

**Important:** Always branch from `origin/<default-branch>`, not local HEAD. This ensures the PR only contains the plan's commits, not any unpushed local work.

4. After creating the worktree, **verify the `.git` file exists** and fail fast if it's missing:

```bash
if [ ! -f "<PlanFolder>/worktrees/<repo-folder-name>/.git" ]; then
    echo "ERROR: Worktree creation failed - .git file missing at <PlanFolder>/worktrees/<repo-folder-name>/.git"
    echo "This indicates git worktree add did not fully initialize the worktree."
    exit 1
fi
cat "<PlanFolder>/worktrees/<repo-folder-name>/.git"
```

This ensures ExecutePlan fails immediately if worktree creation is incomplete, rather than leaving orphaned directories that trigger warnings during cleanup.

### 2.5. Setup Frontend Dependencies (JavaScript/TypeScript Projects Only)

**Note:** This section applies only to projects using npm/pnpm. Skip if not applicable.

**!CRITICAL: Frontend builds in worktrees have known issues with npm package module resolution that cause 15-25 minute timeouts. Follow this workaround to avoid them.**

Frontend directories are detected by the presence of `package.json` files in the repo. Find them by scanning the worktree:

```bash
find "<worktree-path>" -name "package.json" -not -path "*/node_modules/*" -exec dirname {} \;
```

#### Cleanup Leftover Files

Before setting up frontend dependencies, clean up any `.npmrc` files left from previous crashed runs:

```bash
pwsh -NoProfile -File "$env:TENDRIL_HOME/Promptwares/ExecutePlan/Tools/Cleanup-WorktreeFrontend.ps1" -WorktreeRoot "<PlanFolder>/worktrees"
```

This removes temporary `.npmrc` files with auth tokens while preserving tracked files.

#### Default Path (Most Plans)

If the plan does **NOT** modify frontend code (`.tsx`, `.ts`, `.css` files in frontend directories):

1. **Copy pre-built artifacts** from the original repo into each worktree. For each frontend directory that has a `dist/` folder in the original repo, copy it to the corresponding worktree path:

```bash
# For each frontend dir with dist/ in the original repo, copy to worktree
for dist_dir in $(find "<original-repo-path>" -name "dist" -path "*/frontend/dist" -type d); do
  relative_path="${dist_dir#<original-repo-path>/}"
  parent_dir=$(dirname "$relative_path")
  mkdir -p "<worktree-path>/$parent_dir"
  cp -r "$dist_dir" "<worktree-path>/$parent_dir/"
done
```

2. **Create `.npmrc`** in each frontend directory preemptively (in case builds need to load frontend resources):

```bash
for frontend_dir in $(find "<worktree-path>" -name "package.json" -not -path "*/node_modules/*" -exec dirname {} \;); do
  echo "node-linker=hoisted" > "$frontend_dir/.npmrc"
done
```

3. **Skip `pnpm install`** entirely — the copied artifacts are sufficient for build and tests.

#### Exception Path (Frontend Code Changes)

If the plan **modifies frontend code** (adding/editing `.tsx`, `.ts`, `.css` files), you MUST rebuild:

1. **Create `.npmrc`** with `node-linker=hoisted` in each frontend directory (required for pnpm in worktrees)
2. **Run `pnpm install`** in each frontend directory that has a `package.json`
3. **Run `pnpm run build`** to regenerate `dist/`
4. Be prepared for resolution failures — if `pnpm install` fails after 2 attempts, document the failure and recommend the user manually fix the lockfile

**Note:** The `Setup-WorktreeFrontend.ps1` tool can automate authentication and `.npmrc` creation, but by default it also runs `pnpm install`. Only use it for the Exception Path when you need a full rebuild.

### 3. Handle Cross-Repo References

Projects may reference other repos via absolute paths in project files (e.g. `.csproj`, `go.mod`, `package.json`).

These paths point to the original repos, not the worktree copies. Since we only modify files in the worktree, this is usually fine — the build references the original (stable) code.

**Do NOT modify project reference paths.** If a build fails because of cross-repo references, work around it by building from the worktree directory which inherits the original's references.

### 4. Implement

Work exclusively in the worktree directories. Follow the plan's latest revision:

1. **Problem** — Understand what needs to be done
2. **Solution** — Execute the implementation steps in the worktree
3. **Tests** — Write and run all tests specified in the plan

### 5. Commit

Make logically grouped commits in the worktree(s). Each commit should be a coherent unit of work.

Before each commit, run formatting/linting as defined by the project's verifications in `config.yaml`. The exact commands depend on your stack's verification definitions.

**Example patterns** (actual commands come from config.yaml verifications):

```bash
# Get changed files from this execution's commits
CHANGED_FILES=$(git diff --name-only --diff-filter=ACM HEAD~1)

# Run your formatter on changed files (examples):
# - .NET: dotnet format --include <files>
# - JavaScript: npm run format <files>
# - Python: black <files>
# - Go: gofmt -w <files>
```

If your formatter requires a workspace/solution file that isn't in the current directory, pass it as an explicit argument. Check `Memory/` for repo-specific workspace paths.

Commit messages should reference the plan ID:

```
[01105] Add settings app with config display
```

After all commits, verify no uncommitted files remain:

```bash
git status
```

If there are uncommitted changes, either commit them or discard them with a clear reason. The worktree must be clean.

### 5.5. Generate Summary

After all implementation commits are made, create `<PlanFolder>/artifacts/summary.md` summarizing what was done.

The summary should follow this structure:

~~~markdown
# Summary

## Changes

<Brief description of what was implemented — 2-3 sentences max>

## API Changes

<List any new/changed/removed public APIs: classes, methods, properties, endpoints, CLI commands, config keys. Use code formatting. If no API changes, write "None.">

## Files Modified

<Bulleted list of key files changed, grouped by category. Don't list every file — focus on the important ones.>
~~~

Focus on **what changed** (past tense), not what the plan said to do. Emphasize API surface changes — new classes, renamed methods, added properties, changed signatures — since these affect consumers.

Update the summary after verification fixes too — if verifications cause additional commits, append those changes to the summary.

### 5.7. Generate Recommendations

**REQUIRED STEP** — After implementation, actively reflect on what you observed during this plan's execution. Consider each of the following categories and write down any findings:

1. **Follow-up work** — Did you notice functionality that should be extended, edge cases not covered, or related features that would complement this change?
2. **Code quality** — Did you encounter confusing code, missing documentation, inconsistent patterns, or technical debt in the files you touched or read?
3. **Bugs** — Did you notice any unrelated bugs, broken tests, or incorrect behavior in surrounding code?
4. **Optimizations** — Are there performance improvements, unnecessary complexity, or refactoring opportunities in the area you worked in?

If you identified items in ANY category, write them to `<PlanFolder>/artifacts/recommendations.yaml`:

```yaml
- title: "Short descriptive title"
  description: |
    Markdown description with context and location.
  state: Pending
```

**YAML quoting rules:** Titles containing backticks, colons, brackets, braces, or other YAML special characters MUST be double-quoted. Alternatively, use block scalar style (`>` or `|`) for values with special characters.

Do NOT include items that are part of the current plan's scope.

Do NOT include recommendations about code formatting, linting, or style issues (e.g., line wrapping, indentation, trailing whitespace, import ordering). These are handled automatically by DotnetFormat and FrontendLint verifications.

If after genuine reflection you found nothing noteworthy, skip the file — but this should be rare. Most plans touch enough code to surface at least one observation.

### 6. Document Commits

Update `plan.yaml` in the plan folder (NOT in the worktree). Use the Edit tool on the original `plan.yaml` at the `PlanFolder` path.

Append each commit hash to the `commits` list:

```yaml
commits:
  - abc1234
  - def5678
```

Also populate the `verifications` list from the plan revision. Set checked items (`- [x]`) to `Pending` and unchecked items (`- [ ]`) to `Skipped`:

```yaml
verifications:
  - name: DotnetBuild
    status: Pending
  - name: DotnetTest
    status: Skipped
```

If the plan references other plans (e.g. split-from, follow-up), add them to `relatedPlans`:

```yaml
relatedPlans:
  - <PlansDirectory>/01100-OriginalPlan
```

### 7. Run Verifications

Create a `verification/` directory in the plan folder if it doesn't exist.

Check the `## Verification` section in the plan revision for checked items (`- [x]`). Skip unchecked items (`- [ ]`).

For each checked verification:

1. Send a status message: `Invoke-RestMethod -Uri "$env:TENDRIL_URL/api/jobs/$env:TENDRIL_JOB_ID/status" -Method Post -Body ('{"message":"Verifying: <Name>"}') -ContentType "application/json" -ErrorAction SilentlyContinue`
2. Look up its `prompt` in the `verifications` list in `config.yaml`
3. Execute the prompt in the worktree directory
4. If it fails: diagnose, fix the issue, **commit the fix** (e.g. `[01105] Fix lint errors from DotnetBuild`), and re-run. Repeat until it passes (fail the plan after 3+ failed attempts).
5. Document all fix commits in `plan.yaml` just like implementation commits.
6. Update the verification's `status` in `plan.yaml` to `Pass` or `Fail`.

**!IMPORTANT: Every verification MUST produce a report** at `<PlanFolder>/verification/<VerificationName>.md`:

```markdown
# <VerificationName>

- **Date:** <CurrentTime>
- **Result:** Pass / Fail
- **Attempts:** <number>

## Output

<command output or summary>

## Fixes Applied

<list of fix commits made during this verification, or "None">

## Issues Found

<any remaining issues, or "None">
```

A verification is not complete without its report. If the report file does not exist after running a verification, the plan should fail.

### 8. Final Clean Check

After all verifications pass:

1. Kill any remaining sample processes from the plan's artifacts directory:
   ```bash
   powershell.exe -NoProfile -Command "\$planFolder = '<PlanFolder>'.Replace('\\', '\\\\'); Get-Process -ErrorAction SilentlyContinue | Where-Object { \$_.Path -and \$_.Path -match [regex]::Escape(\$planFolder) -and \$_.Path -match '\\\\artifacts\\\\sample\\\\bin\\\\' } | ForEach-Object { Write-Host \"Killing zombie process: \$(\$_.ProcessName) (PID \$(\$_.Id))\"; \$_ | Stop-Process -Force -ErrorAction SilentlyContinue }"
   ```

2. Clean up temporary `.npmrc` files created in Step 2.5:
   ```bash
   pwsh -NoProfile -File "$env:TENDRIL_HOME/Promptwares/ExecutePlan/Tools/Cleanup-WorktreeFrontend.ps1" -WorktreeRoot "<PlanFolder>/worktrees"
   ```

3. Run `git status` in every worktree. If there are any uncommitted files (from verification fixes, generated files, etc.), commit or discard them. The worktrees must be completely clean before finishing.

### 8.5. Worktree Lifecycle

Worktrees are **not** cleaned up by ExecutePlan. They remain on disk so that MakePr can push branches and create PRs directly from the worktree.

**Cleanup happens later, in two places:**
1. **MakePr Step 5** — cleans up worktrees after PRs are created and (for yolo-rule repos) merged.
2. **WorktreeCleanupService** — safety net that runs every 30 minutes and removes worktrees for plans in terminal states (Completed, Failed, Skipped) after a 10-minute grace period.

**Git branches are preserved** until MakePr consumes them — only the worktree filesystem directories are removed.

**Manual inspection:** If you need to inspect worktrees after failure, check the plan folder's `worktrees/` directory before MakePr runs. After PR creation, worktrees are cleaned up automatically. You can also temporarily pause WorktreeCleanupService if needed for extended debugging.

### 9. Plan State

The launcher script handles state transitions (Completed/Failed) based on exit code.

### Ambiguity Handling

You are running in non-interactive mode and CANNOT ask questions. If you are unsure about requirements, encounter conflicting instructions, or cannot find referenced files — STOP and fail with a clear message explaining what needs clarification. Do NOT guess when uncertain.

### Rules

- All work happens in worktree directories, never in the original repos
- Make logically grouped commits — not one giant commit
- Worktrees must be clean (no uncommitted files) when finished
- Document all commit hashes in `plan.yaml`
- Follow the plan instructions exactly as written
- Do NOT skip tests or pre-commit formatting
- Commit messages must reference the plan ID
- All `file:///` paths in plans should be converted to Windows paths when needed
- Do NOT commit artifact files (screenshots, images) to the repo. Test artifacts belong in `<PlanFolder>/artifacts/` only — MakePr handles uploading them to persistent storage.
- Private npm packages (like `@ivy-interactive/ivy-design-system`) require authentication via `.npmrc`. The Setup-WorktreeFrontend.ps1 tool handles this automatically. Credentials come from NPM_TOKEN env var or .NET user secrets (Npm:RegistryToken).
- Do NOT use `subst` to create drive letter mappings for worktree paths. The plans directory is already symlinked to a short path to avoid long-path issues. Using `subst` creates phantom drives that are never cleaned up.
