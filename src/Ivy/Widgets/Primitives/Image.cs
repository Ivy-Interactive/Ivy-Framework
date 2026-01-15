using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Displays an image from a URL. Supports styling, object-fit sizing, and aspect ratio control.
/// </summary>
public record Image : WidgetBase<Image>
{
    public Image(string src) : this()
    {
        Src = src;
    }

    internal Image()
    {
        Width = Size.MinContent();
        Height = Size.MinContent();
    }

    // TODO: Maintain aspect ratio, Clippings: Circular, Square, Rounded

    [Prop] public string Src { get; set; } = String.Empty;
}