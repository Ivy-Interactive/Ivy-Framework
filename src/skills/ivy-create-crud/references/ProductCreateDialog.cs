using System.ComponentModel.DataAnnotations;
using Ivy;
using MyProject.Connections.MyDb;
using Microsoft.EntityFrameworkCore;

namespace MyProject.Apps.Products;

public class ProductCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    // We only include the required fields from the entity.
    // ALWAYS include the required fields of the entity!
    // ALWAYS use the same type as in the target entity, so we don't run into issues when saving the entity to the database.
    private record ProductCreateRequest
    {
        [Required]
        public string Name { get; init; } = "";

        [Required]
        public string Department { get; init; } = "";

        [Required]
        public int? CategoryId { get; init; } = null;
    }

    public override object? Build()
    {
        var factory = UseService<MyDbContextFactory>();
        var customer = UseState(() => new ProductCreateRequest());

        return customer
            .ToForm()
            // IMPORTANT: No .Remove() needed here! The CreateRequest record only contains user input fields.
            // We don't have Id, CreatedAt, UpdatedAt in the request model, so there's nothing to remove.
            //We only specify Builder if we want to customize the input control for the field - ToForm() will scaffold the form based on the properties of the record
            //ToAsyncSelectInput allows us to select foreign keys
            .Builder(e => e.CategoryId, e => e.ToAsyncSelectInput(UseCategorySearch, UseCategoryLookup, placeholder: "Select Category"))
            .OnSubmit(OnSubmit)
            .ToDialog(isOpen, title: "Create Product", submitTitle: "Create");

        async Task OnSubmit(ProductCreateRequest request)
        {
            var productId = await CreateProductAsync(factory, request);
            refreshToken.Refresh(productId);
        }
    }

    private async Task<int> CreateProductAsync(MyDbContextFactory factory, ProductCreateRequest request)
    {
        await using var db = factory.CreateDbContext();

        var product = new Product()
        {
            Name = request.Name,
            CategoryId = request.CategoryId!.Value,
            Department = request.Department,
            // IMPORTANT: Only set CreatedAt and UpdatedAt if these properties exist on the entity!
            // Check the entity definition first. Some entities (especially junction tables) may not have these.
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();

        return product.Id;
    }

    // IMPORTANT: AsyncSelect helper methods MUST return QueryResult<>, NOT Task<>.
    // Search signature: QueryResult<Option<TKey?>[]> MethodName(IViewContext context, string query)
    // Lookup signature: QueryResult<Option<TKey?>?> MethodName(IViewContext context, TKey? id)
    // Use context.UseQuery() inside the method body to return QueryResult.
    private static QueryResult<Option<int?>[]> UseCategorySearch(IViewContext context, string query)
    {
        var factory = context.UseService<MyDbContextFactory>();
        return context.UseQuery(
            key: (nameof(UseCategorySearch), query),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return (await db.Categories
                        .Where(e => e.Name.Contains(query))
                        .Select(e => new { e.Id, e.Name })
                        .Take(50)
                        .ToArrayAsync(ct))
                    .Select(e => new Option<int?>(e.Name, e.Id))
                    .ToArray();
            });
    }

    private static QueryResult<Option<int?>?> UseCategoryLookup(IViewContext context, int? id)
    {
        var factory = context.UseService<MyDbContextFactory>();
        return context.UseQuery(
            key: (nameof(UseCategoryLookup), id),
            fetcher: async ct =>
            {
                if (id == null) return null;
                await using var db = factory.CreateDbContext();
                var category = await db.Categories.FirstOrDefaultAsync(e => e.Id == id, ct);
                if (category == null) return null;
                return new Option<int?>(category.Name, category.Id);
            });
    }
}
