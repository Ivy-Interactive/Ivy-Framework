# Component: Notebook (Tabs)

Odoo's tabbed section container within form views. Organizes related fields into separate tab pages, reducing visual complexity of dense forms. Each `<page>` becomes a clickable tab.

## Odoo

```xml
<form>
    <sheet>
        <!-- Main fields above tabs -->
        <group>
            <field name="name"/>
            <field name="partner_id"/>
        </group>

        <!-- Tabbed sections -->
        <notebook>
            <page string="Order Lines" name="order_lines">
                <field name="order_line">
                    <tree editable="bottom">
                        <field name="product_id"/>
                        <field name="product_uom_qty"/>
                        <field name="price_unit"/>
                        <field name="price_subtotal"/>
                    </tree>
                </field>
            </page>
            <page string="Other Information" name="other_info">
                <group>
                    <group string="Sales Information">
                        <field name="user_id"/>
                        <field name="team_id"/>
                    </group>
                    <group string="Invoicing">
                        <field name="payment_term_id"/>
                        <field name="fiscal_position_id"/>
                    </group>
                </group>
            </page>
            <page string="Notes" name="notes"
                  attrs="{'invisible': [('state', '=', 'cancel')]}">
                <field name="note" placeholder="Terms and conditions..."/>
            </page>
        </notebook>
    </sheet>
</form>
```

## Ivy

```csharp
// Notebook → TabsLayout widget
var form = UseForm(orderState);

form.Place("Name");
form.Place("PartnerId");

// Tabs via TabsLayout widget
var activeTab = UseState(0);
new TabsLayout(activeTab,
    new Tab("Order Lines", () =>
    {
        orderLines.ToDataTable()
            .Header(l => l.ProductName, "Product")
            .Header(l => l.Quantity, "Qty")
            .Header(l => l.UnitPrice, "Unit Price")
            .Header(l => l.Subtotal, "Subtotal");
    }),
    new Tab("Other Information", () =>
    {
        PlaceHorizontal(() =>
        {
            Column(() =>
            {
                new TextBlock("Sales Information").Variant(TextBlockVariants.H4);
                form.Place("UserId");
                form.Place("TeamId");
            });
            Column(() =>
            {
                new TextBlock("Invoicing").Variant(TextBlockVariants.H4);
                form.Place("PaymentTermId");
                form.Place("FiscalPositionId");
            });
        });
    }),
    new Tab("Notes", () =>
    {
        noteState.ToTextInput()
            .Variant(TextInputVariant.Textarea)
            .Placeholder("Terms and conditions...");
    }).Visible(state.Value != "cancel")
);

// Alternative: Standalone TabsLayout without form
var tabState = UseState(0);
new TabsLayout(tabState,
    new Tab("Order Lines", orderLinesContent),
    new Tab("Other Info", otherInfoContent),
    new Tab("Notes", notesContent)
);
```

## Parameters

| Odoo Element | Description | Ivy Equivalent |
|---|---|---|
| `<notebook>` | Tab container | `TabsLayout` widget |
| `<page>` | Individual tab | `new Tab("Title", content)` |
| `string="..."` | Tab title | First parameter of `Tab` constructor |
| `name="..."` | Technical name (for inheritance) | Not needed in Ivy |
| `attrs="{'invisible':...}"` | Conditional tab visibility | `.Visible(condition)` on Tab |
| `autofocus="autofocus"` | Default active tab | Initial `UseState(tabIndex)` |
| Multiple pages | Multiple tabs | Multiple `Tab` instances |
| Nested `<group>` in page | Field grouping within tab | Layout containers inside tab content |
| `<field>` in page | Fields displayed in tab | Ivy widgets inside tab callback |
