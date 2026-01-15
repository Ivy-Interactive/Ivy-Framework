using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A container for displaying a list of Detail items.
/// </summary>
public record Details : WidgetBase<Details>
{
    public Details(IEnumerable<Detail> items) : base(items.Cast<object>().ToArray())
    {
    }

    internal Details()
    {
    }
}