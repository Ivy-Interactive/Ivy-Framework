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
        var tasks = new[]
        {
            new Task { Id = "1", Title = "Design Homepage", Status = "Todo", StatusOrder = 1, Priority = 2, Description = "Create wireframes and mockups", Assignee = "Alice" },
            new Task { Id = "2", Title = "Setup Database", Status = "Todo", StatusOrder = 1, Priority = 1, Description = "Configure PostgreSQL instance", Assignee = "Bob" },
            new Task { Id = "3", Title = "Implement Auth", Status = "In Progress", StatusOrder = 2, Priority = 1, Description = "Add OAuth2 authentication", Assignee = "Charlie" },
            new Task { Id = "4", Title = "Build API", Status = "In Progress", StatusOrder = 2, Priority = 2, Description = "Create REST endpoints", Assignee = "Alice" },
            new Task { Id = "5", Title = "Unit Tests", Status = "Done", StatusOrder = 3, Priority = 2, Description = "Write comprehensive test suite", Assignee = "Bob" },
            new Task { Id = "6", Title = "Deploy to Production", Status = "Done", StatusOrder = 3, Priority = 1, Description = "Configure CI/CD pipeline", Assignee = "Charlie" },
        };

        return Layout.Vertical(
            Text.H3("Task Board Demo"),
            Text.P("Showcasing kanban features: field selectors, column/card ordering, custom column titles, and event handlers."),

            // Kanban with common features
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
                .HandleAdd((string columnKey) => Console.WriteLine($"Card added to column: {columnKey}"))
                .HandleMove(moveData => Console.WriteLine($"Card {moveData.CardId} moved from {moveData.FromColumn} to {moveData.ToColumn}"))
                .HandleDelete(cardId => Console.WriteLine($"Card deleted: {cardId}"))
                .Empty(
                    new Card()
                        .Title("No Tasks")
                        .Description("Create your first task to get started")
                )
        );
    }
}
