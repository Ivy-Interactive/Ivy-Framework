namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.DatabaseZap, group: ["Widgets"], searchHints: ["datatable", "table", "grid", "rows", "columns", "footer", "aggregation", "million", "performance"])]
public class DataTableApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Tabs(
            new Tab("Overview", new DataTableMainSample()),
            new Tab("Header Slots", new DataTableHeaderSlotsSample()),
            new Tab("Footer", new DataTableFooterSample()),
            new Tab("Multi Agg", new DataTableMultiAggSample()),
            new Tab("Million Rows", new DataTablesMillionRowsSample())
        ).Variant(TabsVariant.Content);
    }
}

public class DataTableMainSample : ViewBase
{
    private enum RowAction { Edit, Delete, View, Menu, Archive, Export, Share }

    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var mockService = UseService<MockEmployeeService>();

        var editModalOpen = UseState(() => false);
        var editingEmployee = UseState<EmployeeRecord?>(() => null);
        var refreshToken = UseRefreshToken();

        var dataTable = mockService.GetEmployees().AsQueryable().ToDataTable(idSelector: e => e.Id)
            .RefreshToken(refreshToken)
            .Width(Size.Full())
            .Height(Size.Full())

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

            .AlignContent(e => e.Id, Align.Left)
            .AlignContent(e => e.Age, Align.Left)
            .AlignContent(e => e.Salary, Align.Left)
            .AlignContent(e => e.Performance, Align.Left)
            .AlignContent(e => e.Name, Align.Left)
            .AlignContent(e => e.Email, Align.Left)
            .AlignContent(e => e.Notes, Align.Left)
            .AlignContent(e => e.IsActive, Align.Left)
            .AlignContent(e => e.IsManager, Align.Left)
            .AlignContent(e => e.HireDate, Align.Left)
            .AlignContent(e => e.LastReview, Align.Left)
            .AlignContent(e => e.Status, Align.Left)
            .AlignContent(e => e.Priority, Align.Left)
            .AlignContent(e => e.Department, Align.Left)
            .AlignContent(e => e.OptionalId, Align.Left)
            .AlignContent(e => e.Skills, Align.Left)
            .AlignContent(e => e.WidgetLink, Align.Left)
            .AlignContent(e => e.ProfileLink, Align.Left)

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
            .Badges(e => e.Skills, Colors.Sky)
            .Group(e => e.WidgetLink, "Links")
            .Group(e => e.ProfileLink, "Links")

            .Renderer(e => e.WidgetLink, new LinkDisplayRenderer { Type = LinkDisplayType.Url })
            .Renderer(e => e.ProfileLink, new LinkDisplayRenderer { Type = LinkDisplayType.Url })

            .Sortable(e => e.Email, false)
            .Sortable(e => e.Notes, false)

            .Config(config =>
            {
                config.FreezeColumns = 2;
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
                config.ShowColumnTypeIcons = false;
                config.BatchSize = 50;
                config.LoadAllRows = false;
                config.ShowSearch = true;
            })
            .RowActions(
                MenuItem.Default(Icons.Pencil).Tag(RowAction.Edit).Tooltip("Edit employee").Primary(),
                MenuItem.Default(Icons.Trash2).Tag(RowAction.Delete).Tooltip("Delete employee").Destructive(),
                MenuItem.Default(Icons.Eye).Tag(RowAction.View).Tooltip("View details").Color(Colors.Violet),
                MenuItem.Default(Icons.EllipsisVertical).Tag(RowAction.Menu).Tooltip("More actions")
                    .Children([
                        MenuItem.Default(Icons.Archive).Tag(RowAction.Archive).Label("Archive").Warning(),
                        MenuItem.Default(Icons.Download).Tag(RowAction.Export).Label("Export").Color(Colors.Cyan),
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

        var content = Layout.Vertical().Width(Size.Full()).Height(Size.Full())
            | "This header demonstrates that the DataTable below correctly calculates its height even when placed inside a vertical layout with other elements."
            | dataTable;

        return new Fragment([content, new EmployeeEditDialog(editModalOpen, editingEmployee, refreshToken, updated =>
        {
            mockService.UpdateEmployee(updated);
        })]);
    }
}

public class DataTableHeaderSlotsSample : ViewBase
{
    public override object? Build()
    {
        var products = new[]
        {
            new { Id = 1, Product = "Widget A", Price = 29.99m, Stock = 150 },
            new { Id = 2, Product = "Widget B", Price = 49.99m, Stock = 85 },
            new { Id = 3, Product = "Widget C", Price = 19.99m, Stock = 320 },
            new { Id = 4, Product = "Gadget X", Price = 99.99m, Stock = 42 },
            new { Id = 5, Product = "Gadget Y", Price = 149.99m, Stock = 18 },
        }.AsQueryable();

        return products.ToDataTable()
            .HeaderLeft(ctx => Layout.Horizontal().Gap(2)
                | new Button("Export", icon: Icons.Download).Small()
                | new Badge("Live").Color(Colors.Blue).Small())
            .HeaderRight(ctx => Layout.Horizontal().Gap(2)
                | new Badge($"{products.Count()} items").Color(Colors.Green).Small()
                | new Button("Settings", icon: Icons.Settings).Primary().Small())
            .Height(Size.Units(80));
    }
}

public class DataTableFooterSample : ViewBase
{
    public override object? Build()
    {
        var invoiceLines = new[]
        {
            new { Product = "Pro License", Qty = 15, UnitPrice = 99.00m, Amount = 1485.00m },
            new { Product = "Support Hours", Qty = 40, UnitPrice = 125.00m, Amount = 5000.00m },
            new { Product = "Training Session", Qty = 3, UnitPrice = 500.00m, Amount = 1500.00m },
            new { Product = "Custom Development", Qty = 80, UnitPrice = 150.00m, Amount = 12000.00m },
            new { Product = "Hosting (Annual)", Qty = 1, UnitPrice = 1200.00m, Amount = 1200.00m }
        }.AsQueryable();

        return invoiceLines.ToDataTable()
            .Header(x => x.Product, "Product / Service")
            .Header(x => x.Qty, "Quantity")
                .Footer(x => x.Qty, "Total", values => values.Sum())
            .Header(x => x.UnitPrice, "Unit Price")
                .Footer(x => x.UnitPrice, "Avg", values => values.Average())
            .Header(x => x.Amount, "Amount")
                .Footer(x => x.Amount, "Total", values => values.Sum())
            .Width(x => x.Product, Size.Px(200))
            .Width(x => x.Qty, Size.Units(30))
            .Width(x => x.UnitPrice, Size.Units(30))
            .Width(x => x.Amount, Size.Units(30))
            .AlignContent(x => x.Qty, Align.Right)
            .AlignContent(x => x.UnitPrice, Align.Right)
            .AlignContent(x => x.Amount, Align.Right)
            .Height(Size.Units(80));
    }
}

public class DataTableMultiAggSample : ViewBase
{
    public override object? Build()
    {
        var salesData = new[]
        {
            new { Region = "North", Sales = 45000m, Target = 50000m },
            new { Region = "South", Sales = 62000m, Target = 60000m },
            new { Region = "East", Sales = 38000m, Target = 45000m },
            new { Region = "West", Sales = 71000m, Target = 70000m }
        }.AsQueryable();

        return Layout.Vertical()
            | Text.P(
                "Sales and Target each define two footer aggregates (Total and Avg). The grid shows one at a time; click the footer or the chevron to switch.")
                .Muted()
            | salesData.ToDataTable()
            .Header(x => x.Region, "Sales Region")
            .Header(x => x.Sales, "Actual Sales")
                .Footer(x => x.Sales, new[]
                {
                    ("Total", (Func<IEnumerable<decimal>, object>)(values => values.Sum())),
                    ("Avg", (Func<IEnumerable<decimal>, object>)(values => values.Average()))
                })
            .Header(x => x.Target, "Target")
                .Footer(x => x.Target, new[]
                {
                    ("Total", (Func<IEnumerable<decimal>, object>)(values => values.Sum())),
                    ("Avg", (Func<IEnumerable<decimal>, object>)(values => values.Average()))
                })
            .AlignContent(x => x.Sales, Align.Right)
            .AlignContent(x => x.Target, Align.Right)
            .Height(Size.Units(60));
    }
}

public record MillionRowData(
    int Id,
    string Value,
    DateTime CreatedAt
);

public class DataTablesMillionRowsSample : ViewBase
{
    public override object? Build()
    {
        var millionRows = Enumerable.Range(1, 1_000_000)
            .Select(i => new MillionRowData(
                Id: i,
                Value: $"Row {i:N0}",
                CreatedAt: DateTime.Now.AddSeconds(-i)
            )).AsQueryable();

        return millionRows.ToDataTable()
            .Header(row => row.Id, "ID")
            .Header(row => row.Value, "Value")
            .Header(row => row.CreatedAt, "Created At")
            .Width(row => row.Id, Size.Px(100))
            .Width(row => row.Value, Size.Px(200))
            .Width(row => row.CreatedAt, Size.Px(200))
            .AlignContent(row => row.Id, Align.Left)
            .AlignContent(row => row.Value, Align.Left)
            .AlignContent(row => row.CreatedAt, Align.Left)
            .Icon(row => row.Id, Icons.Hash.ToString())
            .Icon(row => row.Value, Icons.FileText.ToString())
            .Icon(row => row.CreatedAt, Icons.Calendar.ToString())
            .Config(config => config.AllowLlmFiltering = true)
            .LoadAllRows(true);
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

            refreshToken.Refresh();

            return Task.CompletedTask;
        }
    }
}
