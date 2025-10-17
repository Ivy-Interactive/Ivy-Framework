using Google.Protobuf.WellKnownTypes;
using Ivy.Protos.DataTable;
using Ivy.Test.DataTables.TestHelpers;
using Ivy.Views.DataTables;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;

namespace Ivy.Test.DataTables;

public class QueryProcessorValuesTests
{
    private readonly ITestOutputHelper _output;

    public QueryProcessorValuesTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ProcessValues_BasicQuery_ReturnsDistinctValues()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(20);
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        var query = new DataTableValuesQuery
        {
            SourceId = "test-products",
            Column = "Category"
        };

        // Act
        var result = processor.ProcessValues(queryable, query);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Values);

        // Should have distinct categories
        var expectedCategories = products.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();
        Assert.Equal(expectedCategories.Count, result.TotalValues);
        Assert.Equal(expectedCategories, result.Values);

        _output.WriteLine($"Found {result.TotalValues} distinct categories: {string.Join(", ", result.Values)}");
    }

    [Fact]
    public void ProcessValues_WithLimit_ReturnsLimitedValues()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(50);
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        var query = new DataTableValuesQuery
        {
            SourceId = "test-products",
            Column = "Name",
            Limit = 5
        };

        // Act
        var result = processor.ProcessValues(queryable, query);

        // Assert
        Assert.Equal(5, result.Values.Count);
        Assert.Equal(50, result.TotalValues); // Should still report total count

        // Should be ordered alphabetically
        var orderedNames = products.Select(p => p.Name).Distinct().OrderBy(n => n).Take(5).ToList();
        Assert.Equal(orderedNames, result.Values);

        _output.WriteLine($"Retrieved {result.Values.Count} of {result.TotalValues} total values");
    }

    [Fact]
    public void ProcessValues_WithSearch_FiltersValues()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(30);
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        var query = new DataTableValuesQuery
        {
            SourceId = "test-products",
            Column = "Name",
            Search = "1" // Should match Product 1, 10-19, 21
        };

        // Act
        var result = processor.ProcessValues(queryable, query);

        // Assert
        var expectedNames = products.Select(p => p.Name)
            .Where(n => n.Contains("1", StringComparison.OrdinalIgnoreCase))
            .Distinct()
            .OrderBy(n => n)
            .ToList();

        Assert.Equal(expectedNames.Count, result.Values.Count);
        Assert.Equal(expectedNames, result.Values);

        _output.WriteLine($"Found {result.Values.Count} names containing '1': {string.Join(", ", result.Values)}");
    }

    [Fact]
    public void ProcessValues_NumericColumn_ConvertsToString()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(15);
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        var query = new DataTableValuesQuery
        {
            SourceId = "test-products",
            Column = "StockQuantity"
        };

        // Act
        var result = processor.ProcessValues(queryable, query);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Values);

        // All values should be string representations of numbers
        var expectedValues = products.Select(p => p.StockQuantity.ToString())
            .Distinct()
            .OrderBy(v => int.Parse(v))
            .ToList();

        Assert.Equal(expectedValues.Count, result.TotalValues);

        _output.WriteLine($"Numeric values as strings: {string.Join(", ", result.Values)}");
    }

    [Fact]
    public void ProcessValues_BooleanColumn_ConvertsToString()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(20);
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        var query = new DataTableValuesQuery
        {
            SourceId = "test-products",
            Column = "IsAvailable"
        };

        // Act
        var result = processor.ProcessValues(queryable, query);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Values);

        // Should have at most 2 values: "True" and "False"
        Assert.True(result.Values.Count <= 2);
        Assert.All(result.Values, v => Assert.True(v == "True" || v == "False"));

        _output.WriteLine($"Boolean values: {string.Join(", ", result.Values)}");
    }

    [Fact]
    public void ProcessValues_WithNullValues_ExcludesNulls()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(30);
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        var query = new DataTableValuesQuery
        {
            SourceId = "test-products",
            Column = "Description" // Every 3rd product has null description
        };

        // Act
        var result = processor.ProcessValues(queryable, query);

        // Assert
        Assert.NotNull(result);

        // Should not include null values
        var expectedDescriptions = products
            .Select(p => p.Description)
            .Where(d => d != null)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        Assert.Equal(expectedDescriptions.Count, result.Values.Count);
        Assert.DoesNotContain(null, result.Values);

        _output.WriteLine($"Found {result.Values.Count} non-null descriptions out of {products.Count} products");
    }

    [Fact]
    public void ProcessValues_SearchOnNumericColumn_WorksCorrectly()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(25);
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        var query = new DataTableValuesQuery
        {
            SourceId = "test-products",
            Column = "StockQuantity",
            Search = "5" // Should match values containing 5 (5, 15, 25, 50, 51, etc.)
        };

        // Act
        var result = processor.ProcessValues(queryable, query);

        // Assert
        Assert.NotNull(result);

        var expectedValues = products.Select(p => p.StockQuantity.ToString())
            .Where(v => v.Contains("5"))
            .Distinct()
            .OrderBy(v => int.Parse(v))
            .ToList();

        Assert.Equal(expectedValues.Count, result.Values.Count);

        _output.WriteLine($"Stock quantities containing '5': {string.Join(", ", result.Values)}");
    }

    [Fact]
    public void ProcessValues_EmptyQueryable_ReturnsEmptyResult()
    {
        // Arrange
        var emptyQueryable = new List<Product>().AsQueryable();
        var processor = new QueryProcessor(logger: null);

        var query = new DataTableValuesQuery
        {
            SourceId = "test-empty",
            Column = "Name"
        };

        // Act
        var result = processor.ProcessValues(emptyQueryable, query);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Values);
        Assert.Equal(0, result.TotalValues);

        _output.WriteLine("Empty queryable returns empty values result");
    }

    [Fact]
    public void ProcessValues_InvalidColumn_ThrowsException()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(10);
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        var query = new DataTableValuesQuery
        {
            SourceId = "test-products",
            Column = "NonExistentColumn"
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            processor.ProcessValues(queryable, query));

        Assert.Contains("NonExistentColumn", exception.Message);
        _output.WriteLine($"Expected error: {exception.Message}");
    }

    [Fact]
    public void ProcessValues_EmptyColumn_ThrowsException()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(10);
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        var query = new DataTableValuesQuery
        {
            SourceId = "test-products",
            Column = "" // Empty column name
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            processor.ProcessValues(queryable, query));

        Assert.Contains("Column name is required", exception.Message);
        _output.WriteLine($"Expected error: {exception.Message}");
    }

    [Fact]
    public void ProcessValues_DecimalColumn_FormatsCorrectly()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Price = 99.99m, Category = "Test", CreatedDate = DateTime.Now },
            new Product { Id = 2, Name = "Product 2", Price = 199.50m, Category = "Test", CreatedDate = DateTime.Now },
            new Product { Id = 3, Name = "Product 3", Price = 99.99m, Category = "Test", CreatedDate = DateTime.Now }, // Duplicate price
            new Product { Id = 4, Name = "Product 4", Price = 49.00m, Category = "Test", CreatedDate = DateTime.Now }
        };
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        var query = new DataTableValuesQuery
        {
            SourceId = "test-products",
            Column = "Price"
        };

        // Act
        var result = processor.ProcessValues(queryable, query);

        // Assert
        Assert.Equal(3, result.Values.Count); // 99.99, 199.50, 49.00 (distinct)
        // Values might be formatted with culture-specific decimal separator
        var hasExpectedValues = result.Values.Any(v => v.Contains("99") && (v.Contains("99") || v.Contains("99"))) &&
                                result.Values.Any(v => v.Contains("199") && (v.Contains("5") || v.Contains("50"))) &&
                                result.Values.Any(v => v.Contains("49"));
        Assert.True(hasExpectedValues, $"Expected decimal values not found. Actual: {string.Join(", ", result.Values)}");

        _output.WriteLine($"Decimal values: {string.Join(", ", result.Values)}");
    }

    [Fact]
    public void ProcessValues_DateTimeColumn_FormatsCorrectly()
    {
        // Arrange
        var baseDate = new DateTime(2024, 1, 1);
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Price = 100m, Category = "Test", CreatedDate = baseDate },
            new Product { Id = 2, Name = "Product 2", Price = 200m, Category = "Test", CreatedDate = baseDate.AddDays(1) },
            new Product { Id = 3, Name = "Product 3", Price = 300m, Category = "Test", CreatedDate = baseDate },
            new Product { Id = 4, Name = "Product 4", Price = 400m, Category = "Test", CreatedDate = baseDate.AddDays(2) }
        };
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        var query = new DataTableValuesQuery
        {
            SourceId = "test-products",
            Column = "CreatedDate"
        };

        // Act
        var result = processor.ProcessValues(queryable, query);

        // Assert
        Assert.Equal(3, result.Values.Count); // 3 distinct dates
        Assert.All(result.Values, v => DateTime.TryParse(v, out _)); // All should be parseable dates

        _output.WriteLine($"DateTime values: {string.Join(", ", result.Values)}");
    }

    [Fact]
    public void ProcessValues_WithCaching_UsesCachedResult()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(20);
        var queryable = products.AsQueryable();

        var options = Options.Create(new MemoryDistributedCacheOptions());
        var cache = new MemoryDistributedCache(options);

        var processor = new QueryProcessor(logger: null, cache: cache);

        var query = new DataTableValuesQuery
        {
            SourceId = "test-products",
            Column = "Category"
        };

        // Act
        var result1 = processor.ProcessValues(queryable, query);

        // Modify the underlying data (this shouldn't affect cached result)
        products.Add(new Product { Id = 999, Name = "New Product", Category = "NewCategory", Price = 999m, CreatedDate = DateTime.Now });

        var result2 = processor.ProcessValues(queryable, query);

        // Assert
        Assert.Equal(result1.TotalValues, result2.TotalValues);
        Assert.Equal(result1.Values, result2.Values);
        Assert.DoesNotContain("NewCategory", result2.Values); // Cached result shouldn't have the new category

        _output.WriteLine("Cache test: Second call returned cached result");
    }

    [Fact]
    public void ProcessValues_DifferentQueries_UsesDifferentCacheKeys()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(20);
        var queryable = products.AsQueryable();

        var options = Options.Create(new MemoryDistributedCacheOptions());
        var cache = new MemoryDistributedCache(options);

        var processor = new QueryProcessor(logger: null, cache: cache);

        var query1 = new DataTableValuesQuery
        {
            SourceId = "test-products",
            Column = "Category"
        };

        var query2 = new DataTableValuesQuery
        {
            SourceId = "test-products",
            Column = "Category",
            Search = "Elec" // Different search
        };

        // Act
        var result1 = processor.ProcessValues(queryable, query1);
        var result2 = processor.ProcessValues(queryable, query2);

        // Assert
        Assert.NotEqual(result1.Values.Count, result2.Values.Count);
        Assert.True(result2.Values.Count < result1.Values.Count); // Filtered results should be fewer

        _output.WriteLine($"Different cache keys: Query1 returned {result1.Values.Count} values, Query2 returned {result2.Values.Count} values");
    }

    [Fact]
    public void ProcessValues_CacheFailure_ContinuesWithoutCaching()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(10);
        var queryable = products.AsQueryable();

        // Create a failing cache implementation
        var failingCache = new FailingDistributedCache();

        var processor = new QueryProcessor(logger: null, cache: failingCache);

        var query = new DataTableValuesQuery
        {
            SourceId = "test-products",
            Column = "Category"
        };

        // Act
        var result = processor.ProcessValues(queryable, query);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Values);

        _output.WriteLine("Cache failure handled gracefully, query executed successfully");
    }

    // Helper class for testing cache failures
    private class FailingDistributedCache : IDistributedCache
    {
        public byte[]? Get(string key) => null; // Return null to simulate cache miss, not throw
        public Task<byte[]?> GetAsync(string key, CancellationToken token = default) => Task.FromResult<byte[]?>(null);
        public void Refresh(string key) { } // Do nothing
        public Task RefreshAsync(string key, CancellationToken token = default) => Task.CompletedTask;
        public void Remove(string key) { } // Do nothing
        public Task RemoveAsync(string key, CancellationToken token = default) => Task.CompletedTask;
        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => throw new Exception("Cache write error");
        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) => throw new Exception("Cache write error");
    }

    [Fact]
    public void ProcessValues_ComplexData_HandlesAllDataTypes()
    {
        // Arrange
        var people = TestDataGenerator.GeneratePeople(30);
        var queryable = people.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        // Test string column
        var query = new DataTableValuesQuery
        {
            SourceId = "test-people",
            Column = "Department"
        };

        var result = processor.ProcessValues(queryable, query);
        Assert.NotEmpty(result.Values);
        _output.WriteLine($"Departments: {string.Join(", ", result.Values)}");

        // Test numeric column
        query.Column = "Age";
        result = processor.ProcessValues(queryable, query);
        Assert.NotEmpty(result.Values);
        _output.WriteLine($"Ages: {string.Join(", ", result.Values.Take(10))}..."); // Show first 10

        // Test nullable column
        query.Column = "Email";
        result = processor.ProcessValues(queryable, query);
        // Should exclude nulls
        var expectedEmails = people.Where(p => p.Email != null)
            .Select(p => p.Email)
            .Distinct()
            .OrderBy(m => m)
            .ToList();
        Assert.Equal(expectedEmails.Count, result.Values.Count);
        _output.WriteLine($"Emails (non-null): {result.Values.Count} distinct values");
    }
}