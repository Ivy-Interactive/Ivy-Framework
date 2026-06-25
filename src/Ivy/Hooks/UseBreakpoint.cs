// ReSharper disable once CheckNamespace
namespace Ivy;

public static class UseBreakpointExtensions
{
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
