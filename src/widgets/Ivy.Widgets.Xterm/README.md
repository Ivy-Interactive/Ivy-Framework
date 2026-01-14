# Ivy.Widgets.Xterm

A terminal emulator widget for Ivy Framework powered by [xterm.js](https://xtermjs.org/).

## Installation

```bash
dotnet add package Ivy.Widgets.Xterm
```

## Widgets

### Terminal

A fully-featured terminal emulator component.

**External React Libraries Used:**
- [@xterm/xterm](https://www.npmjs.com/package/@xterm/xterm) - Terminal emulator
- [@xterm/addon-fit](https://www.npmjs.com/package/@xterm/addon-fit) - Auto-fit terminal to container
- [@xterm/addon-web-links](https://www.npmjs.com/package/@xterm/addon-web-links) - Clickable URLs

#### Basic Usage

```csharp
using Ivy.Widgets.Xterm;

// Simple terminal with default settings
new Terminal()

// Terminal with dark theme and custom font
new Terminal()
    .DarkTheme()
    .FontSize(16)
    .CursorBlink(true)

// Terminal with initial content
new Terminal()
    .InitialContent("Welcome!\r\n$ ")

// Terminal with event handlers
new Terminal()
    .HandleData(data => Console.WriteLine($"User typed: {data}"))
    .HandleResize((cols, rows) => Console.WriteLine($"Resized: {cols}x{rows}"))
```

#### Props

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `Cols` | `int?` | `null` | Fixed column count (auto-fit if not set) |
| `Rows` | `int?` | `null` | Fixed row count (auto-fit if not set) |
| `FontSize` | `int` | `14` | Font size in pixels |
| `FontFamily` | `string` | `"Menlo, Monaco, 'Courier New', monospace"` | Font family |
| `LineHeight` | `double` | `1.0` | Line height multiplier |
| `CursorBlink` | `bool` | `true` | Enable cursor blinking |
| `CursorStyle` | `CursorStyle` | `Block` | Cursor style (`Block`, `Underline`, `Bar`) |
| `Scrollback` | `int` | `1000` | Lines to keep in scrollback buffer |
| `Theme` | `TerminalTheme?` | Dark theme | Terminal color theme |
| `InitialContent` | `string?` | `null` | Initial content to display |

#### Events

| Event | Args | Description |
|-------|------|-------------|
| `OnData` | `string` | Fired when user types in the terminal |
| `OnResize` | `int cols, int rows` | Fired when terminal dimensions change |
| `OnTitleChange` | `string` | Fired when terminal title changes (via OSC sequences) |

#### Themes

Built-in themes are available:

```csharp
// Dark theme (VS Code-like)
new Terminal().DarkTheme()

// Light theme
new Terminal().LightTheme()

// Custom theme
new Terminal().Theme(new TerminalTheme
{
    Background = "#000000",
    Foreground = "#00ff00",
    Cursor = "#00ff00",
    // ... other colors
})
```

#### TerminalTheme Properties

All color properties accept CSS color strings (hex, rgb, rgba):

- `Background`, `Foreground`, `Cursor`, `CursorAccent`, `Selection`
- `Black`, `Red`, `Green`, `Yellow`, `Blue`, `Magenta`, `Cyan`, `White`
- `BrightBlack`, `BrightRed`, `BrightGreen`, `BrightYellow`, `BrightBlue`, `BrightMagenta`, `BrightCyan`, `BrightWhite`

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
dotnet run Terminal.cs
```

## Creating New Widgets

See [CLAUDE.md](./CLAUDE.md) for detailed instructions on creating new external widgets.
