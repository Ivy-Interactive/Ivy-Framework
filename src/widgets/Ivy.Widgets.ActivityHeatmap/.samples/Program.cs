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

        var selectedColor = UseState(Colors.Primary);
        var showDayLabels = UseState(true);
        var showMonthLabels = UseState(true);
        var endDate = UseState(DateOnly.FromDateTime(DateTime.Today));
        var startDate = UseState(endDate.Value.AddDays(-364));

        var rng = new Random(42);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var start = today.AddDays(-364);
        var data = Enumerable
            .Range(0, 365)
            .Select(start.AddDays)
            .Where(_ => rng.NextDouble() > 0.4)
            .Select(d => new Activity { Date = d, Count = rng.Next(1, 20) })
            .ToArray();

        return Layout
            .Vertical()
            .Gap(4)
            .Width(Size.Auto().At(Breakpoint.Tablet).And(Breakpoint.Desktop, Size.Fit()))

            | Text.H1("ActivityHeatmap")
            | Text.H2("Basic Usage")
            | new CodeBlock(@$"
Activity[] data = 
[
    {{
        Date = DateOnly.FromDateTime(DateTime.Today),
        Count = 16
    }}
];

new ActivityHeatmap()
    .Data(data)
    .StartDate({startDate.Value})
    .EndDate({endDate.Value})
    .ColorScheme(Colors.{selectedColor.Value})
    .ShowDayLabels({showDayLabels.Value.ToString().ToLower()})
    .ShowMonthLabels({showMonthLabels.Value.ToString().ToLower()})
    .OnDayClick(day => Console.WriteLine(...));", Languages.Csharp)

            | (Layout
                .Vertical()
                .Gap(2)
                .Padding(4)
                .BorderThickness(1)
                .BorderRadius(BorderRadius.Rounded)

                | (Layout
                    .Horizontal()
                    .Gap(2)
                    | selectedColor.ToColorInput().Variant(ColorInputVariant.SwatchPicker)
                    | Text.P(selectedColor.Value.ToString())
                    | showDayLabels.ToBoolInput().Label("Show day labels")
                    | showMonthLabels.ToBoolInput().Label("Show month labels")
                    | startDate.ToDateInput().WithLabel("Start date")
                    | endDate.ToDateInput().WithLabel("End date"))

                | new ActivityHeatmap()
                    .Data(data)
                    .StartDate(startDate.Value)
                    .EndDate(endDate.Value)
                    .ColorScheme(selectedColor.Value)
                    .ShowDayLabels(showDayLabels.Value)
                    .ShowMonthLabels(showMonthLabels.Value)
                    .OnDayClick(day => Console.WriteLine($"Clicked {day.Date}: {day.Count}")))

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
                    new Button("Theme").Variant(ButtonVariant.Link).Icon(Icons.SunMoon),
                    MenuItem.Default("Light").Icon(Icons.Sun),
                    MenuItem.Default("Dark").Icon(Icons.Moon),
                    MenuItem.Default("System").Icon(Icons.Computer));
    }
}