# Markdown

<Ingress>
Render rich Markdown content with syntax highlighting, math support, tables, and interactive features in your Ivy applications.
</Ingress>

The `Markdown` widget renders Markdown content as formatted HTML with syntax highlighting, math support, tables, images, and interactive link handling.

## Basic Usage

The Markdown widget supports standard markdown syntax including text formatting, lists, links, and blockquotes. This example demonstrates the most commonly used features for basic content creation.

```csharp demo-tabs 
public class BasicMarkdownView : ViewBase
{
    public override object? Build()
    {
        var markdownContent = 
            """
            Hello World
            
            This is **bold** and *italic* text with `inline code`.
            
            - Unordered list item
            - [x] Task list item
            
            [Link to Google](https://www.google.com)
            
            > This is a blockquote with **bold** text.
            """;
            
        return new Markdown(markdownContent);
    }
}
```

### Tables

Tables in Markdown provide a structured way to display data in rows and columns. They support alignment, headers, and can be easily formatted for better readability.

```csharp demo-tabs 
public class TablesMarkdownView : ViewBase
{
    public override object? Build()
    {
        var markdownContent = 
            """
            | Feature        | Basic | Premium | Enterprise |
            |----------------|-------|---------|------------|
            | Users          | 1     | 10      | Unlimited  |
            | Storage        | 1GB   | 100GB   | 1TB        |
            | Support        | Email | Phone   | 24/7       |
            """;
            
        return new Markdown(markdownContent);
    }
}
```

### Code Blocks

Code blocks support syntax highlighting for various programming languages and can be used for displaying code examples, configuration files, or any formatted text. The language is automatically detected based on the code fence specification.

```csharp demo-tabs 
public class CodeBlocksMarkdownView : ViewBase
{
    public override object? Build()
    {
        var markdownContent = 
            """
            ```csharp
            public class Example
            {
                public void Demo() => Console.WriteLine("Hello, World!");
            }
            ```
            """;
            
        return new Markdown(markdownContent);
    }
}
```

### Math

Mathematical expressions can be rendered using KaTeX, supporting both inline math with single dollar signs and block math with double dollar signs. This feature is perfect for technical documentation and educational content.

```csharp demo-tabs 
public class MathMarkdownView : ViewBase
{
    public override object? Build()
    {
        var markdownContent = 
            """
            Inline: $E = mc^2$
            
            $$
            \int_a^b f(x) dx = F(b) - F(a)
            $$
            """;
            
        return new Markdown(markdownContent);
    }
}
```

### Mermaid Diagrams

Mermaid diagrams allow you to create various types of visual diagrams directly in markdown content. Supported diagram types include flowcharts, sequence diagrams, class diagrams, and more for visualizing complex processes and relationships.

For more information on how to use Mermaid, [follow this link](https://mermaid.js.org/):

```csharp demo-tabs 
public class MermaidView : ViewBase
{
    public override object? Build()
    {
        var markdownContent = 
            """
            ```mermaid
            sequenceDiagram
                participant U as User
                participant F as Frontend
                participant B as Backend
                
                U->>F: Navigate to page
                F->>B: GET /api/data
                B-->>F: JSON response
                F-->>U: Render UI
            ```
            """;
            
        return new Markdown(markdownContent);
    }
}
```

### Emojis

Emoji support enhances content with visual elements and expressions. You can use standard emoji shortcodes to add personality and visual appeal to your markdown content.

```csharp demo-tabs 
public class EmojiView : ViewBase
{
    public override object? Build()
    {
        var markdownContent = 
            """
            Express yourself! :smile: :heart: :star: :rocket:
            
            **People:** :smile: :wink: :heart_eyes: :thumbsup:
            **Nature:** :sunny: :cloud: :zap: :snowflake:
            **Objects:** :computer: :phone: :bulb: :gear:
            """;
            
        return new Markdown(markdownContent);
    }
}
```

### HTML Support and Link Handling

The Markdown widget supports HTML tags for advanced formatting and provides interactive link handling through the OnLinkClick event. This allows for custom navigation logic and enhanced user interactions.

```csharp demo-tabs 
public class HtmlAndLinksView : ViewBase
{
    public override object? Build()
    {
        var markdownContent = 
            """
            <details>
                <summary>Click to expand</summary>
                Hidden content with **markdown** support.
            </details>
            
            - [Navigate to Home](/home)
            - [External Link](https://example.com)
            """;
            
        Action<string> handleLink = url =>
        {
            if (url.StartsWith("/"))
            {
                Console.WriteLine($"Navigating to: {url}");
            }
            else
            {
                Console.WriteLine($"Opening external link: {url}");
            }
        };
            
        return new Markdown(markdownContent)
            .HandleLinkClick(handleLink);
    }
}
```

### Complete Example

This comprehensive example showcases multiple Markdown features working together in a single widget. It demonstrates how different elements can be combined to create rich, interactive content with proper link handling.

```csharp demo-tabs 
public class ComprehensiveMarkdownView : ViewBase
{
    public override object? Build()
    {
        var markdownContent = 
            """
            **Bold text** and *italic text* with `inline code`
            
            - Item 1
            - [x] Completed task
            - [ ] Pending task
            
            ```csharp
            public class Example
            {
                public void ShowFeatures() => Console.WriteLine("Markdown!");
            }
            ```
            
            Inline equation: $f(x) = x^2 + 2x + 1$
            
            | Language | Type       | Performance |
            |----------|------------|-------------|
            | C#       | Compiled   | High        |
            | Python   | Interpreted| Moderate    |
            
            ```mermaid
            graph LR
                A[Start] --> B[Process] --> C[End]
            ```
            
            Made with :heart: using Ivy Framework :rocket:
            """;
            
        Action<string> handleLink = url => Console.WriteLine($"Navigate to: {url}");
            
        return new Markdown(markdownContent)
            .HandleLinkClick(handleLink);
    }
}
```

<WidgetDocs Type="Ivy.Markdown" ExtensionTypes="Ivy.MarkdownExtensions"  SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Primitives/Markdown.cs"/>
