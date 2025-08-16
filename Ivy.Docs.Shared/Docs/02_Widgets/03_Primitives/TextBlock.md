# TextBlock

The `TextBlock` widget displays text content with customizable styling. It's a fundamental building block for creating user interfaces with text, supporting various formatting options and layout properties.

This widget is rarely used directly. Instead, we use the helper class `Ivy.Views.Text` which provides a more user-friendly API for creating text elements.

## Basic Text Variants

The Text helper provides various methods for different text styles and purposes:

```csharp demo-below 
public class TextVariantsDemo : ViewBase
{   
    public override object? Build()
    {
        return Layout.Vertical()
            | Text.Literal("Literal text")
            | Text.H1("Heading 1")
            | Text.H2("Heading 2") 
            | Text.H3("Heading 3")
            | Text.H4("Heading 4")
            | Text.P("Paragraph text")
            | Text.Block("Block text")
            | Text.Blockquote("Blockquote text")
            | Text.InlineCode("InlineCode")
            | Text.Lead("Lead text for prominent display")
            | Text.Large("Large text")
            | Text.Small("Small text")
            | Text.Label("Label text")
            | Text.Strong("Strong/bold text")
            | Text.Muted("Muted text")
            | Text.Danger("Danger text")
            | Text.Warning("Warning text")
            | Text.Success("Success text");
    }
}
```

## Code and Markup Variants

For displaying code and markup content:

```csharp demo-tabs 
public class CodeVariantsDemo : ViewBase
{   
    public override object? Build()
    {
        return Layout.Vertical()
            | Text.Code("public class Example { }", Languages.Csharp)
            | Text.Json("{ \"name\": \"value\", \"number\": 42 }")
            | Text.Xml("<root><item>value</item></root>")
            | Text.Html("<div class='example'>HTML content</div>")
            | Text.Markdown("# Markdown\n**Bold** and *italic* text")
            | Text.Latex("\\frac{a}{b} = \\frac{c}{d}");
    }
}
```

## Text Modifiers

Text elements can be customized with various modifiers:

```csharp demo-tabs 
public class TextModifiersDemo : ViewBase
{   
    public override object? Build()
    {
        return Layout.Vertical()
            | Text.P("Normal paragraph text")
            | Text.P("Colored text").Color(Colors.Primary)
            | Text.P("Amber colored text").Color(Colors.Amber)
            | Text.P("Strikethrough text").StrikeThrough()
            | Text.P("No wrap text that should not wrap to multiple lines").NoWrap()
            | Text.P("Text with custom width").Width(Size.Units(200))
            | Text.P("Text with overflow clip").Overflow(Overflow.Clip).Width(Size.Units(100))
            | Text.P("Text with overflow ellipsis").Overflow(Overflow.Ellipsis).Width(Size.Units(100));
    }
}
```

## Practical Examples

### Article Layout

```csharp demo-tabs 
public class ArticleDemo : ViewBase
{   
    public override object? Build()
    {
        return Layout.Vertical()
            | Text.H1("Getting Started with Ivy")
            | Text.Lead("Ivy is a powerful framework for building interactive web applications with C#.")
            | Text.P("This guide will walk you through the basics of creating your first Ivy project. You'll learn about widgets, layouts, and how to structure your code effectively.")
            | Text.H2("Prerequisites")
            | Text.P("Before you begin, make sure you have:")
            | Text.Block("• .NET 8.0 SDK installed")
            | Text.Block("• A code editor (Visual Studio, VS Code, or Rider)")
            | Text.Block("• Basic knowledge of C#")
            | Text.H2("Installation")
            | Text.P("Install Ivy using the .NET CLI:")
            | Text.InlineCode("dotnet tool install -g Ivy.Console")
            | Text.P("Create a new project:")
            | Text.InlineCode("ivy init --namespace MyFirstProject");
    }
}
```

### Form Labels and Messages

```csharp demo-tabs
public class FormDemo : ViewBase
{   
    public override object? Build()
    {
        return Layout.Vertical()
            | Text.Label("Email Address")
            | Text.P("Enter your email address below")
            | Text.Small("We'll never share your email with anyone else.")
            | Layout.Horizontal()
                | Text.Success("✓ Email sent successfully!")
                | Text.Warning("⚠ Please check your spam folder")
                | Text.Danger("✗ Invalid email format");
    }
}
```

### Code Documentation

```csharp demo-tabs 
public class CodeDocDemo : ViewBase
{   
    public override object? Build()
    {
        return Layout.Vertical()
            | Text.H3("TextHelper Class")
            | Text.P("The TextHelper class provides convenient methods for creating text elements:")
            | Text.Code("""
                public static TextBuilder H1(string content)
                {
                    return new TextBuilder(content, TextVariant.H1);
                }
                """, Languages.Csharp)
            | Text.Blockquote("Note: All Text helper methods return a TextBuilder that supports method chaining for modifiers.")
            | Text.P("Common modifiers include:")
            | Text.Block("• Color() - Set text color")
            | Text.Block("• Width() - Set text width")
            | Text.Block("• StrikeThrough() - Add strikethrough styling")
            | Text.Block("• NoWrap() - Prevent text wrapping");
    }
}
```

### Status Messages

```csharp demo-tabs 
public class StatusDemo : ViewBase
{   
    public override object? Build()
    {
        return Layout.Vertical()
            | Text.H3("System Status")
            | Layout.Horizontal()
                | Text.Success("Database: Connected")
                | Text.Success("API: Online")
                | Text.Warning("Cache: Warming up...")
                | Text.Danger("Backup: Failed")
            | Text.Small("Last updated: 2 minutes ago")
            | Text.Muted("System monitoring is active");
    }
}
```

## TextBuilder Modifiers

The TextBuilder class provides several modifiers for customizing text appearance:

| Modifier | Description | Example |
|----------|-------------|---------|
| `Color()` | Set text color | `Text.P("Red text").Color(Colors.Destructive)` |
| `Width()` | Set text width | `Text.P("Fixed width").Width(Size.Units(200))` |
| `StrikeThrough()` | Add strikethrough | `Text.P("Crossed out").StrikeThrough()` |
| `NoWrap()` | Prevent wrapping | `Text.P("Single line").NoWrap()` |
| `Overflow()` | Handle overflow | `Text.P("Long text").Overflow(Overflow.Clip)` |

## Best Practices

- **Use semantic variants**: Choose the appropriate text variant for your content (H1-H4 for headings, P for paragraphs, etc.)
- **Consistent styling**: Use the same text variants throughout your project for consistency
- **Accessibility**: Use proper heading hierarchy (H1 → H2 → H3) for screen readers
- **Color usage**: Use color modifiers sparingly and ensure sufficient contrast
- **Responsive design**: Use width modifiers carefully to maintain responsive layouts

## Related Components

- **Markdown** - For rendering markdown content
- **Code** - For syntax-highlighted code blocks
- **Json** - For JSON data display
- **Html** - For HTML content rendering
