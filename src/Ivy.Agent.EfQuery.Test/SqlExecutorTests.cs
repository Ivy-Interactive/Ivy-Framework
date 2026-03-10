using Ivy.Samples.Shared.Helpers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Agent.EfQuery.Test;

public class SqlExecutorTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SampleDbContext _context;

    public SqlExecutorTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<SampleDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new SampleDbContext(options);
        _context.Database.EnsureCreated();

        var department = new Department { Id = Guid.NewGuid(), Name = "Electronics" };
        var category = new Category { Id = Guid.NewGuid(), Name = "Gadgets" };
        _context.Departments.Add(department);
        _context.Categories.Add(category);
        _context.SaveChanges();

        _context.Products.Add(new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Price = 75,
            Rating = 4,
            Width = 10,
            Height = 20,
            DepartmentId = department.Id,
            CategoryId = category.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        _context.SaveChanges();
    }

    [Fact]
    public async Task ExecuteAsync_SelectProducts_ReturnsColumnsAndRows()
    {
        var result = await SqlExecutor.ExecuteAsync(_context, "SELECT * FROM Products", CancellationToken.None);

        Assert.Null(result.Error);
        Assert.NotEmpty(result.Columns);
        Assert.Contains("Name", result.Columns);
        Assert.Contains("Price", result.Columns);
        Assert.NotEmpty(result.Rows);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidSql_ReturnsError()
    {
        var result = await SqlExecutor.ExecuteAsync(_context, "DROP TABLE Products", CancellationToken.None);

        Assert.NotNull(result.Error);
        Assert.Empty(result.Rows);
    }

    [Fact]
    public async Task ExecuteAsync_SelectWithFilter_ReturnsFilteredResults()
    {
        var result = await SqlExecutor.ExecuteAsync(_context, "SELECT Name, Price FROM Products WHERE Price > 50", CancellationToken.None);

        Assert.Null(result.Error);
        Assert.Equal(2, result.Columns.Length);
        Assert.Equal("Name", result.Columns[0]);
        Assert.Equal("Price", result.Columns[1]);
        Assert.Single(result.Rows);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
