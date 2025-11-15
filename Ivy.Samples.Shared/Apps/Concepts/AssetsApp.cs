using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Concepts;

[App(icon: Icons.Album, searchHints: ["images", "files", "resources", "static", "media", "picture"])]
public class AssetsApp : ViewBase
{
    public override object? Build()
    {
        return new Image("/ivy/assets/tagline.png");
    }
}