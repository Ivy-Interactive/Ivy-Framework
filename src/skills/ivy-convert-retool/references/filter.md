# Filter

An interface to define filters for Table components. Provides a standalone UI for users to build filter conditions (column, operator, value) that link to and control a connected Table.

## Retool

```toolscript
filter1.linkToTable = true
filter1.linkedTableId = "table1"
filter1.setValue({
  filters: [
    { columnId: "status", operator: "=", value: "active", disabled: false },
    { columnId: "age", operator: ">", value: "25", disabled: false }
  ]
})
filter1.clearValue()
filter1.resetValue()
```

## Ivy

```csharp
// Ivy does not have a standalone Filter widget.
// Filtering is built into the DataTable via config and per-column settings.

sampleUsers.ToDataTable()
    .Filterable(u => u.Name, true)
    .Filterable(u => u.Age, true)
    .Config(config =>
    {
        config.AllowFiltering = true;
        // AI-powered natural language filtering
        config.AllowLlmFiltering = true;
    });

// Users can filter inline via column headers.
// LLM filtering supports expressions like:
//   "salary above 100000"
//   "[HireDate] >= \"2024-01-01\" AND [Status] = \"Active\""
```

## Parameters

| Parameter | Retool | Ivy |
|-----------|--------|-----|
| Standalone filter widget | `Filter` component — separate UI | Not supported — filtering is inline on DataTable columns |
| Link to table | `linkedTableId`, `linkToTable` | Not applicable — filtering is part of DataTable |
| Filter value | `value` — `{ filters: [...] }` object | Not supported as external API |
| Set filter programmatically | `setValue(value)` | Not supported |
| Clear filters | `clearValue()` | Not supported |
| Reset filters | `resetValue()` | Not supported |
| Filter operators | `=`, `!=`, `>`, `<`, etc. via `operator` field | `=`, `!=`, `>`, `>=`, `<`, `<=`, `contains`, `starts with`, `ends with`, `IS BLANK`, `IS NOT BLANK`, `AND`, `OR`, `NOT` |
| Case-sensitive filtering | Via linked Table's `caseSensitiveFiltering` | Not documented |
| Per-column filterable | Via Table column settings | `.Filterable(u => u.Prop, true/false)` |
| LLM / AI filtering | Not supported | `AllowLlmFiltering = true` — natural language queries |
| Visibility | `hidden`, `isHiddenOnDesktop`, `isHiddenOnMobile` | Not applicable |
| Margin | `margin` — e.g. `4px 8px` | Not applicable |
| Custom styling | `style` object | Not applicable |
| Scroll into view | `scrollIntoView()` | Not applicable |
