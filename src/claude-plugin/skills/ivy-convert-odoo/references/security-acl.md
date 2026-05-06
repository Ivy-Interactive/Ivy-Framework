# Security & ACL

Odoo's access control system using model-level ACLs (`ir.model.access.csv`), record-level rules (`ir.rule`), and field-level group restrictions. Controls who can create, read, update, or delete records, and which records are visible to which users.

## Odoo

```csv
# ir.model.access.csv - Model-level access control
id,name,model_id:id,group_id:id,perm_read,perm_write,perm_create,perm_unlink
access_sale_order_user,sale.order.user,model_sale_order,sales_team.group_sale_salesman,1,1,1,0
access_sale_order_manager,sale.order.manager,model_sale_order,sales_team.group_sale_manager,1,1,1,1
access_sale_order_line_user,sale.order.line.user,model_sale_order_line,sales_team.group_sale_salesman,1,1,1,0
```

```xml
<!-- Record rules (ir.rule) - Row-level security -->
<record id="sale_order_personal_rule" model="ir.rule">
    <field name="name">Personal Orders</field>
    <field name="model_id" ref="model_sale_order"/>
    <field name="domain_force">[('user_id', '=', user.id)]</field>
    <field name="groups" eval="[(4, ref('sales_team.group_sale_salesman'))]"/>
    <field name="perm_read" eval="True"/>
    <field name="perm_write" eval="True"/>
    <field name="perm_create" eval="True"/>
    <field name="perm_unlink" eval="False"/>
</record>

<!-- Multi-company record rule -->
<record id="sale_order_comp_rule" model="ir.rule">
    <field name="name">Sale Order multi-company</field>
    <field name="model_id" ref="model_sale_order"/>
    <field name="domain_force">[('company_id', 'in', company_ids)]</field>
</record>
```

```python
class SaleOrder(models.Model):
    _name = 'sale.order'

    # Field-level group restriction
    margin = fields.Float(groups='sale.group_sale_margin')

    @api.model
    def check_access_rights(self, operation, raise_exception=True):
        # Custom access check
        return super().check_access_rights(operation, raise_exception)

    def action_confirm(self):
        self.check_access_rights('write')
        self.check_access_rule('write')
        # ... confirm logic
```

```xml
<!-- Group-based visibility in views -->
<field name="margin" groups="sale.group_sale_margin"/>
<button name="action_delete" groups="base.group_system"/>
```

## Ivy

```csharp
// Security/ACL → role-based visibility and server-side authorization

// Role-based UI visibility
var currentUser = UseCurrentUser();
var isManager = currentUser.HasRole("SalesManager");
var isSalesperson = currentUser.HasRole("Salesperson");

// Field visible only to users with margin permission
new TextBlock($"Margin: {order.Margin:P}")
    .Visible(currentUser.HasRole("SalesMargin"));

// Button visible only to managers
new Button("Delete Order", onClick: async e => {
    await api.DeleteOrder(orderId);
}).Destructive()
    .Visible(isManager);

// Conditional form sections based on role
if (isManager)
{
    new TextBlock("Manager Controls").Variant(TextBlockVariants.H3);
    new Button("Override Price", onClick: async e => {
        await api.OverridePrice(orderId);
    });
}

// Row-level filtering (equivalent to ir.rule)
// Server-side: filter queries by current user
var orders = UseQuery(() => db.SaleOrders
    .Where(o => isManager || o.UserId == currentUser.Id)
    .Where(o => currentUser.CompanyIds.Contains(o.CompanyId))
    .ToList());

// Multi-company filtering
var companyOrders = UseQuery(() => db.SaleOrders
    .Where(o => o.CompanyId == currentUser.CurrentCompanyId)
    .ToList());

// Server-side authorization in API handlers
// [Authorize(Roles = "SalesManager")]
// public async Task DeleteOrder(int orderId) { ... }
```

## Parameters

| Odoo Element | Description | Ivy Equivalent |
|---|---|---|
| `ir.model.access.csv` | Model-level CRUD permissions | Server-side `[Authorize]` attributes on API endpoints |
| `perm_read/write/create/unlink` | CRUD permission flags | Role checks in API handlers |
| `ir.rule` (record rules) | Row-level security filters | `.Where()` clauses filtering by user/company |
| `domain_force` | Record visibility domain | LINQ filter in `UseQuery` |
| `groups="group.name"` | Field/button group restriction | `.Visible(currentUser.HasRole("..."))` |
| `check_access_rights()` | Programmatic access check | `currentUser.HasRole()` or server-side auth |
| `check_access_rule()` | Record-level access check | Server-side query filtering |
| Multi-company rules | Company-scoped records | `.Where(o => o.CompanyId == currentCompanyId)` |
| `user.id` in domain | Current user reference | `currentUser.Id` from `UseCurrentUser()` |
| `company_ids` in domain | User's companies | `currentUser.CompanyIds` collection |
