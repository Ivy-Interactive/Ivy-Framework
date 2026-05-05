# Component: Chatter

Odoo's activity and messaging panel at the bottom of form views. Combines message threads, activity scheduling, and follower management. Requires the `mail` module and models must inherit from `mail.thread` and optionally `mail.activity.mixin`.

## Odoo

```python
class SaleOrder(models.Model):
    _name = 'sale.order'
    _inherit = ['mail.thread', 'mail.activity.mixin']

    # mail.thread provides:
    # message_ids - message history
    # message_follower_ids - followers
    # message_main_attachment_id - main attachment
    #
    # mail.activity.mixin provides:
    # activity_ids - scheduled activities
    # activity_state - overdue/today/planned
    # activity_user_id - responsible user
    # activity_date_deadline - next activity deadline
```

```xml
<form>
    <header>...</header>
    <sheet>...</sheet>

    <!-- Chatter section -->
    <div class="oe_chatter">
        <field name="message_follower_ids"/>
        <field name="activity_ids"/>
        <field name="message_ids"/>
    </div>
</form>
```

## Ivy

```csharp
// Chatter → custom activity/comment feed
// This is a complex component; build a simplified version

// Separator before chatter area
new Separator();

// Activity section
new TextBlock("Activities").Variant(TextBlockVariants.H3);

var activities = UseQuery(() => db.Activities
    .Where(a => a.RecordId == recordId && a.RecordModel == "sale.order")
    .OrderBy(a => a.DateDeadline)
    .ToList());

foreach (var activity in activities.Value)
{
    Card(() =>
    {
        PlaceHorizontal(() =>
        {
            new Badge(activity.TypeName)
                .Variant(activity.DateDeadline < DateOnly.FromDateTime(DateTime.Today)
                    ? BadgeVariants.Destructive
                    : activity.DateDeadline == DateOnly.FromDateTime(DateTime.Today)
                    ? BadgeVariants.Warning : BadgeVariants.Info);
            new TextBlock(activity.Summary);
            new TextBlock(activity.DateDeadline.ToString("MMM dd")).Muted();
            new Avatar(activity.UserName);
        });

        PlaceHorizontal(() =>
        {
            new Button("Done", onClick: async e => {
                await api.CompleteActivity(activity.Id);
            }).Success().Ghost();
            new Button("Reschedule", onClick: async e => {
                // Open reschedule dialog
            }).Ghost();
        });
    });
}

new Button("Schedule Activity", onClick: async e => {
    // Open activity creation dialog
}).Ghost().Icon(Icons.Plus);

// Message thread
new Separator();
new TextBlock("Messages").Variant(TextBlockVariants.H3);

var messages = UseQuery(() => db.Messages
    .Where(m => m.RecordId == recordId)
    .OrderByDescending(m => m.Date)
    .ToList());

// Message input
var newMessage = UseState("");
PlaceHorizontal(() =>
{
    newMessage.ToTextInput()
        .Variant(TextInputVariant.Textarea)
        .Placeholder("Log a note or send a message...");
    new Button("Send", onClick: async e => {
        await api.PostMessage(recordId, newMessage.Value);
        newMessage.Set("");
    }).Primary();
});

// Message list
foreach (var msg in messages.Value)
{
    PlaceHorizontal(() =>
    {
        new Avatar(msg.AuthorName);
        Column(() =>
        {
            PlaceHorizontal(() =>
            {
                new TextBlock(msg.AuthorName).Bold();
                new TextBlock(msg.Date.ToString("MMM dd, HH:mm")).Muted();
            });
            new Html(msg.Body);
        });
    });
}

// Note: Full chatter with followers, email integration, and
// activity types requires significant custom implementation.
```

## Parameters

| Odoo Element | Description | Ivy Equivalent |
|---|---|---|
| `<div class="oe_chatter">` | Chatter container | Custom section below form content |
| `<field name="message_ids"/>` | Message thread | Custom message list with `Card`/layout |
| `<field name="activity_ids"/>` | Activity feed | Activity cards with status badges |
| `<field name="message_follower_ids"/>` | Follower list | User list with add/remove (custom) |
| `mail.thread` mixin | Message tracking | Custom message/audit trail table |
| `mail.activity.mixin` | Activity scheduling | Custom activity table with deadlines |
| Log note | Internal note | TextInput + "Log Note" button |
| Send message | Email to followers | TextInput + "Send" button |
| Schedule activity | Create new activity | Dialog with type, summary, date, assignee |
| `tracking=True` on fields | Field change tracking | Custom audit log for field changes |
| Attachment support | File attachments on messages | FileInput in message composition |
