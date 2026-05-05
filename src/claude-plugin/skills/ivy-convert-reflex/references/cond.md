# Cond (Conditional Rendering)

Conditionally renders components based on a boolean condition. If the condition is `True`, the first component is rendered, otherwise the second component is rendered. If the second component is omitted, nothing is rendered when the condition is `False`.

## Reflex

```python
class CondState(rx.State):
    show: bool = True

    @rx.event
    def change(self):
        self.show = not (self.show)

def cond_example():
    return rx.vstack(
        rx.button("Toggle", on_click=CondState.change),
        rx.cond(
            CondState.show,
            rx.text("Text 1", color="blue"),
            rx.text("Text 2", color="red"),
        ),
    )
```

### Negation

```python
rx.cond(
    ~CondState.show,
    rx.text("Text 1", color="blue"),
    rx.text("Text 2", color="red"),
)
```

### Multiple Conditions

```python
rx.cond(
    (CondComplexState.age >= 18) & (CondComplexState.age <= 65),
    rx.text("You can work!", color="green"),
    rx.text("You cannot work!", color="red"),
)
```

### Nested Conditionals

```python
rx.cond(
    NestedState.num > 0,
    rx.text(f"{NestedState.num} is Positive!", color="orange"),
    rx.cond(
        NestedState.num == 0,
        rx.text(f"{NestedState.num} is Zero!", color="blue"),
        rx.text(f"{NestedState.num} is Negative!", color="red"),
    ),
)
```

## Ivy

Ivy does not have a dedicated `cond` component. Instead, it uses standard C# ternary expressions (`? :`) directly in the `Build()` method. Returning `null` hides a component.

```csharp
public class CondExample : ViewBase
{
    public override object? Build()
    {
        var show = UseState(true);

        return Layout.Vertical()
            | new Button(show.Value ? "Hide" : "Show",
                onClick: _ => show.Set(!show.Value))
            | (show.Value
                ? Text.Success("Text 1")
                : Text.Muted("Text 2"));
    }
}
```

### Negation

```csharp
// Use the ! operator to negate a condition
(!show.Value ? Text.Success("Text 1") : Text.Muted("Text 2"))
```

### Multiple Conditions

```csharp
(age.Value >= 18 && age.Value <= 65
    ? Text.Success("You can work!")
    : Text.Danger("You cannot work!"))
```

### Nested Conditionals

```csharp
(num.Value > 0
    ? Text.P($"{num.Value} is Positive!")
    : num.Value == 0
        ? Text.P($"{num.Value} is Zero!")
        : Text.P($"{num.Value} is Negative!"))
```

## Parameters

| Parameter           | Documentation                                                                                    | Ivy                                                              |
|---------------------|--------------------------------------------------------------------------------------------------|------------------------------------------------------------------|
| `condition`         | A boolean expression that determines which component to render                                   | Standard C# boolean expression in a ternary (`? :`)             |
| `true` component    | The component rendered when the condition is `True`                                              | The expression before `:` in the ternary                         |
| `false` component   | Optional component rendered when the condition is `False`; if omitted, nothing renders           | Return `null` after `:` to render nothing                        |
| `~` (negation)      | Negates the condition using the `~` operator                                                     | Use `!` operator                                                 |
| `\|` (logical OR)   | Combines conditions with logical OR                                                              | Use `\|\|` operator                                              |
| `&` (logical AND)   | Combines conditions with logical AND                                                             | Use `&&` operator                                                |
| Nested `cond`       | Nest `rx.cond()` calls inside each other for if/elif/else chains                                 | Use chained ternary expressions or standard `if/else` in C#     |
