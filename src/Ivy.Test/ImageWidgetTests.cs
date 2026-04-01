using System.Text.Json;
using System.Text.Json.Nodes;
using Ivy;
using Ivy.Core;
using Xunit.Abstractions;

namespace Ivy.Test;

public class ImageWidgetTests(ITestOutputHelper output)
{
    private static JsonNode SerializeImage(Image image)
    {
        image.Id = Guid.NewGuid().ToString();
        return WidgetSerializer.Serialize(image);
    }

    [Fact]
    public void Serialize_DefaultImage_NoBorderOrHoverProps()
    {
        var image = new Image("https://example.com/img.png");
        var result = SerializeImage(image);
        var props = result["props"]!.AsObject();

        output.WriteLine(result.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        Assert.Null(props["borderColor"]);
        Assert.Null(props["borderOpacity"]);
        Assert.Null(props["borderRadius"]);
        Assert.Null(props["borderStyle"]);
        Assert.Null(props["borderThickness"]);
        Assert.Null(props["hoverVariant"]);
    }

    [Fact]
    public void Serialize_WithBorderProps_AreSerialized()
    {
        var image = new Image("https://example.com/img.png")
            .BorderColor(Colors.Blue)
            .BorderStyle(BorderStyle.Solid)
            .BorderThickness(2)
            .BorderRadius(BorderRadius.Rounded);

        var result = SerializeImage(image);
        var props = result["props"]!.AsObject();

        output.WriteLine(result.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        Assert.NotNull(props["borderColor"]);
        Assert.Equal("Blue", props["borderColor"]!.GetValue<string>());
        Assert.NotNull(props["borderStyle"]);
        Assert.Equal("Solid", props["borderStyle"]!.GetValue<string>());
        Assert.NotNull(props["borderRadius"]);
        Assert.Equal("Rounded", props["borderRadius"]!.GetValue<string>());
    }

    [Fact]
    public void Serialize_WithHoverVariant_IsSerialized()
    {
        var image = new Image("https://example.com/img.png")
            .Hover(HoverEffect.Shadow);

        var result = SerializeImage(image);
        var props = result["props"]!.AsObject();

        output.WriteLine(result.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        Assert.NotNull(props["hoverVariant"]);
        Assert.Equal("Shadow", props["hoverVariant"]!.GetValue<string>());
    }

    [Fact]
    public void OnClick_AutoSets_HoverVariant_ToPointerAndTranslate()
    {
        var image = new Image("https://example.com/img.png")
            .OnClick(() => { });

        Assert.Equal(HoverEffect.PointerAndTranslate, image.HoverVariant);
    }

    [Fact]
    public void OnClick_PreservesExisting_HoverVariant()
    {
        var image = new Image("https://example.com/img.png")
            .Hover(HoverEffect.Shadow)
            .OnClick(() => { });

        Assert.Equal(HoverEffect.Shadow, image.HoverVariant);
    }

    [Fact]
    public void BorderColor_WithOpacity_SetsBothProperties()
    {
        var image = new Image("https://example.com/img.png")
            .BorderColor(Colors.Red, 0.5f);

        Assert.Equal(Colors.Red, image.BorderColor);
        Assert.NotNull(image.BorderOpacity);
    }
}
