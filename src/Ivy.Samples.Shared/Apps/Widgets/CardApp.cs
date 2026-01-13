using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.IdCard, path: ["Widgets"], searchHints: ["container", "panel", "box", "section", "wrapper", "border"])]
public class CardApp : SampleBase
{
    protected override object? BuildSample()
    {
        var client = UseService<IClientProvider>();

        var card1 = new Card(
            content: "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec nunc",
            footer: new Button("Sign Me Up", _ => client.Toast("You have signed up!")).TestId("card-app-signup-button"),
            header: Layout.Horizontal().Align(Align.Center)
                    | Text.H4("Card App").WithLayout().Grow()
        ).TestId("card-app");

        var card2 = new Card(
            content: "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec nunc",
            header: Layout.Horizontal().Align(Align.Center)
                    | Text.H4("Card with Border").WithLayout().Grow()
        ).BorderThickness(3)
         .BorderStyle(BorderStyle.Dashed)
         .BorderColor(Colors.Primary)
         .BorderRadius(BorderRadius.Rounded)
         .TestId("card-border");

        var card3 = new Card(
            content: "This card demonstrates the border color fix with a thick red border.",
            header: Layout.Horizontal().Align(Align.Center)
                    | Text.H4("Border Color Test").WithLayout().Grow()
        ).BorderThickness(4)
         .BorderStyle(BorderStyle.Solid)
         .BorderColor(Colors.Red)
         .BorderRadius(BorderRadius.Rounded)
         .TestId("card-border-color");

        var card4 = new Card(
            content: "This card demonstrates OnClick handlers.",
            header: Layout.Horizontal().Align(Align.Center)
                    | Text.H4("OnClick test").WithLayout().Grow()
        ).TestId("card-onclick")
        .HandleClick(_ =>
        {
            client.Toast("Clicked!");
        });

        var smallCard = new Card(
            content: "This is a small card with elements.",
            header: Layout.Horizontal().Align(Align.Center)
                    | Text.H4("Small Card with Elements").WithLayout().Grow()
                    | Icons.Info.ToIcon().Color(Colors.Neutral)
        ).Small()
        .TestId("card-small-with-elements");

        var mediumCard = new Card(
            content: "This is a medium card with elements.",
            header: Layout.Horizontal().Align(Align.Center)
                    | Text.H4("Medium Card with Elements").WithLayout().Grow()
                    | Icons.Info.ToIcon().Color(Colors.Neutral)
        ).Medium()
        .TestId("card-medium-with-elements");

        var largeCard = new Card(
            content: "This is a large card with elements.",
            header: Layout.Horizontal().Align(Align.Center)
                    | Text.H4("Large Card with Elements").WithLayout().Grow()
                    | Icons.Info.ToIcon().Color(Colors.Neutral)
        ).Large();

        return Layout.Vertical()
         | Text.H1("Card")
         | Text.H2("Basic Examples")
         | (Layout.Grid().Columns(4)
            | card1
            | card2
            | card3
            | card4
            )
         | (Layout.Grid().Columns(4)
            | new TotalSalesMetricView()
            | new LongNumberMetricView()
            | new HighPercentageMetricView()
            | new TotalCommentsPerAuthorMetricView()
            )
         | (Layout.Grid().Columns(4)
            | new VeryLongTitleMetricView()
            | new UserEngagementWidget()
            | new TaskCompletionWidget()
            | new SystemHealthWidget()
            )
         | (Layout.Grid().Columns(4)
            | new RevenueGrowthWidget()
            | new IconTextShowcaseWidget()
            | new ProgressBarVariationsWidget()
            | new LayoutTestWidget()
            )
         | (Layout.Grid().Columns(3)
            | new MixedContentWidget()
            | new ResponsiveLayoutWidget()
            | new TextSpacingDemoWidget()
            )
         | (Layout.Grid().Columns(3)
            | new CardPaddingOverrideWidget()
            | new LayoutSpacingControlWidget()
            )
         | Text.H2("Metric Cards with Different Sizes")
         | (Layout.Grid().Columns(3)
            | new SmallMetricView()
            | new MediumMetricView()
            | new LargeMetricView()
            )
        | Text.H2("Size Variants")
         | (Layout.Grid().Columns(3)
            | smallCard
            | mediumCard
            | largeCard
            )
      ;
    }
}

// Metric Views
public class TotalSalesMetricView : ViewBase
{
    public override object? Build()
    {
        return new MetricView("Total Sales");
    }
}

public class MetricView(string title) : ViewBase
{
    public override object? Build()
    {
        return new Card(
                content: (Layout.Horizontal().Align(Align.Left).Gap(2)
                         | Text.P("$84,250").Large()
                         | Icons.TrendingUp.ToIcon().Color(Colors.Emerald)
                         | Text.P("21%").Small().Color(Colors.Emerald)),
                footer: new Progress(21).Goal(800_000.ToString("C0")),
                header: Layout.Horizontal().Align(Align.Center)
                        | Text.H4(title).WithLayout().Grow()
                        | Icons.DollarSign.ToIcon().Color(Colors.Neutral)
            ).TestId("card-total-sales");
    }
}

public class LongNumberMetricView : ViewBase
{
    public override object? Build()
    {
        return new Card(
                content: (Layout.Horizontal().Align(Align.Left).Gap(2)
                         | Text.P("$123,456,789.99").Large()
                         | Icons.TrendingUp.ToIcon().Color(Colors.Emerald)
                         | Text.P("1,234.5%").Small().Color(Colors.Emerald)),
                footer: new Progress(85).Goal("$100,000,000"),
                header: Layout.Horizontal().Align(Align.Center)
                        | Text.H4("Very Long Revenue Number").WithLayout().Grow()
                        | Icons.DollarSign.ToIcon().Color(Colors.Neutral)
            );
    }
}

public class HighPercentageMetricView : ViewBase
{
    public override object? Build()
    {
        return new Card(
                content: (Layout.Horizontal().Align(Align.Left).Gap(2)
                         | Text.P("1,012.50%").Large()
                         | Icons.TrendingUp.ToIcon().Color(Colors.Emerald)
                         | Text.P("38.1%").Small().Color(Colors.Emerald)),
                footer: new Progress(125).Goal("806.67%"),
                header: Layout.Horizontal().Align(Align.Center)
                        | Text.H4("Post Engagement Rate").WithLayout().Grow()
                        | Icons.Activity.ToIcon().Color(Colors.Neutral)
            );
    }
}

public class VeryLongTitleMetricView : ViewBase
{
    public override object? Build()
    {
        return new Card(
                content: (Layout.Horizontal().Align(Align.Left).Gap(2)
                         | Text.P("2.25").Large()
                         | Icons.TrendingUp.ToIcon().Color(Colors.Emerald)
                         | Text.P("38.1%").Small().Color(Colors.Emerald)),
                footer: new Progress(90).Goal("2.50"),
                header: Layout.Horizontal().Align(Align.Center)
                        | Text.H4("Total Comments per Author in This Period").WithLayout().Grow()
                        | Icons.MessageCircle.ToIcon().Color(Colors.Neutral)
            );
    }
}

public class TotalCommentsPerAuthorMetricView : ViewBase
{
    public override object? Build()
    {
        return new Card(
                content: (Layout.Horizontal().Align(Align.Left).Gap(2)
                         | Text.P("2.25").Large()
                         | Icons.TrendingUp.ToIcon().Color(Colors.Emerald)
                         | Text.P("38.1%").Small().Color(Colors.Emerald)),
                footer: new Progress(90).Goal("2.50"),
                header: Layout.Horizontal().Align(Align.Center)
                        | Text.H4("Total Comments per Author").WithLayout().Grow()
                        | Icons.UserCheck.ToIcon().Color(Colors.Neutral)
            );
    }
}

// New widgets for testing icons with text, progress bars, and layouts

public class UserEngagementWidget : ViewBase
{
    public override object? Build()
    {
        return new Card(
                content: Layout.Vertical().Gap(3)
                        | (Layout.Horizontal().Align(Align.Left).Gap(2)
                           | Icons.Users.ToIcon().Color(Colors.Blue)
                           | Text.P("1,247").Large()
                           | Text.P("Active Users").Small().Color(Colors.Gray))
                        | (Layout.Horizontal().Align(Align.Left).Gap(2)
                           | Icons.TrendingUp.ToIcon().Color(Colors.Emerald)
                           | Text.P("+12.5%").Small().Color(Colors.Emerald)
                           | Text.P("vs last month").Small().Color(Colors.Gray)),
                footer: new Progress(75).Goal("1,500 users"),
                header: Layout.Horizontal().Align(Align.Center)
                        | Text.H4("User Engagement").WithLayout().Grow()
                        | Icons.Users.ToIcon().Color(Colors.Neutral)
            );
    }
}

public class TaskCompletionWidget : ViewBase
{
    public override object? Build()
    {
        return new Card(
                content: Layout.Vertical().Gap(3)
                        | (Layout.Horizontal().Align(Align.Left).Gap(2)
                           | Icons.Check.ToIcon().Color(Colors.Emerald)
                           | Text.P("87%").Large()
                           | Text.P("Completed").Small().Color(Colors.Gray))
                        | (Layout.Horizontal().Align(Align.Left).Gap(2)
                           | Icons.Clock.ToIcon().Color(Colors.Orange)
                           | Text.P("23 tasks remaining").Small().Color(Colors.Orange)),
                footer: new Progress(87).Goal("100% completion"),
                header: Layout.Horizontal().Align(Align.Center)
                        | Text.H4("Task Progress").WithLayout().Grow()
                        | Icons.Check.ToIcon().Color(Colors.Neutral)
            );
    }
}

public class SystemHealthWidget : ViewBase
{
    public override object? Build()
    {
        return new Card(
                content: Layout.Vertical().Gap(3)
                        | (Layout.Horizontal().Align(Align.Left).Gap(2)
                           | Icons.Server.ToIcon().Color(Colors.Emerald)
                           | Text.P("99.9%").Large()
                           | Text.P("Uptime").Small().Color(Colors.Gray))
                        | (Layout.Horizontal().Align(Align.Left).Gap(2)
                           | Icons.Cpu.ToIcon().Color(Colors.Blue)
                           | Text.P("CPU: 45%").Small().Color(Colors.Blue)
                           | Icons.HardDrive.ToIcon().Color(Colors.Purple)
                           | Text.P("RAM: 67%").Small().Color(Colors.Purple)),
                footer: new Progress(99).Goal("100% uptime"),
                header: Layout.Horizontal().Align(Align.Center)
                        | Text.H4("System Health").WithLayout().Grow()
                        | Icons.Activity.ToIcon().Color(Colors.Neutral)
            );
    }
}

public class RevenueGrowthWidget : ViewBase
{
    public override object? Build()
    {
        return new Card(
                content: Layout.Vertical().Gap(3)
                        | (Layout.Horizontal().Align(Align.Left).Gap(2)
                           | Icons.DollarSign.ToIcon().Color(Colors.Emerald)
                           | Text.P("$45,230").Large()
                           | Text.P("This Month").Small().Color(Colors.Gray))
                        | (Layout.Horizontal().Align(Align.Left).Gap(2)
                           | Icons.TrendingUp.ToIcon().Color(Colors.Emerald)
                           | Text.P("+18.3%").Small().Color(Colors.Emerald)
                           | Icons.Calendar.ToIcon().Color(Colors.Blue)
                           | Text.P("vs last month").Small().Color(Colors.Blue)),
                footer: new Progress(65).Goal("$70,000 target"),
                header: Layout.Horizontal().Align(Align.Center)
                        | Text.H4("Revenue Growth").WithLayout().Grow()
                        | Icons.TrendingUp.ToIcon().Color(Colors.Neutral)
            );
    }
}

public class IconTextShowcaseWidget : ViewBase
{
    public override object? Build()
    {
        return new Card(
                content: Layout.Vertical().Gap(4)
                        | (Layout.Horizontal().Align(Align.Left).Gap(3)
                           | Icons.Heart.ToIcon().Color(Colors.Red)
                           | Text.P("Likes").Large().Color(Colors.Gray)
                           | Text.P("2,847").Large().Color(Colors.Red))
                        | (Layout.Horizontal().Align(Align.Left).Gap(3)
                           | Icons.MessageCircle.ToIcon().Color(Colors.Blue)
                           | Text.P("Comments").Large().Color(Colors.Gray)
                           | Text.P("156").Large().Color(Colors.Blue))
                        | (Layout.Horizontal().Align(Align.Left).Gap(3)
                           | Icons.Share.ToIcon().Color(Colors.Purple)
                           | Text.P("Shares").Large().Color(Colors.Gray)
                           | Text.P("89").Large().Color(Colors.Purple))
                        | (Layout.Horizontal().Align(Align.Left).Gap(3)
                           | Icons.Eye.ToIcon().Color(Colors.Orange)
                           | Text.P("Views").Large().Color(Colors.Gray)
                           | Text.P("12,456").Large().Color(Colors.Orange)),
                header: Layout.Horizontal().Align(Align.Center)
                        | Text.H4("Social Engagement").WithLayout().Grow()
                        | Icons.Star.ToIcon().Color(Colors.Neutral)
            );
    }
}

public class ProgressBarVariationsWidget : ViewBase
{
    public override object? Build()
    {
        return new Card(
                content: Layout.Vertical().Gap(4)
                        | (Layout.Vertical().Gap(1)
                           | Text.P("Low Progress").Small().Color(Colors.Gray)
                           | new Progress(25).Goal("25%"))
                        | (Layout.Vertical().Gap(1)
                           | Text.P("Medium Progress").Small().Color(Colors.Gray)
                           | new Progress(50).Goal("50%"))
                        | (Layout.Vertical().Gap(1)
                           | Text.P("High Progress").Small().Color(Colors.Gray)
                           | new Progress(85).Goal("85%"))
                        | (Layout.Vertical().Gap(1)
                           | Text.P("Overflow Progress").Small().Color(Colors.Gray)
                           | new Progress(120).Goal("100%"))
                        | (Layout.Horizontal().Align(Align.Left).Gap(2)
                           | Icons.Target.ToIcon().Color(Colors.Emerald)
                           | Text.P("Average: 70%").Small().Color(Colors.Emerald)),
                header: Layout.Horizontal().Align(Align.Center)
                        | Text.H4("Progress Variations").WithLayout().Grow()
                        | Icons.Star.ToIcon().Color(Colors.Neutral)
            );
    }
}

public class LayoutTestWidget : ViewBase
{
    public override object? Build()
    {
        return new Card(
                content: Layout.Vertical().Gap(3)
                        | (Layout.Horizontal().Align(Align.Left)
                           | (Layout.Vertical().Gap(1)
                              | Icons.Star.ToIcon().Color(Colors.Yellow)
                              | Text.P("Rating").Small().Color(Colors.Gray))
                           | (Layout.Vertical().Gap(1)
                              | Text.P("4.8").Large()
                              | Text.P("out of 5").Small().Color(Colors.Gray)))
                        | (Layout.Horizontal().Align(Align.Center).Gap(2)
                           | Icons.ThumbsUp.ToIcon().Color(Colors.Emerald)
                           | Text.P("Excellent").Large().Color(Colors.Emerald)
                           | Icons.ThumbsDown.ToIcon().Color(Colors.Red)
                           | Text.P("Poor").Large().Color(Colors.Red)),
                footer: new Progress(96).Goal("5.0 rating"),
                header: Layout.Horizontal().Align(Align.Center)
                        | Text.H4("Layout Testing").WithLayout().Grow()
                        | Icons.LayoutDashboard.ToIcon().Color(Colors.Neutral)
            );
    }
}

public class MixedContentWidget : ViewBase
{
    public override object? Build()
    {
        return new Card(
                content: Layout.Vertical().Gap(4)
                        | (Layout.Horizontal().Align(Align.Left).Gap(3)
                           | Icons.Download.ToIcon().Color(Colors.Blue)
                           | Text.P("Downloads").Large().Color(Colors.Blue)
                           | Icons.TrendingUp.ToIcon().Color(Colors.Emerald)
                           | Text.P("+25%").Small().Color(Colors.Emerald))
                        | (Layout.Grid().Columns(2).Gap(3)
                           | (Layout.Vertical().Gap(1)
                              | Icons.Smartphone.ToIcon().Color(Colors.Purple)
                              | Text.P("Mobile").Small().Color(Colors.Gray)
                              | Text.P("1,234").Large().Color(Colors.Purple))
                           | (Layout.Vertical().Gap(1)
                              | Icons.Monitor.ToIcon().Color(Colors.Blue)
                              | Text.P("Desktop").Small().Color(Colors.Gray)
                              | Text.P("856").Large().Color(Colors.Blue))),
                footer: new Progress(78).Goal("2,500 total"),
                header: Layout.Horizontal().Align(Align.Center)
                        | Text.H4("Download Analytics").WithLayout().Grow()
                        | Icons.Download.ToIcon().Color(Colors.Neutral)
            );
    }
}

public class ResponsiveLayoutWidget : ViewBase
{
    public override object? Build()
    {
        return new Card(
                content: Layout.Vertical().Gap(4)
                        | (Layout.Horizontal().Align(Align.Left)
                           | (Layout.Vertical().Gap(1)
                              | Icons.Globe.ToIcon().Color(Colors.Blue)
                              | Text.P("Global Reach").Small().Color(Colors.Gray))
                           | (Layout.Vertical().Gap(1)
                              | Text.P("47").Large()
                              | Text.P("Countries").Small().Color(Colors.Gray)))
                        | (Layout.Grid().Columns(3).Gap(2)
                           | (Layout.Vertical().Gap(1)
                              | Icons.Flag.ToIcon().Color(Colors.Red)
                              | Text.P("US").Small().Color(Colors.Gray)
                              | Text.P("35%").Large().Color(Colors.Red))
                           | (Layout.Vertical().Gap(1)
                              | Icons.Flag.ToIcon().Color(Colors.Blue)
                              | Text.P("EU").Small().Color(Colors.Gray)
                              | Text.P("28%").Large().Color(Colors.Blue))
                           | (Layout.Vertical().Gap(1)
                              | Icons.Flag.ToIcon().Color(Colors.Green)
                              | Text.P("APAC").Small().Color(Colors.Gray)
                              | Text.P("37%").Large().Color(Colors.Green))),
                footer: new Progress(85).Goal("50 countries"),
                header: Layout.Horizontal().Align(Align.Center)
                        | Text.H4("Global Distribution").WithLayout().Grow()
                        | Icons.Globe.ToIcon().Color(Colors.Neutral)
            );
    }
}

// Text spacing and padding control examples

public class TextSpacingDemoWidget : ViewBase
{
    public override object? Build()
    {
        return new Card(
                content: Layout.Vertical().Gap(2)
                        | (Layout.Horizontal().Align(Align.Left).Gap(1)
                           | Icons.Info.ToIcon().Color(Colors.Blue)
                           | Text.P("No spacing").Small().Color(Colors.Gray))
                        | (Layout.Horizontal().Align(Align.Left).Gap(1)
                           | Icons.Info.ToIcon().Color(Colors.Blue)
                           | Text.P("Minimal gaps").Small().Color(Colors.Gray))
                        | (Layout.Horizontal().Align(Align.Left).Gap(1)
                           | Icons.Info.ToIcon().Color(Colors.Blue)
                           | Text.P("Compact layout").Small().Color(Colors.Gray)),
                footer: new Progress(60).Goal("Tight spacing"),
                header: Layout.Horizontal().Align(Align.Center)
                        | Text.H4("Text Spacing Demo").WithLayout().Grow()
                        | Icons.Type.ToIcon().Color(Colors.Neutral)
            );
    }
}

public class CardPaddingOverrideWidget : ViewBase
{
    public override object? Build()
    {
        return new Card(
                content: Layout.Vertical().Gap(0)
                        | (Layout.Horizontal().Align(Align.Left).Gap(0)
                           | Icons.Zap.ToIcon().Color(Colors.Orange)
                           | Text.P("Zero Gap").Large().Color(Colors.Orange))
                        | (Layout.Horizontal().Align(Align.Left).Gap(0)
                           | Icons.Zap.ToIcon().Color(Colors.Orange)
                           | Text.P("No spacing between elements").Small().Color(Colors.Gray))
                        | (Layout.Horizontal().Align(Align.Left).Gap(0)
                           | Icons.Zap.ToIcon().Color(Colors.Orange)
                           | Text.P("Compact card content").Small().Color(Colors.Gray)),
                footer: new Progress(75).Goal("Dense layout"),
                header: Layout.Horizontal().Align(Align.Center)
                        | Text.H4("Zero Spacing").WithLayout().Grow()
                        | Icons.Zap.ToIcon().Color(Colors.Neutral)
            );
    }
}

public class LayoutSpacingControlWidget : ViewBase
{
    public override object? Build()
    {
        return new Card(
                content: Layout.Vertical().Gap(1)
                        | (Layout.Horizontal().Align(Align.Left).Gap(1)
                           | Icons.Settings.ToIcon().Color(Colors.Purple)
                           | Text.P("Custom spacing").Small().Color(Colors.Gray))
                        | (Layout.Horizontal().Align(Align.Left).Gap(1)
                           | Icons.Settings.ToIcon().Color(Colors.Purple)
                           | Text.P("Controlled gaps").Small().Color(Colors.Gray))
                        | (Layout.Horizontal().Align(Align.Left).Gap(1)
                           | Icons.Settings.ToIcon().Color(Colors.Purple)
                           | Text.P("Precise layout").Small().Color(Colors.Gray)),
                footer: new Progress(90).Goal("Custom control"),
                header: Layout.Horizontal().Align(Align.Center)
                        | Text.H4("Spacing Control").WithLayout().Grow()
                        | Icons.Settings.ToIcon().Color(Colors.Neutral)
            );
    }
}

// Size variant metric views
public class SmallMetricView : ViewBase
{
    public override object? Build()
    {
        return new Card(
                content: Layout.Horizontal().Align(Align.Left).Gap(1)
                         | Text.Strong("$12.5K")
                         | Icons.TrendingUp.ToIcon().Color(Colors.Emerald)
                         | Text.P("+5%").Small().Color(Colors.Emerald),
                footer: new Progress(25).Goal("$50K target"),
                header: Layout.Horizontal().Align(Align.Center)
                        | Text.H4("Small Revenue").WithLayout().Grow()
                        | Icons.DollarSign.ToIcon().Color(Colors.Neutral)
            ).Small();
    }
}

public class MediumMetricView : ViewBase
{
    public override object? Build()
    {
        return new Card(
                content: Layout.Horizontal().Align(Align.Left).Gap(2)
                         | Text.Strong("$84,250")
                         | Icons.TrendingUp.ToIcon().Color(Colors.Emerald)
                         | Text.P("21%").Small().Color(Colors.Emerald),
                footer: new Progress(21).Goal("$400K target"),
                header: Layout.Horizontal().Align(Align.Center)
                        | Text.H4("Medium Revenue").WithLayout().Grow()
                        | Icons.DollarSign.ToIcon().Color(Colors.Neutral)
            ).Medium();
    }
}

public class LargeMetricView : ViewBase
{
    public override object? Build()
    {
        return new Card(
                content: Layout.Horizontal().Align(Align.Left).Gap(3)
                         | Text.P("$1,234,567").Large()
                         | Icons.TrendingUp.ToIcon().Color(Colors.Emerald)
                         | Text.P("+45%").Color(Colors.Emerald),
                footer: new Progress(75).Goal("$1.5M target"),
                header: Layout.Horizontal().Align(Align.Center)
                        | Text.H4("Large Revenue").WithLayout().Grow()
                        | Icons.DollarSign.ToIcon().Color(Colors.Neutral)
            ).Large();
    }
}