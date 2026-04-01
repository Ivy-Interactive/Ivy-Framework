using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum ImageFit
{
    Cover,
    Contain,
    Fill,
    None,
    ScaleDown
}

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
    [Prop] public string? Alt { get; set; }
    [Prop] public string? Caption { get; set; }
    [Prop] public string? Link { get; set; }
    [Prop] public ImageFit? ObjectFit { get; set; }

    [Prop] public Colors? BorderColor { get; set; }
    [Prop] public float? BorderOpacity { get; set; }
    [Prop] public BorderRadius BorderRadius { get; set; } = BorderRadius.None;
    [Prop] public BorderStyle BorderStyle { get; set; } = BorderStyle.None;
    [Prop] public Thickness BorderThickness { get; set; } = new(0);
    [Prop] public HoverEffect HoverVariant { get; set; } = HoverEffect.None;
    [Prop] public bool Overlay { get; set; }

    [Event] public EventHandler<Event<Image>>? OnClick { get; set; }
}

public static class ImageExtensions
{
    public static Image Link(this Image image, string url)
    {
        var validatedUrl = ValidationHelper.ValidateLinkUrl(url);
        if (validatedUrl == null)
        {
            throw new ArgumentException($"Invalid URL: {url}. Only safe URLs (http/https, relative paths, app://, anchors) are allowed.", nameof(url));
        }
        return image with { Link = validatedUrl };
    }

    public static Image BorderColor(this Image image, Colors color) => image with { BorderColor = color };

    public static Image BorderColor(this Image image, Colors color, float opacity) => image with { BorderColor = color, BorderOpacity = (1.0f - opacity) * 100 };

    public static Image BorderRadius(this Image image, BorderRadius radius) => image with { BorderRadius = radius };

    public static Image BorderStyle(this Image image, BorderStyle style) => image with { BorderStyle = style };

    public static Image BorderThickness(this Image image, int thickness) => image with { BorderThickness = new(thickness) };

    public static Image BorderThickness(this Image image, Thickness thickness) => image with { BorderThickness = thickness };

    public static Image Hover(this Image image, HoverEffect variant) => image with { HoverVariant = variant };

    private static HoverEffect HoverVariantWithClick(this Image image) => image.HoverVariant == HoverEffect.None ? HoverEffect.PointerAndTranslate : image.HoverVariant;

    public static Image OnClick(this Image image, Func<Event<Image>, ValueTask> onClick)
        => image with { HoverVariant = image.HoverVariantWithClick(), OnClick = new(onClick) };

    public static Image OnClick(this Image image, Action<Event<Image>> onClick)
        => image with { HoverVariant = image.HoverVariantWithClick(), OnClick = new(onClick.ToValueTask()) };

    public static Image OnClick(this Image image, Action onClick)
        => image with { HoverVariant = image.HoverVariantWithClick(), OnClick = new(_ => { onClick(); return ValueTask.CompletedTask; }) };

    public static Image OnClick(this Image image, Func<ValueTask> onClick)
        => image with { HoverVariant = image.HoverVariantWithClick(), OnClick = new(_ => onClick()) };
}