# QWeb Reports

Odoo's QWeb template engine for generating PDF reports. Reports combine a QWeb XML/HTML template with a report action definition. Used for invoices, sales orders, delivery slips, and any printable document. Rendered server-side using wkhtmltopdf.

## Odoo

```xml
<!-- Report action definition -->
<record id="action_report_saleorder" model="ir.actions.report">
    <field name="name">Sales Order</field>
    <field name="model">sale.order</field>
    <field name="report_type">qweb-pdf</field>
    <field name="report_name">sale.report_saleorder</field>
    <field name="report_file">sale.report_saleorder</field>
    <field name="binding_model_id" ref="model_sale_order"/>
    <field name="binding_type">report</field>
    <field name="paperformat_id" ref="base.paperformat_us_letter"/>
</record>

<!-- QWeb report template -->
<template id="report_saleorder">
    <t t-call="web.html_container">
        <t t-foreach="docs" t-as="doc">
            <t t-call="web.external_layout">
                <div class="page">
                    <h2>
                        <span t-if="doc.state in ['draft', 'sent']">Quotation #</span>
                        <span t-else="">Sales Order #</span>
                        <span t-field="doc.name"/>
                    </h2>

                    <div class="row mt-4">
                        <div class="col-6">
                            <strong>Customer:</strong>
                            <span t-field="doc.partner_id.name"/>
                        </div>
                        <div class="col-6">
                            <strong>Date:</strong>
                            <span t-field="doc.date_order" t-options="{'widget': 'date'}"/>
                        </div>
                    </div>

                    <table class="table table-sm mt-4">
                        <thead>
                            <tr>
                                <th>Product</th>
                                <th class="text-right">Quantity</th>
                                <th class="text-right">Unit Price</th>
                                <th class="text-right">Subtotal</th>
                            </tr>
                        </thead>
                        <tbody>
                            <t t-foreach="doc.order_line" t-as="line">
                                <tr>
                                    <td><span t-field="line.product_id.name"/></td>
                                    <td class="text-right"><span t-field="line.product_uom_qty"/></td>
                                    <td class="text-right"><span t-field="line.price_unit"/></td>
                                    <td class="text-right"><span t-field="line.price_subtotal"/></td>
                                </tr>
                            </t>
                        </tbody>
                    </table>

                    <div class="row justify-content-end">
                        <div class="col-4">
                            <table class="table table-sm">
                                <tr><td>Untaxed:</td><td class="text-right"><span t-field="doc.amount_untaxed"/></td></tr>
                                <tr><td>Taxes:</td><td class="text-right"><span t-field="doc.amount_tax"/></td></tr>
                                <tr class="border-top"><td><strong>Total:</strong></td><td class="text-right"><strong><span t-field="doc.amount_total"/></strong></td></tr>
                            </table>
                        </div>
                    </div>
                </div>
            </t>
        </t>
    </t>
</template>
```

## Ivy

```csharp
// QWeb reports → print-friendly page layout or PDF generation

// Option 1: Print-friendly page layout
// Create a dedicated page designed for printing
var order = UseQuery(() => db.SaleOrders
    .Include(o => o.OrderLines).ThenInclude(l => l.Product)
    .Include(o => o.Partner)
    .First(o => o.Id == orderId));

// Report header
PlaceHorizontal(() =>
{
    new TextBlock(order.Value.State == "draft" ? "Quotation" : "Sales Order")
        .Variant(TextBlockVariants.H2);
    new TextBlock($"#{order.Value.Name}").Variant(TextBlockVariants.H2);
});

// Customer and date info
PlaceHorizontal(() =>
{
    Column(() =>
    {
        new TextBlock("Customer").Bold();
        new TextBlock(order.Value.Partner.Name);
    });
    Column(() =>
    {
        new TextBlock("Date").Bold();
        new TextBlock(order.Value.DateOrder.ToString("MMM dd, yyyy"));
    });
});

// Order lines table
order.Value.OrderLines.ToDataTable()
    .Header(l => l.Product.Name, "Product")
    .Header(l => l.Quantity, "Quantity", format: v => $"{v:F2}")
    .Header(l => l.UnitPrice, "Unit Price", format: v => $"{v:C}")
    .Header(l => l.PriceSubtotal, "Subtotal", format: v => $"{v:C}")
    .Config(c => c.Sortable(false));

// Summary below table
new TextBlock($"Subtotal: {order.Value.OrderLines.Sum(l => l.PriceSubtotal):C}").Bold();

// Totals
PlaceHorizontal(() =>
{
    new Spacer();
    Column(() =>
    {
        PlaceHorizontal(() => {
            new TextBlock("Untaxed:");
            new TextBlock($"{order.Value.AmountUntaxed:C}");
        });
        PlaceHorizontal(() => {
            new TextBlock("Taxes:");
            new TextBlock($"{order.Value.AmountTax:C}");
        });
        new Separator();
        PlaceHorizontal(() => {
            new TextBlock("Total:").Bold();
            new TextBlock($"{order.Value.AmountTotal:C}").Bold();
        });
    });
});

// Option 2: Print/download button triggering server-side PDF
new Button("Print", onClick: async e => {
    var pdfUrl = await api.GenerateOrderPdf(orderId);
    await Download(pdfUrl);
}).Ghost().Icon(Icons.Printer);

// Option 3: Browser print
new Button("Print", onClick: async e => {
    await Print(); // Triggers browser print dialog
}).Ghost().Icon(Icons.Printer);
```

## Parameters

| Odoo Element | Description | Ivy Equivalent |
|---|---|---|
| `ir.actions.report` | Report action definition | Print button or dedicated report page |
| `report_type="qweb-pdf"` | PDF output format | Server-side PDF generation or browser print |
| `report_name` | Template reference | Report page route |
| `binding_model_id` | Attach to model's print menu | Print button on form page |
| `paperformat_id` | Paper size/margins | CSS `@media print` styles or PDF config |
| `t-call="web.html_container"` | HTML wrapper | Page layout wrapper |
| `t-call="web.external_layout"` | Header/footer with company info | Company branding in report layout |
| `t-foreach` / `t-as` | Loop over records | `foreach` in C# or DataTable rows |
| `t-field="doc.field"` | Display field value | `TextBlock` or data binding |
| `t-if` / `t-else` | Conditional rendering | C# `if`/`else` in layout code |
| `t-options="{'widget': 'date'}"` | Field formatting | `.ToString("format")` or format callback |
| `<table class="table">` | HTML table in report | `DataTable` widget |
| Company header/footer | Letterhead branding | Custom header/footer in print layout |
| wkhtmltopdf rendering | Server-side PDF generation | Server-side PDF library or browser `Print()` |
