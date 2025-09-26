using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.Star, path: ["Widgets", "Primitives"])]
public class IconApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Horizontal(
            new Icon(Icons.SaveAll),
            new Icon(Icons.SaveAll, Colors.Primary),
            new Icon(Icons.Trash, Colors.Destructive),
            new Icon(Icons.Check, Colors.Green)
        );
    }
}