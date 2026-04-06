using Ivy.Core;

namespace Ivy.Tests.Widgets.Charts;

public class BarTests
{
    [Fact]
    public void YAxisIndex_WhenSet_SerializesToJson()
    {
        var bar = new Bar("Revenue").YAxisIndex(1);

        var chart = new BarChart(new object[] { }, bar);
        chart.Id = Guid.NewGuid().ToString();

        var result = WidgetSerializer.Serialize(chart);
        var props = result["props"]!.AsObject();
        var bars = props["bars"]!.AsArray();
        var firstBar = bars[0]!.AsObject();

        Assert.Equal(1, firstBar["yAxisIndex"]!.GetValue<int>());
    }

    [Fact]
    public void YAxisIndex_WhenNull_IsOmittedFromJson()
    {
        var bar = new Bar("Revenue");

        var chart = new BarChart(new object[] { }, bar);
        chart.Id = Guid.NewGuid().ToString();

        var result = WidgetSerializer.Serialize(chart);
        var props = result["props"]!.AsObject();
        var bars = props["bars"]!.AsArray();
        var firstBar = bars[0]!.AsObject();

        Assert.False(firstBar.ContainsKey("yAxisIndex"));
    }

    [Fact]
    public void YAxisIndex_ExtensionMethod_SetsValue()
    {
        var bar = new Bar("Cost").YAxisIndex(2);

        Assert.Equal(2, bar.YAxisIndex);
    }
}
