# Ivy Framework Weekly Notes - Week of 2026-07-02

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

This release focuses on strengthening process execution robustness under Windows, resolving high-severity dependencies vulnerabilities, preventing reentrancy crashes during server teardown, and refining UI/layout behaviors.

## Bug Fixes and Improvements

### Core & Server
- **Fix Reentrancy Crash during App Teardown**: Resolved a `Collection was modified` exception during server stop by snapshotting the disposables list before iterating and disposing.
- **Dependency Security Update**: Upgraded `Microsoft.OpenApi` from `2.0.0` to `2.9.0` to address high-severity vulnerability GHSA-v5pm-xwqc-g5wc.
- **Windows CLI Argument Escaping**: Implemented Windows argument escaping in `UsePty` for robust command-line execution.

### UI & Layout
- **Fix Sidebar News Card Dismissal**: Fixed an issue where the dismissed sidebar news card would still block pointer events and prevent users from clicking underlying menu items.
- **Fix Loading Screen Overflow**: Eliminated unwanted scrollbar on the loading screen by constraining the layout to `h-full` instead of `h-screen`.
- **Fix Text Input Suffix Button Clipping**: Resolved clipping of suffix buttons inside text input fields by rounding/insetting embedded affix buttons and tightening cell padding.
- **Activity Heatmap Background**: Removed background color from the activity heatmap to match themes/dark mode better.
