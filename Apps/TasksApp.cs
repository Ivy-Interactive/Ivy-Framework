using Ivy;
using Ivy.Tendril.Apps.Tasks;

namespace Ivy.Tendril.Apps;

[App(title: "Tasks", icon: Icons.Activity, group: new[] { "Tools" })]
public class TasksApp : ViewBase
{
    public override object? Build()
    {
        var mockService = UseService<MockTaskDataService>();
        var refreshToken = UseRefreshToken();
        var selectedTask = UseState<TaskItem?>(null);
        var dialogOpen = UseState(false);

        UseInterval(() => refreshToken.Refresh(), TimeSpan.FromSeconds(5));

        var tasks = mockService.GetTasks();
        var rows = tasks.Select(t => new TaskItemRow
        {
            Id = t.Id,
            Status = t.Status,
            Plan = t.PlanFile,
            Type = t.Type,
            Queue = t.Queue,
            Timer = FormatTimer(t)
        }).ToList();

        var dataTable = rows.AsQueryable()
            .ToDataTable(idSelector: t => t.Id)
            .RefreshToken(refreshToken)
            .Width(Size.Full())
            .Height(Size.Full())
            .Header(t => t.Status, "Status")
            .Header(t => t.Plan, "Plan")
            .Header(t => t.Type, "Type")
            .Header(t => t.Queue, "Queue")
            .Header(t => t.Timer, "Timer")
            .Renderer(t => t.Status, new LabelsDisplayRenderer())
            .Hidden(t => t.Id)
            .Config(c =>
            {
                c.AllowSorting = true;
                c.AllowFiltering = true;
                c.ShowSearch = true;
                c.SelectionMode = SelectionModes.Rows;
                c.ShowIndexColumn = false;
                c.BatchSize = 50;
            })
            .RowActions(
                new MenuItem(Label: "View Plan", Icon: Icons.FileText, Tag: "view-plan")
            )
            .OnRowAction(e =>
            {
                var tag = e.Value.Tag?.ToString();
                var id = e.Value.Id?.ToString();
                if (tag == "view-plan")
                {
                    var task = tasks.FirstOrDefault(t => t.Id == id);
                    if (task != null)
                    {
                        selectedTask.Set(task);
                        dialogOpen.Set(true);
                    }
                }
                return ValueTask.CompletedTask;
            });

        var header = Layout.Horizontal()
            .Align(Align.Left)
            .Gap(2)
            | Text.Block("Background Tasks").Bold()
            | new Spacer()
            | new Button("Refresh").Icon(Icons.RefreshCw).Ghost()
                .OnClick(() => refreshToken.Refresh());

        var elements = new List<object>
        {
            new HeaderLayout(
                header: header,
                content: dataTable
            ).Height(Size.Full())
        };

        if (dialogOpen.Value && selectedTask.Value != null)
        {
            var plan = selectedTask.Value;
            var planPath = Path.Combine(@"D:\Repos\_Ivy\.plans", plan.PlanFile);
            var planContent = File.Exists(planPath)
                ? File.ReadAllText(planPath)
                : $"Plan file not found: {plan.PlanFile}";

            elements.Add(new Dialog(
                _ => { dialogOpen.Set(false); return ValueTask.CompletedTask; },
                new DialogHeader(plan.PlanFile),
                new DialogBody(
                    Text.Code(planContent)
                ),
                new DialogFooter(
                    new Button("Close").Outline().OnClick(() => dialogOpen.Set(false))
                )
            ).Width(Size.Rem(50)));
        }

        return new Fragment(elements.ToArray());
    }

    private static string FormatTimer(TaskItem task)
    {
        if (task.Status == "Running" && task.StartedAt.HasValue)
        {
            var elapsed = DateTime.UtcNow - task.StartedAt.Value;
            return FormatTimeSpan(elapsed);
        }

        if ((task.Status == "Completed" || task.Status == "Failed") && task.DurationSeconds.HasValue)
        {
            return FormatTimeSpan(TimeSpan.FromSeconds(task.DurationSeconds.Value));
        }

        return "-";
    }

    private static string FormatTimeSpan(TimeSpan span)
    {
        if (span.TotalHours >= 1)
            return $"{(int)span.TotalHours}h {span.Minutes:D2}m";
        return $"{span.Minutes}m {span.Seconds:D2}s";
    }
}
