namespace Ivy.Test.Widgets.Charts;

public class ScatterChartTests
{
    [Fact]
    public void ScatterChart_WithCategoryXAxis_ThrowsInvalidOperationException()
    {
        var data = new[] { new { X = 1, Y = 10 } };
        var chart = new ScatterChart(data);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            chart.XAxis(new XAxis("X").Type(AxisTypes.Category)));

        Assert.Contains("requires numeric axes", ex.Message);
        Assert.Contains("XAxis", ex.Message);
    }

    [Fact]
    public void ScatterChart_WithCategoryYAxis_ThrowsInvalidOperationException()
    {
        var data = new[] { new { X = 10, Y = 1 } };
        var chart = new ScatterChart(data);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            chart.YAxis(new YAxis("Y").Type(AxisTypes.Category)));

        Assert.Contains("requires numeric axes", ex.Message);
        Assert.Contains("YAxis", ex.Message);
    }

    [Fact]
    public void ScatterChart_WithNumericAxes_Succeeds()
    {
        var data = new[] { new { X = 1, Y = 10 } };
        var chart = new ScatterChart(data)
            .XAxis(new XAxis("X").Type(AxisTypes.Number))
            .YAxis(new YAxis("Y").Type(AxisTypes.Number));

        Assert.NotEmpty(chart.XAxis);
        Assert.NotEmpty(chart.YAxis);
    }

    [Fact]
    public void ScatterChart_WithDefaultAxisTypes_Succeeds()
    {
        var data = new[] { new { X = 1, Y = 10 } };

        // XAxis defaults to Category in Axis.cs, but the string-overload
        // creates an XAxis without explicitly setting Type — validation
        // only runs on the XAxis(XAxis) overload.
        var chart = new ScatterChart(data)
            .XAxis("X")
            .YAxis("Y");

        Assert.NotEmpty(chart.XAxis);
        Assert.NotEmpty(chart.YAxis);
    }
}
