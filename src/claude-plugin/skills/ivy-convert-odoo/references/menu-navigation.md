# Menu Navigation & Routing

Odoo's menu system using `ir.ui.menu` records linked to window actions (`ir.actions.act_window`). Defines the sidebar navigation hierarchy, with each menu item opening a specific view (list, form, kanban, etc.) filtered by domain.

## Odoo

```xml
<!-- Top-level menu (app) -->
<menuitem id="sale.sale_menu_root"
          name="Sales"
          web_icon="sale,static/description/icon.png"
          sequence="5"/>

<!-- Sub-menus -->
<menuitem id="sale.sale_order_menu"
          name="Orders"
          parent="sale.sale_menu_root"
          sequence="1"/>

<menuitem id="sale.menu_sale_order"
          name="Quotations"
          parent="sale.sale_order_menu"
          action="sale.action_quotations"
          sequence="1"/>

<menuitem id="sale.menu_sale_order_confirmed"
          name="Sales Orders"
          parent="sale.sale_order_menu"
          action="sale.action_orders"
          sequence="2"/>

<!-- Window action (defines what opens when menu is clicked) -->
<record id="action_quotations" model="ir.actions.act_window">
    <field name="name">Quotations</field>
    <field name="res_model">sale.order</field>
    <field name="view_mode">tree,kanban,form,calendar,pivot,graph</field>
    <field name="domain">[('state', 'in', ['draft', 'sent'])]</field>
    <field name="context">{'default_state': 'draft'}</field>
    <field name="search_view_id" ref="sale.sale_order_view_search_inherit_quotation"/>
    <field name="help" type="html">
        <p class="o_view_nocontent_smiling_face">Create a new quotation!</p>
    </field>
</record>

<record id="action_orders" model="ir.actions.act_window">
    <field name="name">Sales Orders</field>
    <field name="res_model">sale.order</field>
    <field name="view_mode">tree,kanban,form,pivot,graph</field>
    <field name="domain">[('state', 'not in', ['draft', 'sent', 'cancel'])]</field>
</record>

<!-- Menu with group restriction -->
<menuitem id="sale.menu_sale_config"
          name="Configuration"
          parent="sale.sale_menu_root"
          groups="base.group_system"
          sequence="100"/>
```

## Ivy

```csharp
// Menu navigation → Ivy page routing with sidebar navigation
// Each menu item maps to a page/route in the Ivy app

// Page definitions (equivalent to act_window actions)
// /quotations → shows draft/sent orders
// /orders → shows confirmed orders
// /customers → shows partners

// Sidebar navigation is defined by page structure
// Top-level: "Sales" app
// Sub-pages: Quotations, Orders, Customers, Configuration

// Quotations page (filtered list)
var quotations = UseQuery(() => db.SaleOrders
    .Where(o => o.State == "draft" || o.State == "sent")
    .OrderByDescending(o => o.CreateDate)
    .ToList());

quotations.ToDataTable()
    .Header(o => o.Name, "Quotation")
    .Header(o => o.PartnerName, "Customer")
    .Header(o => o.DateOrder, "Date")
    .Header(o => o.AmountTotal, "Total", format: v => $"{v:C}")
    .Header(o => o.State, "Status", format: v => new Badge(v))
    .OnCellClick(async e => Navigate($"/quotation/{e.Value.RowId}"))
    .EmptyState("Create a new quotation!", icon: Icons.Smile);

// Sales Orders page (different filter on same model)
var orders = UseQuery(() => db.SaleOrders
    .Where(o => o.State != "draft" && o.State != "sent" && o.State != "cancel")
    .OrderByDescending(o => o.DateOrder)
    .ToList());

// Navigation between pages
new Button("View Quotation", onClick: e => {
    Navigate($"/quotation/{orderId}");
}).Ghost();

// Programmatic navigation (equivalent to act_window return)
// In Odoo: return {'type': 'ir.actions.act_window', ...}
// In Ivy: Navigate("/target-page?param=value")
async Task OnConfirm()
{
    await api.ConfirmOrder(orderId);
    Navigate($"/order/{orderId}"); // Redirect to orders view
}

// Role-restricted pages (equivalent to groups on menuitem)
// Configuration pages visible only to admins
var isAdmin = UseCurrentUser().HasRole("Admin");
if (isAdmin)
{
    // Configuration section
    new TextBlock("Configuration").Variant(TextBlockVariants.H2);
    // ... settings UI
}
```

## Parameters

| Odoo Element | Description | Ivy Equivalent |
|---|---|---|
| `ir.ui.menu` | Menu item definition | Page/route in Ivy app |
| `parent` attribute | Menu hierarchy | Sidebar navigation nesting |
| `action` attribute | Linked window action | Page content and data query |
| `sequence` | Menu ordering | Page ordering in sidebar |
| `groups` | Role-restricted menu | `.Visible()` or role-based page access |
| `web_icon` | App icon | Sidebar icon for app section |
| `ir.actions.act_window` | Window action (opens view) | Page definition with query and layout |
| `res_model` | Target model | Data source in `UseQuery` |
| `view_mode` | Available view types | Page layout (DataTable, cards, form) |
| `domain` | Default filter | `.Where()` clause in page query |
| `context` | Default values/settings | Query parameters or page state |
| `search_view_id` | Search/filter definition | Filter controls above data view |
| `help` (no content) | Empty state message | `.EmptyState()` on DataTable |
| Breadcrumb navigation | View history stack | Browser back / `Navigate()` history |
