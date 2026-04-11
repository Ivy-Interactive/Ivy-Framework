---
searchHints:
  - release notes
  - changelog
  - version history
  - updates
  - what's new
icon: ScrollText
---

# Release Notes

<Ingress>
Version history, new features, improvements, and bug fixes for each Tendril release.
</Ingress>

## 1.0.14 (2026-04-10)

### Features

- **Job priority queue** — Plans are now executed in priority order. Bug-level plans run before NiceToHave, ensuring critical fixes land first.
- **Import Issues from GitHub** — Import existing GitHub issues directly into Tendril as draft plans via the new Import dialog.
- **Multi-project plan creation** — The Create Plan dialog now supports selecting multiple projects, aggregating their repos into a single plan.
- **WorktreeLifecycleLogger** — Centralized audit trail for worktree create, cleanup, and failure events across PlanReaderService, WorktreeCleanupService, and JobService.
- **Advanced Settings tab** — New tab in Setup for configuring lower-level options.

### Improvements

- **Progressive health check feedback** — Health checks now stream individual results as they complete instead of waiting for all checks to finish.
- **PR status stored in SQLite** — PR merge status is now cached in the local database with a background sync service, reducing GitHub API calls.
- **PlanWatcher simplified** — Replaced heavy FileSystemWatcher usage with a simpler approach to avoid buffer overflow from worktree churn.
- **Worktree diagnostic logging** — Added fail-fast checks for missing `.git` files and improved error messages for worktree creation failures.
- **Recursive worktree artifact detection** — ExecutePlan now detects and removes nested worktree artifacts left in the Plans directory from prior runs.
- **Defensive dictionary access** — MakeSoftwareRow uses `GetValueOrDefault` to prevent KeyNotFoundException in edge cases.

### Bug Fixes

- Fixed Gemini health check opening browser windows during authentication.
- Fixed `anyAgentHealthy` check to use installation status for Gemini agent.
- Fixed ConfigService constructor testability.
- Fixed YAML parsing errors in `recommendations.yaml`.
- Removed redundant Watch Remove from `Ivy.Tendril.csproj`.
- Removed unused `_prStatusCache` from GithubService.

## 1.0.12 (2026-04-10)

### Features

- **Multi-agent support** — Tendril now supports multiple coding agents (Claude, Codex, Gemini) with configurable profiles (deep, balanced, quick) per agent.
- **Windows installer** — New `install.ps1` script for streamlined Windows installation.
- **Doctor command** — Run `tendril doctor` to diagnose configuration and environment issues.

### Improvements

- **Documentation overhaul** — Comprehensive rewrite of all Tendril documentation with improved structure, examples, and onboarding flow.
- **Onboarding wizard polish** — Improved UI, copy, and step layout for the first-run experience.
- **Stack-agnostic promptwares** — Removed stack-specific references from ExecutePlan, MakePlan, and other promptwares to support any tech stack via `config.yaml` verifications.
- **Replaced FolderInput with TextInput** — Simplified path input across Tendril apps.

### Bug Fixes

- Fixed `TENDRIL_HOME` environment variable handling in tests.
- Added error handling to `PlatformHelper.OpenInTerminal` and `OpenInFileManager`.
- Added `File.Exists` check before reading `plan.yaml` in PlanReaderService.

## 1.0.9 (2026-04-09)

### Features

- **Stable NuGet releases** — Tendril now publishes stable versioned NuGet packages using `Directory.Build.props` for centralized versioning.
- **SQLite database** — Local data storage for plans, jobs, and PR status with migration support.
- **Recommendations system** — Plans can now generate follow-up recommendations that are surfaced in the Recommendations app.
- **Plan lifecycle management** — Full plan state machine: Draft, Approved, Executing, Review, Completed, Failed, with automatic transitions.

### Improvements

- **Cost tracking** — Per-job cost and token tracking with dashboard visualization by project and promptware type.
- **Comprehensive job status enum** — String conversion support for all job statuses.
- **Error handling improvements** — Duplicate migration version detection and FTS5 error handling.

## 1.0.0 (2026-04-03)

### Features

- **Initial release** of Tendril plan management system.
- **Plan apps** — Dashboard, Review, Drafts, Jobs, Icebox, Pull Requests, Recommendations, and Trash views.
- **Promptwares** — MakePlan, ExecutePlan, MakePr, UpdatePlan, SplitPlan, ExpandPlan, and CreateIssue.
- **Cross-platform support** — macOS and Windows with automatic platform detection.
- **Worktree-based execution** — Plans execute in isolated git worktrees to keep the main repo clean.
- **Configurable verifications** — DotnetBuild, DotnetTest, DotnetFormat, FrontendLint, and CheckResult.
- **GitHub integration** — Automatic PR creation, status tracking, and merge detection.
- **Keyboard shortcuts** — `Ctrl+Alt+D` for new drafts, with customizable bindings.
