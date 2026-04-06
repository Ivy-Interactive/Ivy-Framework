// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A container for panels that can be resized relative to each other.
/// </summary>
public record ResizablePanelGroup : WidgetBase<ResizablePanelGroup>
{
    public ResizablePanelGroup(params ResizablePanel[] children) : base(children.Cast<object>().ToArray())
    {
        Width = Size.Full();
        Height = Size.Full();
    }

    internal ResizablePanelGroup()
    {
        Width = Size.Full();
        Height = Size.Full();
    }

    [Prop] public bool ShowHandle { get; init; } = true;

    [Prop] public Orientation Direction { get; init; } = Orientation.Horizontal;
}

public static class ResizablePanelsExtensions
{
    public static ResizablePanelGroup ShowHandle(this ResizablePanelGroup widget, bool value) => widget with { ShowHandle = value };

    public static ResizablePanelGroup Direction(this ResizablePanelGroup widget, Orientation value) => widget with { Direction = value };

    public static ResizablePanelGroup Horizontal(this ResizablePanelGroup widget) => widget with { Direction = Orientation.Horizontal };

    public static ResizablePanelGroup Vertical(this ResizablePanelGroup widget) => widget with { Direction = Orientation.Vertical };
}

/// <summary>
/// A panel within a group that can be resized by the user.
/// </summary>
public record ResizablePanel : WidgetBase<ResizablePanel>
{
    public ResizablePanel(Size? defaultSize, params object[] children) : base(children)
    {
        DefaultSize = defaultSize;
    }

    [Prop] public Size? DefaultSize { get; init; }
}
