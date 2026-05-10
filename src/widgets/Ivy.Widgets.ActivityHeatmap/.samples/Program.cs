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

        var rng = new Random(42);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var start = today.AddDays(-364);

        var data = Enumerable
            .Range(0, 365)
            .Select(start.AddDays)
            .Where(_ => rng.NextDouble() > 0.4)
            .Select(d => new Activity { Date = d, Count = rng.Next(1, 20) })
            .ToArray();

        return Layout.Vertical()
            .Gap(16)
            .Width(Size.Auto())

            | new DropDownMenu(@evt =>
                {
                    ThemeMode selectedTheme = @evt.Value switch
                    {
                        "Light" => ThemeMode.Light,
                        "Dark" => ThemeMode.Dark,
                        _ => ThemeMode.System,
                    };
                    client.SetThemeMode(selectedTheme);
                },
                new Button("Theme"),
                MenuItem.Default("Light").Icon(Icons.Sun),
                MenuItem.Default("Dark").Icon(Icons.Moon),
                MenuItem.Default("System").Icon(Icons.Computer))

            | (Layout.Vertical().Gap(2)
                | Text.H4("Daily Downloads")
                | new ActivityHeatmap()
                    .Data(data)
                    .ColorScheme(Colors.Primary)
                    .OnDayClick(day => Console.WriteLine($"Clicked {day.Date}: {day.Count}"))
                    .WithLayout()
                    .Padding(4)
                    .BorderColor(Colors.Secondary)
                    .BorderThickness(1)
                    .BorderRadius(BorderRadius.Rounded))

            | new ActivityHeatmap()
                .Data(data)
                .ColorScheme(Colors.Emerald)
                .OnDayClick(day => Console.WriteLine($"Clicked {day.Date}: {day.Count}"))

            | new ActivityHeatmap()
                .Data(data)
                .ColorScheme(Colors.Blue)
                .ShowDayLabels(false)
                .ShowMonthLabels(false)
                .OnDayClick(day => Console.WriteLine($"Clicked {day.Date}: {day.Count}"));
    }
}