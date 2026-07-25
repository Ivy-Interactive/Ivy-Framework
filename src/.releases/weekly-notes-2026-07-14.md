# Ivy Framework Weekly Notes - Week of 2026-07-14

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

This release prevents backspace key navigation in desktop apps, fixes toolbar icon spacing in `ContentInput`, implements SignalR connection retry backoff logic, bumps Rustino.NET for desktop notifications, and updates TypeScript to 7.0.2.

## New Features & Improvements

### Desktop & CLI
- **Backspace Navigation Prevention**: Disabled default browser back navigation on backspace keypresses in desktop apps when focus is not inside an active input field.
- **Desktop Notification Prompt Fix**: Upgraded `Rustino.NET` to version `0.3.9`, fixing an issue where macOS prompted "Where is use_default?" upon receiving the first desktop notification.
- **MSBuild Targets Tooling Fix**: Restored bare `vp` CLI executable invocation in MSBuild targets, fixing CI build errors caused by npm resolving an unrelated package.

### Core & Network
- **SignalR Retry Backoff**: Implemented progressive retry delay logic for SignalR backend connection attempts, ensuring smoother reconnect behavior during transient network interruptions.

### UI & Tooling
- **ContentInput Icon Spacing**: Adjusted top padding in `ContentInput` toolbars so attachment icons retain proper spacing.
- **TypeScript 7.0.2**: Upgraded TypeScript to version 7.0.2 across frontend packages and widgets.
