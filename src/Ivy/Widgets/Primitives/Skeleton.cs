using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A placeholder for content that is loading.
/// </summary>
public record Skeleton : WidgetBase<Skeleton>
{
    public static object Card() => new Skeleton().Height(Size.Units(100));

    public static object Form() => Layout.Vertical().Gap(4)
        | Enumerable.Range(0, 4).Select(_ =>
            Layout.Vertical().Gap(1)
            | new Skeleton().Height(Size.Units(4)).Width(Size.Units(20))
            | new Skeleton().Height(Size.Units(10))
        );

    public Skeleton()
    {
        Width = Size.Full();
        Height = Size.Full();
    }
}