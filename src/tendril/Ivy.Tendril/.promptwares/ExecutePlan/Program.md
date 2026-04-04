# ExecutePlan

Execute an approved plan in isolated git worktrees.

## Context

The firmware header contains:

- **Args** / **PlanFolder** — path to the plan folder
- **ConfigPath** — absolute path to config.yaml
- **CurrentTime** — current UTC timestamp

Read the plan structure in `../.shared/Plans.md`.
Read `config.yaml` (from `ConfigPath`) for project repos and context.

The launcher script sets the working directory to the project's primary repo.

**Note:** Plans are often executed multiple times. For example, a reviewer may not be satisfied with the first execution and sends the plan back to Draft with comments (via UpdatePlan). When re-executing, the worktree branch from the previous run may already exist — handle this gracefully (delete old worktree first, or create with a new branch suffix). Check for existing artifacts and verification reports from prior runs.

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

### 2. Create Worktrees

For each repo listed in `plan.yaml` `repos` (or the project's repos from `config.yaml` if empty):

1. Fetch latest from remote: `git fetch origin`
2. Detect the default branch: `git symbolic-ref refs/remotes/origin/HEAD | sed 's|refs/remotes/origin/||'` (usually `master` or `main`)
3. If the worktree or branch already exists from a prior execution, remove it first:

```bash
git worktree remove "<PlanFolder>/worktrees/<repo-folder-name>" --force 2>/dev/null
git branch -D "plan-<planId>-<repo-folder-name>" 2>/dev/null
```

1. Create worktree branching from the remote default branch:

```bash
cd <original-repo-path>
git fetch origin
git worktree add "<PlanFolder>/worktrees/<repo-folder-name>" -b "plan-<planId>-<repo-folder-name>" "origin/<default-branch>"
```

Example:

```bash
cd <RepoPath>
git fetch origin
git worktree add "<PlanFolder>/worktrees/<RepoName>" -b "plan-<PlanId>-<RepoName>" origin/master
```

**Important:** Always branch from `origin/<default-branch>`, not local HEAD. This ensures the PR only contains the plan's commits, not any unpushed local work.

### 3. Handle Cross-Repo References

Projects may reference other repos via absolute paths in `.csproj` files (e.g. `<ProjectReference Include="/path/to/other-repo/src/Project.csproj" />`).

These paths point to the original repos, not the worktree copies. Since we only modify files in the worktree, this is usually fine — the build references the original (stable) code.

**Do NOT modify project reference paths.** If a build fails because of cross-repo references, work around it by building from the worktree directory which inherits the original's references.

### 4. Implement

Work exclusively in the worktree directories. Follow the plan's latest revision:

1. **Problem** — Understand what needs to be done
2. **Solution** — Execute the implementation steps in the worktree
3. **Tests** — Write and run all tests specified in the plan

### 5. Commit

Make logically grouped commits in the worktree(s). Each commit should be a coherent unit of work.

Before each commit, run formatting/linting:

**Frontend files** (under `src/frontend/`):

```bash
cd src/frontend && npm run format && npm run lint:fix && cd ../..
```

**C# files**:

```bash
dotnet format
```

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

Do NOT include items that are part of the current plan's scope.

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

After all verifications pass, run `git status` in every worktree. If there are any uncommitted files (from verification fixes, generated files, etc.), commit or discard them. The worktrees must be completely clean before finishing.

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
