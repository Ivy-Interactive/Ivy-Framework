namespace Ivy.Samples.Shared.Apps.Tests;

/// <summary>
/// Reproduces radar chart axis label cropping at the 12 o'clock (top) position.
/// The top indicator name is clipped — compare against side/bottom labels which render fully.
/// </summary>
[App(
    icon: Icons.Radar,
    group: ["Tests"],
    isVisible: false,
    searchHints: ["radar", "chart", "label", "crop", "clip", "overflow", "axis"]
)]
public class RadarChartLabelCropTestApp : SampleBase
{
    private static readonly object[] SkillData =
    [
        new
        {
            name = "Candidate",
            Technical = 90,
            Communication = 75,
            Leadership = 80,
            ProblemSolving = 85,
            Teamwork = 88,
        },
    ];

    private static readonly DepartmentRow[] DepartmentData =
    [
        new("Engineering", 85, 92, 78, 88, 80),
        new("Marketing", 70, 75, 90, 85, 72),
        new("Sales", 90, 68, 65, 92, 95),
    ];

    private record DepartmentRow(
        string Department,
        int Speed,
        int Quality,
        int Innovation,
        int Collaboration,
        int Delivery
    );

    protected override object? BuildSample()
    {
        return Layout.Vertical()
            | Text.H1("Radar Chart — Top Label Crop")
            | Text.P(
                "Bug: the indicator at 12 o'clock is clipped at the top edge. "
                + "Side and bottom labels should render fully for comparison."
            )
            | Text.H2("Minimal — single series, top axis = \"Technical\"")
            | Layout.Grid().Columns(2)
                | ChartInCard("In Card (typical)", BuildSkillChart().Legend())
                | ChartBare("No Card", BuildSkillChart().Legend())
            | Text.H2("Top label length — long name at 12 o'clock")
            | Layout.Grid().Columns(2)
                | ChartInCard(
                    "Long top label (EaseOfUse)",
                    BuildScorecardChart().Legend().Toolbox()
                )
                | ChartInCard(
                    "Long top label + circle shape",
                    BuildScorecardChart().Shape(RadarShape.Circle).Legend()
                )
            | Text.H2("Legend shifts center — top label still crops")
            | Layout.Grid().Columns(3)
                | ChartInCard("No legend", BuildSkillChart())
                | ChartInCard("With legend", BuildSkillChart().Legend())
                | ChartInCard("Legend + toolbox", BuildSkillChart().Legend().Toolbox())
            | Text.H2("ToRadarChart — Speed is top axis (startAngle 90°)")
            | Layout.Grid().Columns(3)
                | ChartInCard(
                    "ToRadarChart Default",
                    DepartmentData.ToRadarChart(
                        x => x.Department,
                        [
                            q => q.Sum(x => x.Speed),
                            q => q.Sum(x => x.Quality),
                            q => q.Sum(x => x.Innovation),
                            q => q.Sum(x => x.Collaboration),
                            q => q.Sum(x => x.Delivery),
                        ]
                    )
                )
                | ChartInCard(
                    "ToRadarChart Circle",
                    DepartmentData.ToRadarChart(
                        x => x.Department,
                        [
                            q => q.Sum(x => x.Speed),
                            q => q.Sum(x => x.Quality),
                            q => q.Sum(x => x.Innovation),
                            q => q.Sum(x => x.Collaboration),
                            q => q.Sum(x => x.Delivery),
                        ],
                        RadarChartStyles.Circle
                    )
                )
                | ChartInCard(
                    "ToRadarChart Dashboard",
                    DepartmentData.ToRadarChart(
                        x => x.Department,
                        [
                            q => q.Sum(x => x.Speed),
                            q => q.Sum(x => x.Quality),
                            q => q.Sum(x => x.Innovation),
                            q => q.Sum(x => x.Collaboration),
                            q => q.Sum(x => x.Delivery),
                        ],
                        RadarChartStyles.Dashboard,
                        polish: chart => chart.Toolbox()
                    )
                )
            | Text.H2("Fixed height — rules out flex stretch only")
            | ChartInCard(
                "Height 320px",
                BuildSkillChart()
                    .Legend()
                    .Height(Size.Px(320))
            );
    }

    private static RadarChart BuildSkillChart() =>
        new RadarChart(SkillData)
            .Indicator("Technical", 100)
            .Indicator("Communication", 100)
            .Indicator("Leadership", 100)
            .Indicator("ProblemSolving", 100)
            .Indicator("Teamwork", 100)
            .Radar("values")
            .Tooltip();

    private static RadarChart BuildScorecardChart()
    {
        var data = new[]
        {
            new { name = "Scores", EaseOfUse = 78, Performance = 85, Reliability = 92, Features = 70, Support = 88 },
        };

        return new RadarChart(data)
            .Indicator("EaseOfUse", 100)
            .Indicator("Performance", 100)
            .Indicator("Reliability", 100)
            .Indicator("Features", 100)
            .Indicator("Support", 100)
            .Radar("values")
            .Tooltip();
    }

    private static object ChartInCard(string title, object chart) =>
        new Card().Title(title).Height(Size.Units(80)) | chart;

    private static object ChartBare(string title, RadarChart chart) =>
        Layout.Vertical()
            | Text.Label(title)
            | chart.Height(Size.Units(80));
}
