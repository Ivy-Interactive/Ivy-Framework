
namespace Ivy.Test;

public class PivotTableTests
{
    public record BrowserSessions(string Name, int Value);

    public record BrowserSessionsPivot(string Browser, int Sessions);

    [Fact]
    public void Test1()
    {
        var raw = new[] {
            new BrowserSessions("Edge", 15),
            new BrowserSessions("Chrome", 55),
            new BrowserSessions("Firefox", 25),
            new BrowserSessions("Safari", 10),
            new BrowserSessions("Others", 5),
            new BrowserSessions("Chrome", 65),
            new BrowserSessions("Firefox", 30),
            new BrowserSessions("Edge", 20),
            new BrowserSessions("Safari", 15),
            new BrowserSessions("Others", 10),
            new BrowserSessions("Chrome", 70),
            new BrowserSessions("Firefox", 35),
            new BrowserSessions("Edge", 25),
            new BrowserSessions("Safari", 20),
            new BrowserSessions("Others", 15)
        };

        raw.ToPivotTable()
            .Dimension(new Dimension<BrowserSessions>("Browser", e => e.Name))
            .Measure(new Measure<BrowserSessions>("Sessions", e => e.Sum(f => f.Value)))
            .Produces<BrowserSessionsPivot>()
            .ExecuteAsync();
    }

    public record HourlyData(DateTime Hour, int Value);

    [Fact]
    public async Task PivotTable_FillGaps_FillsDateTimeGaps()
    {
        var data = new[]
        {
            new HourlyData(new DateTime(2026, 4, 1, 10, 0, 0), 100),
            new HourlyData(new DateTime(2026, 4, 1, 12, 0, 0), 200),
            new HourlyData(new DateTime(2026, 4, 1, 14, 0, 0), 300),
        };

        var results = await data.ToPivotTable()
            .Dimension("Hour", e => e.Hour)
            .Measure("Value", e => e.Sum(f => f.Value))
            .FillGaps(TimeSpan.FromHours(1))
            .ExecuteAsync();

        Assert.Equal(5, results.Length); // 10, 11, 12, 13, 14
        Assert.Equal(new DateTime(2026, 4, 1, 10, 0, 0), results[0]["Hour"]);
        Assert.Equal(0, results[1]["Value"]); // 11:00 filled with 0
        Assert.Equal(new DateTime(2026, 4, 1, 12, 0, 0), results[2]["Hour"]);
        Assert.Equal(0, results[3]["Value"]); // 13:00 filled with 0
        Assert.Equal(new DateTime(2026, 4, 1, 14, 0, 0), results[4]["Hour"]);
    }

    [Fact]
    public async Task PivotTable_FillGaps_PreservesExistingData()
    {
        var data = new[]
        {
            new HourlyData(new DateTime(2026, 4, 1, 10, 0, 0), 100),
            new HourlyData(new DateTime(2026, 4, 1, 11, 0, 0), 200),
            new HourlyData(new DateTime(2026, 4, 1, 12, 0, 0), 300),
        };

        var results = await data.ToPivotTable()
            .Dimension("Hour", e => e.Hour)
            .Measure("Value", e => e.Sum(f => f.Value))
            .FillGaps(TimeSpan.FromHours(1))
            .ExecuteAsync();

        Assert.Equal(3, results.Length); // no gaps to fill
        Assert.Equal(100, results[0]["Value"]);
        Assert.Equal(200, results[1]["Value"]);
        Assert.Equal(300, results[2]["Value"]);
    }

    [Fact]
    public async Task PivotTable_NoFillGaps_ReturnsOnlyExistingData()
    {
        var data = new[]
        {
            new HourlyData(new DateTime(2026, 4, 1, 10, 0, 0), 100),
            new HourlyData(new DateTime(2026, 4, 1, 14, 0, 0), 300),
        };

        var results = await data.ToPivotTable()
            .Dimension("Hour", e => e.Hour)
            .Measure("Value", e => e.Sum(f => f.Value))
            .ExecuteAsync();

        Assert.Equal(2, results.Length); // only existing data, no gap filling
        Assert.Equal(100, results[0]["Value"]);
        Assert.Equal(300, results[1]["Value"]);
    }
}
