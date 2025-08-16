using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Concepts;

[App(icon: Icons.Album)]
public class AssetsApp : ViewBase
{
    public override object? Build()
    {
        return new Image("/assets/tagline.png");
    }
}