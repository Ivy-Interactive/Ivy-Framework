using Ivy.Hooks;
using Ivy.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Ivy.Samples.Apps.Concepts;

[App(icon: Icons.Download)]
public class DownloadsApp : SampleBase
{
    protected override object? BuildSample()
    {
        IState<string?> url = this.UseDownload(GenerateImage, "image/png", "file.png");
        if (url.Value is null) return null;
        return Layout.Vertical()
            | new Image(url.Value)
            | new Button("Download Image").Url(url.Value);
    }

    private byte[] GenerateImage()
    {
        using var image = new Image<Rgba32>(100, 100);
        image.Mutate<Rgba32>(ctx =>
        {
            ctx.BackgroundColor(Color.Blue);
            ctx.Glow(new GraphicsOptions(), Color.White);
        });
        using var ms = new MemoryStream();
        image.SaveAsPng(ms);
        return ms.ToArray();
    }
}
