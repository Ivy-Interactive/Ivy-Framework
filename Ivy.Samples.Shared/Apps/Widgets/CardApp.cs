using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.IdCard, path: ["Widgets"], searchHints: ["container", "panel", "box", "section", "wrapper", "border"])]
public class CardApp : SampleBase
{
    protected override object? BuildSample()
    {
        var client = UseService<IClientProvider>();

        var card1 = new Card(
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec nunc",
            new Button("Sign Me Up", _ => client.Toast("You have signed up!")).TestId("card-app-signup-button")
        ).Title("Card App").Description("This is a card app.").TestId("card-app");

        var card2 = new Card(
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec nunc"
        ).Title("Card with Border")
         .Description("This card has a custom border.")
         .BorderThickness(3)
         .BorderStyle(BorderStyle.Dashed)
         .BorderColor(Colors.Primary)
         .BorderRadius(BorderRadius.Rounded)
         .TestId("card-border");

        var card3 = new Card(
            "This card demonstrates the border color fix with a thick red border."
        ).Title("Border Color Test")
         .Description("Should now display with a red border")
         .BorderThickness(4)
         .BorderStyle(BorderStyle.Solid)
         .BorderColor(Colors.Red)
         .BorderRadius(BorderRadius.Rounded)
         .TestId("card-border-color");

        var card4 = new Card(
           "This card demonstrates OnClick handlers."
       ).Title("OnClick test")
        .Description("Click me!")
        .TestId("card-onclick")
        .HandleClick(_ =>
        {
            client.Toast("Clicked!");
        });

        return Layout.Vertical()
               | Text.H1("Card")
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
                (Layout.Horizontal().Align(Align.Left).Gap(2)
                 | Text.H4("$84,250")
                 | Icons.TrendingUp.ToIcon().Color(Colors.Emerald)
                 | Text.Small("21%").Color(Colors.Emerald)),
                new Progress(21).Goal(800_000.ToString("C0"))
            ).Title(title).Icon(Icons.DollarSign).TestId("card-total-sales")
            ;
    }
}

public class LongNumberMetricView : ViewBase
{
    public override object? Build()
    {
        return new Card(
                (Layout.Horizontal().Align(Align.Left).Gap(2)
                 | Text.H4("$123,456,789.99")
                 | Icons.TrendingUp.ToIcon().Color(Colors.Emerald)
                 | Text.Small("1,234.5%").Color(Colors.Emerald)),
                new Progress(85).Goal("$100,000,000")
            ).Title("Very Long Revenue Number").Icon(Icons.DollarSign)
            ;
    }
}

public class HighPercentageMetricView : ViewBase
{
    public override object? Build()
    {
        return new Card(
                (Layout.Horizontal().Align(Align.Left).Gap(2)
                 | Text.H4("1,012.50%")
                 | Icons.TrendingUp.ToIcon().Color(Colors.Emerald)
                 | Text.Small("38.1%").Color(Colors.Emerald)),
                new Progress(125).Goal("806.67%")
            ).Title("Post Engagement Rate").Icon(Icons.Activity)
            ;
    }
}

public class VeryLongTitleMetricView : ViewBase
{
    public override object? Build()
    {
        return new Card(
                (Layout.Horizontal().Align(Align.Left).Gap(2)
                 | Text.H4("2.25")
                 | Icons.TrendingUp.ToIcon().Color(Colors.Emerald)
                 | Text.Small("38.1%").Color(Colors.Emerald)),
                new Progress(90).Goal("2.50")
            ).Title("Total Comments per Author in This Period").Icon(Icons.MessageCircle)
            ;
    }
}

public class TotalCommentsPerAuthorMetricView : ViewBase
{
    public override object? Build()
    {
        return new Card(
                (Layout.Horizontal().Align(Align.Left).Gap(2)
                 | Text.H4("2.25")
                 | Icons.TrendingUp.ToIcon().Color(Colors.Emerald)
                 | Text.Small("38.1%").Color(Colors.Emerald)),
                new Progress(90).Goal("2.50")
            ).Title("Total Comments per Author").Icon(Icons.UserCheck)
            ;
    }
}

// New widgets for testing icons with text, progress bars, and layouts

public class UserEngagementWidget : ViewBase
{
    public override object? Build()
    {
        return new Card(
                Layout.Vertical().Gap(3)
                | (Layout.Horizontal().Align(Align.Left).Gap(2)
                   | Icons.Users.ToIcon().Color(Colors.Blue)
                   | Text.H4("1,247")
                   | Text.Small("Active Users").Color(Colors.Gray))
                | (Layout.Horizontal().Align(Align.Left).Gap(2)
                   | Icons.TrendingUp.ToIcon().Color(Colors.Emerald)
                   | Text.Small("+12.5%").Color(Colors.Emerald)
                   | Text.Small("vs last month").Color(Colors.Gray))
                | new Progress(75).Goal("1,500 users")
            ).Title("User Engagement").Icon(Icons.Users)
            ;
    }
}

public class TaskCompletionWidget : ViewBase
{
    public override object? Build()
    {
        return new Card(
                Layout.Vertical().Gap(3)
                | (Layout.Horizontal().Align(Align.Left).Gap(2)
                   | Icons.Check.ToIcon().Color(Colors.Emerald)
                   | Text.H4("87%")
                   | Text.Small("Completed").Color(Colors.Gray))
                | (Layout.Horizontal().Align(Align.Left).Gap(2)
                   | Icons.Clock.ToIcon().Color(Colors.Orange)
                   | Text.Small("23 tasks remaining").Color(Colors.Orange))
                | new Progress(87).Goal("100% completion")
            ).Title("Task Progress").Icon(Icons.Check)
            ;
    }
}

public class SystemHealthWidget : ViewBase
{
    public override object? Build()
    {
        return new Card(
                Layout.Vertical().Gap(3)
                | (Layout.Horizontal().Align(Align.Left).Gap(2)
                   | Icons.Server.ToIcon().Color(Colors.Emerald)
                   | Text.H4("99.9%")
                   | Text.Small("Uptime").Color(Colors.Gray))
                | (Layout.Horizontal().Align(Align.Left).Gap(2)
                   | Icons.Cpu.ToIcon().Color(Colors.Blue)
                   | Text.Small("CPU: 45%").Color(Colors.Blue)
                   | Icons.HardDrive.ToIcon().Color(Colors.Purple)
                   | Text.Small("RAM: 67%").Color(Colors.Purple))
                | new Progress(99).Goal("100% uptime")
            ).Title("System Health").Icon(Icons.Activity)
            ;
    }
}

public class RevenueGrowthWidget : ViewBase
{
    public override object? Build()
    {
        return new Card(
                Layout.Vertical().Gap(3)
                | (Layout.Horizontal().Align(Align.Left).Gap(2)
                   | Icons.DollarSign.ToIcon().Color(Colors.Emerald)
                   | Text.H4("$45,230")
                   | Text.Small("This Month").Color(Colors.Gray))
                | (Layout.Horizontal().Align(Align.Left).Gap(2)
                   | Icons.TrendingUp.ToIcon().Color(Colors.Emerald)
                   | Text.Small("+18.3%").Color(Colors.Emerald)
                   | Icons.Calendar.ToIcon().Color(Colors.Blue)
                   | Text.Small("vs last month").Color(Colors.Blue))
                | new Progress(65).Goal("$70,000 target")
            ).Title("Revenue Growth").Icon(Icons.TrendingUp)
            ;
    }
}

public class IconTextShowcaseWidget : ViewBase
{
    public override object? Build()
    {
        return new Card(
                Layout.Vertical().Gap(4)
                | (Layout.Horizontal().Align(Align.Left).Gap(3)
                   | Icons.Heart.ToIcon().Color(Colors.Red)
                   | Text.Large("Likes").Color(Colors.Gray)
                   | Text.H4("2,847").Color(Colors.Red))
                | (Layout.Horizontal().Align(Align.Left).Gap(3)
                   | Icons.MessageCircle.ToIcon().Color(Colors.Blue)
                   | Text.Large("Comments").Color(Colors.Gray)
                   | Text.H4("156").Color(Colors.Blue))
                | (Layout.Horizontal().Align(Align.Left).Gap(3)
                   | Icons.Share.ToIcon().Color(Colors.Purple)
                   | Text.Large("Shares").Color(Colors.Gray)
                   | Text.H4("89").Color(Colors.Purple))
                | (Layout.Horizontal().Align(Align.Left).Gap(3)
                   | Icons.Eye.ToIcon().Color(Colors.Orange)
                   | Text.Large("Views").Color(Colors.Gray)
                   | Text.H4("12,456").Color(Colors.Orange))
            ).Title("Social Engagement").Icon(Icons.Star)
            ;
    }
}

public class ProgressBarVariationsWidget : ViewBase
{
    public override object? Build()
    {
        return new Card(
                Layout.Vertical().Gap(4)
                | (Layout.Vertical().Gap(1)
                   | Text.Small("Low Progress").Color(Colors.Gray)
                   | new Progress(25).Goal("25%"))
                | (Layout.Vertical().Gap(1)
                   | Text.Small("Medium Progress").Color(Colors.Gray)
                   | new Progress(50).Goal("50%"))
                | (Layout.Vertical().Gap(1)
                   | Text.Small("High Progress").Color(Colors.Gray)
                   | new Progress(85).Goal("85%"))
                | (Layout.Vertical().Gap(1)
                   | Text.Small("Overflow Progress").Color(Colors.Gray)
                   | new Progress(120).Goal("100%"))
                | (Layout.Horizontal().Align(Align.Left).Gap(2)
                   | Icons.Target.ToIcon().Color(Colors.Emerald)
                   | Text.Small("Average: 70%").Color(Colors.Emerald))
            ).Title("Progress Variations").Icon(Icons.Star)
            ;
    }
}

public class LayoutTestWidget : ViewBase
{
    public override object? Build()
    {
        return new Card(
                Layout.Vertical().Gap(3)
                | (Layout.Horizontal().Align(Align.Left)
                   | (Layout.Vertical().Gap(1)
                      | Icons.Star.ToIcon().Color(Colors.Yellow)
                      | Text.Small("Rating").Color(Colors.Gray))
                   | (Layout.Vertical().Gap(1)
                      | Text.H4("4.8")
                      | Text.Small("out of 5").Color(Colors.Gray)))
                | (Layout.Horizontal().Align(Align.Center).Gap(2)
                   | Icons.ThumbsUp.ToIcon().Color(Colors.Emerald)
                   | Text.Large("Excellent").Color(Colors.Emerald)
                   | Icons.ThumbsDown.ToIcon().Color(Colors.Red)
                   | Text.Large("Poor").Color(Colors.Red))
                | new Progress(96).Goal("5.0 rating")
            ).Title("Layout Testing").Icon(Icons.LayoutDashboard)
            ;
    }
}

public class MixedContentWidget : ViewBase
{
    public override object? Build()
    {
        return new Card(
                Layout.Vertical().Gap(4)
                | (Layout.Horizontal().Align(Align.Left).Gap(3)
                   | Icons.Download.ToIcon().Color(Colors.Blue)
                   | Text.H4("Downloads").Color(Colors.Blue)
                   | Icons.TrendingUp.ToIcon().Color(Colors.Emerald)
                   | Text.Small("+25%").Color(Colors.Emerald))
                | (Layout.Grid().Columns(2).Gap(3)
                   | (Layout.Vertical().Gap(1)
                      | Icons.Smartphone.ToIcon().Color(Colors.Purple)
                      | Text.Small("Mobile").Color(Colors.Gray)
                      | Text.H4("1,234").Color(Colors.Purple))
                   | (Layout.Vertical().Gap(1)
                      | Icons.Monitor.ToIcon().Color(Colors.Blue)
                      | Text.Small("Desktop").Color(Colors.Gray)
                      | Text.H4("856").Color(Colors.Blue)))
                | new Progress(78).Goal("2,500 total")
            ).Title("Download Analytics").Icon(Icons.Download)
            ;
    }
}

public class ResponsiveLayoutWidget : ViewBase
{
    public override object? Build()
    {
        return new Card(
                Layout.Vertical().Gap(4)
                | (Layout.Horizontal().Align(Align.Left)
                   | (Layout.Vertical().Gap(1)
                      | Icons.Globe.ToIcon().Color(Colors.Blue)
                      | Text.Small("Global Reach").Color(Colors.Gray))
                   | (Layout.Vertical().Gap(1)
                      | Text.H4("47")
                      | Text.Small("Countries").Color(Colors.Gray)))
                | (Layout.Grid().Columns(3).Gap(2)
                   | (Layout.Vertical().Gap(1)
                      | Icons.Flag.ToIcon().Color(Colors.Red)
                      | Text.Small("US").Color(Colors.Gray)
                      | Text.Large("35%").Color(Colors.Red))
                   | (Layout.Vertical().Gap(1)
                      | Icons.Flag.ToIcon().Color(Colors.Blue)
                      | Text.Small("EU").Color(Colors.Gray)
                      | Text.Large("28%").Color(Colors.Blue))
                   | (Layout.Vertical().Gap(1)
                      | Icons.Flag.ToIcon().Color(Colors.Green)
                      | Text.Small("APAC").Color(Colors.Gray)
                      | Text.Large("37%").Color(Colors.Green)))
                | new Progress(85).Goal("50 countries")
            ).Title("Global Distribution").Icon(Icons.Globe)
            ;
    }
}

// Text spacing and padding control examples

public class TextSpacingDemoWidget : ViewBase
{
    public override object? Build()
    {
        return new Card(
                Layout.Vertical().Gap(2)
                | (Layout.Horizontal().Align(Align.Left).Gap(1)
                   | Icons.Info.ToIcon().Color(Colors.Blue)
                   | Text.Small("No spacing").Color(Colors.Gray))
                | (Layout.Horizontal().Align(Align.Left).Gap(1)
                   | Icons.Info.ToIcon().Color(Colors.Blue)
                   | Text.Small("Minimal gaps").Color(Colors.Gray))
                | (Layout.Horizontal().Align(Align.Left).Gap(1)
                   | Icons.Info.ToIcon().Color(Colors.Blue)
                   | Text.Small("Compact layout").Color(Colors.Gray))
                | new Progress(60).Goal("Tight spacing")
            ).Title("Text Spacing Demo").Icon(Icons.Type)
            ;
    }
}

public class CardPaddingOverrideWidget : ViewBase
{
    public override object? Build()
    {
        return new Card(
                Layout.Vertical().Gap(0)
                | (Layout.Horizontal().Align(Align.Left).Gap(0)
                   | Icons.Zap.ToIcon().Color(Colors.Orange)
                   | Text.H4("Zero Gap").Color(Colors.Orange))
                | (Layout.Horizontal().Align(Align.Left).Gap(0)
                   | Icons.Zap.ToIcon().Color(Colors.Orange)
                   | Text.Small("No spacing between elements").Color(Colors.Gray))
                | (Layout.Horizontal().Align(Align.Left).Gap(0)
                   | Icons.Zap.ToIcon().Color(Colors.Orange)
                   | Text.Small("Compact card content").Color(Colors.Gray))
                | new Progress(75).Goal("Dense layout")
            ).Title("Zero Spacing").Icon(Icons.Zap)
            ;
    }
}

public class LayoutSpacingControlWidget : ViewBase
{
    public override object? Build()
    {
        return new Card(
                Layout.Vertical().Gap(1)
                | (Layout.Horizontal().Align(Align.Left).Gap(1)
                   | Icons.Settings.ToIcon().Color(Colors.Purple)
                   | Text.Small("Custom spacing").Color(Colors.Gray))
                | (Layout.Horizontal().Align(Align.Left).Gap(1)
                   | Icons.Settings.ToIcon().Color(Colors.Purple)
                   | Text.Small("Controlled gaps").Color(Colors.Gray))
                | (Layout.Horizontal().Align(Align.Left).Gap(1)
                   | Icons.Settings.ToIcon().Color(Colors.Purple)
                   | Text.Small("Precise layout").Color(Colors.Gray))
                | new Progress(90).Goal("Custom control")
            ).Title("Spacing Control").Icon(Icons.Settings)
            ;
    }
}