# Sheet

<Ingress>
Sheets slide in from the side of the screen and display additional content while allowing the user to dismiss them. They provide a non-intrusive way to show additional information or forms without navigating away from the current page.
</Ingress>

## Basic Usage

The `WithSheet` extension on a `Button` provides an easy way to open a sheet:

```csharp demo-tabs
public class BasicSheetExample : ViewBase
{
    public override object? Build()
    {
        return new Button("Open Sheet").WithSheet(
            () => new SheetView(),
            title: "This is a sheet",
            description: "Lorem ipsum dolor sit amet",
            width: Size.Fraction(1/2f)
        );
    }
}

public class SheetView : ViewBase
{
    public override object? Build()
    {
        return new Card(
            "Welcome to the sheet!",
            "This is the content inside the sheet"
        );
    }
}
```

### Custom Content

The following demonstrates how to create a sheet with custom content using a Fragment and Card. The sheet opens with a title, description, and custom width, showing how to structure content within sheets.

```csharp demo-tabs
public class BasicSheetWithContent : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        return new Button("Open Basic Sheet").WithSheet(
            () => new Fragment(
                new Card(
                    "Welcome to the sheet!",
                    new Button("Action Button", onClick: _ => client.Toast("Button clicked!"))
                ).Title("Sheet Content").Description("This is a simple sheet with custom content")
            ),
            title: "Basic Sheet",
            description: "A simple example of sheet usage",
            width: Size.Fraction(1/3f)
        );
    }
}
```

### Footer Actions

You can also create a sheet with action buttons in the footer using FooterLayout.

```csharp demo-tabs
public class SheetWithFooterActions : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        return new Button("Open Sheet with Actions").WithSheet(
            () => new FooterLayout(
                Layout.Horizontal().Gap(2)
                    | new Button("Save").Variant(ButtonVariant.Primary).HandleClick(_ => client.Toast("Profile saved successfully!"))
                    | new Button("Cancel").Variant(ButtonVariant.Outline).HandleClick(_ => client.Toast("Changes cancelled")),
                new Card(
                    "This sheet has action buttons in the footer"
                ).Title("Content")
            ),
            title: "Actions Sheet",
            width: Size.Fraction(1/2f)
        );
    }
}
```

### Complex Layout

This example shows how to organize complex content within sheets using nested layouts and various input controls.

```csharp demo-tabs
public class ComplexSheetLayout : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        return new Button("Open Complex Sheet").WithSheet(
            () => Layout.Vertical()
                | new Card(
                    Layout.Horizontal()
                        | new Avatar("JD").Size(64)
                        | Layout.Vertical()
                            | Text.Small("John Doe").NoWrap()
                            | Text.Small("john.doe@example.com")
                ).Title("User Information")
                | new Card(
                    Layout.Vertical()
                        | new BoolInput("Dark Mode", true)
                        | new BoolInput("Notifications", false)
                        | new SelectInput<string>(options: new[] { "English", "Spanish", "French" }.ToOptions())
                ).Title("Preferences")
                | new Card(
                    Layout.Horizontal().Gap(2)
                        | new Button("Update Profile").HandleClick(_ => client.Toast("Profile updated!"))
                        | new Button("Change Password").HandleClick(_ => client.Toast("Password change initiated"))
                        | new Button("Delete Account").Variant(ButtonVariant.Destructive).HandleClick(_ => client.Toast("Account deletion requested"))
                ).Title("Actions"),
            title: "User Profile",
            description: "Manage your account settings and preferences",
            width: Size.Fraction(2/3f)
        );
    }
}
```

### Different Widths

The following demonstrates different sheet width options, from small to full-screen layouts.

```csharp demo-tabs
public class SheetWidthExamples : ViewBase
{
    public override object? Build()
    {
        return Layout.Horizontal().Gap(2)
            | new Button("Small Sheet").WithSheet(
                () => new Card("This is a small sheet").Title("Small Content"),
                title: "Small",
                width: Size.Rem(20)
            )
            | new Button("Medium Sheet").WithSheet(
                () => new Card("This is a medium sheet").Title("Medium Content"),
                title: "Medium",
                width: Size.Fraction(1/2f)
            )
            | new Button("Large Sheet").WithSheet(
                () => new Card("This is a large sheet").Title("Large Content"),
                title: "Large",
                width: Size.Fraction(2/3f)
            )
            | new Button("Full Sheet").WithSheet(
                () => new Card("This is a full-width sheet").Title("Full Content"),
                title: "Full Width",
                width: Size.Full()
            );
    }
}
```

### Sheet with Navigation

This example shows a sheet with internal navigation between multiple pages using state management.

```csharp demo-tabs
public class NavigationSheet : ViewBase
{
    public override object? Build()
    {
        return new Button("Open Navigation Sheet").WithSheet(
            () => new NavigationSheetContent(),
            title: "Navigation Sheet",
            width: Size.Fraction(1/2f)
        );
    }
}

public class NavigationSheetContent : ViewBase
{
    public override object? Build()
    {
        var currentPage = UseState<int>(0);
        var pages = new[] { "Home", "Profile", "Settings", "Help" };
        
        return Layout.Vertical()
            | Layout.Horizontal().Gap(2)
                | pages.Select((page, index) => 
                    new Button(page)
                        .Variant(currentPage.Value == index ? ButtonVariant.Primary : ButtonVariant.Outline)
                        .HandleClick(_ => currentPage.Value = index)
                ).ToArray()
            | new Card(
                $"This is the {pages[currentPage.Value]} page content"
            ).Title("Page Content");
    }
}
```

<WidgetDocs Type="Ivy.Sheet" ExtensionTypes="Ivy.SheetExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Sheet.cs"/>

## Advanced example

### Conditional Rendering

The following demonstrates how to conditionally render different content within a sheet based on state or user actions.

```csharp demo-tabs
public class ConditionalSheetExample : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var isOpen = UseState<bool>(false);
        var viewMode = UseState<string>("list"); // "list", "grid", "details"
        
        object RenderContent()
        {
            return viewMode.Value switch
            {
                "list" => new Card(
                    Layout.Vertical().Gap(1)
                        | "Item 1"
                        | "Item 2"
                        | "Item 3"
                ).Title("List View"),
                
                "grid" => new Card(
                    Layout.Horizontal().Gap(2)
                        | new Card("Item 1").Width(Size.Fraction(1/3f))
                        | new Card("Item 2").Width(Size.Fraction(1/3f))
                        | new Card("Item 3").Width(Size.Fraction(1/3f))
                ).Title("Grid View"),
                
                "details" => new Card(
                    Layout.Vertical().Gap(2)
                        | Text.H3("Detailed Information")
                        | Text.Small("This is a detailed view with more information about the selected item.")
                        | new Button("Action").Variant(ButtonVariant.Primary).HandleClick(_ => client.Toast("Action performed on detailed item!"))
                ).Title("Details View"),
                
                _ => new Card("Unknown view mode").Title("Error")
            };
        }
        
        return Layout.Vertical().Gap(2)
            | new Button("Open Conditional Sheet").HandleClick(_ => isOpen.Value = true)
            | (isOpen.Value ? new Sheet((Event<Sheet> _) => isOpen.Value = false,
                Layout.Vertical().Gap(2)
                    | Layout.Horizontal().Gap(2)
                        | new Button("List").Variant(viewMode.Value == "list" ? ButtonVariant.Primary : ButtonVariant.Outline)
                            .HandleClick(_ => {
                                viewMode.Value = "list";
                                client.Toast("Switched to List view");
                            })
                        | new Button("Grid").Variant(viewMode.Value == "grid" ? ButtonVariant.Primary : ButtonVariant.Outline)
                            .HandleClick(_ => {
                                viewMode.Value = "grid";
                                client.Toast("Switched to Grid view");
                            })
                        | new Button("Details").Variant(viewMode.Value == "details" ? ButtonVariant.Primary : ButtonVariant.Outline)
                            .HandleClick(_ => {
                                viewMode.Value = "details";
                                client.Toast("Switched to Details view");
                            })
                    | RenderContent(),
                title: "Conditional Content Sheet",
                description: "Switch between different view modes"
            ).Width(Size.Fraction(2/3f)) : null);
    }
}
```
