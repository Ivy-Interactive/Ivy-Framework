using Ivy.Samples.Shared.Apps;
using Ivy.Shared;
using Ivy.Views.DataTables;
using Ivy.Hooks;
using Ivy.Views.Forms;

namespace Ivy.Samples.Shared.Apps.Widgets;

public class EmployeeRecord
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
    public decimal Salary { get; set; }
    public double Performance { get; set; }
    public bool IsActive { get; set; }
    public bool IsManager { get; set; }
    public DateTime HireDate { get; set; }
    public DateTime LastReview { get; set; }
    public Icons Status { get; set; }
    public Icons Priority { get; set; }
    public Icons Department { get; set; }
    public string Notes { get; set; }
    public int? OptionalId { get; set; }
    public string[] Skills { get; set; }
    public string? WidgetLink { get; set; }
    public string? ProfileLink { get; set; }
}

[App(icon: Icons.DatabaseZap)]
public class DataTableApp : SampleBase
{
    protected override object? BuildSample()
    {
        var client = UseService<IClientProvider>();

        // Create the employee data once at app level (like Kanban caches its tasks)
        var employees = UseState(() =>
        {
            var allSkills = new[] { "C#", "JavaScript", "Python", "SQL", "React", "Leadership", "Communication", "Problem Solving", "Team Player", "Agile" };

            var random = new Random(42);
            var startDate = new DateTime(2020, 1, 1);

            var departments = new[] { Icons.Building, Icons.Code, Icons.Users, Icons.ShoppingCart, Icons.Headphones };
            var statuses = new[] { Icons.CircleCheck, Icons.Clock, Icons.TriangleAlert, Icons.X, Icons.Pause };
            var priorities = new[] { Icons.ArrowUp, Icons.ArrowRight, Icons.ArrowDown, Icons.Flag, Icons.Star };

            var firstNames = new[] { "John", "Jane", "Mike", "Sarah", "David", "Emily", "Chris", "Lisa", "Tom", "Anna" };
            var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez" };

            return Enumerable.Range(1, 1000).Select(i =>
            {
                var firstName = firstNames[random.Next(firstNames.Length)];
                var lastName = lastNames[random.Next(lastNames.Length)];
                var name = $"{firstName} {lastName}";
                var email = $"employee{i}@company.com";
                var age = random.Next(22, 65);
                var salary = (decimal)(random.Next(30000, 150000) / 1000 * 1000);
                var performance = Math.Round(random.NextDouble() * 5, 2);
                var isActive = random.NextDouble() > 0.2;
                var isManager = random.NextDouble() > 0.8;
                var hireDate = startDate.AddDays(random.Next(0, 1826));
                var lastReview = DateTime.Now.AddDays(-random.Next(0, 365));
                var status = statuses[random.Next(statuses.Length)];
                var priority = priorities[random.Next(priorities.Length)];
                var department = departments[random.Next(departments.Length)];
                var notes = $"Employee notes for {i}";
                var optionalId = random.NextDouble() > 0.3 ? (int?)random.Next(1, 1000) : null;

                // Generate 2-5 random skills for each employee
                var skillCount = random.Next(2, 6);
                var skills = Enumerable.Range(0, skillCount)
                    .Select(_ => allSkills[random.Next(allSkills.Length)])
                    .Distinct()
                    .ToArray();

                // Generate link URLs
                var widgetLink = "/widgets/charts/area-chart"; // Internal widget link - relative URL works on any domain
                var profileLink = $"https://linkedin.com/in/{firstName.ToLower()}{lastName.ToLower()}{i}"; // External LinkedIn profile

                return new EmployeeRecord
                {
                    Id = i,
                    EmployeeCode = $"EMP{i:D4}",
                    Name = name,
                    Email = email,
                    Age = age,
                    Salary = salary,
                    Performance = performance,
                    IsActive = isActive,
                    IsManager = isManager,
                    HireDate = hireDate,
                    LastReview = lastReview,
                    Status = status,
                    Priority = priority,
                    Department = department,
                    Notes = notes,
                    OptionalId = optionalId,
                    Skills = skills,
                    WidgetLink = widgetLink,
                    ProfileLink = profileLink
                };
            }).ToList();
        });

        // The DataTable builder will be recreated each time, but use the cached employee data
        var editModalOpen = UseState(() => false);
        var editingEmployee = UseState<EmployeeRecord?>(() => null);
        var queryService = UseService<IQueryService>();

        // Configuration and row actions logic
        var dataTable = employees.Value.AsQueryable().ToDataTable(idSelector: e => e.Id)
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
            // Configure row action buttons
            .RowActions(
                MenuItem.Default(Icons.Pencil, "edit"),
                MenuItem.Default(Icons.Trash2, "delete"),
                MenuItem.Default(Icons.Eye, "view"),
                MenuItem.Default(Icons.EllipsisVertical, "menu")
                    .Children([
                        MenuItem.Default(Icons.Archive, "archive").Label("Archive"),
                        MenuItem.Default(Icons.Download, "export").Label("Export"),
                        MenuItem.Default(Icons.Share2, "share").Label("Share")
                    ])
            )
            .HandleRowAction(async e =>
            {
                var args = e.Value;
                if (args.Tag?.ToString() == "edit" && int.TryParse(args.Id?.ToString() ?? "", out int employeeId))
                {
                    var employee = employees.Value.FirstOrDefault(emp => emp.Id == employeeId);
                    if (employee != null)
                    {
                        editingEmployee.Set(employee);
                        editModalOpen.Set(true);
                    }
                }
                else
                {
                    client.Toast($"Row action: ID: {args.Id}, Tag: {args.Tag}");
                }
                await ValueTask.CompletedTask;
            });

        return new Fragment([dataTable, new EmployeeEditDialog(editModalOpen, editingEmployee)]);
    }
}

public class EmployeeEditDialog(IState<bool> isOpen, IState<EmployeeRecord?> employeeState) : ViewBase
{
    public override object? Build()
    {
        var queryService = UseService<IQueryService>();
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
            .HandleSubmit(OnSubmit)
            .ToDialog(isOpen, title: "Edit Employee", submitTitle: "Save");

        Task OnSubmit(EmployeeRecord? updated)
        {
            if (updated != null)
            {
                // In a real app we'd save to DB here. This is a mock demo, so the object mutations handled
                // by ToForm() on the reference object are already applied to the list item.
                client.Toast($"Employee {updated.Name} saved successfully");
            }

            isOpen.Set((bool)false);
            employeeState.Set((EmployeeRecord?)null);

            // Trigger refresh
            queryService.Invalidate(k => k is string s && s == nameof(EmployeeRecord));

            return Task.CompletedTask;
        }
    }
}
