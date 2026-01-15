using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A widget that renders nothing. Useful for conditional rendering placeholders or stubbing out components.
/// </summary>
public record Empty : WidgetBase<Empty>
{
    public Empty() { }
}