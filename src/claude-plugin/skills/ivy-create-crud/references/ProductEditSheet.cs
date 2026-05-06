using Ivy;
using MyProject.Connections.MyDb;
using Microsoft.EntityFrameworkCore;

namespace MyProject.Apps.Products;

public class ProductEditSheet(IState<bool> isOpen, int productId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<MyDbContextFactory>();
        var queryService = UseService<IQueryService>();

        var productQuery = UseQuery(
            key: (typeof(Product), productId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.Products.FirstAsync(e => e.Id == productId, ct);
            },
            tags: [(typeof(Product), productId)]
        );

        if (productQuery.Loading || productQuery.Value == null)
            return Skeleton.Form().ToSheet(isOpen, "Edit Product");

        return productQuery.Value
            .ToForm()
            // ToForm() will scaffold the form based on the properties of the record and create the appropriate builder for input controls
            // .Build(<expression>, e => To...) will allow us to customize the input control for the field
            // NOTE! Only use this if you're sure about the syntax, otherwise leave it and let the inputs be scaffolded
            .Builder(e => e.Rating, e => e.ToFeedbackInput())
            .Builder(e => e.Description, e => e.ToTextareaInput())
            .Place(e => e.Name, e => e.Department) // Place will specify the order of the fields
            .PlaceHorizontal(e => e.Width, e => e.Height) // This will place the fields side by side - useful for related fields such as width and height or first name and last name
            .Group("Details", e => e.Description, e => e.Brand, e => e.Size, e => e.Variant, e => e.Color) // This will group the fields in a collapsible group - useful for related fields - like address
                                                                                                           // IMPORTANT: Only remove properties that exist on the entity! Check the entity definition first.
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt) // We remove these fields from the form as users should not be able to edit them
            .Builder(e => e.CategoryId, e => e.ToAsyncSelectInput(UseCategorySearch, UseCategoryLookup, placeholder: "Select Category"))
            .OnSubmit(OnSubmit)
            .ToSheet(isOpen, "Edit Product");

        async Task OnSubmit(Product? request)
        {
            if (request == null) return;
            await using var db = factory.CreateDbContext();
            // IMPORTANT: Only set UpdatedAt if the property exists on the entity!
            // Check the entity definition first. If UpdatedAt doesn't exist, remove this line.
            // For non-nullable DateTime: always set without null check
            // For nullable DateTime?: can check for null if needed
            // WARNING: Never compare non-nullable DateTime to null (causes CS8073)
            request.UpdatedAt = DateTime.UtcNow; //Note: For DateTime properties use DateTime.UtcNow directly. For string properties use DateTime.UtcNow.ToString("O") instead.
            db.Products.Update(request);
            await db.SaveChangesAsync();
            queryService.RevalidateByTag((typeof(Product), productId));
        }
    }

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
