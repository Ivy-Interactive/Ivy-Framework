# Ivy Framework Weekly Notes - Week of 2026-02-06

## New Widgets

### Terminal Widget

The `Terminal` widget has been promoted from internal API to a public primitive widget. You can now use it to display terminal-like output in your applications.

The widget allows you to display terminal lines with optional command prompts:

```csharp
new Terminal()
    .Lines(new[]
    {
        new TerminalLine("$ npm install", IsCommand: true),
        new TerminalLine("Installing packages..."),
        new TerminalLine("Done!")
    })
```

Each `TerminalLine` can be customized with:
- `Content`: The text to display
- `IsCommand`: Whether this line represents a command (affects styling)
- `Prompt`: Custom prompt string (defaults to ">")

## Widget Enhancements

### Badge Widget Scale API

The `Badge` widget now fully supports the Scale API, allowing you to control the size of badges to match your UI design. This enhancement fixes previous issues where the Scale API wasn't working correctly with badges.

Available scales:
- **Small**: Compact badges with smaller text and padding
- **Medium**: Default size (default)
- **Large**: Larger, more prominent badges

Example:

```csharp
new Badge("New Feature")
    .Variant(BadgeVariant.Success)
    .Small()

new Badge("Important")
    .Variant(BadgeVariant.Destructive)
    .Large()
```

The scale automatically adjusts padding, text size, and icon spacing for a cohesive appearance. When using icons with badges, the padding is intelligently adjusted based on the icon position.

### Code Widget Scale API

The `Code` widget now supports a `.Scale()` method to control the size of code blocks. This allows you to adjust the font size, padding, and line height to better fit your UI design.

Available scales:
- **Small**: Compact code blocks with smaller text (0.75rem)
- **Medium**: Default size (0.875rem)
- **Large**: Larger, more readable code blocks (1rem)

Example:

```csharp
new Code()
    .Content("console.log('Hello, World!');")
    .Language(CodeLanguage.JavaScript)
    .Scale(Scale.Small)
```

The scale affects the entire code block presentation, including the copy button positioning for a cohesive appearance.

## Documentation Improvements

### Layout Documentation Refinement

Clarified the documentation for layout helpers in Ivy:

- **Stack Layouts**: Use `Layout.Vertical()` or `Layout.Horizontal()` to create stack-based layouts
- **Grid Layouts**: Use `Layout.Grid()` for grid-based layouts
- **Wrap Layouts**: Use `Layout.Wrap()` for wrapping layouts

All layouts support:
- **Children**: Add child elements using the `|` operator to arrange them top-to-bottom (vertical) or left-to-right (horizontal)
- **Spacing**: Use `.Gap(int number)` to set spacing between children (follows Tailwind CSS spacing scale: 1 = 0.25rem, 2 = 0.5rem, etc.)
- **Alignment**: Use `.Left()`, `.Center()`, or `.Right()` methods to control alignment

Example:

```csharp
Layout.Vertical()
    .Gap(4)
    .Center()
    | new Text("Hello")
    | new Text("World")
```
