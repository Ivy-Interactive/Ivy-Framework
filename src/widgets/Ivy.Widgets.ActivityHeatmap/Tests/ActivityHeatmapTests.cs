using Ivy.Widgets.ActivityHeatmap;

namespace Ivy.Widgets.ActivityHeatmap.Tests;

public class ActivityHeatmapTests
{
    [Fact]
    public void ToHour_DateTime_ReturnsHour()
    {
        Assert.Equal(14, ActivityHeatmapBuilder<object>.ToHour(new DateTime(2024, 6, 15, 14, 30, 0)));
    }

    [Fact]
    public void ToHour_NonTemporal_ReturnsNull()
    {
        Assert.Null(ActivityHeatmapBuilder<object>.ToHour(42));
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

    [Fact]
    public void ToDateOnly_DateTime_SameDaySumsCorrectly()
    {
        // Simulate the grouping logic: two DateTime timestamps on the same day
        // should produce a single Activity with summed Count.
        var dt1 = new DateTime(2024, 6, 15, 9, 0, 0);
        var dt2 = new DateTime(2024, 6, 15, 17, 30, 0);

        var rows = new[]
        {
            new Activity { Date = ActivityHeatmapBuilder<object>.ToDateOnly(dt1), Count = 3 },
            new Activity { Date = ActivityHeatmapBuilder<object>.ToDateOnly(dt2), Count = 7 },
        };

        var merged = rows
            .GroupBy(a => a.Date)
            .Select(g => new Activity { Date = g.Key, Count = g.Sum(a => a.Count) })
            .ToArray();

        Assert.Single(merged);
        Assert.Equal(new DateOnly(2024, 6, 15), merged[0].Date);
        Assert.Equal(10, merged[0].Count);
    }
}
