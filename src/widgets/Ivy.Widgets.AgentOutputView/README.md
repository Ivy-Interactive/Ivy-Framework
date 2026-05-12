# Ivy.Widgets.AgentOutputView

A terminal-themed view for rendering streaming agent output. Sits above per-provider
JSON parsers so the visible chrome (animated status label, collapsed tool-call cards,
dark CLI theme) is uniform across Claude, Codex, Gemini, OpenCode, and Copilot.

## Installation

```bash
dotnet add package Ivy.Widgets.AgentOutputView
```

## Usage

```csharp
using Ivy.Widgets.AgentOutputView;

new AgentOutputView()
    .Provider("claude")           // or "codex", "gemini", "opencode", "copilot"
    .Stream(myStream)
    .AutoScroll()
    .ShowStatusLabel()
    .Height(Size.Full())
```

## Props

| Prop                  | Type      | Default   | Description                                                       |
|-----------------------|-----------|-----------|-------------------------------------------------------------------|
| `Provider`            | `string`  | `"claude"`| Agent provider id; selects the matching JSON parser.              |
| `JsonStream`          | `string?` | `null`    | Pre-buffered NDJSON events.                                       |
| `Stream`              | stream    | `null`    | Live streaming input.                                             |
| `AutoScroll`          | `bool`    | `true`    | Auto-scroll to bottom as new events arrive.                       |
| `ShowThinking`        | `bool`    | `false`   | Show thinking/reasoning blocks.                                   |
| `ShowSystemEvents`    | `bool`    | `false`   | Show system init events.                                          |
| `ShowStatusLabel`     | `bool`    | `true`    | Show the animated status label at the bottom.                     |
| `StatusLabelOverride` | `string?` | `null`    | Override the auto-derived status text.                            |
| `ResetToken`          | `int`     | `0`       | Bump to clear accumulated output.                                 |

## Events

| Event        | Args     | Description                                  |
|--------------|----------|----------------------------------------------|
| `OnComplete` | `string` | Fires with the result JSON when the agent finishes. |
