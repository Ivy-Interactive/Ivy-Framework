# Page Input

An input field to jump to a specific page of data. Displays the total number of pages and automatically updates to reflect the currently selected page. Unlike numbered pagination links, Page Input allows direct numeric entry for page navigation.

## Retool

```toolscript
// Set the page value programmatically
pageInput.setValue(5);

// Clear the current value
pageInput.clearValue();

// Reset to default value
pageInput.resetValue();

// Disable/enable the input
pageInput.setDisabled(true);

// Hide/show the component
pageInput.setHidden(false);

// Scroll into view
pageInput.scrollIntoView({ behavior: "smooth", block: "center" });
```

## Ivy

Ivy does not have a dedicated "Page Input" widget. The closest equivalent is the `Pagination` widget, which provides page navigation controls including page links, previous/next buttons, and ellipsis for skipped ranges.

```csharp
var page = UseState(1);
var totalPages = 10;

new Pagination(
    page: page.Value,
    numPages: totalPages,
    onChange: e => page.Set(e.Value)
);
```

## Parameters

| Parameter              | Documentation                                                                 | Ivy                                                                                         |
|------------------------|-------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------|
| `disabled`             | Whether input/interaction is disabled. Default: `false`.                      | `Disabled` (bool)                                                                           |
| `hidden`               | Whether the component is hidden from view. Default: `false`.                  | `Visible` (bool, inverted logic)                                                            |
| `id`                   | Unique identifier for the component. Default: `pageInput1`.                   | Not supported (widgets are referenced by variable)                                          |
| `max`                  | Maximum allowed page value. Default: `null`.                                  | `NumPages` (int?, serves as the upper bound)                                                |
| `textBefore`           | Prefix text (e.g., "Page"). Default: `null`.                                 | Not supported                                                                               |
| `textAfter`            | Suffix text (e.g., "pages"). Default: `null`.                                | Not supported                                                                               |
| `horizontalAlign`      | Content alignment: left, center, right, full. Default: `left`.               | Not supported                                                                               |
| `margin`               | Outer spacing: "Normal" (`4px 8px`) or "None" (`0`). Default: `4px 8px`.     | Not supported (handled by layout)                                                           |
| `style`                | Custom CSS styling. Default: `null`.                                         | Not supported                                                                               |
| `events` (Change)      | Event handler triggered when the page value changes.                         | `OnChange` (async `Func<Event<Pagination, int>, ValueTask>` or sync `Action<Event<Pagination, int>>`) |
| `showInEditor`         | Visibility in edit mode when hidden. Default: `false`.                       | Not supported                                                                               |
| `isHiddenOnDesktop`    | Desktop layout visibility toggle. Default: `false`.                          | Not supported                                                                               |
| `isHiddenOnMobile`     | Mobile layout visibility toggle. Default: `true`.                            | Not supported                                                                               |
| `maintainSpaceWhenHidden` | Whether to occupy space when hidden. Default: `false`.                    | Not supported                                                                               |
| N/A                    | N/A                                                                           | `Siblings` (int?, visible pages adjacent to current page)                                   |
| N/A                    | N/A                                                                           | `Boundaries` (int?, visible pages at range start/end)                                       |
| N/A                    | N/A                                                                           | `Scale` (Scale?, size scaling option)                                                       |
| N/A                    | N/A                                                                           | `Height` (Size, custom height)                                                              |
| N/A                    | N/A                                                                           | `Width` (Size, custom width)                                                                |
| **Method** `setValue`  | Sets the current page value.                                                 | Managed via state: `page.Set(value)`                                                        |
| **Method** `clearValue`| Clears the current value.                                                    | Not supported                                                                               |
| **Method** `resetValue`| Resets to default value.                                                     | Not supported                                                                               |
| **Method** `setDisabled`| Toggles disabled state.                                                     | Set `Disabled` property directly                                                            |
| **Method** `setHidden` | Toggles visibility.                                                          | Set `Visible` property directly                                                             |
| **Method** `scrollIntoView` | Scrolls component into view with behavior/block options.                | Not supported                                                                               |
