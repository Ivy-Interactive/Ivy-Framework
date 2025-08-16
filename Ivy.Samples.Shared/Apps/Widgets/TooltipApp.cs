using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.MessageSquare, path: ["Widgets"])]
public class TooltipApp : SampleBase
{
    protected override object? BuildSample()
    {
        return new Tooltip(new Button("Hoover Me"), "Hello World!");
    }
}