using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A placeholder widget representing no content.
/// </summary>
public record Empty : WidgetBase<Empty>
{
    public Empty() { }
}