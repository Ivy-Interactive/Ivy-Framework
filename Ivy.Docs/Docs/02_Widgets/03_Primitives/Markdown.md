# Markdown

The Markdown widget renders Markdown content as formatted HTML with rich features including syntax highlighting, math support, tables, images, and interactive link handling. It supports standard Markdown, GitHub Flavored Markdown (GFM), and several advanced features.

## Basic Usage

```csharp demo-tabs
public class BasicMarkdownView : ViewBase
{
    public override object? Build()
    {
        var markdownContent = 
            """
            # Hello World
            
            This is **bold** and *italic* text with `inline code`.
            
            ## Lists
            
            - Unordered list item 1
            - Unordered list item 2
              - Nested item
            
            1. Ordered list item 1
            2. Ordered list item 2
            
            ## Links and Images
            
            [Link to Google](https://www.google.com)
            
            ![Cat Image](https://placecats.com/300/200)
            
            ## Blockquotes
            
            > This is a blockquote with some **bold** text.
            > It can span multiple lines.
            """;
            
        return new Markdown(markdownContent);
    }
}
```

## Advanced Features

### Tables

```csharp demo-tabs
public class TableMarkdownView : ViewBase
{
    public override object? Build()
    {
        var markdownContent = 
            """
            ## Feature Comparison
            
            | Feature        | Basic | Premium | Enterprise |
            |----------------|-------|---------|------------|
            | Users          | 1     | 10      | Unlimited  |
            | Storage        | 1GB   | 100GB   | 1TB        |
            | Support        | Email | Phone   | 24/7       |
            | Custom Themes  | ❌    | ✅      | ✅         |
            | API Access    | ❌    | ✅      | ✅         |
            | Analytics      | ❌    | ❌      | ✅         |
            """;
            
        return new Markdown(markdownContent);
    }
}
```

### Task Lists

```csharp demo-tabs
public class TaskListView : ViewBase
{
    public override object? Build()
    {
        var markdownContent = 
            """
            ## Project Tasks
            
            - [x] Set up project structure
            - [x] Create basic components
            - [ ] Add authentication
            - [ ] Implement user dashboard
            - [ ] Write tests
            - [ ] Deploy to production
            
            ## Shopping List
            
            - [x] Milk
            - [x] Bread
            - [ ] Eggs
            - [ ] Cheese
            """;
            
        return new Markdown(markdownContent);
    }
}
```

### Code Blocks with Syntax Highlighting

```csharp demo-tabs
public class CodeMarkdownView : ViewBase
{
    public override object? Build()
    {
        var markdownContent = 
            """
            ## Code Examples
            
            ### C# Example
            
            ```csharp
            public class Example
            {
                public void Demo()
                {
                    Console.WriteLine("Hello, World!");
                    var numbers = new[] { 1, 2, 3, 4, 5 };
                    var sum = numbers.Sum();
                }
            }
            ```
            
            ### JavaScript Example
            
            ```javascript
            const greeting = 'Hello, World!';
            console.log(greeting);
            
            const numbers = [1, 2, 3, 4, 5];
            const sum = numbers.reduce((a, b) => a + b, 0);
            ```
            
            ### Diff Example
            
            ```diff
            - const oldValue = 'old';
            + const newValue = 'new';
            
            - function oldFunction() {
            -     return 'old';
            - }
            + function newFunction() {
            +     return 'new';
            + }
            ```
            
            Inline code: `const x = 10;`
            """;
            
        return new Markdown(markdownContent);
    }
}
```

### Math Support

The Markdown widget supports KaTeX for rendering mathematical expressions both inline and as blocks.

```csharp demo-tabs
public class MathMarkdownView : ViewBase
{
    public override object? Build()
    {
        var markdownContent = 
            """
            ## Mathematical Expressions
            
            ### Inline Math
            
            The famous equation is $E = mc^2$, and the quadratic formula is $x = \frac{-b \pm \sqrt{b^2 - 4ac}}{2a}$.
            
            ### Block Math
            
            The fundamental theorem of calculus:
            
            $$
            \int_a^b f(x) dx = F(b) - F(a)
            $$
            
            Matrix representation:
            
            $$
            \begin{pmatrix}
            a & b \\
            c & d
            \end{pmatrix}
            \begin{pmatrix}
            x \\
            y
            \end{pmatrix}
            =
            \begin{pmatrix}
            ax + by \\
            cx + dy
            \end{pmatrix}
            $$
            
            ### Code Block Math
            
            ```math
            \left( \sum_{k=1}^n a_k b_k \right)^2 \leq \left( \sum_{k=1}^n a_k^2 \right) \left( \sum_{k=1}^n b_k^2 \right)
            ```
            """;
            
        return new Markdown(markdownContent);
    }
}
```

### Emojis

```csharp demo-tabs
public class EmojiMarkdownView : ViewBase
{
    public override object? Build()
    {
        var markdownContent = 
            """
            ## Emoji Support
            
            Express yourself with emojis! :smile: :heart: :star: :+1: :rocket:
            
            ### Categories
            
            **People:** :smile: :wink: :heart_eyes: :thumbsup: :clap: :wave:
            
            **Nature:** :sunny: :cloud: :zap: :snowflake: :fire: :ocean:
            
            **Objects:** :computer: :phone: :bulb: :gear: :key: :lock:
            
            **Symbols:** :white_check_mark: :x: :warning: :information_source: :question:
            
            **Fun:** :pizza: :coffee: :beer: :cake: :gift: :tada:
            """;
            
        return new Markdown(markdownContent);
    }
}
```

### HTML Support

```csharp demo-tabs
public class HtmlMarkdownView : ViewBase
{
    public override object? Build()
    {
        var markdownContent = 
            """
            ## HTML Integration
            
            Markdown supports HTML tags for advanced formatting:
            
            This is <sub>subscript</sub> and <sup>superscript</sup> text.
            
            <details>
                <summary>Click to expand</summary>
                Hidden content that can be revealed by clicking the summary.
                
                - You can include **markdown** inside HTML tags
                - Lists work too
                - And `code` formatting
            </details>
            
            <details>
                <summary>Another collapsible section</summary>
                
                ```csharp
                public class HiddenCode
                {
                    public void Example() => Console.WriteLine("Hidden!");
                }
                ```
            </details>
            
            You can also use HTML for <mark>highlighting</mark> and <del>strikethrough</del> text.
            """;
            
        return new Markdown(markdownContent);
    }
}
```

## Link Handling

The Markdown widget provides interactive link handling through the `OnLinkClick` event.

```csharp demo-tabs
public class LinkHandlingView : ViewBase
{
    public override object? Build()
    {
        var markdownContent = 
            """
            ## Interactive Links
            
            Click these links to see custom handling:
            
            - [Navigate to Home](/home)
            - [Open Settings](/settings)
            - [View Profile](/profile)
            - [External Link](https://example.com)
            
            Internal links are handled by the OnLinkClick event, while external links open in new tabs.
            """;
            
        return new Markdown(markdownContent)
            .HandleLinkClick(url =>
            {
                // Handle internal navigation
                if (url.StartsWith("/"))
                {
                    // Navigate to internal route
                    Console.WriteLine($"Navigating to: {url}");
                }
                else
                {
                    // Handle external links
                    Console.WriteLine($"Opening external link: {url}");
                }
            });
    }
}
```

## Comprehensive Example

```csharp demo-tabs
public class ComprehensiveMarkdownView : ViewBase
{
    public override object? Build()
    {
        var markdownContent = 
            """
            # The Complete Markdown Guide
            
            This document showcases all the features available in the Ivy Markdown widget.
            
            ## Text Formatting
            
            **Bold text** and *italic text* and ***bold italic text***
            
            ~~Strikethrough text~~ and `inline code`
            
            ## Lists and Tasks
            
            ### Unordered Lists
            - Item 1
            - Item 2
              - Nested item
              - Another nested item
            
            ### Ordered Lists
            1. First item
            2. Second item
            3. Third item
            
            ### Task Lists
            - [x] Completed task
            - [ ] Pending task
            - [ ] Another pending task
            
            ## Links and Images
            
            [Internal link](/dashboard) | [External link](https://github.com)
            
            ![Sample Image](https://placecats.com/400/300)
            
            ## Code and Math
            
            Inline equation: $f(x) = x^2 + 2x + 1$
            
            Block equation:
            $$
            \sum_{i=1}^n i = \frac{n(n+1)}{2}
            $$
            
            ```csharp
            public class MarkdownExample
            {
                public void ShowFeatures()
                {
                    var markdown = new Markdown("# Hello World");
                    Console.WriteLine("Markdown rendered!");
                }
            }
            ```
            
            ## Tables
            
            | Language | Type       | Performance | Learning Curve |
            |----------|------------|-------------|----------------|
            | C#       | Compiled   | High        | Moderate       |
            | Python   | Interpreted| Moderate    | Easy           |
            | Rust     | Compiled   | Very High   | Steep          |
            | Go       | Compiled   | High        | Easy           |
            
            ## Blockquotes
            
            > "The best way to predict the future is to create it."
            > 
            > — Peter Drucker
            
            ## Emojis and Fun
            
            Made with :heart: using Ivy Framework :rocket:
            
            Status: :white_check_mark: Complete | :warning: In Progress | :x: Failed
            
            ## HTML Elements
            
            <details>
                <summary>Advanced Configuration</summary>
                
                ```json
                {
                    "theme": "dark",
                    "syntax": "csharp",
                    "math": true,
                    "emoji": true
                }
                ```
            </details>
            
            ---
            
            *This documentation was generated using the Ivy Markdown widget.*
            """;
            
        return new Markdown(markdownContent)
            .HandleLinkClick(url =>
            {
                if (url.StartsWith("/"))
                {
                    // Handle internal navigation
                    Console.WriteLine($"Navigate to: {url}");
                }
            });
    }
}
```

<WidgetDocs Type="Ivy.Markdown" ExtensionTypes="Ivy.MarkdownExtensions"  SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Primitives/Markdown.cs"/> 