# Link List

A group of links to trigger actions when clicked. Displays a list of labeled, clickable links with optional icons, captions, tooltips, and configurable layout (single column, multi-column, or wrap). Commonly used for navigation menus and action lists.

## Retool

```toolscript
// Basic link list with labels and click handler
linkList1.labels = ["Dashboard", "Settings", "Profile"];
linkList1.captionByIndex = ["Main overview", "Configuration", "Your account"];
linkList1.iconByIndex = [
  "/icon:bold/interface-home",
  "/icon:bold/interface-setting-cog",
  "/icon:bold/interface-user-single"
];
linkList1.groupLayout = "singleColumn";
linkList1.showUnderline = "hover";

// Disable/enable programmatically
linkList1.setDisabled(true);
linkList1.setHidden(false);

// Event handler: Click
// Configured in Inspector > Interaction > Event handlers
```

## Ivy

```csharp
public class LinkListDemo : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();

        var onItemClick = new Action<Event<ListItem>>(e =>
        {
            client.Toast($"Navigated to: {e.Sender.Title}", "Navigation");
        });

        return new List(new[]
        {
            new ListItem("Dashboard", icon: Icons.House, subtitle: "Main overview", onClick: onItemClick),
            new ListItem("Settings", icon: Icons.Settings, subtitle: "Configuration", onClick: onItemClick),
            new ListItem("Profile", icon: Icons.User, subtitle: "Your account", onClick: onItemClick)
        });
    }
}
```

## Parameters

| Parameter          | Documentation                                                             | Ivy                                                        |
| ------------------ | ------------------------------------------------------------------------- | ---------------------------------------------------------- |
| `labels`           | A list of labels for each item                                            | `ListItem(title: "...")` — first constructor argument       |
| `captionByIndex`   | A list of captions for each item, by index                                | `ListItem(subtitle: "...")`                                |
| `iconByIndex`      | A list of icons for each item, by index                                   | `ListItem(icon: Icons.X)`                                  |
| `tooltipByIndex`   | A list of tooltips for each item, by index                                | Not supported                                              |
| `tooltipText`      | Tooltip text next to the label on hover                                   | Not supported                                              |
| `disabled`         | Whether interaction is disabled                                           | Not supported (no direct disable on List)                  |
| `disabledByIndex`  | Per-item disabled state, by index                                         | Not supported                                              |
| `hiddenByIndex`    | Per-item hidden state, by index                                           | Not supported                                              |
| `clickable`        | Whether there is an enabled Click event handler                           | `ListItem(onClick: handler)`                               |
| `events` (Click)   | An item is clicked or pressed                                             | `ListItem(onClick: Action<Event<ListItem>>)`               |
| `groupLayout`      | Layout of items: `auto`, `singleColumn`, `multiColumn`, `wrap`            | Not supported (always vertical)                            |
| `minColumnWidth`   | Minimum column width in multi-column layout                               | Not supported                                              |
| `showUnderline`    | Underline style: `always`, `hover`, `never`                               | Not supported                                              |
| `underlineStyle`   | Underline line style: `solid`, `dashed`, `dotted`                         | Not supported                                              |
| `labelPosition`    | Position of the label: `top` or `left`                                    | Not supported                                              |
| `margin`           | Margin around the component                                               | Not supported (use `Layout` gap/spacing)                   |
| `style`            | Custom style options object                                               | Not supported                                              |
| `isHiddenOnMobile` | Whether hidden in mobile layout                                           | `Visible` property (not platform-specific)                 |
| `isHiddenOnDesktop`| Whether hidden in desktop layout                                          | `Visible` property (not platform-specific)                 |
| `maintainSpaceWhenHidden` | Whether to reserve space when hidden                               | Not supported                                              |
| `showInEditor`     | Whether visible in editor when hidden                                     | Not supported (no editor concept)                          |
| `id`               | Unique identifier/name                                                    | Not applicable (C# object reference)                       |
| `scrollIntoView()` | Scroll the canvas to bring the component into view                        | Not supported                                              |
| `setDisabled()`    | Programmatically toggle disabled state                                    | Not supported                                              |
| `setHidden()`      | Programmatically toggle visibility                                        | `Visible` property                                         |
| `Height`           | —                                                                         | `Height` property on `List`                                |
| `Width`            | —                                                                         | `Width` property on `List`                                 |
| `Scale`            | —                                                                         | `Scale` property on `List`                                 |
| `badge`            | —                                                                         | `ListItem(badge: "3")` — Ivy-only feature                  |
