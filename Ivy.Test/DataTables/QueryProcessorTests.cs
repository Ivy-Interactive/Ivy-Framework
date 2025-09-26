using Google.Protobuf.WellKnownTypes;
using Ivy.Protos.DataTable;
using Ivy.Test.DataTables.TestHelpers;
using Ivy.Views.DataTables;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Ivy.Test.DataTables;

public class QueryProcessorTests
{
    private readonly ITestOutputHelper _output;
    private readonly ILogger<QueryProcessor>? _logger;

    public QueryProcessorTests(ITestOutputHelper output)
    {
        _output = output;
        // Optionally create a logger that writes to test output
        // _logger = new TestLogger<QueryProcessor>(output);
    }

    [Fact]
    public void SimpleQuery_NoFiltersOrSorting_ReturnsAllData()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(10);
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(_logger);

        var query = new DataTableQuery
        {
            SourceId = "test-products",
            Offset = 0,
            Limit = 100  // Larger than dataset
        };

        // Act
        var result = processor.ProcessQuery(queryable, query);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ArrowData);
        Assert.True(result.ArrowData.Length > 0);
        Assert.Equal(0, result.Offset);
        Assert.Equal(10, result.RowCount);
        Assert.Equal(10, result.TotalRows);

        // Parse the Arrow data
        var batch = ArrowTestHelper.ParseArrowData(result.ArrowData);

        // Verify schema
        Assert.Equal(12, batch.Schema.FieldsList.Count); // Product has 12 properties
        Assert.Contains(batch.Schema.FieldsList, f => f.Name == "Id");
        Assert.Contains(batch.Schema.FieldsList, f => f.Name == "Name");
        Assert.Contains(batch.Schema.FieldsList, f => f.Name == "Price");
        Assert.Contains(batch.Schema.FieldsList, f => f.Name == "Category");

        // Verify row count
        Assert.Equal(10, batch.Length);

        // Verify some actual data
        var ids = ArrowTestHelper.GetColumnValues(batch, "Id");
        Assert.Equal(new object[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, ids);

        var names = ArrowTestHelper.GetColumnValues(batch, "Name");
        Assert.Equal("Product 1", names[0]);
        Assert.Equal("Product 10", names[9]);

        // Verify nullable fields are handled correctly
        var descriptions = ArrowTestHelper.GetColumnValues(batch, "Description");
        Assert.Null(descriptions[2]); // Product 3 should have null description (i % 3 == 0)
        Assert.NotNull(descriptions[0]); // Product 1 should have description

        // Print debug info
        _output.WriteLine($"Total Arrow data size: {result.ArrowData.Length} bytes");
        _output.WriteLine($"Schema fields: {string.Join(", ", batch.Schema.FieldsList.Select(f => $"{f.Name}:{f.DataType}"))}");
        _output.WriteLine("\nFirst 3 rows:");
        var rows = ArrowTestHelper.GetAllRows(batch).Take(3);
        foreach (var row in rows)
        {
            _output.WriteLine(string.Join(", ", row.Select(kvp => $"{kvp.Key}={kvp.Value ?? "null"}")));
        }
    }

    [Fact]
    public void Query_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(25);
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(_logger);

        var query = new DataTableQuery
        {
            SourceId = "test-products",
            Offset = 10,
            Limit = 5
        };

        // Act
        var result = processor.ProcessQuery(queryable, query);

        // Assert
        Assert.Equal(10, result.Offset);
        Assert.Equal(5, result.RowCount);
        Assert.Equal(25, result.TotalRows);

        // Parse and verify data
        var batch = ArrowTestHelper.ParseArrowData(result.ArrowData);
        Assert.Equal(5, batch.Length);

        var ids = ArrowTestHelper.GetColumnValues(batch, "Id");
        Assert.Equal(new object[] { 11, 12, 13, 14, 15 }, ids);
    }

    [Fact]
    public void Query_WithSimpleFilter_ReturnsFilteredData()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(20);
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(_logger);

        var query = new DataTableQuery
        {
            SourceId = "test-products",
            Offset = 0,
            Limit = 100,
            Filter = new Filter
            {
                Condition = new Condition
                {
                    Column = "Category",
                    Function = "equals",
                    Args = { Google.Protobuf.WellKnownTypes.Any.Pack(new Google.Protobuf.WellKnownTypes.StringValue { Value = "Electronics" }) }
                }
            }
        };

        // Act
        var result = processor.ProcessQuery(queryable, query);

        // Assert
        var batch = ArrowTestHelper.ParseArrowData(result.ArrowData);
        var categories = ArrowTestHelper.GetColumnValues(batch, "Category");

        // All results should be Electronics
        Assert.All(categories, cat => Assert.Equal("Electronics", cat));

        // Should have filtered some items out (not all 20)
        Assert.True(result.RowCount < 20);
        Assert.Equal(result.RowCount, result.TotalRows); // No pagination, so they match

        _output.WriteLine($"Filtered to {result.RowCount} Electronics products out of 20 total");
    }

    [Fact]
    public void Query_WithSorting_ReturnsSortedData()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(15);
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(_logger);

        var query = new DataTableQuery
        {
            SourceId = "test-products",
            Offset = 0,
            Limit = 100,
            Sort =
            {
                new SortOrder
                {
                    Column = "Price",
                    Direction = Ivy.Protos.DataTable.SortDirection.Desc
                }
            }
        };

        // Act
        var result = processor.ProcessQuery(queryable, query);

        // Assert
        var batch = ArrowTestHelper.ParseArrowData(result.ArrowData);
        var prices = ArrowTestHelper.GetColumnValues(batch, "Price")
            .Cast<decimal>()
            .ToList();

        // Verify descending order
        for (int i = 1; i < prices.Count; i++)
        {
            Assert.True(prices[i - 1] >= prices[i],
                $"Prices not in descending order: {prices[i - 1]} should be >= {prices[i]}");
        }

        _output.WriteLine($"Price range: {prices.First():C} to {prices.Last():C}");
    }

    [Fact]
    public void Query_WithMultipleSorts_AppliesInOrder()
    {
        // Arrange
        var people = TestDataGenerator.GeneratePeople(30);
        var queryable = people.AsQueryable();
        var processor = new QueryProcessor(_logger);

        var query = new DataTableQuery
        {
            SourceId = "test-people",
            Offset = 0,
            Limit = 100,
            Sort =
            {
                new SortOrder
                {
                    Column = "Department",
                    Direction = Ivy.Protos.DataTable.SortDirection.Asc
                },
                new SortOrder
                {
                    Column = "Age",
                    Direction = Ivy.Protos.DataTable.SortDirection.Desc
                }
            }
        };

        // Act
        var result = processor.ProcessQuery(queryable, query);

        // Assert
        var batch = ArrowTestHelper.ParseArrowData(result.ArrowData);
        var rows = ArrowTestHelper.GetAllRows(batch);

        // Verify Department is primary sort, Age is secondary
        string? lastDept = null;
        int? lastAge = null;

        foreach (var row in rows)
        {
            var dept = row["Department"] as string;
            var age = Convert.ToInt32(row["Age"]);

            if (lastDept != null && dept == lastDept)
            {
                // Within same department, age should be descending
                Assert.True(lastAge >= age,
                    $"Within {dept}, age {age} should be <= {lastAge}");
            }
            else if (lastDept != null)
            {
                // Department changed, should be alphabetically after
                Assert.True(string.Compare(lastDept, dept, StringComparison.Ordinal) <= 0,
                    $"Department {dept} should come after {lastDept}");
            }

            lastDept = dept;
            lastAge = age;
        }

        _output.WriteLine("Multi-sort verified: Department ASC, then Age DESC");
    }

    [Fact]
    public void Query_WithComplexFilters_CombinesLogically()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(50);
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(_logger);

        // Find products with price > 500 AND stock < 50
        var query = new DataTableQuery
        {
            SourceId = "test-products",
            Offset = 0,
            Limit = 100,
            Filter = new Filter
            {
                Group = new FilterGroup
                {
                    Op = FilterGroup.Types.LogicalOperator.And,
                    Filters =
                    {
                        new Filter
                        {
                            Condition = new Condition
                            {
                                Column = "Price",
                                Function = "greaterThan",
                                Args = { Any.Pack(new DoubleValue { Value = 500.0 }) }
                            }
                        },
                        new Filter
                        {
                            Condition = new Condition
                            {
                                Column = "StockQuantity",
                                Function = "lessThan",
                                Args = { Any.Pack(new Int32Value { Value = 50 }) }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var result = processor.ProcessQuery(queryable, query);

        // Assert
        var batch = ArrowTestHelper.ParseArrowData(result.ArrowData);
        var rows = ArrowTestHelper.GetAllRows(batch);

        foreach (var row in rows)
        {
            var price = Convert.ToDecimal(row["Price"]);
            var stock = Convert.ToInt32(row["StockQuantity"]);

            Assert.True(price > 500m, $"Price {price} should be > 500");
            Assert.True(stock < 50, $"Stock {stock} should be < 50");
        }

        _output.WriteLine($"Found {result.RowCount} products with price > 500 AND stock < 50");
    }
}
