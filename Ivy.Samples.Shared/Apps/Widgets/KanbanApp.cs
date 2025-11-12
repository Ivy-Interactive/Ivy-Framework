using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Ivy.Shared;
using Ivy.Views.Kanban;

namespace Ivy.Samples.Shared.Apps.Widgets;

public record Task
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Status { get; init; }
    public required int Priority { get; init; }
    public required string Description { get; init; }
    public required string Assignee { get; init; }
}

[App(icon: Icons.Kanban, path: ["Widgets"], searchHints: ["board"])]
public class KanbanApp : SampleBase
{
    protected override object? BuildSample()
    {
        var selectedTaskId = this.UseState((string?)null);
        var tasks = UseState(
            ImmutableArray.CreateRange(new[]
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
            }));

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
                    tasks.Set(tasks.Value.Add(newTask));
                })
                .HandleMove(moveData =>
                {
                    var taskId = moveData.CardId?.ToString();
                    if (string.IsNullOrEmpty(taskId)) return;

                    var columns = BuildColumnMap(tasks.Value, GetColumnKey);
                    if (!ApplyMove(columns, moveData, taskId))
                    {
                        return;
                    }

                    tasks.Set(RebuildTasks(tasks.Value, columns, moveData.ToColumn));
                })
                .HandleDelete(cardId =>
                {
                    var taskId = cardId?.ToString();
                    if (string.IsNullOrEmpty(taskId)) return;

                    tasks.Set(tasks.Value.RemoveAll(task => task.Id == taskId));
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

    private object BuildTaskSheet(IState<string?>? selectedTaskId, IState<ImmutableArray<Task>> tasks)
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

    private static Dictionary<string, ImmutableArray<Task>.Builder> BuildColumnMap(
        ImmutableArray<Task> source,
        Func<Task, string> columnKeySelector)
    {
        var map = new Dictionary<string, ImmutableArray<Task>.Builder>(StringComparer.Ordinal);

        foreach (var task in source)
        {
            var key = columnKeySelector(task);
            if (!map.TryGetValue(key, out var builder))
            {
                builder = ImmutableArray.CreateBuilder<Task>();
                map[key] = builder;
            }

            builder.Add(task);
        }

        return map;
    }

    private static bool ApplyMove(
        Dictionary<string, ImmutableArray<Task>.Builder> columns,
        (object? CardId, string FromColumn, string ToColumn, int? TargetIndex) moveData,
        string taskId)
    {
        if (!columns.TryGetValue(moveData.FromColumn, out var sourceColumn))
        {
            return false;
        }

        if (!columns.TryGetValue(moveData.ToColumn, out var targetColumn))
        {
            targetColumn = ImmutableArray.CreateBuilder<Task>();
            columns[moveData.ToColumn] = targetColumn;
        }

        var sameColumn = ReferenceEquals(sourceColumn, targetColumn);
        var originalIndex = IndexOfTask(sourceColumn, taskId);
        var movingTask = sourceColumn[originalIndex];

        if (sameColumn && moveData.TargetIndex.HasValue)
        {
            SwapInColumn(sourceColumn, originalIndex, moveData.TargetIndex.Value);
            return true;
        }

        sourceColumn.RemoveAt(originalIndex);

        var insertIndex = Math.Clamp(moveData.TargetIndex ?? targetColumn.Count, 0, targetColumn.Count);

        if (!sameColumn && moveData.TargetIndex.HasValue && targetColumn.Count > 0 && insertIndex < targetColumn.Count)
        {
            var targetTask = targetColumn[insertIndex];
            targetColumn[insertIndex] = movingTask with { Status = moveData.ToColumn };
            var safeInsertIndex = Math.Clamp(originalIndex, 0, sourceColumn.Count);
            sourceColumn.Insert(safeInsertIndex, targetTask with { Status = moveData.FromColumn });
        }
        else
        {
            var updatedTask = movingTask with { Status = moveData.ToColumn };
            if (!sameColumn)
            {
                RemoveTaskById(targetColumn, updatedTask.Id);
            }
            targetColumn.Insert(insertIndex, updatedTask);
        }

        return true;
    }

    private static void SwapInColumn(ImmutableArray<Task>.Builder column, int originalIndex, int targetIndex)
    {
        if (column.Count == 0)
        {
            return;
        }

        var boundedTarget = Math.Clamp(targetIndex, 0, column.Count - 1);
        if (boundedTarget == originalIndex)
        {
            return;
        }

        (column[boundedTarget], column[originalIndex]) = (column[originalIndex], column[boundedTarget]);
    }

    private static int IndexOfTask(ImmutableArray<Task>.Builder column, string taskId)
    {
        for (var i = 0; i < column.Count; i++)
        {
            if (column[i].Id == taskId)
            {
                return i;
            }
        }

        throw new InvalidOperationException($"Task '{taskId}' was not found in the source column.");
    }

    private static void RemoveTaskById(ImmutableArray<Task>.Builder column, string taskId)
    {
        for (var i = column.Count - 1; i >= 0; i--)
        {
            if (column[i].Id == taskId)
            {
                column.RemoveAt(i);
                break;
            }
        }
    }

    private static ImmutableArray<Task> RebuildTasks(
        ImmutableArray<Task> original,
        Dictionary<string, ImmutableArray<Task>.Builder> columns,
        string targetColumn)
    {
        var orderedStatuses = original
            .Select(GetColumnKey)
            .Concat(new[] { targetColumn })
            .Distinct()
            .OrderBy(GetStatusOrder)
            .ThenBy(t => t);

        var result = ImmutableArray.CreateBuilder<Task>(original.Length);
        foreach (var status in orderedStatuses)
        {
            if (columns.TryGetValue(status, out var builder))
            {
                result.AddRange(builder);
            }
        }

        return result.ToImmutable();
    }

    private static string GetColumnKey(Task task) => task.Status;

    private static int GetStatusOrder(string status) => status switch
    {
        "Todo" => 1,
        "In Progress" => 2,
        "Done" => 3,
        _ => 0
    };

    private static int GetNextPriority(string columnKey, ImmutableArray<Task> tasks)
    {
        var count = tasks.Count(t => t.Status == columnKey);
        return count + 1;
    }
}
