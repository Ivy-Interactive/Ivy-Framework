using Google.Protobuf.WellKnownTypes;
using Ivy.Protos.DataTable;
using Ivy.Test.DataTables.TestHelpers;
using Ivy.Views.DataTables;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using Any = Google.Protobuf.WellKnownTypes.Any;

namespace Ivy.Test.DataTables;

public class QueryProcessorTests
{
    private readonly ITestOutputHelper _output;

    public QueryProcessorTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void SimpleQuery_NoFiltersOrSorting_ReturnsAllData()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(10);
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

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
        var processor = new QueryProcessor(logger: null);

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
        var processor = new QueryProcessor(logger: null);

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
        var processor = new QueryProcessor(logger: null);

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
        var processor = new QueryProcessor(logger: null);

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
        var processor = new QueryProcessor(logger: null);

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

    // TODO: Fix decimal conversion issue with Apache Arrow's SqlDecimal representation
    // The SqlDecimal values from Arrow are stored as unscaled integers with separate scale metadata
    // [Fact]
    // public void Query_WithOrFilter_CombinesLogically()
    // {
    //     // Arrange
    //     var products = TestDataGenerator.GenerateProducts(30);
    //     var queryable = products.AsQueryable();
    //     var processor = new QueryProcessor(_logger);

    //     // Find products in Electronics OR price < 100
    //     var query = new DataTableQuery
    //     {
    //         SourceId = "test-products",
    //         Offset = 0,
    //         Limit = 100,
    //         Filter = new Filter
    //         {
    //             Group = new FilterGroup
    //             {
    //                 Op = FilterGroup.Types.LogicalOperator.Or,
    //                 Filters =
    //                 {
    //                     new Filter
    //                     {
    //                         Condition = new Condition
    //                         {
    //                             Column = "Category",
    //                             Function = "equals",
    //                             Args = { Any.Pack(new StringValue { Value = "Electronics" }) }
    //                         }
    //                     },
    //                     new Filter
    //                     {
    //                         Condition = new Condition
    //                         {
    //                             Column = "Price",
    //                             Function = "lessThan",
    //                             Args = { Any.Pack(new DoubleValue { Value = 100.0 }) }
    //                         }
    //                     }
    //                 }
    //             }
    //         }
    //     };

    //     // Act
    //     var result = processor.ProcessQuery(queryable, query);

    //     // Assert
    //     var batch = ArrowTestHelper.ParseArrowData(result.ArrowData);
    //     var rows = ArrowTestHelper.GetAllRows(batch);

    //     foreach (var row in rows)
    //     {
    //         var category = row["Category"] as string;
    //         var price = Convert.ToDecimal(row["Price"]);

    //         Assert.True(category == "Electronics" || price < 100m,
    //             $"Product should be Electronics OR price < 100, but got {category} with price {price}");
    //     }

    //     _output.WriteLine($"Found {result.RowCount} products that are Electronics OR price < 100");
    // }

    [Fact]
    public void Query_WithNestedFilterGroups_AppliesCorrectLogic()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(50);
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        // (Category = Electronics AND Price > 500) OR (Category = Home AND StockQuantity > 75)
        var query = new DataTableQuery
        {
            SourceId = "test-products",
            Offset = 0,
            Limit = 100,
            Filter = new Filter
            {
                Group = new FilterGroup
                {
                    Op = FilterGroup.Types.LogicalOperator.Or,
                    Filters =
                    {
                        new Filter
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
                                            Column = "Category",
                                            Function = "equals",
                                            Args = { Any.Pack(new StringValue { Value = "Electronics" }) }
                                        }
                                    },
                                    new Filter
                                    {
                                        Condition = new Condition
                                        {
                                            Column = "Price",
                                            Function = "greaterThan",
                                            Args = { Any.Pack(new DoubleValue { Value = 500.0 }) }
                                        }
                                    }
                                }
                            }
                        },
                        new Filter
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
                                            Column = "Category",
                                            Function = "equals",
                                            Args = { Any.Pack(new StringValue { Value = "Home" }) }
                                        }
                                    },
                                    new Filter
                                    {
                                        Condition = new Condition
                                        {
                                            Column = "StockQuantity",
                                            Function = "greaterThan",
                                            Args = { Any.Pack(new Int32Value { Value = 75 }) }
                                        }
                                    }
                                }
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
            var category = row["Category"] as string;
            var price = Convert.ToDecimal(row["Price"]);
            var stock = Convert.ToInt32(row["StockQuantity"]);

            var matchesFirst = category == "Electronics" && price > 500m;
            var matchesSecond = category == "Home" && stock > 75;

            Assert.True(matchesFirst || matchesSecond,
                $"Product should match nested filter criteria but got {category}, price={price}, stock={stock}");
        }

        _output.WriteLine($"Found {result.RowCount} products matching nested filter groups");
    }

    [Fact]
    public void Query_WithStringFilterFunctions_AppliesCorrectly()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(20);
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        // Test contains function
        var query = new DataTableQuery
        {
            SourceId = "test-products",
            Offset = 0,
            Limit = 100,
            Filter = new Filter
            {
                Condition = new Condition
                {
                    Column = "Name",
                    Function = "contains",
                    Args = { Any.Pack(new StringValue { Value = "5" }) }
                }
            }
        };

        // Act
        var result = processor.ProcessQuery(queryable, query);

        // Assert
        var batch = ArrowTestHelper.ParseArrowData(result.ArrowData);
        var names = ArrowTestHelper.GetColumnValues(batch, "Name").Cast<string>().ToList();

        Assert.All(names, name => Assert.Contains("5", name));
        _output.WriteLine($"Found {result.RowCount} products with '5' in name");

        // Test startsWith
        query.Filter.Condition.Function = "startsWith";
        query.Filter.Condition.Args.Clear();
        query.Filter.Condition.Args.Add(Any.Pack(new StringValue { Value = "Product 1" }));

        result = processor.ProcessQuery(queryable, query);
        batch = ArrowTestHelper.ParseArrowData(result.ArrowData);
        names = ArrowTestHelper.GetColumnValues(batch, "Name").Cast<string>().ToList();

        Assert.All(names, name => Assert.StartsWith("Product 1", name));
        _output.WriteLine($"Found {result.RowCount} products starting with 'Product 1'");

        // Test endsWith
        query.Filter.Condition.Function = "endsWith";
        query.Filter.Condition.Args.Clear();
        query.Filter.Condition.Args.Add(Any.Pack(new StringValue { Value = "0" }));

        result = processor.ProcessQuery(queryable, query);
        batch = ArrowTestHelper.ParseArrowData(result.ArrowData);
        names = ArrowTestHelper.GetColumnValues(batch, "Name").Cast<string>().ToList();

        Assert.All(names, name => Assert.EndsWith("0", name));
        _output.WriteLine($"Found {result.RowCount} products ending with '0'");

        // Test notEquals
        query.Filter.Condition.Function = "notEquals";
        query.Filter.Condition.Args.Clear();
        query.Filter.Condition.Args.Add(Any.Pack(new StringValue { Value = "Product 1" }));

        result = processor.ProcessQuery(queryable, query);
        batch = ArrowTestHelper.ParseArrowData(result.ArrowData);
        names = ArrowTestHelper.GetColumnValues(batch, "Name").Cast<string>().ToList();

        Assert.All(names, name => Assert.NotEqual("Product 1", name));
        Assert.Equal(19, result.RowCount); // Should be all except Product 1
    }

    [Fact]
    public void Query_WithNullValueFiltering_HandlesCorrectly()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(30);
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        // Filter for null descriptions (every 3rd product has null description)
        var query = new DataTableQuery
        {
            SourceId = "test-products",
            Offset = 0,
            Limit = 100,
            Filter = new Filter
            {
                Condition = new Condition
                {
                    Column = "Description",
                    Function = "equals",
                    Args = { Any.Pack(new StringValue { Value = "" }) } // Empty string for null
                }
            }
        };

        // Act
        var result = processor.ProcessQuery(queryable, query);

        // Assert
        var batch = ArrowTestHelper.ParseArrowData(result.ArrowData);
        var descriptions = ArrowTestHelper.GetColumnValues(batch, "Description");

        Assert.All(descriptions, desc => Assert.Null(desc));
        Assert.Equal(10, result.RowCount); // Products 3, 6, 9, 12, 15, 18, 21, 24, 27, 30

        // Test not null
        query.Filter.Condition.Function = "notEquals";

        result = processor.ProcessQuery(queryable, query);
        batch = ArrowTestHelper.ParseArrowData(result.ArrowData);
        descriptions = ArrowTestHelper.GetColumnValues(batch, "Description");

        Assert.All(descriptions, desc => Assert.NotNull(desc));
        Assert.Equal(20, result.RowCount); // The other 20 products
    }

    [Fact]
    public void Query_WithEdgeCases_HandlesCorrectly()
    {
        // Test 1: Empty queryable
        var emptyQueryable = new List<Product>().AsQueryable();
        var processor = new QueryProcessor(logger: null);

        var query = new DataTableQuery
        {
            SourceId = "test-empty",
            Offset = 0,
            Limit = 10
        };

        var result = processor.ProcessQuery(emptyQueryable, query);
        Assert.Equal(0, result.RowCount);
        Assert.Equal(0, result.TotalRows);

        // Test 2: Zero limit
        var products = TestDataGenerator.GenerateProducts(10);
        var queryable = products.AsQueryable();

        query = new DataTableQuery
        {
            SourceId = "test-products",
            Offset = 0,
            Limit = 0
        };

        result = processor.ProcessQuery(queryable, query);
        Assert.Equal(0, result.RowCount);
        Assert.Equal(10, result.TotalRows);

        // Test 3: Offset beyond available data
        query = new DataTableQuery
        {
            SourceId = "test-products",
            Offset = 100,
            Limit = 10
        };

        result = processor.ProcessQuery(queryable, query);
        Assert.Equal(0, result.RowCount);
        Assert.Equal(10, result.TotalRows);
        Assert.Equal(100, result.Offset);

        // Test 4: Very large limit
        query = new DataTableQuery
        {
            SourceId = "test-products",
            Offset = 0,
            Limit = int.MaxValue
        };

        result = processor.ProcessQuery(queryable, query);
        Assert.Equal(10, result.RowCount);
        Assert.Equal(10, result.TotalRows);
    }

    // TODO: Fix field name mapping issue - CreatedDate field not appearing in Arrow schema
    // [Fact]
    // public void Query_WithDateTimeFiltering_HandlesCorrectly()
    // {
    //     // Arrange
    //     var products = TestDataGenerator.GenerateProducts(30);
    //     var queryable = products.AsQueryable();
    //     var processor = new QueryProcessor(_logger);

    //     var cutoffDate = DateTime.Now.AddDays(-15);

    //     // Filter for products created after cutoff date
    //     var query = new DataTableQuery
    //     {
    //         SourceId = "test-products",
    //         Offset = 0,
    //         Limit = 100,
    //         Filter = new Filter
    //         {
    //             Condition = new Condition
    //             {
    //                 Column = "CreatedDate",
    //                 Function = "greaterThan",
    //                 Args = { Any.Pack(new StringValue { Value = cutoffDate.ToString("O") }) }
    //             }
    //         }
    //     };

    //     // Act
    //     var result = processor.ProcessQuery(queryable, query);

    //     // Assert
    //     var batch = ArrowTestHelper.ParseArrowData(result.ArrowData);
    //     var dates = ArrowTestHelper.GetColumnValues(batch, "CreatedDate")
    //         .Cast<DateTime?>()
    //         .Where(d => d.HasValue)
    //         .Select(d => d!.Value)
    //         .ToList();

    //     Assert.All(dates, date => Assert.True(date > cutoffDate,
    //         $"Date {date} should be after {cutoffDate}"));

    //     _output.WriteLine($"Found {result.RowCount} products created after {cutoffDate}");

    //     // Test date range
    //     var startDate = DateTime.Now.AddDays(-20);
    //     var endDate = DateTime.Now.AddDays(-10);

    //     query.Filter = new Filter
    //     {
    //         Group = new FilterGroup
    //         {
    //             Op = FilterGroup.Types.LogicalOperator.And,
    //             Filters =
    //             {
    //                 new Filter
    //                 {
    //                     Condition = new Condition
    //                     {
    //                         Column = "CreatedDate",
    //                         Function = "greaterThanOrEqual",
    //                         Args = { Any.Pack(new StringValue { Value = startDate.ToString("O") }) }
    //                     }
    //                 },
    //                 new Filter
    //                 {
    //                     Condition = new Condition
    //                     {
    //                         Column = "CreatedDate",
    //                         Function = "lessThanOrEqual",
    //                         Args = { Any.Pack(new StringValue { Value = endDate.ToString("O") }) }
    //                     }
    //                 }
    //             }
    //         }
    //     };

    //     result = processor.ProcessQuery(queryable, query);
    //     batch = ArrowTestHelper.ParseArrowData(result.ArrowData);
    //     dates = ArrowTestHelper.GetColumnValues(batch, "CreatedAt")
    //         .Cast<DateTime?>()
    //         .Where(d => d.HasValue)
    //         .Select(d => d!.Value)
    //         .ToList();

    //     Assert.All(dates, date => Assert.True(date >= startDate && date <= endDate,
    //         $"Date {date} should be between {startDate} and {endDate}"));
    // }

    [Fact]
    public void Query_WithBooleanFiltering_HandlesCorrectly()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(20);
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        // Filter for active products
        var query = new DataTableQuery
        {
            SourceId = "test-products",
            Offset = 0,
            Limit = 100,
            Filter = new Filter
            {
                Condition = new Condition
                {
                    Column = "IsAvailable",
                    Function = "equals",
                    Args = { Any.Pack(new BoolValue { Value = true }) }
                }
            }
        };

        // Act
        var result = processor.ProcessQuery(queryable, query);

        // Assert
        var batch = ArrowTestHelper.ParseArrowData(result.ArrowData);
        var activeValues = ArrowTestHelper.GetColumnValues(batch, "IsAvailable")
            .Cast<bool>()
            .ToList();

        Assert.All(activeValues, isActive => Assert.True(isActive));

        // Count active vs inactive
        var totalActive = result.RowCount;

        // Test inactive products
        query.Filter.Condition.Args.Clear();
        query.Filter.Condition.Args.Add(Any.Pack(new BoolValue { Value = false }));

        result = processor.ProcessQuery(queryable, query);
        batch = ArrowTestHelper.ParseArrowData(result.ArrowData);
        activeValues = ArrowTestHelper.GetColumnValues(batch, "IsAvailable")
            .Cast<bool>()
            .ToList();

        Assert.All(activeValues, isActive => Assert.False(isActive));

        var totalInactive = result.RowCount;
        Assert.Equal(20, totalActive + totalInactive);

        _output.WriteLine($"Found {totalActive} active and {totalInactive} inactive products");
    }

    // TODO: Fix decimal conversion issue with Apache Arrow's SqlDecimal representation
    // SqlDecimal stores unscaled values that need proper scale application
    // [Fact]
    // public void Query_WithDecimalPrecision_PreservesAccuracy()
    // {
    //     // Arrange
    //     var products = new List<Product>
    //     {
    //         new Product { Id = 1, Name = "Product 1", Price = 123.45m, Category = "Test", CreatedDate = DateTime.Now },
    //         new Product { Id = 2, Name = "Product 2", Price = 999.999m, Category = "Test", CreatedDate = DateTime.Now },
    //         new Product { Id = 3, Name = "Product 3", Price = 0.01m, Category = "Test", CreatedDate = DateTime.Now },
    //         new Product { Id = 4, Name = "Product 4", Price = 1234567.89m, Category = "Test", CreatedDate = DateTime.Now },
    //         new Product { Id = 5, Name = "Product 5", Price = 0.123456789m, Category = "Test", CreatedDate = DateTime.Now }
    //     };
    //     var queryable = products.AsQueryable();
    //     var processor = new QueryProcessor(_logger);

    //     var query = new DataTableQuery
    //     {
    //         SourceId = "test-precision",
    //         Offset = 0,
    //         Limit = 100
    //     };

    //     // Act
    //     var result = processor.ProcessQuery(queryable, query);

    //     // Assert
    //     var batch = ArrowTestHelper.ParseArrowData(result.ArrowData);
    //     var prices = ArrowTestHelper.GetColumnValues(batch, "Price")
    //         .Cast<decimal>()
    //         .ToList();

    //     Assert.Equal(123.45m, prices[0]);
    //     Assert.Equal(999.999m, prices[1]);
    //     Assert.Equal(0.01m, prices[2]);
    //     Assert.Equal(1234567.89m, prices[3]);
    //     Assert.Equal(0.123456789m, prices[4]);

    //     _output.WriteLine("Decimal precision preserved in Arrow conversion");
    //     foreach (var (original, converted) in products.Zip(prices, (p, c) => (p.Price, c)))
    //     {
    //         _output.WriteLine($"Original: {original}, Converted: {converted}");
    //     }
    // }

    [Fact]
    public void Query_WithInvalidColumnName_ThrowsException()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(10);
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        var query = new DataTableQuery
        {
            SourceId = "test-products",
            Offset = 0,
            Limit = 100,
            Filter = new Filter
            {
                Condition = new Condition
                {
                    Column = "NonExistentColumn",
                    Function = "equals",
                    Args = { Any.Pack(new StringValue { Value = "test" }) }
                }
            }
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            processor.ProcessQuery(queryable, query));

        Assert.Contains("NonExistentColumn", exception.Message);
        _output.WriteLine($"Expected error for invalid column: {exception.Message}");
    }

    [Fact]
    public void Query_WithInvalidFilterFunction_ThrowsException()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(10);
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        var query = new DataTableQuery
        {
            SourceId = "test-products",
            Offset = 0,
            Limit = 100,
            Filter = new Filter
            {
                Condition = new Condition
                {
                    Column = "Name",
                    Function = "invalidFunction",
                    Args = { Any.Pack(new StringValue { Value = "test" }) }
                }
            }
        };

        // Act & Assert
        var exception = Assert.Throws<NotSupportedException>(() =>
            processor.ProcessQuery(queryable, query));

        Assert.Contains("invalidFunction", exception.Message);
        _output.WriteLine($"Expected error for invalid function: {exception.Message}");
    }
}
