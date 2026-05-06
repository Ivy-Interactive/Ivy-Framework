# Component: Header

Odoo's form header bar containing action buttons and the status bar widget. Appears at the top of a form view, above the sheet. Contains workflow action buttons (Confirm, Cancel, etc.) and the `statusbar` widget.

## Odoo

```xml
<form>
    <header>
        <!-- Primary action button -->
        <button name="action_confirm" type="object" string="Confirm"
                class="oe_highlight"
                attrs="{'invisible': [('state', '!=', 'draft')]}"/>

        <!-- Secondary action button -->
        <button name="action_cancel" type="object" string="Cancel"
                attrs="{'invisible': [('state', 'in', ['cancel', 'done'])]}"/>

        <!-- Print/Action dropdown -->
        <button name="action_quotation_send" type="object" string="Send by Email"
                attrs="{'invisible': [('state', '!=', 'draft')]}"/>

        <!-- Status bar -->
        <field name="state" widget="statusbar"
               statusbar_visible="draft,sent,sale,done"/>
    </header>
    <sheet>
        <!-- ... form content ... -->
    </sheet>
</form>
```

## Ivy

```csharp
// Header → horizontal layout at top of form with action buttons + status bar

// Action buttons row
PlaceHorizontal(() =>
{
    // Primary action
    new Button("Confirm", onClick: async e => {
        await api.ConfirmOrder(orderId);
    }).Primary().Icon(Icons.Check)
        .Visible(state.Value == "draft");

    // Secondary action
    new Button("Cancel", onClick: async e => {
        var confirmed = await Confirm("Are you sure you want to cancel?");
        if (confirmed) await api.CancelOrder(orderId);
    }).Destructive()
        .Visible(state.Value != "cancel" && state.Value != "done");

    // Email action
    new Button("Send by Email", onClick: async e => {
        await api.SendQuotation(orderId);
    }).Ghost().Icon(Icons.Mail)
        .Visible(state.Value == "draft");

    // Spacer to push status bar right
    new Spacer();

    // Status bar
    state.ToSelectInput(new[] {
        new Option("draft", "Quotation"),
        new Option("sent", "Quotation Sent"),
        new Option("sale", "Sales Order"),
        new Option("done", "Locked"),
    }).Variant(SelectInputVariant.Toggle);
});

new Separator();

// ... rest of form content
```

## Parameters

| Odoo Element | Description | Ivy Equivalent |
|---|---|---|
| `<header>` | Form header container | `PlaceHorizontal` with buttons + status |
| `<button class="oe_highlight">` | Primary action button | `new Button("...").Primary()` |
| `<button type="object">` | Server action button | `onClick` handler calling API |
| `<button type="action">` | Window action button | `onClick` with `Navigate()` |
| `attrs="{'invisible':...}"` | Conditional button visibility | `.Visible(condition)` |
| `confirm="..."` | Confirmation dialog | `await Confirm("message")` |
| `<field widget="statusbar">` | Status progression bar | `SelectInput` with Toggle variant |
| Button ordering | Left-to-right priority | Widget placement order |
| Separator from sheet | Visual divider | `Separator` widget |
