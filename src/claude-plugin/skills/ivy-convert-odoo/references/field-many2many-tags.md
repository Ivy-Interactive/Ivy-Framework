# Field: Many2many Tags

Odoo's tag chip display widget for Many2many relational fields. Shows selected records as colored pill/tag chips with add and remove capabilities. Commonly used for categories, labels, skills, and other tagging scenarios.

## Odoo

```python
class CrmLead(models.Model):
    _name = 'crm.lead'

    tag_ids = fields.Many2many('crm.tag', string='Tags')

class CrmTag(models.Model):
    _name = 'crm.tag'

    name = fields.Char(string='Name', required=True)
    color = fields.Integer(string='Color Index')

class HrEmployee(models.Model):
    _name = 'hr.employee'

    skill_ids = fields.Many2many('hr.skill', string='Skills')
    category_ids = fields.Many2many('hr.employee.category', string='Tags')
```

```xml
<!-- Basic many2many tags -->
<field name="tag_ids" widget="many2many_tags"/>

<!-- With colors -->
<field name="tag_ids" widget="many2many_tags" options="{'color_field': 'color'}"/>

<!-- No create (only select existing) -->
<field name="tag_ids" widget="many2many_tags"
       options="{'no_create': true, 'no_create_edit': true}"/>

<!-- In kanban card -->
<field name="tag_ids" widget="many2many_tags" options="{'color_field': 'color'}"/>

<!-- As avatar tags (with images) -->
<field name="user_ids" widget="many2many_avatar_user"/>

<!-- In list view -->
<field name="tag_ids" widget="many2many_tags" optional="show"/>

<!-- With limit on visible tags -->
<field name="tag_ids" widget="many2many_tags" options="{'color_field': 'color'}"/>
```

## Ivy

```csharp
// Many2many tags → SelectInput with collection state (auto multi-select)
var allTags = UseQuery(() => db.CrmTags.ToList());
var selectedTagIds = UseState<int[]>([]);

selectedTagIds.ToSelectInput(allTags.Value.ToOptions(t => t.Id, t => t.Name))
    .Placeholder("Add tags...")
    .Searchable()
    .WithField()
    .Label("Tags");

// Display as Badge chips (read-only)
PlaceHorizontal(() =>
{
    foreach (var tag in selectedTags)
    {
        new Badge(tag.Name)
            .Variant(GetBadgeVariant(tag.Color));
    }
});

// Avatar tags for users
var userIds = UseState<int[]>([]);
userIds.ToSelectInput(users.Value.ToOptions(u => u.Id, u => u.Name))
    .Placeholder("Add assignees...")
    .Searchable()
    .WithField()
    .Label("Assignees");

// Display user avatars
PlaceHorizontal(() =>
{
    foreach (var user in assignedUsers)
    {
        new Avatar(user.Name).Image(user.AvatarUrl);
    }
});

// Skills as tags
var skillIds = UseState<int[]>([]);
skillIds.ToSelectInput(allSkills.Value.ToOptions(s => s.Id, s => s.Name))
    .WithField()
    .Label("Skills");

// In DataTable column
leads.ToDataTable()
    .Header(l => l.Name, "Lead")
    .Header(l => l.Tags, "Tags", format: tags =>
    {
        PlaceHorizontal(() =>
        {
            foreach (var tag in tags)
                new Badge(tag.Name).Variant(GetBadgeVariant(tag.Color));
        });
    });

// Helper for Odoo color index to Ivy badge variant
BadgeVariants GetBadgeVariant(int colorIndex) => colorIndex switch
{
    1 => BadgeVariants.Destructive,
    2 => BadgeVariants.Warning,
    3 => BadgeVariants.Success,
    4 => BadgeVariants.Info,
    _ => BadgeVariants.Secondary
};
```

## Parameters

| Odoo Field/Widget | Description | Ivy Equivalent |
|---|---|---|
| `widget="many2many_tags"` | Tag chip display | `SelectInput` with collection state (auto multi-select) |
| `options="{'color_field': 'color'}"` | Colored tags by field | `Badge` with variant based on color index |
| `options="{'no_create': true}"` | Disable creating new tags | Default SelectInput behavior (select only) |
| `options="{'no_create_edit': true}"` | Disable create and edit | Default SelectInput behavior |
| `widget="many2many_avatar_user"` | User avatar chips | `Avatar` widgets per selected user |
| `widget="many2many_avatar"` | Generic avatar chips | `Avatar` widgets per selected record |
| Tag color | Visual tag color | `Badge` variant (Success, Warning, Info, etc.) |
| Add tag | Select/create new tag | `SelectInput` dropdown selection |
| Remove tag (x) | Remove tag from selection | SelectInput built-in removal |
| `optional="show"` | Column visibility in list | DataTable column visibility |
| `attrs="{'readonly':...}"` | Conditional read-only | `.Disabled(condition)` |
