---
name: git-diff-patch-notes
description: >-
  Generates release patch notes from git diffs/commits between two tags or refs, excluding any frontend changes.
---

# Git Diff Patch Notes Generator

## Overview
This skill helps generate release patch notes by extracting commits between two git tags or refs, automatically excluding any frontend changes (such as updates to TSX/TS components under `src/frontend/` or any widget `frontend/` subdirectories).

## Dependencies
None.

## Quick Start

To generate patch notes between two versions:

1. Identify the starting tag (e.g., `v1.2.59`) and ending tag (e.g., `v1.2.60`).
2. Run the `GeneratePatchNotes.ps1` PowerShell script located in `src/.releases/` to extract and filter the relevant commits:
   ```powershell
   pwsh src/.releases/GeneratePatchNotes.ps1 -FromRef v1.2.59 -ToRef v1.2.60 -Output src/.releases/non_fe_commits.json
   ```
3. Use the contents of the generated JSON file to summarize and draft a beautiful release notes list focusing entirely on backend C#, assembly, and framework core changes.

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
4. **Output to User**: Display the compiled patch notes in markdown.

## Common Mistakes

- **Including FE changes**: Double-check the modified files and commit messages to ensure frontend visual updates, styling adjustments, or React-like component edits did not slip into the notes.
- **Missing tag path history**: If tags are not fetched locally or the repository path is wrong, the script will fail. Make sure the repository path is correct.
