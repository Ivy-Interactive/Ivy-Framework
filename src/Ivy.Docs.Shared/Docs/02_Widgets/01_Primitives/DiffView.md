---
searchHints:
  - diff
  - git diff
  - code review
  - compare
  - unified diff
  - split diff
  - react-diff-view
---

# DiffView

<Ingress>
The DiffView widget displays unified diffs (such as git diff output) in either unified or split view mode. It is powered by [react-diff-view](https://github.com/otakustay/react-diff-view).
</Ingress>

## Installation

```terminal
dotnet add package Ivy.Widgets.DiffView
```

## Basic Usage

```csharp
using Ivy.Widgets.DiffView;

new DiffView()
    .Diff(myDiffString)
```

## Split View

```csharp
new DiffView()
    .Diff(myDiffString)
    .Split()
    .OldRevision("a/file.txt")
    .NewRevision("b/file.txt")
```

## Line Click Handler

```csharp
new DiffView()
    .Diff(myDiffString)
    .OnLineClick(line => Console.WriteLine($"Line {line} clicked"))
```

## Props

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `Diff` | `string?` | `null` | Unified diff string (git diff output) |
| `ViewType` | `DiffViewType` | `Unified` | View mode (`Unified` or `Split`) |
| `Language` | `string?` | `null` | Language hint for syntax highlighting |
| `OldRevision` | `string?` | `null` | Old file revision name displayed in the header |
| `NewRevision` | `string?` | `null` | New file revision name displayed in the header |

## Events

| Event | Value Type | Description |
|-------|-----------|-------------|
| `OnLineClick` | `int` | Fired when a line gutter is clicked, with the line number |
