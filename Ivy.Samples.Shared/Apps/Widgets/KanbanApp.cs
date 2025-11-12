using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Diagnostics;
using System.Linq.Expressions;
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

internal record KanbanState(
    ImmutableDictionary<string, ImmutableArray<Task>> Columns,
    ImmutableArray<string> ColumnOrder);

[App(icon: Icons.Kanban, path: ["Widgets"], searchHints: ["board"])]
public class KanbanApp : SampleBase
{
    protected override object? BuildSample()
    {
        var selectedTaskId = this.UseState((string?)null);
        var tasks = UseState(CreateInitialState(CreateInitialTasks()));
        Expression<Func<Task, string>> columnSelector = task => task.Status;

        var kanban = FlattenTasks(tasks.Value)
                .ToKanban(
                    groupBySelector: columnSelector,
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
                        Id = (GetTotalTaskCount(tasks.Value) + 1).ToString(),
                        Title = $"New Task in {columnKey}",
                        Status = columnKey,
                        Priority = GetNextPriority(columnKey, tasks.Value),
                        Description = $"Auto-generated task for {columnKey} column",
                        Assignee = "Unassigned"
                    };
                    tasks.Set(AddTask(tasks.Value, columnKey, newTask));
                })
                .HandleMove(moveData =>
                {
                    var taskId = moveData.CardId?.ToString();
                    if (string.IsNullOrEmpty(taskId)) return;

                    var updated = ApplyMove(tasks.Value, moveData, taskId);
                    if (updated != null)
                    {
                        tasks.Set(updated);
                    }
                })
                .HandleDelete(cardId =>
                {
                    var taskId = cardId?.ToString();
                    if (string.IsNullOrEmpty(taskId)) return;

                    var updated = DeleteTask(tasks.Value, taskId);
                    if (updated != null)
                    {
                        tasks.Set(updated);
                    }
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

    private object BuildTaskSheet(IState<string?>? selectedTaskId, IState<KanbanState> tasks)
    {
        var task = selectedTaskId?.Value is { } id ? FindTask(tasks.Value, id) : null;
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

    private static KanbanState AddTask(KanbanState state, string columnKey, Task task)
    {
        var columns = state.Columns;
        var columnExists = columns.TryGetValue(columnKey, out var items);
        var column = columnExists
            ? items!.Add(task)
            : ImmutableArray.Create(task);

        var updatedColumns = columns.SetItem(columnKey, column);
        var updatedOrder = columnExists
            ? state.ColumnOrder
            : BuildColumnOrder(updatedColumns, state.ColumnOrder);

        return state with { Columns = updatedColumns, ColumnOrder = updatedOrder };
    }

    private static KanbanState? ApplyMove(
        KanbanState state,
        (object? CardId, string FromColumn, string ToColumn, int? TargetIndex) moveData,
        string taskId)
    {
        if (!state.Columns.TryGetValue(moveData.FromColumn, out var sourceColumn))
        {
            return null;
        }

        var sameColumn = moveData.FromColumn == moveData.ToColumn;
        var sourceBuilder = sourceColumn.ToBuilder();
        var targetBuilder = sameColumn
            ? sourceBuilder
            : state.Columns.TryGetValue(moveData.ToColumn, out var existingTarget)
                ? existingTarget.ToBuilder()
                : ImmutableArray.CreateBuilder<Task>();

        var originalIndex = IndexOfTask(sourceBuilder, taskId, moveData.FromColumn);
        var movingTask = sourceBuilder[originalIndex];

        if (sameColumn && moveData.TargetIndex.HasValue)
        {
            SwapInColumn(sourceBuilder, originalIndex, moveData.TargetIndex.Value);
        }
        else
        {
            sourceBuilder.RemoveAt(originalIndex);

            var insertIndex = Math.Clamp(moveData.TargetIndex ?? targetBuilder.Count, 0, targetBuilder.Count);

            if (!sameColumn && moveData.TargetIndex.HasValue && targetBuilder.Count > 0 && insertIndex < targetBuilder.Count)
            {
                var targetTask = targetBuilder[insertIndex];
                targetBuilder[insertIndex] = movingTask with { Status = moveData.ToColumn };
                var safeInsertIndex = Math.Clamp(originalIndex, 0, sourceBuilder.Count);
                sourceBuilder.Insert(safeInsertIndex, targetTask with { Status = moveData.FromColumn });
            }
            else
            {
                var updatedTask = movingTask with { Status = moveData.ToColumn };
                if (!sameColumn)
                {
                    RemoveTaskById(targetBuilder, updatedTask.Id);
                }
                targetBuilder.Insert(insertIndex, updatedTask);
            }
        }

        var updatedColumns = sameColumn
            ? state.Columns.SetItem(moveData.FromColumn, sourceBuilder.ToImmutable())
            : state.Columns
                .SetItem(moveData.FromColumn, sourceBuilder.ToImmutable())
                .SetItem(moveData.ToColumn, targetBuilder.ToImmutable());

        var updatedOrder = ShouldRebuildColumnOrder(state.Columns, updatedColumns)
            ? BuildColumnOrder(updatedColumns, state.ColumnOrder)
            : state.ColumnOrder;

        return state with { Columns = updatedColumns, ColumnOrder = updatedOrder };
    }

    private static KanbanState? DeleteTask(KanbanState state, string taskId)
    {
        if (!TryLocateTask(state, taskId, out var columnKey, out var index))
        {
            return null;
        }

        var builder = state.Columns[columnKey].ToBuilder();
        builder.RemoveAt(index);

        var updatedColumns = state.Columns.SetItem(columnKey, builder.ToImmutable());
        var updatedOrder = ShouldRebuildColumnOrder(state.Columns, updatedColumns)
            ? BuildColumnOrder(updatedColumns, state.ColumnOrder)
            : state.ColumnOrder;

        return state with { Columns = updatedColumns, ColumnOrder = updatedOrder };
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

    private static int IndexOfTask(ImmutableArray<Task>.Builder column, string taskId, string columnKey)
    {
        for (var i = 0; i < column.Count; i++)
        {
            if (column[i].Id == taskId)
            {
                return i;
            }
        }

        Debug.WriteLine($"[Kanban] Task '{taskId}' was not found in column '{columnKey}'.");
        throw new InvalidOperationException($"Task '{taskId}' was not found in the source column '{columnKey}'.");
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

    private static bool TryLocateTask(KanbanState state, string taskId, out string columnKey, out int index)
    {
        foreach (var key in state.ColumnOrder)
        {
            if (!state.Columns.TryGetValue(key, out var column))
            {
                continue;
            }

            for (var i = 0; i < column.Length; i++)
            {
                if (column[i].Id == taskId)
                {
                    columnKey = key;
                    index = i;
                    return true;
                }
            }
        }

        columnKey = string.Empty;
        index = -1;
        return false;
    }

    private static Task? FindTask(KanbanState state, string taskId)
    {
        foreach (var column in state.Columns.Values)
        {
            foreach (var task in column)
            {
                if (task.Id == taskId)
                {
                    return task;
                }
            }
        }

        return null;
    }

    private static KanbanState CreateInitialState(ImmutableArray<Task> tasks)
    {
        var columns = tasks
            .GroupBy(task => task.Status)
            .ToImmutableDictionary(
                group => group.Key,
                group => group.ToImmutableArray());

        var order = BuildColumnOrder(columns, ImmutableArray<string>.Empty);

        return new KanbanState(columns, order);
    }

    private static ImmutableArray<Task> CreateInitialTasks() =>
        ImmutableArray.Create(
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
            new Task { Id = "12", Title = "User Training", Status = "Done", Priority = 3, Description = "Train users on new features", Assignee = "Alice" });

    private static IEnumerable<Task> FlattenTasks(KanbanState state)
    {
        foreach (var key in state.ColumnOrder)
        {
            if (state.Columns.TryGetValue(key, out var column))
            {
                foreach (var task in column)
                {
                    yield return task;
                }
            }
        }
    }

    private static ImmutableArray<string> BuildColumnOrder(
        ImmutableDictionary<string, ImmutableArray<Task>> columns,
        ImmutableArray<string> previousOrder)
    {
        return columns.Keys
            .Concat(previousOrder)
            .Distinct()
            .OrderBy(GetStatusOrder)
            .ThenBy(k => k)
            .ToImmutableArray();
    }

    private static bool ShouldRebuildColumnOrder(
        ImmutableDictionary<string, ImmutableArray<Task>> previous,
        ImmutableDictionary<string, ImmutableArray<Task>> updated)
    {
        if (previous.Count != updated.Count)
        {
            return true;
        }

        foreach (var key in previous.Keys)
        {
            if (!updated.ContainsKey(key))
            {
                return true;
            }
        }

        return false;
    }

    private static int GetTotalTaskCount(KanbanState state) =>
        state.Columns.Values.Sum(column => column.Length);

    private static int GetStatusOrder(string status) => status switch
    {
        "Todo" => 1,
        "In Progress" => 2,
        "Done" => 3,
        _ => 0
    };

    private static int GetNextPriority(string columnKey, KanbanState state)
    {
        var count = state.Columns.TryGetValue(columnKey, out var column)
            ? column.Length
            : 0;
        return count + 1;
    }
}
