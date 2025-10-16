---
searchHints:
  - wrapper
  - container
  - grouping
  - fragment
  - virtual
  - invisible
---

# Fragment

<Ingress>
Group multiple elements without adding extra DOM markup, similar to React Fragments, for clean component composition.
</Ingress>

The `Fragment` widget is a container component that doesn't produce any HTML elements itself. It's useful for grouping multiple elements without adding extra markup to the DOM, similar to React Fragments. This makes it perfect for conditional rendering, returning multiple widgets from a view, and creating clean component compositions.

## Basic Usage

Fragment groups multiple widgets without adding DOM elements. Here's the simplest example:

```csharp demo-tabs
public class BasicFragmentView : ViewBase
{
    public override object? Build()
    {
        return new Fragment(
            Text.P("Welcome"),
            Text.P("This text is grouped with the heading above.")
        );
    }
}
```

### Conditional Rendering

Fragment is excellent for conditional rendering, allowing you to show or hide content based on state:

```csharp demo-tabs
public class ConditionalRenderingView : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var viewMode = UseState("user"); 
        
        return Layout.Vertical().Gap(4)
            | Text.P("User Dashboard")
            | (Layout.Horizontal().Gap(2)
                | new Button("User View", _ => {
                    viewMode.Set("user");
                    client.Toast("Switched to user view");
                  })
                    .Variant(viewMode.Value == "user" ? ButtonVariant.Primary : ButtonVariant.Secondary)
                | new Button("Admin View", _ => {
                    viewMode.Set("admin");
                    client.Toast("Switched to admin view");
                  })
                    .Variant(viewMode.Value == "admin" ? ButtonVariant.Primary : ButtonVariant.Secondary))
            | (viewMode.Value == "admin"
                ? new Fragment(
                    Text.P("Admin Controls"),
                    Layout.Horizontal().Gap(2)
                        | new Button("Reset System", _ => client.Toast("System reset initiated!"), variant: ButtonVariant.Destructive)
                        | new Button("View Logs", _ => client.Toast("Opening system logs..."))
                        | new Button("Manage Users", _ => client.Toast("User management panel opened"))
                  )
                : new Fragment(
                    Text.P("User Profile"),
                    Layout.Horizontal().Gap(2)
                        | new Button("Edit Profile", _ => client.Toast("Profile editor opened"))
                        | new Button("Change Password", _ => client.Toast("Password change dialog opened"))
                        | new Button("View Settings", _ => client.Toast("User settings displayed"))
                  ));
    }
}
```

<Callout Type="tip">
Fragment excels at conditional rendering. You can use it to show different content based on state, user roles, or any other conditions while keeping your code clean and readable.
</Callout>

### Multiple Return Elements

Fragment allows you to return multiple widgets from a single view method:

```csharp demo-tabs
public class MultipleElementsView : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var selectedTab = UseState(0);
        var tabs = new[] { "Overview", "Details", "Settings" };
        
        return new Fragment(
            Text.P("Application"),
            new Spacer().Height(4),
            // Tab navigation
            Layout.Horizontal().Gap(2)
                | tabs.Select((tab, index) => 
                    new Button(tab, _ => {
                        selectedTab.Set(index);
                        client.Toast($"Switched to {tab} tab");
                      })
                        .Variant(selectedTab.Value == index ? ButtonVariant.Primary : ButtonVariant.Secondary)
                ),
            new Spacer().Height(4),
            // Content area
            new Card(
                selectedTab.Value == 0 ? Text.P("Overview content here...") :
                selectedTab.Value == 1 ? Text.P("Details content here...") :
                Text.P("Settings content here...")
            ).Title("Content"),
            new Spacer().Height(4),
            // Header section
            Layout.Horizontal().Gap(4)
                | new Button("Save", _ => client.Toast("Changes saved successfully!"))
                | new Button("Cancel", _ => client.Toast("Changes cancelled"))
        );
    }
}
```

<WidgetDocs Type="Ivy.Fragment" ExtensionTypes="Ivy.FragmentExtensions"  SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Primitives/Fragment.cs"/>

## Examples

<Details>
<Summary>
Dynamic Content Generation
</Summary>
<Body>
Fragment can be used to dynamically generate content based on data:

```csharp demo-tabs
public class DynamicContentView : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var items = UseState(() => new string[] { "Item 1", "Item 2" });
        var showDetails = UseState(false);
        
        return Layout.Vertical().Gap(4)
            | Text.P("Dynamic Content")
            | new Fragment(
                // Static controls
                Layout.Horizontal().Gap(2)
                    | new Button("Add Item", _ => {
                        var newItems = items.Value.Append($"Item {items.Value.Length + 1}").ToArray();
                        items.Set(newItems);
                        client.Toast($"Added Item {newItems.Length}");
                      })
                    | new Button("Toggle Details", _ => {
                        showDetails.Set(!showDetails.Value);
                        client.Toast(!showDetails.Value ? "Details hidden" : "Details shown");
                      })
                    | new Spacer().Width(Size.Grow())
                    | new Button("Reset", _ => {
                        items.Set(new string[] { "Item 1", "Item 2" });
                        client.Toast("Table reset to default");
                      }),
                
                // Dynamic list
                Layout.Vertical().Gap(2)
                    | items.Value.Select(item => 
                        new Card(
                            Layout.Vertical().Gap(2)
                                | Text.H3(item)
                                | Text.Small("Active").Color(Colors.Green)
                                | (showDetails.Value ? Text.Small($"Details for {item}") : Text.Small("Click 'Toggle Details' to see more"))
                        ).Title("List Item")
                    )
              );
    }
}
```

</Body>
</Details>