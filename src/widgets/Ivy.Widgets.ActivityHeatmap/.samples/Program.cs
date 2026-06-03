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
    private record RepoStats(
        DateOnly Date,
        DateTime Timestamp,
        int Downloads,
        int Stars);
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

        var dailyData = Enumerable
            .Range(0, 365)
            .Select(start.AddDays)
            .Where(_ => rng.NextDouble() > 0.4)
            .Select(d => new RepoStats(
                Date: d,
                Timestamp: d.ToDateTime(new TimeOnly()),
                Downloads: rng.Next(1, 20),
                Stars: 0))
            .ToList();

        var basicUsageExample = Layout.Vertical().Width(Size.Full())
            | Text.H3("Basic Usage").Anchor("basic-usage")
            | new CodeBlock("""
                public record RepoStats(
                    DateTime Timestamp,
                    int Stars,
                    int Downloads);

                public interface IMyRepoService
                {
                    IEnumerable<RepoStats> GetDailyStats(),
                    IEnumerable<RepoStats> GetHourlyStats()
                }

                public class ActivityHeatmapDemo : ViewBase
                {
                    public override object Build()
                    {
                        var repoService = UseService<IMyRepoService>();
                        var dailyRepoStats = repoService.GetDailyStats().ToList();

                        return Layout.Vertical()
                            | dailyRepoStats.ToActivityHeatmap(
                                dimension: e => e.Date,
                                measure: e => e.Downloads);
                    }
                }
                """)

        | new Card(
        Layout.Vertical()
                | dailyData.ToActivityHeatmap(
                    dimension: e => e.Date,
                    measure: e => e.Downloads)
        );

        var showMonthLabelsValue = !showMonthLabels.Value ? "\n\t\t\t\t.showMonthLabels(false)" : "";
        var showDayLabelsValue = !showDayLabels.Value ? "\n\t\t\t\t.showDayLabels(false)" : "";
        var optionalPropsExample = Layout.Vertical()
            | Text.H3("With Optional Properties").Anchor("optional-props")
            | new CodeBlock($$"""
                                public class ActivityHeatmapOptionalProps : ViewBase
                                {
                                    public override object Build()
                                    {
                                        var repoService = UseService<IMyRepoService>();
                                        var dailyRepoStats = repoService.GetDailyStats().ToList();

                                        return Layout.Vertical()
                                            | dailyRepoStats
                                                .ToActivityHeatmap(
                                                    dimension: e => e.Date,
                                                    measure: e => e.Downloads){{showDayLabelsValue}}{{showMonthLabelsValue}}
                                                .StartDate(DateOnly.Parse({{$"\"{startDate}\""}}))
                                                .EndDate(DateOnly.Parse({{$"\"{endDate}\""}}))
                                                .ColorScheme(Colors.{{selectedColor.Value}})
                                                .OnDayClick(day => client.Toast($"Clicked {day.Date}: {day.Count} downloads"));
                                    }
                                }
                                """)

            | (Layout.Horizontal().Width(Size.Full())
                | selectedColor.ToColorInput().Variant(ColorInputVariant.SwatchPicker).WithField()
                    .Label("Color").Width(Size.MinContent())
                | showDayLabels.ToBoolInput().WithField().Label("Show days")
                    .Width(Size.MaxContent())
                | showMonthLabels.ToBoolInput().WithField().Label("Show months")
                    .Width(Size.MaxContent())
                | nullableRange.ToDateRangeInput().WithField().Label("Time period")
                    .Width(Size.Fit()))

            | new Card(dailyData
                .ToActivityHeatmap(
                    dimension: e => e.Date,
                    measure: e => e.Downloads)
                .StartDate(startDate)
                .EndDate(endDate)
                .ColorScheme(selectedColor.Value)
                .ShowDayLabels(showDayLabels.Value)
                .ShowMonthLabels(showMonthLabels.Value)
                .OnDayClick(day => client.Toast($"Clicked {day.Date}: {day.Count} downloads")));

        var start2 = DateTime.Now.AddDays(-30);
        var hourlyData = Enumerable
            .Range(0, 24 * 30)
            .Select(d => start2.AddHours(d))
            .Where(_ => rng.NextDouble() > 0.2)
            .Select(d => new RepoStats(Timestamp: d, Date: DateOnly.FromDateTime(d), Stars: GenerateDailyActivity(d), Downloads: 0))
            .ToList();

        var hourlyIntervalExample = Layout.Vertical()
            | Text.H3("With Hourly Interval")
            | new CodeBlock("""
                        public class HourlyIntervalExample
                        {
                            public override object Build()
                            {
                                var repoService = UseService<IMyRepoService>();
                                var hourlyRepoStats = repoService.GetHourlyStats().ToList();
                        
                                return Layout.Vertical()
                                    | hourlyRepoStats
                                    .ToActivityHeatmap(
                                        dimension: e => e.Timestamp,
                                        measure: e => e.Stars)
                                    .ColorScheme(Colors.Emerald)
                                    .Height(Size.Units(40))
                            }
                        }
                        """)
            | new Card(hourlyData
                .ToActivityHeatmap(
                    dimension: e => e.Timestamp,
                    measure: e => e.Stars)
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
            | Text.H2("ActivityHeatmap")
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