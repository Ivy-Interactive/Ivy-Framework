using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets.Primitives;

[App(icon: Icons.Video, path: ["Widgets", "Primitives"])]
public class EmbedApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
               | new Embed("https://www.youtube.com/watch?v=rhxQoDlt2AU")
            ;
    }
}