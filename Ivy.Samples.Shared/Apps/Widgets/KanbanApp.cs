using System;
using System.Collections.Generic;
using System.Linq;
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
                    titleSelector: e => e.Title,
                    descriptionSelector: e => e.Description,
                    orderSelector: e => e.Priority)
                .ColumnOrder(e => GetStatusOrder(e.Status))
                .Width(Size.Full())
                .Width(e => e.Status, Size.Fraction(0.33f))
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

                    var columns = tasks.Value
                        .GroupBy(t => t.Status)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(task => new Task
                            {
                                Id = task.Id,
                                Title = task.Title,
                                Status = task.Status,
                                Priority = task.Priority,
                                Description = task.Description,
                                Assignee = task.Assignee
                            }).ToList());

                    if (!columns.TryGetValue(moveData.FromColumn, out var sourceList))
                    {
                        return;
                    }

                    var originalIndex = sourceList.FindIndex(t => t.Id == taskId);
                    if (originalIndex < 0)
                    {
                        return;
                    }

                    var movingTask = sourceList[originalIndex];

                    if (!columns.TryGetValue(moveData.ToColumn, out var targetList))
                    {
                        targetList = new List<Task>();
                        columns[moveData.ToColumn] = targetList;
                    }

                    if (moveData.FromColumn == moveData.ToColumn && moveData.TargetIndex.HasValue)
                    {
                        var swapIndex = Math.Clamp(moveData.TargetIndex.Value, 0, targetList.Count - 1);
                        if (swapIndex == originalIndex)
                        {
                            return;
                        }

                        var targetTask = targetList[swapIndex];
                        var movingClone = CloneTask(movingTask, moveData.ToColumn);
                        var targetClone = CloneTask(targetTask, moveData.ToColumn);

                        targetList[swapIndex] = movingClone;
                        targetList[originalIndex] = targetClone;
                    }
                    else
                    {
                        sourceList.RemoveAt(originalIndex);

                        var insertIndex = moveData.TargetIndex ?? targetList.Count;
                        insertIndex = Math.Clamp(insertIndex, 0, targetList.Count);

                        if (moveData.TargetIndex.HasValue && targetList.Count > 0 && insertIndex < targetList.Count)
                        {
                            var targetTask = targetList[insertIndex];
                            var movingClone = CloneTask(movingTask, moveData.ToColumn);
                            var targetCloneForSource = CloneTask(targetTask, moveData.FromColumn);

                            targetList[insertIndex] = movingClone;
                            sourceList.Insert(originalIndex, targetCloneForSource);
                        }
                        else
                        {
                            var movingClone = CloneTask(movingTask, moveData.ToColumn);
                            targetList.RemoveAll(task => task.Id == movingClone.Id);
                            targetList.Insert(insertIndex, movingClone);
                        }
                    }

                    var orderedStatuses = tasks.Value
                        .Select(t => t.Status)
                        .Concat(new[] { moveData.ToColumn })
                        .Distinct()
                        .OrderBy(GetStatusOrder)
                        .ThenBy(t => t);

                    var reorderedTasks = new List<Task>();
                    foreach (var status in orderedStatuses)
                    {
                        if (columns.TryGetValue(status, out var list))
                        {
                            reorderedTasks.AddRange(list);
                        }
                    }

                    tasks.Set(reorderedTasks.ToArray());

                    static Task CloneTask(Task task, string status) => new Task
                    {
                        Id = task.Id,
                        Title = task.Title,
                        Status = status,
                        Priority = task.Priority,
                        Description = task.Description,
                        Assignee = task.Assignee
                    };
                })
                .HandleDelete(cardId =>
                {
                    var taskId = cardId?.ToString();
                    if (string.IsNullOrEmpty(taskId)) return;

                    var updatedTasks = tasks.Value.Where(task => task.Id != taskId).ToArray();
                    tasks.Set(updatedTasks);
                })
                .HandleClick(cardId =>
                {
                    var taskId = cardId?.ToString();
                    if (taskId != null)
                        selectedTaskId.Set(taskId);
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

    private static int GetNextPriority(string columnKey, Task[] tasks)
    {
        var tasksInColumn = tasks.Where(t => t.Status == columnKey).ToList();
        return tasksInColumn.Count + 1;
    }
}
