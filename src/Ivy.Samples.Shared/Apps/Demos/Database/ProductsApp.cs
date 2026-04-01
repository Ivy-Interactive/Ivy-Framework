using System.Reactive.Linq;
using System.ComponentModel.DataAnnotations;
using Ivy.Samples.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Samples.Shared.Apps.Demos.Database;

[App(icon: Icons.Database, searchHints: ["crud", "management", "list", "details", "forms", "entity"])]
public class ProductsApp : SampleBase
{
    protected override object? BuildSample()
    {
        var blades = UseBlades(() => new ProductsListBlade(), "Search", Size.Units(75));
        return blades;
    }
}

public record ProductListRecord(Guid Id, string Name, string? Department);

public class ProductsListBlade : ViewBase
{
    public override object? Build()
    {
        var blades = UseContext<IBladeContext>();
        var refreshToken = UseRefreshToken();

        // Filter state with throttling for search
        var filter = UseState("");
        var throttledFilter = UseState("");

        UseEffect(() =>
        {
            throttledFilter.Set(filter.Value);
            blades.Pop(this); // Close any open detail blades when filter changes
        }, [filter.Throttle(TimeSpan.FromMilliseconds(250)).ToTrigger()]);

        var productsQuery = UseProductListRecords(Context, throttledFilter.Value);

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is Guid productId)
            {
                blades.Pop(this);
                productsQuery.Mutator.Revalidate();
                blades.Push(this, new ProductDetailsBlade(productId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var product = (ProductListRecord)e.Sender.Tag!;
            blades.Push(this, new ProductDetailsBlade(product.Id), product.Name, width: Size.Units(100)); // by setting the width we avoid jank when different blades are opened
        });

        object CreateItem(ProductListRecord listRecord) => new FuncView(context =>
        {
            var itemQuery = UseProductListRecord(context, listRecord);
            if (itemQuery.Loading || itemQuery.Value == null)
            {
                return new ListItem();
            }
            var record = itemQuery.Value;
            return new ListItem(title: record.Name, onClick: onItemClicked, tag: record, subtitle: record.Department);
        });

        var createBtn = Icons.Plus
            .ToButton(_ =>
            {
                blades.Pop(this); // make sure only the current blade is visible
            })
            .Ghost()
            .ToTrigger((isOpen) => new ProductCreateDialog(isOpen, refreshToken));

        var items = (productsQuery.Value ?? []).Select(CreateItem);

        var header = Layout.Horizontal().Gap(1)
                     | filter.ToSearchInput().Placeholder("Search").Width(Size.Grow())
                     | createBtn;

        return new Fragment()
               | new BladeHeader(header)
               | (productsQuery.Value == null ? Text.Muted("Loading...") : new List(items));
    }

    public static QueryResult<ProductListRecord[]> UseProductListRecords(IViewContext context, string filter)
    {
        var factory = context.UseService<SampleDbContextFactory>();
        return context.UseQuery(
            key: (nameof(UseProductListRecords), filter),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();

                var linq = db.Products.Include(e => e.Department).AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    linq = linq.Where(e => e.Name.Contains(filter) || (e.Department != null && e.Department.Name.Contains(filter)));
                }

                return await linq
                    .OrderByDescending(e => e.CreatedAt)
                    .Take(50)
                    .Select(e => new ProductListRecord(e.Id, e.Name, e.Department != null ? e.Department.Name : null))
                    .ToArrayAsync(ct);
            },
            tags: [typeof(ProductListRecord[])],
            options: new QueryOptions()
            {
                KeepPrevious = true
            }
        );
    }

    public static QueryResult<ProductListRecord?> UseProductListRecord(IViewContext context, ProductListRecord record)
    {
        var factory = context.UseService<SampleDbContextFactory>();
        return context.UseQuery(
            key: (nameof(UseProductListRecord), record.Id),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.Products
                    .Include(e => e.Department)
                    .Where(e => e.Id == record.Id)
                    .Select(e => new ProductListRecord(e.Id, e.Name, e.Department != null ? e.Department.Name : null))
                    .FirstOrDefaultAsync(ct);
            },
            options: new QueryOptions { RevalidateOnMount = false },
            initialValue: record,
            tags: [(typeof(Product), record.Id)]
        );
    }
}

public class ProductDetailsBlade(Guid productId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<SampleDbContextFactory>();
        var blades = UseContext<IBladeContext>();
        var queryService = UseService<IQueryService>();

        var productQuery = UseQuery(
            key: (nameof(ProductDetailsBlade), productId),
            fetcher: async (ct) =>
            {
                await using var db = factory.CreateDbContext();
                return await db.Products
                    .Include(e => e.Category)
                    .Include(e => e.Department)
                    .SingleOrDefaultAsync(e => e.Id == productId, ct);
            },
            tags: [(typeof(Product), productId)]
        );

        if (productQuery.Loading) return Skeleton.Card();

        if (productQuery.Value == null)
        {
            return new Callout($"Product '{productId}' not found. It may have been deleted.")
                .Variant(CalloutVariant.Warning);
        }

        var product = productQuery.Value;

        var deleteBtn = new Button("Delete", onClick: async _ =>
            {
                blades.Pop(refresh: true);
                await DeleteAsync(factory);
                queryService.RevalidateByTag(typeof(ProductListRecord[]));
            })
            .Variant(ButtonVariant.Destructive)
            .Icon(Icons.Trash)
            .Width(Size.Grow())
            .WithConfirm($"Are you sure you want to delete product '{product.Name}'?", "Delete Product", confirmLabel: "Delete", destructive: true);

        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .Width(Size.Grow())
            .ToTrigger((isOpen) => new ProductEditSheet(isOpen, productId));

        var productCard = new Card(
            content: new
            {
                product.Id,
                product.Name,
                Category = product.Category?.Name,
                Department = product.Department?.Name
            }.ToDetails()
            .RemoveEmpty() // Removes "empty" fields from the details
            .Builder(e => e.Id, e => e.CopyToClipboard()),
            footer:
                Layout.Horizontal().Gap(2).Width(Size.Full())
                | deleteBtn
                | editBtn
            ).Title("Product Details");

        return new Fragment()
               | new BladeHeader(Text.Literal(product.Name)) //todo: why is this bigger?
               | productCard;
    }

    private async Task DeleteAsync(SampleDbContextFactory dbFactory)
    {
        await using var db = dbFactory.CreateDbContext();
        var product = await db.Products.FindAsync(productId);
        if (product != null)
        {
            db.Products.Remove(product);
            await db.SaveChangesAsync();
        }
    }
}

public record ProductCreateRequest
{
    [Required]
    public string Name { get; init; } = "";

    [Required]
    public Guid? DepartmentId { get; init; } = null;

    [Required]
    public Guid? CategoryId { get; init; } = null;
}

public class ProductCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<SampleDbContextFactory>();
        var customer = UseState(() => new ProductCreateRequest());

        var departmentsQuery = Context.UseDepartmentOptions();

        if (departmentsQuery.Loading) return Skeleton.Form().ToDialog(isOpen, "Create Product");

        return customer
            .ToForm()
            .Builder(e => e.DepartmentId, e => e.ToSelectInput(departmentsQuery.Value!, placeholder: "Select Department"))
            .Builder(e => e.CategoryId, e => e.ToAsyncSelectInput(ProductHelpers.UseCategoryOptions, ProductHelpers.UseCategoryOption, placeholder: "Select Category"))
            .OnSubmit(OnSubmit)
            .ToDialog(isOpen, title: "Create Product", submitTitle: "Create");

        async Task OnSubmit(ProductCreateRequest request)
        {
            var productId = await CreateProduct(factory, request);
            refreshToken.Refresh(productId);
        }
    }

    private async Task<Guid> CreateProduct(SampleDbContextFactory factory, ProductCreateRequest request)
    {
        await using var db = factory.CreateDbContext();

        var id = Guid.NewGuid();

        db.Products.Add(new Product()
        {
            Id = id,
            Name = request.Name,
            CategoryId = request.CategoryId!.Value,
            DepartmentId = request.DepartmentId!.Value,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        return id;
    }
}

public class ProductEditSheet(IState<bool> isOpen, Guid id) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<SampleDbContextFactory>();
        var queryService = UseService<IQueryService>();

        var productQuery = UseQuery(
            key: (typeof(Product), id),
            fetcher: async (_, ct) =>
            {
                await using var db = factory.CreateDbContext();
                return await db.Products.FirstAsync(e => e.Id == id, ct);
            },
            tags: [(typeof(Product), id)]
        );

        var departmentsQuery = Context.UseDepartmentOptions();

        if (productQuery.Loading || departmentsQuery.Loading || productQuery.Value == null)
            return Skeleton.Form().ToSheet(isOpen, "Edit Product");

        return productQuery.Value!
            .ToForm()
            .Builder(e => e.Rating, e => e.ToFeedbackInput())
            .Builder(e => e.Description, e => e.ToTextareaInput())
            .Place(e => e.Name, e => e.DepartmentId) // Place will specify the order of the fields
            .PlaceHorizontal(e => e.Width, e => e.Height) // This will place the fields side by side - useful for related fields
            .Group("Details", open: true, e => e.Description, e => e.Meta) // This will group the fields in a collapsible group that is open by default - useful for related fields that are less common
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt, e => e.Department, e => e.Category) // We remove these fields from the form as users should not be able to edit them
            .Builder(e => e.DepartmentId, e => e.ToSelectInput(departmentsQuery.Value!, placeholder: "Select Department"))
            .Builder(e => e.CategoryId, e => e.ToAsyncSelectInput(ProductHelpers.UseCategoryOptions, ProductHelpers.UseCategoryOption, placeholder: "Select Category"))
            .OnSubmit(OnSubmit)
            .ToSheet(isOpen, "Edit Product");

        async Task OnSubmit(Product? request)
        {
            if (request == null) return;
            var db = factory.CreateDbContext();
            request.UpdatedAt = DateTime.UtcNow;
            db.Products.Update(request);
            await db.SaveChangesAsync();
            queryService.RevalidateByTag((typeof(Product), id));
        }
    }
}

public static class ProductHelpers
{
    public static QueryResult<Option<Guid>[]> UseCategoryOptions(this IViewContext context)
    {
        var factory = context.UseService<SampleDbContextFactory>();
        return context.UseQuery(
            key: (nameof(UseCategoryOptions)),
            fetcher: async (_, ct) =>
            {
                await using var db = factory.CreateDbContext();
                return await db.Categories
                    .Select(e => new Option<Guid>(e.Name, e.Id))
                    .ToArrayAsync(ct);
            },
            tags: [nameof(UseCategoryOptions)]);
    }

    public static QueryResult<Option<Guid>[]> UseDepartmentOptions(this IViewContext context)
    {
        var factory = context.UseService<SampleDbContextFactory>();
        return context.UseQuery(
            key: (nameof(UseDepartmentOptions)),
            fetcher: async (_, ct) =>
            {
                await using var db = factory.CreateDbContext();
                return await db.Departments
                    .Select(e => new Option<Guid>(e.Name, e.Id))
                    .ToArrayAsync(ct);
            },
            tags: [nameof(UseDepartmentOptions)]);
    }

    public static QueryResult<Option<Guid?>[]> UseCategoryOptions(IViewContext context, string filter)
    {
        var factory = context.UseService<SampleDbContextFactory>();
        return context.UseQuery(
            key: (nameof(UseCategoryOptions), query: filter),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return (await db.Categories
                        .Where(e => e.Name.Contains(filter))
                        .Select(e => new { e.Id, e.Name })
                        .Take(50)
                        .ToArrayAsync(ct))
                    .Select(e => new Option<Guid?>(e.Name, e.Id))
                    .ToArray();
            },
            tags: [nameof(UseCategoryOptions)]);
    }

    public static QueryResult<Option<Guid?>?> UseCategoryOption(IViewContext context, Guid? id)
    {
        var factory = context.UseService<SampleDbContextFactory>();
        return context.UseQuery(
            key: (nameof(UseCategoryOption), id),
            fetcher: async ct =>
            {
                if (id == null) return null;
                await using var db = factory.CreateDbContext();
                var category = await db.Categories.FindAsync([id], ct);
                if (category == null) return null;
                return new Option<Guid?>(category.Name, category.Id);
            },
            tags: [nameof(UseCategoryOptions)]);
    }
}
