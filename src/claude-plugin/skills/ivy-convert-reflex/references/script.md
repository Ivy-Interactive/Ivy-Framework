# Script

The `rx.script` component enables inclusion of inline JavaScript or external JavaScript files via URL in a Reflex application. It wraps Next.js's `next/script` component and supports conditional rendering for controlled script side effects. Typical use cases include tracking/analytics scripts, social media embeds, and third-party service integrations.

## Reflex

```python
import reflex as rx

rx.script(src="https://example.com/tracking.js")

rx.script("alert('Hello World!')")
```

## Ivy

Ivy does not have a direct equivalent of the Reflex `Script` component. The closest primitive is the `Html` widget, which renders raw HTML content — however it **explicitly strips all `<script>` tags, `on*` event handlers, and `javascript:` URLs** for security.

```csharp
// Html widget — scripts are NOT executed
public class HtmlExample : ViewBase
{
    public override object? Build()
    {
        return new Html("<p>Hello, <strong>World</strong>!</p>");
    }
}
```

## Parameters

| Parameter         | Documentation                                              | Ivy           |
|-------------------|------------------------------------------------------------|---------------|
| `src`             | URL to an external JavaScript file                         | Not supported |
| `async_`          | Execute script asynchronously                              | Not supported |
| `defer`           | Defer script execution until page load                     | Not supported |
| `type`            | MIME type of the script                                    | Not supported |
| `char_set`        | Character encoding specification                           | Not supported |
| `cross_origin`    | Cross-origin request handling (`anonymous`, `use-credentials`) | Not supported |
| `integrity`       | Subresource integrity hash for verification                | Not supported |
| `referrer_policy` | Referrer policy for script requests                        | Not supported |
| `custom_attrs`    | Additional HTML attributes passed to the script tag        | Not supported |
