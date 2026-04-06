using Ivy.Core;

namespace Ivy.Tests.Widgets.Charts;

public class ScatterTests
{
    [Fact]
    public void YAxisIndex_WhenSet_SerializesToJson()
    {
        var scatter = new Scatter("Value").YAxisIndex(1);

        var chart = new ScatterChart(new object[] { }, scatter);
        chart.Id = Guid.NewGuid().ToString();

        var result = WidgetSerializer.Serialize(chart);
        var props = result["props"]!.AsObject();
        var scatters = props["scatters"]!.AsArray();
        var firstScatter = scatters[0]!.AsObject();

        Assert.Equal(1, firstScatter["yAxisIndex"]!.GetValue<int>());
    }

    [Fact]
    public void YAxisIndex_WhenNull_IsOmittedFromJson()
    {
        var scatter = new Scatter("Value");

        var chart = new ScatterChart(new object[] { }, scatter);
        chart.Id = Guid.NewGuid().ToString();

        var result = WidgetSerializer.Serialize(chart);
        var props = result["props"]!.AsObject();
        var scatters = props["scatters"]!.AsArray();
        var firstScatter = scatters[0]!.AsObject();

        Assert.False(firstScatter.ContainsKey("yAxisIndex"));
    }

    [Fact]
    public void YAxisIndex_ExtensionMethod_SetsValue()
    {
        var scatter = new Scatter("Value").YAxisIndex(2);

        Assert.Equal(2, scatter.YAxisIndex);
    }
}
