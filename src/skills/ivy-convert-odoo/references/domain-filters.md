# Domain Filters

Odoo's domain expression syntax for filtering records. Domains are lists of tuples `[('field', 'operator', value)]` combined with `'&'`, `'|'`, `'!'` operators (Polish notation). Used everywhere: view filters, record rules, action domains, field domains, and search views.

## Odoo

```python
# Basic domain syntax
# [('field', 'operator', value)]

# Simple filter
domain = [('state', '=', 'draft')]

# Multiple conditions (implicit AND)
domain = [('state', '=', 'sale'), ('amount_total', '>', 1000)]

# OR conditions
domain = ['|', ('state', '=', 'draft'), ('state', '=', 'sent')]

# NOT condition
domain = ['!', ('active', '=', False)]

# Complex: (state=draft OR state=sent) AND partner_id=5
domain = ['|', ('state', '=', 'draft'), ('state', '=', 'sent'),
          ('partner_id', '=', 5)]

# IN operator
domain = [('state', 'in', ['draft', 'sent', 'sale'])]

# String operators
domain = [('name', 'ilike', 'order')]     # case-insensitive contains
domain = [('name', '=like', 'SO%')]        # SQL LIKE pattern
domain = [('name', 'not ilike', 'test')]   # not contains

# Relational operators
domain = [('partner_id.country_id', '=', ref('base.us'))]  # Dotted path
domain = [('order_line.product_id', 'in', [1, 2, 3])]      # One2many/Many2many
domain = [('partner_id', 'child_of', parent_id)]            # Hierarchy

# Date operators
domain = [('date_order', '>=', '2024-01-01')]
domain = [('create_date', '>=', fields.Date.today())]

# Used in views
# <field name="partner_id" domain="[('customer_rank', '>', 0)]"/>
# <filter domain="[('user_id', '=', uid)]"/>
# Record rules: domain_force="[('company_id', 'in', company_ids)]"
```

```xml
<!-- Domain in view filter -->
<filter name="my_orders" string="My Orders"
        domain="[('user_id', '=', uid)]"/>

<!-- Domain on relational field -->
<field name="partner_id"
       domain="[('customer_rank', '>', 0), ('active', '=', True)]"/>

<!-- Domain in window action -->
<field name="domain">[('state', 'in', ['draft', 'sent'])]</field>
```

## Ivy

```csharp
// Domain filters → LINQ .Where() expressions

// Simple equality: [('state', '=', 'draft')]
.Where(o => o.State == "draft")

// Multiple AND: [('state', '=', 'sale'), ('amount_total', '>', 1000)]
.Where(o => o.State == "sale" && o.AmountTotal > 1000)

// OR: ['|', ('state', '=', 'draft'), ('state', '=', 'sent')]
.Where(o => o.State == "draft" || o.State == "sent")

// NOT: ['!', ('active', '=', False)]
.Where(o => o.Active != false)
// or simply: .Where(o => o.Active)

// IN: [('state', 'in', ['draft', 'sent', 'sale'])]
var validStates = new[] { "draft", "sent", "sale" };
.Where(o => validStates.Contains(o.State))

// String contains (ilike): [('name', 'ilike', 'order')]
.Where(o => o.Name.Contains("order", StringComparison.OrdinalIgnoreCase))
// or with EF Core: .Where(o => EF.Functions.ILike(o.Name, "%order%"))

// String starts with (=like 'SO%'): [('name', '=like', 'SO%')]
.Where(o => o.Name.StartsWith("SO"))

// Relational dotted path: [('partner_id.country_id', '=', usId)]
.Where(o => o.Partner.CountryId == usId)

// One2many/Many2many: [('order_line.product_id', 'in', [1, 2, 3])]
var productIds = new[] { 1, 2, 3 };
.Where(o => o.OrderLines.Any(l => productIds.Contains(l.ProductId)))

// Date comparison: [('date_order', '>=', '2024-01-01')]
.Where(o => o.DateOrder >= new DateTime(2024, 1, 1))

// Current user: [('user_id', '=', uid)]
var currentUser = UseCurrentUser();
.Where(o => o.UserId == currentUser.Id)

// Company filter: [('company_id', 'in', company_ids)]
.Where(o => currentUser.CompanyIds.Contains(o.CompanyId))

// Full query example
var orders = UseQuery(() => db.SaleOrders
    .Include(o => o.Partner)
    .Where(o => o.State == "draft" || o.State == "sent")
    .Where(o => o.UserId == currentUser.Id)
    .Where(o => o.AmountTotal > 0)
    .OrderByDescending(o => o.DateOrder)
    .ToList());

// Dynamic filtering with user input
var stateFilter = UseState<string?>(null);
var searchTerm = UseState("");

var filtered = UseQuery(() => db.SaleOrders
    .Where(o => stateFilter.Value == null || o.State == stateFilter.Value)
    .Where(o => string.IsNullOrEmpty(searchTerm.Value)
        || o.Name.Contains(searchTerm.Value)
        || o.Partner.Name.Contains(searchTerm.Value))
    .ToList());
```

## Parameters

| Odoo Domain | Description | LINQ Equivalent |
|---|---|---|
| `('field', '=', value)` | Equals | `o.Field == value` |
| `('field', '!=', value)` | Not equals | `o.Field != value` |
| `('field', '>', value)` | Greater than | `o.Field > value` |
| `('field', '>=', value)` | Greater or equal | `o.Field >= value` |
| `('field', '<', value)` | Less than | `o.Field < value` |
| `('field', '<=', value)` | Less or equal | `o.Field <= value` |
| `('field', 'in', list)` | In list | `list.Contains(o.Field)` |
| `('field', 'not in', list)` | Not in list | `!list.Contains(o.Field)` |
| `('field', 'ilike', str)` | Case-insensitive contains | `.Contains(str, OrdinalIgnoreCase)` |
| `('field', 'like', str)` | Case-sensitive contains | `.Contains(str)` |
| `('field', '=like', pattern)` | SQL LIKE pattern | `.StartsWith()` / `.EndsWith()` |
| `('field', 'child_of', id)` | Hierarchical children | Recursive query or CTE |
| `'&'` (implicit) | AND | `&&` in LINQ predicate |
| `'\|'` | OR | `\|\|` in LINQ predicate |
| `'!'` | NOT | `!` prefix in predicate |
| `('rel.field', '=', v)` | Dotted relational path | `.Include()` + navigation property |
| `uid` | Current user ID | `currentUser.Id` |
| `company_ids` | User's companies | `currentUser.CompanyIds` |
