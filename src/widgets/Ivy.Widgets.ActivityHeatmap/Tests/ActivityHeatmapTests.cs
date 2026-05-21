using Ivy.Widgets.ActivityHeatmap;

namespace Ivy.Widgets.ActivityHeatmap.Tests;

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
            new Activity { Date = new DateOnly(2024, 3, 13), Count = 5 }, // Wednesday
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
            new Activity { Date = new DateOnly(2024, 3, 13), Count = 5 }, // Wednesday
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
            new Activity { Date = new DateOnly(2024, 3, 13), Count = 5 }, // Wednesday
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
            new Activity { Date = new DateOnly(2024, 1, 1), Count = 1 },
            new Activity { Date = new DateOnly(2024, 1, 21), Count = 2 },
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
            .Select(i => new Activity { Date = monday.AddDays(i), Count = i + 1 })
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

    [Fact]
    public void BuildGrid_WithStartDateOnly_ClampsToStartDate()
    {
        // Data starts on 2024-03-13 (Wednesday), but startDate overrides to an earlier date
        var startDate = new DateOnly(2024, 3, 1); // Friday
        var data = new[]
        {
            new Activity { Date = new DateOnly(2024, 3, 13), Count = 5 },
        };

        var weeks = ActivityHeatmapGrid.BuildGrid(data, startDate: startDate);

        // Grid must start at the Sunday on or before 2024-03-01 (which is 2024-02-25)
        Assert.Equal(DayOfWeek.Sunday, weeks[0][0].Date.DayOfWeek);
        Assert.True(weeks[0][0].Date <= startDate);
    }

    [Fact]
    public void BuildGrid_WithEndDateOnly_ClampsToEndDate()
    {
        // Data ends on 2024-03-13, but endDate overrides to a later date
        var endDate = new DateOnly(2024, 3, 31); // Sunday
        var data = new[]
        {
            new Activity { Date = new DateOnly(2024, 3, 13), Count = 5 },
        };

        var weeks = ActivityHeatmapGrid.BuildGrid(data, endDate: endDate);

        var lastWeek = weeks[^1];
        // Grid must end on Saturday on or after 2024-03-31 (which is 2024-04-06)
        Assert.Equal(DayOfWeek.Saturday, lastWeek[6].Date.DayOfWeek);
        Assert.True(lastWeek[6].Date >= endDate);
    }

    [Fact]
    public void BuildGrid_WithBothDates_IgnoresDataRange()
    {
        // Data spans Jan 2024, but we pin to March 2024
        var startDate = new DateOnly(2024, 3, 1);
        var endDate = new DateOnly(2024, 3, 31);
        var data = new[]
        {
            new Activity { Date = new DateOnly(2024, 1, 5), Count = 3 },
            new Activity { Date = new DateOnly(2024, 1, 20), Count = 7 },
        };

        var weeks = ActivityHeatmapGrid.BuildGrid(data, startDate: startDate, endDate: endDate);

        // Grid should be anchored to March 2024 range
        Assert.Equal(DayOfWeek.Sunday, weeks[0][0].Date.DayOfWeek);
        Assert.True(weeks[0][0].Date <= startDate);
        Assert.Equal(DayOfWeek.Saturday, weeks[^1][6].Date.DayOfWeek);
        Assert.True(weeks[^1][6].Date >= endDate);

        // Grid must NOT extend back to January
        Assert.True(weeks[0][0].Date >= new DateOnly(2024, 2, 1));
    }

    [Fact]
    public void BuildGrid_52WeekWindow_HasCorrectWeekCount()
    {
        var endDate = new DateOnly(2024, 3, 13);
        var startDate = endDate.AddDays(-364);

        var weeks = ActivityHeatmapGrid.BuildGrid([], startDate: startDate, endDate: endDate);

        // 365 days = 52 weeks + 1 day; padded to week boundaries gives 53 weeks
        Assert.True(weeks.Length >= 52);
        Assert.True(weeks.Length <= 54);
        Assert.All(weeks, week => Assert.Equal(7, week.Length));
    }

    [Fact]
    public void BuildGrid_InvertedStartEnd_NormalizesToChronologicalRange()
    {
        var startDate = new DateOnly(2024, 3, 31);
        var endDate = new DateOnly(2024, 3, 1);
        var data = new[]
        {
            new Activity { Date = new DateOnly(2024, 3, 15), Count = 5 },
        };

        var weeks = ActivityHeatmapGrid.BuildGrid(data, startDate: startDate, endDate: endDate);

        Assert.NotEmpty(weeks);
        Assert.All(weeks, week => Assert.Equal(7, week.Length));
        Assert.Equal(DayOfWeek.Sunday, weeks[0][0].Date.DayOfWeek);
        Assert.Equal(DayOfWeek.Saturday, weeks[^1][6].Date.DayOfWeek);
        Assert.True(weeks[0][0].Date <= endDate);
        Assert.True(weeks[^1][6].Date >= startDate);
        var midWeek = weeks.SelectMany(w => w).Single(d => d.Date == new DateOnly(2024, 3, 15));
        Assert.Equal(5, midWeek.Count);
    }

    [Fact]
    public void BuildGridFromRange_InvertedFirstLast_SwapsToOrderedRange()
    {
        var first = new DateOnly(2024, 3, 20);
        var last = new DateOnly(2024, 3, 1);
        var weeks = ActivityHeatmapGrid.BuildGridFromRange([], first, last);

        Assert.NotEmpty(weeks);
        Assert.True(weeks[0][0].Date <= last);
        Assert.True(weeks[^1][6].Date >= first);
    }

    [Fact]
    public void ToDateOnly_DateOnly_ReturnsAsIs()
    {
        var expected = new DateOnly(2024, 6, 15);
        var result = ActivityHeatmapBuilder<object>.ToDateOnly(expected);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToDateOnly_DateTime_ConvertsCorrectly()
    {
        var dt = new DateTime(2024, 6, 15, 10, 30, 0);
        var result = ActivityHeatmapBuilder<object>.ToDateOnly(dt);
        Assert.Equal(new DateOnly(2024, 6, 15), result);
    }

    [Fact]
    public void ToDateOnly_String_ParsesCorrectly()
    {
        var result = ActivityHeatmapBuilder<object>.ToDateOnly("2024-06-15");
        Assert.Equal(new DateOnly(2024, 6, 15), result);
    }
}
