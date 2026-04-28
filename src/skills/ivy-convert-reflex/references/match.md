# Match (Dynamic Rendering)

`rx.match` is Reflex's pattern-matching component for conditional rendering. It takes a reactive value and a series of `(case, component)` tuples, rendering the component whose case matches the current value. It supports multiple cases per branch, a default fallback, and can be used inline as a prop value. It is the multi-branch alternative to `rx.cond`.

## Reflex

```python
from typing import List

import reflex as rx

class MatchState(rx.State):
    cat_breed: str = ""
    animal_options: List[str] = [
        "persian",
        "siamese",
        "maine coon",
        "ragdoll",
        "pug",
        "corgi",
    ]

    @rx.event
    def set_cat_breed(self, breed: str):
        self.cat_breed = breed

def match_demo():
    return rx.flex(
        rx.match(
            MatchState.cat_breed,
            ("persian", rx.text("Persian cat selected.")),
            ("siamese", rx.text("Siamese cat selected.")),
            ("maine coon", rx.text("Maine Coon cat selected.")),
            ("ragdoll", rx.text("Ragdoll cat selected.")),
            rx.text("Unknown cat breed selected."),
        ),
        rx.select.root(
            rx.select.trigger(),
            rx.select.content(
                rx.select.group(
                    rx.foreach(
                        MatchState.animal_options, lambda x: rx.select.item(x, value=x)
                    )
                ),
            ),
            value=MatchState.cat_breed,
            on_change=MatchState.set_cat_breed,
        ),
        direction="column",
        gap="2",
    )
```

## Ivy

Ivy does not have a dedicated `Match` widget. Since Ivy views are written in C#, you use native **switch expressions** and **ternary operators** directly inside the `Build()` method. State changes trigger a re-render automatically, so the switch expression is re-evaluated with the new value.

```csharp
public class MatchDemo : ViewBase
{
    public override object? Build()
    {
        var catBreed = UseState("");
        var options = new[] { "persian", "siamese", "maine coon", "ragdoll", "pug", "corgi" };

        return Layout.Vertical().Gap(2)
            | (catBreed.Value switch
            {
                "persian"   => Text.P("Persian cat selected."),
                "siamese"   => Text.P("Siamese cat selected."),
                "maine coon"=> Text.P("Maine Coon cat selected."),
                "ragdoll"   => Text.P("Ragdoll cat selected."),
                _           => Text.P("Unknown cat breed selected.")
            })
            | catBreed.ToSelectInput(
                options.Select(o => new SelectOption(o, o))
            );
    }
}
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `condition` | The reactive value to match against | C# switch expression scrutinee (any variable/expression) |
| `(case, component)` | Tuple of a matching value and its corresponding component | `"value" => component` arm in a switch expression |
| Multiple cases per branch | `("a", "b", "c", component)` — last element is the return value | `"a" or "b" or "c" => component` using `or` pattern |
| `default_component` | Last non-tuple argument; fallback when no case matches | `_ => component` discard arm in switch expression |
| Inline as prop value | `rx.match(...)` can be used directly as a prop (e.g. `color_scheme=rx.match(...)`) | Switch expression is a C# expression and can be used anywhere a value is expected |
| Reactive re-evaluation | Match is re-evaluated on the client whenever the condition state var changes | `UseState` triggers `Build()` re-execution; the switch expression runs again with the new value |
| Optional default (components) | Default is optional when returning components; implicitly `rx.fragment` | Not supported — C# switch expressions require exhaustive matching or a `_` discard arm |
