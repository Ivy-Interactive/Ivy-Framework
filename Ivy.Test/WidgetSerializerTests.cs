using System.Text.Json;
using Ivy.Core;
using Ivy.Widgets;
using Xunit.Abstractions;

namespace Ivy.Test;

public class WidgetSerializerTests(ITestOutputHelper output)
{
    [Fact]
    public void Serialize_SimpleWidget_ReturnsValidJson()
    {
        var widget = new TextBlock("Hello, World!");
        widget.Id = Guid.NewGuid().ToString();

        var result = WidgetSerializer.Serialize(widget);

        Assert.NotNull(result);
        Assert.NotNull(result["id"]);
        Assert.NotNull(result["type"]);
        Assert.NotNull(result["children"]);
        Assert.NotNull(result["props"]);

        output.WriteLine(result.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }
}
