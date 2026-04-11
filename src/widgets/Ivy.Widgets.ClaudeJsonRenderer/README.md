# Ivy.Widgets.ClaudeJsonRenderer

A widget for rendering Claude Code's `--output-format=stream-json` output in a conversation-style UI.

## Installation

```bash
dotnet add package Ivy.Widgets.ClaudeJsonRenderer
```

## Widgets

### ClaudeJsonRenderer

Renders newline-delimited JSON events from Claude Code into a formatted conversation view with markdown rendering, collapsible tool use/result cards, and completion summaries.

**External React Libraries Used:**
- [react-markdown](https://www.npmjs.com/package/react-markdown) - Markdown rendering
- [rehype-highlight](https://www.npmjs.com/package/rehype-highlight) - Syntax highlighting

#### Basic Usage

```csharp
using Ivy.Widgets.ClaudeJsonRenderer;

// Render a stream
new ClaudeJsonRenderer()
    .JsonStream(myStreamOutput)

// With all options
new ClaudeJsonRenderer()
    .JsonStream(myStreamOutput)
    .AutoScroll(true)
    .ShowThinking(true)
    .ShowSystemEvents(true)
    .OnComplete(result => Console.WriteLine($"Done: {result}"))
```

#### Props

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `JsonStream` | `string?` | `null` | Newline-delimited JSON events from Claude Code |
| `AutoScroll` | `bool` | `true` | Auto-scroll to bottom as new events arrive |
| `ShowThinking` | `bool` | `false` | Show thinking/reasoning blocks |
| `ShowSystemEvents` | `bool` | `false` | Show system init events |

#### Events

| Event | Args | Description |
|-------|------|-------------|
| `OnComplete` | `string` | Fired with JSON result when stream completes |

## Development

### Building

1. Install frontend dependencies:

   ```bash
   cd frontend
   pnpm install
   ```

2. Build the frontend:

   ```bash
   pnpm build
   ```

3. Build the project from the root folder:

   ```bash
   dotnet build
   ```
