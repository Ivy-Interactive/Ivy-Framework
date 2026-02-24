# Ivy Framework Weekly Notes - Week of 2026-02-24

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

This update brings a new Tree widget, some minor API changes to existing widgets and security updates. We also have improved the LLM's observability and the Ivy Docs project based on feedback from the users.

## Breaking Changes

### Code Widget Renamed to CodeBlock

The `Code` widget has been renamed to `CodeBlock` to improve clarity and reduce ambiguity. This change affects all code that uses the Code widget for displaying syntax-highlighted code snippets.

**Migration:**

```csharp
// Before
new Code("var x = 1;", Languages.Csharp)
    .ShowCopyButton()
    .ShowLineNumbers()

// After
new CodeBlock("var x = 1;", Languages.Csharp)
    .ShowCopyButton()
    .ShowLineNumbers()
```

## New Features

### Tree Widget - Display Hierarchical Data

A new Tree widget is now available for displaying hierarchical data structures like file trees, folder systems, nested categories, and organizational chart.

**Basic Example:**

```csharp
var selectedFile = UseState("");

new Tree(
    new MenuItem("src")
        .Icon(Icons.Folder)
        .Expanded()
        .Children(
            new MenuItem("App.tsx").Icon(Icons.Code).Tag("App.tsx"),
            new MenuItem("index.ts").Icon(Icons.Code).Tag("index.ts")
        )
).HandleSelect(e => selectedFile.Set(e.Value?.ToString() ?? ""))
```

The Tree widget uses MenuItem for each node, supporting all the features you'd expect like icons, nested children with `.Children()`, expand/collapse with `.Expanded()`, disabled items with `.Disabled()`, and custom click handling through `.Tag()` and `.HandleSelect()`. It also includes full keyboard navigation support (Arrow keys to expand/collapse, Enter/Space to select).

## Improvements

### SelectInput - Simplified String Options API

SelectInput now supports passing string collections directly without needing to call `.ToOptions()` first. You can also use the new `.Options()` fluent method for cleaner, more readable code.

**Example:**

```csharp
// Direct string collection in ToSelectInput
var defaultBehavior = UseState("Allowed");
defaultBehavior.ToSelectInput(["Refused", "Allowed", "Ignored"])

// Or use the fluent Options() method
var notificationTypes = UseState<string[]>([]);
notificationTypes
    .ToSelectInput()
    .Options(["Email", "SMS", "Push", "In-App"])
    .Variant(SelectInputs.List)
    .Placeholder("Select notification types...")
```

### Card Widget - Fluent API for Content and Footer

The Card widget now supports setting content and footer using fluent extension methods, matching the existing `.Header()` API style.

**Example:**

```csharp
// Build a card using the fluent API
new Card()
    .Header(Text.H4("Dashboard Stats"), icon: Icons.Activity.ToIcon())
    .Content(Layout.Vertical().Gap(2)
        | Text.P("Your application metrics")
        | Text.Strong("Active Users: 1,247")
    )
    .Footer(new Button("View Details", _ => ShowDetails()))
```

### ListItem - Embed Custom Widgets with Content()

ListItem now supports embedding child widgets directly within list rows using the new `.Content()` extension method.

**Example:**

```csharp
var notifications = UseState(false);

new List(new[]
{
    // Add a switch control
    new ListItem("Notifications", icon: Icons.Bell)
        .Content(new BoolInput(notifications, variant: BoolInputs.Switch)),

    // Add multiple widgets with a layout
    new ListItem("Status", icon: Icons.Activity)
        .Content(
            Layout.Horizontal().Gap(2)
                | new Badge("Online", BadgeVariant.Success)
                | Text.Muted("Last seen 2 min ago")
        ),

    // Add an inline text input
    new ListItem("Search", icon: Icons.Search)
        .Content(new TextInput("", placeholder: "Type to search..."))
})
```

### DataTable - Automatic Data Refresh

DataTables now automatically refresh when the underlying data source changes. Previously, you had to manually trigger refreshes when adding, removing, or modifying data. Now the DataTable detects changes to the `IQueryable` and reloads automatically.
