---
searchHints:
  - modal
  - popup
  - window
  - dialog
  - overlay
  - confirm
  - alert
---

# Dialog

<Ingress>
Modal windows that interrupt the current workflow to request information or confirmation. Dialogs consist of three sections: Header, Body, and Footer.
</Ingress>

A `Dialog` consists of three sections:

- **`DialogHeader`**: Contains the title and close button. Accepts a `string` title.
- **`DialogBody`**: The main content area. Accepts any widgets or views as children.
- **`DialogFooter`**: Typically contains action buttons (Cancel, Confirm, etc.). Accepts any widgets as children.

## Basic Usage

```csharp demo-below
public class BasicDialogExample : ViewBase
{
    public override object? Build()
    {
        var isOpen = UseState(false);
        var client = UseService<IClientProvider>();

        return Layout.Vertical(
            new Button("Open Dialog", _ => isOpen.Set(true)),
            isOpen.Value
                ? new Dialog(
                    _ => isOpen.Set(false),
                    new DialogHeader("Confirm Action"),
                    new DialogBody(
                        Text.P("Are you sure you want to proceed with this action?")
                    ),
                    new DialogFooter(
                        new Button("Cancel", _ => isOpen.Set(false)).Outline(),
                        new Button("Confirm", _ =>
                        {
                            isOpen.Set(false);
                            client.Toast("Action confirmed!");
                        })
                    )
                )
                : null
        );
    }
}
```

## Simple Content

Use the `ToDialog()` extension method to display any content in a dialog:

```csharp demo-below
public class SimpleDialogExample : ViewBase
{
    public override object? Build()
    {
        var isOpen = UseState(false);

        var content = Layout.Vertical()
            | Text.P("This is some content")
            | new Badge("Info")
            | Text.P("You can put any widgets here.");

        return Layout.Vertical(
            new Button("Open Dialog", _ => isOpen.Set(true)),
            content.ToDialog(isOpen, title: "Simple Dialog", description: "This dialog contains simple content.")
        );
    }
}
```

## Forms in Dialogs

Use the `ToDialog()` extension method to display [forms](../../01_Onboarding/02_Concepts/08_Forms.md) in dialogs:

```csharp demo-below
public class FormDialogExample : ViewBase
{
    public record NewItemModel(string Name, string Description);

    public override object? Build()
    {
        var isOpen = UseState(false);
        var client = UseService<IClientProvider>();
        var model = UseState(() => new NewItemModel("", ""));

        async Task OnSubmit(NewItemModel submittedModel)
        {
            await Task.Delay(500);
            client.Toast($"Created: {submittedModel.Name}");
            model.Set(new NewItemModel("", ""));
        }

        return Layout.Vertical(
            new Button("Create Item", _ => isOpen.Set(true)),
            model.Value
                .ToForm()
                .OnSubmit(OnSubmit)
                .Builder(e => e.Name, e => e.ToTextInput().Placeholder("Enter item name"))
                .Builder(e => e.Description, e => e.ToTextareaInput().Placeholder("Enter description"))
                .ToDialog(isOpen, title: "Create New Item", description: "Fill in the details below.", submitTitle: "Create")
        );
    }
}
```

<WidgetDocs Type="Ivy.Dialog" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Dialogs/Dialog.cs"/>
