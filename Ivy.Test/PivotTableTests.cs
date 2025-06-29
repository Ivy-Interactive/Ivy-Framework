using Ivy.Charts;

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
}