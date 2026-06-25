using System.Collections.Immutable;

namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.MonitorSmartphone, group: ["Widgets", "Primitives"], searchHints: ["breakpoint", "responsive", "mobile", "resize", "screen", "media query"])]
public class BreakpointListenerApp : SampleBase
{
    protected override object? BuildSample()
    {
        var (breakpoint, listener) = Context.UseBreakpoint();

        var log = UseState(ImmutableArray<string>.Empty);

        UseEffect(() =>
        {
            log.Set(log.Value.Add($"[{DateTime.Now:HH:mm:ss}] Breakpoint → {breakpoint.Value}"));
        }, breakpoint);

        var currentCard = new Card(
            Layout.Vertical()
            | Text.Muted("Current breakpoint")
            | Text.H2(breakpoint.Value.ToString())
            | Text.Muted("Thresholds: Mobile < 640px ≤ Tablet < 768px ≤ Desktop < 1024px ≤ Wide").Small()
        ).Title("Live value");

        var logCard = new Card(
            AutoScroll.FromChildren(log.Value.Select(line => Text.Muted(line)))
                .Height(Size.Px(180))
                .Width(Size.Full())
        ).Title("OnChange events");

        return new Fragment(
            listener,
            Layout.Vertical()
            | Text.H1("BreakpointListener")
            | Text.P(
                "Reports the browser's active responsive breakpoint back to the server so C# code can branch "
                + "on screen size. Drag the window narrower and wider — crossing 640px, 768px, or 1024px fires "
                + "an OnChange event and updates the value below.")
            | Text.P("Usage: var (breakpoint, listener) = Context.UseBreakpoint(); render listener, read breakpoint.Value.")
                .Small()
            | (Layout.Horizontal().Wrap()
               | currentCard
               | logCard)
        );
    }
}
