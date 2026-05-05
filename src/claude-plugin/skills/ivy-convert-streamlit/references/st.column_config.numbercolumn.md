# NumberColumn

Configure a number column in a data table. This is the default column type for integer and float values. It supports numeric formatting (currency, percent, scientific, etc.) and constraining input with min/max/step values.

## Streamlit

```python
import pandas as pd
import streamlit as st

data_df = pd.DataFrame({"price": [20, 950, 250, 500]})

st.data_editor(
    data_df,
    column_config={
        "price": st.column_config.NumberColumn(
            "Price (in USD)",
            help="The price of the product in USD",
            min_value=0,
            max_value=1000,
            step=1,
            format="$%d",
        )
    },
    hide_index=True,
)
```

## Ivy

```csharp
public record Product(int Id, string Name, decimal Price);

[App]
public class ProductTableApp : ViewBase
{
    public override object? Build()
    {
        var products = new List<Product>
        {
            new(1, "Widget", 20),
            new(2, "Gadget", 950),
            new(3, "Doohickey", 250),
            new(4, "Thingamajig", 500),
        }.AsQueryable();

        return products.ToDataTable(p => p.Id)
            .Header(p => p.Price, "Price (in USD)")
            .Help(p => p.Price, "The price of the product in USD")
            .Align(p => p.Price, Align.Right)
            .Icon(p => p.Price, Icons.DollarSign.ToString())
            .Config(config =>
            {
                config.AllowSorting = true;
                config.AllowFiltering = true;
            })
            .Height(Size.Units(100));
    }
}
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| label | The label shown at the top of the column. Uses the column name if `None`. | `.Header(x => x.Prop, "Label")` |
| help | Tooltip displayed when hovering over the column header. Supports GitHub-flavored Markdown. | `.Help(x => x.Prop, "tooltip text")` |
| disabled | Whether editing is disabled for this column. Defaults to `False`. | Not supported (DataTable is read-only; use forms for editable input) |
| required | Whether the column requires a value when adding new rows. Defaults to `False`. | Not supported (use `[Required]` attribute in form models) |
| pinned | Whether the column is pinned to the left side during horizontal scrolling. | `config.FreezeColumns = N` (freezes the first N columns) |
| default | Default value when a user adds a new row. | Not supported (DataTable does not support adding rows inline) |
| format | Printf-style format string (e.g. `"$%d"`, `"%.2f%%"`) or a preset like `"dollar"`, `"percent"`, `"compact"`, `"scientific"`, etc. | Not supported (numbers are auto-formatted by `DefaultContentBuilder`; custom formatting requires a custom `IContentBuilder`) |
| min_value | Minimum allowed value for input. | Not supported (use `[Range(min, max)]` attribute or `.Min()` on `NumberInput` in forms) |
| max_value | Maximum allowed value for input. | Not supported (use `[Range(min, max)]` attribute or `.Max()` on `NumberInput` in forms) |
| step | Step increment for the input control. Defaults to `1` for integers, unrestricted for floats. | Not supported (use `.Step()` on `NumberInput` in forms) |
