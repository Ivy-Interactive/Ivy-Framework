# Event List

A content area to display a list of events. Supports grouping by day, sorting, timestamps, and custom date/time formatting. Useful for timelines, activity feeds, and scheduled event displays.

## Retool

```javascript
// Event List items are configured via the Inspector.
// Items are provided as an array of strings, with optional timestamps.
// Grouping by day and descending sort can be toggled.

// Programmatic methods:
eventList.scrollIntoView({ behavior: "smooth", block: "center" });
eventList.setHidden(false);
```

## Ivy

Ivy does not have a dedicated Event List widget. The closest equivalent is the **List** widget with **ListItem** components, which can be combined with date/time formatting in subtitles to approximate event list behavior.

```csharp
public class EventListDemo : ViewBase
{
    public override object? Build()
    {
        var events = new[]
        {
            new { Title = "Deployment completed", Time = DateTime.Now.AddHours(-1) },
            new { Title = "Build started", Time = DateTime.Now.AddHours(-2) },
            new { Title = "PR merged", Time = DateTime.Now.AddHours(-5) },
        };

        var items = events
            .OrderByDescending(e => e.Time)
            .Select(e => new ListItem(
                title: e.Title,
                subtitle: e.Time.ToString("MMM dd, yyyy hh:mm tt"),
                icon: Icons.Clock
            ));

        return new List(items);
    }
}
```

## Parameters

| Parameter       | Documentation                                                           | Ivy                                                                                      |
|-----------------|-------------------------------------------------------------------------|------------------------------------------------------------------------------------------|
| `items`         | The items in the Event List (array of strings).                         | `List(items)` constructor accepts `Object[]` or `IEnumerable<object>` of `ListItem`.     |
| `dateFormat`    | The displayed date format (e.g., `"MM/DD/YYYY"`).                       | Not supported. Format dates manually in `ListItem` subtitle via `DateTime.ToString()`.   |
| `timeFormat`    | The displayed time format.                                              | Not supported. Format times manually in `ListItem` subtitle via `DateTime.ToString()`.   |
| `isGrouped`     | Whether to group items.                                                 | Not supported. Implement manually by rendering separate `List` widgets per group.         |
| `groupBy`       | Grouping option: `"day"` or `"none"`.                                   | Not supported.                                                                           |
| `sortedDesc`    | Whether to sort items in descending order when grouped.                 | Not supported. Sort with LINQ (`OrderByDescending`) before passing items to `List`.       |
| `timestamps`    | A list of timestamps for each item when grouped.                        | Not supported. Embed timestamps in `ListItem` subtitle or custom `items` content.        |
| `pending`       | Label for pending items when grouping is disabled.                      | Not supported.                                                                           |
| `renderAsHtml`  | Whether to render text as HTML.                                         | Not supported. Use Ivy's `Html` widget inside `ListItem.items` for rich content.          |
| `events`        | Event handlers (supports Click).                                        | `ListItem(onClick: Action<Event<ListItem>>)` for click handling.                         |
| `id`            | Unique component identifier.                                            | Not applicable. Ivy widgets are referenced by variable name.                              |
| `setHidden()`   | Toggle component visibility.                                            | `Visible` property on `List` (read-only; control via conditional rendering).              |
| `scrollIntoView()` | Scroll component into view with behavior and position options.       | Not supported.                                                                           |
