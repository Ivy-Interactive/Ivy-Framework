
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

    public record IntDimensionData(int Id, int Score);

    [Fact]
    public async Task PivotTable_FillGaps_FillsIntGaps()
    {
        var data = new[]
        {
            new IntDimensionData(1, 100),
            new IntDimensionData(3, 300),
            new IntDimensionData(5, 500),
        };

        var results = await data.ToPivotTable()
            .Dimension("Id", e => e.Id)
            .Measure("Score", e => e.Sum(f => f.Score))
            .FillGaps()
            .ExecuteAsync();

        Assert.Equal(5, results.Length); // 1, 2, 3, 4, 5
        Assert.Equal(1, results[0]["Id"]);
        Assert.Equal(100, results[0]["Score"]);
        Assert.Equal(2, results[1]["Id"]);
        Assert.Equal(0, results[1]["Score"]); // filled with 0
        Assert.Equal(3, results[2]["Id"]);
        Assert.Equal(300, results[2]["Score"]);
        Assert.Equal(4, results[3]["Id"]);
        Assert.Equal(0, results[3]["Score"]); // filled with 0
        Assert.Equal(5, results[4]["Id"]);
        Assert.Equal(500, results[4]["Score"]);
    }

    [Fact]
    public async Task PivotTable_FillGaps_IntWithCustomInterval()
    {
        var data = new[]
        {
            new IntDimensionData(0, 10),
            new IntDimensionData(5, 50),
            new IntDimensionData(10, 100),
            new IntDimensionData(20, 200),
        };

        var results = await data.ToPivotTable()
            .Dimension("Id", e => e.Id)
            .Measure("Score", e => e.Sum(f => f.Score))
            .FillGaps(5)
            .ExecuteAsync();

        Assert.Equal(5, results.Length); // 0, 5, 10, 15, 20
        Assert.Equal(0, results[0]["Id"]);
        Assert.Equal(10, results[0]["Score"]);
        Assert.Equal(5, results[1]["Id"]);
        Assert.Equal(50, results[1]["Score"]);
        Assert.Equal(10, results[2]["Id"]);
        Assert.Equal(100, results[2]["Score"]);
        Assert.Equal(15, results[3]["Id"]);
        Assert.Equal(0, results[3]["Score"]); // filled with 0
        Assert.Equal(20, results[4]["Id"]);
        Assert.Equal(200, results[4]["Score"]);
    }

    [Fact]
    public async Task PivotTable_FillGaps_IntPreservesExistingData()
    {
        var data = new[]
        {
            new IntDimensionData(1, 100),
            new IntDimensionData(2, 200),
            new IntDimensionData(3, 300),
        };

        var results = await data.ToPivotTable()
            .Dimension("Id", e => e.Id)
            .Measure("Score", e => e.Sum(f => f.Score))
            .FillGaps()
            .ExecuteAsync();

        Assert.Equal(3, results.Length); // no gaps to fill
        Assert.Equal(100, results[0]["Score"]);
        Assert.Equal(200, results[1]["Score"]);
        Assert.Equal(300, results[2]["Score"]);
    }

    public record StringDimensionData(string Hour, int Count);

    [Fact]
    public async Task FillGaps_ThrowsForUnsupportedDimensionType()
    {
        var data = new[]
        {
            new StringDimensionData("2026-04-06 10:00", 5),
            new StringDimensionData("2026-04-06 12:00", 3),
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await data.ToPivotTable()
                .Dimension("Hour", e => e.Hour)
                .Measure("Count", e => e.Sum(f => f.Count))
                .FillGaps(TimeSpan.FromHours(1))
                .ExecuteAsync();
        });

        Assert.Contains("FillGaps() is only supported for DateTime and int dimensions", exception.Message);
        Assert.Contains("Dimension 'Hour' has type 'String'", exception.Message);
    }

    [Fact]
    public async Task PivotTable_FillGaps_MixedTypesDontInterfere()
    {
        // Verify DateTime gap filling still works after int support was added
        var data = new[]
        {
            new HourlyData(new DateTime(2026, 4, 1, 10, 0, 0), 100),
            new HourlyData(new DateTime(2026, 4, 1, 12, 0, 0), 200),
        };

        var results = await data.ToPivotTable()
            .Dimension("Hour", e => e.Hour)
            .Measure("Value", e => e.Sum(f => f.Value))
            .FillGaps(TimeSpan.FromHours(1))
            .ExecuteAsync();

        Assert.Equal(3, results.Length); // 10, 11, 12
        Assert.Equal(new DateTime(2026, 4, 1, 10, 0, 0), results[0]["Hour"]);
        Assert.Equal(100, results[0]["Value"]);
        Assert.Equal(0, results[1]["Value"]); // 11:00 filled with 0
        Assert.Equal(new DateTime(2026, 4, 1, 12, 0, 0), results[2]["Hour"]);
        Assert.Equal(200, results[2]["Value"]);
    }
}
