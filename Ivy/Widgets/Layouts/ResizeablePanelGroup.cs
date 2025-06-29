using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record ResizeablePanelGroup : WidgetBase<ResizeablePanelGroup>
{
    public ResizeablePanelGroup(params ResizeablePanel[] children) : base(children.Cast<object>().ToArray())
    {
        Width = Size.Full();
        Height = Size.Full();
    }

    [Prop] public bool ShowHandle { get; init; } = true;
    [Prop] public Orientation Direction { get; init; } = Orientation.Horizontal;
}

public static class ResizeablePanelsExtensions
{
    public static ResizeablePanelGroup ShowHandle(this ResizeablePanelGroup widget, bool value) => widget with { ShowHandle = value };
    public static ResizeablePanelGroup Direction(this ResizeablePanelGroup widget, Orientation value) => widget with { Direction = value };
    public static ResizeablePanelGroup Horizontal(this ResizeablePanelGroup widget) => widget with { Direction = Orientation.Horizontal };
    public static ResizeablePanelGroup Vertical(this ResizeablePanelGroup widget) => widget with { Direction = Orientation.Vertical };
}

public record ResizeablePanel : WidgetBase<ResizeablePanel>
{
    public ResizeablePanel(int? defaultSize, params object[] children) : base(children)
    {
        DefaultSize = defaultSize;
    }

    [Prop] public int? DefaultSize { get; init; }
}
