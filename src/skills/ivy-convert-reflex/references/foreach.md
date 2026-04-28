# Foreach

Iterates over a collection (list, tuple, or dictionary) and renders each item using a provided function. Used for dynamically rendering lists of items from application state.

## Reflex

```python
from typing import List

class ForeachState(rx.State):
    color: List[str] = ["red", "green", "blue", "yellow", "orange", "purple"]

def colored_box(color: str):
    return rx.box(rx.text(color), bg=color)

def foreach_example():
    return rx.grid(
        rx.foreach(ForeachState.color, colored_box),
        columns="2",
    )
```

### With index

```python
def colored_box_index(color: str, index: int):
    return rx.box(rx.text(index), bg=color)

def foreach_example_index():
    return rx.grid(
        rx.foreach(
            ForeachState.color, lambda color, index: colored_box_index(color, index)
        ),
        columns="2",
    )
```

### Nested foreach

```python
class NestedForeachState(rx.State):
    numbers: List[List[str]] = [["1", "2", "3"], ["4", "5", "6"], ["7", "8", "9"]]

def display_row(row):
    return rx.hstack(
        rx.foreach(
            row,
            lambda item: rx.box(item, border="1px solid black", padding="0.5em"),
        ),
    )

def nested_foreach_example():
    return rx.vstack(rx.foreach(NestedForeachState.numbers, display_row))
```

### Dictionary iteration

```python
class SimpleDictForeachState(rx.State):
    color_chart: dict[int, str] = {1: "blue", 2: "red", 3: "green"}

def display_color(color: List):
    return rx.box(rx.text(color[0]), bg=color[1])

def foreach_dict_example():
    return rx.grid(
        rx.foreach(SimpleDictForeachState.color_chart, display_color), columns="2"
    )
```

## Ivy

Ivy does not have a dedicated `foreach` component. Instead, C#'s LINQ `.Select()` method is used to project collections into UI widgets. The pipe operator (`|`) composes the results into layouts.

```csharp
public class ForeachExample : ViewBase
{
    public override object? Build()
    {
        var colors = UseState(new[] { "red", "green", "blue", "yellow", "orange", "purple" });

        return Layout.Grid()
            | colors.Value.Select(color =>
                new Box(new Text(color)).BackgroundColor(color)
            );
    }
}
```

### With index

```csharp
colors.Value.Select((color, index) =>
    new Box(new Text(index.ToString())).BackgroundColor(color)
)
```

### Nested iteration

```csharp
var numbers = UseState(new[] {
    new[] { "1", "2", "3" },
    new[] { "4", "5", "6" },
    new[] { "7", "8", "9" }
});

return Layout.Vertical()
    | numbers.Value.Select(row =>
        Layout.Horizontal()
            | row.Select(item => new Box(new Text(item)))
    );
```

### Dictionary iteration

```csharp
var colorChart = UseState(new Dictionary<int, string> {
    { 1, "blue" }, { 2, "red" }, { 3, "green" }
});

return Layout.Grid()
    | colorChart.Value.Select(kvp =>
        new Box(new Text(kvp.Key.ToString())).BackgroundColor(kvp.Value)
    );
```

## Parameters

| Parameter        | Documentation                                                                 | Ivy                                                        |
|------------------|-------------------------------------------------------------------------------|------------------------------------------------------------|
| `iterable`       | The state var (list, tuple, or dict) to iterate over                          | Any `IEnumerable<T>` — passed directly via LINQ `.Select()` |
| `render_fn`      | A function that receives each item and returns a component                    | Lambda expression passed to `.Select(item => ...)`         |
| `index`          | Optional second argument in the render function for the current item's index  | `.Select((item, index) => ...)` overload                   |
| Nested foreach   | Nest `rx.foreach` calls; requires explicit type annotations for type checking | Nest `.Select()` calls — C# type inference handles types   |
| Dict iteration   | Dicts are converted to key-value pair lists (`color[0]`, `color[1]`)          | Use `KeyValuePair<K,V>` with `kvp.Key` and `kvp.Value`    |
