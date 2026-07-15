# Ivy Framework Weekly Notes - Week of 2026-07-03 (Release 2)

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

This release introduces the `Dismissable` property to the Dialog API with close confirmation options, corrects Dialog vertical centering, fixes copy/paste issues in the Terminal widget, and resolves click hitbox bugs on checkboxes and sidebar styling.

## New Features & Improvements

### Dialogs
- **Non-Dismissable Dialogs**: Added support for non-dismissable dialogs. Using `.Dismissable(false)` prevents users from closing the dialog by clicking the backdrop or pressing the Escape key, while keeping the close (X) button active.
- **Close Confirmation Message**: You can now specify a confirmation message when setting a dialog as non-dismissable. Clicking the close (X) button will prompt the user with a confirmation alert before closing:
  
  ```csharp
  new Dialog("Settings")
      .Dismissable(false, confirmationMessage: "You have unsaved changes. Are you sure you want to close?")
  ```
- **Vertical Centering**: Corrected Dialog vertical centering to match alert dialog alignment by positioning them centered relative to the viewport.

### Terminal & CLI
- **Terminal Copy/Paste**: Resolved an issue with clipboard keyboard shortcuts inside the Xterm `Terminal` widget on macOS/Windows, allowing standard keyboard copying and pasting to work correctly.

### UI & Layouts
- **Checkbox Hitbox**: Fixed click target hitbox issues where checkbox, switch, and toggle labels could not be clicked to change the state.
- **Sidebar Styling**: Adjusted sidebar list item hover state colors to look cleaner and more consistent in both light and dark modes.

## What's Changed
* fix: adjust sidebar list items hover colors for light and dark modes by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/4699
* [00629] Add Dismissable Property to Dialog API by @dcrjodle in https://github.com/Ivy-Interactive/Ivy-Framework/pull/4683
* [00031] Fix Copy Paste in Xterm Widget by @nielsbosma in https://github.com/Ivy-Interactive/Ivy-Framework/pull/4701
* [00033] Fix Checkbox Label Click Hitbox by @nielsbosma in https://github.com/Ivy-Interactive/Ivy-Framework/pull/4702
* Release: Merge development into main by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/4703


**Full Changelog**: https://github.com/Ivy-Interactive/Ivy-Framework/compare/v1.3.4...v1.3.5
