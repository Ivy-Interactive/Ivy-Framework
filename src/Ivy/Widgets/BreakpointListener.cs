// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// An invisible widget that reports the browser's current responsive <see cref="Breakpoint"/>
/// back to the server. Rendered into the tree by <see cref="UseBreakpointExtensions.UseBreakpoint"/>;
/// it produces no visible UI and exists solely to round-trip the active breakpoint so server-side
/// code can branch its layout on screen size.
/// </summary>
public record BreakpointListener : WidgetBase<BreakpointListener>
{
    /// <summary>Initializes a new <see cref="BreakpointListener"/>.</summary>
    public BreakpointListener()
    {
    }

    /// <summary>
    /// Raised by the frontend whenever the active breakpoint changes (and once on mount), carrying
    /// the newly resolved <see cref="Breakpoint"/>.
    /// </summary>
    [Event] public EventHandler<Event<BreakpointListener, Breakpoint>>? OnChange { get; set; }
}
