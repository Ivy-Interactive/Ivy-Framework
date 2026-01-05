using Ivy.Core;
using Ivy.Shared;
using Ivy.Views;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Skeleton : WidgetBase<Skeleton>
{
    public static object Card() => new Skeleton().Height(100);

    public static object Form() => Layout.Vertical().Gap(4)
        | Enumerable.Range(0, 4).Select(_ =>
            Layout.Vertical().Gap(1)
            | new Skeleton().Height(4).Width(Size.Units(20))
            | new Skeleton().Height(10)
        );

    public Skeleton()
    {
        Width = Size.Full();
        Height = Size.Full();
    }
}