# Refactor Icons Enum

The `Icons` enum has been updated to align with Lucide React 0.562.0. Some icons have been renamed, and others removed.

## Goal

Find usages of specific `Icons` enum members and update them to their new names or remove them if they no longer exist.

## Locate Code

Search for usages of `Icons.{Name}` throughout the codebase.

## Required Changes

### Renames

Apply the following replacements:

- `Icons.AlignCenter` -> `Icons.TextAlignCenter`
- `Icons.Chrome` -> `Icons.Chromium`
- `Icons.Justify` -> `Icons.TextAlignJustify` (if applicable check context)
- `Icons.Left` -> `Icons.TextAlignLeft` (if applicable check context)
- `Icons.Right` -> `Icons.TextAlignRight` (if applicable check context)
<!-- Add other 6 renamed icons if known, otherwise instructions to check compiler errors for "Icons does not contain definition for X" -->

### Removals

The following icons have been removed. If found, you must decide a suitable replacement or remove the usage.

- `Icons.FileCheck2` -> Use `Icons.FileCheck`
- `Icons.Text` -> Use `Icons.FileText` or similar
- `Icons.FileJson` -> Use `Icons.FileCode` or generic `Icons.File`
- `Icons.LetterText` -> Use `Icons.FileText`

**General Strategy for Removals:**
If an icon like `FileCheck2` is missing, try removing the suffix (e.g., `FileCheck`). For others, look for the closest semantic match in the `Icons` enum.

## Verification

After changes, ensure the code compiles without "does not contain definition for 'X'" errors related to the `Icons` enum.
