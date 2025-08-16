using Microsoft.EntityFrameworkCore;
using Bogus;

namespace Ivy.Samples.Shared.Helpers;

public class Product
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; }
    public int Price { get; set; }
    public int Rating { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string? Description { get; set; }
    public string? Meta { get; set; }
    [Required]
    public string Department { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    [Required]
    public Guid CategoryId { get; set; }
    public virtual Category? Category { get; set; }
}

public class Category
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; }
}

public class Order
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public Guid CustomerId { get; set; }
    public virtual Customer? Customer { get; set; }
    [Required]
    public Guid ProductId { get; set; }
    public virtual Product? Product { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Customer
{
    [Key]
    public Guid Id { get; init; }
    [Required]
    public string Name { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}


public class SampleDbContext(DbContextOptions<SampleDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Category> Categories { get; set; }
}

public class SampleDbContextFactory : IDbContextFactory<SampleDbContext>
{
    public SampleDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SampleDbContext>()
            .UseInMemoryDatabase(databaseName: "StoreDb")
            .Options;

        var context = new SampleDbContext(options);

        SeedDatabase(context);

        return context;
    }

    static void SeedDatabase(SampleDbContext context)
    {
        if (context.Products.Any() || context.Customers.Any() || context.Orders.Any())
        {
            return; // Database has already been seeded
        }

        var categories = new Faker<Category>()
            .RuleFor(c => c.Name, f => f.Commerce.Categories(1).First())
            .Generate(10);

        context.Categories.AddRange(categories);
        context.SaveChanges();

        var products = new Faker<Product>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Department, f => f.Commerce.Department())
            .RuleFor(p => p.Rating, f => f.Random.Int(0, 5))
            .RuleFor(p => p.Price, f => f.Random.Int(1, 100))
            .RuleFor(p => p.Width, f => f.Random.Int(1, 100))
            .RuleFor(p => p.Height, f => f.Random.Int(1, 100))
            .RuleFor(p => p.Description, f => f.Lorem.Sentence())
            .RuleFor(p => p.Meta, f => f.Lorem.Sentence())
            .RuleFor(p => p.CreatedAt, f => f.Date.Past(1))
            .RuleFor(p => p.UpdatedAt, f => f.Date.Past(1))
            .RuleFor(p => p.CategoryId, f => f.PickRandom(categories).Id)
            .Generate(50);

        var customers = new Faker<Customer>()
            .RuleFor(c => c.Name, f => f.Name.FullName())
            .RuleFor(c => c.Email, f => f.Internet.Email())
            .RuleFor(c => c.CreatedAt, f => f.Date.Past(1))
            .RuleFor(c => c.UpdatedAt, f => f.Date.Past(1))
            .Generate(20);

        context.Products.AddRange(products);
        context.Customers.AddRange(customers);
        context.SaveChanges();

        var orders = new Faker<Order>()
            .RuleFor(o => o.CustomerId, f => f.PickRandom(customers).Id)
            .RuleFor(o => o.ProductId, f => f.PickRandom(products).Id)
            .RuleFor(o => o.OrderDate, f => f.Date.Past(1))
            .RuleFor(o => o.CreatedAt, f => f.Date.Past(1))
            .RuleFor(o => o.UpdatedAt, f => f.Date.Past(1))
            .Generate(100);

        context.Orders.AddRange(orders);
        context.SaveChanges();
    }
}