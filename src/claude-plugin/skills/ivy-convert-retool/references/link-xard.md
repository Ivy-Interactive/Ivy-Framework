# Link Card

A container preconfigured with an icon and text. It is a preset version of the Container component, designed for quick navigation or action links with visual appeal.

## Retool

```toolscript
// Link Card is a preset Container with Icon and Text components.
// Configure via Inspector:
// - Set the icon
// - Set the text label
// - Add click event handler for navigation

// Typical use: navigation cards in dashboards
// Properties inherited from Container component:
linkCard1.setHidden(false);
linkCard1.setDisabled(true);
```

## Ivy

```csharp
// Ivy equivalent: Card widget with Icon and Text content
new Card(
    header: "Dashboard",
    content: new StackLayout(new object[]
    {
        new Icon(Icons.Home),
        new Text("Go to Dashboard"),
    })
)
.OnClick(() =>
{
    // Navigate or perform action
})

// Simpler approach with just icon and text:
new Card(
    content: new StackLayout(new object[]
    {
        new Icon(Icons.Settings),
        new Text("Settings"),
    })
)
.Hover(HoverEffect.Shadow)
.OnClick(() => Navigate("/settings"))
```

## Parameters

| Parameter          | Documentation                                      | Ivy                                         |
|--------------------|---------------------------------------------------|----------------------------------------------|
| icon               | The icon to display                                | `new Icon(Icons.X)` as content child         |
| text               | The text label to display                          | `new Text("...")` as content child            |
| hidden             | Whether the component is hidden                    | `Visible` (bool)                             |
| disabled           | Whether interaction is disabled                    | Not supported (Card is always interactive)   |
| clickable          | Whether click event is enabled                     | `OnClick` method                         |
| showBorder         | Whether to show a border                           | Not supported (Card has default styling)     |
| padding            | The amount of padding inside                       | Not supported                                |
| margin             | The amount of margin outside                       | Not supported                                |
| style              | Custom style options                               | `Hover` (HoverEffect)                        |
| events (Click)     | Click event handler                                | `OnClick` / `OnClick` event              |
| tooltipText        | Tooltip text on hover                              | Not supported                                |
| isHiddenOnMobile   | Whether to hide on mobile layout                   | Not supported                                |
| isHiddenOnDesktop  | Whether to hide on desktop layout                  | Not supported                                |
| Width              | Not applicable (drag-to-resize)                    | `Width` (Size)                               |
| Height             | Not applicable (drag-to-resize)                    | `Height` (Size)                              |
| header             | Not directly exposed                               | `header` constructor parameter               |
| footer             | Not directly exposed                               | `footer` constructor parameter               |
