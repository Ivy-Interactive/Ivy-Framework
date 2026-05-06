# Dropdown Button

A dropdown menu with buttons to trigger actions when clicked. The component displays a button that, when pressed, reveals a list of selectable menu items.

## Retool

```toolscript
dropdownButton1.text = "Menu"
dropdownButton1.styleVariant = "outline"
dropdownButton1.icon = "bold/interface-arrows-button-down"
dropdownButton1.disabled = false
dropdownButton1.tooltipText = "Click to open menu"

// Event handler
dropdownButton1.click = (item) => {
  console.log("Selected:", item)
}
```

## Ivy

```csharp
new DropDownMenu(
    @evt => client.Toast("Selected: " + @evt.Value),
    new Button("Menu"),
    MenuItem.Default("Profile"),
    MenuItem.Default("Settings"),
    MenuItem.Separator(),
    MenuItem.Default("Logout"))
```

## Parameters

| Parameter              | Documentation                                        | Ivy                                                    |
|------------------------|------------------------------------------------------|--------------------------------------------------------|
| `text`                 | Primary button label text (default: "Menu")          | Set via the trigger element, e.g. `new Button("Menu")` |
| `disabled`             | Disables interaction (default: false)                | Not supported directly; disable the trigger Button     |
| `disabledByIndex`      | Per-item disabled state by index                     | Not supported                                          |
| `styleVariant`         | Appearance style: solid or outline                   | Styled via the trigger Button variant                  |
| `icon`                 | Prefix icon identifier                               | Set via the trigger Button icon                        |
| `iconPosition`         | Icon placement relative to text                      | Not supported                                          |
| `horizontalAlign`      | Content alignment (left, center, right, stretch)     | `.Align(AlignOptions)`                                 |
| `overlayMaxHeight`     | Maximum dropdown list height in pixels               | `.Height(Size)`                                        |
| `overlayMinWidth`      | Minimum dropdown list width in pixels                | `.Width(Size)`                                         |
| `tooltipText`          | Markdown-formatted tooltip on hover                  | Not supported                                          |
| `ariaLabel`            | Accessible label for screen readers                  | Not supported                                          |
| `isHiddenOnDesktop`    | Hide on desktop layout                               | Not supported                                          |
| `isHiddenOnMobile`     | Hide on mobile layout                                | Not supported                                          |
| `maintainSpaceWhenHidden` | Reserve space when hidden                         | Not supported                                          |
| `margin`               | External spacing (default: "4px 8px")                | Not supported                                          |
| `showInEditor`         | Visibility in editor mode                            | Not supported                                          |
| `style`                | Custom styling object                                | Not supported                                          |
| Click event            | Fired when a menu item is clicked                    | `onSelect` callback                                    |
| `scrollIntoView()`     | Scroll component into viewport                       | Not supported                                          |
| `setDisabled()`        | Programmatically toggle disabled state               | Not supported                                          |
| `setHidden()`          | Programmatically toggle visibility                   | `.Visible` property                                    |
| N/A                    | N/A                                                  | `.Side(SideOptions)` - menu position (top/bottom/left/right) |
| N/A                    | N/A                                                  | `MenuItem.Checkbox()` - toggle items                   |
| N/A                    | N/A                                                  | `MenuItem.Separator()` - visual dividers               |
| N/A                    | N/A                                                  | `.Children()` - nested submenus                        |
