using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Utility widget representing empty/null content. Renders nothing but maintains widget tree structure.</summary>
public record Empty : WidgetBase<Empty>
{
}