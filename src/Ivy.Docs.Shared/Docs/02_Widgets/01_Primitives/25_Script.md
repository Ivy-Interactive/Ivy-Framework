---
searchHints:
  - script
  - javascript
  - js
  - analytics
  - tracking
  - external
  - inline
---

# Script

<Ingress>
Load external JavaScript files or execute inline JavaScript code as a side effect, without rendering any visible output.
</Ingress>

The `Script` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) allows you to include external JavaScript files or run inline JavaScript snippets in your [app](../../01_Onboarding/02_Concepts/15_Apps.md). It renders no visible HTML — it simply injects a `<script>` element into the page head.

## Basic Usage

### External Script

Load an external JavaScript file by URL:

```csharp demo-below
new Script("https://cdn.jsdelivr.net/npm/canvas-confetti@1.9.3/dist/confetti.browser.min.js")
```

### Inline JavaScript

Execute inline JavaScript code:

```csharp demo-below
new Script().InlineCode("console.log('Hello from Ivy!');")
```

## Configuration Options

### Async and Defer

Control script loading behavior:

```csharp demo-tabs
Layout.Vertical().Gap(4)
    | Text.H4("Async Loading")
    | new Script("https://example.com/analytics.js").Async()
    | Text.H4("Deferred Execution")
    | new Script("https://example.com/widget.js").Defer()
```

### Subresource Integrity

Verify external scripts haven't been tampered with:

```csharp
new Script("https://cdn.example.com/lib.js")
    .Integrity("sha384-oqVuAfXRKap7fdgcCY5uykM6+R9GqQ8K/uxy9rx7HNQlGYl1kPzQho1wx4JwY8w")
    .CrossOrigin("anonymous")
```

## Security Considerations

- **Never pass user-supplied input** to `Src` or `InlineCode` — this widget executes arbitrary JavaScript
- Use `Integrity` for external scripts to prevent tampering
- Set `CrossOrigin` when using integrity checks

<WidgetDocs Type="Ivy.Script" ExtensionTypes="Ivy.ScriptExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Primitives/Script.cs"/>
