using System.Collections.Immutable;

namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.ScrollText, group: ["Widgets", "Primitives"], searchHints: ["scroll", "log", "feed", "auto", "stream", "tail"])]
public class AutoScrollApp : SampleBase
{
    private static ImmutableArray<string> AutoScrollLogSeed() =>
        ImmutableArray.Create(
            "[AutoScroll] This sample uses new AutoScroll(…) — arbitrary children, fixed height, scrolls to the bottom when content grows.",
            "[AutoScroll] Turn off “Follow new lines” to read earlier lines without being pulled down.",
            "[AutoScroll] Append lines to simulate a live log or activity feed.");

    protected override object? BuildSample()
    {
        var logLines = UseState(AutoScrollLogSeed());
        var autoscrollFollow = UseState(true);

        var autoScrollPanel = new AutoScroll(
                logLines.Value.Select(line => Text.Muted(line)).ToArray<object>())
            .Height(Size.Px(100))
            .Width(Size.Full())
            .Enabled(autoscrollFollow.Value);

        return Layout.Vertical()
               | Text.H1("AutoScroll")
               | Text.P(
                   "A scrollable container that keeps the bottom in view when children change — useful for logs, feeds, or any append-only output. Set a height (and width when needed) so overflow scrolling works.")

               | (new Card(
                   Layout.Vertical()
                   | Text.P(
                       "Typical usage: new AutoScroll(children).Height(Size.Px(320)).Width(Size.Full()).Enabled(follow);")
                       .Small()
                   | autoScrollPanel
                   | (Layout.Horizontal().Wrap()
                      | new Button(
                          "Append line",
                          () =>
                              logLines.Set(
                                  logLines.Value.Add($"[{DateTime.Now:HH:mm:ss}] Line #{logLines.Value.Length + 1}")))
                      | new Button("Reset", () => logLines.Set(AutoScrollLogSeed())).Variant(ButtonVariant.Outline)
                      | autoscrollFollow.ToBoolInput().Label("Follow new lines").Variant(BoolInputVariant.Switch))
               ).Title("AutoScroll widget"));
    }
}
