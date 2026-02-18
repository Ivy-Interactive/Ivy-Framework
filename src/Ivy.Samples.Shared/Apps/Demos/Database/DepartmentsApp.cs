using System.Reactive.Linq;
using System.ComponentModel.DataAnnotations;
using Ivy.Samples.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Samples.Shared.Apps.Demos.Database;

[App(icon: Icons.Building, searchHints: ["crud", "management", "list", "details", "forms", "entity"])]
public class DepartmentsApp : SampleBase
{
    protected override object? BuildSample()
    {
        return UseBlades(() => new DepartmentsListBlade(), "Search", Size.Units(75));
    }
}

public record DepartmentListRecord(Guid Id, string Name);

public class DepartmentsListBlade : ViewBase
{
    public override object? Build()
    {
        var blades = UseContext<IBladeService>();
        var refreshToken = UseRefreshToken();

        var filter = UseState("");
        var throttledFilter = UseState("");

        UseEffect(() =>
        {
            throttledFilter.Set(filter.Value);
            blades.Pop(this);
        }, [filter.Throttle(TimeSpan.FromMilliseconds(250)).ToTrigger()]);

        var departmentsQuery = UseDepartmentListRecords(Context, throttledFilter.Value);

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is Guid departmentId)
            {
                blades.Pop(this);
                departmentsQuery.Mutator.Revalidate();
                blades.Push(this, new DepartmentDetailsBlade(departmentId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var department = (DepartmentListRecord)e.Sender.Tag!;
            blades.Push(this, new DepartmentDetailsBlade(department.Id), department.Name, width: Size.Units(100));
        });

        object CreateItem(DepartmentListRecord listRecord) => new FuncView(context =>
        {
            var itemQuery = UseDepartmentListRecord(context, listRecord);
            if (itemQuery.Loading || itemQuery.Value == null)
            {
                return new ListItem();
            }
            var record = itemQuery.Value;
            return new ListItem(title: record.Name, onClick: onItemClicked, tag: record);
        });

        var createBtn = Icons.Plus
            .ToButton(_ =>
            {
                blades.Pop(this);
            })
            .Ghost()
            .ToTrigger((isOpen) => new DepartmentCreateDialog(isOpen, refreshToken));

        var items = (departmentsQuery.Value ?? []).Select(CreateItem);

        var header = Layout.Horizontal().Gap(1)
                     | filter.ToSearchInput().Placeholder("Search").Width(Size.Grow())
                     | createBtn;

        return new Fragment()
               | new BladeHeader(header)
               | (departmentsQuery.Value == null ? Text.Muted("Loading...") : new List(items));
    }

    public static QueryResult<DepartmentListRecord[]> UseDepartmentListRecords(IViewContext context, string filter)
    {
        var factory = context.UseService<SampleDbContextFactory>();
        return context.UseQuery(
            key: (nameof(UseDepartmentListRecords), filter),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();

                var linq = db.Departments.AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    linq = linq.Where(e => e.Name.Contains(filter));
                }

                return await linq
                    .OrderBy(e => e.Name)
                    .Take(50)
                    .Select(e => new DepartmentListRecord(e.Id, e.Name))
                    .ToArrayAsync(ct);
            },
            tags: [typeof(DepartmentListRecord[])],
            options: new QueryOptions()
            {
                KeepPrevious = true
            }
        );
    }

    public static QueryResult<DepartmentListRecord?> UseDepartmentListRecord(IViewContext context, DepartmentListRecord record)
    {
        var factory = context.UseService<SampleDbContextFactory>();
        return context.UseQuery(
            key: (nameof(UseDepartmentListRecord), record.Id),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.Departments
                    .Where(e => e.Id == record.Id)
                    .Select(e => new DepartmentListRecord(e.Id, e.Name))
                    .FirstOrDefaultAsync(ct);
            },
            options: new QueryOptions { RevalidateOnMount = false },
            initialValue: record,
            tags: [(typeof(Department), record.Id)]
        );
    }
}

public class DepartmentDetailsBlade(Guid departmentId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<SampleDbContextFactory>();
        var blades = UseContext<IBladeService>();
        var queryService = UseService<IQueryService>();

        var departmentQuery = UseQuery(
            key: (nameof(DepartmentDetailsBlade), departmentId),
            fetcher: async (ct) =>
            {
                await using var db = factory.CreateDbContext();
                return await db.Departments
                    .SingleOrDefaultAsync(e => e.Id == departmentId, ct);
            },
            tags: [(typeof(Department), departmentId)]
        );

        if (departmentQuery.Loading) return Skeleton.Card();

        if (departmentQuery.Value == null)
        {
            return new Callout($"Department '{departmentId}' not found. It may have been deleted.")
                .Variant(CalloutVariant.Warning);
        }

        var department = departmentQuery.Value;

        var deleteBtn = new Button("Delete", onClick: async _ =>
            {
                blades.Pop(refresh: true);
                await DeleteAsync(factory);
                queryService.RevalidateByTag(typeof(DepartmentListRecord[]));
                queryService.RevalidateByTag(typeof(ProductListRecord[]));
                queryService.RevalidateByTag(nameof(ProductHelpers.UseDepartmentOptions));
            })
            .Variant(ButtonVariant.Destructive)
            .Icon(Icons.Trash)
            .Width(Size.Grow())
            .WithConfirm($"Are you sure you want to delete department '{department.Name}'?", "Delete Department");

        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .Width(Size.Grow())
            .ToTrigger((isOpen) => new DepartmentEditSheet(isOpen, departmentId));

        var departmentCard = new Card(
            content: new
            {
                department.Id,
                department.Name
            }.ToDetails()
            .RemoveEmpty()
            .Builder(e => e.Id, e => e.CopyToClipboard()),
            footer:
                Layout.Horizontal().Gap(2).Width(Size.Full())
                | deleteBtn
                | editBtn
            ).Title("Department Details");

        return new Fragment()
               | new BladeHeader(Text.Literal(department.Name))
               | departmentCard;
    }

    private async Task DeleteAsync(SampleDbContextFactory dbFactory)
    {
        await using var db = dbFactory.CreateDbContext();
        var department = await db.Departments.FindAsync(departmentId);
        if (department != null)
        {
            db.Departments.Remove(department);
            await db.SaveChangesAsync();
        }
    }
}

public record DepartmentCreateRequest
{
    [Required]
    public string Name { get; init; } = "";
}

public class DepartmentCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<SampleDbContextFactory>();
        var queryService = UseService<IQueryService>();
        var department = UseState(() => new DepartmentCreateRequest());

        return department
            .ToForm()
            .OnSubmit(OnSubmit)
            .ToDialog(isOpen, title: "Create Department", submitTitle: "Create");

        async Task OnSubmit(DepartmentCreateRequest request)
        {
            var departmentId = await CreateDepartment(factory, request);
            queryService.RevalidateByTag(typeof(ProductListRecord[]));
            queryService.RevalidateByTag(nameof(ProductHelpers.UseDepartmentOptions));
            refreshToken.Refresh(departmentId);
        }
    }

    private async Task<Guid> CreateDepartment(SampleDbContextFactory factory, DepartmentCreateRequest request)
    {
        await using var db = factory.CreateDbContext();

        var id = Guid.NewGuid();

        db.Departments.Add(new Department()
        {
            Id = id,
            Name = request.Name
        });
        await db.SaveChangesAsync();

        return id;
    }
}

public class DepartmentEditSheet(IState<bool> isOpen, Guid id) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<SampleDbContextFactory>();
        var queryService = UseService<IQueryService>();

        var departmentQuery = UseQuery(
            key: (typeof(Department), id),
            fetcher: async (_, ct) =>
            {
                await using var db = factory.CreateDbContext();
                return await db.Departments.FirstAsync(e => e.Id == id, ct);
            },
            tags: [(typeof(Department), id)]
        );

        if (departmentQuery.Loading || departmentQuery.Value == null)
            return Skeleton.Form().ToSheet(isOpen, "Edit Department");

        return departmentQuery.Value!
            .ToForm()
            .Remove(e => e.Id)
            .OnSubmit(OnSubmit)
            .ToSheet(isOpen, "Edit Department");

        async Task OnSubmit(Department? request)
        {
            if (request == null) return;
            var db = factory.CreateDbContext();
            db.Departments.Update(request);
            await db.SaveChangesAsync();
            queryService.RevalidateByTag((typeof(Department), id));
            queryService.RevalidateByTag(typeof(DepartmentListRecord[]));
            queryService.RevalidateByTag(typeof(ProductListRecord[]));
            queryService.RevalidateByTag(nameof(ProductHelpers.UseDepartmentOptions));
        }
    }
}
