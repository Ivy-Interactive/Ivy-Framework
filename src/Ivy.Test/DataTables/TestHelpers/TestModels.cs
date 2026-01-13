namespace Ivy.Test.DataTables.TestHelpers;

/// <summary>
/// Test model representing a product for testing DataTable queries
/// </summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public double? Rating { get; set; }
    public byte[]? ImageData { get; set; }
    public Guid? SupplierId { get; set; }
}

/// <summary>
/// Test model representing a person for testing different data types
/// </summary>
public class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string? Email { get; set; }
    public DateTime DateOfBirth { get; set; }
    public decimal? Salary { get; set; }
    public bool IsActive { get; set; }
    public string Department { get; set; } = string.Empty;
}

/// <summary>
/// Test data generator
/// </summary>
public static class TestDataGenerator
{
    public static List<Product> GenerateProducts(int count = 100)
    {
        var categories = new[] { "Electronics", "Clothing", "Food", "Books", "Toys", "Sports", "Home" };
        var products = new List<Product>();
        var random = new Random(42); // Fixed seed for reproducible tests

        for (int i = 1; i <= count; i++)
        {
            products.Add(new Product
            {
                Id = i,
                Name = $"Product {i}",
                Description = i % 3 == 0 ? null : $"Description for product {i}",
                Price = (decimal)(random.NextDouble() * 1000),
                StockQuantity = random.Next(0, 100),
                Category = categories[random.Next(categories.Length)],
                IsAvailable = random.Next(2) == 1,
                CreatedDate = DateTime.Now.AddDays(-random.Next(365)),
                LastModifiedDate = i % 5 == 0 ? null : DateTime.Now.AddDays(-random.Next(30)),
                Rating = i % 4 == 0 ? null : random.NextDouble() * 5,
                ImageData = i % 10 == 0 ? new byte[] { 0x01, 0x02, 0x03 } : null,
                SupplierId = i % 7 == 0 ? null : Guid.NewGuid()
            });
        }

        return products;
    }

    public static List<Person> GeneratePeople(int count = 50)
    {
        var firstNames = new[] { "John", "Jane", "Bob", "Alice", "Charlie", "Diana", "Eve", "Frank" };
        var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis" };
        var departments = new[] { "HR", "IT", "Sales", "Marketing", "Finance", "Operations" };
        var people = new List<Person>();
        var random = new Random(42);

        for (int i = 1; i <= count; i++)
        {
            var age = random.Next(20, 65);
            people.Add(new Person
            {
                Id = i,
                FirstName = firstNames[random.Next(firstNames.Length)],
                LastName = lastNames[random.Next(lastNames.Length)],
                Age = age,
                Email = i % 4 == 0 ? null : $"person{i}@example.com",
                DateOfBirth = DateTime.Now.AddYears(-age).AddDays(-random.Next(365)),
                Salary = i % 6 == 0 ? null : (decimal)(30000 + random.NextDouble() * 120000),
                IsActive = random.Next(10) > 2, // 80% active
                Department = departments[random.Next(departments.Length)]
            });
        }

        return people;
    }
}