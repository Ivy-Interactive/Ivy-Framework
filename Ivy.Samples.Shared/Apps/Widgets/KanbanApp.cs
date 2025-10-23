using Ivy.Shared;
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

        return
            // Kanban with common features
            tasks.Value
                .ToKanban(
                    groupBySelector: e => e.Status,
                    idSelector: e => e.Id,
                    titleSelector: e => e.Title,
                    descriptionSelector: e => e.Description,
                    orderSelector: e => e.Priority)
                .ColumnOrder(e => GetStatusOrder(e.Status))
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

                    var updatedTasks = tasks.Value.ToList();
                    var taskToMove = updatedTasks.FirstOrDefault(t => t.Id == taskId);
                    if (taskToMove == null) return;

                    // Update the task's status
                    taskToMove = new Task
                    {
                        Id = taskToMove.Id,
                        Title = taskToMove.Title,
                        Status = moveData.ToColumn,
                        Priority = taskToMove.Priority,
                        Description = taskToMove.Description,
                        Assignee = taskToMove.Assignee
                    };

                    // Remove the task from its current position
                    updatedTasks.RemoveAll(t => t.Id == taskId);

                    // Insert the task at the desired position within the target column
                    var tasksInTargetColumn = updatedTasks.Where(t => t.Status == moveData.ToColumn).ToList();
                    if (moveData.TargetIndex.HasValue && moveData.TargetIndex.Value < tasksInTargetColumn.Count)
                    {
                        // Insert at specific position
                        var insertIndex = moveData.TargetIndex.Value;
                        updatedTasks.InsertRange(insertIndex, new[] { taskToMove });
                    }
                    else
                    {
                        // Add to end of column
                        updatedTasks.Add(taskToMove);
                    }

                    tasks.Set(updatedTasks.ToArray());
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
