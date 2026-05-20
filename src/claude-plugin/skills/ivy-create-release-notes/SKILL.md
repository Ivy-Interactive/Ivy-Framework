---
name: ivy-create-release-notes
description: >
  Generate release notes / weekly notes for the Ivy Framework by analyzing git commits and code diffs since the last tag or a specific commit.
  Use when the user wants to compile patch notes, changelogs, or weekly updates for the project.
allowed-tools: Bash(git:*) Read Write Edit Glob Grep
effort: high
argument-hint: "[optional start tag or commit hash]"
---

# ivy-create-release-notes

Analyze recent changes and generate structured release/weekly notes for the Ivy Framework.

## Pre-flight: Read Learnings

If the file `.ivy/learnings/ivy-create-release-notes.md` exists in the project directory, read it first and apply any lessons learned from previous runs of this skill.

## Workflow

### 1. Plan and Determine Range
- Check git tags in the repository using `git tag --sort=-v:refname`.
- Identify the latest tag (e.g. `v1.2.56`).
- Ask the user to confirm the tag/commit range. By default, analyze changes from the latest release tag to the current `HEAD` (or the last 7 days).
- If the user specified a start commit or tag in their prompt, use that.

### 2. Download and Extract Commits
- Run the PowerShell script `src/.releases/CreateNotes.ps1` to download and extract commits for the selected period into the `src/.releases/Commits/` directory.
- This creates individual markdown files containing commit messages and code diffs for all commits.

### 3. Generate Release Notes
Depending on the local environment, choose one of the following paths:

#### Path A: Script Execution (If `claude` CLI is installed and configured)
- Allow the `CreateNotes.ps1` script to run the automated pipeline.
- It will invoke `LlmEach.ps1` which uses the local `claude` command-line utility to incrementally write changes to `src/.releases/weekly-notes-YYYY-MM-DD.md`.

#### Path B: Agent-Guided Fallback (Recommended if `claude` CLI is missing or errors)
If `claude` is not available, process the files using your own agent context:
1. Initialize the release notes file `src/.releases/weekly-notes-YYYY-MM-DD.md` (if it does not exist) with the header:
   ```markdown
   # Ivy Framework Weekly Notes - Week of YYYY-MM-DD

   > [!NOTE]
   > We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.
   ```
2. Read the commit files from `src/.releases/Commits/` sorted sequentially.
3. For each commit:
   - Analyze the commit message and file diffs.
   - Ignore internal refactorings, merge commits, or test-only changes that do not affect users of the framework/CLI.
   - Categorize the change (e.g., New Features, Component Updates, Breaking Changes, Bug Fixes).
   - Write clear, user-facing descriptions.
   - For new APIs or major updates, write a concise C# code block showing how to use the feature. Refer to `references/AGENTS.md` to ensure correct Ivy conventions.
   - Edit `src/.releases/weekly-notes-YYYY-MM-DD.md` to add/merge the changes cleanly.
4. Clean up any temporary files or the `Commits/` folder if requested.

### 4. Review and Finalize
- Read the generated `weekly-notes-YYYY-MM-DD.md` file.
- Verify that the layout, headers, and C# examples conform to the standard style (e.g. look at `weekly-notes-2026-04-07.md` for reference).
- Present the location and key highlights of the generated release notes to the user.
