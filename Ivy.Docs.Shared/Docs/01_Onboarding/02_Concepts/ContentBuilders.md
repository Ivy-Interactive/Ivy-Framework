# Content Builders

<Ingress>
Flexible system for transforming and formatting content for display in Ivy.
</Ingress>

## Overview

Content Builders follow a middleware pattern, allowing for flexible and extensible content transformation. The system is built around the `IContentBuilder` interface, which defines two key methods:

- `CanHandle(object? content)`: Determines if the builder can process a specific type of content
- `Format(object? content)`: Transforms the content into its appropriate visual representation

## Default Content Builder

Ivy provides a comprehensive `DefaultContentBuilder` that handles a wide variety of content types:

- **Basic Types**:
  - Strings → Text blocks
  - Numbers (int, long, double, decimal) → Formatted text blocks
  - Booleans → Check icons (green checkmark for true, none for false)
  - Dates (DateTime, DateTimeOffset, DateOnly) → Formatted date strings

- **Complex Types**:
  - Exceptions → Error views
  - Tasks → Task views
  - Observables → Observable views
  - JSON nodes → JSON views
  - XML objects → XML views
  - Collections → Tables
  - Icons → Icon views
  - Widgets and Views → Direct rendering

## Custom Content Builders

You can create custom content builders by implementing the `IContentBuilder` interface. This allows you to:

1. Define specific formatting rules for your data types
2. Create specialized visualizations for your domain objects
3. Extend the default formatting behavior

Example of creating a custom content builder:

```csharp
public class CustomContentBuilder : IContentBuilder
{
    public bool CanHandle(object? content)
    {
        return content is YourCustomType;
    }

    public object? Format(object? content)
    {
        // Transform your custom type into a visual representation
        return new YourCustomView(content);
    }
}
```

## Using Content Builders

Content Builders are typically used through the Ivy server configuration:

```csharp
var server = new Server()
    .UseContentBuilder(new CustomContentBuilder());
```

You can also chain multiple content builders using the middleware pattern:

```csharp
var builder = new ContentBuilder()
    .Use(new CustomBuilder1())
    .Use(new CustomBuilder2())
    .Use(new DefaultContentBuilder());
```

## Builder Factory

Ivy provides a `BuilderFactory` that offers convenient methods for creating common types of builders:

- `Default()`: Creates a default builder that passes through values
- `Text()`: Creates a text builder for string content
- `Number()`: Creates a number builder for numeric content
- `Link()`: Creates a link builder for URL content
- `CopyToClipboard()`: Creates a builder that adds copy-to-clipboard functionality

## Best Practices

1. **Specificity**: Create builders that handle specific types of content rather than trying to handle everything in one builder.

2. **Fallback**: Always provide a fallback to the `DefaultContentBuilder` for unhandled content types.

3. **Performance**: Implement `CanHandle` efficiently to quickly determine if a builder can process the content.

4. **Consistency**: Maintain consistent formatting across similar types of content.

## Examples

Here's a simple example showing how different types of content are automatically formatted:

```csharp demo-tabs 
public class ContentBuilderDemo : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical(
            null,                    // Empty view
            "Hello World",          // Text block
            123_456.78,            // Formatted number
            false,                  // Boolean icon
            true,                   // Boolean icon
            DateTime.Now,           // Formatted date
            new int[] { 1,2,3,4 }, // Table
            new List<string> { "a", "b", "c" } // Table
        );
    }
}
```

Each item in this layout will be automatically formatted by the appropriate content builder, resulting in a clean and consistent visual representation.
