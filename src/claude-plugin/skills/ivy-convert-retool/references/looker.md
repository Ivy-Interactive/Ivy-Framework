# Looker

A content area to display a Looker visualization dashboard. Embeds a Looker dashboard directly within the Retool application.

## Retool

```toolscript
// Looker component configured in the inspector
// embedUrl is read-only, configured via the inspector
looker1.embedUrl // "https://mycompany.looker.com/embed/dashboards/42"

// Scroll into view
looker.scrollIntoView({behavior: 'auto', block: 'nearest'});
```

## Ivy

Ivy does not have a dedicated Looker widget. Use the `Iframe` widget to embed Looker dashboards, or the `Embed` widget for auto-detected content.

```csharp
// Using Iframe to embed a Looker dashboard
new Iframe("https://mycompany.looker.com/embed/dashboards/42")
    .Width(Size.Full())
    .Height(Size.Units(150));

// Or using Embed for auto-detected content
new Embed("https://mycompany.looker.com/embed/dashboards/42");
```

## Parameters

| Parameter              | Documentation                                                  | Ivy                                                          |
|------------------------|----------------------------------------------------------------|--------------------------------------------------------------|
| embedUrl               | The URL of the Looker dashboard (read-only)                    | `new Iframe(src)` / `new Embed(url)`                         |
| hidden                 | Whether the component is hidden from view                      | `.Visible(bool)` on Iframe                                   |
| id                     | The unique identifier (name)                                   | Variable name in C#                                          |
| margin                 | The amount of margin to render outside                         | Not supported (use layout containers)                        |
| style                  | Custom style options                                           | `.Width()`, `.Height()`, `.Scale()` on Iframe                |
| isHiddenOnDesktop      | Whether to hide on desktop layout                              | Not supported                                                |
| isHiddenOnMobile       | Whether to hide on mobile layout                               | Not supported                                                |
| maintainSpaceWhenHidden| Whether to take up space when hidden                           | Not supported                                                |
| showInEditor           | Whether to show in editor when hidden                          | Not supported                                                |
| scrollIntoView()       | Scrolls the component into the visible area                    | Not supported                                                |
