# Reorderable List

A content area to display a list of text items that can be reordered when dragged. Displays items in a vertical list and allows users to rearrange them via drag-and-drop interactions. A `Change` event fires when the order changes.

## Retool

```toolscript
// Configure value via Inspector: ["Item A", "Item B", "Item C", "Item D"]
// renderedValue supports Markdown/HTML per item: ["**Item A**", "**Item B**", ...]

// Read the current (possibly reordered) value
reorderableList1.value // => ["Item C", "Item A", "Item B", "Item D"]

// Event handler: fires when items are reordered
// Configure via Inspector > Interaction > Event handlers > Change
reorderableList1.onReorder = {{ reorderQuery.trigger() }}

// Scroll into view programmatically
reorderableList1.scrollIntoView({ behavior: "smooth", block: "center" });
```

## Ivy

Ivy's `List` widget displays items vertically with titles, subtitles, icons, badges, and click handlers. It does **not** natively support drag-and-drop reordering.

```csharp
public class ReorderableListComparison : ViewBase
{
    public override object? Build()
    {
        var items = UseState(new[] { "Item A", "Item B", "Item C", "Item D" });

        var onClick = new Action<Event<ListItem>>(e =>
        {
            var client = UseService<IClientProvider>();
            client.Toast($"Clicked: {e.Sender.Title}");
        });

        var listItems = items.Value.Select(item =>
            new ListItem(item, onClick: onClick, icon: Icons.GripVertical));

        return new List(listItems);
    }
}
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `value` | Array of string items (reflects user-reordered state) | `new List(object[])` or `new List(IEnumerable<object>)` |
| `renderedValue` | Markdown/HTML-formatted text rendered for each item | `ListItem` with `title`, `subtitle`, `icon`, `badge` |
| `onReorder` (Change event) | Event fired when items are reordered by dragging | Not supported |
| `events` | Configurable event handlers that trigger actions or queries | `ListItem.onClick` handler |
| `hidden` | Whether the component is hidden from view | `Visible` property (inverted logic) |
| `id` | Unique identifier (name) of the component | Not applicable (C# variable reference) |
| `isHiddenOnDesktop` | Show/hide in desktop layout | Not supported |
| `isHiddenOnMobile` | Show/hide in mobile layout | Not supported |
| `maintainSpaceWhenHidden` | Whether to reserve space when hidden | Not supported |
| `margin` | Margin around the component (`"4px 8px"` or `"0"`) | Not supported (use layout wrappers) |
| `style` | Custom style options object | Not supported |
| `showInEditor` | Whether visible in editor when hidden | Not applicable |
| `scrollIntoView()` | Scrolls the canvas so the component appears in the visible area | Not supported |
| Item subtitle | Not supported | `new ListItem(title, subtitle: "...")` |
| Item icon | Not supported | `new ListItem(title, icon: Icons.Star)` |
| Item badge | Not supported | `new ListItem(title, badge: "New")` |
| Nested content | Not supported | `new ListItem(title, items: widget[])` |
| Search / filter | Not supported | Manual via `UseState` + filtering logic |
| `Height` | Not documented | `.Height(Size)` |
| `Width` | Not documented | `.Width(Size)` |
| `Scale` | Not documented | `.Scale(Scale?)` |
