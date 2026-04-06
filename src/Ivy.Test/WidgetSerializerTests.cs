using System.Text.Json;
using Ivy.Core;
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

    [Fact]
    public void Serialize_DefaultValues_AreNotSerialized()
    {
        var widget = new Button("Click me");
        widget.Id = Guid.NewGuid().ToString();

        var result = WidgetSerializer.Serialize(widget);
        var props = result["props"]!.AsObject();

        output.WriteLine(result.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        // Title is set, so it should be serialized
        Assert.NotNull(props["title"]);
        Assert.Equal("Click me", props["title"]!.GetValue<string>());

        // These are all default values, so they should NOT be serialized
        Assert.Null(props["variant"]); // Default is Primary (0)
        Assert.Null(props["iconPosition"]); // Default is Left
        Assert.Null(props["disabled"]); // Default is false
        Assert.Null(props["loading"]); // Default is false
        Assert.Null(props["borderRadius"]); // Default is Rounded
        Assert.Null(props["visible"]); // Default is true (from WidgetBase)
    }

    [Fact]
    public void Serialize_NonDefaultValues_AreSerialized()
    {
        var widget = new Button("Delete")
        {
            Variant = ButtonVariant.Destructive,
            Disabled = true,
            Loading = true
        };
        widget.Id = Guid.NewGuid().ToString();

        var result = WidgetSerializer.Serialize(widget);
        var props = result["props"]!.AsObject();

        output.WriteLine(result.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        // All non-default values should be serialized
        Assert.NotNull(props["variant"]);
        Assert.Equal("Destructive", props["variant"]!.GetValue<string>());

        Assert.NotNull(props["disabled"]);
        Assert.True(props["disabled"]!.GetValue<bool>());

        Assert.NotNull(props["loading"]);
        Assert.True(props["loading"]!.GetValue<bool>());
    }

    [Fact]
    public void Serialize_TextBlock_DefaultVariantNotSerialized()
    {
        var widget = new TextBlock("Hello");
        widget.Id = Guid.NewGuid().ToString();

        var result = WidgetSerializer.Serialize(widget);
        var props = result["props"]!.AsObject();

        output.WriteLine(result.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        // Content is set
        Assert.NotNull(props["content"]);

        // Default values should not be serialized
        Assert.Null(props["variant"]); // Default is Literal
        Assert.Null(props["noWrap"]); // Default is false
        Assert.Null(props["strikeThrough"]); // Default is false
        Assert.Null(props["bold"]); // Default is false
        Assert.Null(props["italic"]); // Default is false
        Assert.Null(props["muted"]); // Default is false
    }

    [Fact]
    public void Serialize_TextBlock_NonDefaultVariantIsSerialized()
    {
        var widget = new TextBlock("Title") { Variant = TextVariant.H1, Bold = true };
        widget.Id = Guid.NewGuid().ToString();

        var result = WidgetSerializer.Serialize(widget);
        var props = result["props"]!.AsObject();

        output.WriteLine(result.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        Assert.NotNull(props["variant"]);
        Assert.Equal("H1", props["variant"]!.GetValue<string>());

        Assert.NotNull(props["bold"]);
        Assert.True(props["bold"]!.GetValue<bool>());
    }

    [Fact]
    public void Serialize_NestedProps_DefaultValuesNotSerialized()
    {
        // Size is serialized as a string representation
        var widget = new Button("Sized") { Width = Size.Units(100) };
        widget.Id = Guid.NewGuid().ToString();

        var result = WidgetSerializer.Serialize(widget);
        var props = result["props"]!.AsObject();

        output.WriteLine(result.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        // Width should be serialized since it's non-null
        Assert.NotNull(props["width"]);
        Assert.Equal("Units:100", props["width"]!.GetValue<string>());
    }

    [Fact]
    public void Serialize_GridLayout_ColumnsAreSerialized()
    {
        var def = new GridDefinition { Columns = 2 };
        var child1 = new TextBlock("A") { Id = "a" };
        var child2 = new TextBlock("B") { Id = "b" };
        var widget = new GridLayout(def, child1, child2);
        widget.Id = Guid.NewGuid().ToString();

        var result = WidgetSerializer.Serialize(widget);
        var props = result["props"]!.AsObject();

        output.WriteLine(result.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        // Columns should be serialized since it's non-default (null)
        Assert.NotNull(props["columns"]);
        Assert.Equal(2, props["columns"]!.GetValue<int>());
    }
}
