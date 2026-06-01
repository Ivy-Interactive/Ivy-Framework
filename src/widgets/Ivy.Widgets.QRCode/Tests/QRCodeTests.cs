using System.Text.Json;
using System.Text.Json.Nodes;
using Ivy;
using Ivy.Core;
using Ivy.Widgets.QRCode;
using Xunit.Abstractions;

namespace Ivy.Widgets.QRCode.Tests;

public class QRCodeTests(ITestOutputHelper output)
{
    private static JsonNode SerializeQr(QRCode qr)
    {
        qr.Id = Guid.NewGuid().ToString();
        return WidgetSerializer.Serialize(qr);
    }

    [Fact]
    public void Defaults_ValueEmpty_LowCorrection_OptionalNull()
    {
        var qr = new QRCode();

        Assert.Equal("", qr.Value);
        Assert.Null(qr.PixelSize);
        Assert.Equal(QrErrorCorrectionLevel.Low, qr.ErrorCorrectionLevel);
        Assert.Null(qr.Background);
        Assert.Null(qr.Foreground);
    }

    [Fact]
    public void Value_Extension_SetsValue()
    {
        var qr = new QRCode().Value("https://ivy.app");

        Assert.Equal("https://ivy.app", qr.Value);
    }

    [Fact]
    public void PixelSize_Extension_SetsPixelSize()
    {
        var qr = new QRCode().PixelSize(128);

        Assert.Equal(128, qr.PixelSize);
    }

    [Fact]
    public void ErrorCorrectionLevel_Extension_SetsLevel()
    {
        var qr = new QRCode().ErrorCorrectionLevel(QrErrorCorrectionLevel.High);

        Assert.Equal(QrErrorCorrectionLevel.High, qr.ErrorCorrectionLevel);
    }

    [Fact]
    public void Background_Foreground_Extensions_SetColors()
    {
        var qr = new QRCode()
            .Background(Colors.White)
            .Foreground(Colors.Black);

        Assert.Equal(Colors.White, qr.Background);
        Assert.Equal(Colors.Black, qr.Foreground);
    }

    [Fact]
    public void Extensions_ReturnNewInstance_PreservesOriginal()
    {
        var original = new QRCode();
        var chained = original
            .Value("x")
            .PixelSize(64)
            .ErrorCorrectionLevel(QrErrorCorrectionLevel.Medium);

        Assert.Equal("", original.Value);
        Assert.Null(original.PixelSize);
        Assert.Equal(QrErrorCorrectionLevel.Low, original.ErrorCorrectionLevel);

        Assert.Equal("x", chained.Value);
        Assert.Equal(64, chained.PixelSize);
        Assert.Equal(QrErrorCorrectionLevel.Medium, chained.ErrorCorrectionLevel);
        Assert.NotSame(original, chained);
    }

    [Fact]
    public void Serialize_TypeName_IsFullWidgetName()
    {
        var qr = new QRCode();
        var result = SerializeQr(qr);

        output.WriteLine(result.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        Assert.Equal("Ivy.Widgets.QRCode.QRCode", result["type"]!.GetValue<string>());
    }

    [Fact]
    public void Serialize_DefaultProps_OmitRedundantValues()
    {
        var qr = new QRCode();
        var result = SerializeQr(qr);
        var props = result["props"]!.AsObject();

        output.WriteLine(result.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        Assert.Null(props["value"]);
        Assert.Null(props["pixelSize"]);
        Assert.Null(props["errorCorrectionLevel"]);
        Assert.Null(props["background"]);
        Assert.Null(props["foreground"]);
    }

    [Fact]
    public void Serialize_NonDefaultProps_ArePresent()
    {
        var qr = new QRCode()
            .Value("https://example.com")
            .PixelSize(96)
            .ErrorCorrectionLevel(QrErrorCorrectionLevel.Quartile)
            .Background(Colors.Slate)
            .Foreground(Colors.Rose);

        var result = SerializeQr(qr);
        var props = result["props"]!.AsObject();

        output.WriteLine(result.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        Assert.Equal("https://example.com", props["value"]!.GetValue<string>());
        Assert.Equal(96, props["pixelSize"]!.GetValue<int>());
        Assert.Equal("Quartile", props["errorCorrectionLevel"]!.GetValue<string>());
        Assert.Equal("Slate", props["background"]!.GetValue<string>());
        Assert.Equal("Rose", props["foreground"]!.GetValue<string>());
    }
}
