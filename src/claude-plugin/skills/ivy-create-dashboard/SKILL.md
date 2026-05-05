---
name: ivy-create-dashboard
description: >
  Create a Dashboard app with metrics, charts, and KPIs for an Ivy project.
  Use when the user asks for a dashboard, analytics page, KPI view, reporting view,
  data visualization app, or metrics overview. Handles MetricView cards, line charts,
  bar charts, pie charts, area charts, date range filtering, and grid layouts.
allowed-tools: Bash(dotnet:*) Read Write Edit Glob Grep
effort: high
argument-hint: "[description of the dashboard to create]"
---

# ivy-create-dashboard

Create a Dashboard app with metrics, charts, and KPIs for an Ivy project.

## Pre-flight: Read Learnings

If the file `.ivy/learnings/ivy-create-dashboard.md` exists in the project directory, read it first and apply any lessons learned from previous runs of this skill.

## Reference Files

Read these before implementing:
- [references/AGENTS.md](references/AGENTS.md) -- Ivy framework API reference (widgets, hooks, layouts, inputs, colors)
- [references/DashboardApp.cs](references/DashboardApp.cs) -- app structure with HeaderLayout, date range selector, grid layout
- [references/TotalSalesMetricView.cs](references/TotalSalesMetricView.cs) -- MetricView with trend and goal
- [references/OrdersMetricView.cs](references/OrdersMetricView.cs) -- MetricView with count aggregation
- [references/CustomerRetentionRateMetricView.cs](references/CustomerRetentionRateMetricView.cs) -- MetricView with rate calculation
- [references/SalesByDayLineChartView.cs](references/SalesByDayLineChartView.cs) -- line chart pattern (daily granularity)
- [references/OrdersByChannelPieChartView.cs](references/OrdersByChannelPieChartView.cs) -- pie chart pattern
- [references/SalesByCategoryBarChartView.cs](references/SalesByCategoryBarChartView.cs) -- bar chart pattern
- [references/SalesByStoreTypeAreaChartView.cs](references/SalesByStoreTypeAreaChartView.cs) -- area chart pattern

## Workflow

This skill guides you through creating a complete dashboard application:

1. **Plan** - Analyze the data models and propose metrics + charts
2. **Review** - Present the plan for user approval (accept / request changes / cancel)
3. **Implement** - Generate all dashboard files following reference patterns

## Step 1: Plan the Dashboard

If the user has specified a database connection, read the connection context using `ivy cli explain connections/{ConnectionName}`. If not, ask the user which connection to use.

Analyze the data models and suggest approximately 6 metrics (numeric KPIs) and 6 charts.

### Metric Guidelines
- At least one metric should be a "North Star Metric" - the single most important KPI capturing core value
- All metrics must be numerical (not categorical). Avoid metrics better represented as charts (e.g., "Sales by Region")
- The dashboard automatically compares metrics with the previous period - factor this into calculation descriptions

### Chart Guidelines
- Prefer Line charts unless another type clearly fits better
- Line chart granularity should be daily
- Verify all data models and fields exist before suggesting metrics/charts

### Plan Format

Present the plan using this markdown structure:

```
# Dashboard Plan

**Connection**: `{ConnectionName}`
**Icon**: `LayoutDashboard`

## Metrics

### Metric: [Title]

**Icon**: `[LucideIcon]`
**File**: `Apps/Dashboard/[ClassName]MetricView.cs`
**Namespace**: `{ProjectNamespace}.Apps.Dashboard`
**Calculation**: `[description of how to calculate, including comparison to previous period]`

## Charts

### Chart: [Title] ([ChartType])

**File**: `Apps/Dashboard/[ClassName][ChartType]ChartView.cs`
**Namespace**: `{ProjectNamespace}.Apps.Dashboard`
**Calculation**: `[description including dimensions and measures]`
```

### Chrome Decision
- If the dashboard is the **only app** in the project and no multi-app navigation is needed, add a `### Update Program.cs` section that replaces `server.UseAppShell(...)` with `server.UseDefaultApp(typeof(DashboardApp));`.
- If the project has **multiple apps** or the user explicitly wants sidebar navigation, add a `### Update Program.cs` section that ensures `server.UseAppShell(...)` is present.

Present the plan to the user and ask them to **Accept**, **Request Changes**, or **Cancel**.

## Step 2: Handle Changes

If the user requests changes, apply them to the plan and present the updated plan again.

## Step 3: Implement

Once approved, implement all dashboard files following the reference patterns exactly.

### Project Structure

```
Apps/
  DashboardApp.cs                          # Namespace: {ProjectNamespace}.Apps
  Dashboard/
    {ClassName}MetricView.cs               # Namespace: {ProjectNamespace}.Apps.Dashboard
    {ClassName}{ChartType}ChartView.cs     # Namespace: {ProjectNamespace}.Apps.Dashboard
```

### Required Using Directives

Every generated `.cs` file must include explicit `using` statements. Do NOT rely on global usings.

**Connection usings:**
- `using {ProjectNamespace}.Connections.{ConnectionName};` -- for context factory
- `using {ProjectNamespace}.Connections.{ConnectionName}.Models;` -- for entity classes (only if the project uses a Models subfolder; check the connection context)
- `using Microsoft.EntityFrameworkCore;` -- for ToListAsync, SumAsync, CountAsync, etc.

**Dashboard app:** `Ivy`, `Ivy.Apps`, `Ivy.Core.Hooks`, `Ivy.Hooks`, `Ivy.Shared`, `Ivy.Views`, `Ivy.Views.Dashboards`
**Metric views:** `Ivy`, `Ivy.Core.Hooks`, `Ivy.Hooks`, `Ivy.Shared`, `Ivy.Views`, `Ivy.Views.Dashboards`
**Chart views:** `Ivy`, `Ivy.Core.Hooks`, `Ivy.Hooks`, `Ivy.Shared`, `Ivy.Views`, `Ivy.Views.Charts`

### Program.cs Chrome Configuration

Check if the approved plan includes a `### Update Program.cs` section. If it does:
- If it specifies `UseDefaultApp`: Open `Program.cs` and replace the `server.UseAppShell(...)` line with `server.UseDefaultApp(typeof(DashboardApp));`. Remove the `UseAppShell` line entirely. Do NOT keep both.
- If it specifies `UseAppShell`: Ensure `server.UseAppShell(...)` is present in `Program.cs`.
- Follow the approved plan exactly.

### Implementation Instructions

1. Read the reference files listed above for exact code patterns.
2. Create `Apps/DashboardApp.cs`:
   - Namespace: `{ProjectNamespace}.Apps`
   - Using: `{ProjectNamespace}.Apps.Dashboard`
   - Attribute: `[App(icon: Icons.LayoutDashboard, group: ["Apps"])]`
   - Follow the DashboardApp reference exactly (date range selector, grid layout, HeaderLayout, etc.)
3. For each metric in the plan, create `Apps/Dashboard/{ClassName}.cs`:
   - Namespace: `{ProjectNamespace}.Apps.Dashboard`
   - Follow the metric reference patterns (MetricView, UseQuery, trend calculation, goals)
   - Constructor: `(DateTime fromDate, DateTime toDate)`
4. For each chart in the plan, create `Apps/Dashboard/{ClassName}.cs`:
   - Namespace: `{ProjectNamespace}.Apps.Dashboard`
   - Follow the chart reference patterns for the specific chart type
   - Constructor: `(DateTime fromDate, DateTime toDate)`
5. Use the correct ContextFactory name based on the connection.
6. Keep all numbers as double to avoid conversion issues.
7. After writing all files, summarize what was created.

### Common Pitfalls

These are critical rules for generating correct dashboard code:

#### EF Core Limitations
- Do NOT use `Split()`, `Join()`, `Reverse()`, `Last()`, `LastOrDefault()`, or custom methods in LINQ queries - they cannot be translated to SQL.
- Use `Substring()` and `IndexOf()` instead of `Split()` and `Last()`.
- **IMPORTANT**: Never use positional record constructors (`new ChartData(...)`) inside EF Core LINQ expressions (`.Select()`, `.GroupBy().Select()`, etc.). EF Core/SQLite cannot translate them to SQL. Instead:
  1. Use anonymous types in the server-side query: `.Select(x => new { x.Name, Total = x.Sum(...) })`
  2. Materialize with `.ToArrayAsync()` / `.ToListAsync()`
  3. Project to record types client-side: `raw.Select(x => new ChartData(x.Name, x.Total)).ToArray()`

#### Nullable Types
- `DateTime?` does not have `.Date` - use `.Value.Date` or filter nulls with `.Where(x => x.Date.HasValue)` first.
- For `UseState` with nullable types, use explicit cast: `(object?)null`, not just `null`.
- Call `.Value.ToString("N2")` on nullable types, not `.ToString("N2")` directly.
- Only use null-coalescing `??` with actually nullable types.

#### GroupBy
- Always use **named types** (not anonymous types) when using GroupBy followed by aggregation.

#### Aggregation
- Chart measures MUST use aggregation functions (`.Sum()`, `.Count()`, `.Average()`), never direct property access - even on pre-aggregated data.
- Check `.Any()` before calling `.Average()` to avoid exceptions on empty collections.
- `Include()` navigation properties before referencing foreign key relationships.

#### Junction Tables
- Junction/bridge tables often lack `CreatedAt`/`UpdatedAt` fields. Verify before filtering by date - use related entity timestamps instead.

#### Status Fields
- Always verify the actual type of status ID fields (`int`, `int?`, `string`) before comparisons.
- Use actual database seed values, not undefined constants.

#### Date Fields
- Check entity definitions to determine if date fields are `DateTime` or `string`. If `string`, use `DateTime.Parse()`.

#### ViewBase Inheritance
- **All metric and chart view classes MUST extend `ViewBase`** and use `public override object? Build()`.
- Plain classes without `: ViewBase` will render as their `ToString()` output when used with the `|` layout operator.

## Post-run: Evaluate and Improve

After completing the task:

1. **Evaluate**: Did the build succeed? Were there compilation errors, unexpected behavior, or manual corrections needed during this run?
2. **Update learnings**: If anything required correction or was surprising, append a concise entry to `.ivy/learnings/ivy-create-dashboard.md` (create the file and `.ivy/learnings/` directory if they don't exist). Each entry should note: the date, what went wrong, why, and what to do differently next time.
3. **Skip if clean**: If everything succeeded without issues, do not update the learnings file.
