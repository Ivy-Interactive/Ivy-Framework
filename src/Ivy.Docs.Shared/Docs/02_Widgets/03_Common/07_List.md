---
searchHints:
  - items
  - collection
  - listitem
  - menu
  - rows
  - scroll
---

# List

<Ingress>
Display collections of items in organized, styled lists with customizable formatting and interactive [elements](../../01_Onboarding/02_Concepts/03_Widgets.md).
</Ingress>

The `List` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) is a container designed to render collections of items in a vertical layout. It works seamlessly with `ListItem` components to create interactive, searchable, and customizable lists that are perfect for [navigation menus](../../01_Onboarding/02_Concepts/14_Navigation.md), data displays, and [user interfaces](../../01_Onboarding/02_Concepts/02_Views.md).

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

`ListItem`s are highly customizable, supporting titles, subtitles, [icons](../01_Primitives/02_Icon.md), and [badges](02_Badge.md).

```csharp demo-tabs
public class ListConfigDemo : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical().Gap(4)
            | Text.P("Title and Subtitle").Large()
            | new List(new[]
            {
                new ListItem("John Doe", subtitle: "Software Engineer"),
                new ListItem("Jane Smith", subtitle: "Product Manager")
            })
            | Text.P("Icons").Large()
            | new List(new[]
            {
                new ListItem("Dashboard", icon: Icons.House, subtitle: "Main overview"),
                new ListItem("Settings", icon: Icons.Settings, subtitle: "Configuration")
            })
            | Text.P("Badges").Large()
            | new List(new[]
            {
                new ListItem("New Message", subtitle: "From John Doe", badge: "3"),
                new ListItem("System Update", subtitle: "Available now", badge: "!")
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
            new ListItem("Click me!", onClick: onItemClick, icon: Icons.MousePointer),
            new ListItem("Me too!", onClick: onItemClick, icon: Icons.MousePointer)
        };

        return new List(items);
    }
}
```

### Dynamic Content

Create lists from dynamic data sources using [UseState](../../03_Hooks/Core/03_UseState.md).

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

Lists in Ivy are highly customizable. You can combine them with other widgets like Cards, Badges, and Buttons to create rich, interactive [interfaces](../../01_Onboarding/02_Concepts/02_Views.md). The `onClick` event on ListItems makes it easy to build [navigation](../../01_Onboarding/02_Concepts/14_Navigation.md) and user interactions.

### Search and Filter

Implement search functionality with filtered lists:

```csharp demo-tabs
public class SearchableListDemo : ViewBase
{
    public override object? Build()
    {
        var allItems = new[] { "Apple", "Banana", "Cherry", "Date" };
        var searchTerm = UseState("");
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

<WidgetDocs Type="Ivy.List" ExtensionTypes="Ivy.WidgetBaseExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Lists/List.cs"/>

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
        
        var customItems = products.Select(product => new ListItem(
            title: product.Name,
            subtitle: $"${product.Price} - {product.Stock} in stock",
            items: new object[]
            {
                Layout.Horizontal().Gap(2)
                    | Text.Block($"${product.Price}")
                    | new Badge(product.Stock.ToString()).Variant(BadgeVariant.Secondary)
            }
        ));

        // Time Rendering Data
        var timeItem = new ListItem(
            title: "Task created",
            subtitle: $"Created at {DateTime.Now:HH:mm:ss}"
        );

        return Layout.Vertical().Gap(4)
            | Text.P("Custom Item Rendering").Large()
            | new List(customItems)
            | Text.P("Time Rendering").Large()
            | new List(new[] { timeItem });
    }
}
```
