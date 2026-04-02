using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A placeholder for content that is loading.
/// </summary>
public record Skeleton : WidgetBase<Skeleton>
{
    public static object Card() => new Skeleton().Height(Size.Units(100));

    public static object Text(int lines = 3) => Layout.Vertical().Gap(2)
        | Enumerable.Range(0, lines).Select(i =>
            new Skeleton().Height(Size.Units(4)).Width(i == lines - 1 ? Size.Fraction(0.66f) : Size.Full())
        );

    public static object DataTable(int rows = 5) => Layout.Vertical().Gap(2)
        | new Skeleton().Height(Size.Units(8))
        | Enumerable.Range(0, rows).Select(_ =>
            new Skeleton().Height(Size.Units(10))
        );

    public static object Feed(int items = 3) => Layout.Vertical().Gap(4)
        | Enumerable.Range(0, items).Select(_ =>
            Layout.Vertical().Gap(2)
            | (Layout.Horizontal().Gap(2)
                | new Skeleton().Height(Size.Units(10)).Width(Size.Units(10))
                | (Layout.Vertical().Gap(1)
                    | new Skeleton().Height(Size.Units(4)).Width(Size.Units(24))
                    | new Skeleton().Height(Size.Units(3)).Width(Size.Units(16)))
            )
            | new Skeleton().Height(Size.Units(16))
        );

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