# st.table

Display a static table. Designed for small, styled tables without interactive features like sorting or scrolling. Ideal for displaying matrices, leaderboards, and supports Markdown formatting in all cells.

## Streamlit

```python
import pandas as pd
import streamlit as st

confusion_matrix = pd.DataFrame(
    {
        "Predicted Cat": [85, 3, 2, 1],
        "Predicted Dog": [2, 78, 4, 0],
        "Predicted Bird": [1, 5, 72, 3],
        "Predicted Fish": [0, 2, 1, 89],
    },
    index=["Actual Cat", "Actual Dog", "Actual Bird", "Actual Fish"],
)
st.table(confusion_matrix)
```

## Ivy

```csharp
public class TableExample : ViewBase
{
    public override object? Build()
    {
        var products = new[]
        {
            new { Sku = "1234", Name = "T-shirt", Price = 10 },
            new { Sku = "1235", Name = "Jeans", Price = 20 }
        };

        return products.ToTable()
            .Width(Size.Full())
            .Header(p => p.Price, "Unit Price")
            .Align(p => p.Price, Align.Right);
    }
}
```

## Parameters

| Parameter | Documentation                                                                                                                                             | Ivy                                    |
|-----------|-----------------------------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------------|
| data      | The table data. Can be anything supported by `st.dataframe`. All cells including index and column headers can contain GitHub-flavored Markdown.            | `ToTable()` extension on `IEnumerable` or `new Table(TableRow[] rows)` |
| border    | Controls border visibility: `True` (default) shows borders around and between cells; `False` removes all borders; `"horizontal"` shows only horizontal borders between rows. | Not supported                           |
