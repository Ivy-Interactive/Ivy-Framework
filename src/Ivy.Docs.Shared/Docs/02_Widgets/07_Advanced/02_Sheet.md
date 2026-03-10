---
searchHints:
  - sidebar
  - drawer
  - panel
  - slide-out
  - modal
  - overlay
---

# Sheet

<Ingress>
Sheets slide in from the side of the screen and display additional content while allowing the user to dismiss them. They provide a non-intrusive way to show additional information or [forms](../../01_Onboarding/02_Concepts/08_Forms.md) without navigating away from the current page.
</Ingress>

## Basic Usage

Use [UseTrigger](../../03_Hooks/02_Core/12_UseTrigger.md) to open a sheet. When open, render a `Sheet` with an `onClose` action that calls `isOpen.Set(false)`:

```csharp demo-below
public class BasicSheetExample : ViewBase
{
    public override object? Build()
    {
        var (sheetView, showSheet) = UseTrigger((IState<bool> isOpen) =>
            isOpen.Value ? new Sheet(_ => isOpen.Set(false),
                new Card(
                    "Welcome to the sheet!",
                    "This is the content inside the sheet"
                ),
                title: "This is a sheet",
                description: "Lorem ipsum dolor sit amet")
                .Width(Size.Fraction(1/2f)) : null);

        return Layout.Vertical()
            | new Button("Open Sheet", onClick: _ => showSheet())
            | sheetView;
    }
}
```

## Custom Content

You can build sheet content with a [Fragment](../01_Primitives/05_Fragment.md), [Card](../03_Common/04_Card.md), and any [widgets](../../01_Onboarding/02_Concepts/03_Widgets.md). The following uses a card with a title, description, and an action button:

```csharp demo-tabs
public class BasicSheetWithContent : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var (sheetView, showSheet) = UseTrigger((IState<bool> isOpen) =>
            isOpen.Value ? new Sheet(_ => isOpen.Set(false),
                new Fragment(
                    new Card(
                        "Welcome to the sheet!",
                        new Button("Action Button", onClick: _ => client.Toast("Button clicked!"))
                    ).Title("Sheet Content").Description("This is a simple sheet with custom content")
                ),
                title: "Basic Sheet",
                description: "A simple example of sheet usage")
                .Width(Size.Fraction(1/3f)) : null);

        return Layout.Vertical()
            | new Button("Open Basic Sheet", onClick: _ => showSheet())
            | sheetView;
    }
}
```

## Footer Actions

Use [FooterLayout](../02_Layouts/08_FooterLayout.md) to place action buttons in the sheet footer:

```csharp demo-tabs
public class SheetWithFooterActions : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var (sheetView, showSheet) = UseTrigger((IState<bool> isOpen) =>
            isOpen.Value ? new Sheet(_ => isOpen.Set(false),
                new FooterLayout(
                    Layout.Horizontal().Gap(2)
                        | new Button("Save").Variant(ButtonVariant.Primary).OnClick(_ => client.Toast("Profile saved successfully!").Success())
                        | new Button("Cancel").Variant(ButtonVariant.Outline).OnClick(_ => isOpen.Set(false)),
                    new Card(
                        "This sheet has action buttons in the footer"
                    ).Title("Content")
                ),
                title: "Actions Sheet")
                .Width(Size.Fraction(1/2f)) : null);

        return Layout.Vertical()
            | new Button("Open Sheet with Actions", onClick: _ => showSheet())
            | sheetView;
    }
}
```

## Different Widths

Control sheet width with the `.Width()` extension. For top/bottom sides, use `.Height()` instead. Widths use [Size](../../04_ApiReference/Ivy/Size.md) values such as `Size.Rem(20)`, `Size.Fraction(1/2f)`, and `Size.Full()`. Use `UseTrigger<Size>` so each button opens a sheet with a different width:

```csharp demo-tabs
public class SheetWidthExamples : ViewBase
{
    public override object? Build()
    {
        var (sheetView, showSheet) = UseTrigger((IState<bool> isOpen, Size width) =>
            isOpen.Value ? new Sheet(_ => isOpen.Set(false),
                new Card("This sheet uses a custom width.").Title("Width Example"),
                title: "Custom Width")
                .Width(width) : null);

        return Layout.Horizontal().Gap(2)
            | new Button("Rem(20)", onClick: _ => showSheet(Size.Rem(20)))
            | new Button("Half", onClick: _ => showSheet(Size.Fraction(1/2f)))
            | new Button("Two thirds", onClick: _ => showSheet(Size.Fraction(2/3f)))
            | sheetView;
    }
}
```

## Different Sides

Sheets can slide in from any edge using the `.Side()` extension with `SheetSide`: `Left`, `Right` (default), `Top`, or `Bottom`. For top/bottom, the size parameter controls height instead of width. Use `UseTrigger<SheetSide>` when the trigger needs to know which side to open:

```csharp demo-tabs
public class SheetSideExamples : ViewBase
{
    public override object? Build()
    {
        var (sheetView, showSheet) = UseTrigger((IState<bool> isOpen, SheetSide side) =>
        {
            if (!isOpen.Value) return null;
            var sheet = new Sheet(_ => isOpen.Set(false),
                new Card($"This sheet slides in from the {side}.").Title($"{side} Sheet"),
                title: $"{side} Sheet")
                .Side(side);
            return side is SheetSide.Top or SheetSide.Bottom
                ? sheet.Height(Size.Rem(20))
                : sheet.Width(Size.Fraction(1/3f));
        });

        return Layout.Horizontal().Gap(2)
            | new Button("Left Sheet", onClick: _ => showSheet(SheetSide.Left))
            | new Button("Right Sheet", onClick: _ => showSheet(SheetSide.Right))
            | new Button("Top Sheet", onClick: _ => showSheet(SheetSide.Top))
            | new Button("Bottom Sheet", onClick: _ => showSheet(SheetSide.Bottom))
            | sheetView;
    }
}
```

## Opening from a Button

When the only trigger is a single button, you can use the `WithSheet` extension on [Button](../03_Common/01_Button.md) instead of wiring `UseTrigger` yourself. It creates the open state and sheet for you:

```csharp demo-tabs
public class ButtonWithSheetExample : ViewBase
{
    public override object? Build()
    {
        return new Button("Open Sheet").WithSheet(
            () => new SheetView(),
            title: "This is a sheet",
            description: "Lorem ipsum dolor sit amet",
            width: Size.Fraction(1/2f)
        );
    }
}

public class SheetView : ViewBase
{
    public override object? Build()
    {
        return new Card(
            "Welcome to the sheet!",
            "This is the content inside the sheet"
        );
    }
}
```

<WidgetDocs Type="Ivy.Sheet" ExtensionTypes="Ivy.SheetExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Sheet.cs"/>

## Examples

<Details>
<Summary>
Sheet with Navigation
</Summary>
<Body>
You can keep internal navigation state inside the sheet (e.g. tabs or wizard steps) using [UseState](../../03_Hooks/02_Core/03_UseState.md):

```csharp demo-tabs
public class NavigationSheet : ViewBase
{
    public override object? Build()
    {
        var (sheetView, showSheet) = UseTrigger((IState<bool> isOpen) =>
            isOpen.Value ? new Sheet(_ => isOpen.Set(false),
                new NavigationSheetContent(),
                title: "Navigation Sheet")
                .Width(Size.Fraction(1/2f)) : null);

        return Layout.Vertical()
            | new Button("Open Navigation Sheet", onClick: _ => showSheet())
            | sheetView;
    }
}

public class NavigationSheetContent : ViewBase
{
    public override object? Build()
    {
        var currentPage = UseState(0);
        var pages = new[] { "Home", "Profile", "Settings", "Help" };

        return Layout.Vertical()
            | (Layout.Horizontal().Gap(2)
                | pages.Select((page, index) =>
                    new Button(page)
                        .Variant(currentPage.Value == index ? ButtonVariant.Primary : ButtonVariant.Outline)
                        .OnClick(_ => currentPage.Set(index))
                ).ToArray())
            | new Card(
                $"This is the {pages[currentPage.Value]} page content"
            ).Title("Page Content");
    }
}
```

</Body>
</Details>

<Details>
<Summary>
Complex Layout with Kanban
</Summary>
<Body>
This pattern uses `UseTrigger` with a parameter so that clicking a card opens an edit sheet for that item. The trigger callback receives the task id; the sheet content reads it and updates shared state:

```csharp demo-tabs
public record TaskItem(string Id, string Title, string Status, int Priority, string Description);

public class KanbanWithSheetExample : ViewBase
{
    public override object? Build()
    {
        var tasks = UseState(new[]
        {
            new TaskItem("1", "Design Homepage", "Todo", 1, "Create wireframes and mockups"),
            new TaskItem("2", "Setup Database", "Todo", 2, "Configure PostgreSQL instance"),
            new TaskItem("3", "Code Review", "In Progress", 1, "Review pull requests"),
            new TaskItem("4", "Performance Optimization", "In Progress", 2, "Optimize database queries"),
            new TaskItem("5", "Unit Tests", "Done", 1, "Write comprehensive test suite"),
        });

        var client = UseService<IClientProvider>();

        var (sheetView, showEdit) = UseTrigger((IState<bool> isOpen, string taskId) =>
            new TaskFormSheet(isOpen, taskId, tasks, client));

        var kanban = tasks.Value
            .ToKanban(
                groupBySelector: t => t.Status,
                idSelector: t => t.Id,
                orderSelector: t => t.Priority)
            .CardBuilder(task => new Card(task.Title, task.Description)
                .OnClick(() => showEdit(task.Id)))
            .OnMove(moveData =>
            {
                var taskId = moveData.CardId?.ToString();
                if (string.IsNullOrEmpty(taskId)) return;

                var updatedTasks = tasks.Value.ToList();
                var taskToMove = updatedTasks.FirstOrDefault(t => t.Id == taskId);
                if (taskToMove == null) return;

                var updated = taskToMove with { Status = moveData.ToColumn };
                updatedTasks.RemoveAll(t => t.Id == taskId);

                int insertIndex = updatedTasks.Count;

                var taskAtTargetIndex = updatedTasks
                    .Where(t => t.Status == moveData.ToColumn)
                    .ElementAtOrDefault(moveData.TargetIndex ?? -1);

                if (taskAtTargetIndex != null)
                {
                    insertIndex = updatedTasks.IndexOf(taskAtTargetIndex);
                }
                else
                {
                    var lastTaskInColumn = updatedTasks.LastOrDefault(t => t.Status == moveData.ToColumn);
                    if (lastTaskInColumn != null)
                    {
                        insertIndex = updatedTasks.IndexOf(lastTaskInColumn) + 1;
                    }
                }

                updatedTasks.Insert(insertIndex, updated);
                tasks.Set(updatedTasks.ToArray());
            });

        return new Fragment()
            | kanban
            | sheetView;
    }
}

public class TaskFormSheet : ViewBase
{
    private readonly IState<bool> _isOpen;
    private readonly string _taskId;
    private readonly IState<TaskItem[]> _tasks;
    private readonly IClientProvider _client;

    public TaskFormSheet(IState<bool> isOpen, string taskId, IState<TaskItem[]> tasks, IClientProvider client)
    {
        _isOpen = isOpen;
        _taskId = taskId;
        _tasks = tasks;
        _client = client;
    }

    public override object? Build()
    {
        var task = UseState(() => _tasks.Value.FirstOrDefault(t => t.Id == _taskId) ??
            new TaskItem(_taskId, "", "Todo", 1, ""));

        var (onSubmit, formView, validationView, loading) = UseForm(() => task.ToForm()
            .Required(m => m.Title, m => m.Description)
            .Builder(m => m.Status, s => s.ToSelectInput(new[] { "Todo", "In Progress", "Done" }.ToOptions()))
            .Builder(m => m.Description, s => s.ToTextareaInput())
            .Remove(m => m.Id));

        async ValueTask HandleSubmit()
        {
            if (await onSubmit())
            {
                var updatedTasks = _tasks.Value.ToList();
                var index = updatedTasks.FindIndex(t => t.Id == _taskId);
                if (index >= 0)
                {
                    updatedTasks[index] = task.Value;
                }
                _tasks.Set(updatedTasks.ToArray());
                _client.Toast($"Updated: {task.Value.Title}").Success();
                _isOpen.Set(false);
            }
        }

        var layout = new FooterLayout(
            Layout.Horizontal().Gap(2)
                | new Button("Save").OnClick(_ => HandleSubmit())
                    .Loading(loading).Disabled(loading)
                | new Button("Cancel").Variant(ButtonVariant.Outline).OnClick(_ => _isOpen.Set(false))
                | validationView,
            formView
        );

        return new Sheet(_ => _isOpen.Set(false), layout,
            title: "Edit Task",
            description: "Update task details")
            .Width(Size.Fraction(1/3f));
    }
}
```

</Body>
</Details>
