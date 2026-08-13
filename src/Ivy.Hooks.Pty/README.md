# Ivy.Hooks.Pty

Server-side PTY (pseudo-terminal) hosting for Ivy apps, built on [Porta.Pty](https://www.nuget.org/packages/Porta.Pty). Spawns a real process (a shell, `claude`, `codex`, ...) attached to a pseudo-terminal and streams its raw output to a `Ivy.Widgets.Xterm.Terminal` widget, or lets server code observe/capture that output directly.

## Three concepts, three directions

The `Terminal` widget and `UsePty` hook are routinely confused with each other because there are three distinct data paths:

| Concept                | Direction         | Carries                                                                 |
|-------------------------|--------------------|--------------------------------------------------------------------------|
| `Terminal.OnInput`      | browser → server   | keystrokes the user types (xterm `onData`), forwarded to PTY stdin       |
| `Terminal.Stream`       | server → browser   | raw PTY output bytes, one way, base64-encoded over SignalR                |
| `PtyOptions.OnOutput`   | server → server    | the only server-side tap on what the hosted process printed              |

`OnInput` never sees program output, and `Stream` is write-only towards the browser — so `PtyOptions.OnOutput` (and the related `PtyOptions.CaptureOutput` / `PtyHandle.Output`, below) is the only way for your own C# code to observe a hosted process.

## `UsePty`

```csharp
var pty = context.UsePty(
    OperatingSystem.IsWindows() ? ["cmd"] : ["bash"],
    workingDirectory,
    new PtyOptions { /* ... */ }
);

return new Terminal()
    .Stream(pty.Stream)
    .OnInput(pty.HandleInput)
    .OnResize(pty.HandleResize)
    .Closed(pty.Closed);
```

`UsePty` is itself a hook (backed by `UseState`/`UseEffect`/`UseRef`), so it must be called unconditionally and, per analyser rule `IVYHOOK005`, as the **first statement** in `Build()`. If its arguments need non-trivial computation, compute them in static helper methods and call those inline as arguments — do not compute them in statements preceding the call (see `ChatView.BuildClaudeCommandLine` in `Ivy.IvyML.Studio` for the pattern).

### `PtyOptions`

| Property           | Default       | Meaning                                                                                   |
|---------------------|---------------|---------------------------------------------------------------------------------------------|
| `WorkingDirectory`  | `null`        | Process working directory (overridable by `UsePty`'s own `workingDirectory` parameter).      |
| `Environment`       | `null`        | Extra environment variables merged over the parent process's environment. An empty/null value removes the key. |
| `Cols` / `Rows`     | `120` / `30`  | Initial PTY size.                                                                            |
| `OnOutput`          | `null`        | Invoked with decoded text as output arrives (see below).                                     |
| `CaptureOutput`     | `false`       | Opt-in: accumulate output into `PtyHandle.Output` (see below).                               |
| `MaxCaptureLength`  | `1_000_000`   | Character cap for the accumulated transcript. Only relevant when `CaptureOutput` is set.     |

### `PtyHandle`

`Stream`, `HandleInput`, `HandleResize`, `Kill`, `Closed`, `ExitCode` wire directly into a `Terminal` widget, as shown above. Two additional members exist purely for server-side observation:

- **`OnOutput`** is invoked on the PTY reader thread (not the render thread) every time a chunk of output is read, with the chunk's text decoded across chunk boundaries — a multi-byte UTF-8 sequence split across two reads decodes correctly instead of producing `�` replacement characters. The text still contains raw ANSI escape sequences; pipe it through `AnsiEscape.Strip` for readable text.
- **`Output`** (`PtyHandle.Output`) is the accumulated transcript when `PtyOptions.CaptureOutput` is set (empty string otherwise). It shares the same decode stream as `OnOutput`, is capped at `MaxCaptureLength` characters (keeping the *newest* output, dropping the oldest), and materializes a new string on each access — read it when you need it (e.g. on a poll interval or user action), not on every render.

`PtyHandle` is rebuilt on every render, but the underlying process, decoder, and transcript buffer live in hook state (a `UseRef`) and persist across renders exactly like `pty.Stream` already does.

## `AnsiEscape`

```csharp
var readable = AnsiEscape.Strip(pty.Output);
```

Strips ANSI/VT escape sequences (OSC, CSI, and other `ESC`-prefixed sequences) and non-whitespace control characters, while preserving newlines, carriage returns, and tabs so the result stays readable multi-line text. A C# port of the frontend's stripper in `Terminal.tsx`.

## Example: capturing output alongside a live terminal

See `src/widgets/Ivy.Widgets.Xterm/.samples/Apps/OutputCaptureApp.cs` for a full sample that shows a live `Terminal` next to a read-only pane rendering `AnsiEscape.Strip(pty.Output)`, refreshed on a `UseInterval` poll.
