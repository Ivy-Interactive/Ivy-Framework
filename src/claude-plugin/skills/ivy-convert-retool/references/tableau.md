# Tableau

A content area to display a Tableau visualization dashboard. Embeds a Tableau dashboard directly within the Retool application with support for reading sheet data and selected marks.

## Retool

```toolscript
// Tableau component configured in the inspector
tableau1.tableauUrl = "https://public.tableau.com/views/MyDashboard"

// Read sheet data
const data = tableau1.sheetData
const names = tableau1.sheetNames
const marks = tableau1.selectedMarks

// Scroll into view
tableau.scrollIntoView({behavior: 'auto', block: 'nearest'});
```

## Ivy

Ivy does not have a dedicated Tableau widget. Use the `Iframe` widget to embed Tableau dashboards, or the `Embed` widget for auto-detected content. For interactive features like reading sheet data or selected marks, build a custom `ExternalWidget` wrapping the Tableau JavaScript API.

```csharp
// Using Iframe to embed a Tableau dashboard
new Iframe("https://public.tableau.com/views/MyDashboard")
    .Width(Size.Full())
    .Height(Size.Units(150));

// Or using Embed for auto-detected content
new Embed("https://public.tableau.com/views/MyDashboard");

// For interactive features, use an ExternalWidget
[ExternalWidget(
    "frontend/dist/TableauWidget.js",
    StylePath = "frontend/dist/style.css",
    ExportName = "TableauWidget",
    GlobalName = "MyApp_Widgets_TableauWidget")]
public record TableauWidget : WidgetBase<TableauWidget>
{
    [Prop] public string? TableauUrl { get; set; }
    [Event] public Func<Event<TableauWidget>, ValueTask>? OnMarksSelected { get; set; }
}
```

## Parameters

| Parameter              | Documentation                                                  | Ivy                                                          |
|------------------------|----------------------------------------------------------------|--------------------------------------------------------------|
| tableauUrl             | The URL of the Tableau dashboard                               | `new Iframe(src)` / `new Embed(url)`                         |
| sheetData              | The data in the sheet (read-only)                              | Not supported (requires ExternalWidget)                      |
| sheetNames             | The names of the sheets in the workbook (read-only)            | Not supported (requires ExternalWidget)                      |
| selectedMarks          | The selected marks (read-only)                                 | Not supported (requires ExternalWidget)                      |
| hidden                 | Whether the component is hidden from view                      | `.Visible(bool)` on Iframe                                   |
| id                     | The unique identifier (name)                                   | Variable name in C#                                          |
| margin                 | The amount of margin to render outside                         | Not supported (use layout containers)                        |
| style                  | Custom style options                                           | `.Width()`, `.Height()`, `.Scale()` on Iframe                |
| isHiddenOnDesktop      | Whether to hide on desktop layout                              | Not supported                                                |
| isHiddenOnMobile       | Whether to hide on mobile layout                               | Not supported                                                |
| maintainSpaceWhenHidden| Whether to take up space when hidden                           | Not supported                                                |
| showInEditor           | Whether to show in editor when hidden                          | Not supported                                                |
| scrollIntoView()       | Scrolls the component into the visible area                    | Not supported                                                |
