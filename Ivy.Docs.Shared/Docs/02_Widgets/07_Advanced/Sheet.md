---
searchHints:
  - sidebar
  - drawer
  - panel
  - slide-out
  - modal
  - overlay
---

# Sheet

<Ingress>
Sheets slide in from the side of the screen and display additional content while allowing the user to dismiss them. They provide a non-intrusive way to show additional information or forms without navigating away from the current page.
</Ingress>

## Basic Usage

Here's the simplest way to use sheets: keep the trigger and the sheet inside a single `Fragment`, flip a boolean state, and render the sheet only when it's open.

```csharp demo-tabs
public class BasicSheetExample : ViewBase
{
    public override object? Build()
    {
        var isSheetOpen = UseState(false);
        var sheet = isSheetOpen.Value
            ? new Sheet(() => isSheetOpen.Set(false),
                "Hello from the sheet!"
            )
            : null;

        return new Fragment(
            new Button("Open Sheet").HandleClick(() => isSheetOpen.Set(true)),
            sheet
        );
    }
}
```

<Callout Type="info">
For additional background on fragments, review the [Fragment widget guide](../03_Primitives/Fragment.md).
</Callout>

### Custom Content

The following demonstrates how to create a sheet with custom content using a Fragment and Card. The sheet opens with a title, description, and custom width, showing how to structure content within sheets.

```csharp demo-tabs
public class BasicSheetWithContent : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var isSheetOpen = UseState(false);
        var sheet = isSheetOpen.Value
            ? new Sheet(() => isSheetOpen.Set(false),
                new Fragment(
                    new Card(
                        "Welcome to the sheet!",
                        new Button("Action Button", onClick: _ => client.Toast("Button clicked!"))
                    ).Title("Sheet Content").Description("This is a simple sheet with custom content")
                ),
                title: "Basic Sheet",
                description: "A simple example of sheet usage"
            ).Width(Size.Fraction(1/3f))
            : null;

        return new Fragment(
            new Button("Open Basic Sheet").HandleClick(() => isSheetOpen.Set(true)),
            sheet
        );
    }
}
```

### Using ToTrigger

The `.ToTrigger()` extension method provides a convenient way to open sheets without manually managing state. This pattern is useful when you want to encapsulate the sheet logic in a separate component.

```csharp demo-tabs
public class TriggerSheetExample : ViewBase
{
    public override object? Build()
    {
        var refreshToken = this.UseRefreshToken();
        
        var openBtn = new Button("Open Details Sheet")
            .Icon(Icons.Plus)
            .Outline()
            .ToTrigger((isOpen) => new DetailsSheet(isOpen, refreshToken));

        return openBtn;
    }
}

public class DetailsSheet(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return isOpen.Value
            ? new Sheet(() => isOpen.Set(false),
                Layout.Vertical().Gap(2)
                    | new Card(
                        Layout.Vertical().Gap(2)
                            | Text.H3("Sheet Details")
                            | Text.P("This sheet was opened using .ToTrigger()")
                            | new Button("Perform Action")
                                .Variant(ButtonVariant.Primary)
                                .HandleClick(_ =>
                                {
                                    client.Toast("Action performed!");
                                    refreshToken.Refresh();
                                })
                    ).Title("Content")
                | new Card("Additional information can go here")
                    .Title("More Details"),
                title: "Details Sheet",
                description: "An example of using ToTrigger with sheets"
            ).Width(Size.Fraction(1/2f))
            : null;
    }
}
```

### Conditional Rendering with Selection

Display items inside a sheet and conditionally render different content based on the selected item. This pattern is commonly used in list views, tables, or kanban boards where items are shown in a sheet and clicking an item shows its details.

```csharp demo-tabs
public record Item(
    Guid Id,
    string Name,
    string Description
);

public class SelectionSheetExample : ViewBase
{
    public override object? Build()
    {
        var refreshToken = this.UseRefreshToken();
        var items = UseState<Item[]>(() => new[]
        {
            new Item(Guid.NewGuid(), "Item 1", "Description for item 1"),
            new Item(Guid.NewGuid(), "Item 2", "Description for item 2"),
            new Item(Guid.NewGuid(), "Item 3", "Description for item 3")
        });
        var selectedItemId = UseState((Guid?)null);
        var isSheetOpen = UseState(false);

        // Close sheet after action completes
        UseEffect(() =>
        {
            if (refreshToken.ReturnValue != null)
            {
                isSheetOpen.Set(false);
                selectedItemId.Set((Guid?)null);
            }
        }, [refreshToken]);

        var itemsSheet = isSheetOpen.Value
            ? new ItemsSheet(
                isSheetOpen,
                refreshToken,
                items,
                selectedItemId
            )
            : null;

        return new Fragment(
            new Button("View Items")
                .Variant(ButtonVariant.Outline)
                .HandleClick(() => isSheetOpen.Set(true)),
            itemsSheet
        );
    }
}

public class ItemsSheet(
    IState<bool> isOpen,
    RefreshToken refreshToken,
    IState<Item[]> items,
    IState<Guid?> selectedItemId
) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();

        void HandleItemAction(Item item)
        {
            // Perform action on item
            client.Toast($"Action performed on '{item.Name}'!");
            refreshToken.Refresh();
        }

        object RenderContent()
        {
            if (selectedItemId.Value.HasValue)
            {
                var item = items.Value.First(i => i.Id == selectedItemId.Value!.Value);
                return Layout.Vertical().Gap(2)
                    | new Card(
                        Layout.Vertical().Gap(2)
                            | Text.H3(item.Name)
                            | Text.P(item.Description)
                            | new Button("Perform Action")
                                .Variant(ButtonVariant.Primary)
                                .HandleClick(_ => HandleItemAction(item))
                    ).Title("Item Details")
                    | new Button("Back to List")
                        .Variant(ButtonVariant.Outline)
                        .HandleClick(() => selectedItemId.Set((Guid?)null));
            }

            return Layout.Vertical().Gap(2)
                | items.Value.Select(item =>
                    new Card(item.Name, item.Description)
                        .HandleClick(() => selectedItemId.Set(item.Id))
                ).ToArray();
        }

        return isOpen.Value
            ? new Sheet(() => isOpen.Set(false),
                RenderContent(),
                title: selectedItemId.Value.HasValue 
                    ? items.Value.First(i => i.Id == selectedItemId.Value!.Value).Name
                    : "Items",
                description: selectedItemId.Value.HasValue
                    ? "View and manage item details"
                    : "Select an item to view details"
            ).Width(Size.Fraction(1/2f))
            : null;
    }
}
```

### Footer Actions

For sheets with custom footer actions, use `FooterLayout` to create a fixed footer with action buttons while allowing the main content to scroll. This pattern is useful for forms with multiple actions or when you need custom button behavior.

```csharp demo-tabs
public class FooterActionsExample : ViewBase
{
    public override object? Build()
    {
        var refreshToken = this.UseRefreshToken();
        
        var openBtn = new Button("Open Article Editor")
            .Icon(Icons.Plus)
            .Outline()
            .ToTrigger((isOpen) => new ArticleEditorSheet(isOpen, refreshToken));

        return openBtn;
    }
}

public class ArticleEditorSheet(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var title = this.UseState("Getting Started with Ivy Framework");
        var content = this.UseState("Write your article content here...");
        var isPublished = this.UseState(false);

        void HandleSaveDraft()
        {
            // Save draft logic here
            client.Toast("Draft saved!");
        }

        void HandlePublish()
        {
            // Publish logic here
            isPublished.Set(true);
            client.Toast("Article published!");
            refreshToken.Refresh();
        }

        var sheetContent = new FooterLayout(
            footer: Layout.Horizontal().Gap(2).Align(Align.Right)
                | new Button("Save Draft").HandleClick(_ => HandleSaveDraft())
                | new Button("Publish")
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => HandlePublish()),
            content: Layout.Vertical().Gap(2)
                | new Card(
                    Layout.Vertical().Gap(2)
                        | title.ToTextInput("Article Title")
                        | content.ToTextInput("Article Content")
                            .Variant(TextInputs.Textarea)
                ).Title("Article Details")
                | new Card("Article preview will appear here as you type...")
                    .Title("Preview")
        );

        return isOpen.Value
            ? new Sheet(() => isOpen.Set(false),
                sheetContent,
                title: "Article Editor",
                description: "Create and edit your articles"
            ).Width(Size.Fraction(2/3f))
            : null;
    }
}
```

<WidgetDocs Type="Ivy.Sheet" ExtensionTypes="Ivy.SheetExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Sheet.cs"/>

<Callout Type="info">
For examples of using forms in sheets, see the [Forms documentation](../../01_Onboarding/02_Concepts/Forms.md).
</Callout>

## Examples

<Details>
<Summary>
Conditional Rendering
</Summary>
<Body>
The following demonstrates how to conditionally render different content within a sheet based on state or user actions.

```csharp demo-tabs
public class ConditionalSheetExample : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var isOpen = UseState(false);
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
        
        var body = Layout.Vertical().Gap(2)
            | new Button("Open Conditional Sheet").HandleClick(() => isOpen.Set(true));

        var sheet = isOpen.Value
            ? new Sheet(() => isOpen.Set(false),
                Layout.Vertical().Gap(2)
                    | (Layout.Horizontal().Gap(2)
                        | new Button("List").Variant(viewMode.Value == "list" ? ButtonVariant.Primary : ButtonVariant.Outline)
                            .HandleClick(_ => {
                                viewMode.Set("list");
                                client.Toast("Switched to List view");
                            })
                        | new Button("Grid").Variant(viewMode.Value == "grid" ? ButtonVariant.Primary : ButtonVariant.Outline)
                            .HandleClick(_ => {
                                viewMode.Set("grid");
                                client.Toast("Switched to Grid view");
                            })
                        | new Button("Details").Variant(viewMode.Value == "details" ? ButtonVariant.Primary : ButtonVariant.Outline)
                            .HandleClick(_ => {
                                viewMode.Set("details");
                                client.Toast("Switched to Details view");
                            }))
                    | RenderContent(),
                title: "Conditional Content Sheet",
                description: "Switch between different view modes"
            ).Width(Size.Fraction(2/3f))
            : null;

        return new Fragment(body, sheet);
    }
}
```

</Body>
</Details>
