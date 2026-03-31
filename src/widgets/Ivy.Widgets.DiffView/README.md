# Ivy.Widgets.DiffView

A diff viewer widget for Ivy Framework powered by [react-diff-view](https://github.com/otakustay/react-diff-view).

## Installation

```bash
dotnet add package Ivy.Widgets.DiffView
```

## Widgets

### DiffView

A unified/split diff viewer component for displaying git-style diffs.

**External React Libraries Used:**
- [react-diff-view](https://www.npmjs.com/package/react-diff-view) - Diff viewer component
- [unidiff](https://www.npmjs.com/package/unidiff) - Unified diff parser

#### Basic Usage

```csharp
using Ivy.Widgets.DiffView;

// Simple unified diff
new DiffView()
    .Diff(myDiffString)

// Split view with revision names
new DiffView()
    .Diff(myDiffString)
    .Split()
    .OldRevision("a/file.txt")
    .NewRevision("b/file.txt")

// With line click handler
new DiffView()
    .Diff(myDiffString)
    .OnLineClick(line => Console.WriteLine($"Line {line} clicked"))
```

#### Props

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `Diff` | `string?` | `null` | Unified diff string (git diff output) |
| `ViewType` | `DiffViewType` | `Unified` | View mode (`Unified` or `Split`) |
| `Language` | `string?` | `null` | Language hint for syntax highlighting |
| `OldRevision` | `string?` | `null` | Old file revision name |
| `NewRevision` | `string?` | `null` | New file revision name |

#### Events

| Event | Args | Description |
|-------|------|-------------|
| `OnLineClick` | `int` | Fired when a line gutter is clicked |

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
