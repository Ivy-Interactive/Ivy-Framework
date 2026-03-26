---
searchHints:
  - auto scroll
  - scroll
  - log
  - feed
  - tail
  - stream
  - overflow
  - container
---

# AutoScroll

<Ingress>
Keep the newest content in view inside a fixed-height scroll area — ideal for live logs, activity feeds, or any append-only output in your [views](../../01_Onboarding/02_Concepts/02_Views.md).
</Ingress>

The `AutoScroll` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) is a scrollable container that automatically scrolls to the bottom when its children grow. Pass any widgets as children (not only text). Give the container an explicit [Size](../../04_ApiReference/Ivy/Size.md) — especially height — so the browser can show a scrollbar and the follow behavior can measure overflow.

## Basic Usage

`AutoScroll` takes a `params` array of children. Use `Height` (and optionally `Width`) from `WidgetBase` so the scroll region is bounded.

```csharp demo-tabs
public class AutoScrollBasicDemo : ViewBase
{
    public override object? Build()
    {
        var lines = UseState(ImmutableArray.Create("First line", "Second line"));

        return Layout.Vertical()
            | new AutoScroll(
                  lines.Value.Select(l => Text.Muted(l)).ToArray<object>())
              .Height(Size.Px(100))
              .Width(Size.Full())
            | new Button("Add line", () =>
                lines.Set(lines.Value.Add($"Line at {DateTime.Now:HH:mm:ss}")));
    }
}
```

## Enabled follow

When `Enabled` is `true` (default), new content scrolls the view to the bottom. When `false`, the user can scroll manually but the view does not jump when children update — useful when someone is reading older lines while new data arrives.

Use the fluent `.Enabled(bool)` extension method:

```csharp demo-tabs
public class AutoScrollEnabledDemo : ViewBase
{
    public override object? Build()
    {
        var lines = UseState(ImmutableArray.Create("Line A", "Line B"));
        var follow = UseState(true);

        return Layout.Vertical()
            | new AutoScroll(
                  lines.Value.Select(l => Text.Block(l)).ToArray<object>())
              .Height(Size.Px(100))
              .Width(Size.Full())
              .Enabled(follow.Value)
            | (Layout.Horizontal()
                | new Button("Append", () =>
                    lines.Set(lines.Value.Add($"Entry {lines.Value.Length + 1}")))
                | follow.ToBoolInput().Label("Follow new lines").Variant(BoolInputVariant.Switch));
    }
}
```

While follow is on, scrolling up with the wheel or touch pauses auto-follow until the user scrolls back to the bottom (same behavior as the chat message list).

<WidgetDocs Type="Ivy.AutoScroll" ExtensionTypes="Ivy.AutoScrollExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Primitives/AutoScroll.cs"/>
