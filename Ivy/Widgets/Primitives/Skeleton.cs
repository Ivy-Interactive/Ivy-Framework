using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Skeleton : WidgetBase<Skeleton>
{
    public static object Card() => new Skeleton().Height(100);

    public Skeleton()
    {
        Width = Size.Full();
        Height = Size.Full();
    }
}