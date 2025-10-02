using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Image widget with automatic sizing and aspect ratio preservation. Supports JPEG, PNG, GIF, WebP, SVG.</summary>
public record Image : WidgetBase<Image>
{
    /// <summary>Initializes image widget. Default size: min-content (natural dimensions).</summary>
    /// <param name="src">Image URL, path, or data URI.</param>
    public Image(string src)
    {
        Src = src;
        Width = Size.MinContent();
        Height = Size.MinContent();
    }

    // TODO: Maintain aspect ratio, Clippings: Circular, Square, Rounded

    /// <summary>Image source URL, path, or data URI.</summary>
    [Prop] public string Src { get; set; }
}