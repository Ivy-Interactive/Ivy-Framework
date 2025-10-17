using Ivy.Filters;

namespace Ivy.Filters.Tests;

public class FilterParserTests
{
    private readonly FilterParser _parser;
    private readonly FieldMeta[] _testColumns;

    public FilterParserTests()
    {
        // Set up test columns as specified in the PRD
        _testColumns = new[]
        {
            new FieldMeta("Age", "age", FieldType.Number),
            new FieldMeta("Sport", "sport", FieldType.Text),
            new FieldMeta("Country", "country", FieldType.Text),
            new FieldMeta("Price", "price", FieldType.Number),
            new FieldMeta("Start Date", "startDate", FieldType.Date),
            new FieldMeta("Name", "name", FieldType.Text),
            new FieldMeta("Date", "date", FieldType.Date),
            new FieldMeta("X", "x", FieldType.Text),
            new FieldMeta("IsActive", "isActive", FieldType.Boolean)
        };

        _parser = new FilterParser(_testColumns);
    }

    [Theory]
    [InlineData("[Age] > 23")]
    [InlineData("[Country] is blank")]
    [InlineData("[Name] is not blank")]
    public void Parse_ValidBasicFilters_ShouldSucceed(string filter)
    {
        // Act
        var result = _parser.Parse(filter);

        // Assert
        Assert.False(result.HasErrors, $"Filter '{filter}' should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);
        Assert.NotNull(result.Model);
    }

    [Theory]
    [InlineData("[Sport] ends with \"ing\"")]
    [InlineData("[Name] contains \"test\"")]
    [InlineData("[Country] starts with \"US\"")]
    [InlineData("[Sport] not contains \"ball\"")]
    public void Parse_ValidTextOperations_ShouldSucceed(string filter)
    {
        // Act
        var result = _parser.Parse(filter);

        // Assert
        Assert.False(result.HasErrors, $"Filter '{filter}' should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);
        Assert.IsType<Leaf>(result.Ast);

        var leaf = (Leaf)result.Ast;
        Assert.Equal(FieldType.Text, leaf.Type);
    }

    [Theory]
    [InlineData("[Age] = 25")]
    [InlineData("[Age] == 25")]
    [InlineData("[Age] != 30")]
    [InlineData("[Price] >= 100")]
    [InlineData("[Price] <= 500")]
    [InlineData("[Age] greater than 18")]
    [InlineData("[Price] less than or equal 1000")]
    public void Parse_ValidNumberComparisons_ShouldSucceed(string filter)
    {
        // Act
        var result = _parser.Parse(filter);

        // Assert
        Assert.False(result.HasErrors, $"Filter '{filter}' should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);
        Assert.IsType<Leaf>(result.Ast);

        var leaf = (Leaf)result.Ast;
        Assert.Equal(FieldType.Number, leaf.Type);
    }

    [Theory]
    [InlineData("[Age] > 25 AND [Country] = \"USA\"")]
    [InlineData("[Sport] contains \"ball\" OR [Price] < 50")]
    [InlineData("([Age] >= 18 AND [Age] <= 65) AND [Country] is blank")]
    [InlineData("[Name] is not blank OR ([Sport] ends with \"ing\" AND [Price] > 0)")]
    public void Parse_ValidLogicalOperations_ShouldSucceed(string filter)
    {
        // Act
        var result = _parser.Parse(filter);

        // Assert
        Assert.False(result.HasErrors, $"Filter '{filter}' should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);

        // Should be a logical operation (And/Or)
        Assert.True(result.Ast is And or Or, $"Expected logical operation, got {result.Ast.GetType().Name}");
    }

    [Theory]
    [InlineData("NOT [Country] is blank")]
    [InlineData("NOT ([Age] > 25 AND [Sport] contains \"ball\")")]
    public void Parse_ValidNegationOperations_ShouldSucceed(string filter)
    {
        // Act
        var result = _parser.Parse(filter);

        // Assert
        Assert.False(result.HasErrors, $"Filter '{filter}' should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);
        Assert.IsType<Not>(result.Ast);
    }

    [Fact]
    public void Parse_ComplexNestedFilter_ShouldSucceed()
    {
        // Arrange
        var filter = "([Age] > 23 OR [Sport] ends with \"ing\") AND [Country] contains \"united\"";

        // Act
        var result = _parser.Parse(filter);

        // Assert
        Assert.False(result.HasErrors, $"Complex filter should parse successfully. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Ast);
        Assert.IsType<And>(result.Ast);

        var and = (And)result.Ast;
        Assert.IsType<Or>(and.Left);
        Assert.IsType<Leaf>(and.Right);
    }

    [Theory]
    [InlineData("[Age] contains \"12\"", "text operation on number column")]
    [InlineData("[Sport] > \"test\"", "comparison operation on text column")]
    [InlineData("[Country] in range \"A\" AND \"Z\"", "range operation on text column")]
    public void Parse_InvalidSemanticFilters_ShouldFail(string filter, string reason)
    {
        // Act
        var result = _parser.Parse(filter);

        // Assert
        Assert.True(result.HasErrors, $"Filter '{filter}' should fail semantic validation ({reason})");
        Assert.Contains(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    [Theory]
    [InlineData("[UnknownColumn] = 5", "unknown column")]
    [InlineData("[Price] in range 10 AND", "missing range operand")]
    [InlineData("[Age] equals", "missing operand")]
    public void Parse_InvalidSemanticFilters_WithSpecificErrors_ShouldFail(string filter, string expectedErrorType)
    {
        // Act
        var result = _parser.Parse(filter);

        // Assert
        Assert.True(result.HasErrors, $"Filter '{filter}' should fail ({expectedErrorType})");
        Assert.NotEmpty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
    }

    [Theory]
    [InlineData("[Age > 23", "missing closing bracket")]
    [InlineData("\"unterminated string", "unterminated string literal")]
    [InlineData("([Age] = 25 OR )", "incomplete logical expression")]
    [InlineData("[Age] = = 25", "double equals")]
    public void Parse_InvalidSyntaxFilters_ShouldFail(string filter, string reason)
    {
        // Act
        var result = _parser.Parse(filter);

        // Assert
        Assert.True(result.HasErrors, $"Filter '{filter}' should fail syntax validation ({reason})");
        Assert.Contains(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Parse_EscapedStringLiterals_ShouldHandleCorrectly()
    {
        // Arrange
        var filter = "[Name] contains \"test\\\"quote\"";

        // Act
        var result = _parser.Parse(filter);

        // Assert
        Assert.False(result.HasErrors, "Filter with escaped quotes should parse successfully");
        Assert.NotNull(result.Ast);
        Assert.IsType<Leaf>(result.Ast);

        var leaf = (Leaf)result.Ast;
        Assert.Equal("test\"quote", leaf.A); // Escaped quote should be unescaped in the value
    }

    [Fact]
    public void Parse_ColumnsWithSpaces_ShouldWork()
    {
        // Arrange
        var filter = "[Start Date] > \"2024-01-01\"";

        // Act
        var result = _parser.Parse(filter);

        // Assert
        Assert.False(result.HasErrors, "Columns with spaces should work");
        Assert.NotNull(result.Ast);
        Assert.IsType<Leaf>(result.Ast);

        var leaf = (Leaf)result.Ast;
        Assert.Equal("Start Date", leaf.FieldDisplay);
        Assert.Equal("startDate", leaf.FieldId);
    }

    [Fact]
    public void IsValid_ValidFilter_ReturnsTrue()
    {
        // Act & Assert
        Assert.True(_parser.IsValid("[Age] > 25"));
    }

    [Fact]
    public void IsValid_InvalidFilter_ReturnsFalse()
    {
        // Act & Assert
        Assert.False(_parser.IsValid("[Age > 25")); // Missing closing bracket
    }

    [Fact]
    public void GetAvailableColumns_ReturnsAllColumns()
    {
        // Act
        var columns = _parser.GetAvailableFields().ToList();

        // Assert
        Assert.Equal(_testColumns.Length, columns.Count);
        Assert.Contains(columns, c => c.DisplayName == "Age" && c.ColId == "age");
        Assert.Contains(columns, c => c.DisplayName == "Start Date" && c.ColId == "startDate");
    }

    [Theory]
    [InlineData("age", true)]  // Case insensitive
    [InlineData("Age", true)]  // Exact match
    [InlineData("AGE", true)]  // Case insensitive
    public void Parse_ColumnNameCaseSensitivity_ShouldBeHandledCorrectly(string columnName, bool shouldSucceed)
    {
        // Arrange
        var filter = $"[{columnName}] > 25";

        // Act
        var result = _parser.Parse(filter);

        // Assert
        if (shouldSucceed)
        {
            Assert.False(result.HasErrors, $"Filter with column '{columnName}' should succeed");
        }
        else
        {
            Assert.True(result.HasErrors, $"Filter with column '{columnName}' should fail");
        }
    }
}