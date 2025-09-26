# Prompts

<Ingress>
Interact with users and gather input using Ivy's alert and dialog system.
</Ingress>

## Overview

The alert system in Ivy supports:

- Confirmation dialogs
- Custom alert buttons
- Dialog-based prompts
- Form-based input collection
- Alert chaining

## Basic Usage

Here's a simple example of showing a confirmation alert:

```csharp demo-tabs
public class BasicPromptView : ViewBase
{
    public override object? Build()
    {
        var (alertView, showAlert) = this.UseAlert();
        var client = this.UseService<IClientProvider>();
        
        return Layout.Vertical(
            new Button("Delete Item", onClick: _ =>
            {
                showAlert("Are you sure you want to delete this item?", result =>
                {
                    if (result == AlertResult.Ok)
                    {
                        // Item would be deleted here
                        client.Toast("Item deleted!", "Success");
                    }
                }, "Delete Item");
            }),
            alertView
        );
    }
}
```

### Text Input Prompts

Collect text input from users using dialogs with forms:

```csharp demo-tabs
public record RenameRequest
{
    public string Name { get; set; } = "Current Item Name";
}

public class RenameView : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var isOpen = this.UseState(false);
        var renameData = this.UseState(new RenameRequest());
        
        this.UseEffect(() => {
            if (!isOpen.Value && !string.IsNullOrEmpty(renameData.Value.Name))
            {
                // Item would be renamed here
                client.Toast($"Renamed to: {renameData.Value.Name}", "Success");
            }
        }, [isOpen]);
        
        return Layout.Vertical(
            new Button(
                "Rename",
                onClick: _ => isOpen.Set(true)
            ),
            isOpen.Value ? renameData.ToForm()
                .Label(e => e.Name, "Enter new name:")
                .ToDialog(isOpen, 
                    title: "Rename Item", 
                    submitTitle: "Rename"
                ) : null
        );
    }
}
```

### File Selection Prompts

Handle file selection through dialogs with file inputs:

```csharp demo-tabs
public record UploadRequest
{
    public IEnumerable<FileInput> Files { get; set; } = Array.Empty<FileInput>();
}

public class UploadView : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var isOpen = this.UseState(false);
        var uploadData = this.UseState(new UploadRequest());
        
        this.UseEffect(() => {
            if (!isOpen.Value && uploadData.Value.Files.Any())
            {
                // Files would be uploaded here
                client.Toast($"Selected {uploadData.Value.Files.Count()} file(s)", "Success");
            }
        }, [isOpen]);
        
        return Layout.Vertical(
            new Button(
                "Upload File",
                onClick: _ => isOpen.Set(true)
            ),
            isOpen.Value ? uploadData.ToForm()
                .Builder(e => e.Files, e => e.ToFileInput().Accept(".pdf,.doc,.docx"))
                .Label(e => e.Files, "Select Files:")
                .ToDialog(isOpen, 
                    title: "Upload Files", 
                    submitTitle: "Upload"
                ) : null
        );
    }
}
```

### Custom Prompts

Create custom dialogs with multiple inputs:

```csharp demo-tabs
public record CustomOptions
{
    public bool Option1 { get; set; }
    public bool Option2 { get; set; }
    public string CustomText { get; set; } = "";
}

public class CustomPromptView : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var isOpen = this.UseState(false);
        var options = this.UseState(new CustomOptions());
        
        this.UseEffect(() => {
            if (!isOpen.Value && (options.Value.Option1 || options.Value.Option2 || !string.IsNullOrEmpty(options.Value.CustomText)))
            {
                // Options would be saved here
                client.Toast("Options saved!", "Success");
            }
        }, [isOpen]);
        
        return Layout.Vertical(
            new Button(
                "Custom Prompt",
                onClick: _ => isOpen.Set(true)
            ),
            isOpen.Value ? options.ToForm()
                .Builder(e => e.Option1, e => e.ToBoolInput("Option 1"))
                .Builder(e => e.Option2, e => e.ToBoolInput("Option 2"))
                .Builder(e => e.CustomText, e => e.ToTextInput("Custom text"))
                .ToDialog(isOpen, 
                    title: "Select Options", 
                    submitTitle: "Save"
                ) : null
        );
    }
}
```

## Examples

<Details>
<Summary>
Confirmation with Custom Options
</Summary>
<Body>

```csharp demo-tabs
public record DeleteOptions
{
    public bool DeleteAssociatedFiles { get; set; }
    public bool ArchiveInsteadOfDelete { get; set; }
    public string ReasonForDeletion { get; set; } = "";
}

public class DeleteWithOptionsView : ViewBase
{
    public override object? Build()
    {
        var (alertView, showAlert) = this.UseAlert();
        var client = this.UseService<IClientProvider>();
        var isOptionsOpen = this.UseState(false);
        var deleteOptions = this.UseState(new DeleteOptions());
        
        this.UseEffect(() => {
            if (!isOptionsOpen.Value && (deleteOptions.Value.DeleteAssociatedFiles || deleteOptions.Value.ArchiveInsteadOfDelete || !string.IsNullOrEmpty(deleteOptions.Value.ReasonForDeletion)))
            {
                // Deletion with options would be performed here
                client.Toast("Item deleted with custom options", "Success");
            }
        }, [isOptionsOpen]);
        
        return Layout.Vertical(
            new Button(
                "Delete with Options",
                variant: ButtonVariant.Destructive,
                onClick: _ => {
                    showAlert(
                        "This will permanently delete the item. Do you want to configure deletion options?",
                        result => {
                            if (result == AlertResult.Yes)
                            {
                                isOptionsOpen.Set(true);
                            }
                            else if (result == AlertResult.No)
                            {
                                // Simple delete without options
                                client.Toast("Item deleted", "Success");
                            }
                        },
                        "Delete Options",
                        AlertButtonSet.YesNoCancel
                    );
                }
            ),
            alertView,
            isOptionsOpen.Value ? deleteOptions.ToForm()
                .Builder(e => e.DeleteAssociatedFiles, e => e.ToBoolInput("Delete associated files"))
                .Builder(e => e.ArchiveInsteadOfDelete, e => e.ToBoolInput("Archive instead of delete"))
                .Builder(e => e.ReasonForDeletion, e => e.ToTextInput("Reason for deletion"))
                .ToDialog(isOptionsOpen, 
                    title: "Delete Options", 
                    submitTitle: "Delete"
                ) : null
        );
    }
}
```

</Body>
</Details>
