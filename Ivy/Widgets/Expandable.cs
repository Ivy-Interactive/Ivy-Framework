using Ivy.Core;
using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Expandable : WidgetBase<Expandable>
{
    public Expandable(object header, object content) : base([new Slot("Header", header), new Slot("Content", content)])
    {

    }

    [Prop] public bool Disabled { get; set; } = false;

    [Prop] public bool Open { get; set; } = false;

    /// <summary>
    /// When true, shows a switch in the header that controls whether the expandable is enabled.
    /// </summary>
    [Prop] public bool EnableToggle { get; set; } = false;

    [Event] public Func<Event<Expandable, bool>, ValueTask>? OnEnableToggleChange { get; set; }
}

public static class ExpandableExtensions
{
    public static Expandable Disabled(this Expandable widget, bool disabled = true)
    {
        widget.Disabled = disabled;
        return widget;
    }

    public static Expandable Open(this Expandable widget, bool open = true)
    {
        widget.Open = open;
        return widget;
    }

    /// <summary>
    /// Adds a switch to the header that controls whether the expandable is enabled.
    /// </summary>
    /// <param name="widget">The expandable widget</param>
    /// <param name="enabledState">State that controls whether the expandable is enabled</param>
    public static Expandable WithEnableToggle(this Expandable widget, IState<bool> enabledState)
    {
        widget.EnableToggle = true;
        widget.Disabled = !enabledState.Value;
        widget.OnEnableToggleChange = e =>
        {
            enabledState.Set(e.Value);
            return ValueTask.CompletedTask;
        };
        return widget;
    }

    /// <summary>
    /// Adds a switch to the header that controls whether the expandable is enabled.
    /// The state is managed internally (starts disabled).
    /// </summary>
    public static IView WithEnableToggle(this Expandable widget)
    {
        return new WithEnableToggleView(widget);
    }
}

public class WithEnableToggleView(Expandable expandable) : ViewBase
{
    public override object? Build()
    {
        var enabledState = this.UseState(false);
        return expandable.WithEnableToggle(enabledState);
    }
}
