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

The `AutoScroll` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) is a scrollable container that follows new content to the bottom when its children grow. Pass any widgets as children (not only text). Give the container an explicit [Size](../../04_ApiReference/Ivy/Size.md) — especially height — so the browser can show a scrollbar and the follow behavior can measure overflow.

## Basic Usage

`AutoScroll` takes a `params` array of children. Use `Height` (and optionally `Width`) from `WidgetBase` so the scroll region is bounded.

When you build children from LINQ (for example `lines.Select(…)`) use `AutoScroll.FromChildren(…)` so you do not need `ToArray<object>()` — C# does not pass a `Widget[]` as `params object[]`.

```csharp demo-tabs
public class AutoScrollBasicDemo : ViewBase
{
    public override object? Build()
    {
        var lines = UseState(ImmutableArray.Create("First line", "Second line"));

        return Layout.Vertical()
            | AutoScroll.FromChildren(lines.Value.Select(l => Text.Muted(l)))
              .Height(Size.Px(100))
            | new Button("Add line", () =>
                lines.Set(lines.Value.Add($"Line at {DateTime.Now:HH:mm:ss}")));
    }
}
```

## Disabled

`Disabled` defaults to **`false`**: new or resized content scrolls the view to the bottom. Set **`Disabled(true)`** (fluent extension, same style as [Button](../../03_Common/01_Button.md) or [Card](../../03_Common/04_Card.md)) when the user should scroll manually and the view must not jump when children update — for example while reading older lines in a live log.

```csharp demo-tabs
public class AutoScrollDisabledDemo : ViewBase
{
    public override object? Build()
    {
        var lines = UseState(ImmutableArray.Create("Line A", "Line B"));
        var follow = UseState(true);

        return Layout.Vertical()
            | AutoScroll.FromChildren(lines.Value.Select(l => Text.Block(l)))
              .Height(Size.Px(100))
              .Width(Size.Full())
              .Disabled(!follow.Value)
            | (Layout.Horizontal()
                | new Button("Append", () =>
                    lines.Set(lines.Value.Add($"Entry {lines.Value.Length + 1}")))
                | follow.ToBoolInput().Label("Follow new lines").Variant(BoolInputVariant.Switch));
    }
}
```

While follow is on, scrolling up with the wheel or touch pauses auto-follow until the user scrolls back to the bottom (same behavior as the chat message list).

<WidgetDocs Type="Ivy.AutoScroll" ExtensionTypes="Ivy.AutoScrollExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Primitives/AutoScroll.cs"/>
