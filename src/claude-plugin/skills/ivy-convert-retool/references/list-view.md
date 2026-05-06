# List View

A repeatable list of components with values that map to a list of data. Renders a collection of items in a scrollable, configurable layout supporting both horizontal and vertical orientations, pagination, and virtualization.

## Retool

```toolscript
{{listView1.data}} = {{ query1.data }}

// Configure via inspector:
// direction: "column"
// heightType: "auto"
// numColumns: 2
// overflowType: "pagination"

// Scroll to a specific row
listView1.scrollToIndex(5)

// Access aggregated instance values
listView1.instanceValues
```

## Ivy

```csharp
// Basic list bound to data
return new List(query1.Data.Select(item =>
    new ListItem(item.Name, subtitle: item.Description)
));

// Interactive list with click handlers
var onItemClick = new Action<Event<ListItem>>(e =>
{
    client.Toast($"Clicked: {e.Sender.Title}", "Selected");
});

return new List(items.Select(item =>
    new ListItem(item.Name,
        subtitle: item.Description,
        icon: Icons.ChevronRight,
        onClick: onItemClick)
));

// Dynamic list with search/filter via state
var search = UseState("");
var filtered = items.Where(i =>
    i.Name.Contains(search.Value, StringComparison.OrdinalIgnoreCase));

return new Stack(
    search.ToTextInput(),
    new List(filtered.Select(i => new ListItem(i.Name)))
);
```

## Parameters

| Parameter                    | Documentation                                                     | Ivy                                                    |
|------------------------------|-------------------------------------------------------------------|--------------------------------------------------------|
| `data`                       | The data source array/object rendered as list items               | Constructor parameter (`Object[]` or `IEnumerable<object>`) |
| `direction`                  | Layout orientation: `row` (horizontal) or `column` (vertical)     | Not supported (vertical only)                          |
| `heightType`                 | Sizing behavior: `fixed`, `auto`, or `fill`                       | `Height` (`Size`)                                      |
| `numColumns`                 | Number of columns to display                                      | Not supported                                          |
| `overflowType`               | Overflow handling: `scroll` or `pagination`                       | Not supported                                          |
| `enableInstanceValues`       | Aggregates descendant values on `instanceValues` property         | Not supported                                          |
| `formDataKey`                | Key for Form component data binding                               | Not supported                                          |
| `primaryKeyFieldNameOverride`| Custom primary key specification                                  | Not supported                                          |
| `itemWidth`                  | Individual item width                                             | `Width` (`Size`)                                       |
| `maxHeight`                  | Maximum component height                                          | `Height` (`Size`)                                      |
| `hidden`                     | Toggles component visibility                                      | `Visible` (`bool`)                                     |
| `margin`                     | Outer spacing                                                     | Not supported                                          |
| `padding`                    | Inner spacing                                                     | Not supported                                          |
| `scrollIntoView()`           | Scrolls component into visible area                               | Not supported                                          |
| `scrollToIndex(index)`       | Navigates to a specific row index                                 | Not supported                                          |
| `setHidden(boolean)`         | Programmatically toggles visibility                               | `Visible` (`bool`)                                     |
