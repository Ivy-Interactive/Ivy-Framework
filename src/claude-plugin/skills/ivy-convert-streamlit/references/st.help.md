# st.help

Displays help and other information for a given object, including its name, type, value, signature, docstring, and member variables/methods. Useful for debugging and inspecting objects at runtime.

## Streamlit

```python
import streamlit as st
import pandas

st.help(pandas.DataFrame)

class Dog:
    '''A typical dog.'''
    def __init__(self, breed, color):
        self.breed = breed
        self.color = color
    def bark(self):
        return 'Woof!'

fido = Dog("poodle", "white")
st.help(fido)
```

## Ivy

There is no direct equivalent. The closest alternative is the `Json` widget, which can display serialized object data in a formatted, syntax-highlighted view.

```csharp
using System.Text.Json;
using Ivy;
using Ivy.Widgets.Primitives;

var dog = new { Breed = "poodle", Color = "white", Type = "Dog" };
var json = JsonSerializer.Serialize(dog, new JsonSerializerOptions { WriteIndented = true });

new StackLayout(direction: LayoutDirection.Vertical, gap: 4)
{
    new Json(json)
};
```

## Parameters

| Parameter | Documentation                                                                                       | Ivy           |
|-----------|-----------------------------------------------------------------------------------------------------|---------------|
| obj       | The object whose information should be displayed. If left unspecified, displays help for Streamlit. | `Json(string content)` or `Json(JsonNode json)` accepts serialized object data |
