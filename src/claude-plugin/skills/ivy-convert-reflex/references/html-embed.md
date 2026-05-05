# HTML Embed

Renders raw HTML content directly within the application. Useful for displaying formatted markup, external content, or custom HTML that doesn't have a dedicated component equivalent.

## Reflex

```python
rx.vstack(
    rx.html("<h1>Hello World</h1>"),
    rx.html("<h2>Hello World</h2>"),
    rx.html("<img src='https://reflex.dev/reflex_banner.png' />"),
)
```

## Ivy

```csharp
public class BasicHtmlView : ViewBase
{
    public override object? Build()
    {
        var simpleHtml = "<p>Hello, <strong>World</strong>!</p>";
        return new Html(simpleHtml);
    }
}
```

## Parameters

| Parameter                  | Documentation                                                                 | Ivy                                                                                      |
|----------------------------|-------------------------------------------------------------------------------|------------------------------------------------------------------------------------------|
| `dangerouslySetInnerHTML`  | `Dict[str, str]` — sets raw HTML content (React-style)                        | Replaced by constructor parameter `string content` passed to `new Html(content)`         |
| `style`                    | Standard Reflex style props for the wrapping element                          | Not directly supported; limited inline styles via HTML tags only                         |
| Event triggers             | Full set of default event triggers (on_click, on_mouse_over, etc.)            | Not supported                                                                            |
| Width / Height             | Controlled via style props                                                    | `Width` and `Height` properties (`Size` type)                                            |
| Visibility                 | Controlled via `display` style or conditional rendering                       | `Visible` property (`bool`)                                                              |
| Scale                      | Controlled via style props                                                    | `Scale` property (`Scale?`)                                                              |
| Security / sanitization    | No built-in sanitization — renders any HTML via `dangerouslySetInnerHTML`     | Auto-sanitizes: strips `<script>`, blocks `on*` handlers, whitelists safe tags only      |
