using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Displays an image.
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

    [Prop] public string Src { get; set; } = String.Empty;
}