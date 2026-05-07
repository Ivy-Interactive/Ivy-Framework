# Model & View Inheritance

Odoo's inheritance mechanisms for extending existing models and views without modifying the original. Supports three types: class inheritance (`_inherit` same model), prototype inheritance (`_inherit` + `_name`), and delegation inheritance (`_inherits`). Views can be extended via `inherit_id` with XPath expressions.

## Odoo

```python
# 1. Class inheritance (_inherit, same model) - Extends existing model in-place
class ResPartner(models.Model):
    _inherit = 'res.partner'

    # Add new fields to existing model
    loyalty_points = fields.Integer(string='Loyalty Points', default=0)
    membership_level = fields.Selection([
        ('bronze', 'Bronze'),
        ('silver', 'Silver'),
        ('gold', 'Gold'),
    ], string='Membership', compute='_compute_membership')

    @api.depends('loyalty_points')
    def _compute_membership(self):
        for partner in self:
            if partner.loyalty_points >= 1000:
                partner.membership_level = 'gold'
            elif partner.loyalty_points >= 500:
                partner.membership_level = 'silver'
            else:
                partner.membership_level = 'bronze'

    # Override existing method
    def action_confirm(self):
        res = super().action_confirm()
        # Add custom logic after original
        self._send_confirmation_email()
        return res


# 2. Prototype inheritance (_inherit + _name) - New model based on existing
class CrmLead(models.Model):
    _name = 'crm.lead'
    _inherit = ['mail.thread', 'mail.activity.mixin', 'utm.mixin']
    # Copies fields/methods from mail.thread etc. into crm.lead


# 3. Delegation inheritance (_inherits) - Composition via linked record
class ResUsers(models.Model):
    _name = 'res.users'
    _inherits = {'res.partner': 'partner_id'}

    partner_id = fields.Many2one('res.partner', required=True, ondelete='restrict')
    login = fields.Char(required=True)
    # Can access partner fields directly: user.name → partner.name
```

```xml
<!-- View inheritance - extend existing view -->
<record id="view_partner_form_inherit_loyalty" model="ir.ui.view">
    <field name="name">res.partner.form.inherit.loyalty</field>
    <field name="model">res.partner</field>
    <field name="inherit_id" ref="base.view_partner_form"/>
    <field name="arch" type="xml">
        <!-- Add field after existing field -->
        <field name="email" position="after">
            <field name="loyalty_points"/>
            <field name="membership_level"/>
        </field>

        <!-- Add page to notebook -->
        <xpath expr="//notebook" position="inside">
            <page string="Loyalty">
                <field name="loyalty_history_ids"/>
            </page>
        </xpath>

        <!-- Replace existing element -->
        <field name="phone" position="replace">
            <field name="phone" widget="phone"/>
        </field>

        <!-- Add attributes to existing element -->
        <field name="name" position="attributes">
            <attribute name="required">1</attribute>
        </field>
    </field>
</record>
```

## Ivy

```csharp
// Model inheritance → C# class inheritance and composition

// 1. Class inheritance (extend existing model) → C# partial classes or inheritance
// Adding fields to existing entity
public class Partner  // Base entity
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
}

// Extension via inheritance
public class PartnerWithLoyalty : Partner
{
    public int LoyaltyPoints { get; set; }
    public string MembershipLevel =>
        LoyaltyPoints >= 1000 ? "gold" :
        LoyaltyPoints >= 500 ? "silver" : "bronze";
}

// Or via partial classes (same model, different files)
// File: Partner.cs
public partial class Partner { /* base fields */ }
// File: Partner.Loyalty.cs
public partial class Partner
{
    public int LoyaltyPoints { get; set; }
    public string MembershipLevel => /* computed */;
}

// 2. Mixin inheritance → C# interfaces + composition
public interface ITrackable
{
    List<Message> Messages { get; set; }
    List<Activity> Activities { get; set; }
}

public class CrmLead : ITrackable
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<Message> Messages { get; set; } = new();
    public List<Activity> Activities { get; set; } = new();
}

// 3. Delegation inheritance → C# composition
public class User
{
    public int Id { get; set; }
    public string Login { get; set; } = "";

    // Delegation: user has a partner
    public int PartnerId { get; set; }
    public Partner Partner { get; set; } = null!;

    // Convenience accessors
    public string Name => Partner.Name;
    public string Email => Partner.Email;
}

// View inheritance → extending page layout with additional fields
// Base partner form
void RenderPartnerForm(Partner partner)
{
    var name = UseState(partner.Name);
    var email = UseState(partner.Email);

    name.ToTextInput().WithField().Label("Name");
    email.ToTextInput().WithField().Label("Email");
}

// Extended form with loyalty fields
void RenderPartnerFormWithLoyalty(PartnerWithLoyalty partner)
{
    // Base fields
    RenderPartnerForm(partner);

    // Added fields (equivalent to position="after")
    new TextBlock($"Loyalty Points: {partner.LoyaltyPoints}");
    new Badge(partner.MembershipLevel)
        .Variant(partner.MembershipLevel switch {
            "gold" => BadgeVariants.Warning,
            "silver" => BadgeVariants.Info,
            _ => BadgeVariants.Secondary
        });
}

// Method override → C# virtual/override
public class SaleService
{
    public virtual async Task ConfirmOrder(int orderId)
    {
        // Base confirmation logic
    }
}

public class CustomSaleService : SaleService
{
    public override async Task ConfirmOrder(int orderId)
    {
        await base.ConfirmOrder(orderId);
        // Custom logic after original
        await SendConfirmationEmail(orderId);
    }
}
```

## Parameters

| Odoo Element | Description | Ivy Equivalent |
|---|---|---|
| `_inherit = 'model'` (same name) | Extend existing model in-place | C# partial class or class inheritance |
| `_inherit + _name` | New model from mixins | C# class implementing interfaces |
| `_inherits = {'model': 'field'}` | Delegation/composition | Composition with navigation property |
| `super().method()` | Call parent implementation | `base.Method()` in C# override |
| `inherit_id` on view | Extend existing view | Reuse/extend base layout method |
| `position="after"` | Insert after element | Add widgets after base layout call |
| `position="before"` | Insert before element | Add widgets before base layout call |
| `position="inside"` | Insert inside element | Add content within container |
| `position="replace"` | Replace element | Override layout section |
| `position="attributes"` | Modify attributes | Change widget properties in extended layout |
| `<xpath expr="...">` | XPath selection in view | Direct reference to layout section |
| Mixin (`mail.thread`) | Add behavior via inheritance | Interface implementation + composition |
