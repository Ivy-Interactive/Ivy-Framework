# Timeline

Display a Gantt chart of events. Visualizes events over time with configurable timescales, milestones, and color-coded entries.

## Retool

```toolscript
timeline1.data = {{ getProjects.data }}
timeline1.eventTitleByIndex = {{ getProjects.data.map(d => d.name) }}
timeline1.eventStartDateByIndex = {{ getProjects.data.map(d => d.startDate) }}
timeline1.eventEndDateByIndex = {{ getProjects.data.map(d => d.endDate) }}
timeline1.eventColorByIndex = {{ getProjects.data.map(d => d.color) }}
timeline1.timescale = { unit: "month", split: 1 }
timeline1.showTodayIndicator = true
```

## Ivy

Ivy does not have a dedicated Timeline or Gantt chart widget. For simple timelines, compose `List` with `ListItem` widgets. For full Gantt chart functionality, use an `Iframe` or `Embed` with an external charting library.

```csharp
// Simple timeline using List + ListItem
var events = new[]
{
    new { Title = "Phase 1: Planning", Date = "Jan 2025 - Feb 2025" },
    new { Title = "Phase 2: Development", Date = "Mar 2025 - Jun 2025" },
    new { Title = "Phase 3: Testing", Date = "Jul 2025 - Aug 2025" },
};

new List(events.Select(e =>
    new ListItem(title: e.Title, subtitle: e.Date, icon: Icons.Calendar)
));

// For a full Gantt chart, embed an external library
new Iframe("https://your-gantt-chart-url.com")
    .Width(Size.Full())
    .Height(Size.Units(100));
```

## Parameters

| Parameter                    | Documentation                                     | Ivy           |
|------------------------------|---------------------------------------------------|---------------|
| `data`                       | Custom event data configuration                   | Not supported |
| `eventTitleByIndex`          | Event titles by index                             | Not supported |
| `eventStartDateByIndex`      | Event start dates by index                        | Not supported |
| `eventEndDateByIndex`        | Event end dates by index                          | Not supported |
| `eventColorByIndex`          | Color assignments for each event                  | Not supported |
| `eventIdByIndex`             | Event identifiers by index                        | Not supported |
| `eventTooltipLabelByIndex`   | Tooltip text for events                           | Not supported |
| `initialDate`                | Starting date for display                         | Not supported |
| `timescale`                  | Visible time period (unit + split)                | Not supported |
| `showTodayIndicator`         | Display current date marker                       | Not supported |
| `milestoneData`              | Milestone information                             | Not supported |
| `metaEventData`              | Event metadata                                    | Not supported |
| `hidden`                     | Visibility toggle                                 | Not supported |
| `margin`                     | External spacing                                  | Not supported |
| Events: Click                | Triggered when an item is clicked                 | Not supported |
| `scrollIntoView()`           | Scrolls component into visible area               | Not supported |
| `setHidden(hidden)`          | Toggles component visibility                      | Not supported |
