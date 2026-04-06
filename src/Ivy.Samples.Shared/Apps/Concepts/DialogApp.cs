namespace Ivy.Samples.Shared.Apps.Concepts;

[App(icon: Icons.MessageSquare, searchHints: ["modal", "popup", "window", "dialog", "overlay", "confirm"])]
public class DialogApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical().Gap(4)
               | Text.H1("Dialog")
               | Text.P("Modal windows with Header, Body, and Footer sections. Each example demonstrates different use cases.")
               | Layout.Grid().Columns(2).Gap(4)
                   | new CreateDialogExample()
                   | new DeleteDialogExample()
                   | new ExitCommentDialogExample()
                   | new AutoFocusDialogExample();
    }
}

public class DeleteDialogExample : ViewBase
{
    public override object? Build()
    {
        var isOpen = UseState(false);
        var client = UseService<IClientProvider>();

        return Layout.Vertical().Gap(2)
               | new Card(
                   Layout.Vertical().Gap(3)
                   | Layout.Horizontal()
                       | Icons.Trash2
                       | Text.H3("Delete Confirmation")
                   | Text.P("Confirm destructive actions with a clear warning dialog.")
                   | new Button("Delete Item", _ => isOpen.Set(true))
                       .Variant(ButtonVariant.Destructive)
                       .Icon(Icons.Trash2)
               )
               | (isOpen.Value
                   ? new Dialog(
                       _ => isOpen.Set(false),
                       new DialogHeader("Delete Item"),
                       new DialogBody(
                           Layout.Vertical().Gap(3)
                           | Text.P("Are you sure you want to delete this item?")
                           | Text.P("This action cannot be undone. All associated data will be permanently removed.")
                           | Layout.Vertical().Gap(2)
                               | Text.Label("Item Details").Bold()
                               | new Card(new
                               {
                                   Name = "Sample Item",
                                   Type = "Document",
                                   Created = DateTime.Now.AddDays(-30),
                                   Size = "2.5 MB"
                               }.ToDetails())
                       ),
                       new DialogFooter(
                           new Button("Cancel", _ => isOpen.Set(false), variant: ButtonVariant.Outline),
                           new Button("Delete", _ =>
                           {
                               isOpen.Set(false);
                               client.Toast("Item deleted successfully!");
                           }, variant: ButtonVariant.Destructive)
                       )
                   )
                   : null);
    }
}

public class CreateDialogExample : ViewBase
{
    public record NewItemModel(string Name, string Description, string Category);

    public override object? Build()
    {
        var isOpen = UseState(false);
        var client = UseService<IClientProvider>();
        var model = UseState(() => new NewItemModel("", "", ""));

        async Task OnSubmit(NewItemModel submittedModel)
        {
            await Task.Delay(500);
            client.Toast($"Created: {submittedModel.Name}");
            model.Set(new NewItemModel("", "", ""));
        }

        return Layout.Vertical().Gap(2)
               | new Card(
                   Layout.Vertical().Gap(3)
                   | Layout.Horizontal()
                       | Icons.Plus
                       | Text.H3("Create New Item")
                   | Text.P("Use forms in dialogs to collect information when creating new items.")
                   | new Button("Create Item", _ => isOpen.Set(true))
                       .Primary()
                       .Icon(Icons.Plus)
               )
               | model.Value
                   .ToForm()
                   .OnSubmit(OnSubmit)
                   .Builder(e => e.Name, e => e.ToTextInput().Placeholder("Enter item name"))
                   .Builder(e => e.Description, e => e.ToTextareaInput().Placeholder("Enter description"))
                   .Builder(e => e.Category, e => e.ToTextInput().Placeholder("Enter category"))
                   .ToDialog(isOpen, title: "Create New Item", description: "Fill in the details below to create a new item.", submitTitle: "Create", width: Size.Units(200));
    }
}

public class ExitCommentDialogExample : ViewBase
{
    public override object? Build()
    {
        var isOpen = UseState(false);
        var comment = UseState("");
        var client = UseService<IClientProvider>();

        return Layout.Vertical().Gap(2)
               | new Card(
                   Layout.Vertical().Gap(3)
                   | Layout.Horizontal()
                       | Icons.MessageSquare
                       | Text.H3("Exit Comment")
                   | Text.P("Request feedback or comments when users leave or exit a workflow.")
                   | new Button("Leave Feedback", _ =>
                   {
                       comment.Set("");
                       isOpen.Set(true);
                   })
                       .Variant(ButtonVariant.Outline)
                       .Icon(Icons.MessageSquare)
               )
               | (isOpen.Value
                   ? new Dialog(
                       _ => isOpen.Set(false),
                       new DialogHeader("Leave a Comment"),
                       new DialogBody(
                           Layout.Vertical().Gap(3)
                           | Text.P("Please share your feedback or reason for leaving:")
                           | comment.ToTextareaInput()
                               .Placeholder("Enter your comment here...")
                               .Height(Size.Units(100))
                       ),
                       new DialogFooter(
                           new Button("Skip", _ => isOpen.Set(false), variant: ButtonVariant.Outline),
                           new Button("Submit", _ =>
                           {
                               if (string.IsNullOrWhiteSpace(comment.Value))
                               {
                                   client.Toast("Comment is empty, skipping...");
                               }
                               else
                               {
                                   client.Toast($"Thank you for your feedback: {comment.Value}");
                               }
                               comment.Set("");
                               isOpen.Set(false);
                           })
                       )
                   )
                   : null);
    }
}

public class AutoFocusDialogExample : ViewBase
{
    public override object? Build()
    {
        var isOpen = UseState(false);
        var searchQuery = UseState("");

        return Layout.Vertical().Gap(2)
               | new Card(
                   Layout.Vertical().Gap(3)
                   | Layout.Horizontal()
                       | Icons.Search
                       | Text.H3("AutoFocus Example")
                   | Text.P("When the dialog opens, the input is automatically focused.")
                   | new Button("Open Quick Search", _ => isOpen.Set(true))
                       .Variant(ButtonVariant.Outline)
                       .Icon(Icons.Search)
               )
               | (isOpen.Value
                   ? new Dialog(
                       _ => isOpen.Set(false),
                       new DialogHeader("Quick Search"),
                       new DialogBody(
                           Layout.Vertical().Gap(3)
                           | Text.P("Start typing to search...")
                           | searchQuery.ToSearchInput()
                               .Placeholder("Type your search query...")
                               .AutoFocus()
                       ),
                       new DialogFooter(
                           new Button("Cancel", _ => isOpen.Set(false), variant: ButtonVariant.Outline)
                       )
                   )
                   : null);
    }
}
