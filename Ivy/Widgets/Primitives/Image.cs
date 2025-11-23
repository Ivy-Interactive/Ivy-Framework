using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Image : WidgetBase<Image>
{
    public Image(string src)
    {
        Src = src;
        Width = Size.MinContent();
        Height = Size.MinContent();
    }

    // TODO: Maintain aspect ratio, Clippings: Circular, Square, Rounded

    [Prop] public string Src { get; set; }
}