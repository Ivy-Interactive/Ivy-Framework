# Ivy.Widgets.ActivityHeatmap

GitHub-style activity heatmap widget for Ivy Framework. Renders a 52-week × 7-day activity graph grid.

## Usage

```csharp
using Ivy.Widgets.ActivityHeatmap;

var data = new[]
{
    new Activity { Date = DateOnly.FromDateTime(DateTime.Today), Count = 5 },
};

new ActivityHeatmap()
    .Data(data)
    .ColorScheme(Colors.Green)
    .OnDayClick(day => Console.WriteLine($"Clicked {day.Date}: {day.Count}"));
```

## Props

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `Data` | `Activity[]` | `[]` | Daily activity data |
| `ColorScheme` | `Ivy.Colors` | `Colors.Primary` | Color scheme supports semantic and chromatic and neutral color tokens |
| `ShowTooltip` | `bool` | `true` | Show date/count tooltip on hover |
| `ShowMonthLabels` | `bool` | `true` | Show month labels along the top |
| `ShowDayLabels` | `bool` | `true` | Show Mon/Wed/Fri labels on the left |
| `StartDate` | `DateOnly?` | `null` | Pins the start of the visible range; when set, overrides the minimum date derived from `Data` |
| `EndDate` | `DateOnly?` | `null` | Pins the end of the visible range; when set, overrides the maximum date derived from `Data` |

If both `StartDate` and `EndDate` are set and `EndDate` is before `StartDate`, the widget treats the range in chronological order (same as swapping the two values), so the grid still renders instead of collapsing to zero weeks.

## Data Constraints

- Duplicate dates in `Data` are not supported.
- Provide at most one `Activity` entry per `DateOnly` day.
- If your source can produce duplicates, aggregate them first (for example by summing counts per date) before passing data to `ActivityHeatmap`.

## Events

| Event | Args | Description |
|-------|------|-------------|
| `OnDayClick` | `Activity` | Fired when user clicks a day cell |

## Development

### Building

1. Install frontend dependencies:

   ```bash
   cd frontend
   pnpm install
   ```

2. Build the frontend:

   ```bash
   pnpm build
   ```

3. Build the widget (repository root, or any path that builds `Ivy.Widgets.ActivityHeatmap.csproj`):

   ```bash
   dotnet build
   ```

   The widget project uses Ivy’s external-widget MSBuild targets: `dotnet build` runs `vp install` / `vp build` in `frontend` when needed, so after dependencies exist you can rely on that instead of repeating steps 1–2.

### Frontend watch

While changing React/TypeScript under `frontend/src`, run a watch build in another terminal:

```bash
cd frontend
pnpm exec vp build --watch
```

### Sample app

```bash
cd .samples
dotnet run
```
