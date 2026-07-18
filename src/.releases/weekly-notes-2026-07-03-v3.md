# Ivy Framework Weekly Notes - Week of 2026-07-03 (Release 3)

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

This release adds new command-line options for running the local docs and samples, and implements streaming terminal output for improved interactivity.

## New Features & Improvements

### CLI & Tools
- **Docs and Samples Startup Options**: Added `--find-available-port` and `--browse` options to the `IvyDocs.ps1` and `IvySamples.ps1` startup scripts. This allows the host to automatically find an open port and open the web browser on launch.

### Terminal
- **Streaming Terminal Output**: Updated the terminal command runner to use streaming, providing real-time log outputs and enhanced feedback.

## What's Changed
* Release: Merge development into main by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/4704


**Full Changelog**: https://github.com/Ivy-Interactive/Ivy-Framework/compare/v1.3.5...v1.3.6
