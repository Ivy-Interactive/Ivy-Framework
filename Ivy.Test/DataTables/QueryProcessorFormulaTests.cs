using Google.Protobuf.WellKnownTypes;
using Ivy.Protos.DataTable;
using Ivy.Test.DataTables.TestHelpers;
using Ivy.Views.DataTables;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using Any = Google.Protobuf.WellKnownTypes.Any;

namespace Ivy.Test.DataTables;

public class QueryProcessorFormulaTests
{
    private readonly ITestOutputHelper _output;

    public QueryProcessorFormulaTests(ITestOutputHelper output)
    {
        _output = output;
    }

    #region NotContains Tests

    [Fact]
    public void Filter_NotContains_ExcludesMatchingRows()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(10);
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        var filter = new Filter
        {
            Condition = new Condition
            {
                Column = "Name",
                Function = "notcontains",
                Args = { Any.Pack(new StringValue { Value = "5" }) }
            }
        };

        var query = new DataTableQuery
        {
            SourceId = "test-products",
            Filter = filter,
            Offset = 0,
            Limit = 100
        };

        // Act
        var result = processor.ProcessQuery(queryable, query);

        // Assert
        Assert.Equal(9, result.TotalRows); // Should exclude "Product 5"

        var batch = ArrowTestHelper.ParseArrowData(result.ArrowData);
        var names = ArrowTestHelper.GetColumnValues(batch, "Name").Cast<string>().ToList();
        Assert.DoesNotContain("Product 5", names);
        Assert.Contains("Product 1", names);
        Assert.Contains("Product 6", names);
    }

    #endregion

    #region Blank/NotBlank Tests

    [Fact]
    public void Filter_Blank_ReturnsNullOrEmptyValues()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(10);
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        var filter = new Filter
        {
            Condition = new Condition
            {
                Column = "Description",
                Function = "blank",
                Args = { } // No arguments needed
            }
        };

        var query = new DataTableQuery
        {
            SourceId = "test-products",
            Filter = filter,
            Offset = 0,
            Limit = 100
        };

        // Act
        var result = processor.ProcessQuery(queryable, query);

        // Assert
        // Products with i % 3 == 0 have null descriptions
        var batch = ArrowTestHelper.ParseArrowData(result.ArrowData);
        var ids = ArrowTestHelper.GetColumnValues(batch, "Id").Cast<int>().ToList();

        foreach (var id in ids)
        {
            Assert.True(id % 3 == 0, $"Product {id} should have null description");
        }
    }

    [Fact]
    public void Filter_NotBlank_ReturnsNonNullValues()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(10);
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        var filter = new Filter
        {
            Condition = new Condition
            {
                Column = "Description",
                Function = "notblank",
                Args = { } // No arguments needed
            }
        };

        var query = new DataTableQuery
        {
            SourceId = "test-products",
            Filter = filter,
            Offset = 0,
            Limit = 100
        };

        // Act
        var result = processor.ProcessQuery(queryable, query);

        // Assert
        var batch = ArrowTestHelper.ParseArrowData(result.ArrowData);
        var descriptions = ArrowTestHelper.GetColumnValues(batch, "Description").ToList();

        foreach (var desc in descriptions)
        {
            Assert.NotNull(desc);
            Assert.NotEqual(string.Empty, desc);
        }
    }

    [Fact]
    public void Filter_Blank_OnStringField_DetectsEmptyStrings()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Description = "Has description", CreatedDate = DateTime.Now.AddDays(-1) },
            new Product { Id = 2, Name = "Product 2", Description = "", CreatedDate = DateTime.Now.AddDays(-2) }, // Empty string
            new Product { Id = 3, Name = "Product 3", Description = null, CreatedDate = DateTime.Now.AddDays(-3) }, // Null
            new Product { Id = 4, Name = "Product 4", Description = "Another description", CreatedDate = DateTime.Now.AddDays(-4) }
        };
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        var filter = new Filter
        {
            Condition = new Condition
            {
                Column = "Description",
                Function = "blank",
                Args = { }
            }
        };

        var query = new DataTableQuery
        {
            SourceId = "test-products",
            Filter = filter,
            Offset = 0,
            Limit = 100
        };

        // Act
        var result = processor.ProcessQuery(queryable, query);

        // Assert
        Assert.Equal(2, result.TotalRows); // Products 2 and 3

        var batch = ArrowTestHelper.ParseArrowData(result.ArrowData);
        var ids = ArrowTestHelper.GetColumnValues(batch, "Id").Cast<int>().ToList();
        Assert.Contains(2, ids);
        Assert.Contains(3, ids);
    }

    #endregion

    #region InRange Tests

    [Fact]
    public void Filter_InRange_ForNumbers_ReturnsValuesInRange()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(10);

        // Debug: See what prices we're starting with
        _output.WriteLine("Original prices:");
        foreach (var p in products)
        {
            _output.WriteLine($"Product {p.Id}: {p.Price}");
        }

        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        var filter = new Filter
        {
            Condition = new Condition
            {
                Column = "Price",
                Function = "inrange",
                Args =
                {
                    Any.Pack(new DoubleValue { Value = 25.0 }), // Lower bound
                    Any.Pack(new DoubleValue { Value = 75.0 })  // Upper bound
                }
            }
        };

        var query = new DataTableQuery
        {
            SourceId = "test-products",
            Filter = filter,
            Offset = 0,
            Limit = 100
        };

        // Act
        var result = processor.ProcessQuery(queryable, query);

        // Debug: Check what we actually got
        _output.WriteLine($"Total rows returned: {result.TotalRows}");
        _output.WriteLine($"Row count: {result.RowCount}");

        // Assert
        var batch = ArrowTestHelper.ParseArrowData(result.ArrowData);
        var prices = ArrowTestHelper.GetColumnValues(batch, "Price").Cast<decimal>().ToList();

        _output.WriteLine($"Prices returned: {string.Join(", ", prices)}");

        foreach (var price in prices)
        {
            Assert.True(price >= 25.0m && price <= 75.0m, $"Price {price} should be in range [25, 75]");
        }
    }

    [Fact]
    public void Filter_InRange_ForDates_ReturnsValuesInRange()
    {
        // Arrange
        var baseDate = new DateTime(2024, 1, 1);
        var products = TestDataGenerator.GenerateProducts(10).ToList();

        // Set specific dates for testing
        for (int i = 0; i < products.Count; i++)
        {
            products[i].CreatedDate = baseDate.AddDays(i * 10); // 0, 10, 20, ... days from base
        }

        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        var filter = new Filter
        {
            Condition = new Condition
            {
                Column = "CreatedDate",
                Function = "inrange",
                Args =
                {
                    Any.Pack(new StringValue { Value = "2024-01-10" }), // Lower bound
                    Any.Pack(new StringValue { Value = "2024-01-31" })  // Upper bound
                }
            }
        };

        var query = new DataTableQuery
        {
            SourceId = "test-products",
            Filter = filter,
            Offset = 0,
            Limit = 100
        };

        // Act
        var result = processor.ProcessQuery(queryable, query);

        // Assert
        var batch = ArrowTestHelper.ParseArrowData(result.ArrowData);
        var dates = ArrowTestHelper.GetColumnValues(batch, "CreatedDate").Cast<DateTime>().ToList();

        var lowerBound = new DateTime(2024, 1, 10);
        var upperBound = new DateTime(2024, 1, 31);

        foreach (var date in dates)
        {
            Assert.True(date >= lowerBound && date <= upperBound,
                $"Date {date:yyyy-MM-dd} should be in range [2024-01-10, 2024-01-31]");
        }
    }

    [Fact]
    public void Filter_InRange_WithInsufficientArgs_ReturnsNull()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(10);
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        var filter = new Filter
        {
            Condition = new Condition
            {
                Column = "Price",
                Function = "inrange",
                Args = { Any.Pack(new DoubleValue { Value = 25.0 }) } // Only one argument
            }
        };

        var query = new DataTableQuery
        {
            SourceId = "test-products",
            Filter = filter,
            Offset = 0,
            Limit = 100
        };

        // Act
        var result = processor.ProcessQuery(queryable, query);

        // Assert - should return all rows as the filter is invalid
        Assert.Equal(10, result.TotalRows);
    }

    #endregion

    #region Before/After (Date Aliases) Tests

    [Fact]
    public void Filter_Before_ReturnsEarlierDates()
    {
        // Arrange
        var baseDate = new DateTime(2024, 1, 1);
        var products = TestDataGenerator.GenerateProducts(10).ToList();

        for (int i = 0; i < products.Count; i++)
        {
            products[i].CreatedDate = baseDate.AddDays(i * 5);
        }

        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        var filter = new Filter
        {
            Condition = new Condition
            {
                Column = "CreatedDate",
                Function = "before", // Alias for lessThan
                Args = { Any.Pack(new StringValue { Value = "2024-01-15" }) }
            }
        };

        var query = new DataTableQuery
        {
            SourceId = "test-products",
            Filter = filter,
            Offset = 0,
            Limit = 100
        };

        // Act
        var result = processor.ProcessQuery(queryable, query);

        // Assert
        var batch = ArrowTestHelper.ParseArrowData(result.ArrowData);
        var dates = ArrowTestHelper.GetColumnValues(batch, "CreatedDate").Cast<DateTime>().ToList();
        var cutoffDate = new DateTime(2024, 1, 15);

        foreach (var date in dates)
        {
            Assert.True(date < cutoffDate, $"Date {date:yyyy-MM-dd} should be before 2024-01-15");
        }
    }

    [Fact]
    public void Filter_After_ReturnsLaterDates()
    {
        // Arrange
        var baseDate = new DateTime(2024, 1, 1);
        var products = TestDataGenerator.GenerateProducts(10).ToList();

        for (int i = 0; i < products.Count; i++)
        {
            products[i].CreatedDate = baseDate.AddDays(i * 5);
        }

        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        var filter = new Filter
        {
            Condition = new Condition
            {
                Column = "CreatedDate",
                Function = "after", // Alias for greaterThan
                Args = { Any.Pack(new StringValue { Value = "2024-01-15" }) }
            }
        };

        var query = new DataTableQuery
        {
            SourceId = "test-products",
            Filter = filter,
            Offset = 0,
            Limit = 100
        };

        // Act
        var result = processor.ProcessQuery(queryable, query);

        // Assert
        var batch = ArrowTestHelper.ParseArrowData(result.ArrowData);
        var dates = ArrowTestHelper.GetColumnValues(batch, "CreatedDate").Cast<DateTime>().ToList();
        var cutoffDate = new DateTime(2024, 1, 15);

        foreach (var date in dates)
        {
            Assert.True(date > cutoffDate, $"Date {date:yyyy-MM-dd} should be after 2024-01-15");
        }
    }

    #endregion

    #region Complex Combined Filter Tests

    [Fact]
    public void Filter_ComplexCombination_WithAndOrNot_WorksCorrectly()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(20).ToList();
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        // Create filter: (Price in range [20, 60] AND Category not contains "B") OR Description is blank
        var filter = new Filter
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
                                        Column = "Price",
                                        Function = "inrange",
                                        Args =
                                        {
                                            Any.Pack(new DoubleValue { Value = 20.0 }),
                                            Any.Pack(new DoubleValue { Value = 60.0 })
                                        }
                                    }
                                },
                                new Filter
                                {
                                    Condition = new Condition
                                    {
                                        Column = "Category",
                                        Function = "notcontains",
                                        Args = { Any.Pack(new StringValue { Value = "B" }) }
                                    }
                                }
                            }
                        }
                    },
                    new Filter
                    {
                        Condition = new Condition
                        {
                            Column = "Description",
                            Function = "blank",
                            Args = { }
                        }
                    }
                }
            }
        };

        var query = new DataTableQuery
        {
            SourceId = "test-products",
            Filter = filter,
            Offset = 0,
            Limit = 100
        };

        // Act
        var result = processor.ProcessQuery(queryable, query);

        // Assert
        var batch = ArrowTestHelper.ParseArrowData(result.ArrowData);
        var allRows = ArrowTestHelper.GetAllRows(batch).ToList();

        foreach (var row in allRows)
        {
            var price = Convert.ToDecimal(row["Price"]);
            var category = row["Category"]?.ToString() ?? "";
            var description = row["Description"]?.ToString();

            bool priceInRange = price >= 20m && price <= 60m;
            bool categoryNotContainsB = !category.Contains("B", StringComparison.OrdinalIgnoreCase);
            bool descriptionIsBlank = string.IsNullOrEmpty(description);

            bool shouldMatch = (priceInRange && categoryNotContainsB) || descriptionIsBlank;

            Assert.True(shouldMatch,
                $"Row with Price={price}, Category={category}, Description={description ?? "null"} should match filter");
        }
    }

    [Fact]
    public void Filter_WithNegation_InvertsCondition()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(10);
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        var filter = new Filter
        {
            Condition = new Condition
            {
                Column = "Price",
                Function = "greaterthan",
                Args = { Any.Pack(new DoubleValue { Value = 500.0 }) } // Use 500 instead of 50
            },
            Negate = true // Negate the condition, so it becomes "NOT (Price > 500)"
        };

        var query = new DataTableQuery
        {
            SourceId = "test-products",
            Filter = filter,
            Offset = 0,
            Limit = 100
        };

        // Act
        var result = processor.ProcessQuery(queryable, query);

        // Assert
        var batch = ArrowTestHelper.ParseArrowData(result.ArrowData);
        var prices = ArrowTestHelper.GetColumnValues(batch, "Price").Cast<decimal>().ToList();

        foreach (var price in prices)
        {
            Assert.True(price <= 500m, $"Price {price} should be <= 500 (negated > 500)");
        }
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Filter_BlankOnNonNullableValueType_ReturnsFalse()
    {
        // Arrange
        var products = TestDataGenerator.GenerateProducts(10);
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        var filter = new Filter
        {
            Condition = new Condition
            {
                Column = "Id", // Non-nullable int
                Function = "blank",
                Args = { }
            }
        };

        var query = new DataTableQuery
        {
            SourceId = "test-products",
            Filter = filter,
            Offset = 0,
            Limit = 100
        };

        // Act
        var result = processor.ProcessQuery(queryable, query);

        // Assert
        Assert.Equal(0, result.TotalRows); // No rows should match as Id can't be blank
    }

    [Fact]
    public void Filter_NotBlankOnNullableField_HandlesCorrectly()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Rating = 4.5, CreatedDate = DateTime.Now.AddDays(-1) },
            new Product { Id = 2, Name = "Product 2", Rating = null, CreatedDate = DateTime.Now.AddDays(-2) },
            new Product { Id = 3, Name = "Product 3", Rating = 3.2, CreatedDate = DateTime.Now.AddDays(-3) },
            new Product { Id = 4, Name = "Product 4", Rating = null, CreatedDate = DateTime.Now.AddDays(-4) }
        };
        var queryable = products.AsQueryable();
        var processor = new QueryProcessor(logger: null);

        var filter = new Filter
        {
            Condition = new Condition
            {
                Column = "Rating",
                Function = "notblank",
                Args = { }
            }
        };

        var query = new DataTableQuery
        {
            SourceId = "test-products",
            Filter = filter,
            Offset = 0,
            Limit = 100
        };

        // Act
        var result = processor.ProcessQuery(queryable, query);

        // Assert
        Assert.Equal(2, result.TotalRows); // Products 1 and 3

        var batch = ArrowTestHelper.ParseArrowData(result.ArrowData);
        var ratings = ArrowTestHelper.GetColumnValues(batch, "Rating").ToList();

        foreach (var rating in ratings)
        {
            Assert.NotNull(rating);
        }
    }

    #endregion
}