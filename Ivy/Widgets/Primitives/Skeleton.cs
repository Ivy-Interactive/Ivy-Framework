using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Skeleton : WidgetBase<Skeleton>
{
    public Skeleton()
    {
        Width = Size.Full();
        Height = Size.Full();
    }
}