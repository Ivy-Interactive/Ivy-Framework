using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum MockupVariant
{
    Mobile,
    Website,
    Tablet,
    Desktop
}

/// <summary>
/// A hand-drawn device or browser frame that wraps real Ivy content. Children are laid
/// out inside the frame's screen area, so a mockup can hold any widget tree.
/// </summary>
public record WireframeMockup : WidgetBase<WireframeMockup>
{
    public WireframeMockup(params IEnumerable<object> content) : base(content.ToArray())
    {
    }

    internal WireframeMockup()
    {
    }

    [Prop] public MockupVariant Variant { get; set; } = MockupVariant.Mobile;

    [Prop] public Colors Color { get; set; } = Colors.Black;

    /// <summary>Window title on the Desktop variant. Ignored by the other variants.</summary>
    [Prop] public string? Title { get; set; }

    /// <summary>Address bar text on the Website variant. Ignored by the other variants.</summary>
    [Prop] public string? Url { get; set; }
}

public static class WireframeMockupExtensions
{
    public static WireframeMockup Variant(this WireframeMockup mockup, MockupVariant variant)
        => mockup with { Variant = variant };

    public static WireframeMockup Color(this WireframeMockup mockup, Colors color)
        => mockup with { Color = color };

    public static WireframeMockup Title(this WireframeMockup mockup, string title)
        => mockup with { Title = title };

    public static WireframeMockup Url(this WireframeMockup mockup, string url)
        => mockup with { Url = url };
}
