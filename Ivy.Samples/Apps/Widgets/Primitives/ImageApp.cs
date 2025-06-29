using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets.Primitives;

[App(icon: Icons.Image, path: ["Widgets", "Primitives"])]
public class ImageApp : SampleBase
{
    protected override object? BuildSample()
    {
        return new Image("https://placehold.co/600x400");
    }
}