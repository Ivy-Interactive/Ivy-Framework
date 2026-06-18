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
- Identify the latest tag (e.g. `v1.2.67`) and the previous tag (e.g. `v1.2.66`).
- Ask the user to confirm the tag/commit range. By default, analyze changes from the previous release tag to the latest release tag.

### 2. Extract and Filter Commits
- Run the PowerShell script `src/.releases/GeneratePatchNotes.ps1` to extract non-frontend commits for the selected range into a JSON file:
  ```bash
  pwsh src/.releases/GeneratePatchNotes.ps1 -FromRef <FromRef> -ToRef <ToRef> -Output src/.releases/Commits.json
  ```

### 3. Generate Release Notes File
1. Parse the generated `src/.releases/Commits.json` file.
2. Filter and categorize the commits (ignoring internal refactors, merge commits, or test-only changes that do not affect users of the framework/CLI).
3. Group changes into sections (e.g., `New Features`, `Bug Fixes and Improvements`, etc.).
4. Write clear, user-facing descriptions for each change.
5. Provide concise C# usage code blocks for new features or major API additions.
6. Initialize/write the release notes file `src/.releases/weekly-notes-YYYY-MM-DD.md` (e.g., `weekly-notes-2026-06-18.md`) with the header:
   ```markdown
   # Ivy Framework Weekly Notes - Week of YYYY-MM-DD

   > [!NOTE]
   > We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.
   ```
7. Commit and push the generated `weekly-notes-YYYY-MM-DD.md` file to the `development` branch, and remove the temporary `Commits.json` file.

### 4. Update GitHub Release Notes
1. Retrieve the existing body of the GitHub release:
   ```bash
   gh release view <tag> --json body -q .body
   ```
2. Combine the new weekly release notes text and the existing body, placing the new weekly notes at the top, before the "What's Changed" section.
3. Update the GitHub release description using `gh`:
   ```bash
   gh release edit <tag> --notes-file <path_to_combined_notes_file>
   ```

