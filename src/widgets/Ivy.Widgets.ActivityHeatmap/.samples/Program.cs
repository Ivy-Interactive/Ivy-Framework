using Ivy;
using Ivy.Widgets.ActivityHeatmap;

var server = new Server();
server
    .UseHotReload()
    .AddApp<ActivityHeatmapDemo>();
await server.RunAsync();

[App]
class ActivityHeatmapDemo : ViewBase
{
    public override object Build()
    {
        var client = UseService<IClientProvider>();

        var selectedColor = UseState(Colors.Emerald);
        var showDayLabels = UseState(false);
        var showMonthLabels = UseState(false);
        var nullableRange = UseState<(DateOnly?, DateOnly?)>(() =>
            (DateOnly.FromDateTime(DateTime.Today.AddDays(-364)),
             DateOnly.FromDateTime(DateTime.Today)));
        var startDate = nullableRange.Value.Item1;
        var endDate = nullableRange.Value.Item2;

        var rng = new Random(42);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var start = today.AddDays(-364);

        var data = Enumerable
            .Range(0, 365)
            .Select(start.AddDays)
            .Where(_ => rng.NextDouble() > 0.4)
            .Select(d => new { Date = d, Count = rng.Next(1, 20) })
            .ToList();

        var basicUsageExample = Layout.Vertical().Width(Size.Full())
            | Text.H2("Basic Usage").Anchor("basic-usage")
            | new CodeBlock($$"""
                public class ActivityHeatmapDemo : ViewBase
                {
                    public override object Build()
                    {
                        var rng = new Random(42);
                        var today = DateOnly.FromDateTime(DateTime.Today);
                        var start = today.AddDays(-364);

                        var data = Enumerable
                            .Range(0, 365)
                            .Select(start.AddDays)
                            .Where(_ => rng.NextDouble() > 0.4)
                            .Select(d => new Activity { Date = d, Count = rng.Next(1, 20) })
                            .ToList();

                        return Layout.Vertical()
                            | data.ToActivityHeatmap()
                                .Dimension(ActivityDimension.Days, d => d.Date)
                                .Measure("Count", e => e.Sum(d => d.Count));
                    }
                }
                """)

        | new Card(
        Layout.Vertical()
                | data.ToActivityHeatmap()
                    .Dimension(ActivityDimension.Days, d => d.Date)
                    .Measure("Count", e => e.Sum(d => d.Count))
        );

        var optionalPropsExample = Layout.Vertical()
            | Text.H2("With Optional Properties").Anchor("optional-props")
            | new CodeBlock($$"""
                public class ActivityHeatmapOptionalProps : ViewBase
                {
                    public override object Build()
                    {
                        return Layout.Vertical()
                            | data
                                .ToActivityHeatmap()
                                .Dimension(ActivityDimension.Days, d => d.Date)
                                .Measure("Downloads", e => e.Sum(d => d.Count))
                                .ShowMonthLabels({{showMonthLabels.Value.ToString().ToLower()}})
                                .ShowDayLabels({{showDayLabels.Value.ToString().ToLower()}})
                                .StartDate(DateOnly.Parse({{$"\"{startDate}\""}}))
                                .EndDate(DateOnly.Parse({{$"\"{endDate}\""}}))
                                .ColorScheme(Colors.{{selectedColor.Value}})
                                .OnDayClick(day => Console.WriteLine(...));
                }
                """)

            | (Layout.Horizontal().Width(Size.Full())
                | selectedColor.ToColorInput().Variant(ColorInputVariant.SwatchPicker).WithField().Label("Color").Width(Size.MinContent())
                | showDayLabels.ToBoolInput().WithField().Label("Show days").Width(Size.MaxContent())
                | showMonthLabels.ToBoolInput().WithField().Label("Show months").Width(Size.MaxContent())
                | nullableRange.ToDateRangeInput().WithField().Label("Time period").Width(Size.Fit()))

            | new Card(data
                .ToActivityHeatmap()
                .Dimension(ActivityDimension.Days, d => d.Date)
                .Measure("Downloads", e => e.Sum(d => d.Count))
                .StartDate(startDate)
                .EndDate(endDate)
                .ColorScheme(selectedColor.Value)
                .ShowDayLabels(showDayLabels.Value)
                .ShowMonthLabels(showMonthLabels.Value)
                .OnDayClick(day => Console.WriteLine($"Clicked {day.Date}: {day.Count}")))
                .Width(Size.Full()).WithLayout().Horizontal();

        var start2 = DateTime.Now.AddDays(-30);
        var hourlyData = Enumerable
            .Range(0, 24 * 30)
            .Select(d => start2.AddHours(d))
            .Where(_ => rng.NextDouble() > 0.2)
            .Select(d => new { Timestamp = d, Value = GenerateDailyActivity(d) })
            .ToList();

        var hourlyIntervalExample = Layout.Vertical()
            | Text.H2("With Hourly Interval")
            | new CodeBlock("""
                        public class HourlyIntervalExample
                        {
                            public override object Build()
                            {
                                var start2 = DateTime.Now.AddDays(-30);
                                var hourlyData = Enumerable
                                    .Range(0, 24)
                                    .Select(d => start2.AddHours(d))
                                    .Where(_ => rng.NextDouble() > 0.4)
                                    .Select(d => new { Timestamp = d, Value = rng.Next(1, 20) })
                                    .ToList();
                        
                                return Layout.Vertical()
                                    | hourlyData
                                    .ToActivityHeatmap(
                                        dimension: e => e.Timestamp,
                                        measure: e => e.Value,
                                        aggregation: ActivityAggregation.Average)
                                    .Interval(ActivityInterval.Hourly)
                                    .ColorScheme(Colors.Emerald)
                                    .Height(Size.Units(40))
                            }
                        }
                        """)
            | new Card(hourlyData
                .ToActivityHeatmap(
                    dimension: e => e.Timestamp,
                    measure: e => e.Value,
                    aggregation: ActivityAggregation.Sum)
                .Interval(ActivityInterval.Hourly)
                .ColorScheme(Colors.Emerald)
                .Height(Size.Units(40))
            );

        var themeSelector = new Button("Theme")
            .Icon(Icons.SunMoon)
            .Ghost()
            .WithDropDown(
                MenuItem.Default(nameof(ThemeMode.Light)).Icon(Icons.Sun),
                MenuItem.Default(nameof(ThemeMode.Dark)).Icon(Icons.Moon),
                MenuItem.Default(nameof(ThemeMode.System)).Icon(Icons.Computer))
            .OnSelect(@evt =>
            {
                if (Enum.TryParse<ThemeMode>(@evt.Value.ToString(), out var theme))
                    client.SetThemeMode(theme);
            });

        var mainContent = Layout.Vertical().Width(Size.Full())
            | Text.H1("ActivityHeatmap")
            | basicUsageExample
            | optionalPropsExample
            | hourlyIntervalExample
            | new FloatingPanel(themeSelector).AlignSelf(Align.BottomRight);

        return Layout.Vertical().AlignContent(Align.Center)
            | (Layout.Vertical().Width(Size.Units(200).At(Breakpoint.Desktop))
                | mainContent);
    }

    /// <summary>
    /// Generates an activity value between 1 and 10 based on the time of day.
    /// Activity peaks during typical office hours (~9 AM–5 PM) and bottoms out overnight.
    /// </summary>
    private static int GenerateDailyActivity(DateTime timestamp)
    {
        // Seed from the timestamp so each cell gets its own random draw while staying
        // stable across rebuilds. A fixed seed here would make every cell identical.
        var rng = new Random(timestamp.GetHashCode());

        // Hour as a continuous value (e.g. 13.5 for 1:30 PM) for smoother curves.
        double hour = timestamp.Hour + timestamp.Minute / 60.0;

        // Gaussian "bell curve" centered on 1 PM (13:00), the busiest part of the day.
        // sigma controls how quickly activity falls off away from the peak.
        const double peakHour = 13.0;
        const double sigma = 4.0;
        double bell = Math.Exp(-Math.Pow(hour - peakHour, 2) / (2 * sigma * sigma));

        // Weekends are much quieter than weekdays.
        double weekendFactor = timestamp.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday ? 0.35 : 1.0;

        // Probability that any single "activity unit" fires this hour.
        double p = Math.Clamp(bell * weekendFactor, 0.0, 1.0);

        int value = 0;
        for (int i = 0; i < 10; i++)
        {
            if (rng.NextDouble() < p)
                value++;
        }

        return value;
    }
}