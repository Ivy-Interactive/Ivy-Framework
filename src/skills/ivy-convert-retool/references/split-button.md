# Split Button

A dropdown menu with buttons to trigger actions when clicked. Enables users to select from multiple action options via a dropdown interface.

## Retool

```toolscript
splitButton1.selectedLabel
splitButton1.selectedItem
splitButton1.selectedIndex
```

## Ivy

In Ivy, a Split Button is composed using a `DropDownMenu` with a `Button` as the trigger element.

```csharp
new DropDownMenu(
    onSelect: e => client.Toast($"Selected: {e.Value}"),
    trigger: new Button("Actions", icon: Icons.ChevronDown),
    items: new[]
    {
        MenuItem.Default("Save", Icons.Save).Tag("save"),
        MenuItem.Default("Export", Icons.Download).Tag("export"),
        MenuItem.Separator(),
        MenuItem.Default("Delete", Icons.Trash).Tag("delete"),
    }
);
```

## Parameters

| Parameter              | Documentation                                          | Ivy                                                                 |
|------------------------|--------------------------------------------------------|---------------------------------------------------------------------|
| `disabled`             | Disables input and interaction                         | `Button.Disabled`                                                   |
| `horizontalAlign`      | Alignment: left, center, right, stretch                | `DropDownMenu.Align(AlignOptions)`                                  |
| `id`                   | Unique component identifier                            | Not supported (implicit)                                            |
| `isHiddenOnDesktop`    | Hides component in desktop layout                      | Not supported                                                       |
| `isHiddenOnMobile`     | Hides component in mobile layout                       | Not supported                                                       |
| `loading`              | Displays loading indicator                             | `Button.Loading`                                                    |
| `maintainSpaceWhenHidden` | Reserves space when component is hidden             | Not supported                                                       |
| `margin`               | External spacing around the component                  | Not supported (handled by layout)                                   |
| `overlayMaxHeight`     | Maximum dropdown height in pixels                      | Not supported                                                       |
| `overlayMinWidth`      | Minimum dropdown width in pixels                       | Not supported                                                       |
| `selectedIndex`        | Index of the selected item (read-only)                 | Not supported (use `OnSelect` event value)                          |
| `selectedItem`         | Selected item object (read-only)                       | `Event<DropDownMenu, object>.Value` in `OnSelect`                   |
| `selectedLabel`        | Label text of selected item (read-only)                | Not supported (use `Tag` on `MenuItem`)                             |
| `styleVariant`         | Visual style: solid or outline                         | `Button.Variant` (Primary, Secondary, Outline, Ghost, etc.)         |
| `tooltipText`          | Helper text on hover                                   | `Button.Tooltip`                                                    |
| `setDisabled()`        | Method to toggle disabled state                        | Set `Button.Disabled` directly                                      |
| `setHidden()`          | Method to toggle visibility                            | `Button.Visible` / `DropDownMenu.Visible`                           |
| `scrollIntoView()`     | Scrolls component into view                            | Not supported                                                       |
| Click event            | Triggered when an item is selected                     | `OnSelect` event handler                                            |
