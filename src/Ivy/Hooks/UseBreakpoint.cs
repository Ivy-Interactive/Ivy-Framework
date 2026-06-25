// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Hook for reading the browser's current responsive <see cref="Breakpoint"/> from server-side code.
/// </summary>
public static class UseBreakpointExtensions
{
    /// <summary>
    /// Tracks the browser's active responsive <see cref="Breakpoint"/> so server-side code can branch
    /// its layout on screen size (for example, rendering a bottom <c>Sheet</c> on mobile and a
    /// <c>Dialog</c> otherwise).
    /// </summary>
    /// <param name="context">The view context.</param>
    /// <param name="initial">
    /// The breakpoint assumed before the browser reports its first measurement. Defaults to
    /// <see cref="Breakpoint.Desktop"/>, so the first render behaves like a wide layout until the
    /// frontend corrects it (typically within the same paint).
    /// </param>
    /// <returns>
    /// A tuple of the current breakpoint state and a listener widget. The listener <b>must be rendered
    /// somewhere in the returned tree</b> (it is invisible) for the breakpoint to update.
    /// </returns>
    public static (IState<Breakpoint> breakpoint, object listener) UseBreakpoint(
        this IViewContext context,
        Breakpoint initial = Breakpoint.Desktop)
    {
        var breakpoint = context.UseState(initial);

        var listener = new BreakpointListener
        {
            OnChange = new(e =>
            {
                breakpoint.Set(e.Value);
                return ValueTask.CompletedTask;
            })
        };

        return (breakpoint, listener);
    }
}
