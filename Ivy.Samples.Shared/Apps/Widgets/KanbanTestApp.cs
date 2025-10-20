using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.Kanban, path: ["Widgets"], searchHints: ["kanban test"])]
public class KanbanTestApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical(
            Text.H3("Simple Kanban Test"),
            Text.P("Testing frontend Kanban widget integration"),

            new Kanban(
                new KanbanColumn(
                    new KanbanCard(
                        new Card()
                            .Title("Task 1")
                            .Description("First task")
                    )
                    { CardId = "1" },
                    new KanbanCard(
                        new Card()
                            .Title("Task 2")
                            .Description("Second task")
                    )
                    { CardId = "2" }
                )
                { Title = "To Do", ColumnKey = "todo" },

                new KanbanColumn(
                    new KanbanCard(
                        new Card()
                            .Title("Task 3")
                            .Description("In progress task")
                    )
                    { CardId = "3" }
                )
                { Title = "In Progress", ColumnKey = "inprogress" },

                new KanbanColumn() { Title = "Done", ColumnKey = "done" }
            )
            {
                ShowCounts = true,
                AllowMove = true,
                AllowAdd = true,
                AllowDelete = true,
                OnMove = e => { Console.WriteLine($"Moved card {e.Value.CardId} from {e.Value.FromColumn} to {e.Value.ToColumn}"); return ValueTask.CompletedTask; },
                OnDelete = e => { Console.WriteLine($"Deleted card {e.Value}"); return ValueTask.CompletedTask; }
            }
        );
    }
}
