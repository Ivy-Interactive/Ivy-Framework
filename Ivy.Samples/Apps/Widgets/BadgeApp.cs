using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets;

[App(icon: Icons.Pill, path: ["Widgets"])]
public class BadgeApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical(
            new Badge("Default"),
            new Badge("Destructive", variant: BadgeVariant.Destructive),
            new Badge("Outline", variant: BadgeVariant.Outline),
            new Badge("Secondary", variant: BadgeVariant.Secondary),
            new Badge("With Icon", icon: Icons.Bell, onClick: e => Console.WriteLine("Clicked")).Disabled()
        );
    }
}