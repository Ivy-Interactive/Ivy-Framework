# Container

A container to group other components together with flexible layout controls. Supports header, body, and footer sections, multiple switchable views, click events, loading states, and visibility toggling.

## Retool

```toolscript
// Basic container with header and footer
container1.setShowHeader(true);
container1.setShowFooter(true);

// Switch between views
container1.setCurrentView("userView");
container1.setCurrentViewIndex(1);
container1.showNextView();
container1.showPreviousView();

// Toggle visibility and disabled state
container1.setHidden(false);
container1.setDisabled(true);

// Scroll into view
container1.scrollIntoView({ behavior: 'smooth', block: 'center' });
```

## Ivy

The Retool Container maps primarily to Ivy's `Card` widget (for grouping content with header/footer) and `TabsLayout` (for multi-view switching). The `Box` primitive provides lower-level container styling.

```csharp
// Card — closest equivalent to a container with header, body, and footer
new Card(
    content: "Body content goes here",
    footer: new Button("Submit", _ => client.Toast("Submitted!")),
    header: Text.H3("Custom Header")
).Title("My Container")
 .Description("A grouped section")
 .Width(Size.Units(100))

// Card with click handler
new Card("Clickable container content")
    .Title("Clickable Card")
    .OnClick(_ => client.Toast("Card clicked!"))

// TabsLayout — for multi-view switching (Retool Container views)
Layout.Tabs(
    new Tab("View 1", "Content for view 1"),
    new Tab("View 2", "Content for view 2"),
    new Tab("View 3", "Content for view 3")
)

// Box — lower-level container with border, padding, and color control
new Box("Grouped content")
    .Color(Colors.White)
    .BorderRadius(BorderRadius.Rounded)
    .BorderThickness(1)
    .Padding(8)
    .Margin(4)
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `id` | The unique identifier (name) of the container. Default: `container1` | Not needed — widgets are referenced by variable name |
| `showHeader` | Whether to show the header area. Default: `false` | `Card` constructor `header` parameter; pass `null` to hide |
| `showFooter` | Whether to show the footer area. Default: `false` | `Card` constructor `footer` parameter; pass `null` to hide |
| `showBody` | Whether to show the body. | `Card` constructor `content` parameter |
| `showBorder` | Whether to show a border. Default: `false` | `Box.BorderStyle(BorderStyle.Solid)` / `BorderStyle.None` |
| `showHeaderBorder` | Whether to show a border under the header. Default: `true` | Not supported |
| `showFooterBorder` | Whether to show a border above the footer. | Not supported |
| `headerPadding` | Padding within the header. Default: `8px 12px` | Not supported (Card header padding is automatic) |
| `footerPadding` | Padding within the footer. Default: `8px 12px` | Not supported (Card footer padding is automatic) |
| `padding` | Padding inside the container. Default: `4px 8px` | `Box.Padding(Thickness)` or `StackLayout` padding parameter |
| `margin` | Margin outside the container. Default: `4px 8px` | `Box.Margin(Thickness)` |
| `hidden` | Whether the component is hidden. Default: `false` | `Card.Visible` / `Box.Visible` (bool) |
| `disabled` | Whether interaction is disabled. Default: `false` | Not supported |
| `loading` | Whether to display a loading indicator. Default: `false` | Not supported (use `UseQuery` for automatic loading states) |
| `hoistFetching` | Show loading when nested objects fetch data. Default: `false` | Not supported (handled automatically by `UseQuery`) |
| `clickable` | Whether there is a click event handler. Default: `false` | `Card.OnClick(_ => ...)` — automatically makes it clickable |
| `maintainSpaceWhenHidden` | Take up space when hidden. Default: `false` | Not supported |
| `showInEditor` | Remain visible in editor when hidden. Default: `false` | Not supported (no editor concept) |
| `isHiddenOnDesktop` | Hide on desktop layout. Default: `false` | Not supported |
| `isHiddenOnMobile` | Hide on mobile layout. Default: `true` | Not supported |
| `enableFullBleed` | Expand contents to fill available space. Default: `false` | `Size.Fraction(1f)` on width/height for full expansion |
| `views` | List of views configured for the container. | `TabsLayout` with `Tab` objects |
| `currentViewIndex` | Index of the selected view. Read-only. | `TabsLayout.SelectedIndex` |
| `currentViewKey` | Key of the selected view. Read-only. | Not supported (use index-based selection) |
| `viewKeys` | List of view keys. Read-only. | Not supported |
| `itemMode` | Config mode: `static` or `dynamic`. Default: `static` | Not supported |
| `transition` | Animation when switching views (`none`, `fade`, `slide`). | Not supported |
| `labels` | List of labels for each view. | `Tab` constructor first parameter (name) |
| `tooltipText` | Tooltip text on hover. | Not supported |
| `tooltipByIndex` | Tooltips for each view, by index. | Not supported |
| `iconByIndex` | Icons for each view, by index. | `Tab.Icon(Icons.X)` |
| `iconPositionByIndex` | Icon positions relative to labels. | Not supported |
| `style` | Custom style options. | `Box.Color()`, `.BorderStyle()`, `.BorderRadius()`, etc. |
| `hovered` | Whether the component is hovered. Read-only. | Not supported |
| `events` | Configured event handlers (Click, Change). | `Card.OnClick`, `TabsLayout.OnSelect` |

### Methods

| Method | Documentation | Ivy |
|--------|---------------|-----|
| `setCurrentView(key)` | Set the current view by key. | Not supported (use `selectedIndex` in `TabsLayout`) |
| `setCurrentViewIndex(index)` | Set the current view by index. | `TabsLayout` `selectedIndex` parameter |
| `showNextView()` | Show the next view. | Not supported |
| `showPreviousView()` | Show the previous view. | Not supported |
| `showNextVisibleView()` | Show the next visible view. | Not supported |
| `showPreviousVisibleView()` | Show the previous visible view. | Not supported |
| `setHidden(bool)` | Toggle visibility. | Set `Visible` property on `Card`/`Box` |
| `setDisabled(bool)` | Toggle disabled state. | Not supported |
| `setShowBody(bool)` | Toggle body visibility. | Not supported |
| `setShowHeader(bool)` | Toggle header visibility. | Not supported (header is present if passed to constructor) |
| `setShowFooter(bool)` | Toggle footer visibility. | Not supported (footer is present if passed to constructor) |
| `focus()` | Set focus on the container. | Not supported |
| `scrollIntoView(options)` | Scroll the container into the visible area. | Not supported |
