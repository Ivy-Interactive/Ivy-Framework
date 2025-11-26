using Ivy.Shared;
using Ivy.Views.Builders;
using Ivy.Views.Kanban;

namespace Ivy.Samples.Shared.Apps.Widgets;

public class Task
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required string Status { get; set; }
    public required int Priority { get; set; }
    public required string Description { get; set; }
    public required string Assignee { get; set; }
}

[App(icon: Icons.Kanban, path: ["Widgets"], searchHints: ["board"])]
public class KanbanApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Tabs(
            new Tab("Basic Example", new BasicKanbanExample()),
            new Tab("Builder Example", new KanbanBuilderExample()),
            new Tab("Width Examples", new KanbanWidthExamples()),
            new Tab("Header Layout Example", new KanbanHeaderLayoutExample())
        ).Variant(TabsVariant.Content);
    }
}

public class BasicKanbanExample : ViewBase
{
    public override object? Build()
    {
        var selectedTaskId = this.UseState((string?)null);
        var tasks = UseState(new[]
        {
            new Task { Id = "1", Title = "Design Homepage", Status = "Todo", Priority = 2, Description = "Create wireframes and mockups", Assignee = "Alice" },
            new Task { Id = "2", Title = "Setup Database", Status = "Todo", Priority = 1, Description = "Configure PostgreSQL instance", Assignee = "Bob" },
            new Task { Id = "3", Title = "Implement Auth", Status = "Todo", Priority = 3, Description = "Add OAuth2 authentication", Assignee = "Charlie" },
            new Task { Id = "4", Title = "Build API", Status = "Todo", Priority = 4, Description = "Create REST endpoints", Assignee = "Alice" },
            new Task { Id = "5", Title = "Write Tests", Status = "Todo", Priority = 5, Description = "Unit and integration tests", Assignee = "Bob" },
            new Task { Id = "6", Title = "Code Review", Status = "In Progress", Priority = 1, Description = "Review pull requests", Assignee = "Charlie" },
            new Task { Id = "7", Title = "Performance Optimization", Status = "In Progress", Priority = 2, Description = "Optimize database queries", Assignee = "Alice" },
            new Task { Id = "8", Title = "Bug Fixes", Status = "In Progress", Priority = 3, Description = "Fix reported bugs", Assignee = "Bob" },
            new Task { Id = "9", Title = "Documentation", Status = "In Progress", Priority = 4, Description = "Update API documentation", Assignee = "Charlie" },
            new Task { Id = "10", Title = "Unit Tests", Status = "Done", Priority = 1, Description = "Write comprehensive test suite", Assignee = "Bob" },
            new Task { Id = "11", Title = "Deploy to Production", Status = "Done", Priority = 2, Description = "Configure CI/CD pipeline", Assignee = "Charlie" },
            new Task { Id = "12", Title = "User Training", Status = "Done", Priority = 3, Description = "Train users on new features", Assignee = "Alice" },
        });

        var kanban = tasks.Value
                .ToKanban(
                    groupBySelector: e => e.Status,
                    idSelector: e => e.Id,
                    orderSelector: e => e.Priority)
                .CardBuilder(task => new Card()
                    .Title(task.Title)
                    .Description(task.Description))
                .ColumnOrder(e => GetStatusOrder(e.Status))
                .Width(Size.Full())
                .HandleMove(moveData =>
                {
                    var taskId = moveData.CardId?.ToString();
                    if (string.IsNullOrEmpty(taskId)) return;

                    var updatedTasks = tasks.Value.ToList();
                    var taskToMove = updatedTasks.FirstOrDefault(t => t.Id == taskId);
                    if (taskToMove == null) return;

                    // Update the task's status
                    var newTask = new Task
                    {
                        Id = taskToMove.Id,
                        Title = taskToMove.Title,
                        Status = moveData.ToColumn,
                        Priority = taskToMove.Priority,
                        Description = taskToMove.Description,
                        Assignee = taskToMove.Assignee
                    };

                    // Remove the old task reference directly
                    updatedTasks.Remove(taskToMove);

                    // Determine insertion index
                    int insertIndex = updatedTasks.Count;

                    // Try to find the task that is currently at the target index in the target column
                    var taskAtTargetIndex = updatedTasks
                        .Where(t => t.Status == moveData.ToColumn)
                        .ElementAtOrDefault(moveData.TargetIndex ?? -1);

                    if (taskAtTargetIndex != null)
                    {
                        insertIndex = updatedTasks.IndexOf(taskAtTargetIndex);
                    }
                    else
                    {
                        // If not found (e.g. appended to end), find the last task in that column
                        var lastTaskInColumn = updatedTasks.LastOrDefault(t => t.Status == moveData.ToColumn);
                        if (lastTaskInColumn != null)
                        {
                            insertIndex = updatedTasks.IndexOf(lastTaskInColumn) + 1;
                        }
                    }

                    updatedTasks.Insert(insertIndex, newTask);

                    tasks.Set(updatedTasks.ToArray());
                })
                .Empty(
                    new Card()
                        .Title("No Tasks")
                        .Description("Create your first task to get started")
                );

        return new Fragment(
            kanban,
            selectedTaskId.Value != null ? BuildTaskSheet(selectedTaskId as IState<string?>, tasks) : null
        );
    }

    private object BuildTaskSheet(IState<string?>? selectedTaskId, IState<Task[]> tasks)
    {
        var task = tasks.Value.FirstOrDefault(t => t.Id == selectedTaskId?.Value);
        if (task == null) return new Fragment();

        return new Sheet(
            onClose: () => selectedTaskId?.Set((string?)null),
            content: Layout.Vertical()
                | new Card()
                    .Title(task.Title)
                    .Description(task.Description)
                | Layout.Horizontal()
                    | new Card().Title("Priority").Description($"P{task.Priority}")
                    | new Card().Title("Assignee").Description(task.Assignee)
                    | new Card().Title("Status").Description(task.Status),
            title: task.Title,
            description: "Task Details"
        ).Width(Size.Rem(32));
    }

    private static int GetStatusOrder(string status) => status switch
    {
        "Todo" => 1,
        "In Progress" => 2,
        "Done" => 3,
        _ => 0
    };
}

public class KanbanBuilderExample : ViewBase
{
    public override object? Build()
    {
        var selectedTaskId = this.UseState((string?)null);
        var tasks = UseState(new[]
        {
            new Task { Id = "1", Title = "Design Homepage", Status = "Todo", Priority = 2, Description = "Create wireframes and mockups", Assignee = "Alice" },
            new Task { Id = "2", Title = "Setup Database", Status = "Todo", Priority = 1, Description = "Configure PostgreSQL instance", Assignee = "Bob" },
            new Task { Id = "3", Title = "Implement Auth", Status = "Todo", Priority = 3, Description = "Add OAuth2 authentication", Assignee = "Charlie" },
            new Task { Id = "4", Title = "Build API", Status = "In Progress", Priority = 1, Description = "Create REST endpoints", Assignee = "Alice" },
            new Task { Id = "5", Title = "Write Tests", Status = "In Progress", Priority = 2, Description = "Unit and integration tests", Assignee = "Bob" },
            new Task { Id = "6", Title = "Deploy to Production", Status = "Done", Priority = 1, Description = "Configure CI/CD pipeline", Assignee = "Charlie" },
        });

        var kanban = tasks.Value
                .ToKanban(
                    groupBySelector: e => e.Status,
                    idSelector: e => e.Id,
                    orderSelector: e => e.Priority)
                .CardBuilder(task => new Card(
                    content: task.ToDetails()
                        .Remove(x => x.Id)
                        .MultiLine(x => x.Description)
                ))
                .ColumnOrder(e => GetStatusOrder(e.Status))
                .Width(Size.Full())
                .HandleMove(moveData =>
    {
        var taskId = moveData.CardId?.ToString();
        if (string.IsNullOrEmpty(taskId)) return;

        var updatedTasks = tasks.Value.ToList();
        var taskToMove = updatedTasks.FirstOrDefault(t => t.Id == taskId);
        if (taskToMove == null) return;

        var newTask = new Task
        {
            Id = taskToMove.Id,
            Title = taskToMove.Title,
            Status = moveData.ToColumn,
            Priority = taskToMove.Priority,
            Description = taskToMove.Description,
            Assignee = taskToMove.Assignee
        };

        updatedTasks.Remove(taskToMove);

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

        updatedTasks.Insert(insertIndex, newTask);

        tasks.Set(updatedTasks.ToArray());
    })
                .Empty(
                    new Card()
                        .Title("No Tasks")
                        .Description("Create your first task to get started")
                );

        return new Fragment(
            kanban,
            selectedTaskId.Value != null ? BuildTaskSheet(selectedTaskId as IState<string?>, tasks) : null
        );
    }

    private object BuildTaskSheet(IState<string?>? selectedTaskId, IState<Task[]> tasks)
    {
        var task = tasks.Value.FirstOrDefault(t => t.Id == selectedTaskId?.Value);
        if (task == null) return new Fragment();

        return new Sheet(
            onClose: () => selectedTaskId?.Set((string?)null),
            content: Layout.Vertical()
                | new Card()
                    .Title(task.Title)
                    .Description(task.Description)
                | Layout.Horizontal()
                    | new Card().Title("Priority").Description($"P{task.Priority}")
                    | new Card().Title("Assignee").Description(task.Assignee)
                    | new Card().Title("Status").Description(task.Status),
            title: task.Title,
            description: "Task Details"
        ).Width(Size.Rem(32));
    }

    private static int GetStatusOrder(string status) => status switch
    {
        "Todo" => 1,
        "In Progress" => 2,
        "Done" => 3,
        _ => 0
    };
}

public class KanbanWidthExamples : ViewBase
{
    public override object? Build()
    {
        var tasks = UseState(new[]
        {
            new Task { Id = "1", Title = "Design Homepage", Status = "Todo", Priority = 2, Description = "Create wireframes", Assignee = "Alice" },
            new Task { Id = "2", Title = "Setup Database", Status = "Todo", Priority = 1, Description = "Configure PostgreSQL", Assignee = "Bob" },
            new Task { Id = "3", Title = "Implement Auth", Status = "In Progress", Priority = 1, Description = "Add OAuth2", Assignee = "Charlie" },
            new Task { Id = "4", Title = "Write Tests", Status = "In Progress", Priority = 2, Description = "Unit tests", Assignee = "Bob" },
            new Task { Id = "5", Title = "Deploy to Production", Status = "Done", Priority = 1, Description = "CI/CD pipeline", Assignee = "Charlie" },
        });

        return Layout.Vertical()
            | Layout.Horizontal()
                | new Card()
                    .Title("Narrow Kanban (50rem width)")
                    .Description("Kanban with narrow overall width")
                    .Width(Size.Full())
            | tasks.Value
                .ToKanban(
                    groupBySelector: e => e.Status,
                    idSelector: e => e.Id,
                    orderSelector: e => e.Priority)
                .CardBuilder(task => new Card()
                    .Title(task.Title)
                    .Description(task.Description))
                .ColumnOrder(e => GetStatusOrder(e.Status))
                .Width(Size.Rem(50))
                .Empty(new Card().Title("No Tasks").Description("Empty state"))
            | Layout.Horizontal()
                | new Card()
                    .Title("Wide Kanban (80rem width)")
                    .Description("Kanban with wide overall width")
                    .Width(Size.Full())
            | tasks.Value
                .ToKanban(
                    groupBySelector: e => e.Status,
                    idSelector: e => e.Id,
                    orderSelector: e => e.Priority)
                .CardBuilder(task => new Card()
                    .Title(task.Title)
                    .Description(task.Description))
                .ColumnOrder(e => GetStatusOrder(e.Status))
                .Width(Size.Rem(80))
                .Empty(new Card().Title("No Tasks").Description("Empty state"))
            | Layout.Horizontal()
                | new Card()
                    .Title("Narrow Columns (12rem per column)")
                    .Description("Kanban with narrow column width")
                    .Width(Size.Full())
            | tasks.Value
                .ToKanban(
                    groupBySelector: e => e.Status,
                    idSelector: e => e.Id,
                    orderSelector: e => e.Priority)
                .CardBuilder(task => new Card()
                    .Title(task.Title)
                    .Description(task.Description))
                .ColumnOrder(e => GetStatusOrder(e.Status))
                .Width(Size.Full())
                .ColumnWidth(Size.Rem(12))
                .Empty(new Card().Title("No Tasks").Description("Empty state"))
            | Layout.Horizontal()
                | new Card()
                    .Title("Wide Columns (25rem per column)")
                    .Description("Kanban with wide column width")
                    .Width(Size.Full())
            | tasks.Value
                .ToKanban(
                    groupBySelector: e => e.Status,
                    idSelector: e => e.Id,
                    orderSelector: e => e.Priority)
                .CardBuilder(task => new Card()
                    .Title(task.Title)
                    .Description(task.Description))
                .ColumnOrder(e => GetStatusOrder(e.Status))
                .Width(Size.Full())
                .ColumnWidth(Size.Rem(25))
                .Empty(new Card().Title("No Tasks").Description("Empty state"));
    }

    private static int GetStatusOrder(string status) => status switch
    {
        "Todo" => 1,
        "In Progress" => 2,
        "Done" => 3,
        _ => 0
    };
}

public class KanbanHeaderLayoutExample : ViewBase
{
    public override object? Build()
    {
        var tasks = UseState(new[]
        {
            new Task { Id = "1", Title = "Design Homepage", Status = "Todo", Priority = 2, Description = "Create wireframes and mockups", Assignee = "Alice" },
            new Task { Id = "2", Title = "Setup Database", Status = "Todo", Priority = 1, Description = "Configure PostgreSQL instance", Assignee = "Bob" },
            new Task { Id = "3", Title = "Implement Auth", Status = "Todo", Priority = 3, Description = "Add OAuth2 authentication", Assignee = "Charlie" },
            new Task { Id = "4", Title = "Build API", Status = "In Progress", Priority = 1, Description = "Create REST endpoints", Assignee = "Alice" },
            new Task { Id = "5", Title = "Write Tests", Status = "In Progress", Priority = 2, Description = "Unit and integration tests", Assignee = "Bob" },
            new Task { Id = "6", Title = "Code Review", Status = "In Progress", Priority = 3, Description = "Review pull requests", Assignee = "Charlie" },
            new Task { Id = "7", Title = "Deploy to Production", Status = "Done", Priority = 1, Description = "Configure CI/CD pipeline", Assignee = "Alice" },
            new Task { Id = "8", Title = "User Training", Status = "Done", Priority = 2, Description = "Train users on new features", Assignee = "Bob" },
        });

        var client = UseService<IClientProvider>();

        void OnAddTask(Event<Button> @event)
        {
            var newTask = new Task
            {
                Id = Guid.NewGuid().ToString(),
                Title = $"New Task {tasks.Value.Length + 1}",
                Status = "Todo",
                Priority = 1,
                Description = "A newly created task",
                Assignee = "Unassigned"
            };
            tasks.Set(tasks.Value.Append(newTask).ToArray());
            client.Toast($"Added task: {newTask.Title}");
        }

        var createBtn = new Button("Add Task")
            .Icon(Icons.Plus)
            .Variant(ButtonVariant.Primary)
            .HandleClick(OnAddTask);

        var kanban = tasks.Value
            .ToKanban(
                groupBySelector: e => e.Status,
                idSelector: e => e.Id,
                orderSelector: e => e.Priority)
            .CardBuilder(task => new Card()
                .Title(task.Title)
                .Description(task.Description))
            .ColumnOrder(e => GetStatusOrder(e.Status))
            .Width(Size.Full())
            .Height(Size.Full())
            .HandleMove(moveData =>
            {
                var taskId = moveData.CardId?.ToString();
                if (string.IsNullOrEmpty(taskId)) return;

                var updatedTasks = tasks.Value.ToList();
                var taskToMove = updatedTasks.FirstOrDefault(t => t.Id == taskId);
                if (taskToMove == null) return;

                var newTask = new Task
                {
                    Id = taskToMove.Id,
                    Title = taskToMove.Title,
                    Status = moveData.ToColumn,
                    Priority = taskToMove.Priority,
                    Description = taskToMove.Description,
                    Assignee = taskToMove.Assignee
                };

                updatedTasks.Remove(taskToMove);

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

                updatedTasks.Insert(insertIndex, newTask);
                tasks.Set(updatedTasks.ToArray());
            })
            .Empty(
                new Card()
                    .Title("No Tasks")
                    .Description("Create your first task to get started")
            );

        var header = Layout.Horizontal() | createBtn;

        var body = new HeaderLayout(header, kanban)
            .Scroll(Scroll.None);

        return body;
    }

    private static int GetStatusOrder(string status) => status switch
    {
        "Todo" => 1,
        "In Progress" => 2,
        "Done" => 3,
        _ => 0
    };
}
