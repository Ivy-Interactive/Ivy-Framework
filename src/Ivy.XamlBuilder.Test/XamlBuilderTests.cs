using System.Text.Json;
using Ivy;
using Ivy.Core;

namespace Ivy.XamlBuilderTests;

public class XamlBuilderTests
{
    private readonly XamlBuilder _builder = new();

    // --- Widget resolution ---

    [Fact]
    public void Build_Badge()
    {
        var widget = _builder.Build("<Badge />");
        Assert.IsType<Badge>(widget);
    }

    [Fact]
    public void Build_Button()
    {
        var widget = _builder.Build("<Button />");
        Assert.IsType<Button>(widget);
    }

    [Fact]
    public void Build_StackLayout()
    {
        var widget = _builder.Build("<StackLayout />");
        Assert.IsType<StackLayout>(widget);
    }

    [Fact]
    public void Build_UnknownWidget_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _builder.Build("<NonExistent />"));
    }

    // --- Props: type conversion ---

    [Fact]
    public void Build_StringProp()
    {
        var widget = _builder.Build("<Badge Title=\"Hello\" />");
        var badge = Assert.IsType<Badge>(widget);
        Assert.Equal("Hello", badge.Title);
    }

    [Fact]
    public void Build_EnumProp()
    {
        var widget = _builder.Build("<Badge Variant=\"Success\" />");
        var badge = Assert.IsType<Badge>(widget);
        Assert.Equal(BadgeVariant.Success, badge.Variant);
    }

    [Fact]
    public void Build_EnumProp_CaseInsensitive()
    {
        var widget = _builder.Build("<Badge Variant=\"destructive\" />");
        var badge = Assert.IsType<Badge>(widget);
        Assert.Equal(BadgeVariant.Destructive, badge.Variant);
    }

    [Fact]
    public void Build_BoolProp()
    {
        var widget = _builder.Build("<Button Disabled=\"true\" />");
        var button = Assert.IsType<Button>(widget);
        Assert.True(button.Disabled);
    }

    [Fact]
    public void Build_IntProp()
    {
        var widget = _builder.Build("<StackLayout Gap=\"8\" />");
        var stack = Assert.IsType<StackLayout>(widget);
        Assert.Equal(8, stack.Gap);
    }

    [Fact]
    public void Build_UnknownProp_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _builder.Build("<Badge Foo=\"bar\" />"));
    }

    // --- Props: Size parsing ---

    [Theory]
    [MemberData(nameof(SizeTestData))]
    public void Build_SizeProp(string sizeValue, Size expected)
    {
        var widget = _builder.Build($"<Badge Width=\"{sizeValue}\" />");
        var badge = Assert.IsType<Badge>(widget);
        Assert.Equal(expected, badge.Width);
    }

    public static TheoryData<string, Size> SizeTestData => new()
    {
        { "Full", Size.Full() },
        { "Fit", Size.Fit() },
        { "Auto", Size.Auto() },
        { "Screen", Size.Screen() },
        { "Half", Size.Half() },
        { "50%", Size.Fraction(0.5f) },
        { "100px", Size.Px(100) },
        { "2rem", Size.Rem(2) },
        { "10", Size.Units(10) },
    };

    // --- Children ---

    [Fact]
    public void Build_NestedWidgets()
    {
        var widget = _builder.Build(
            "<StackLayout><Badge Title=\"A\" /><Badge Title=\"B\" /></StackLayout>");
        var stack = Assert.IsType<StackLayout>(widget);
        Assert.Equal(2, stack.Children.Length);
        Assert.Equal("A", Assert.IsType<Badge>(stack.Children[0]).Title);
        Assert.Equal("B", Assert.IsType<Badge>(stack.Children[1]).Title);
    }

    [Fact]
    public void Build_DeeplyNested()
    {
        var widget = _builder.Build(
            "<StackLayout><StackLayout><StackLayout><Badge Title=\"Deep\" /></StackLayout></StackLayout></StackLayout>");
        var s1 = Assert.IsType<StackLayout>(widget);
        var s2 = Assert.IsType<StackLayout>(s1.Children[0]);
        var s3 = Assert.IsType<StackLayout>(s2.Children[0]);
        var badge = Assert.IsType<Badge>(s3.Children[0]);
        Assert.Equal("Deep", badge.Title);
    }

    [Fact]
    public void Build_PropsAndChildren()
    {
        var widget = _builder.Build(
            "<StackLayout Gap=\"12\"><Badge Title=\"A\" /><Button Disabled=\"true\" /></StackLayout>");
        var stack = Assert.IsType<StackLayout>(widget);
        Assert.Equal(12, stack.Gap);
        Assert.Equal(2, stack.Children.Length);
        Assert.Equal("A", Assert.IsType<Badge>(stack.Children[0]).Title);
        Assert.True(Assert.IsType<Button>(stack.Children[1]).Disabled);
    }

    // --- Property element syntax: single objects ---

    [Fact]
    public void Build_PropertyElement_SingleObject()
    {
        var widget = _builder.Build(
            "<AreaChart><AreaChart.CartesianGrid StrokeDashArray=\"3 3\" /></AreaChart>");
        var chart = Assert.IsType<AreaChart>(widget);
        Assert.NotNull(chart.CartesianGrid);
        Assert.Equal("3 3", chart.CartesianGrid!.StrokeDashArray);
    }

    [Fact]
    public void Build_PropertyElement_DefaultInstance()
    {
        var widget = _builder.Build(
            "<AreaChart><AreaChart.Tooltip /></AreaChart>");
        var chart = Assert.IsType<AreaChart>(widget);
        Assert.NotNull(chart.Tooltip);
    }

    [Fact]
    public void Build_PropertyElement_Legend_NestedEnum()
    {
        var widget = _builder.Build(
            "<AreaChart><AreaChart.Legend Layout=\"Horizontal\" Align=\"Center\" /></AreaChart>");
        var chart = Assert.IsType<AreaChart>(widget);
        Assert.NotNull(chart.Legend);
        Assert.Equal(Legend.Layouts.Horizontal, chart.Legend!.Layout);
        Assert.Equal(Legend.Alignments.Center, chart.Legend.Align);
    }

    // --- Property element syntax: arrays ---

    [Fact]
    public void Build_PropertyElement_Array()
    {
        var widget = _builder.Build(
            "<AreaChart><AreaChart.Areas><Area DataKey=\"v1\" /><Area DataKey=\"v2\" /></AreaChart.Areas></AreaChart>");
        var chart = Assert.IsType<AreaChart>(widget);
        Assert.Equal(2, chart.Areas.Length);
        Assert.Equal("v1", chart.Areas[0].DataKey);
        Assert.Equal("v2", chart.Areas[1].DataKey);
    }

    [Fact]
    public void Build_PropertyElement_Array_WithProps()
    {
        var widget = _builder.Build(
            "<AreaChart><AreaChart.Areas><Area DataKey=\"v1\" CurveType=\"Linear\" Stroke=\"Blue\" FillOpacity=\"0.5\" /></AreaChart.Areas></AreaChart>");
        var chart = Assert.IsType<AreaChart>(widget);
        Assert.Single(chart.Areas);
        Assert.Equal(CurveTypes.Linear, chart.Areas[0].CurveType);
        Assert.Equal(Colors.Blue, chart.Areas[0].Stroke);
        Assert.Equal(0.5, chart.Areas[0].FillOpacity);
    }

    [Fact]
    public void Build_PropertyElement_XAxis()
    {
        var widget = _builder.Build(
            "<AreaChart><AreaChart.XAxis><XAxis DataKey=\"month\" Type=\"Category\" /></AreaChart.XAxis></AreaChart>");
        var chart = Assert.IsType<AreaChart>(widget);
        Assert.Single(chart.XAxis);
        Assert.Equal("month", chart.XAxis[0].DataKey);
        Assert.Equal(AxisTypes.Category, chart.XAxis[0].Type);
    }

    // --- Property element syntax: nested nesting ---

    [Fact]
    public void Build_PropertyElement_NestedNesting()
    {
        var widget = _builder.Build(
            "<AreaChart><AreaChart.Areas><Area DataKey=\"v1\"><Area.Label Position=\"Top\" Offset=\"10\" /></Area></AreaChart.Areas></AreaChart>");
        var chart = Assert.IsType<AreaChart>(widget);
        Assert.Single(chart.Areas);
        Assert.NotNull(chart.Areas[0].Label);
        Assert.Equal(Positions.Top, chart.Areas[0].Label!.Position);
        Assert.Equal(10, chart.Areas[0].Label!.Offset);
    }

    // --- Full AreaChart integration ---

    [Fact]
    public void Build_AreaChart_Full()
    {
        var xml = """
            <AreaChart ColorScheme="Default" StackOffset="None">
                <AreaChart.CartesianGrid StrokeDashArray="3 3" Horizontal="true" />
                <AreaChart.Tooltip />
                <AreaChart.Legend Layout="Horizontal" Align="Center" />
                <AreaChart.Areas>
                    <Area DataKey="value" CurveType="Natural" Stroke="Blue" FillOpacity="0.3" />
                    <Area DataKey="value2" CurveType="Linear" Stroke="Red" />
                </AreaChart.Areas>
                <AreaChart.XAxis>
                    <XAxis DataKey="month" Type="Category" />
                </AreaChart.XAxis>
                <AreaChart.YAxis>
                    <YAxis />
                </AreaChart.YAxis>
            </AreaChart>
            """;

        var widget = _builder.Build(xml);
        var chart = Assert.IsType<AreaChart>(widget);

        Assert.Equal(ColorScheme.Default, chart.ColorScheme);
        Assert.Equal(StackOffsetTypes.None, chart.StackOffset);

        Assert.NotNull(chart.CartesianGrid);
        Assert.Equal("3 3", chart.CartesianGrid!.StrokeDashArray);
        Assert.True(chart.CartesianGrid.Horizontal);

        Assert.NotNull(chart.Tooltip);

        Assert.NotNull(chart.Legend);
        Assert.Equal(Legend.Layouts.Horizontal, chart.Legend!.Layout);
        Assert.Equal(Legend.Alignments.Center, chart.Legend.Align);

        Assert.Equal(2, chart.Areas.Length);
        Assert.Equal("value", chart.Areas[0].DataKey);
        Assert.Equal(CurveTypes.Natural, chart.Areas[0].CurveType);
        Assert.Equal(Colors.Blue, chart.Areas[0].Stroke);
        Assert.Equal(0.3, chart.Areas[0].FillOpacity);
        Assert.Equal("value2", chart.Areas[1].DataKey);
        Assert.Equal(CurveTypes.Linear, chart.Areas[1].CurveType);
        Assert.Equal(Colors.Red, chart.Areas[1].Stroke);

        Assert.Single(chart.XAxis);
        Assert.Equal("month", chart.XAxis[0].DataKey);
        Assert.Equal(AxisTypes.Category, chart.XAxis[0].Type);

        Assert.Single(chart.YAxis);
    }

    // --- Property element mixed with simple props ---

    [Fact]
    public void Build_PropertyElement_WithSimpleProps()
    {
        var widget = _builder.Build(
            "<AreaChart ColorScheme=\"Rainbow\"><AreaChart.Tooltip /></AreaChart>");
        var chart = Assert.IsType<AreaChart>(widget);
        Assert.Equal(ColorScheme.Rainbow, chart.ColorScheme);
        Assert.NotNull(chart.Tooltip);
    }

    // --- JSON data arrays ---

    [Fact]
    public void Build_JsonData_ParsesCorrectly()
    {
        var xml = """
            <LineChart>
                <Data><![CDATA[
                    [{"Month": "Jan", "Revenue": 4000}]
                ]]></Data>
            </LineChart>
            """;
        var widget = _builder.Build(xml);
        var chart = Assert.IsType<LineChart>(widget);
        var data = Assert.IsType<Dictionary<string, object>[]>(chart.Data);
        Assert.Single(data);
        Assert.Equal("Jan", data[0]["Month"]);
        Assert.IsType<string>(data[0]["Month"]);
        Assert.Equal(4000.0, data[0]["Revenue"]);
        Assert.IsType<double>(data[0]["Revenue"]);
    }

    [Fact]
    public void Build_JsonData_StringValues_PreserveExact()
    {
        var xml = """
            <LineChart>
                <Data><![CDATA[
                    [{"Id": "007"}]
                ]]></Data>
            </LineChart>
            """;
        var widget = _builder.Build(xml);
        var chart = Assert.IsType<LineChart>(widget);
        var data = Assert.IsType<Dictionary<string, object>[]>(chart.Data);
        Assert.Single(data);
        Assert.Equal("007", data[0]["Id"]);
        Assert.IsType<string>(data[0]["Id"]);
    }

    [Fact]
    public void Build_JsonData_BoolValues_PreserveType()
    {
        var xml = """
            <BarChart>
                <Data><![CDATA[
                    [{"Active": true}]
                ]]></Data>
            </BarChart>
            """;
        var widget = _builder.Build(xml);
        var chart = Assert.IsType<BarChart>(widget);
        var data = Assert.IsType<Dictionary<string, object>[]>(chart.Data);
        Assert.Single(data);
        Assert.Equal(true, data[0]["Active"]);
        Assert.IsType<bool>(data[0]["Active"]);
    }

    [Fact]
    public void Build_JsonData_MalformedJson_Throws()
    {
        var xml = """
            <LineChart>
                <Data><![CDATA[
                    [{"Month": "Jan", bad json}]
                ]]></Data>
            </LineChart>
            """;
        Assert.Throws<JsonException>(() => _builder.Build(xml));
    }

    [Fact]
    public void Build_LineChart_Full_WithJsonData()
    {
        var xml = """
            <LineChart ColorScheme="Default">
                <Data><![CDATA[
                    [
                        {"Month": "Jan", "Revenue": 100, "Costs": 80},
                        {"Month": "Feb", "Revenue": 120, "Costs": 90}
                    ]
                ]]></Data>
                <LineChart.Lines>
                    <Line DataKey="Revenue" />
                    <Line DataKey="Costs" />
                </LineChart.Lines>
                <LineChart.XAxis>
                    <XAxis DataKey="Month" />
                </LineChart.XAxis>
            </LineChart>
            """;
        var widget = _builder.Build(xml);
        var chart = Assert.IsType<LineChart>(widget);
        Assert.Equal(ColorScheme.Default, chart.ColorScheme);
        var data = Assert.IsType<Dictionary<string, object>[]>(chart.Data);
        Assert.Equal(2, data.Length);
        Assert.Equal(2, chart.Lines.Length);
        Assert.Single(chart.XAxis);
    }

    // --- Error cases ---

    [Fact]
    public void Build_PropertyElement_UnknownProperty_Throws()
    {
        Assert.Throws<InvalidOperationException>(
            () => _builder.Build("<AreaChart><AreaChart.Nope /></AreaChart>"));
    }

    [Fact]
    public void Build_PropertyElement_TypeMismatch_Throws()
    {
        Assert.Throws<InvalidOperationException>(
            () => _builder.Build(
                "<AreaChart><AreaChart.Areas><Badge /></AreaChart.Areas></AreaChart>"));
    }
}
