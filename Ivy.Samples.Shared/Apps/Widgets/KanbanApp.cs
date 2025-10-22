using Ivy.Shared;
using Ivy.Views.Kanban;

namespace Ivy.Samples.Shared.Apps.Widgets;

public class Task
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required string Status { get; set; }
    public required int StatusOrder { get; set; }
    public required int Priority { get; set; }
    public required string Description { get; set; }
    public required string Assignee { get; set; }
}

[App(icon: Icons.Kanban, path: ["Widgets"], searchHints: ["board"])]
public class KanbanApp : SampleBase
{
    protected override object? BuildSample()
    {
        var tasks = UseState(new[]
        {
            new Task { Id = "1", Title = "Design Homepage", Status = "Todo", StatusOrder = 1, Priority = 2, Description = "Create wireframes and mockups", Assignee = "Alice" },
            new Task { Id = "2", Title = "Setup Database", Status = "Todo", StatusOrder = 1, Priority = 1, Description = "Configure PostgreSQL instance", Assignee = "Bob" },
            new Task { Id = "3", Title = "Implement Auth", Status = "In Progress", StatusOrder = 2, Priority = 1, Description = "Add OAuth2 authentication", Assignee = "Charlie" },
            new Task { Id = "4", Title = "Build API", Status = "In Progress", StatusOrder = 2, Priority = 2, Description = "Create REST endpoints", Assignee = "Alice" },
            new Task { Id = "5", Title = "Unit Tests", Status = "Done", StatusOrder = 3, Priority = 2, Description = "Write comprehensive test suite", Assignee = "Bob" },
            new Task { Id = "6", Title = "Deploy to Production", Status = "Done", StatusOrder = 3, Priority = 1, Description = "Configure CI/CD pipeline", Assignee = "Charlie" },
        });

        return Layout.Vertical(
            Text.H3("Task Board Demo"),
            Text.P("Showcasing kanban features: field selectors, column/card ordering with precise drag-and-drop positioning, custom column titles, and event handlers."),

            // Kanban with common features
            tasks.Value
                .ToKanban(
                    groupBySelector: e => e.Status,
                    idSelector: e => e.Id,
                    titleSelector: e => e.Title,
                    descriptionSelector: e => e.Description,
                    orderSelector: e => e.Priority)
                .ColumnOrder(e => e.StatusOrder)
                .ColumnTitle(status => status switch
                {
                    "Todo" => "Custom Todo",
                    "In Progress" => "Custom In Progress",
                    "Done" => "Custom Done",
                    _ => status
                })
                .HandleAdd(columnKey =>
                {
                    var newTask = new Task
                    {
                        Id = (tasks.Value.Length + 1).ToString(),
                        Title = $"New Task in {columnKey}",
                        Status = columnKey,
                        StatusOrder = GetStatusOrder(columnKey),
                        Priority = GetNextPriority(columnKey, tasks.Value),
                        Description = $"Auto-generated task for {columnKey} column",
                        Assignee = "Unassigned"
                    };
                    tasks.Set(tasks.Value.Append(newTask).ToArray());
                })
                .HandleMove(moveData =>
                {
                    var taskId = moveData.CardId?.ToString();
                    if (string.IsNullOrEmpty(taskId)) return;

                    var updatedTasks = tasks.Value.Select(task =>
                        task.Id == taskId
                            ? new Task
                            {
                                Id = task.Id,
                                Title = task.Title,
                                Status = moveData.ToColumn,
                                StatusOrder = GetStatusOrder(moveData.ToColumn),
                                Priority = moveData.TargetIndex.HasValue ? moveData.TargetIndex.Value + 1 : task.Priority,
                                Description = task.Description,
                                Assignee = task.Assignee
                            }
                            : task
                    ).ToArray();

                    tasks.Set(updatedTasks);
                })
                .HandleDelete(cardId =>
                {
                    var taskId = cardId?.ToString();
                    if (string.IsNullOrEmpty(taskId)) return;

                    var updatedTasks = tasks.Value.Where(task => task.Id != taskId).ToArray();
                    tasks.Set(updatedTasks);
                })
                .Empty(
                    new Card()
                        .Title("No Tasks")
                        .Description("Create your first task to get started")
                )
        );
    }

    private static int GetStatusOrder(string status) => status switch
    {
        "Todo" => 1,
        "In Progress" => 2,
        "Done" => 3,
        _ => 0
    };

    private static int GetNextPriority(string columnKey, Task[] tasks)
    {
        var tasksInColumn = tasks.Where(t => t.Status == columnKey).ToList();
        return tasksInColumn.Count + 1;
    }
}
