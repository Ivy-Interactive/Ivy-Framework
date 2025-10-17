using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.Image, path: ["Widgets", "Primitives"], searchHints: ["picture", "photo", "img", "graphics", "media", "visual"])]
public class ImageApp : SampleBase
{
    protected override object? BuildSample()
    {
        return new Image("https://placehold.co/600x400");
    }
}