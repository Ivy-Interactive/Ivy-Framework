// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A layout that positions children absolutely using CanvasLeft and CanvasTop attached properties.
/// </summary>
public record CanvasLayout : WidgetBase<CanvasLayout>
{
    public CanvasLayout(params object[] children) : base(children) { }

    internal CanvasLayout() { }

    [Prop] public Thickness Padding { get; set; } = new(0);

    [Prop] public Colors? Background { get; set; }

    [Prop(attached: nameof(CanvasExtensions.CanvasLeft))] public Size?[] ChildLeft { get; set; } = null!;

    [Prop(attached: nameof(CanvasExtensions.CanvasTop))] public Size?[] ChildTop { get; set; } = null!;
}

public static class CanvasLayoutExtensions
{
    public static CanvasLayout Background(this CanvasLayout canvas, Colors color)
        => canvas with { Background = color };

    public static CanvasLayout Padding(this CanvasLayout canvas, Thickness padding)
        => canvas with { Padding = padding };

    public static CanvasLayout Padding(this CanvasLayout canvas, int uniform)
        => canvas with { Padding = new Thickness(uniform) };
}

public static class CanvasExtensions
{
    public static WidgetBase<T> CanvasLeft<T>(this WidgetBase<T> child, Size left) where T : WidgetBase<T>
    {
        child.SetAttachedValue(typeof(CanvasLayout), nameof(CanvasLeft), left);
        return child;
    }

    public static WidgetBase<T> CanvasTop<T>(this WidgetBase<T> child, Size top) where T : WidgetBase<T>
    {
        child.SetAttachedValue(typeof(CanvasLayout), nameof(CanvasTop), top);
        return child;
    }

}
