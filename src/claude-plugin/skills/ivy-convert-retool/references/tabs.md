# Tabs

A set of clickable tab items that allow users to switch between different content sections. Can be linked to containers to create tabbed navigation interfaces.

## Retool

```toolscript
tabs.setValue("security")
tabs.setDisabled(true)
tabs.clearValue()
```

## Ivy

```csharp
Layout.Tabs(
    new Tab("Profile", "User profile information").Icon(Icons.User),
    new Tab("Security", "Security settings").Icon(Icons.Lock).Badge("3"),
    new Tab("Preferences", "User preferences").Icon(Icons.Settings)
).Variant(TabsVariant.Tabs)
```

```csharp
new TabsLayout(
    onSelect: (e) => Console.WriteLine($"Selected: {e.Value}"),
    onClose: (e) => Console.WriteLine($"Closed: {e.Value}"),
    onRefresh: (e) => Console.WriteLine($"Refreshed: {e.Value}"),
    onReorder: null,
    selectedIndex: 0,
    new Tab("Tab 1", "Content 1"),
    new Tab("Tab 2", "Content 2"),
    new Tab("Tab 3", "Content 3")
)
```

## Parameters

| Parameter            | Documentation                                                    | Ivy                                                        |
|----------------------|------------------------------------------------------------------|------------------------------------------------------------|
| `alignment`          | Horizontal alignment of tabs: `left`, `center`, `right`          | Not supported                                              |
| `data`               | Data source for dynamically mapped tabs                          | Not supported (tabs are defined statically via `Tab[]`)    |
| `disabled`           | Toggles whether the entire component is disabled                 | Not supported (individual tabs can be disabled)            |
| `disabledByIndex`    | Array of booleans to disable specific tabs by index              | `Tab.Disabled` property                                    |
| `heightType`         | `fixed` or `auto` sizing                                         | `Height` property (`Size`)                                 |
| `hiddenByIndex`      | Array of booleans to hide specific tabs by index                 | Not supported                                              |
| `iconByIndex`        | Icon identifiers per tab                                         | `Tab.Icon(Icons.*)`                                        |
| `iconPositionByIndex`| Icon position (`left` or `right`) per tab                        | Not supported                                              |
| `id`                 | Unique component identifier                                      | Not applicable (C# object reference)                       |
| `isHiddenOnDesktop`  | Whether to hide the component on desktop layouts                 | Not supported                                              |
| `isHiddenOnMobile`   | Whether to hide the component on mobile layouts                  | Not supported                                              |
| `itemMode`           | `dynamic` (data-mapped) or `static` (manual) tab configuration  | Static only (`Tab[]` array)                                |
| `labels`             | Display labels for each tab                                      | `Tab` constructor first argument (label string)            |
| `linePosition`       | Underline placement: `top`, `bottom`, `left`, `right`            | Not supported                                              |
| `maintainSpaceWhenHidden` | Whether to preserve layout space when the component is hidden | Not supported                                              |
| `margin`             | Outer spacing around the component                               | `Padding` property (`Thickness?`)                          |
| `selectedIndex`      | Index of the currently selected tab                              | `SelectedIndex` property (`int?`)                          |
| `selectedItem`       | The currently selected tab data object                           | Not supported (use `OnSelect` event)                       |
| `selectedLabel`      | Label of the currently selected tab                              | Not supported (use `OnSelect` event)                       |
| `styleVariant`       | Visual style: `solid`, `lineBottom`, `pill`                      | `Variant` property: `TabsVariant.Tabs`, `TabsVariant.Content` |
| `tooltipByIndex`     | Tooltips per tab item                                            | Not supported                                              |
| `tooltipText`        | General tooltip for the entire component                         | Not supported                                              |
| `value`              | Current value of the selected tab                                | `SelectedIndex`                                            |
| `values`             | Array of available tab values                                    | Defined via `Tab[]` array                                  |
| `visible`            | Whether the component is visible                                 | `Visible` property (`bool`)                                |
| **Methods**          |                                                                  |                                                            |
| `clearValue()`       | Removes the current selection                                    | Not supported                                              |
| `resetValue()`       | Restores the default selection                                   | Not supported                                              |
| `scrollIntoView()`   | Scrolls the component into the viewport                          | Not supported                                              |
| `setDisabled()`      | Toggles the disabled state programmatically                      | Not supported                                              |
| `setHidden()`        | Toggles visibility programmatically                              | `Visible` property                                         |
| `setValue()`         | Sets the selected tab value programmatically                     | `SelectedIndex` property                                   |
| **Events**           |                                                                  |                                                            |
| Change               | Fires when the selected tab changes                              | `OnSelect` event                                           |
| —                    | —                                                                | `OnClose` event (closable tabs)                            |
| —                    | —                                                                | `OnRefresh` event                                          |
| —                    | —                                                                | `OnReorder` event (drag-and-drop)                          |
| —                    | —                                                                | `OnAddButtonClick` event                                   |
| **Ivy-only**         |                                                                  |                                                            |
| —                    | —                                                                | `Badge(string)` per tab                                    |
| —                    | —                                                                | `AddButtonText` (add tab button)                           |
| —                    | —                                                                | `RemoveParentPadding` (`bool`)                             |
| —                    | —                                                                | `Scale` property                                           |
| —                    | —                                                                | `Width` property (`Size`)                                  |
