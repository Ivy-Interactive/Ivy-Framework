using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A simple layout with a footer slot fixed at the bottom.
/// </summary>
public record FooterLayout : WidgetBase<FooterLayout>
{
    public FooterLayout(object footer, object content) : base([new Slot("Footer", footer), new Slot("Content", content)])
    {
    }

    internal FooterLayout()
    {
    }

    public static FooterLayout operator |(FooterLayout widget, object child)
    {
        throw new NotSupportedException("FooterLayout does not support children.");
    }
}