using Ivy;
using Ivy.Widgets.ActivityHeatmap;

var server = new Server();
server.AddApp<ActivityHeatmapDemo>();
await server.RunAsync();

[App]
class ActivityHeatmapDemo : ViewBase
{
    public override object Build()
    {
        var rng = new Random(42);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var start = today.AddDays(-364);

        var data = Enumerable
            .Range(0, 365)
            .Select(i => start.AddDays(i))
            .Where(_ => rng.NextDouble() > 0.4)
            .Select(d => new Activity { Date = d, Count = rng.Next(1, 20) })
            .ToArray();

        return Layout.Vertical().Gap(16)
            | new Card()
                .Header(Text.H4("Daily Downloads"))
                .Content(
                    new ActivityHeatmap()
                        .Data(data)
                        .ColorScheme(Colors.Primary)
                        .OnDayClick(day => Console.WriteLine($"Clicked {day.Date}: {day.Count}")))
                .Density(Density.Small)
                .WithBox().Padding(2, 0, 0, 0)
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
