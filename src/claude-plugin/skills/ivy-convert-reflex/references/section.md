# Section

A semantic container that groups related content vertically with configurable padding. In Reflex this renders as an HTML `<section>` element with vertical spacing controlled by a `size` prop.

## Reflex

```python
import reflex as rx

def index():
    return rx.box(
        rx.section(
            rx.heading("First"),
            rx.text("This is the first content section"),
        ),
        rx.section(
            rx.heading("Second"),
            rx.text("This is the second content section"),
            size="3",
        ),
    )
```

## Ivy

Ivy does not have a dedicated `Section` widget. The closest equivalent is `StackLayout`, which arranges children vertically by default with configurable gap and padding.

```csharp
public class SectionExample : ViewBase
{
    public override object? Build()
    {
        return new StackLayout([
            new StackLayout([
                Text.H2("First"),
                Text.Label("This is the first content section")
            ], padding: new Thickness(4)),

            new StackLayout([
                Text.H2("Second"),
                Text.Label("This is the second content section")
            ], padding: new Thickness(6)),
        ]);
    }
}
```

## Parameters

| Parameter | Documentation                                          | Ivy                                                                 |
|-----------|--------------------------------------------------------|---------------------------------------------------------------------|
| `size`    | Controls vertical padding. Values `"1"` through `"4"`, default `"2"` | Use `padding` (`Thickness`) and `gap` (`int`) on `StackLayout` |
| children  | Arbitrary child components                             | `children` (`Object[]`) constructor parameter                       |
| semantic  | Renders as HTML `<section>` element                    | Not supported (no semantic HTML element mapping)                    |
