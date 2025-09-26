using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.CloudAlert, path: ["Widgets", "Primitives"])]
public class CalloutApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
               | new Callout("Info", "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec nunc.", CalloutVariant.Info)
               | new Callout("Success", "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec nunc.", CalloutVariant.Success)
               | new Callout("Warning", "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec nunc.", CalloutVariant.Warning)
               | new Callout("Error", "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec nunc.", CalloutVariant.Error)
               | new Callout("Error", "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec nunc.", CalloutVariant.Error, icon: Icons.Bug)
            ;

    }
}