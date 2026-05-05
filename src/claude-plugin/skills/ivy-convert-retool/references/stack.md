# Stack

A container to group other components together in a flexbox layout. Supports multiple views, header/footer areas, border controls, and event handlers similar to Container but with flexbox-based arrangement.

## Retool

```toolscript
// Stack groups components in a flexbox layout.
// Configure direction, alignment, gap, and wrapping in the Inspector.

// Switch between views:
stack1.setCurrentView("userView");
stack1.setCurrentViewIndex(1);

// Navigate views:
stack1.showNextView();
stack1.showPreviousView();

// Toggle sections:
stack1.setShowHeader(true);
stack1.setShowFooter(true);
stack1.setShowBody(false);

// Control visibility and interaction:
stack1.setHidden(false);
stack1.setDisabled(true);

// Scroll into view:
stack1.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
```

## Ivy

```csharp
// Ivy equivalent: StackLayout widget
new StackLayout(
    new object[]
    {
        new Text("Item 1"),
        new Text("Item 2"),
        new Text("Item 3"),
    },
    orientation: Orientation.Vertical,
    gap: 4,
    padding: new Thickness(2),
    align: Align.Center
)

// Horizontal stack:
new StackLayout(
    new object[]
    {
        new Button("Save"),
        new Button("Cancel"),
    },
    orientation: Orientation.Horizontal,
    gap: 2
)

// With background color:
new StackLayout(
    new object[] { new Text("Styled Stack") },
    background: Colors.Sky,
    padding: new Thickness(4)
)
```

## Parameters

| Parameter               | Documentation                                            | Ivy                                              |
|-------------------------|----------------------------------------------------------|--------------------------------------------------|
| direction/layout        | Flexbox layout direction                                 | `Orientation` (Orientation.Vertical/Horizontal)  |
| gap                     | Spacing between child components                         | `Gap` (int, default: 4)                          |
| padding                 | The amount of padding inside                             | `Padding` (Thickness)                            |
| margin                  | The amount of margin outside                             | `Margin` (Thickness)                             |
| alignment               | Content alignment                                        | `Align` (Align)                                  |
| hidden                  | Whether the component is hidden                          | `Visible` (bool)                                 |
| disabled                | Whether interaction is disabled                          | Not supported                                    |
| loading                 | Whether to display a loading indicator                   | Not supported                                    |
| showBody                | Whether to show the body                                 | Not supported                                    |
| showHeader              | Whether to show the header area                          | Not supported                                    |
| showFooter              | Whether to show the footer area                          | Not supported                                    |
| showBorder              | Whether to show a border                                 | Not supported (use Box wrapper)                  |
| showHeaderBorder        | Border under header                                      | Not supported                                    |
| showFooterBorder        | Border above footer                                      | Not supported                                    |
| headerPadding           | Padding within header                                    | Not supported                                    |
| footerPadding           | Padding within footer                                    | Not supported                                    |
| hoistFetching           | Show loading when nested objects fetch                   | Not supported                                    |
| enableFullBleed         | Expand contents to fill space                            | `RemoveParentPadding` (bool)                     |
| clickable               | Whether click handler is enabled                         | Not supported                                    |
| currentViewIndex        | Index of the selected view                               | Not supported                                    |
| views                   | List of configured views                                 | Not supported                                    |
| transition              | View switch animation                                    | Not supported                                    |
| tooltipText             | Tooltip text on hover                                    | Not supported                                    |
| style                   | Custom style options                                     | `Background` (Colors)                            |
| events (Click, Change)  | Event handlers                                           | Not supported                                    |
| Width                   | Not applicable (drag-to-resize)                          | `Width` (Size)                                   |
| Height                  | Not applicable (drag-to-resize)                          | `Height` (Size)                                  |
| Scale                   | Not applicable                                           | `Scale` (Scale?)                                 |
