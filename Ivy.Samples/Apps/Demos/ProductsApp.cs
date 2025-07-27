using Ivy.Hooks;
using Ivy.Shared;
using Ivy.Views.Blades;
using Ivy.Views.Forms;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Samples.Apps.Demos;

[App(icon: Icons.Database)]
public class ProductsApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new ProductsListBlade(), "Search");
    }
}

public record ProductListRecord(Guid Id, string Name, string? Department);

public class ProductsListBlade : ViewBase
{
    public override object? Build()
    {
        //This blade will display a list of products - we choose to include the name and department of the product as these are the most relevant fields for the user.

        var blades = this.UseContext<IBladeController>();
        var factory = this.UseService<SampleDbContextFactory>();
        var refreshToken = this.UseRefreshToken();

        this.UseEffect(() =>
        {
            if (refreshToken.ReturnValue is Guid productId)
            {
                blades.Pop(this, true); // make sure the list has been refreshed
                blades.Push(this, new ProductDetailsBlade(productId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var product = (ProductListRecord)e.Sender.Tag!;
            blades.Push(this, new ProductDetailsBlade(product.Id), product.Name, width: Size.Units(100)); // by setting the width we avoid jank when different blades are opened   
        });

        ListItem CreateItem(ProductListRecord record) =>
            new(title: record.Name, onClick: onItemClicked, tag: record, subtitle: record.Department);

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this); // make sure only the current blade is visible
        }).ToTrigger((isOpen) => new ProductCreateDialog(isOpen, refreshToken));

        return new FilteredListView<ProductListRecord>(
            fetchRecords: (filter) => FetchProducts(factory, filter),
            createItem: CreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );
    }

    private async Task<ProductListRecord[]> FetchProducts(SampleDbContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var linq = db.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            linq = linq.Where(e => e.Name.Contains(filter) || e.Department.Contains(filter));
        }

        return await linq
            .OrderByDescending(e => e.CreatedAt)
            .Take(50)
            .Select(e => new ProductListRecord(e.Id, e.Name, e.Category != null ? e.Category.Name : null))
            .ToArrayAsync();
    }
}

public class ProductDetailsBlade(Guid productId) : ViewBase
{
    public override object? Build()
    {
        var factory = this.UseService<SampleDbContextFactory>();
        var blades = this.UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var product = this.UseState<Product?>(() => null!);

        this.UseEffect(async () =>
        {
            product.Set((await factory.CreateDbContext().Products.Include(e => e.Category).SingleOrDefaultAsync(e => e.Id == productId))!);
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (product.Value == null) return null;

        var _product = product.Value;

        var deleteBtn = new Button("Delete", onClick: e =>
            {
                Delete(factory);
                blades.Pop(refresh: true);
            })
            .Variant(ButtonVariant.Destructive)
            .Icon(Icons.Trash)
            .Width(Size.Grow())
            .WithConfirm($"Are you sure you want to delete product '{_product.Name}'?", "Delete Product");

        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .Width(Size.Grow())
            .ToTrigger((isOpen) => new ProductEditSheet(isOpen, productId, refreshToken));

        return Layout.Vertical() | new Card(
            content: new
            {
                // We include some of the interesting fields of the entity here
                _product.Id,
                _product.Name,
                Category = _product.Category?.Name
            }.ToDetails()
            .RemoveEmpty() // Removes "empty" fields from the details
            .Builder(e => e.Id, e => e.CopyToClipboard()),
            footer:
                Layout.Horizontal().Gap(2).Width(Size.Full())
                | deleteBtn
                | editBtn
            ).Title("Product Details");
    }

    private void Delete(SampleDbContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var product = db.Products.Find(productId)!;
        db.Products.Remove(product);
        db.SaveChanges();
    }
}

// When creating a product we only select the most relevant fields for the user to input - Always the required fields.
public record ProductCreateRequest
{
    [Required]
    public string Name { get; init; } = "";

    [Required]
    public string Department { get; init; } = "";

    [Required]
    public Guid? CategoryId { get; init; } = null;
}

public class ProductCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    public override object? Build()
    {
        var factory = this.UseService<SampleDbContextFactory>();
        var customer = this.UseState(() => new ProductCreateRequest());

        this.UseEffect(() =>
        {
            var productId = CreateProduct(factory, customer.Value);
            refreshToken.Refresh(productId);
        }, [customer]);

        return customer
            .ToForm()
            //We only specify Builder if we want to customize the input control for the field - ToForm() will scaffold the form based on the properties of the record
            //ToAsyncSelectInput allows us to select foreign keys
            .Builder(e => e.CategoryId, e => e.ToAsyncSelectInput(ProductHelpers.QueryCategories(factory), ProductHelpers.LookupCategory(factory), placeholder: "Select Category"))
            .ToDialog(isOpen, title: "Create Product", submitTitle: "Create");
    }

    private Guid CreateProduct(SampleDbContextFactory factory, ProductCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var id = Guid.NewGuid();

        db.Products.Add(new Product()
        {
            Id = id,
            Name = request.Name,
            CategoryId = request.CategoryId!.Value,
            Department = request.Department,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        db.SaveChanges();

        return id;
    }
}

public class ProductEditSheet(IState<bool> isOpen, Guid id, RefreshToken refreshToken) : ViewBase
{
    public override object? Build()
    {
        var factory = this.UseService<SampleDbContextFactory>();
        var product = this.UseState(() => factory.CreateDbContext().Products.Find(id)!);

        this.UseEffect(() =>
        {
            var db = factory.CreateDbContext();
            product.Value.UpdatedAt = DateTime.UtcNow;
            db.Products.Update(product.Value);
            db.SaveChanges();
            refreshToken.Refresh();
        }, [product]);

        return product
            .ToForm()
            // ToForm() will scaffold the form based on the properties of the record and create the appropriate builder for input controls
            // .Build(<expression>, e => To...) will allow us to customize the input control for the field 
            // NOTE! Only use this if you're sure about the syntax, otherwise leave it and let the inputs be scaffolded
            .Builder(e => e.Rating, e => e.ToFeedbackInput())
            .Builder(e => e.Description, e => e.ToTextAreaInput())
            .Place(e => e.Name, e => e.Department) // Place will specify the order of the fields
            .Place(true, e => e.Width, e => e.Height) // This will place the fields side by side - useful for related fields
            .Group("Details", e => e.Description, e => e.Meta) // This will group the fields in a collapsible group - useful for related field that are less common
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt) // We remove these fields from the form as users should not be able to edit them
            .Builder(e => e.CategoryId, e => e.ToAsyncSelectInput(ProductHelpers.QueryCategories(factory), ProductHelpers.LookupCategory(factory), placeholder: "Select Category"))
            .ToSheet(isOpen, "Edit Product");
    }
}

public static class ProductHelpers
{
    public static AsyncSelectQueryDelegate<Guid?> QueryCategories(SampleDbContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Categories
                    .Where(e => e.Name.Contains(query))
                    .Select(e => new { e.Id, e.Name })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<Guid?>(e.Name, e.Id))
                .ToArray();
        };
    }

    public static AsyncSelectLookupDelegate<Guid?> LookupCategory(SampleDbContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var category = await db.Categories.FindAsync(id);
            if (category == null) return null;
            return new Option<Guid?>(category.Name, category.Id);
        };
    }
}