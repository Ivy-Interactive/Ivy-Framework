using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Represents a resizable panel group widget that creates layouts with multiple panels separated by draggable handles, allowing users to resize sections interactively.
/// This widget supports both horizontal and vertical orientations and can be nested for complex layout arrangements.
/// </summary>
public record ResizeablePanelGroup : WidgetBase<ResizeablePanelGroup>
{
    /// <summary>
    /// Initializes a new instance of the ResizeablePanelGroup class with the specified resizable panels.
    /// The panel group will automatically size itself to fill the available space and arrange the panels according to the specified direction and handle visibility settings.
    /// </summary>
    /// <param name="children">Variable number of ResizeablePanel elements to arrange in the resizable layout. Each panel can have its own default size and content, and will be separated by draggable handles.</param>
    public ResizeablePanelGroup(params ResizeablePanel[] children) : base(children.Cast<object>().ToArray())
    {
        Width = Size.Full();
        Height = Size.Full();
    }

    /// <summary>
    /// Gets or sets whether the resize handles between panels are visible and interactive.
    /// When true, users can see and drag the handles to resize panels. When false, the handles are hidden and panels maintain their default sizes without the ability to resize.
    /// This property is useful for creating both interactive resizable layouts and fixed-size panel arrangements using the same widget structure.
    /// </summary>
    [Prop] public bool ShowHandle { get; init; } = true;

    /// <summary>
    /// Gets or sets the orientation direction for arranging panels within the group.
    /// This controls whether panels are arranged horizontally (side by side) or vertically (stacked on top of each other), affecting both the layout direction and handle orientation.
    /// Horizontal orientation creates vertical drag handles, while vertical orientation creates horizontal drag handles for resizing panels.
    /// </summary>
    [Prop] public Orientation Direction { get; init; } = Orientation.Horizontal;
}

/// <summary>
/// Provides extension methods for the ResizeablePanelGroup widget that enable a fluent API for configuring panel group behavior and orientation.
/// These methods allow you to easily control handle visibility, set direction, and use convenience methods for common orientation settings.
/// </summary>
public static class ResizeablePanelsExtensions
{
    public static ResizeablePanelGroup ShowHandle(this ResizeablePanelGroup widget, bool value) => widget with { ShowHandle = value };

    public static ResizeablePanelGroup Direction(this ResizeablePanelGroup widget, Orientation value) => widget with { Direction = value };

    public static ResizeablePanelGroup Horizontal(this ResizeablePanelGroup widget) => widget with { Direction = Orientation.Horizontal };

    public static ResizeablePanelGroup Vertical(this ResizeablePanelGroup widget) => widget with { Direction = Orientation.Vertical };
}

/// <summary>
/// Represents an individual resizable panel within a ResizeablePanelGroup that can contain any content and has a configurable default size.
/// Each panel can be resized by users dragging the handles between panels, allowing for dynamic layout adjustments.
/// </summary>
public record ResizeablePanel : WidgetBase<ResizeablePanel>
{
    /// <summary>
    /// Initializes a new instance of the ResizeablePanel class with the specified default size and content.
    /// The panel will be sized according to the default size setting and can contain any combination of widgets or content elements.
    /// </summary>
    public ResizeablePanel(int? defaultSize, params object[] children) : base(children)
    {
        DefaultSize = defaultSize;
    }

    /// <summary>
    /// Gets or sets the default size of the panel as a percentage of the total available space.
    /// When null, the panel will automatically size itself based on available space and content, distributing remaining space evenly among auto-sized panels.
    /// When specified as a percentage (e.g., 30 for 30%), the panel will maintain approximately this proportion of the total space.
    /// </summary>
    [Prop] public int? DefaultSize { get; init; }
}
