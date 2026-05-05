# Pagination

A navigation menu to jump to a specific page of data. Displays page links, previous/next buttons, and optional ellipsis for skipped ranges. Automatically adjusts visible pages to fit available width.

## Retool

```toolscript
// Read the current page number
pagination1.value

// Set the page programmatically
pagination1.setValue(5);

// React to page changes via the Change event handler
// Configure max pages
pagination1.max = 20;

// Disable/enable
pagination1.setDisabled(true);
pagination1.setHidden(false);
```

## Ivy

```csharp
[Inject] public IState<int> Page { get; set; }

new Pagination(
    page: Page.Value,
    numPages: 20,
    onChange: e => Page.Set(e.Value)
)
.Siblings(2)
.Boundaries(1)
```

## Parameters

| Parameter                | Documentation                                                    | Ivy                                              |
|--------------------------|------------------------------------------------------------------|--------------------------------------------------|
| `value` / `valueNumber`  | Current page number (read-only in Retool)                        | `page` (int?) constructor parameter              |
| `max`                    | Maximum number of pages                                          | `numPages` (int?) constructor parameter           |
| `disabled`               | Whether the component is disabled                                | `disabled` (bool) constructor parameter           |
| `hidden`                 | Whether the component is visible                                 | `Visible` (bool) property                         |
| `margin`                 | Outer spacing around the component                               | Not supported                                     |
| `isHiddenOnMobile`       | Hide on mobile layout                                            | Not supported                                     |
| `isHiddenOnDesktop`      | Hide on desktop layout                                           | Not supported                                     |
| `maintainSpaceWhenHidden`| Keep layout space when hidden                                    | Not supported                                     |
| `showInEditor`           | Display in editor mode                                           | Not supported                                     |
| `style`                  | Custom styling object                                            | `Scale` (Scale?) property                         |
| `events.Change`          | Fires when the page value changes                                | `onChange` callback in constructor                 |
| N/A                      | N/A                                                              | `Siblings` (int?) - visible links near current    |
| N/A                      | N/A                                                              | `Boundaries` (int?) - links at range start/end    |
| `setValue()`             | Set pagination to a specific page                                | Set via `page` parameter + state                  |
| `clearValue()`           | Remove current selection                                         | Not supported                                     |
| `resetValue()`           | Restore default value                                            | Not supported                                     |
| `setDisabled()`          | Toggle disabled state                                            | Set via `disabled` parameter                      |
| `setHidden()`            | Toggle visibility                                                | Set via `Visible` property                        |
| `scrollIntoView()`       | Scroll component into viewport                                   | Not supported                                     |
