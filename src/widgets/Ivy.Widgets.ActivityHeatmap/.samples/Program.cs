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

        var basicUsageHeading = new ArticleHeading("basic-usage", "Basic Usage", 1);
        var basicUsageExample = Layout.Vertical().Width(Size.Full())
            | Text.H2(basicUsageHeading.Text).Anchor(basicUsageHeading.Id)
            | new CodeBlock(@$"public class ActivityHeatmapDemo : ViewBase
{{
    public override object Build()
    {{
        var rng = new Random(42);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var start = today.AddDays(-364);

        var data = Enumerable
            .Range(0, 365)
            .Select(start.AddDays)
            .Where(_ => rng.NextDouble() > 0.4)
            .Select(d => new Activity {{ Date = d, Count = rng.Next(1, 20) }})
            .ToList();

        return Layout.Vertical()
            | data.ToActivityHeatmap()
                .Dimension(ActivityDimension.Days, d => d.Date)
                .Measure(""Count"", e => e.Sum(d => d.Count));
    }}
}}").Width(Size.Full())
            
        |new Card(    
        Layout.Vertical() 
                | data.ToActivityHeatmap()
                    .Dimension(ActivityDimension.Days, d => d.Date)
                    .Measure("Count", e => e.Sum(d => d.Count))
        );
               
        var optionalPropsUsageHeading = new ArticleHeading("optional-props", "With Optional Properties", 1);// .WithMargin(0, 0, 0, 16);
        var optionalPropsExample = Layout.Vertical()
            | Text.H2(optionalPropsUsageHeading.Text).Anchor(optionalPropsUsageHeading.Id)
            | new CodeBlock(@$"public class ActivityHeatmapOptionalProps : ViewBase
{{
    public override object Build()
    {{
        return Layout.Vertical()
            | data
                .ToActivityHeatmap()
                .Dimension(ActivityDimension.Days, d => d.Date)
                .Measure(""Downloads"", e => e.Sum(d => d.Count))
                .ShowMonthLabels({showMonthLabels.Value.ToString().ToLower()})
                .ShowDayLabels({showDayLabels.Value.ToString().ToLower()})
                .StartDate(DateOnly.Parse({$"\"{startDate}\""}))
                .EndDate(DateOnly.Parse({$"\"{endDate}\""}))
                .ColorScheme(Colors.{selectedColor.Value})
                .OnDayClick(day => Console.WriteLine(...));
}}")
         
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

        var themeSelector = new DropDownMenu(@evt =>
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

        var mainContent = Layout.Vertical().Width(Size.Full())
            .Gap(20)
            | Text.H1("ActivityHeatmap")
            | basicUsageExample
            | optionalPropsExample
            | new FloatingPanel(themeSelector).AlignSelf(Align.BottomRight);

        var article = new Article(
            new object[] { mainContent })
            .Headings([basicUsageHeading, optionalPropsUsageHeading]);

        return article;
    }
}