# Component: Sheet

Odoo's main content area within a form view. The `<sheet>` element creates a centered, white card-like container that holds the primary form content. It sits between the `<header>` (status bar) and the `<div class="oe_chatter">` (activity log).

## Odoo

```xml
<form>
    <header>
        <!-- Buttons and status bar -->
        <button name="action_confirm" string="Confirm" class="oe_highlight"/>
        <field name="state" widget="statusbar"/>
    </header>

    <sheet>
        <!-- Smart/stat buttons -->
        <div class="oe_button_box" name="button_box">
            <button class="oe_stat_button" icon="fa-usd" type="object" name="action_view_invoice">
                <field name="invoice_count" widget="statinfo" string="Invoices"/>
            </button>
        </div>

        <!-- Title area -->
        <div class="oe_title">
            <h1><field name="name" placeholder="Order Name"/></h1>
        </div>

        <!-- Form content groups -->
        <group>
            <group><field name="partner_id"/></group>
            <group><field name="date_order"/></group>
        </group>

        <notebook>
            <page string="Lines">
                <field name="order_line"/>
            </page>
        </notebook>
    </sheet>

    <div class="oe_chatter">
        <field name="message_ids"/>
    </div>
</form>
```

## Ivy

```csharp
// Sheet is implicit in Ivy's page/form layout
// The form content area is the default rendering context

// Header area → top section with buttons and status
PlaceHorizontal(() =>
{
    new Button("Confirm", onClick: async e => {
        await api.Confirm(orderId);
    }).Primary().Visible(state.Value == "draft");

    state.ToSelectInput(statusOptions).Variant(SelectInputVariant.Toggle);
});

new Separator();

// Stat buttons → Card row with metrics
PlaceHorizontal(() =>
{
    new Card(() => {
        new TextBlock(invoiceCount.ToString()).Variant(TextBlockVariants.H2);
        new TextBlock("Invoices").Variant(TextBlockVariants.Muted);
    }).OnClick(e => Navigate($"/invoices?orderId={orderId}"));
});

// Title
new TextBlock(orderName).Variant(TextBlockVariants.H1);

// Content groups
PlaceHorizontal(() =>
{
    partnerId.ToSelectInput(partners).WithField().Label("Customer");
    dateOrder.ToDateTimeInput().WithField().Label("Order Date");
});

// Tabs
// ... (notebook content)
```

## Parameters

| Odoo Element | Description | Ivy Equivalent |
|---|---|---|
| `<sheet>` | Main content card | Implicit in page layout (default content area) |
| `<div class="oe_title">` | Title section | `TextBlock` with H1 variant |
| `<h1><field name="name"/></h1>` | Large title field | `TextBlock` with H1 variant or large TextInput |
| `<div class="oe_button_box">` | Stat button container | `PlaceHorizontal` with Card/Metric widgets |
| Content width | Centered, max-width container | Page default max-width layout |
| Background | White card on gray background | Default page styling |
| Between header and chatter | Vertical layout position | Natural content flow |
