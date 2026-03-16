using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Ivy.Samples.Shared.Apps.Concepts;

[App(icon: Icons.Download, searchHints: ["files", "export", "save", "generate", "blob", "binary"])]
public class DownloadsApp : SampleBase
{
    protected override object? BuildSample()
    {
        IState<string?> imageUrl = UseDownload(GenerateImage, "image/png", "file.png");
        IState<string?> streamUrl = UseDownload(GenerateStream, "application/octet-stream", "large-file.bin");

        if (imageUrl.Value is null) return null;

        return Layout.Vertical()
            | new Image(imageUrl.Value)
            | new Button("Download Image (byte[])").Url(imageUrl.Value)
            | (streamUrl.Value is not null
                ? new Button("Download Stream").Url(streamUrl.Value)
                : null);
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

    private Stream GenerateStream()
    {
        return new MemoryStream(new byte[1024 * 1024]);
    }
}
