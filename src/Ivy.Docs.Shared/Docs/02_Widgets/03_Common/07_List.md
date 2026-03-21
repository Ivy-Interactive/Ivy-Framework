---
searchHints:
  - items
  - collection
  - listitem
  - list-item
  - tag
  - disabled
  - menu
  - rows
  - scroll
---

# List

<Ingress>
Display collections of items in organized, styled lists with customizable formatting and interactive [elements](../../01_Onboarding/02_Concepts/03_Widgets.md).
</Ingress>

The `List` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) is a container designed to render collections of items in a vertical layout. It works seamlessly with `ListItem` components to create interactive, searchable, and customizable lists that are perfect for [navigation menus](../../01_Onboarding/02_Concepts/09_Navigation.md), data displays, and [user interfaces](../../01_Onboarding/02_Concepts/02_Views.md).

## Basic Usage

The simplest way to create a list is by passing items directly to the constructor:

```csharp demo-below
public class BasicListDemo : ViewBase
{
    public override object? Build()
    {
        var items = new[]
        {
            new ListItem("Apple"),
            new ListItem("Banana"),
            new ListItem("Cherry")
        };

        return new List(items);
    }
}
```

## ListItem Configuration

`ListItem`s are highly customizable, supporting titles, subtitles, [icons](../01_Primitives/02_Icon.md), [badges](02_Badge.md), custom content, and interactive states.

- **Title & Subtitle** - Primary and secondary text labels.
- **Icon** - Visual indicator using the [Icons](../../04_ApiReference/Ivy/Icons.md) enum.
- **Badge** - Small status indicator or counter.
- **Tag** - Hidden object reference (e.g., a database entity) used to identify the item in click handlers.
- **Disabled** - Disables interactions and applies a muted visual style.
- **Content** - Custom widget content injected into the item (e.g., a switch, input, or complex layout).

```csharp demo-tabs
public class ListConfigDemo : ViewBase
{
    public override object? Build()
    {
        var notifications = UseState(false);
        var searchPattern = UseState("");

        return Layout.Vertical().Gap(4)
            | Text.P("Title and Subtitle").Large()
            | new List(new[]
            {
                new ListItem("John Doe").Subtitle("Software Engineer"),
                new ListItem("Jane Smith").Subtitle("Product Manager")
            })
            | Text.P("Icons").Large()
            | new List(new[]
            {
                new ListItem("Dashboard").Icon(Icons.House).Subtitle("Main overview"),
                new ListItem("Settings").Icon(Icons.Settings).Subtitle("Configuration")
            })
            | Text.P("Badges").Large()
            | new List(new[]
            {
                new ListItem("New Message").Subtitle("From John Doe").Badge("3"),
                new ListItem("System Update").Subtitle("Available now").Badge("!")
            })
            | Text.P("Custom Content").Large()
            | new List(new[]
            {
                new ListItem("Notifications").Icon(Icons.Bell)
                    .Content(notifications.ToBoolInput().Variant(BoolInputVariant.Switch)),
                new ListItem("Status").Icon(Icons.Activity)
                    .Content(
                        Layout.Horizontal().Gap(2)
                            | new Badge("Online", BadgeVariant.Success)
                            | Text.Muted("Last seen 2 min ago")
                    ),
                new ListItem("Search").Icon(Icons.Search)
                    .Content(searchPattern.ToTextInput().Placeholder("Type to search..."))
            });
    }
}
```

## Interactive Lists

Make list items interactive with click handlers and dynamic updates.

### Clickable Items

```csharp demo-tabs
public class InteractiveListDemo : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();

        var onItemClick = new Action<Event<ListItem>>(e =>
        {
            var item = e.Sender;
            client.Toast($"Clicked: {item.Title}", "Item Selected");
        });

        var items = new[]
        {
            new ListItem("Click me!").OnClick(onItemClick).Icon(Icons.MousePointer),
            new ListItem("Me too!").OnClick(onItemClick).Icon(Icons.MousePointer),
            new ListItem("Unavailable action").OnClick(onItemClick).Icon(Icons.Ban).Disabled()
        };

        return new List(items);
    }
}
```

### Dynamic Content

Create lists from dynamic data sources using [UseState](../../03_Hooks/02_Core/03_UseState.md).

```csharp demo-tabs
public class DynamicListDemo : ViewBase
{
    public override object? Build()
    {
        var items = UseState(new[] { "Item 1", "Item 2", "Item 3" });

        var addItem = new Action<Event<Button>>(e =>
        {
            var newItems = items.Value.Append($"Item {items.Value.Length + 1}").ToArray();
            items.Set(newItems);
        });

        var removeItem = new Action<Event<Button>>(e =>
        {
            if (items.Value.Length > 0)
            {
                var newItems = items.Value.Take(items.Value.Length - 1).ToArray();
                items.Set(newItems);
            }
        });

        return Layout.Vertical().Gap(2)
            | (Layout.Horizontal().Gap(1)
                | new Button("Add Item", addItem).Variant(ButtonVariant.Secondary)
                | new Button("Remove Item", removeItem).Variant(ButtonVariant.Destructive))
            | new List(items.Value.Select(item => new ListItem(item)));
    }
}
```

Lists in Ivy are highly customizable. You can combine them with other widgets like Cards, Badges, and Buttons to create rich, interactive [interfaces](../../01_Onboarding/02_Concepts/02_Views.md). The `OnClick` event on ListItems makes it easy to build [navigation](../../01_Onboarding/02_Concepts/14_Navigation.md) and user interactions.

### Search and Filter

Implement search functionality with filtered lists:

```csharp demo-tabs
public class SearchableListDemo : ViewBase
{
    public override object? Build()
    {
        var searchTerm = UseState("");
        var allItems = new[] { "Apple", "Banana", "Cherry", "Date" };
        var filteredItems = UseState(allItems);

        UseEffect(() =>
        {
            var filtered = allItems.Where(item =>
                item.Contains(searchTerm.Value, StringComparison.OrdinalIgnoreCase)).ToArray();
            filteredItems.Set(filtered);
        }, [searchTerm]);

        var listItems = filteredItems.Value.Select(item => new ListItem(item));

        return Layout.Vertical().Gap(2)
            | searchTerm.ToSearchInput().Placeholder("Search fruits...")
            | new List(listItems);
    }
}
```

<WidgetDocs Type="Ivy.List" ExtensionTypes="Ivy.WidgetBaseExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Lists/List.cs"/>
<WidgetDocs Type="Ivy.ListItem" ExtensionTypes="Ivy.ListItemExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Lists/ListItem.cs"/>

## Examples

```csharp demo-tabs
public class ExamplesListDemo : ViewBase
{
    public override object? Build()
    {
        // Custom Item Rendering Data
        var products = new[]
        {
            new { Name = "Laptop", Price = 999.99m, Stock = 15 },
            new { Name = "Mouse", Price = 29.99m, Stock = 50 }
        };

        var customItems = products.Select(product => new ListItem(product.Name)
            .Subtitle($"${product.Price} - {product.Stock} in stock")
            .Content(
                Layout.Horizontal().Gap(2)
                    | Text.Block($"${product.Price}")
                    | new Badge(product.Stock.ToString()).Variant(BadgeVariant.Secondary)
            )
        );

        // Time Rendering Data
        var timeItem = new ListItem("Task created")
            .Subtitle($"Created at {DateTime.Now:HH:mm:ss}");

        return Layout.Vertical().Gap(4)
            | Text.P("Custom Item Rendering").Large()
            | new List(customItems)
            | Text.P("Time Rendering").Large()
            | new List(new[] { timeItem });
    }
}
```
