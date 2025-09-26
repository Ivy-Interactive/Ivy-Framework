---
prepare: |
  var client = this.UseService<IClientProvider>();
---

# DropDownMenu

<Ingress>
Create interactive dropdown menus with customizable options, actions, and styling for navigation and user choices. DropDownMenu provides a flexible way to display hierarchical menus with various item types including checkboxes, radio buttons, separators, and nested submenus.
</Ingress>

## Basic Usage

Here's a simple example of a `DropDownMenu` that shows a toast message when an item is selected:

```csharp demo-below
new DropDownMenu(@evt => client.Toast("Selected: " + @evt.Value), 
    new Button("Basic Menu"),
    MenuItem.Default("Profile"), 
    MenuItem.Default("Settings"), 
    MenuItem.Default("Logout"))
```

### Default Menu Items

Default menu items are the most common type, providing simple clickable options. The second example shows how to add custom tags for more advanced event handling.

```csharp demo-tabs
Layout.Horizontal().Gap(2).Center()
    | new DropDownMenu(@evt => client.Toast("Selected: " + @evt.Value), 
        new Button("Default Items"),
        MenuItem.Default("Copy"),
        MenuItem.Default("Paste"),
        MenuItem.Default("Cut"),
        MenuItem.Default("Delete"))
    | new DropDownMenu(@evt => client.Toast("Selected: " + @evt.Value), 
        new Button("With Tags"),
        MenuItem.Default("Save").Tag("save-action"),
        MenuItem.Default("Export").Tag("export-action"),
        MenuItem.Default("Import").Tag("import-action"))
```

Default menu items are the most common type, providing simple clickable options. The second example shows how to add custom tags for more advanced event handling.

### Checkbox Menu Items

Checkbox menu items allow users to toggle options on/off. The second example demonstrates mixing different menu item types for more complex interfaces.

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

Separators help organize menu items into logical groups, making the interface more readable and user-friendly.

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

## Positioning Options

### Side Positioning

Control which side of the trigger the menu appears on:

```csharp demo-tabs
Layout.Vertical().Gap(2)
    | Text.H4("Side Positioning")
    | Text.P("Control which side of the trigger button the menu appears on:")
    | (Layout.Horizontal().Gap(2).Center()
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
            .Left())
```

## Advanced Features

### Headers

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

### Button Integration

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

Custom event handling allows you to implement complex business logic based on menu selections, making your dropdowns more interactive and useful.

```csharp demo-tabs
new DropDownMenu(@evt => {
        var value = @evt.Value?.ToString();
        if (value == "delete") {
            client.Toast("Deleting item...");
        } else if (value == "export") {
            client.Toast("Exporting data...");
        } else {
            client.Toast($"Selected: {value}");
        }
    }, 
    new Button("Custom Actions"))
    | MenuItem.Default("View").Tag("view")
    | MenuItem.Default("Edit").Tag("edit")
    | MenuItem.Default("Delete").Tag("delete")
    | MenuItem.Separator()
    | MenuItem.Default("Export").Tag("export")
    | MenuItem.Default("Share").Tag("share")
```

<WidgetDocs Type="Ivy.DropDownMenu" ExtensionTypes="Ivy.DropDownMenuExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/DropDownMenu.cs"/>

## Examples

<Details>
<Summary>
Complex using
</Summary>
<Body>

Here's a comprehensive example combining multiple features:

```csharp demo-tabs
Layout.Horizontal().Gap(2).Center()
    | new DropDownMenu(@evt => client.Toast("Selected: " + @evt.Value), 
        new Button("User Menu"),
        MenuItem.Separator(),
        MenuItem.Default("View Profile").Tag("profile"),
        MenuItem.Default("Account Settings").Tag("settings"),
        MenuItem.Default("Preferences").Tag("preferences"),
        MenuItem.Separator(),
        MenuItem.Default("Theme")
            .Children(
                MenuItem.Checkbox("Light").Tag("theme-light"),
                MenuItem.Checkbox("Dark").Checked().Tag("theme-dark"),
                MenuItem.Checkbox("System").Tag("theme-system")
            ),
        MenuItem.Default("Notifications")
            .Children(
                MenuItem.Checkbox("Email").Checked().Tag("notify-email"),
                MenuItem.Checkbox("Push").Checked().Tag("notify-push"),
                MenuItem.Checkbox("SMS").Tag("notify-sms")
            ),
        MenuItem.Separator(),
        MenuItem.Default("Help & Support").Tag("help"),
        MenuItem.Default("About").Tag("about"),
        MenuItem.Separator(),
        MenuItem.Default("Logout").Tag("logout"))
        .Header(Text.Muted("Signed in as john.doe@company.com"))
        .Top()
        .Align(DropDownMenu.AlignOptions.End)
    | new DropDownMenu(@evt => client.Toast("Selected: " + @evt.Value), 
        new Button("Settings Menu"),
        MenuItem.Default("General").Tag("general"),
        MenuItem.Default("Appearance").Tag("appearance"),
        MenuItem.Default("Privacy").Tag("privacy"),
        MenuItem.Default("Security").Tag("security"),
        MenuItem.Separator(),
        MenuItem.Default("Updates").Tag("updates"),
        MenuItem.Default("Support").Tag("support"))
        .Header(Text.Muted("Application Settings"))
```

</Body>
</Details>
