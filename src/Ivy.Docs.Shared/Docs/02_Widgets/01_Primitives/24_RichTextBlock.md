---
searchHints:
  - rich text
  - inline formatting
  - text run
  - mixed styling
  - bold italic
  - highlight
  - link
  - streaming text
---

# RichTextBlock

The `RichTextBlock` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) displays rich text composed of individually styled `TextRun` segments. Use it when you need mixed formatting within a single line or paragraph — for example, bold + normal + colored text together.

For uniform text styling, use [TextBlock](./01_TextBlock.md) instead.

## Basic Usage

Use `Text.Rich()` to create a `RichTextBuilder`, then chain run methods to add styled segments:

```csharp demo-below
public class RichTextBasicDemo : ViewBase
{
    public override object? Build()
    {
        return Text.Rich()
            .Run("Hello ")
            .Bold("world")
            .Run("! This is ")
            .Italic("rich text", color: Colors.Blue)
            .Run(".");
    }
}
```

## Word Auto-Spacing

When `Word` is true, a space is automatically prepended before the content (except for the first run). This is useful for composing sentences word-by-word:

```csharp demo-below
public class RichTextWordDemo : ViewBase
{
    public override object? Build()
    {
        return Text.Rich()
            .Word("This")
            .Word("is")
            .Word("automatically")
            .Bold("spaced", word: true)
            .Word("text.");
    }
}
```

## Combined Styles

Each run method accepts optional parameters for combining multiple styles in a single call:

```csharp demo-below
public class RichTextCombinedDemo : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
            | Text.Rich()
                .Run("Status: ")
                .Bold("Active", color: Colors.Green)
                .Run(" | ")
                .Run("Inactive", color: Colors.Gray, strikeThrough: true)
            | Text.Rich()
                .Run("This has a ")
                .Bold("highlighted", highlightColor: Colors.Yellow)
                .Run(" word.");
    }
}
```

## Links

Use `Link()` to add clickable text runs. Links are words by default (auto-spaced). Use `OnLinkClick` to handle clicks:

```csharp demo-below
public class RichTextLinkDemo : ViewBase
{
    public override object? Build()
    {
        return Text.Rich()
            .Run("Visit")
            .Link("Ivy Docs", "https://example.com")
            .Run("or")
            .Link("open in new tab", "https://example.com", linkTarget: LinkTarget.Blank)
            .OnLinkClick((Action<string>)(url => { /* handle navigation */ }));
    }
}
```

## Streaming

Use `UseStream` to dynamically append runs after the initial set. The `Runs` array provides the initial content; all streamed runs are appended after them on the frontend:

```csharp demo-tabs
public class RichTextStreamDemo : ViewBase
{
    public override object? Build()
    {
        var stream = Context.UseStream<TextRun>();

        return Layout.Vertical()
            | Text.Rich()
                .Run("Thinking")
                .UseStream(stream)
            | new Button("Add word", onClick: () =>
            {
                stream.Write(new TextRun("...") { Word = true });
            });
    }
}
```

## TextRun Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Content` | `string` | `""` | The text content of the run |
| `Bold` | `bool` | `false` | Renders the text in bold |
| `Italic` | `bool` | `false` | Renders the text in italic |
| `StrikeThrough` | `bool` | `false` | Renders the text with a strikethrough line |
| `Color` | `Colors?` | `null` | Sets the text color |
| `HighlightColor` | `Colors?` | `null` | Sets the background highlight color |
| `Word` | `bool` | `false` | Auto-prepends a space before the content (except the first run) |
| `Link` | `string?` | `null` | When set, renders the run as a clickable link |
| `LinkTarget` | `LinkTarget` | `Self` | Controls whether the link opens in the same tab or a new tab |

## RichTextBlock Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Runs` | `TextRun[]` | `[]` | The text runs to display |
| `Stream` | `IWriteStream<TextRun>?` | `null` | Stream for dynamically appending runs |
| `TextAlignment` | `TextAlignment?` | `null` | Text alignment (Left, Center, Right) |
| `NoWrap` | `bool` | `false` | Prevents text from wrapping |
| `Overflow` | `Overflow?` | `null` | Overflow behavior (Auto, Clip, Ellipsis) |

## Builder API

Entry point: `Text.Rich()` returns a `RichTextBuilder`.

### Run Methods

| Method | Description |
|---|---|
| `Run(content, ...)` | Add a text run with optional styling parameters |
| `Bold(content, ...)` | Add a bold run (shorthand for `Run` with `bold: true`) |
| `Italic(content, ...)` | Add an italic run |
| `StrikeThrough(content, ...)` | Add a strikethrough run |
| `Word(content, ...)` | Add a run with `Word=true` for auto-spacing |
| `Link(content, url, ...)` | Add a link run (word by default) |

### Block-Level Methods

| Method | Description |
|---|---|
| `NoWrap()` | Prevent text wrapping |
| `Overflow(overflow)` | Set overflow behavior |
| `Scale(scale)` / `Small()` / `Medium()` / `Large()` | Set text scale |
| `Align(alignment)` / `Left()` / `Center()` / `Right()` | Set text alignment |
| `UseStream(stream)` | Attach a stream for dynamic run appending |
| `OnLinkClick(handler)` | Set a callback for link clicks (accepts `Action<string>`, `Action<Event<...>>`, or `Func<Event<...>, ValueTask>`) |

## Related Components

- [TextBlock](./01_TextBlock.md) — for uniform text styling
- [Markdown](./14_Markdown.md) — for rendering markdown content
