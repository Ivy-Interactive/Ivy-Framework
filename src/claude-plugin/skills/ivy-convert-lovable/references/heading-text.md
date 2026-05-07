# Heading & Text

Typography elements. Lovable apps use HTML heading tags and paragraphs styled with Tailwind classes.

## Lovable

```tsx
<h1 className="text-3xl font-bold">Page Title</h1>
<h2 className="text-2xl font-semibold">Section Title</h2>
<h3 className="text-xl font-medium">Subsection</h3>
<p className="text-sm text-muted-foreground">Description text</p>
<p className="text-lg font-bold">$12,345</p>
<span className="text-xs text-red-500">Error message</span>
```

## Ivy

```csharp
new Heading("Page Title", HeadingLevel.H1);
new Heading("Section Title", HeadingLevel.H2);
new Heading("Subsection", HeadingLevel.H3);
new Text("Description text").Muted();
new Text("$12,345").Bold();
new Text("Error message").Color(Colors.Red);
```

## Parameters

| Pattern | Lovable | Ivy |
|---------|---------|-----|
| `<h1>` | `className="text-3xl font-bold"` | `new Heading(text, HeadingLevel.H1)` |
| `<h2>` | `className="text-2xl font-semibold"` | `new Heading(text, HeadingLevel.H2)` |
| `<h3>` | `className="text-xl font-medium"` | `new Heading(text, HeadingLevel.H3)` |
| `<p>` | Paragraph text | `new Text(text)` |
| Bold text | `font-bold` | `.Bold()` |
| Muted text | `text-muted-foreground` | `.Muted()` |
| Colored text | `text-red-500` etc. | `.Color(Colors.Red)` |
| Small text | `text-sm` or `text-xs` | `.Small()` |
