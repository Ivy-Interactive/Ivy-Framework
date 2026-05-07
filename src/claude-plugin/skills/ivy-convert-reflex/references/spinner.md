# Spinner

Displays an animated loading indicator to provide visual feedback while a task is in progress.

## Reflex

```python
rx.spinner()

rx.hstack(
    rx.spinner(size="1"),
    rx.spinner(size="2"),
    rx.spinner(size="3"),
)

# Built-in button loading state
rx.button("Bookmark", loading=True)

# Spinner wrapping an icon inside a button
rx.button(rx.spinner(loading=True), "Bookmark", disabled=True)
```

## Ivy

Ivy has no dedicated Spinner widget. The equivalent is achieved by applying a rotate animation to a loader icon:

```csharp
Icons.LoaderCircle
    .ToIcon()
    .Color(Colors.Blue)
    .WithAnimation(AnimationType.Rotate)
    .Trigger(AnimationTrigger.Auto)
    .Duration(2)
```

## Parameters

| Parameter | Documentation                                      | Ivy                                                                 |
|-----------|----------------------------------------------------|---------------------------------------------------------------------|
| `size`    | Controls spinner dimensions (`"1"`, `"2"`, `"3"`)  | Not supported (use `.Scale()` or `.Height()` / `.Width()` on icon)  |
| `loading` | `bool` — whether the spinner is animating           | Not supported (control visibility or add/remove the animation)      |
