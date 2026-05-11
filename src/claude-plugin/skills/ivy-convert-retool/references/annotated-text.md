# Annotated Text

An interface to display and annotate text. Allows highlighting and labeling specific portions of text content using character-position-based tags.

## Retool

```toolscript
// Configure the Annotated Text component
annotatedText1.text = "The quick brown fox jumps over the lazy dog";
annotatedText1.tags = [
  { id: 1, start: 4, end: 9,  label: "Adjective" },
  { id: 2, start: 10, end: 15, label: "Adjective" },
  { id: 3, start: 16, end: 19, label: "Noun" },
  { id: 4, start: 35, end: 39, label: "Adjective" },
  { id: 5, start: 40, end: 43, label: "Noun" }
];

// Scroll into view
annotatedText1.scrollIntoView({ behavior: 'smooth', block: 'center' });
```

## Ivy

No dedicated Annotated Text widget exists in Ivy. The closest approach is using the `Html` widget with styled `<span>` elements to simulate text annotations:

```csharp
// Using Html to render annotated text with highlighted spans
var html = @"
  <p>
    The <span style='background-color: #fde68a; padding: 2px 4px; border-radius: 2px;'>quick</span>
    <span style='background-color: #fde68a; padding: 2px 4px; border-radius: 2px;'>brown</span>
    <span style='background-color: #bfdbfe; padding: 2px 4px; border-radius: 2px;'>fox</span>
    jumps over the
    <span style='background-color: #fde68a; padding: 2px 4px; border-radius: 2px;'>lazy</span>
    <span style='background-color: #bfdbfe; padding: 2px 4px; border-radius: 2px;'>dog</span>
  </p>";

return new Html(html);
```

## Parameters

| Parameter              | Documentation                                                                 | Ivy           |
|------------------------|-------------------------------------------------------------------------------|---------------|
| `text`                 | The primary text content to display                                           | `Html.Content` (raw HTML string) |
| `tags`                 | Array of annotations with `id`, `start`, `end`, and `label` fields            | Not supported (must be constructed manually in HTML) |
| `labels`               | Read-only list of labels for each annotated item                              | Not supported |
| `id`                   | Unique component identifier                                                   | Not applicable (C# variable reference) |
| `margin`               | External spacing around the component (`4px 8px` or `0`)                      | Not supported (use inline CSS in HTML) |
| `isHiddenOnDesktop`    | Whether to hide the component on desktop                                      | `Html.Visible` |
| `isHiddenOnMobile`     | Whether to hide the component on mobile                                       | Not supported |
| `maintainSpaceWhenHidden` | Whether to reserve layout space when hidden                                | Not supported |
| `showInEditor`         | Whether the component remains visible in the editor when hidden               | Not supported |
| `events` / `onTagChange` | Event handler triggered when the annotation value changes                  | Not supported |
| `scrollIntoView()`     | Scrolls the component into the visible area                                   | Not supported |
