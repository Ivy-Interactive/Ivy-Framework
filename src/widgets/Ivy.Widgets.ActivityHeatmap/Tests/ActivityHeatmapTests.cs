using Ivy;
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
    public void TruncateDimensionValue_DateTime_Daily_StripsTime()
    {
        var dt = new DateTime(2024, 6, 15, 17, 30, 45);
        var result = ActivityHeatmapBuilder<object>.TruncateDimensionValue(dt, ActivityInterval.Daily);
        Assert.Equal(new DateTime(2024, 6, 15, 0, 0, 0), result);
    }

    [Fact]
    public void TruncateDimensionValue_DateTime_Hourly_StripsMinutesAndSeconds()
    {
        var dt = new DateTime(2024, 6, 15, 17, 30, 45);
        var result = ActivityHeatmapBuilder<object>.TruncateDimensionValue(dt, ActivityInterval.Hourly);
        Assert.Equal(new DateTime(2024, 6, 15, 17, 0, 0), result);
    }

    [Fact]
    public void TruncateDimensionValue_String_Hourly_ParsesAndTruncates()
    {
        var result = ActivityHeatmapBuilder<object>.TruncateDimensionValue("2024-06-15T17:30:45", ActivityInterval.Hourly);
        Assert.Equal(new DateTime(2024, 6, 15, 17, 0, 0), result);
    }

    private record Sample(DateTime Timestamp, int Value);

    [Fact]
    public async Task TruncatedHourlyPivot_Average_DoesNotCollapseIntoSum()
    {
        // Three records inside the same hour with different minutes; values 2, 4, 6.
        // Truncating the dimension to the hour must collapse them into a single group so that
        // Average reflects the true per-hour aggregate rather than re-summing to 12.
        var data = new[]
        {
            new Sample(new DateTime(2024, 6, 15, 9, 5, 0), 2),
            new Sample(new DateTime(2024, 6, 15, 9, 25, 0), 4),
            new Sample(new DateTime(2024, 6, 15, 9, 55, 0), 6),
        };

        var results = await data.AsQueryable()
            .ToPivotTable()
            .Dimension(new Dimension<Sample>(
                "Hour",
                s => ActivityHeatmapBuilder<Sample>.TruncateDimensionValue(s.Timestamp, ActivityInterval.Hourly)))
            .Measure(new Measure<Sample>("Value", q => q.Average(s => s.Value)))
            .ExecuteAsync();

        var row = Assert.Single(results);
        Assert.Equal(new DateTime(2024, 6, 15, 9, 0, 0), row["Hour"]);
        Assert.Equal(4d, Convert.ToDouble(row["Value"]));
    }
}
