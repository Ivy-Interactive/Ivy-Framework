# Collapsible Container

A container that groups other components together with a preconfigured collapsible body area and toggle button. It is a preset version of the Container component.

## Retool

```toolscript
CollapsibleContainer {
  title: "My Section"
  collapsed: false
  showBody: true
  showHeader: true
  showBorder: true
  onToggle: handleToggle
}
```

## Ivy

```csharp
new Expandable("My Section",
    Layout.Vertical()
        | new Text("Content inside the collapsible area")
        | new Button("Action")
).Open()
```

## Parameters

| Parameter              | Documentation                                                                 | Ivy                                                    |
|------------------------|-------------------------------------------------------------------------------|--------------------------------------------------------|
| `collapsed`            | Whether the container body is collapsed. Default `false`.                     | `Open` property / `.Open()` fluent setter (inverted)   |
| `title`                | Title displayed on the toggle button. Default `"Collapsible Container"`.      | `header` constructor parameter                         |
| `showBody`             | Whether to show the body area.                                                | Controlled via `Open` property                         |
| `showHeader`           | Whether to show the header area.                                              | Not supported (header is always visible)               |
| `showFooter`           | Whether to show the footer area.                                              | Not supported                                          |
| `showBorder`           | Whether to show a border.                                                     | Not supported (styled via theme)                       |
| `showHeaderBorder`     | Whether to show a border under the header.                                    | Not supported                                          |
| `showFooterBorder`     | Whether to show a border above the footer.                                    | Not supported                                          |
| `disabled`             | Whether interaction is disabled.                                              | `Disabled` property / `.Disabled()` fluent setter      |
| `loading`              | Whether to display a loading indicator.                                       | Not supported                                          |
| `padding`              | The amount of padding inside the container.                                   | Not supported (styled via theme)                       |
| `headerPadding`        | The amount of padding inside the header.                                      | Not supported                                          |
| `footerPadding`        | The amount of padding inside the footer.                                      | Not supported                                          |
| `margin`               | The amount of margin outside the container.                                   | Not supported (styled via theme)                       |
| `enableFullBleed`      | Whether to expand contents to fill available space.                           | Not supported                                          |
| `hoistFetching`        | Whether to show a loading indicator when nested objects are fetching data.    | Not supported                                          |
| `isHiddenOnMobile`     | Whether to hide the component in the mobile layout.                           | Not supported                                          |
| `isHiddenOnDesktop`    | Whether to hide the component in the desktop layout.                          | Not supported                                          |
| `maintainSpaceWhenHidden` | Whether to occupy space on the canvas when hidden.                         | Not supported                                          |
| `tooltipText`          | Tooltip text displayed on hover.                                              | Not supported                                          |
| `style`                | Custom CSS style options.                                                     | Not supported                                          |
| `Visible`              | —                                                                             | `Visible` property (Ivy only)                          |
| `Width`                | —                                                                             | `Width` property (Ivy only)                            |
| `Height`               | —                                                                             | `Height` property (Ivy only)                           |
| `Scale`                | —                                                                             | `Scale` property (Ivy only)                            |
| **Events**             |                                                                               |                                                        |
| `onToggle`             | Triggered when the toggle button is clicked.                                  | Not supported                                          |
| `Change`               | Triggered when the value changes (inherited from Container).                  | Not supported                                          |
| `Click`                | Triggered when an item is clicked (inherited from Container).                 | Not supported                                          |
| **Methods**            |                                                                               |                                                        |
| `toggle()`             | Programmatically toggle the collapsed state.                                  | Set `Open` property programmatically                   |
| `setShowBody()`        | Toggle body visibility.                                                       | Set `Open` property programmatically                   |
| `setHidden()`          | Toggle component visibility.                                                  | `Visible` property                                     |
| `setDisabled()`        | Toggle disabled state.                                                        | `Disabled` property                                    |
| `focus()`              | Set focus on the component.                                                   | Not supported                                          |
| `scrollIntoView()`     | Scroll the canvas so the component is visible.                                | Not supported                                          |
