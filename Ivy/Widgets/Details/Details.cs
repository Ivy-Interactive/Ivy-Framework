using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Details : WidgetBase<Details>
{
    public Details(IEnumerable<Detail> items) : base(items.Cast<object>().ToArray())
    {
    }
}