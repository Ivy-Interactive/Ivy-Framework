# HTML

Renders raw HTML and CSS markup for displaying formatted content, custom layouts, and embedded markup within the application. Script execution is blocked for security; event handlers are available in Retool.

## Retool

```toolscript
html1.setValue('<div data-click-target="primary"><p>Hello, <strong>World</strong>!</p><button>Click me</button></div>');
html1.setCss('p { color: blue; } button { background: #4CAF50; color: white; }');
```

## Ivy

```csharp
public class BasicHtmlView : ViewBase
{
    public override object? Build()
    {
        var html = "<p>Hello, <strong>World</strong>!</p>";
        return new Html(html);
    }
}
```

## Parameters

| Parameter                | Documentation                                                                 | Ivy                            |
|--------------------------|-------------------------------------------------------------------------------|--------------------------------|
| `html`                   | The HTML markup to display                                                    | `Content` (via constructor)    |
| `css`                    | CSS content for styling the HTML                                              | Not supported (inline styles only) |
| `clickable`              | Enables a Click event handler on the component                                | Not supported                  |
| `hidden`                 | Controls visibility of the component                                          | `Visible`                      |
| `id`                     | Unique identifier for the component                                           | Not applicable (C# reference)  |
| `isHiddenOnDesktop`      | Controls visibility in desktop layout                                         | Not supported                  |
| `isHiddenOnMobile`       | Controls visibility in mobile layout                                          | Not supported                  |
| `maintainSpaceWhenHidden` | Reserves space in layout when component is hidden                            | Not supported                  |
| `margin`                 | External spacing around the component (`4px 8px` default)                     | Not supported                  |
| `showInEditor`           | Shows the component in the editor even when hidden                            | Not supported                  |
| `style`                  | Custom style configuration object                                             | Not supported                  |
| N/A                      | N/A                                                                           | `Height` — vertical dimensions |
| N/A                      | N/A                                                                           | `Width` — horizontal dimensions |
| N/A                      | N/A                                                                           | `Scale` — optional scaling     |
| **Event: Change**        | Triggered when the HTML value is modified                                     | Not supported                  |
| **Event: Click**         | Triggered on click; supports `data-click-target` for targeting elements       | Not supported                  |
