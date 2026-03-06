# Ivy.Widgets.Tiptap

A rich text editor widget for [Ivy Framework](https://github.com/Ivy-Interactive/Ivy) powered by [Tiptap](https://tiptap.dev/).

## Installation

```bash
dotnet add package Ivy.Widgets.Tiptap
```

## Usage

### Basic Usage with State

```csharp
using Ivy.Widgets.Tiptap;

var content = UseState("<p>Hello, world!</p>");

new TiptapInput(content);
```

### Controlled Usage

```csharp
new TiptapInput(
    value: "<p>Initial content</p>",
    onChange: e => Console.WriteLine($"Content changed: {e.Value}")
);
```

### With Placeholder

```csharp
new TiptapInput(content, placeholder: "Start typing...");
```

### Extension Methods

```csharp
content.ToTiptapInput()
    .Placeholder("Enter your text...")
    .AutoFocus()
    .ShowToolbar()
    .HandleFocus(() => Console.WriteLine("Focused"))
    .OnBlur(() => Console.WriteLine("Blurred"));
```

## Features

- Bold, Italic, Strikethrough, Code formatting
- Headings (H1, H2, H3)
- Bullet and Ordered lists
- Blockquotes and Code blocks
- Horizontal rules
- Undo/Redo support
- Customizable toolbar
- Placeholder text
- Read-only mode
- Focus/Blur event handlers

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Value` | `string` | `""` | The HTML content of the editor |
| `Placeholder` | `string?` | `null` | Placeholder text when empty |
| `Disabled` | `bool` | `false` | Disables the editor |
| `Editable` | `bool` | `true` | Makes the editor read-only when false |
| `AutoFocus` | `bool` | `false` | Automatically focus on mount |
| `ShowToolbar` | `bool` | `true` | Show/hide the formatting toolbar |
| `Nullable` | `bool` | `false` | Whether the value can be null |

## Events

| Event | Description |
|-------|-------------|
| `OnChange` | Fired when the content changes |
| `OnFocus` | Fired when the editor gains focus |
| `OnBlur` | Fired when the editor loses focus |

## License

Apache-2.0
