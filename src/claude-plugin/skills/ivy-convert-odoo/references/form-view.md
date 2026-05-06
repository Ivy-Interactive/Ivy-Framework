# Form View

Odoo's primary view for displaying and editing a single record. Contains fields, buttons, notebooks (tabs), groups, and status bars arranged in a structured XML layout.

## Odoo

```xml
<record id="view_partner_form" model="ir.ui.view">
    <field name="name">res.partner.form</field>
    <field name="model">res.partner</field>
    <field name="arch" type="xml">
        <form string="Partner">
            <header>
                <button name="action_confirm" type="object" string="Confirm" class="oe_highlight"
                        attrs="{'invisible': [('state', '!=', 'draft')]}"/>
                <field name="state" widget="statusbar" statusbar_visible="draft,confirmed,done"/>
            </header>
            <sheet>
                <div class="oe_title">
                    <h1><field name="name" placeholder="Name"/></h1>
                </div>
                <group>
                    <group>
                        <field name="email"/>
                        <field name="phone"/>
                    </group>
                    <group>
                        <field name="company_id"/>
                        <field name="category_id" widget="many2many_tags"/>
                    </group>
                </group>
                <notebook>
                    <page string="Sales" name="sales">
                        <field name="sale_order_ids">
                            <tree><field name="name"/><field name="amount_total"/></tree>
                        </field>
                    </page>
                    <page string="Notes" name="notes">
                        <field name="comment" placeholder="Internal notes..."/>
                    </page>
                </notebook>
            </sheet>
            <div class="oe_chatter">
                <field name="message_follower_ids"/>
                <field name="message_ids"/>
            </div>
        </form>
    </field>
</record>
```

```python
class ResPartner(models.Model):
    _inherit = 'res.partner'

    state = fields.Selection([
        ('draft', 'Draft'),
        ('confirmed', 'Confirmed'),
        ('done', 'Done'),
    ], default='draft')

    def action_confirm(self):
        self.write({'state': 'confirmed'})
```

## Ivy

```csharp
// Use .ToForm() for automatic form generation from a state class
var partner = UseState(new PartnerFormState());

// Automatic form with DataAnnotations
partner.ToForm();

// Or manual layout with UseForm
var form = UseForm(partner);
form.Place("Name");
form.PlaceHorizontal("Email", "Phone");
form.PlaceHorizontal("CompanyId", "CategoryIds");
// Tabs → TabsLayout widget
var activeTab = UseState(0);
new TabsLayout(activeTab,
    new Tab("Sales", salesTable),
    new Tab("Notes", UseState(partner.Value.Comment).ToTextInput().Variant(TextInputVariant.Textarea))
);

// State class with validation
public class PartnerFormState
{
    [Required]
    [Display(Name = "Name")]
    public string Name { get; set; } = "";

    [EmailAddress]
    public string Email { get; set; } = "";

    public string Phone { get; set; } = "";

    [Display(Name = "Company")]
    public int? CompanyId { get; set; }

    [Display(Name = "Categories")]
    public int[] CategoryIds { get; set; } = [];

    public string Comment { get; set; } = "";
}

// Status bar → SelectInput or stepper
var state = UseState("draft");
state.ToSelectInput(new[] { "draft", "confirmed", "done" }.ToOptions())
    .Variant(SelectInputVariant.Toggle);

// Action buttons
new Button("Confirm", onClick: async _ => {
    await api.ConfirmPartner(partnerId);
}).Primary();
```

## Parameters

| Odoo Element | Description | Ivy Equivalent |
|---|---|---|
| `<form>` | Root form view element | `.ToForm()` or `UseForm()` |
| `<header>` | Top bar with buttons and status | Layout with `Button` + status widget |
| `<sheet>` | Main content area | Implicit in form layout |
| `<group>` | Two-column field grouping | `.PlaceHorizontal()` or layout containers |
| `<notebook>` / `<page>` | Tabbed sections | `TabsLayout` widget with `Tab` instances |
| `<field>` | Field display/input | Mapped to Ivy input widgets per field type |
| `attrs="{'invisible': ...}"` | Conditional visibility | `.Visible(condition)` on widget |
| `attrs="{'readonly': ...}"` | Conditional read-only | `.Disabled(condition)` on widget |
| `attrs="{'required': ...}"` | Conditional required | `.Invalid(msg)` with validation logic |
| `widget="statusbar"` | Workflow status indicator | `SelectInput` with Toggle variant or custom stepper |
| `class="oe_highlight"` | Primary button styling | `.Primary()` on Button |
| `<div class="oe_chatter">` | Activity/message log | Not supported (use custom activity feed) |
| `<div class="oe_title">` | Large title field | `Heading` widget or form title |
| `string="..."` | View/page/group label | `Label`, `Heading`, or `Tab` title |
| `placeholder="..."` | Field placeholder text | `.Placeholder("...")` |
