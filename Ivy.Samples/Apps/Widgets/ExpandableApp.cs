using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets;

[App(icon: Icons.ChevronsUpDown)]
public class ExpandableApp : SampleBase
{
    protected override object? BuildSample()
    {
        return new Expandable("This is an expandable", "This is the content of the expandable");
    }
}