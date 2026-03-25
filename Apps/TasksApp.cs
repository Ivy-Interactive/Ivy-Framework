using Ivy;
using Ivy.Tendril.Apps.Tasks;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

[App(title: "Tasks", icon: Icons.Activity, group: new[] { "Tools" })]
public class TasksApp : ViewBase
{
    public override object? Build()
    {
        var taskService = UseService<TaskService>();
        var refreshToken = UseRefreshToken();
        var selectedTaskId = UseState<string?>(null);
        var dialogMode = UseState<string?>(null);

        UseInterval(() => refreshToken.Refresh(), TimeSpan.FromSeconds(5));

        var tasks = taskService.GetTasks();
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
                new MenuItem(Label: "View Output", Icon: Icons.Terminal, Tag: "view-output"),
                new MenuItem(Label: "View Plan", Icon: Icons.FileText, Tag: "view-plan")
            )
            .OnRowAction(e =>
            {
                var tag = e.Value.Tag?.ToString();
                var id = e.Value.Id?.ToString();
                var task = tasks.FirstOrDefault(t => t.Id == id);
                if (task != null)
                {
                    selectedTaskId.Set(task.Id);
                    dialogMode.Set(tag);
                }
                return ValueTask.CompletedTask;
            });

        var header = Layout.Horizontal()
            | Text.Block("Background Tasks").Bold()
            | new Button("Refresh").Icon(Icons.RefreshCw).Ghost()
                .OnClick(() => refreshToken.Refresh());

        var elements = new List<object>
        {
            new HeaderLayout(
                header: header,
                content: dataTable.Height(Size.Full())
            ).Height(Size.Full())
        };

        if (dialogMode.Value != null && selectedTaskId.Value is { } taskId)
        {
            var task = taskService.GetTask(taskId);
            if (task != null)
            {
                void CloseDialog()
                {
                    dialogMode.Set(null);
                    selectedTaskId.Set(null);
                }

                if (dialogMode.Value == "view-output" && !string.IsNullOrEmpty(task.ScriptPath))
                {
                    elements.Add(new Dialog(
                        _ => { CloseDialog(); return ValueTask.CompletedTask; },
                        new DialogHeader($"Task {task.Id} - {task.Type}"),
                        new DialogBody(
                            new TaskDetailView(task, taskService)
                        ),
                        new DialogFooter(
                            new Button("Close").Outline().OnClick(CloseDialog)
                        )
                    ).Width(Size.Rem(60)).Height(Size.Rem(40)));
                }
                else
                {
                    var planPath = Path.Combine(@"D:\Repos\_Ivy\.plans", task.PlanFile);
                    var planContent = File.Exists(planPath)
                        ? File.ReadAllText(planPath)
                        : $"Plan file not found: {task.PlanFile}";

                    elements.Add(new Dialog(
                        _ => { CloseDialog(); return ValueTask.CompletedTask; },
                        new DialogHeader(task.PlanFile),
                        new DialogBody(
                            Text.Code(planContent)
                        ),
                        new DialogFooter(
                            new Button("Close").Outline().OnClick(CloseDialog)
                        )
                    ).Width(Size.Rem(50)));
                }
            }
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
