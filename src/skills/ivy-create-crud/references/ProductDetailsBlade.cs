using Ivy;
using MyProject.Connections.MyDb;
using Microsoft.EntityFrameworkCore;

namespace MyProject.Apps.Products;

public class ProductDetailsBlade(int productId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<MyDbContextFactory>();
        var blades = UseContext<IBladeContext>();
        var queryService = UseService<IQueryService>();

        var productQuery = UseQuery(
            key: (nameof(ProductDetailsBlade), productId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.Products.Include(e => e.Category).SingleOrDefaultAsync(e => e.Id == productId, ct);
            },
            tags: [(typeof(Product), productId)]
        );

        // We can also include interesting computed fields
        var soldQuery = UseQuery(
            key: (nameof(ProductDetailsBlade), "sold", productId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.Lines.Where(e => e.ProductId == productId).SumAsync(e => e.Quantity, ct);
            },
            tags: [(typeof(Product), productId)]
        );

        if (productQuery.Loading) return Skeleton.Card();

        if (productQuery.Value == null)
        {
            return new Callout($"Product '{productId}' not found. It may have been deleted.")
                .Variant(CalloutVariant.Warning);
        }

        var productValue = productQuery.Value;

        var deleteBtn = new Button("Delete", onClick: async _ =>
            {
                blades.Pop(refresh: true);
                await DeleteAsync(factory);
                queryService.RevalidateByTag(typeof(Product[]));
            })
            .Variant(ButtonVariant.Destructive)
            .Icon(Icons.Trash)
            .WithConfirm("Are you sure you want to delete this product?", "Delete Product");

        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .Width(Size.Grow())
            //We can assume that a <Entity>EditSheet has been defined in a previous step
            .ToTrigger((isOpen) => new ProductEditSheet(isOpen, productId));

        var detailsCard = new Card(
            content: new
            {
                // We include some of the interesting fields of the entity here
                Id = productValue.Id,
                Name = productValue.Name,
                Department = productValue.Department,
                Category = productValue.Category.Name,
                Description = productValue.Description,
                // We can also include interesting computed fields
                Sold = soldQuery.Value,
            }
                .ToDetails()
                //Description is that likely has a lot of text, so we make it multiline
                .Multiline(e => e.Description)
                .RemoveEmpty() // Removes "empty" fields from the details
                .Builder(e => e.Id, e => e.CopyToClipboard()),
            footer: Layout.Horizontal().Gap(2).AlignContent(Align.Right)
                    | deleteBtn
                    | editBtn
        ).Title("Product Details").Width(Size.Units(100));

        return new Fragment()
               | new BladeHeader(Text.Literal(productValue.Name))
               | (Layout.Vertical() | detailsCard);
    }

    private async Task DeleteAsync(MyDbContextFactory dbFactory)
    {
        await using var db = dbFactory.CreateDbContext();
        var product = await db.Products.FirstOrDefaultAsync(e => e.Id == productId);
        if (product != null)
        {
            db.Products.Remove(product);
            await db.SaveChangesAsync();
        }
    }
}
