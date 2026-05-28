---
name: git-diff-patch-notes
description: >-
  Generates release patch notes from git diffs/commits between two tags or refs, excluding any frontend changes, and updates the corresponding GitHub release.
---

# Git Diff Patch Notes Generator

## Overview
This skill helps generate release patch notes by extracting commits between two git tags or refs, automatically excluding any frontend changes (such as updates to TSX/TS components under `src/frontend/` or any widget `frontend/` subdirectories), and updating the corresponding GitHub release.

> [!IMPORTANT]
> Always use the pre-authenticated `gh` CLI for all GitHub interactions (such as retrieving release details or publishing/editing release notes). Do not attempt to use browser tools or standard HTTP request tools to access `github.com` directly, as network request permissions may be restricted.

## Dependencies
None.

## Quick Start

To generate and publish patch notes between two versions:

1. Identify the starting tag (e.g., `v1.2.59`) and ending tag (e.g., `v1.2.60`).
2. Run the `GeneratePatchNotes.ps1` PowerShell script located in `src/.releases/` to extract and filter the relevant commits:
   ```powershell
   pwsh src/.releases/GeneratePatchNotes.ps1 -FromRef v1.2.59 -ToRef v1.2.60 -Output src/.releases/non_fe_commits.json
   ```
3. Draft a beautiful release notes list focusing entirely on backend C#, assembly, and framework core changes.
4. Save the drafted notes to `src/.releases/weekly-notes-YYYY-MM-DD.md`.
5. Update the corresponding GitHub release with the notes:
   ```powershell
   gh release edit v1.2.60 --notes-file src/.releases/weekly-notes-YYYY-MM-DD.md
   ```

## Utility Scripts

### `GeneratePatchNotes.ps1`

Lists and filters commits in a range.

**Usage:**
```powershell
pwsh src/.releases/GeneratePatchNotes.ps1 `
  -FromRef <FROM_REF> `
  -ToRef <TO_REF> `
  -RepoPath <PATH_TO_REPO> `
  -Output <OUTPUT_FILE_PATH>
```

**Parameters:**
- `-FromRef` (Required): Starting tag or commit.
- `-ToRef` (Required): Ending tag or commit.
- `-RepoPath` (Optional): Path to the git repository directory. Defaults to the repository root where the script is located.
- `-Output` (Required): Path to the output JSON file.

## Workflow

1. **Extract Commits**: Locate the target repository and identify the tag range. Run the `GeneratePatchNotes.ps1` script to output the non-frontend commits to a temporary JSON file.
2. **Review Output**: Read the generated JSON file. Each commit object includes the hash, author, subject, body, and changed files list.
3. **Draft Patch Notes**:
   - Organize the changes into categories such as `Features & Enhancements`, `Bug Fixes`, `Performance`, and `Miscellaneous`.
   - **CRITICAL**: Do NOT include any styling, layout, components, or other frontend-only changes (anything under paths containing `frontend/`) in the patch notes. If a commit touched both backend and frontend, only mention the backend effects.
   - Link back to PRs or issues if mentioned in the commit message (e.g., `#4500`).
   - Save the drafted notes to `src/.releases/weekly-notes-YYYY-MM-DD.md`.
4. **Update GitHub Release**:
   - Run the `gh` CLI to update the GitHub release notes for the target tag:
     ```powershell
     gh release edit <tag> --notes-file src/.releases/weekly-notes-YYYY-MM-DD.md
     ```
5. **Commit & Pull Request**: Create a feature branch, commit the markdown notes file, push it, and open a PR into `development`.

## Common Mistakes

- **Including FE changes**: Double-check the modified files and commit messages to ensure frontend visual updates, styling adjustments, or React-like component edits did not slip into the notes.
- **Missing tag path history**: If tags are not fetched locally or the repository path is wrong, the script will fail. Make sure the repository path is correct.
