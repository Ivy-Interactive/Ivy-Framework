# Inset

Applies a negative margin to allow content to bleed into the surrounding container. Commonly used inside a `Card` to render images or other content edge-to-edge, ignoring the card's default padding.

## Reflex

```python
rx.card(
    rx.inset(
        rx.image(src="/reflex_banner.png", width="100%", height="auto"),
        side="top",
        pb="current",
    ),
    rx.text(
        "Reflex is a web framework that allows developers to build their app in pure Python."
    ),
    width="25vw",
)
```

## Ivy

Ivy does not have a dedicated Inset widget. The closest primitive is `Box`, which supports `Padding` and `Margin` via `Thickness` but does **not** support negative margins or content bleeding into a parent container.

A partial workaround is to set zero padding on the Card/Box and manually control spacing on the non-bleed content:

```csharp
new Card(
    new StackLayout(
        new Image("reflex_banner.png").Width(Size.Percent(100)),
        new Text("Reflex is a web framework that allows developers to build their app in pure Python.")
            .WithMargin(new Thickness(4))
    ).Gap(0)
).Width(Size.Units(100))
```

> **Note:** This is not a true equivalent — Reflex's `Inset` surgically removes padding on a chosen side while preserving it on the others. In Ivy you would need to manage padding manually on sibling elements.

## Parameters

| Parameter | Documentation                                                                 | Ivy           |
|-----------|-------------------------------------------------------------------------------|---------------|
| `side`    | `"x" \| "y" \| "top" \| "bottom" \| "left" \| "right"` — which edges receive the negative margin | Not supported |
| `clip`    | `"border-box" \| "padding-box"` — clipping behavior for overflowing content   | Not supported |
| `p`       | `Union[int, str]` — padding on all sides                                      | `Padding` (`Thickness`) on `Box` |
| `px`      | `Union[int, str]` — horizontal padding                                        | `Thickness(left, 0, right, 0)` on `Box` |
| `py`      | `Union[int, str]` — vertical padding                                          | `Thickness(0, top, 0, bottom)` on `Box` |
| `pt`      | `Union[int, str]` — top padding                                               | `Thickness(0, top, 0, 0)` on `Box` |
| `pr`      | `Union[int, str]` — right padding                                             | `Thickness(0, 0, right, 0)` on `Box` |
| `pb`      | `Union[int, str]` — bottom padding                                            | `Thickness(0, 0, 0, bottom)` on `Box` |
| `pl`      | `Union[int, str]` — left padding                                              | `Thickness(left, 0, 0, 0)` on `Box` |
