# Ivy Framework Release Notes - Version 1.3.21 (2026-08-18)

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

This release brings PTY output capture and UTF-8 streaming fixes to `Ivy.Hooks.Pty`, process tree lifecycle improvements, Mac keyboard glyphs in tooltips, bottom-right toast positioning, sidebar collapsed footer fixes, TextBlock ellipsis truncation, and SignalR connection parameter encoding.

## New Features & Enhancements

### PTY Output Capture & UTF-8 Stream Decoding (`Ivy.Hooks.Pty`)
- **Output Capture**: Added opt-in transcript accumulation via `PtyOptions.CaptureOutput` and `MaxCaptureLength`, accessible directly via `PtyHandle.Output`.
- **ANSI Escape Stripping**: Added `AnsiEscape.Strip(string)` for converting terminal output with ANSI color/control codes into plain text (e.g. detecting server URLs printed by hosted child processes).
- **Process ID Accessor**: Added `PtyHandle.GetProcessId()` so parent processes can retrieve the PID of the spawned process and perform explicit tree termination via `Process.Kill(entireProcessTree: true)`.
- **Chunk-Boundary UTF-8 Decoding**: Hoisted a stateful `PtyOutputDecoder` so multi-byte UTF-8 character sequences that straddle 4KB read boundaries decode without replacement character (`U+FFFD`) corruption.

```csharp
var pty = UsePty(new PtyOptions
{
    CaptureOutput = true,
    MaxCaptureLength = 8000
});

// Read accumulated terminal output as plain text
var cleanText = AnsiEscape.Strip(pty.Output);
```

### UI & UX Improvements
- **Mac Keyboard Modifier Glyphs**: Tooltips displaying keyboard shortcuts on macOS now render native Mac keyboard glyphs (`⌘` for Command, `⌥` for Option, `⇧` for Shift, `⌃` for Control).
- **Sidebar Toggle Tooltip**: Added `Cmd+B` (`⌘B` on Mac) indicator to the sidebar toggle button tooltip.
- **Toast Positioning**: Repositioned toast notifications to appear in the bottom-right corner of the application.

## Bug Fixes & Stability

- **Redirected Pipe Exit Hangs**: Fixed `ProcessExtensions.WaitForExitAsync` to prevent indefinite hangs when child or grandchild processes inherit and hold redirected stdout/stderr pipes open.
- **SignalR Connection Parameter Encoding**: Safely URL-encoded connection parameters in `use-backend`, resolving infinite loading issues when opening tabs with special characters (e.g., `#`) in arguments.
- **Sidebar Collapsed Footer Width**: Ensured full-width alignment for the collapsed sidebar footer and its dropdown menu trigger.
- **DiffView Collapsed State Reset**: Keyed collapsed diff file state by file path and reset state upon diff change.
- **TextBlock Ellipsis Truncation**: Fixed text overflow truncation for `TextBlock` with `Block` variant and `Ellipsis` overflow mode.
