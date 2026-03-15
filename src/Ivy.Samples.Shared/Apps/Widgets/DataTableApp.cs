using Ivy.Samples.Shared.Apps;

namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.DatabaseZap)]
public class DataTableApp : SampleBase
{
    private enum RowAction { Edit, Delete, View, Menu, Archive, Export, Share }

    protected override object? BuildSample()
    {
        var client = UseService<IClientProvider>();
        var mockService = UseService<MockEmployeeService>();

        // The DataTable builder will be recreated each time, but use the cached employee data
        var editModalOpen = UseState(() => false);
        var editingEmployee = UseState<EmployeeRecord?>(() => null);
        var refreshToken = UseRefreshToken();

        // Configuration and row actions logic
        var dataTable = mockService.GetEmployees().AsQueryable().ToDataTable(idSelector: e => e.Id)
            .RefreshToken(refreshToken)
            // Table dimensions (fix for issue #1311)
            .Width(Size.Full()) // Table width set to 120 units (30rem)
            .Height(Size.Full()) // Table height set to 120 units (30rem)

            // Column titles
            .Header(e => e.Id, "ID")
            .Header(e => e.Age, "Age")
            .Header(e => e.Salary, "Salary")
            .Header(e => e.Performance, "Performance")
            .Header(e => e.OptionalId, "Badge #")
            .Header(e => e.EmployeeCode, "Code")
            .Header(e => e.Name, "Name")
            .Header(e => e.Email, "Email")
            .Header(e => e.Notes, "Notes")
            .Header(e => e.IsActive, "Active")
            .Header(e => e.IsManager, "Manager")
            .Header(e => e.HireDate, "Hire Date")
            .Header(e => e.LastReview, "Last Review")
            .Header(e => e.Status, "Status")
            .Header(e => e.Priority, "Priority")
            .Header(e => e.Department, "Dept")
            .Header(e => e.Skills, "Skills")
            .Header(e => e.WidgetLink, "Widgets")
            .Header(e => e.ProfileLink, "Profiles")

            // Column widths
            .Width(e => e.Id, Size.Px(40))
            .Width(e => e.EmployeeCode, Size.Px(100))
            .Width(e => e.Name, Size.Px(120))
            .Width(e => e.Email, Size.Px(250))
            .Width(e => e.Age, Size.Px(70))
            .Width(e => e.Salary, Size.Px(120))
            .Width(e => e.Performance, Size.Px(110))
            .Width(e => e.IsActive, Size.Px(80))
            .Width(e => e.IsManager, Size.Px(90))
            .Width(e => e.HireDate, Size.Px(120))
            .Width(e => e.LastReview, Size.Px(140))
            .Width(e => e.Status, Size.Px(90))
            .Width(e => e.Priority, Size.Px(90))
            .Width(e => e.Department, Size.Px(90))
            .Width(e => e.Notes, Size.Px(150))
            .Width(e => e.OptionalId, Size.Px(100))
            .Width(e => e.Skills, Size.Px(300))
            .Width(e => e.WidgetLink, Size.Px(200))
            .Width(e => e.ProfileLink, Size.Px(250))

            // Alignments
            .Align(e => e.Id, Align.Left)
            .Align(e => e.Age, Align.Left)
            .Align(e => e.Salary, Align.Left)
            .Align(e => e.Performance, Align.Left)
            .Align(e => e.Name, Align.Left)
            .Align(e => e.Email, Align.Left)
            .Align(e => e.Notes, Align.Left)
            .Align(e => e.IsActive, Align.Left)
            .Align(e => e.IsManager, Align.Left)
            .Align(e => e.HireDate, Align.Left)
            .Align(e => e.LastReview, Align.Left)
            .Align(e => e.Status, Align.Left)
            .Align(e => e.Priority, Align.Left)
            .Align(e => e.Department, Align.Left)
            .Align(e => e.OptionalId, Align.Left)
            .Align(e => e.Skills, Align.Left)
            .Align(e => e.WidgetLink, Align.Left)
            .Align(e => e.ProfileLink, Align.Left)

            // Groups
            .Group(e => e.Id, "Identity")
            .Group(e => e.EmployeeCode, "Identity")
            .Group(e => e.Name, "Personal")
            .Group(e => e.Email, "Personal")
            .Group(e => e.Age, "Personal")
            .Group(e => e.Salary, "Compensation")
            .Group(e => e.Performance, "Compensation")
            .Group(e => e.IsActive, "Status")
            .Group(e => e.IsManager, "Status")
            .Group(e => e.Status, "Status")
            .Group(e => e.Priority, "Status")
            .Group(e => e.Department, "Status")
            .Group(e => e.HireDate, "Timeline")
            .Group(e => e.LastReview, "Timeline")
            .Group(e => e.Notes, "Other")
            .Group(e => e.OptionalId, "Other")
            .Group(e => e.Skills, "Personal")
            .Group(e => e.WidgetLink, "Links")
            .Group(e => e.ProfileLink, "Links")

            // Column renderers - LinkDisplayRenderer automatically sets ColType.Link
            .Renderer(e => e.WidgetLink, new LinkDisplayRenderer { Type = LinkDisplayType.Url })
            .Renderer(e => e.ProfileLink, new LinkDisplayRenderer { Type = LinkDisplayType.Url })

            // Sorting
            .Sortable(e => e.Email, false) // Email not sortable
            .Sortable(e => e.Notes, false) // Notes not sortable

            // Configuration
            .Config(config =>
            {
                config.FreezeColumns = 2; // Freeze ID and Code
                config.AllowSorting = true;
                config.AllowFiltering = true;
                config.AllowLlmFiltering = true;
                config.AllowColumnReordering = true;
                config.AllowColumnResizing = true;
                config.AllowCopySelection = true;
                config.SelectionMode = SelectionModes.Columns;
                config.ShowIndexColumn = false;
                config.ShowGroups = true;
                config.ShowVerticalBorders = true;
                config.ShowColumnTypeIcons = false; // Show type icons
                config.BatchSize = 50; // Load 50 rows at a time
                config.LoadAllRows = false; // Use pagination
                config.ShowSearch = true;
            })
            // Row actions: fluent API + enum (compile-time safe); .Tag() overwrites default tag
            .RowActions(
                MenuItem.Default(Icons.Pencil).Tag(RowAction.Edit),
                MenuItem.Default(Icons.Trash2).Tag(RowAction.Delete),
                MenuItem.Default(Icons.Eye).Tag(RowAction.View),
                MenuItem.Default(Icons.EllipsisVertical).Tag(RowAction.Menu)
                    .Children([
                        MenuItem.Default(Icons.Archive).Tag(RowAction.Archive).Label("Archive"),
                        MenuItem.Default(Icons.Download).Tag(RowAction.Export).Label("Export"),
                        MenuItem.Default(Icons.Share2).Tag(RowAction.Share).Label("Share")
                    ])
            )
            .OnRowAction(e =>
            {
                var args = e.Value;
                if (!Enum.TryParse<RowAction>(args.Tag?.ToString(), ignoreCase: true, out var action)) return ValueTask.CompletedTask;
                if (!int.TryParse(args.Id?.ToString() ?? "", out int employeeId)) return ValueTask.CompletedTask;

                switch (action)
                {
                    case RowAction.Edit:
                        var toEdit = mockService.GetEmployees().FirstOrDefault(emp => emp.Id == employeeId);
                        if (toEdit != null) { editingEmployee.Set(toEdit); editModalOpen.Set(true); }
                        break;
                    case RowAction.Delete:
                        var toDelete = mockService.GetEmployees().FirstOrDefault(emp => emp.Id == employeeId);
                        if (toDelete != null)
                        {
                            mockService.DeleteEmployee(toDelete.Id);
                            refreshToken.Refresh();
                            client.Toast($"Employee {toDelete.Name} deleted");
                        }
                        break;
                    default:
                        client.Toast($"Row action: {action} on row ID: {args.Id}");
                        break;
                }
                return ValueTask.CompletedTask;
            });

        return new Fragment([dataTable, new EmployeeEditDialog(editModalOpen, editingEmployee, refreshToken, updated =>
        {
            mockService.UpdateEmployee(updated);
        })]);
    }
}

public class EmployeeEditDialog(IState<bool> isOpen, IState<EmployeeRecord?> employeeState, RefreshToken refreshToken, Action<EmployeeRecord> onSave) : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();

        if (employeeState.Value == null)
        {
            return new Empty();
        }

        // We bind the form to the employee state. ToForm() will handle the object mutations and submission.
        return employeeState.Value
            .ToForm()
            .Remove(e => e.Id)
            .Remove(e => e.EmployeeCode)
            .Remove(e => e.HireDate)
            .OnSubmit(OnSubmit)
            .ToDialog(isOpen, title: "Edit Employee", submitTitle: "Save");

        Task OnSubmit(EmployeeRecord? updated)
        {
            if (updated != null)
            {
                onSave(updated);
                client.Toast($"Employee {updated.Name} saved successfully");
            }

            isOpen.Set((bool)false);
            employeeState.Set((EmployeeRecord?)null);

            // Trigger refresh
            refreshToken.Refresh();

            return Task.CompletedTask;
        }
    }
}
