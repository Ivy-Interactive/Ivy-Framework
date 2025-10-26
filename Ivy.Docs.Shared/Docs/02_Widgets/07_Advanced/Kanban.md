---
prepare: |
  var tasks = new[]
  {
      new { Id = "1", Title = "Design Homepage", Status = "Todo", Priority = 1, Description = "Create wireframes and mockups", Assignee = "Alice" },
      new { Id = "2", Title = "Setup Database", Status = "Todo", Priority = 2, Description = "Configure PostgreSQL instance", Assignee = "Bob" },
      new { Id = "3", Title = "Implement Auth", Status = "Todo", Priority = 3, Description = "Add OAuth2 authentication", Assignee = "Charlie" },
      new { Id = "4", Title = "Build API", Status = "Todo", Priority = 4, Description = "Create REST endpoints", Assignee = "Alice" },
      new { Id = "5", Title = "Code Review", Status = "In Progress", Priority = 1, Description = "Review pull requests", Assignee = "Charlie" },
      new { Id = "6", Title = "Performance Optimization", Status = "In Progress", Priority = 2, Description = "Optimize database queries", Assignee = "Alice" },
      new { Id = "7", Title = "Bug Fixes", Status = "In Progress", Priority = 3, Description = "Fix reported bugs", Assignee = "Bob" },
      new { Id = "8", Title = "Unit Tests", Status = "Done", Priority = 1, Description = "Write comprehensive test suite", Assignee = "Bob" },
      new { Id = "9", Title = "Deploy to Production", Status = "Done", Priority = 2, Description = "Configure CI/CD pipeline", Assignee = "Charlie" },
      new { Id = "10", Title = "User Training", Status = "Done", Priority = 3, Description = "Train users on new features", Assignee = "Alice" },
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

## Drag and Drop

Enable drag-and-drop functionality by providing a `HandleMove` handler. Users can drag cards between columns to update their status:

```csharp demo-tabs
public class KanbanWithMoveExample : ViewBase
{
    record Task(string Id, string Title, string Status, int Priority, string Description, string Assignee);
    
    public override object? Build()
    {
        var taskState = UseState(new[]
        {
            new Task("1", "Design Homepage", "Todo", 1, "Create wireframes and mockups", "Alice"),
            new Task("2", "Setup Database", "Todo", 2, "Configure PostgreSQL instance", "Bob"),
            new Task("3", "Implement Auth", "Todo", 3, "Add OAuth2 authentication", "Charlie"),
            new Task("4", "Build API", "Todo", 4, "Create REST endpoints", "Alice"),
            new Task("5", "Code Review", "In Progress", 1, "Review pull requests", "Charlie"),
            new Task("6", "Performance Optimization", "In Progress", 2, "Optimize database queries", "Alice"),
            new Task("7", "Bug Fixes", "In Progress", 3, "Fix reported bugs", "Bob"),
            new Task("8", "Unit Tests", "Done", 1, "Write comprehensive test suite", "Bob"),
            new Task("9", "Deploy to Production", "Done", 2, "Configure CI/CD pipeline", "Charlie"),
            new Task("10", "User Training", "Done", 3, "Train users on new features", "Alice"),
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
        "Todo" => "To Do",
        "In Progress" => "In Progress",
        "Done" => "Completed",
        _ => status
    };
    
    public override object? Build()
    {
        var taskState = UseState(new[]
        {
            new Task("1", "Design Homepage", "Todo", 1, "Create wireframes and mockups", "Alice", GetColumnOrder("Todo")),
            new Task("2", "Setup Database", "Todo", 2, "Configure PostgreSQL instance", "Bob", GetColumnOrder("Todo")),
            new Task("3", "Implement Auth", "Todo", 3, "Add OAuth2 authentication", "Charlie", GetColumnOrder("Todo")),
            new Task("4", "Build API", "Todo", 4, "Create REST endpoints", "Alice", GetColumnOrder("Todo")),
            new Task("5", "Code Review", "In Progress", 1, "Review pull requests", "Charlie", GetColumnOrder("In Progress")),
            new Task("6", "Performance Optimization", "In Progress", 2, "Optimize database queries", "Alice", GetColumnOrder("In Progress")),
            new Task("7", "Bug Fixes", "In Progress", 3, "Fix reported bugs", "Bob", GetColumnOrder("In Progress")),
            new Task("8", "Unit Tests", "Done", 1, "Write comprehensive test suite", "Bob", GetColumnOrder("Done")),
            new Task("9", "Deploy to Production", "Done", 2, "Configure CI/CD pipeline", "Charlie", GetColumnOrder("Done")),
            new Task("10", "User Training", "Done", 3, "Train users on new features", "Alice", GetColumnOrder("Done")),
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
        var issueState = UseState(new[]
        {
            new Issue("1", "Bug in login page", "Open"),
            new Issue("2", "Add dark mode support", "Open"),
            new Issue("3", "Improve search functionality", "Open"),
            new Issue("4", "Update documentation", "Open"),
            new Issue("5", "Fix mobile responsive design", "In Progress"),
            new Issue("6", "Optimize image loading", "In Progress"),
            new Issue("7", "Add export feature", "In Progress"),
            new Issue("8", "Performance optimization completed", "Closed"),
            new Issue("9", "Security patch applied", "Closed"),
            new Issue("10", "Database migration successful", "Closed"),
        });
        
        return issueState.Value.ToKanban(
            groupBySelector: i => i.Status,
            idSelector: i => i.Id,
            titleSelector: i => i.Title,
            descriptionSelector: i => i.Id
        ).HandleMove(moveData =>
        {
            var issueId = moveData.CardId?.ToString();
            var updatedIssues = issueState.Value.ToList();
            var issueToMove = updatedIssues.FirstOrDefault(i => i.Id == issueId);
            
            if (issueToMove != null)
            {
                var updated = issueToMove with { Status = moveData.ToColumn };
                updatedIssues.RemoveAll(i => i.Id == issueId);
                updatedIssues.Add(updated);
                issueState.Set(updatedIssues.ToArray());
            }
        });
    }
}
```

</Body>
</Details>

<WidgetDocs Type="Ivy.Kanban" ExtensionTypes="Ivy.KanbanColumnExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Kanban/Kanban.cs"/>
