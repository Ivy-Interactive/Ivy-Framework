# Ivy.Widgets.AnimatedStatusLabel

A small status label widget with a shimmering text animation and a spinning leading icon.
Intended as a lightweight "working…" indicator — drop it anywhere a long-running operation
is in progress.

## Installation

```bash
dotnet add package Ivy.Widgets.AnimatedStatusLabel
```

## Usage

```csharp
using Ivy.Widgets.AnimatedStatusLabel;

// While running
new AnimatedStatusLabel("Setting up verifications…", isComplete: false)

// Completed
new AnimatedStatusLabel("Done", isComplete: true)
    .RightLabel("4.2s")
```

## Props

| Prop          | Type      | Default | Description                                                  |
|---------------|-----------|---------|--------------------------------------------------------------|
| `StatusText`  | `string`  | `""`    | The status text. Shimmer animates while not complete.        |
| `IsComplete`  | `bool`    | `false` | When true, drops the shimmer and shows a static done state.  |
| `ShowIcon`    | `bool`    | `true`  | Leading icon (spinner while running, check when complete).   |
| `RightLabel`  | `string?` | `null`  | Optional right-aligned label.                                |

## Development

```bash
cd frontend && pnpm install && pnpm build
cd .. && dotnet build
```
