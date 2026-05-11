# Kanban View

Odoo's card-based view for displaying records in columns, often used for pipeline/workflow stages. Supports drag-and-drop between stages, quick create, and custom card templates.

## Odoo

```xml
<record id="view_task_kanban" model="ir.ui.view">
    <field name="name">project.task.kanban</field>
    <field name="model">project.task</field>
    <field name="arch" type="xml">
        <kanban default_group_by="stage_id" class="o_kanban_small_column"
                on_create="quick_create" quick_create_view="project.quick_create_task_form">
            <field name="name"/>
            <field name="stage_id"/>
            <field name="user_ids"/>
            <field name="priority"/>
            <field name="color"/>
            <progressbar field="kanban_state"
                         colors='{"normal": "secondary", "done": "success", "blocked": "danger"}'/>
            <templates>
                <t t-name="kanban-box">
                    <div t-attf-class="oe_kanban_card #{kanban_color(record.color.raw_value)}">
                        <div class="oe_kanban_content">
                            <strong><field name="name"/></strong>
                            <div><field name="user_ids" widget="many2many_avatar_user"/></div>
                            <field name="priority" widget="priority"/>
                            <field name="tag_ids" widget="many2many_tags"
                                   options="{'color_field': 'color'}"/>
                        </div>
                    </div>
                </t>
            </templates>
        </kanban>
    </field>
</record>
```

## Ivy

```csharp
// Kanban boards map to a custom app with card layouts and drag-drop
// Use DataTable grouped view or build custom card layout

var tasks = UseQuery(() => db.Tasks.Include(t => t.Stage).ToList());
var stages = UseQuery(() => db.Stages.OrderBy(s => s.Sequence).ToList());

// Option 1: Card-based layout with columns per stage
foreach (var stage in stages.Value)
{
    Column(() => {
        new Heading(stage.Name, level: 3);
        foreach (var task in tasks.Value.Where(t => t.StageId == stage.Id))
        {
            Card(() => {
                new Text(task.Name).Bold();
                new Badge(task.Priority);
                // Avatar display for assigned users
                foreach (var user in task.Users)
                    new Avatar(user.Name);
            }).OnClick(e => Navigate($"/task/{task.Id}"));
        }
    });
}

// Option 2: DataTable with grouping
tasks.ToDataTable()
    .Header(t => t.Name)
    .Header(t => t.StageName)
    .Header(t => t.Priority)
    .Config(c => c.Groupable(true));
```

## Parameters

| Odoo Element | Description | Ivy Equivalent |
|---|---|---|
| `<kanban>` | Root kanban view | Custom card layout or grouped DataTable |
| `default_group_by` | Column grouping field | Group tasks by stage in layout logic |
| `quick_create` | Inline record creation | Button + Dialog/Sheet for quick create |
| `<templates>` / `<t t-name="kanban-box">` | Card template | Custom `Card(() => { ... })` layout |
| `<field>` in template | Field display in card | Ivy widgets inside Card callback |
| `widget="many2many_avatar_user"` | User avatar chips | `Avatar` widget per user |
| `widget="priority"` | Star priority selector | `Badge` or custom star rating |
| `widget="many2many_tags"` | Colored tag chips | `Badge` per tag with color |
| `<progressbar>` | Stage progress indicator | `ProgressBar` widget |
| `kanban_color()` | Card background color | `.Style()` or CSS class on Card |
| `on_create="quick_create"` | Quick create mode | Button with inline form or Dialog |
| drag-and-drop | Move cards between columns | Custom drag-drop (not built-in, use server action on move) |
