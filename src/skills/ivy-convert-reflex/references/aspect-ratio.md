# Aspect Ratio

Displays content within a container that maintains a desired width-to-height ratio. Setting the `ratio` prop adjusts the dimensions so that `width / height` equals the specified value. For responsive scaling, the child content should use `width="100%"` and `height="100%"` rather than setting dimensions on the `aspect_ratio` component itself.

## Reflex

```python
rx.grid(
    rx.aspect_ratio(
        rx.box(
            "Widescreen 16:9",
            background_color="papayawhip",
            width="100%",
            height="100%",
        ),
        ratio=16 / 9,
    ),
    rx.aspect_ratio(
        rx.box(
            "Square 1:1",
            background_color="green",
            width="100%",
            height="100%",
        ),
        ratio=1,
    ),
    spacing="2",
    width="25%",
)
```

## Ivy

Ivy does not have a dedicated AspectRatio widget. The closest approach is to use a `Box` with explicit `Width` and `Height` values that maintain the desired ratio manually.

```csharp
new Box(
    new Text("Widescreen 16:9")
).Width(Size.Units(320)).Height(Size.Units(180)).Color(Colors.Warning)
```

## Parameters

| Parameter | Documentation                                                                 | Ivy           |
|-----------|-------------------------------------------------------------------------------|---------------|
| ratio     | `Union[int, float]` &mdash; The desired width/height ratio (e.g. `16/9`, `1`) | Not supported |
