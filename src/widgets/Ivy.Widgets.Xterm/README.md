# Ivy.Widgets.Xterm

A terminal emulator widget for Ivy Framework powered by [xterm.js](https://xtermjs.org/).

> **Naming note:** `Ivy.Widgets.Xterm.Terminal` (this widget) is a different type from `Ivy.Terminal`,
> a static display primitive documented in [14_Terminal.md](../../Ivy.Docs.Shared/Docs/02_Widgets/03_Common/14_Terminal.md).
> The samples in this package alias the import to avoid the clash:
> `using Terminal = Ivy.Widgets.Xterm.Terminal;`

## Installation

```bash
dotnet add package Ivy.Widgets.Xterm
```

## Widgets

### Terminal

A fully-featured terminal emulator component that renders a raw byte stream (typically from a
server-side PTY, see [Terminal + PTY](#terminal--pty) below).

**External React Libraries Used:**
- [@xterm/xterm](https://www.npmjs.com/package/@xterm/xterm) - Terminal emulator
- [@xterm/addon-fit](https://www.npmjs.com/package/@xterm/addon-fit) - Auto-fit terminal to container
- [@xterm/addon-web-links](https://www.npmjs.com/package/@xterm/addon-web-links) - Clickable URLs

#### Basic Usage

```csharp
using Terminal = Ivy.Widgets.Xterm.Terminal;

// Simple terminal with default settings
new Terminal()

// Terminal with initial content
new Terminal()
    .InitialContent("Welcome!\r\n$ ")

// Terminal with a loading overlay shown until the attached
// process writes its first (visible) output to the stream
new Terminal()
    .Stream(pty.Stream)
    .Loading("Starting Claude Code...")

// Terminal with event handlers
new Terminal()
    .OnInput(data => Console.WriteLine($"User typed: {data}"))
    .OnResize((cols, rows) => Console.WriteLine($"Resized: {cols}x{rows}"))
    .OnLinkClick(url => Console.WriteLine($"Link clicked: {url}"))
```

#### Props

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `Cols` | `int?` | `null` | Fixed column count (auto-fit if not set) |
| `Rows` | `int?` | `null` | Fixed row count (auto-fit if not set) |
| `CursorBlink` | `bool` | `true` | Enable cursor blinking |
| `CursorStyle` | `CursorStyle` | `Block` | Cursor style (`Block`, `Underline`, `Bar`) |
| `Scrollback` | `int` | `1000` | Lines to keep in scrollback buffer |
| `InitialContent` | `string?` | `null` | Initial content to display |
| `Closed` | `bool` | `false` | Marks the terminal as closed (e.g. the attached process exited); typically bound to `pty.Closed` |
| `AllowClipboard` | `bool` | `true` | Allow clipboard copy/paste inside the terminal |
| `AutoFocus` | `bool` | `true` | Automatically focus the terminal on mount so it receives keyboard input |
| `Loading` | `bool` | `false` | Show a loading overlay (spinner + text) until the first visible stream data arrives |
| `LoadingText` | `string?` | `"Loading..."` | Text shown in the loading overlay |
| `Background` | `Colors?` | `null` | Terminal background color |
| `Foreground` | `Colors?` | `null` | Terminal foreground (text) color |
| `Stream` | `IWriteStream<byte[]>?` | `null` | Raw output byte stream rendered by the terminal, typically `pty.Stream` |

#### Events

| Event | Args | Description |
|-------|------|-------------|
| `OnInput` | `string` | Fired when the user types in the terminal (raw keystroke data from xterm's `onData`) |
| `OnResize` | `TerminalSize` (`Cols`, `Rows`), with `Action<int, int>` and `Action<TerminalSize>` overloads | Fired when terminal dimensions change |
| `OnLinkClick` | `string` | Fired when the user clicks a detected URL in the terminal output |

## Terminal + PTY

`Terminal` only renders bytes and forwards keystrokes — it does not spawn or manage a process.
Pair it with `Ivy.Hooks.Pty`'s `UsePty` hook to host a real process:

```csharp
using Ivy.Hooks.Pty;
using Terminal = Ivy.Widgets.Xterm.Terminal;

// IVYHOOK005: UsePty must be the first statement in Build().
var pty = Context.UsePty(OperatingSystem.IsWindows() ? ["cmd"] : ["bash"], workingDirectory);

return new Terminal()
    .Stream(pty.Stream)
    .OnInput(pty.HandleInput)
    .OnResize(pty.HandleResize)
    .Closed(pty.Closed)
    .AllowClipboard();
```

See [ClaudeCodeApp.cs](.samples/Apps/ClaudeCodeApp.cs) for a complete example, and the
[`Ivy.Hooks.Pty` README](../../Ivy.Hooks.Pty/README.md) for how `Terminal.OnInput`, `Terminal.Stream`
and `PtyOptions.OnOutput`/`PtyOptions.CaptureOutput` relate to each other — they are three distinct,
one-directional data paths and are routinely confused.

## Development

### Building

1. Install frontend dependencies:

   ```bash
   cd frontend
   npm install
   ```

2. Build the frontend:

   ```bash
   npm run build
   ```

3. Build the project from the root folder:

   ```bash
   dotnet build
   ```

### Running Samples

```bash
cd .samples
dotnet run
```

The sample server uses an app shell with multiple demo apps (in `.samples/Apps/`):

- **Shell** — an interactive system shell (`cmd` on Windows, `bash` otherwise)
- **Hello Console** — runs the Spectre.Console demo app from `.console/HelloApp`
- **Claude Code** — runs the `claude` CLI in the terminal (requires Claude Code to be installed and on `PATH`)
- **Output Capture** — a shell paired with a read-only pane showing `PtyHandle.Output` (via `PtyOptions.CaptureOutput`), demonstrating chunk-boundary-safe UTF-8 decoding and `AnsiEscape.Strip`

## Creating New Widgets

See [CLAUDE.md](./CLAUDE.md) for detailed instructions on creating new external widgets.
