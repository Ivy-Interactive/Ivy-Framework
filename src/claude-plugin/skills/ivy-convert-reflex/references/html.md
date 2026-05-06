# Html

Renders raw HTML content. In Reflex, `rx.el.html` represents the root `<html>` element of a document. In Ivy, `Html` renders a sanitized HTML string directly into the page.

## Reflex

```python
import reflex as rx

def html_example():
    return rx.el.html(
        rx.el.head(
            rx.el.title("My Page Title"),
            rx.el.meta(name="viewport", content="width=device-width"),
        ),
        rx.el.body(
            rx.el.h1("Hello World"),
            rx.el.p("This is a paragraph"),
        ),
        manifest="/manifest.json",
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

| Parameter  | Documentation                                          | Ivy           |
|------------|--------------------------------------------------------|---------------|
| `manifest` | URL of the web app manifest for PWA configuration      | Not supported |
| `content`  | Child elements (Reflex) / HTML string (Ivy)            | `Content`     |
| `height`   | Not a direct prop on `rx.el.html`                      | `Height`      |
| `width`    | Not a direct prop on `rx.el.html`                      | `Width`       |
| `scale`    | Not a direct prop on `rx.el.html`                      | `Scale`       |
| `visible`  | Not a direct prop on `rx.el.html`                      | `Visible`     |
