using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Details : WidgetBase<Details>
{
    /// <summary>They are typically generated automatically from objects using the ToDetails() extension method and DetailsBuilder.</summary>
    public Details(IEnumerable<Detail> items) : base(items.Cast<object>().ToArray())
    {
    }
}