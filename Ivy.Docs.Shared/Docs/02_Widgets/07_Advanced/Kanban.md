---
prepare: |
  var tasks = new[]
  {
      new { Id = "1", Title = "Design Homepage", Status = "Todo", Priority = 1, Description = "Create wireframes and mockups", Assignee = "Alice" },
      new { Id = "2", Title = "Setup Database", Status = "Todo", Priority = 2, Description = "Configure PostgreSQL instance", Assignee = "Bob" },
      new { Id = "3", Title = "Implement Auth", Status = "Todo", Priority = 3, Description = "Add OAuth2 authentication", Assignee = "Charlie" },
      new { Id = "4", Title = "Code Review", Status = "In Progress", Priority = 1, Description = "Review pull requests", Assignee = "Alice" },
      new { Id = "5", Title = "Bug Fixes", Status = "In Progress", Priority = 2, Description = "Fix reported bugs", Assignee = "Bob" },
      new { Id = "6", Title = "Unit Tests", Status = "Done", Priority = 1, Description = "Write comprehensive test suite", Assignee = "Charlie" },
      new { Id = "7", Title = "Deploy to Production", Status = "Done", Priority = 2, Description = "Configure CI/CD pipeline", Assignee = "Alice" },
  };
searchHints:
  - board
  - columns
  - cards
  - drag
  - drop
  - project management
  - workflow
  - agile
  - scrum
  - trello
---

# Kanban

<Ingress>
Visualize and manage workflows with interactive kanban boards featuring drag-and-drop cards, customizable columns, and real-time updates for agile project management.
</Ingress>

The `Kanban` widget provides a powerful way to organize and track items through different stages of a workflow. It automatically groups data into columns and supports drag-and-drop interactions, making it perfect for task management, project tracking, and workflow visualization.

## Basic Usage

Create a Kanban board from any collection using the `.ToKanban()` extension method. Specify which field determines the column grouping:

```csharp demo-below
tasks.ToKanban(
    groupBySelector: t => t.Status,
    idSelector: t => t.Id,
    titleSelector: t => t.Title,
    descriptionSelector: t => t.Description
)
```

## Interactive Features

### Adding Cards

Enable card creation by providing a `HandleAdd` handler. Users can click the "+" button in column headers to add new cards:

```csharp demo-tabs
public class KanbanWithAddExample : ViewBase
{
    record Task(string Id, string Title, string Status, int Priority, string Description, string Assignee);
    
    public override object? Build()
    {
        var taskState = UseState(new[]
        {
            new Task("1", "Design Homepage", "Todo", 1, "Create wireframes", "Alice"),
            new Task("2", "Code Review", "In Progress", 2, "Review PRs", "Bob"),
        });
        
        return taskState.Value
            .ToKanban(
                groupBySelector: t => t.Status,
                idSelector: t => t.Id,
                titleSelector: t => t.Title,
                descriptionSelector: t => t.Description)
            .HandleAdd(columnKey =>
            {
                var newTask = new Task(
                    Id: (taskState.Value.Length + 1).ToString(),
                    Title: "New Task",
                    Status: columnKey,
                    Priority: 1,
                    Description: "Task description",
                    Assignee: "Unassigned"
                );
                taskState.Set(taskState.Value.Append(newTask).ToArray());
            });
    }
}
```

### Moving Cards

Enable drag-and-drop by providing a `HandleMove` handler. Users can drag cards between columns or reorder them within a column:

```csharp demo-tabs
public class KanbanWithMoveExample : ViewBase
{
    record Task(string Id, string Title, string Status, int Priority, string Description, string Assignee);
    
    public override object? Build()
    {
        var taskState = UseState(new[]
        {
            new Task("1", "Design Homepage", "Todo", 1, "Create wireframes", "Alice"),
            new Task("2", "Code Review", "In Progress", 2, "Review PRs", "Bob"),
            new Task("3", "Deploy", "Done", 3, "Deploy to production", "Charlie"),
        });
        
        return taskState.Value
            .ToKanban(
                groupBySelector: t => t.Status,
                idSelector: t => t.Id,
                titleSelector: t => t.Title,
                descriptionSelector: t => t.Description)
            .HandleMove(moveData =>
            {
                var taskId = moveData.CardId?.ToString();
                var updatedTasks = taskState.Value.ToList();
                var taskToMove = updatedTasks.FirstOrDefault(t => t.Id == taskId);
                
                if (taskToMove != null)
                {
                    // Update task status to match new column
                    var updated = taskToMove with { Status = moveData.ToColumn };
                    updatedTasks.RemoveAll(t => t.Id == taskId);
                    updatedTasks.Add(updated);
                    taskState.Set(updatedTasks.ToArray());
                }
            });
    }
}
```

### Deleting Cards

Enable card deletion by providing a `HandleDelete` handler. Users can click the delete icon on cards:

```csharp demo-tabs
public class KanbanWithDeleteExample : ViewBase
{
    record Task(string Id, string Title, string Status, int Priority, string Description, string Assignee);
    
    public override object? Build()
    {
        var taskState = UseState(new[]
        {
            new Task("1", "Design Homepage", "Todo", 1, "Create wireframes", "Alice"),
            new Task("2", "Code Review", "In Progress", 2, "Review PRs", "Bob"),
            new Task("3", "Deploy", "Done", 3, "Deploy to production", "Charlie"),
        });
        
        return taskState.Value
            .ToKanban(
                groupBySelector: t => t.Status,
                idSelector: t => t.Id,
                titleSelector: t => t.Title,
                descriptionSelector: t => t.Description)
            .HandleDelete(cardId =>
            {
                var taskId = cardId?.ToString();
                var updatedTasks = taskState.Value
                    .Where(t => t.Id != taskId)
                    .ToArray();
                taskState.Set(updatedTasks);
            });
    }
}
```

<Callout Type="tip">
The Kanban widget automatically enables interactive features when handlers are configured. You don't need to manually set `AllowAdd`, `AllowMove`, or `AllowDelete` properties - they're automatically enabled when you provide the corresponding handlers.
</Callout>

<Callout Type="info">
When using the `ToKanban` extension method, you can provide just the `groupBySelector` for basic boards, or include `idSelector`, `titleSelector`, `descriptionSelector`, and `orderSelector` for more control over card appearance and behavior.
</Callout>

## Low-Level API

For advanced scenarios, you can manually construct kanban boards using the `Kanban`, `KanbanColumn`, and `KanbanCard` widgets:

```csharp demo-tabs
public class ManualKanbanExample : ViewBase
{
    public override object? Build()
    {
        return new Kanban(
            new KanbanColumn(
                new KanbanCard("Design mockups") { CardId = "1" },
                new KanbanCard("Create wireframes") { CardId = "2" }
            ).Title("To Do").ColumnKey("todo"),
            
            new KanbanColumn(
                new KanbanCard("Implement feature") { CardId = "3" }
            ).Title("In Progress").ColumnKey("in-progress"),
            
            new KanbanColumn(
                new KanbanCard("Deploy to staging") { CardId = "4" }
            ).Title("Done").ColumnKey("done")
        ) { ShowCounts = true };
    }
}
```

## Examples

<Details>
<Summary>
Complete Project Management Board
</Summary>
<Body>

```csharp demo-tabs
public class FullKanbanExample : ViewBase
{
    record Task(string Id, string Title, string Status, int Priority, string Description, string Assignee, int ColumnOrder);
    
    int GetColumnOrder(string status) => status switch
    {
        "Todo" => 1,
        "In Progress" => 2,
        "Done" => 3,
        _ => 0
    };
    
    string GetColumnTitle(string status) => status switch
    {
        "Todo" => "📋 To Do",
        "In Progress" => "⚡ In Progress",
        "Done" => "✅ Completed",
        _ => status
    };
    
    public override object? Build()
    {
        var taskState = UseState(new[]
        {
            new Task("1", "Design Homepage", "Todo", 1, "Create wireframes", "Alice", GetColumnOrder("Todo")),
            new Task("2", "Setup Database", "Todo", 2, "Configure database", "Bob", GetColumnOrder("Todo")),
            new Task("3", "Code Review", "In Progress", 1, "Review PRs", "Charlie", GetColumnOrder("In Progress")),
            new Task("4", "Unit Tests", "Done", 1, "Write tests", "Alice", GetColumnOrder("Done")),
        });
        
        return taskState.Value
            .ToKanban(
                groupBySelector: t => t.Status,
                idSelector: t => t.Id,
                titleSelector: t => t.Title,
                descriptionSelector: t => t.Description,
                orderSelector: t => t.Priority)
            .ColumnOrder(t => t.ColumnOrder)
            .ColumnTitle(GetColumnTitle)
            .HandleAdd(columnKey =>
            {
                var newTask = new Task(
                    Id: Guid.NewGuid().ToString(),
                    Title: "New Task",
                    Status: columnKey,
                    Priority: taskState.Value.Count(t => t.Status == columnKey) + 1,
                    Description: "Add task description",
                    Assignee: "Unassigned",
                    ColumnOrder: GetColumnOrder(columnKey)
                );
                taskState.Set(taskState.Value.Append(newTask).ToArray());
            })
            .HandleMove(moveData =>
            {
                var taskId = moveData.CardId?.ToString();
                var updatedTasks = taskState.Value.ToList();
                var taskToMove = updatedTasks.FirstOrDefault(t => t.Id == taskId);
                
                if (taskToMove != null)
                {
                    var updated = taskToMove with 
                    { 
                        Status = moveData.ToColumn,
                        ColumnOrder = GetColumnOrder(moveData.ToColumn)
                    };
                    updatedTasks.RemoveAll(t => t.Id == taskId);
                    updatedTasks.Add(updated);
                    taskState.Set(updatedTasks.ToArray());
                }
            })
            .HandleDelete(cardId =>
            {
                var taskId = cardId?.ToString();
                taskState.Set(taskState.Value.Where(t => t.Id != taskId).ToArray());
            })
            .Empty(
                new Card()
                    .Title("No Tasks")
                    .Description("Create your first task to get started")
            )
            .Width(Size.Full())
            .Height(Size.Full());
    }
}
```

</Body>
</Details>

<Details>
<Summary>
Simple Status Board
</Summary>
<Body>

```csharp demo-tabs
public class SimpleStatusBoard : ViewBase
{
    public record Issue(string Id, string Title, string Status);
    
    public override object? Build()
    {
        var issues = new[]
        {
            new Issue("1", "Bug in login", "Open"),
            new Issue("2", "Feature request", "Open"),
            new Issue("3", "Performance issue", "Closed"),
        };
        
        return issues.ToKanban(
            groupBySelector: i => i.Status,
            idSelector: i => i.Id,
            titleSelector: i => i.Title,
            descriptionSelector: i => i.Id
        );
    }
}
```

</Body>
</Details>

<WidgetDocs Type="Ivy.Kanban" ExtensionTypes="Ivy.KanbanColumnExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Kanban/Kanban.cs"/>
