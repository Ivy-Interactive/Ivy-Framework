using Ivy.Widgets.ActivityHeatmap;

namespace Ivy.Test;

public class ActivityHeatmapTests
{
    [Fact]
    public void BuildGrid_EmptyData_Returns52OrMoreWeeks()
    {
        var weeks = ActivityHeatmapGrid.BuildGrid([]);

        Assert.True(weeks.Length >= 52);
        Assert.All(weeks, week => Assert.Equal(7, week.Length));
    }

    [Fact]
    public void BuildGrid_StartsOnSunday()
    {
        var data = new[]
        {
            new ContributionDay { Date = new DateOnly(2024, 3, 13), Count = 5 }, // Wednesday
        };

        var weeks = ActivityHeatmapGrid.BuildGrid(data);

        // First day must be a Sunday
        Assert.Equal(DayOfWeek.Sunday, weeks[0][0].Date.DayOfWeek);
    }

    [Fact]
    public void BuildGrid_EndsOnSaturday()
    {
        var data = new[]
        {
            new ContributionDay { Date = new DateOnly(2024, 3, 13), Count = 5 }, // Wednesday
        };

        var weeks = ActivityHeatmapGrid.BuildGrid(data);

        var lastWeek = weeks[^1];
        Assert.Equal(DayOfWeek.Saturday, lastWeek[6].Date.DayOfWeek);
    }

    [Fact]
    public void BuildGrid_MissingDaysAreZero()
    {
        // Provide only one day in the middle of a week
        var data = new[]
        {
            new ContributionDay { Date = new DateOnly(2024, 3, 13), Count = 5 }, // Wednesday
        };

        var weeks = ActivityHeatmapGrid.BuildGrid(data);

        // All days except the provided one should have Count = 0
        var allDays = weeks.SelectMany(w => w).ToList();
        var nonZero = allDays.Where(d => d.Count > 0).ToList();
        Assert.Single(nonZero);
        Assert.Equal(new DateOnly(2024, 3, 13), nonZero[0].Date);
        Assert.Equal(5, nonZero[0].Count);
    }

    [Fact]
    public void BuildGrid_MultipleWeeks_CorrectDimensions()
    {
        // Data spanning 3 weeks
        var data = new[]
        {
            new ContributionDay { Date = new DateOnly(2024, 1, 1), Count = 1 },
            new ContributionDay { Date = new DateOnly(2024, 1, 21), Count = 2 },
        };

        var weeks = ActivityHeatmapGrid.BuildGrid(data);

        Assert.True(weeks.Length >= 4);
        Assert.All(weeks, week => Assert.Equal(7, week.Length));
    }

    [Fact]
    public void GetLevel_Zero_ReturnsZero()
    {
        Assert.Equal(0, ActivityHeatmapGrid.GetLevel(0, 10));
    }

    [Fact]
    public void GetLevel_MaxZero_ReturnsZero()
    {
        Assert.Equal(0, ActivityHeatmapGrid.GetLevel(5, 0));
    }

    [Fact]
    public void GetLevel_QuartileBands_CorrectLevels()
    {
        int max = 100;

        Assert.Equal(1, ActivityHeatmapGrid.GetLevel(1, max));    // ≤ 25%
        Assert.Equal(1, ActivityHeatmapGrid.GetLevel(25, max));   // = 25%
        Assert.Equal(2, ActivityHeatmapGrid.GetLevel(26, max));   // > 25%, ≤ 50%
        Assert.Equal(2, ActivityHeatmapGrid.GetLevel(50, max));   // = 50%
        Assert.Equal(3, ActivityHeatmapGrid.GetLevel(51, max));   // > 50%, ≤ 75%
        Assert.Equal(3, ActivityHeatmapGrid.GetLevel(75, max));   // = 75%
        Assert.Equal(4, ActivityHeatmapGrid.GetLevel(76, max));   // > 75%
        Assert.Equal(4, ActivityHeatmapGrid.GetLevel(100, max));  // = max
    }

    [Fact]
    public void BuildGrid_AllDaysPresent_CountsPreserved()
    {
        // Provide data for Mon–Sat of one week (stays within a single Sunday-start week)
        var monday = new DateOnly(2024, 3, 11); // Monday, DayOfWeek = 1
        var data = Enumerable.Range(0, 6) // Mon through Sat
            .Select(i => new ContributionDay { Date = monday.AddDays(i), Count = i + 1 })
            .ToArray();

        var weeks = ActivityHeatmapGrid.BuildGrid(data);

        // Find the week whose Sunday precedes Monday March 11 (i.e. Sunday March 10)
        var targetWeek = weeks.FirstOrDefault(w => w[0].Date == new DateOnly(2024, 3, 10));
        Assert.NotNull(targetWeek);
        // Sunday (index 0) has Count=0 (not in data)
        Assert.Equal(0, targetWeek[0].Count);
        // Monday–Saturday (indices 1–6) have counts 1–6
        for (int i = 0; i < 6; i++)
        {
            Assert.Equal(i + 1, targetWeek[1 + i].Count);
        }
    }
}
