# FooterLayout

`FooterLayout` pins footer content to the bottom of the view while the main content
scrolls above it.

```csharp
new FooterLayout(
    new Button("Save", _ => client.Toast("Sheet Saved")),
    "This is the content"
);
```
