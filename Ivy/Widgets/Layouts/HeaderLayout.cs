using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record HeaderLayout : WidgetBase<HeaderLayout>
{
    public HeaderLayout(object header, object content) : base([new Slot("Header", header), new Slot("Content", content)])
    {
    }

    public static HeaderLayout operator |(HeaderLayout widget, object child)
    {
        throw new NotSupportedException("HeaderLayout does not support children.");
    }
}