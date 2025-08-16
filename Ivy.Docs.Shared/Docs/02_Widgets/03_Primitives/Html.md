# Html

<Ingress>
Render raw HTML content directly in your Ivy application for external content integration, formatted text, and custom markup control.
</Ingress>

The `Html` widget allows you to render raw HTML content in your Ivy app. This is useful when you need to include content from external sources, display formatted text, or when you want direct control over the markup.

## Basic Usage

```csharp demo-tabs 
public class BasicHtmlView : ViewBase
{
    public override object? Build()
    {
        var simpleHtml = "<p>Hello, <strong>World</strong>!</p>";
        
        return new Html(simpleHtml);
    }
}
```

**Rendered Result:**
> Hello, **World**!

## Content Examples

### Text Formatting

```csharp demo-tabs 
public class TextFormattingView : ViewBase
{
    public override object? Build()
    {
        var formattedText = 
            """
            <h1>Main Heading</h1>
            <h2>Subheading</h2>
            <p>This paragraph contains <strong>bold text</strong>, <em>italic text</em>, 
               and <b>bold using b tag</b>, plus <i>italic using i tag</i>.</p>
            <p>You can also use <span style="color: blue;">colored text</span> 
               and <span style="text-decoration: underline;">underlined text</span>.</p>
            """;
        
        return new Html(formattedText);
    }
}
```

**Rendered Result:**

> # Main Heading
>
> ## Subheading
>
> This paragraph contains **bold text**, *italic text*, and **bold using b tag**, plus *italic using i tag*.
>
> You can also use <span style="color: blue;">colored text</span> and <span style="text-decoration: underline;">underlined text</span>.

### Lists and Structure

```csharp demo-tabs 
public class ListsView : ViewBase
{
    public override object? Build()
    {
        var listsHtml = 
            """
            <h3>Unordered List</h3>
            <ul>
                <li>First item</li>
                <li>Second item with <strong>bold text</strong></li>
                <li>Third item with <em>italic text</em></li>
            </ul>
            
            <h3>Ordered List</h3>
            <ol>
                <li>Step one</li>
                <li>Step two</li>
                <li>Step three</li>
            </ol>
            
            <h3>Nested Lists</h3>
            <ul>
                <li>Parent item
                    <ul>
                        <li>Child item 1</li>
                        <li>Child item 2</li>
                    </ul>
                </li>
                <li>Another parent item</li>
            </ul>
            """;
        
        return new Html(listsHtml);
    }
}
```

**Rendered Result:**

> ### Unordered List
>
> - First item
> - Second item with **bold text**
> - Third item with *italic text*
>
> ### Ordered List
>
> 1. Step one
> 2. Step two
> 3. Step three
>
> ### Nested Lists
>
> - Parent item
>   - Child item 1
>   - Child item 2
> - Another parent item

### Links and Navigation

```csharp demo-tabs 
public class LinksView : ViewBase
{
    public override object? Build()
    {
        var linksHtml = 
            """
            <p>Visit <a href="https://github.com/Ivy-Interactive/Ivy-Framework">Ivy Framework</a> on GitHub.</p>
            <p>You can also link to <a href="#section1">internal sections</a> or 
               <a href="mailto:example@example.com">email addresses</a>.</p>
            """;
        
        return new Html(linksHtml);
    }
}
```

**Rendered Result:**
> Visit [Ivy Framework](https://github.com/Ivy-Interactive/Ivy-Framework) on GitHub.
>
> You can also link to internal sections or email addresses.

### Tables

```csharp demo-tabs 
public class TablesView : ViewBase
{
    public override object? Build()
    {
        var tableHtml = 
            """
            <table border="1" style="width: 100%; border-collapse: collapse; margin: 10px 0;">
                <tr style="background-color: #f0f0f0;">
                    <th style="padding: 12px; text-align: left; border: 1px solid #ddd;">Feature</th>
                    <th style="padding: 12px; text-align: left; border: 1px solid #ddd;">Status</th>
                    <th style="padding: 12px; text-align: left; border: 1px solid #ddd;">Description</th>
                </tr>
                <tr>
                    <td style="padding: 8px; border: 1px solid #ddd;">HTML Rendering</td>
                    <td style="padding: 8px; border: 1px solid #ddd; color: green;">✓ Supported</td>
                    <td style="padding: 8px; border: 1px solid #ddd;">Safe HTML rendering with sanitization</td>
                </tr>
                <tr>
                    <td style="padding: 8px; border: 1px solid #ddd;">Custom Styling</td>
                    <td style="padding: 8px; border: 1px solid #ddd; color: green;">✓ Supported</td>
                    <td style="padding: 8px; border: 1px solid #ddd;">Inline styles are preserved</td>
                </tr>
                <tr>
                    <td style="padding: 8px; border: 1px solid #ddd;">JavaScript</td>
                    <td style="padding: 8px; border: 1px solid #ddd; color: red;">✗ Blocked</td>
                    <td style="padding: 8px; border: 1px solid #ddd;">Scripts are removed for security</td>
                </tr>
            </table>
            """;
        
        return new Html(tableHtml);
    }
}
```

**Rendered Result:**
>
> | Feature | Status | Description |
> |---------|--------|-------------|
> | HTML Rendering | ✓ Supported | Safe HTML rendering with sanitization |
> | Custom Styling | ✓ Supported | Inline styles are preserved |
> | JavaScript | ✗ Blocked | Scripts are removed for security |

## Complex Layout Example

```csharp demo-tabs 
public class ComplexLayoutView : ViewBase
{
    public override object? Build()
    {
        var complexHtml = 
            """
            <div style="background-color: var(--muted); padding: 20px; border-radius: 8px; border-left: 4px solid var(--primary);">
                <h2 style="color: var(--primary); margin-top: 0;">Product Features</h2>
                <p style="font-size: 16px; color: var(--muted-foreground);">Discover what makes our product special:</p>
                
                <div style="display: flex; gap: 20px; margin: 20px 0;">
                    <div style="flex: 1; background: var(--background); padding: 15px; border-radius: 5px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
                        <h3 style="color: var(--primary); margin-top: 0;">Performance</h3>
                        <p>Lightning-fast rendering with optimized algorithms.</p>
                    </div>
                    <div style="flex: 1; background: var(--background); padding: 15px; border-radius: 5px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
                        <h3 style="color: var(--destructive); margin-top: 0;">Security</h3>
                        <p>Built-in HTML sanitization protects against XSS attacks.</p>
                    </div>
                </div>
                
                <div style="background: var(--accent); padding: 15px; border-radius: 5px; margin-top: 20px;">
                    <h4 style="margin-top: 0;">Quick Stats</h4>
                    <ul style="margin: 0;">
                        <li><strong>Rendering Speed:</strong> <span style="color: var(--primary);">99.9% faster</span></li>
                        <li><strong>Memory Usage:</strong> <span style="color: var(--accent-foreground);">50% less</span></li>
                        <li><strong>Security Score:</strong> <span style="color: var(--primary);">A+</span></li>
                    </ul>
                </div>
            </div>
            """;
        
        return Layout.Vertical().Gap(4)
            | Text.H1("HTML Widget Demo")
            | new Html(complexHtml);
    }
}
```

**Rendered Result:**

> # HTML Widget Demo
>
> <div style="background-color: var(--muted); padding: 20px; border-radius: 8px; border-left: 4px solid var(--primary);">
>   <h2 style="color: var(--primary); margin-top: 0;">Product Features</h2>
>   <p style="font-size: 16px; color: var(--muted-foreground);">Discover what makes our product special:</p>
>
> **Performance**: Lightning-fast rendering with optimized algorithms.
> **Security**: Built-in HTML sanitization protects against XSS attacks.
>
> **Quick Stats**
>
> - **Rendering Speed:** 99.9% faster
> - **Memory Usage:** 50% less
> - **Security Score:** A+
>
> </div>

```

## Security Features

The Html widget includes robust security measures to protect against malicious content:

### Allowed HTML Tags
Only these HTML tags are permitted:
- **Text formatting:** `p`, `div`, `span`, `strong`, `em`, `b`, `i`, `br`
- **Headings:** `h1`, `h2`, `h3`, `h4`, `h5`, `h6`
- **Lists:** `ul`, `ol`, `li`
- **Links:** `a`

### Security Measures
- **Script removal:** All `<script>` tags are completely removed
- **Event handler blocking:** All `on*` event handlers (onclick, onload, etc.) are stripped
- **JavaScript URL blocking:** `javascript:` URLs in href attributes are removed
- **Tag whitelisting:** Only approved HTML tags are allowed

### Example of Security in Action

```csharp demo-tabs
public class SecurityDemoView : ViewBase
{
    public override object? Build()
    {
        // This potentially dangerous HTML...
        var unsafeHtml = 
            """
            <p>Safe content</p>
            <script>alert('This will be removed!');</script>
            <div onclick="alert('This will be removed!')">Click me</div>
            <a href="javascript:alert('Blocked!')">Blocked link</a>
            <iframe src="https://evil.com">This tag will be removed</iframe>
            """;
        
        // ...becomes safe when rendered
        return new Html(unsafeHtml);
    }
}
```

**Rendered Result (after sanitization):**
> Safe content
>
> Click me
>
> Blocked link

## Best Practices & Use Cases

### Best Practices

#### Keep It Simple

```csharp
// Good: Simple, clear HTML
var goodHtml = "<p>Welcome to our <strong>project</strong>!</p>";

// Avoid: Overly complex nested structures
var complexHtml = "<div><div><div><p>Deep nesting</p></div></div></div>";
```

#### Validate External Content

```csharp
public class ExternalContentView : ViewBase
{
    public override object? Build()
    {
        var externalHtml = GetContentFromExternalSource();
        
        // Always validate external content
        if (string.IsNullOrEmpty(externalHtml))
        {
            return new Html("<p>No content available</p>");
        }
        
        return new Html(externalHtml);
    }
    
    private string GetContentFromExternalSource()
    {
        // Your external content logic here
        return "<p>External content</p>";
    }
}
```

#### Handle Long Content

```csharp
public class LongContentView : ViewBase
{
    public override object? Build()
    {
        var longContent = GetLongHtmlContent();
        
        return Layout.Vertical().Gap(4)
            | Text.H2("Article")
            | new Html($"<div style='max-height: 400px; overflow-y: auto;'>{longContent}</div>");
    }
}
```

### Common Use Cases

#### Rich Text from CMS

```csharp
public class CMSContentView : ViewBase
{
    public override object? Build()
    {
        var cmsContent = 
            """
            <h2>Latest News</h2>
            <p><em>Published: January 15, 2024</em></p>
            <p>We're excited to announce new features in our latest release...</p>
            """;
        
        return new Html(cmsContent);
    }
}
```

#### Documentation and Help Text

```csharp
public class HelpContentView : ViewBase
{
    public override object? Build()
    {
        var helpHtml = 
            """
            <h3>How to Use This Feature</h3>
            <ol>
                <li>Click the <strong>Start</strong> button</li>
                <li>Select your preferences</li>
                <li>Review the results</li>
            </ol>
            <p><em>Need more help? <a href='mailto:support@example.com'>Contact support</a></em></p>
            """;
        
        return new Html(helpHtml);
    }
}
```

#### Formatted User Content

```csharp
public class UserContentView : ViewBase
{
    public override object? Build()
    {
        var userComment = 
            """
            <div style='background: #f9f9f9; padding: 15px; border-radius: 5px;'>
                <p><strong>User123:</strong> This is a great feature! I especially like the <em>ease of use</em>.</p>
                <p><small>Posted 2 hours ago</small></p>
            </div>
            """;
        
        return new Html(userComment);
    }
}
```

## Limitations

- **No JavaScript:** All JavaScript code is removed for security
- **Limited HTML tags:** Only a subset of HTML tags are supported
- **No form elements:** Input fields, buttons, and forms are not supported
- **No embedded content:** iframes, objects, and embeds are blocked
- **No CSS imports:** External stylesheets cannot be imported

## When to Use Html Widget

**Use Html widget when:**

- Displaying content from external sources (CMS, APIs)
- Showing rich text with formatting
- Rendering documentation or help content
- Displaying user-generated content (with proper sanitization)
- Creating complex layouts with custom styling

**Don't use Html widget when:**

- You need interactive elements (use Button, Input, etc.)
- You want to embed external content (use Iframe widget)
- You need JavaScript functionality
- Simple text formatting would suffice (use Text widget)

<WidgetDocs Type="Ivy.Html" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Primitives/Html.cs"/>
