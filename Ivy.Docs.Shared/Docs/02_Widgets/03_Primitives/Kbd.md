# Kbd

<Ingress>
Display keyboard shortcuts and key combinations with proper styling to help users identify commands and improve documentation.
</Ingress>

The `Kbd` widget displays keyboard shortcuts or key combinations with proper styling. It helps users identify key commands and improves documentation clarity.

```csharp demo-below 
Layout.Horizontal() | 
    new Kbd("Ctrl + C") | 
    new Kbd("Shift + Ctrl + C")
```

<WidgetDocs Type="Ivy.Kbd" ExtensionTypes="Ivy.KbdExtensions"  SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Primitives/Kbd.cs"/>
