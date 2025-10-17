---
prepare: |
    var tasks = new[]
    {
        new { Id = "1", Title = "Design Homepage", Status = "Todo", StatusOrder = 1, Priority = 2, Description = "Create wireframes and mockups", Assignee = "Alice" },
        new { Id = "2", Title = "Setup Database", Status = "Todo", StatusOrder = 1, Priority = 1, Description = "Configure PostgreSQL instance", Assignee = "Bob" },
        new { Id = "3", Title = "Implement Auth", Status = "In Progress", StatusOrder = 2, Priority = 1, Description = "Add OAuth2 authentication", Assignee = "Charlie" },
        new { Id = "4", Title = "Build API", Status = "In Progress", StatusOrder = 2, Priority = 2, Description = "Create REST endpoints", Assignee = "Alice" },
        new { Id = "5", Title = "Unit Tests", Status = "Done", StatusOrder = 3, Priority = 2, Description = "Write comprehensive test suite", Assignee = "Bob" },
        new { Id = "6", Title = "Deploy to Production", Status = "Done", StatusOrder = 3, Priority = 1, Description = "Configure CI/CD pipeline", Assignee = "Charlie" },
    };
searchHints:
  - board
  - project management  
  - drag drop
  - columns
  - cards
  - workflow
---

# Kanban

<Ingress>
Display and manage tasks using a Kanban board format with columns and draggable cards, perfect for project management and workflow visualization.
</Ingress>

The Kanban widget creates interactive project boards with columns representing different states and draggable cards for individual tasks or items.

## Basic Usage

Create a Kanban board using the fluent API to transform your data into organized columns:

```csharp demo-below
tasks
    .ToKanban(
        groupBySelector: e => e.Status,
        cardIdSelector: e => e.Id,
        cardTitleSelector: e => e.Title,
        cardDescriptionSelector: e => e.Description)
    .ColumnOrder(e => e.StatusOrder)
    .CardOrder(e => e.Priority)
    .ColumnTitle(status => status switch
    {
        "Todo" => "📋 To Do",
        "In Progress" => "🚀 In Progress", 
        "Done" => "✅ Done",
        _ => status
    })
```

## With Event Handlers

Add interactivity with event handlers for adding, moving, and deleting cards:

```csharp demo-below
tasks
    .ToKanban(
        groupBySelector: e => e.Status,
        cardIdSelector: e => e.Id,
        cardTitleSelector: e => e.Title,
        cardDescriptionSelector: e => e.Description)
    .ColumnOrder(e => e.StatusOrder)
    .CardOrder(e => e.Priority)
    .ColumnTitle(status => status switch
    {
        "Todo" => "📋 To Do",
        "In Progress" => "🚀 In Progress",
        "Done" => "✅ Done",
        _ => status
    })
    .HandleAdd((string columnKey) => 
        Console.WriteLine($"Card added to column: {columnKey}"))
    .HandleMove(moveData => 
        Console.WriteLine($"Card {moveData.CardId} moved from {moveData.FromColumn} to {moveData.ToColumn}"))
    .HandleDelete(cardId => 
        Console.WriteLine($"Card deleted: {cardId}"))
```

## Custom Card Rendering

Use custom card builders for more complex card layouts:

```csharp demo-below
tasks
    .ToKanban(
        groupBySelector: e => e.Status,
        cardIdSelector: e => e.Id)
    .CardBuilder(task => 
        new Card()
            .Title(task.Title)
            .Description(task.Description)
            .Footer(Layout.Horizontal(
                new Badge { Text = task.Assignee },
                new Badge { Text = $"Priority {task.Priority}" }
            )))
    .ColumnTitle(status => status switch
    {
        "Todo" => "📋 To Do",
        "In Progress" => "🚀 In Progress",
        "Done" => "✅ Done",
        _ => status
    })
```

## Empty State

Show a helpful message when the board has no tasks:

```csharp demo-below
new List<object>()
    .ToKanban(
        groupBySelector: e => "Empty",
        cardIdSelector: e => "empty")
    .Empty(
        new Card()
            .Title("No Tasks")
            .Description("Create your first task to get started")
    )
```

## Direct Widget Construction

For more control, you can construct Kanban widgets directly:

```csharp demo-below
new Kanban(
    new KanbanColumn(
        new KanbanCard(
            new Card()
                .Title("Task 1")
                .Description("First task")
        ) { CardId = "1" },
        new KanbanCard(
            new Card()
                .Title("Task 2") 
                .Description("Second task")
        ) { CardId = "2" }
    ).Title("📋 To Do").ColumnKey("todo"),
    
    new KanbanColumn(
        new KanbanCard(
            new Card()
                .Title("Task 3")
                .Description("In progress task")
        ) { CardId = "3" }
    ).Title("🚀 In Progress").ColumnKey("progress")
)
```

## API Reference

### Kanban Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| ShowCounts | bool | true | Whether to show card counts in column headers |
| AllowAdd | bool | false | Whether to allow adding cards (automatically set when HandleAdd is configured) |
| AllowMove | bool | false | Whether to allow moving cards between columns (automatically set when HandleMove is configured) |
| AllowDelete | bool | false | Whether to allow deleting cards (automatically set when HandleDelete is configured) |

### Kanban Events

| Event | Parameters | Description |
|-------|------------|-------------|
| OnDelete | object? CardId | Fired when a card is deleted |
| OnMove | (object? CardId, object? FromColumn, object? ToColumn) | Fired when a card is moved between columns |

### KanbanColumn Properties

| Property | Type | Description |
|----------|------|-------------|
| Title | string? | Column title displayed in the header |
| ColumnKey | object? | Unique identifier for the column used in events |

### KanbanColumn Events

| Event | Parameters | Description |
|-------|------------|-------------|
| OnAdd | object? ColumnKey | Fired when a card is added to this column |

### KanbanCard Properties

| Property | Type | Description |
|----------|------|-------------|
| CardId | object? | Unique identifier for the card used in events |

### KanbanBuilder Methods

| Method | Description |
|--------|-------------|
| Builder() | Set a custom builder for rendering card content |
| CardBuilder() | Set a custom card renderer function |
| ColumnTitle() | Set custom formatter for column titles |
| ColumnOrder() | Set the order of columns by sorting field |
| CardOrder() | Set the order of cards within columns by sorting field |
| HandleAdd() | Set event handler for adding cards |
| HandleMove() | Set event handler for moving cards |
| HandleDelete() | Set event handler for deleting cards |
| Empty() | Set content to display when board is empty |

## Best Practices

1. **Use meaningful column keys** - They're passed to event handlers for identifying columns
2. **Provide card IDs** - Essential for move and delete operations
3. **Order your data** - Use ColumnOrder() and CardOrder() for consistent layouts
4. **Handle events** - Implement add/move/delete handlers to make boards interactive
5. **Show empty states** - Use Empty() to guide users when boards are empty
6. **Custom rendering** - Use CardBuilder() for rich card layouts with additional metadata