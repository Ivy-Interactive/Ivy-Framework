---
prepare: |
  var client = this.UseService<IClientProvider>();
---

# Progress

<Ingress>
Show task completion status with customizable progress bars that support dynamic updates and multiple color variants.
</Ingress>

The `Progress` widget is used to visually represent the completion status of a task or process. It displays a visual progress bar that can be customized with different color variants and can be bound to state for dynamic updates.

## Basic Usage

Here's a simple example of a progress bar initialized at 50%.

```csharp demo-below
public class BasicProgressApp : ViewBase
{
    public override object? Build() => new Progress(50);
}
```

## State-Bound Progress

The `Progress` widget can be bound to state for dynamic updates:

```csharp demo-below
public class StateBoundProgressApp : ViewBase
{
    public override object? Build()
    {
        var progress = this.UseState(25);

        return Layout.Vertical(
            new Progress(progress.Value),
            Layout.Horizontal(
                new Button("0%", _ => progress.Set(0)),
                new Button("25%", _ => progress.Set(25)),
                new Button("50%", _ => progress.Set(50)),
                new Button("75%", _ => progress.Set(75)),
                new Button("100%", _ => progress.Set(100))
            )
        );
    }
}
```

## Color Variants

The `Progress` widget supports different color variants to match your application's design:

### Primary (Default)

```csharp demo-below 
public class PrimaryProgressApp : ViewBase
{
    public override object? Build() => new Progress(65);
}
```

### Emerald Gradient

```csharp demo-below 
public class EmeraldProgressApp : ViewBase
{
    public override object? Build() => new Progress(65).ColorVariant(Progress.ColorVariants.EmeraldGradient);
}
```

## Goal Display

You can add a goal text to provide context about what the progress represents:

```csharp demo-below 
public class GoalProgressApp : ViewBase
{
    public override object? Build() => Layout.Vertical(
        new Progress(75).Goal("Task completion"),
        new Progress(40).Goal("Upload progress").ColorVariant(Progress.ColorVariants.EmeraldGradient),
        new Progress(90).Goal("Loading...")
    );
}
```

## Interactive Example

Here's a comprehensive interactive example that demonstrates progress tracking with increment/decrement controls:

```csharp demo-below 
public class InteractiveProgressApp : ViewBase
{
    public override object? Build()
    {
        var taskProgress = this.UseState(0);
        var uploadProgress = this.UseState(33);
        var uploadGoal = this.UseState("Ready to upload");

        return Layout.Vertical()
            | Text.H3("Task Progress")
            | new Progress(taskProgress.Value).Goal($"Tasks completed: {taskProgress.Value}%")
            | Layout.Horizontal(
                new Button("Reset", _ => taskProgress.Set(0)).Secondary(),
                new Button("-10", _ => taskProgress.Set(Math.Max(0, taskProgress.Value - 10))),
                new Button("+10", _ => taskProgress.Set(Math.Min(100, taskProgress.Value + 10))),
                new Button("Complete", _ => taskProgress.Set(100))
            )
            | Text.H3("Upload Progress") 
            | new Progress(uploadProgress.Value)
                .Goal(uploadGoal.Value)
                .ColorVariant(Progress.ColorVariants.EmeraldGradient)
            | Layout.Horizontal(
                new Button("Simulate Upload", _ => {
                    uploadProgress.Set(0);
                    uploadGoal.Set("Starting upload...");
                    
                    Task.Run(async () => {
                        var steps = new[] { 
                            "Preparing files...", 
                            "Uploading...", 
                            "Processing...", 
                            "Finalizing...", 
                            "Complete!" 
                        };
                        
                        for(int i = 0; i < steps.Length; i++) {
                            uploadGoal.Set(steps[i]);
                            uploadProgress.Set((i + 1) * 20);
                            await Task.Delay(500);
                        }
                    });
                })
            );
    }
}
```

## Common Use Cases

### File Upload Progress

```csharp demo-below 
public class FileUploadProgressApp : ViewBase
{
    public override object? Build()
    {
        var fileProgress = this.UseState(67);
        return Layout.Vertical(
            new Progress(fileProgress.Value)
                .Goal("Uploading document.pdf")
                .ColorVariant(Progress.ColorVariants.EmeraldGradient),
            Text.Small($"{fileProgress.Value}% complete")
        );
    }
}
```

### Task Completion

```csharp demo-below 
public class TaskCompletionApp : ViewBase
{
    public override object? Build()
    {
        var completedTasks = this.UseState(4);
        var totalTasks = 7;
        var percentage = (int)((completedTasks.Value / (double)totalTasks) * 100);

        return Layout.Vertical(
            new Progress(percentage).Goal($"Tasks: {completedTasks.Value}/{totalTasks}"),
            Layout.Horizontal(
                new Button("Complete Task", _ => completedTasks.Set(Math.Min(totalTasks, completedTasks.Value + 1))),
                new Button("Reset", _ => completedTasks.Set(0)).Secondary()
            )
        );
    }
}
```

### Loading States

```csharp demo-below 
public class LoadingStatesApp : ViewBase
{
    public override object? Build()
    {
        var loadingProgress = this.UseState(0);
        var loadingGoal = this.UseState("Ready to start");

        return Layout.Vertical(
            new Progress(loadingProgress.Value).Goal(loadingGoal.Value),
            new Button("Start Loading", _ => {
                loadingProgress.Set(0);
                loadingGoal.Set("Initializing...");
                
                Task.Run(async () => {
                    var steps = new[] { 
                        "Initializing...", 
                        "Loading modules...", 
                        "Connecting to services...", 
                        "Ready!" 
                    };
                    
                    for(int i = 0; i < steps.Length; i++) {
                        loadingGoal.Set(steps[i]);
                        loadingProgress.Set((i + 1) * 25);
                        await Task.Delay(800);
                    }
                });
            })
        );
    }
}
```

<WidgetDocs Type="Ivy.Progress" ExtensionTypes="Ivy.ProgressExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Progress.cs"/>
