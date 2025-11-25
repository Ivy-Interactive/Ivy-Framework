using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Svg : WidgetBase<Svg>
{
    public Svg(string content)
    {
        Content = content;
        Width = Size.Auto();
        Height = Size.Auto();
    }

    [Prop] public string Content { get; set; }
}