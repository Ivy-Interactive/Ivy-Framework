using Ivy.Hooks;
using Ivy.Shared;
using Ivy.Views.Alerts;
using Ivy.Views.Blades;
using Ivy.Views.Builders;
using Ivy.Views.Forms;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Samples.Shared.Apps.Demos.Database;

[App(icon: Icons.Tag, searchHints: ["crud", "management", "list", "details", "forms", "entity"])]
public class CategoriesApp : SampleBase
{
    protected override object? BuildSample()
    {
        return UseBlades(() => new CategoriesListBlade(), "Search", Size.Units(75));
    }
}

public record CategoryListRecord(Guid Id, string Name);

public class CategoriesListBlade : ViewBase
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

        var categoriesQuery = UseCategoryListRecords(Context, throttledFilter.Value);

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is Guid categoryId)
            {
                blades.Pop(this);
                categoriesQuery.Mutator.Revalidate();
                blades.Push(this, new CategoryDetailsBlade(categoryId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var category = (CategoryListRecord)e.Sender.Tag!;
            blades.Push(this, new CategoryDetailsBlade(category.Id), category.Name, width: Size.Units(100));
        });

        object CreateItem(CategoryListRecord listRecord) => new FuncView(context =>
        {
            var itemQuery = UseCategoryListRecord(context, listRecord);
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
            .ToTrigger((isOpen) => new CategoryCreateDialog(isOpen, refreshToken));

        var items = (categoriesQuery.Value ?? []).Select(CreateItem);

        var header = Layout.Horizontal().Gap(1)
                     | filter.ToSearchInput().Placeholder("Search").Width(Size.Grow())
                     | createBtn;

        return new Fragment()
               | new BladeHeader(header)
               | (categoriesQuery.Value == null ? Text.Muted("Loading...") : new List(items));
    }

    public static QueryResult<CategoryListRecord[]> UseCategoryListRecords(IViewContext context, string filter)
    {
        var factory = context.UseService<SampleDbContextFactory>();
        return context.UseQuery(
            key: (nameof(UseCategoryListRecords), filter),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();

                var linq = db.Categories.AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    linq = linq.Where(e => e.Name.Contains(filter));
                }

                return await linq
                    .OrderBy(e => e.Name)
                    .Take(50)
                    .Select(e => new CategoryListRecord(e.Id, e.Name))
                    .ToArrayAsync(ct);
            },
            tags: [typeof(CategoryListRecord[])],
            options: new QueryOptions()
            {
                KeepPrevious = true
            }
        );
    }

    public static QueryResult<CategoryListRecord?> UseCategoryListRecord(IViewContext context, CategoryListRecord record)
    {
        var factory = context.UseService<SampleDbContextFactory>();
        return context.UseQuery(
            key: (nameof(UseCategoryListRecord), record.Id),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.Categories
                    .Where(e => e.Id == record.Id)
                    .Select(e => new CategoryListRecord(e.Id, e.Name))
                    .FirstOrDefaultAsync(ct);
            },
            options: new QueryOptions { RevalidateOnMount = false },
            initialValue: record,
            tags: [(typeof(Category), record.Id)]
        );
    }
}

public class CategoryDetailsBlade(Guid categoryId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<SampleDbContextFactory>();
        var blades = UseContext<IBladeService>();
        var queryService = UseService<IQueryService>();

        var categoryQuery = UseQuery(
            key: (nameof(CategoryDetailsBlade), categoryId),
            fetcher: async (ct) =>
            {
                await using var db = factory.CreateDbContext();
                return await db.Categories
                    .SingleOrDefaultAsync(e => e.Id == categoryId, ct);
            },
            tags: [(typeof(Category), categoryId)]
        );

        if (categoryQuery.Loading) return Skeleton.Card();

        if (categoryQuery.Value == null)
        {
            return new Callout($"Category '{categoryId}' not found. It may have been deleted.")
                .Variant(CalloutVariant.Warning);
        }

        var category = categoryQuery.Value;

        var deleteBtn = new Button("Delete", onClick: async _ =>
            {
                blades.Pop(refresh: true);
                await DeleteAsync(factory);
                queryService.RevalidateByTag(typeof(CategoryListRecord[]));
                queryService.RevalidateByTag(nameof(ProductHelpers.UseCategoryOptions));
            })
            .Variant(ButtonVariant.Destructive)
            .Icon(Icons.Trash)
            .Width(Size.Grow())
            .WithConfirm($"Are you sure you want to delete category '{category.Name}'?", "Delete Category");

        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .Width(Size.Grow())
            .ToTrigger((isOpen) => new CategoryEditSheet(isOpen, categoryId));

        var categoryCard = new Card(
            content: new
            {
                category.Id,
                category.Name
            }.ToDetails()
            .RemoveEmpty()
            .Builder(e => e.Id, e => e.CopyToClipboard()),
            footer:
                Layout.Horizontal().Gap(2).Width(Size.Full())
                | deleteBtn
                | editBtn
            ).Title("Category Details");

        return new Fragment()
               | new BladeHeader(Text.Literal(category.Name))
               | categoryCard;
    }

    private async Task DeleteAsync(SampleDbContextFactory dbFactory)
    {
        await using var db = dbFactory.CreateDbContext();
        var category = await db.Categories.FindAsync(categoryId);
        if (category != null)
        {
            db.Categories.Remove(category);
            await db.SaveChangesAsync();
        }
    }
}

public record CategoryCreateRequest
{
    [Required]
    public string Name { get; init; } = "";
}

public class CategoryCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<SampleDbContextFactory>();
        var queryService = UseService<IQueryService>();
        var category = UseState(() => new CategoryCreateRequest());

        return category
            .ToForm()
            .HandleSubmit(OnSubmit)
            .ToDialog(isOpen, title: "Create Category", submitTitle: "Create");

        async Task OnSubmit(CategoryCreateRequest request)
        {
            var categoryId = await CreateCategory(factory, request);
            queryService.RevalidateByTag(nameof(ProductHelpers.UseCategoryOptions));
            refreshToken.Refresh(categoryId);
        }
    }

    private async Task<Guid> CreateCategory(SampleDbContextFactory factory, CategoryCreateRequest request)
    {
        await using var db = factory.CreateDbContext();

        var id = Guid.NewGuid();

        db.Categories.Add(new Category()
        {
            Id = id,
            Name = request.Name
        });
        await db.SaveChangesAsync();

        return id;
    }
}

public class CategoryEditSheet(IState<bool> isOpen, Guid id) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<SampleDbContextFactory>();
        var queryService = UseService<IQueryService>();

        var categoryQuery = UseQuery(
            key: (typeof(Category), id),
            fetcher: async (_, ct) =>
            {
                await using var db = factory.CreateDbContext();
                return await db.Categories.FirstAsync(e => e.Id == id, ct);
            },
            tags: [(typeof(Category), id)]
        );

        if (categoryQuery.Loading || categoryQuery.Value == null)
            return Skeleton.Form().ToSheet(isOpen, "Edit Category");

        return categoryQuery.Value!
            .ToForm()
            .Remove(e => e.Id)
            .HandleSubmit(OnSubmit)
            .ToSheet(isOpen, "Edit Category");

        async Task OnSubmit(Category? request)
        {
            if (request == null) return;
            var db = factory.CreateDbContext();
            db.Categories.Update(request);
            await db.SaveChangesAsync();
            queryService.RevalidateByTag((typeof(Category), id));
            queryService.RevalidateByTag(typeof(CategoryListRecord[]));
            queryService.RevalidateByTag(nameof(ProductHelpers.UseCategoryOptions));
        }
    }
}
