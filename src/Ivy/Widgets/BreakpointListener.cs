// ReSharper disable once CheckNamespace
namespace Ivy;

public record BreakpointListener : WidgetBase<BreakpointListener>
{
    public BreakpointListener()
    {
    }

    [Event] public EventHandler<Event<BreakpointListener, Breakpoint>>? OnChange { get; set; }
}
