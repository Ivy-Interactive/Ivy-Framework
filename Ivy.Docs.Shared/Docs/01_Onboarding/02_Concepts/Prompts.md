# Prompts

<Ingress>
Interact with users and gather input using Ivy's prompt system.
</Ingress>

## Overview

The prompt system in Ivy supports:

- Confirmation dialogs
- Text input prompts
- File selection prompts
- Custom prompts
- Prompt chaining
- Async prompt handling

## Basic Usage

Here's a simple example of showing a confirmation prompt:

```csharp
var client = this.UseService<IClientProvider>();
var result = await client.Prompt.ConfirmAsync(
    "Delete Item",
    "Are you sure you want to delete this item?"
);
if (result)
{
    await DeleteItem();
}
```

### Confirmation Prompts

Get user confirmation for important actions:

```csharp
public class DeleteView : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Button(
            "Delete",
            variant: ButtonVariant.Destructive,
            onClick: async _ => {
                var confirmed = await client.Prompt.ConfirmAsync(
                    "Delete Item",
                    "This action cannot be undone. Are you sure?"
                );
                
                if (confirmed)
                {
                    await DeleteItem();
                }
            }
        );
    }
}
```

### Text Input Prompts

Collect text input from users:

```csharp
public class RenameView : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Button(
            "Rename",
            onClick: async _ => {
                var newName = await client.Prompt.TextAsync(
                    "Rename Item",
                    "Enter new name:",
                    defaultValue: currentName
                );
                
                if (newName != null)
                {
                    await RenameItem(newName);
                }
            }
        );
    }
}
```

### File Selection Prompts

Handle file selection through prompts:

```csharp
public class UploadView : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Button(
            "Upload File",
            onClick: async _ => {
                var files = await client.Prompt.FilesAsync(
                    "Select Files",
                    multiple: true,
                    accept: ".pdf,.doc,.docx"
                );
                
                if (files?.Length > 0)
                {
                    await UploadFiles(files);
                }
            }
        );
    }
}
```

### Custom Prompts

Create custom prompts for specific needs:

```csharp
public class CustomPromptView : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Button(
            "Custom Prompt",
            onClick: async _ => {
                var result = await client.Prompt.CustomAsync(
                    "Select Options",
                    new CustomPromptOptions
                    {
                        Content = new VerticalLayout(
                            Text.Inline("Choose your options:"),
                            new Checkbox("Option 1"),
                            new Checkbox("Option 2"),
                            new TextInput("Custom text")
                        ),
                        Buttons = new[]
                        {
                            new PromptButton("Cancel", PromptButtonType.Cancel),
                            new PromptButton("Save", PromptButtonType.Confirm)
                        }
                    }
                );
                
                if (result.Confirmed)
                {
                    await SaveOptions(result.Data);
                }
            }
        );
    }
}
```

### Prompt Chaining

Chain multiple prompts together:

```csharp
public class MultiStepView : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Button(
            "Start Process",
            onClick: async _ => {
                // Step 1: Confirmation
                var confirmed = await client.Prompt.ConfirmAsync(
                    "Start Process",
                    "Begin the multi-step process?"
                );
                
                if (!confirmed) return;
                
                // Step 2: Text Input
                var name = await client.Prompt.TextAsync(
                    "Enter Name",
                    "What is your name?"
                );
                
                if (name == null) return;
                
                // Step 3: File Selection
                var files = await client.Prompt.FilesAsync(
                    "Select Files",
                    multiple: true
                );
                
                if (files?.Length > 0)
                {
                    await ProcessFiles(name, files);
                }
            }
        );
    }
}
```

### Best Practices

1. **Clear Messages**: Use clear and concise prompt messages
2. **Default Values**: Provide sensible default values when appropriate
3. **Validation**: Implement input validation for text prompts
4. **Error Handling**: Handle prompt cancellation and errors gracefully
5. **User Experience**: Keep prompts focused and avoid asking too many questions
6. **Accessibility**: Ensure prompts are accessible to screen readers
7. **Consistency**: Maintain consistent prompt styling across your project

## Examples

### Form Validation with Prompts

```csharp
public class FormView : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var formData = UseState(new FormData());
        
        return new Form(
            onSubmit: async () => {
                if (string.IsNullOrEmpty(formData.Value.Name))
                {
                    var name = await client.Prompt.TextAsync(
                        "Missing Name",
                        "Please enter your name:"
                    );
                    
                    if (name == null) return;
                    formData.Set(v => v.Name = name);
                }
                
                await SubmitForm(formData.Value);
            }
        )
        | new TextInput("Name", value: formData.Value.Name, onChange: v => formData.Set(v))
        | new Button("Submit");
    }
}
```

### Confirmation with Custom Options

```csharp
public class DeleteWithOptionsView : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Button(
            "Delete with Options",
            variant: ButtonVariant.Destructive,
            onClick: async _ => {
                var result = await client.Prompt.CustomAsync(
                    "Delete Options",
                    new CustomPromptOptions
                    {
                        Content = new VerticalLayout(
                            Text.Inline("Choose deletion options:"),
                            new Checkbox("Delete associated files"),
                            new Checkbox("Archive instead of delete"),
                            new TextInput("Reason for deletion")
                        ),
                        Buttons = new[]
                        {
                            new PromptButton("Cancel", PromptButtonType.Cancel),
                            new PromptButton("Delete", PromptButtonType.Destructive)
                        }
                    }
                );
                
                if (result.Confirmed)
                {
                    await DeleteWithOptions(result.Data);
                }
            }
        );
    }
}
```
