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
Display collections of items in organized, styled lists with customizable formatting and interactive elements.
</Ingress>

The `List` widget is a container designed to render collections of items in a vertical layout. It works seamlessly with `ListItem` components to create interactive, searchable, and customizable lists that are perfect for navigation menus, data displays, and user interfaces.

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

## ListItem Properties

### Title and Subtitle

Each `ListItem` can have a title and subtitle for hierarchical information display:

```csharp demo-tabs
public class TitleSubtitleDemo : ViewBase
{
    public override object? Build()
    {
        var users = new[]
        {
            new ListItem("John Doe", subtitle: "Software Engineer"),
            new ListItem("Jane Smith", subtitle: "Product Manager"),
            new ListItem("Bob Johnson", subtitle: "Designer"),
            new ListItem("Alice Brown", subtitle: "Developer")
        };
        
        return new List(users);
    }
}
```

### Icons

Add visual indicators with icons to make lists more intuitive:

```csharp demo-tabs
public class IconListDemo : ViewBase
{
    public override object? Build()
    {
        var menuItems = new[]
        {
            new ListItem("Dashboard", icon: Icons.House, subtitle: "Main overview"),
            new ListItem("Users", icon: Icons.Users, subtitle: "Manage users"),
            new ListItem("Settings", icon: Icons.Settings, subtitle: "Configuration"),
            new ListItem("Reports", icon: Icons.ChartBar, subtitle: "Analytics")
        };
        
        return new List(menuItems);
    }
}
```

### Badges

Use badges to show additional information like counts, status, or labels:

```csharp demo-tabs
public class BadgeListDemo : ViewBase
{
    public override object? Build()
    {
        var notifications = new[]
        {
            new ListItem("New Message", subtitle: "From John Doe", badge: "3"),
            new ListItem("System Update", subtitle: "Available now", badge: "!"),
            new ListItem("Task Reminder", subtitle: "Due tomorrow", badge: "5")
        };
        
        return new List(notifications);
    }
}
```

## Interactive Lists

### Clickable Items

Make list items interactive by adding click handlers:

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

Create lists from dynamic data sources:

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

<Callout type="info">
Lists in Ivy are highly customizable. You can combine them with other widgets like Cards, Badges, and Buttons to create rich, interactive interfaces. The `onClick` event on ListItems makes it easy to build navigation and user interactions.
</Callout>

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

<Details>
<Summary>
Custom Item Rendering
</Summary>
<Body>
Use the `items` parameter to create complex list items with custom layouts:

```csharp demo-tabs
public class CustomItemDemo : ViewBase
{
    public override object? Build()
    {
        var products = new[]
        {
            new { Name = "Laptop", Price = 999.99m, Stock = 15 },
            new { Name = "Mouse", Price = 29.99m, Stock = 50 },
            new { Name = "Keyboard", Price = 89.99m, Stock = 25 }
        };
        
        var listItems = products.Select(product => new ListItem(
            title: product.Name,
            subtitle: $"${product.Price} - {product.Stock} in stock",
            items: new object[]
            {
                Layout.Horizontal().Gap(2)
                    | Text.Block($"${product.Price}")
                    | new Badge(product.Stock.ToString()).Variant(BadgeVariant.Secondary)
            }
        ));
        
        return new List(listItems);
    }
}
```

</Body>
</Details>

<Details>
<Summary>
Simple Dynamic List
</Summary>
<Body>
A basic example showing how to add and remove list items:

```csharp demo-tabs
public class SimpleListDemo : ViewBase
{
    public override object? Build()
    {
        var items = UseState(new[] { "First Item" });
        
        var addItem = new Action<Event<Button>>(e =>
        {
            var newItems = items.Value.Append($"Item {items.Value.Length + 1}").ToArray();
            items.Set(newItems);
        });
        
        var removeItem = new Action<Event<Button>>(e =>
        {
            if (items.Value.Length > 1)
            {
                var newItems = items.Value.Take(items.Value.Length - 1).ToArray();
                items.Set(newItems);
            }
        });
        
        var listItems = items.Value.Select(item => new ListItem(item));
        
        return Layout.Vertical().Gap(2)
            | (Layout.Horizontal().Gap(2)
                | new Button("Add Item", addItem)
                | new Button("Remove Item", removeItem))
            | new List(listItems);
    }
}
```

</Body>
</Details>

<Details>
<Summary>
List with Time Rendering
</Summary>
<Body>
Show when each item was created:

```csharp demo-tabs
public class TimeListDemo : ViewBase
{
    public override object? Build()
    {
        var items = UseState(new[] { new { Text = "Item 1", CreatedAt = DateTime.Now } });
        
        var addItem = new Action<Event<Button>>(e =>
        {
            var newItem = new { Text = $"Item {items.Value.Length + 1}", CreatedAt = DateTime.Now };
            var newItems = items.Value.Append(newItem).ToArray();
            items.Set(newItems);
        });
        
        var clearItems = new Action<Event<Button>>(e =>
        {
            items.Set(new[] { new { Text = "Item 1", CreatedAt = DateTime.Now } });
        });
        
        var listItems = items.Value.Select(item => new ListItem(
            title: item.Text,
            subtitle: $"Created at {item.CreatedAt:HH:mm:ss}"
        ));
        
        return Layout.Vertical().Gap(2)
            | (Layout.Horizontal().Gap(2)
                | new Button("Add Item", addItem)
                | new Button("Clear All", clearItems).Variant(ButtonVariant.Destructive))
            | new List(listItems);
    }
}
```

</Body>
</Details>
