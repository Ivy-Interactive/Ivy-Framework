# st.json

Display object or string as a pretty-printed, interactive JSON element.

## Streamlit

```python
import streamlit as st

st.json(
    {
        "foo": "bar",
        "stuff": [
            "stuff 1",
            "stuff 2",
            "stuff 3",
        ],
        "level1": {"level2": {"level3": {"a": "b"}}},
    },
    expanded=2,
)
```

## Ivy

```csharp
public class JsonExample : ViewBase
{
    public override object? Build()
    {
        var data = new
        {
            foo = "bar",
            stuff = new[] { "stuff 1", "stuff 2", "stuff 3" },
            level1 = new { level2 = new { level3 = new { a = "b" } } }
        };

        return new Json(System.Text.Json.JsonSerializer.Serialize(data));
    }
}
```

## Parameters

| Parameter | Documentation                                                                                             | Ivy           |
|-----------|-----------------------------------------------------------------------------------------------------------|---------------|
| body      | The object to display as JSON. All referenced objects must be serializable. Can be an object or a string.  | `Json(string content)` or `Json(JsonNode json)` |
| expanded  | Initial expansion state: `True` (fully expanded), `False` (collapsed), or an integer for specific depth.  | Not supported |
