---
prepare: |
  var client = UseService<IClientProvider>();
searchHints:
  - menu
  - dropdown
  - context
  - options
  - actions
  - popup
---

# DropDownMenu

<Ingress>
Create interactive dropdown menus with customizable options, actions, and styling for [navigation](../../01_Onboarding/02_Concepts/09_Navigation.md) and user choices. DropDownMenu provides a flexible way to display hierarchical menus with various item types including checkboxes, radio buttons, separators, and nested submenus.
</Ingress>

## Basic Usage

Here's a simple example of a `DropDownMenu` that shows a [toast message](../../01_Onboarding/02_Concepts/13_Clients.md) when an item is selected:

```csharp demo-below
new DropDownMenu(@evt => client.Toast("Selected: " + @evt.Value), 
    new Button("Basic Menu"),
    MenuItem.Default("Profile"), 
    MenuItem.Default("Settings"), 
    MenuItem.Default("Logout"))
```

### Default Menu Items

Default menu items are the most common type, providing simple clickable options. The second example shows how to add custom tags for more advanced [event handling](../../01_Onboarding/02_Concepts/07_EventHandlers.md).

```csharp demo-tabs
public class DefaultMenuItemsDemo : ViewBase
{
    private enum MenuAction { Save, Export, Import }

    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        return Layout.Horizontal().Gap(2).Center()
            | new DropDownMenu(@evt => client.Toast("Selected: " + @evt.Value), 
                new Button("Default Items"),
                MenuItem.Default("Copy"),
                MenuItem.Default("Paste"),
                MenuItem.Default("Cut"),
                MenuItem.Default("Delete"))
            | new DropDownMenu(@evt => client.Toast("Selected: " + @evt.Value), 
                new Button("With Tags"),
                MenuItem.Default("Save").Tag(MenuAction.Save),
                MenuItem.Default("Export").Tag(MenuAction.Export),
                MenuItem.Default("Import").Tag(MenuAction.Import));
    }
}
```

Default menu items are the most common type, providing simple clickable options. The second example shows how to add custom tags for more advanced event handling.

### Checkbox Menu Items

Checkbox menu items allow users to toggle options on/off. The second example demonstrates mixing different menu item types for more complex [interfaces](../../01_Onboarding/02_Concepts/02_Views.md).

```csharp demo-tabs
Layout.Horizontal().Gap(2).Center()
    | new DropDownMenu(@evt => client.Toast("Selected: " + @evt.Value), 
        new Button("Checkboxes"),
        MenuItem.Checkbox("Dark Theme").Checked(),
        MenuItem.Checkbox("Notifications"),
        MenuItem.Checkbox("Auto-save").Checked(),
        MenuItem.Checkbox("Debug Mode"))
    | new DropDownMenu(@evt => client.Toast("Selected: " + @evt.Value), 
        new Button("Mixed Types"),
        MenuItem.Default("Profile"),
        MenuItem.Separator(),
        MenuItem.Checkbox("Email Notifications").Checked(),
        MenuItem.Checkbox("SMS Notifications"),
        MenuItem.Checkbox("Push Notifications").Checked())
```

### Separators

Separators help organize menu items into logical groups, making the [interface](../../01_Onboarding/02_Concepts/02_Views.md) more readable and user-friendly.

```csharp demo-tabs
new DropDownMenu(@evt => client.Toast("Selected: " + @evt.Value), 
    new Button("With Separators"))
    | MenuItem.Default("New File")
    | MenuItem.Default("Open File")
    | MenuItem.Separator()
    | MenuItem.Default("Save")
    | MenuItem.Default("Save As")
    | MenuItem.Separator()
    | MenuItem.Default("Print")
    | MenuItem.Default("Export")
```

### Nested Menu Items

Nested menu items create submenus for better organization of complex menu structures.

```csharp demo-tabs
new DropDownMenu(@evt => client.Toast("Selected: " + @evt.Value), 
    new Button("Nested Items"))
    | MenuItem.Default("File")
        .Children(
            MenuItem.Default("New"),
            MenuItem.Default("Open"),
            MenuItem.Default("Save")
        )
    | MenuItem.Default("Edit")
        .Children(
            MenuItem.Default("Undo"),
            MenuItem.Default("Redo"),
            MenuItem.Default("Cut")
        )
```

## Semantic Colors

You can apply semantic colors to menu items to provide visual cues about their action or status. These colors align with the standard Ivy design system:

```csharp demo-tabs
new DropDownMenu(@evt => client.Toast("Selected: " + @evt.Value), 
    new Button("Semantic Colors"),
    MenuItem.Default("Primary").Primary(),
    MenuItem.Default("Secondary").Secondary(),
    MenuItem.Default("Success").Success(),
    MenuItem.Default("Warning").Warning(),
    MenuItem.Default("Info").Info(),
    MenuItem.Separator(),
    MenuItem.Default("Delete").Destructive())
```

## Positioning

Control which side of the trigger the menu appears on:

```csharp demo-tabs
Layout.Horizontal().Gap(2).Center()
    | new DropDownMenu(@evt => client.Toast("Selected: " + @evt.Value), 
        new Button("Top"), MenuItem.Default("Item 1"), MenuItem.Default("Item 2"))
        .Top()
    | new DropDownMenu(@evt => client.Toast("Selected: " + @evt.Value), 
        new Button("Right"), MenuItem.Default("Item 1"), MenuItem.Default("Item 2"))
        .Right()
    | new DropDownMenu(@evt => client.Toast("Selected: " + @evt.Value), 
        new Button("Bottom"), MenuItem.Default("Item 1"), MenuItem.Default("Item 2"))
        .Bottom()
    | new DropDownMenu(@evt => client.Toast("Selected: " + @evt.Value), 
        new Button("Left"), MenuItem.Default("Item 1"), MenuItem.Default("Item 2"))
        .Left()
```

## Advanced Features

### Menu Headers

Headers provide context and user information, making menus more informative and professional-looking.

```csharp demo-tabs
Layout.Horizontal().Gap(2).Center()
    | new DropDownMenu(@evt => client.Toast("Selected: " + @evt.Value), 
        new Button("Muted Header"),
        MenuItem.Separator(),
        MenuItem.Default("Profile"),
        MenuItem.Default("Settings"),
        MenuItem.Default("Logout"))
        .Header(Text.Muted("Signed in as user@example.com"))
    | new DropDownMenu(@evt => client.Toast("Selected: " + @evt.Value), 
        new Button("Label Header"),
        MenuItem.Separator(),
        MenuItem.Default("View Profile"),
        MenuItem.Default("Account Settings"))
        .Header(Text.Label("John Doe - Administrator"))
```

### Fluent Syntax

The `WithDropDown` extension method provides a clean, fluent API for quickly adding dropdown functionality to existing buttons.

```csharp demo-tabs
Layout.Horizontal().Gap(2).Center()
    | new Button("Quick Menu")
        .WithDropDown(
            MenuItem.Default("Option 1"),
            MenuItem.Default("Option 2"),
            MenuItem.Default("Option 3")
        )
    | new Button("Settings")
        .Secondary()
        .WithDropDown(
            MenuItem.Default("Preferences"),
            MenuItem.Default("Account"),
            MenuItem.Default("Help")
        )
```

### Custom Event Handling

Custom event handling allows you to implement complex [business logic](../../03_Hooks/02_Core/11_UseService.md) based on menu selections, making your dropdowns more interactive and useful.

```csharp demo-tabs
public class CustomActionsDropDownDemo : ViewBase
{
    private enum ItemAction { View, Edit, Delete, Export, Share }

    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        return new DropDownMenu(@evt => {
            if (Enum.TryParse<ItemAction>(@evt.Value?.ToString(), ignoreCase: true, out var action))
            {
                switch (action)
                {
                    case ItemAction.Delete: client.Toast("Deleting item..."); break;
                    case ItemAction.Export: client.Toast("Exporting data..."); break;
                    default: client.Toast($"Selected: {action}"); break;
                }
            }
        }, 
        new Button("Custom Actions"))
            | MenuItem.Default("View").Tag(ItemAction.View)
            | MenuItem.Default("Edit").Tag(ItemAction.Edit)
            | MenuItem.Default("Delete").Tag(ItemAction.Delete)
            | MenuItem.Separator()
            | MenuItem.Default("Export").Tag(ItemAction.Export)
            | MenuItem.Default("Share").Tag(ItemAction.Share);
    }
}
```

<WidgetDocs Type="Ivy.DropDownMenu" ExtensionTypes="Ivy.DropDownMenuExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/DropDownMenu.cs"/>

## Examples

<Details>
<Summary>
Complex using
</Summary>
<Body>

Here's a comprehensive example combining multiple features:

```csharp demo-tabs
public class ComplexDropDownDemo : ViewBase
{
    private enum UserMenuAction { Profile, Settings, Preferences, ThemeLight, ThemeDark, ThemeSystem, NotifyEmail, NotifyPush, NotifySms, Help, About, Logout }
    private enum SettingsMenuAction { General, Appearance, Privacy, Security, Updates, Support }

    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        return Layout.Horizontal().Gap(2).Center()
            | new DropDownMenu(@evt => client.Toast("Selected: " + @evt.Value), 
                new Button("User Menu"),
                MenuItem.Separator(),
                MenuItem.Default("View Profile").Tag(UserMenuAction.Profile),
                MenuItem.Default("Account Settings").Tag(UserMenuAction.Settings),
                MenuItem.Default("Preferences").Tag(UserMenuAction.Preferences),
                MenuItem.Separator(),
                MenuItem.Default("Theme")
                    .Children(
                        MenuItem.Checkbox("Light").Tag(UserMenuAction.ThemeLight),
                        MenuItem.Checkbox("Dark").Checked().Tag(UserMenuAction.ThemeDark),
                        MenuItem.Checkbox("System").Tag(UserMenuAction.ThemeSystem)
                    ),
                MenuItem.Default("Notifications")
                    .Children(
                        MenuItem.Checkbox("Email").Checked().Tag(UserMenuAction.NotifyEmail),
                        MenuItem.Checkbox("Push").Checked().Tag(UserMenuAction.NotifyPush),
                        MenuItem.Checkbox("SMS").Tag(UserMenuAction.NotifySms)
                    ),
                MenuItem.Separator(),
                MenuItem.Default("Help & Support").Tag(UserMenuAction.Help),
                MenuItem.Default("About").Tag(UserMenuAction.About),
                MenuItem.Separator(),
                MenuItem.Default("Logout").Tag(UserMenuAction.Logout))
                .Header(Text.Muted("Signed in as john.doe@company.com"))
                .Top()
                .Align(DropDownMenu.AlignOptions.End)
            | new DropDownMenu(@evt => client.Toast("Selected: " + @evt.Value), 
                new Button("Settings Menu"),
                MenuItem.Default("General").Tag(SettingsMenuAction.General),
                MenuItem.Default("Appearance").Tag(SettingsMenuAction.Appearance),
                MenuItem.Default("Privacy").Tag(SettingsMenuAction.Privacy),
                MenuItem.Default("Security").Tag(SettingsMenuAction.Security),
                MenuItem.Separator(),
                MenuItem.Default("Updates").Tag(SettingsMenuAction.Updates),
                MenuItem.Default("Support").Tag(SettingsMenuAction.Support))
                .Header(Text.Muted("Application Settings"));
    }
}
```

</Body>
</Details>
