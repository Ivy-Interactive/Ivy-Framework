# Direct Main Branch Commits

## Context

Some plans may have their changes committed directly to the main branch instead of using the worktree workflow. This can happen when:
- A simple fix is made directly on main during development
- Emergency hotfixes bypass the normal workflow
- The change is so trivial it doesn't warrant a PR review

## Detection

MakePr detects this scenario when:
1. `plan.yaml` has `state: Building` and empty `commits: []` / `prs: []`
2. No worktrees exist in `<PlanFolder>/worktrees/`
3. A commit exists on the main branch that implements the plan's requested changes

## Handling

When detected, MakePr should:
1. Find the relevant commit on main (via `git log` search)
2. Update `plan.yaml`:
   - Set `state: Completed`
   - Add commit hash to `commits` list
   - Add commit URL to `prs` list (using GitHub commit URL format)
3. Skip all PR creation steps
4. Log the outcome as "Completed (No PR needed)"

## Example

Plan 02000 (2026-04-06): Remove unused Command import in llm_markdown.rs
- Fix was committed directly to main as `b901a893e`
- MakePr found the commit and updated plan.yaml accordingly
- No PR was created since the change was already on main

## Implementation Note

The commit URL format for the `prs` list should be:
`https://github.com/<owner>/<repo>/commit/<hash>`

This provides a clickable reference to the change even though no formal PR was created.
