# Ivy Framework Weekly Notes - Week of 2026-06-29 (Release 3)

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

This release introduces improvements to the desktop client clipboard fallback on macOS, enhancements to the plugin configuration UI, and layout fixes for sheets.

## New Features & Improvements

### Plugin & Configuration UI
- **Boolean Plugin Settings**: Replaced the true/false dropdown with a checkbox for boolean plugin configuration fields. Long descriptions on these fields now wrap naturally without truncation.

### Desktop Client
- **macOS Clipboard Fallback**: Implemented native macOS clipboard copy fallback using `pbcopy` when running in the desktop environment.

## Bug Fixes and Improvements

### UI & Layout
- **Sheet Padding**: Added default bottom padding to sheet layouts to prevent content from rendering too close to the screen edge.

## What's Changed
* Use checkbox for boolean plugin config fields by @zachwolfe in https://github.com/Ivy-Interactive/Ivy-Framework/pull/4669
* [00604] Add Bottom Default Padding to Sheets by @dcrjodle in https://github.com/Ivy-Interactive/Ivy-Framework/pull/4670
* fix(desktop): implement native macOS clipboard copy fallback using pbcopy by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/4671
* Release: Merge development into main for v1.3.0 by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/4672


**Full Changelog**: https://github.com/Ivy-Interactive/Ivy-Framework/compare/v1.2.72...v1.3.0
