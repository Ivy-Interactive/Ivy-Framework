# Container List View

A flexible list of repeated components with containers. The Container List View is a preset version of List View that comes preconfigured with a nested Container component. It maps an array of data to repeating instances — you configure the layout once and it repeats for every item in the data source. Each instance exposes an `item` variable (the current data entry) and an `i` variable (the index).

## Retool

```javascript
// Data source (set via Inspector or JavaScript)
listView1.data = [
  { id: 1, first_name: "Bob", team: "Engineering", sales: 150 },
  { id: 2, first_name: "Kate", team: "Marketing", sales: 210 },
  { id: 3, first_name: "Sarah", team: "Sales", sales: 305 }
];

// Inside each repeated container, reference the current item:
// {{ item.first_name }}  → "Bob", "Kate", "Sarah"
// {{ item.team }}         → "Engineering", "Marketing", "Sales"
// {{ i }}                 → 0, 1, 2

// Filter with a transformer
var salesPerson = {{ listView1.data }};
var highSales = salesPerson.filter(i => i.sales > 200);
return highSales;

// Aggregate form input values across all instances
const formData = {{ listView1.instanceValues }};
const sum = formData.reduce((sum, data) => sum += data.numberInput1, 0);
return sum;

// Nested List Views — parent item accessible via namespace
// listView1 → item or listView1.item
// listView2 (nested) → item or listView2.item, parent via listView1.item
```

## Ivy

```csharp
// Basic list bound to data
public class UserListDemo : ViewBase
{
    public override object? Build()
    {
        var users = new[]
        {
            new { Name = "Bob", Team = "Engineering", Sales = 150 },
            new { Name = "Kate", Team = "Marketing", Sales = 210 },
            new { Name = "Sarah", Team = "Sales", Sales = 305 }
        };

        var items = users.Select(user => new ListItem(
            title: user.Name,
            subtitle: $"{user.Team} — {user.Sales} sales",
            icon: Icons.User
        ));

        return new List(items);
    }
}

// Interactive list with click handlers
public class InteractiveListDemo : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();

        var onItemClick = new Action<Event<ListItem>>(e =>
        {
            client.Toast($"Clicked: {e.Sender.Title}", "Item Selected");
        });

        var items = new[]
        {
            new ListItem("Dashboard", onClick: onItemClick, icon: Icons.House, subtitle: "Main overview"),
            new ListItem("Settings", onClick: onItemClick, icon: Icons.Settings, subtitle: "Configuration")
        };

        return new List(items);
    }
}

// Dynamic list with add/remove (equivalent to instance values)
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

        return Layout.Vertical().Gap(2)
            | new Button("Add Item", addItem).Variant(ButtonVariant.Secondary)
            | new List(items.Value.Select(item => new ListItem(item)));
    }
}

// Custom item rendering with nested widgets
public class CustomListDemo : ViewBase
{
    public override object? Build()
    {
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

        return new List(customItems);
    }
}

// Searchable/filterable list
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

        return Layout.Vertical().Gap(2)
            | searchTerm.ToSearchInput().Placeholder("Search fruits...")
            | new List(filteredItems.Value.Select(item => new ListItem(item)));
    }
}
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `data` | The data source array to repeat over. Accepts string, number, boolean, object, or array. | Constructor parameter: `new List(Object[] items)` or `new List(IEnumerable<object> items)` — data is passed directly as `ListItem` collections. |
| `direction` | Layout direction: `row` (horizontal) or `column` (vertical). | Not supported — Ivy List renders vertically only. Use `GridLayout` for grid arrangements. |
| `numColumns` | Number of columns to display in a grid layout. | Not supported — use `GridLayout` with `.Columns(n)` for multi-column layouts. |
| `itemWidth` | Width of each item in the list. | Not supported |
| `heightType` | Height behavior: `fixed`, `auto`, or `fill`. | `Height` property via `.Height(Size)` fluent method. |
| `maxHeight` | Maximum height limit. | Not supported directly — control via `Height` property. |
| `overflowType` | Overflow behavior: `scroll` or `pagination`. | Not supported |
| `hidden` | Whether the component is hidden. `boolean`, default `false`. | `Visible` property (`bool`). |
| `isHiddenOnDesktop` | Whether hidden on desktop layout. Default `false`. | Not supported |
| `isHiddenOnMobile` | Whether hidden on mobile layout. Default `true`. | Not supported |
| `maintainSpaceWhenHidden` | Whether to reserve space when hidden. Default `false`. | Not supported |
| `margin` | External spacing. Values: `4px 8px` (Normal) or `0` (None). | Not supported directly — use parent layout `.Gap()` or `.Padding()`. |
| `padding` | Internal spacing. Values: `4px 8px` or `0`. | Not supported directly — use parent layout `.Padding()`. |
| `style` | Custom CSS style options. | Not supported — styling is handled via fluent API methods and variants. |
| `enableInstanceValues` | Aggregates form input values across all instances into `instanceValues` array. | Not supported — use `UseState` to manage collected values manually. |
| `instanceValues` | Read-only array of form data from all repeated instances. | Not supported — manage state via `UseState` hook. |
| `formDataKey` | Key used by Form components for default values. Default `{{ self.id }}`. | Not supported |
| `primaryKeyFieldNameOverride` | Primary key field for list item identity. | Not supported |
| `id` | Unique component identifier. Default `listView1`. | Not applicable — components are referenced by variable name in C#. |
| `showInEditor` | Whether visible in editor when hidden. Default `false`. | Not supported |
| `item` variable | References the current data entry in each repeated instance (`listView1.data[i]`). | Handled via LINQ `.Select()` — the lambda parameter serves as the `item` variable. |
| `i` variable | Index of the current entry in the data source. | Handled via LINQ `.Select((item, i) => ...)` for index access. |
| Nested List Views | Up to 3 levels of nesting with namespace-based item access (`listView1.item`, `listView2.item`). | Supported — nest `List` widgets inside `ListItem.items` with their own data projections. |
| `scrollIntoView()` | Scrolls the component into the visible area. Options: `behavior` (`auto`/`smooth`), `block` (`nearest`/`start`/`center`/`end`). | Not supported |
| `scrollToIndex(index)` | Scrolls to a specific row by index. | Not supported |
| `setHidden(hidden)` | Toggles component visibility. | Not supported as a method — set `Visible` property reactively. |
| `clearInstanceValues()` | Clears all aggregated form input values. | Not supported — clear state via `UseState.Set()`. |
| `resetInstanceValues()` | Resets aggregated form input values to defaults. | Not supported — reset state via `UseState.Set()`. |
| ListItem `title` | N/A (configured on nested components) | `ListItem` constructor parameter: `title` (`string`). |
| ListItem `subtitle` | N/A (configured on nested components) | `ListItem` parameter: `subtitle` (`string`). |
| ListItem `icon` | N/A (configured on nested components) | `ListItem` parameter: `icon` (`Icons` enum). |
| ListItem `badge` | N/A (configured on nested components) | `ListItem` parameter: `badge` (`string`). |
| ListItem `onClick` | N/A (event handlers on nested components) | `ListItem` parameter: `onClick` (`Action<Event<ListItem>>`). |
| ListItem `items` | N/A (nested component tree in container) | `ListItem` parameter: `items` (`object[]`) — allows embedding arbitrary widgets. |
