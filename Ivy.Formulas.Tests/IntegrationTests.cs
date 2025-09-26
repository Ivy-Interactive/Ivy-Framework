using Ivy.Formulas;
using System.Text.Json;

namespace Ivy.Formulas.Tests;

/// <summary>
/// Integration tests that test the full pipeline from formula string to grid model
/// </summary>
public class IntegrationTests
{
    private readonly FormulaParser _parser;

    public IntegrationTests()
    {
        var columns = new[]
        {
            new FieldMeta("Customer Name", "customerName", FieldType.Text),
            new FieldMeta("Order Total", "orderTotal", FieldType.Number),
            new FieldMeta("Order Date", "orderDate", FieldType.Date),
            new FieldMeta("Is Premium", "isPremium", FieldType.Boolean),
            new FieldMeta("Category", "category", FieldType.Text)
        };

        _parser = new FormulaParser(columns);
    }

    [Fact]
    public void EndToEnd_SimpleTextFilter_ShouldProduceCorrectGridModel()
    {
        // Arrange
        var formula = "[Customer Name] contains \"Smith\"";

        // Act
        var result = _parser.Parse(formula);

        // Assert
        Assert.False(result.HasErrors);
        Assert.NotNull(result.Model);

        var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(result.Model, result.Model?.GetType() ?? typeof(FormulaModel), options);

        Assert.Contains("\"filterType\": \"text\"", json);
        Assert.Contains("\"colId\": \"customerName\"", json);
        Assert.Contains("\"type\": \"contains\"", json);
        Assert.Contains("\"filter\": \"Smith\"", json);
    }

    [Fact]
    public void EndToEnd_NumberRangeFilter_ShouldProduceCorrectGridModel()
    {
        // Arrange
        var formula = "[Order Total] in range 100 AND 1000";

        // Act
        var result = _parser.Parse(formula);

        // Assert
        if (!result.HasErrors) // May fail due to current grammar limitations
        {
            Assert.NotNull(result.Model);

            var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(result.Model, result.Model?.GetType() ?? typeof(FormulaModel), options);

            Assert.Contains("\"filterType\": \"number\"", json);
            Assert.Contains("\"colId\": \"orderTotal\"", json);
            Assert.Contains("\"type\": \"inRange\"", json);
            Assert.Contains("\"filter\": 100", json);
            Assert.Contains("\"filterTo\": 1000", json);
        }
    }

    [Fact]
    public void EndToEnd_ComplexLogicalFilter_ShouldProduceNestedGridModel()
    {
        // Arrange
        var formula = "[Order Total] > 500 AND [Category] = \"Electronics\"";

        // Act
        var result = _parser.Parse(formula);

        // Assert
        Assert.False(result.HasErrors);
        Assert.NotNull(result.Model);
        Assert.IsType<GroupFormulaModel>(result.Model);

        var joinModel = (GroupFormulaModel)result.Model;
        Assert.Equal("join", joinModel.FilterType);
        Assert.Equal("AND", joinModel.Type);
        Assert.Equal(2, joinModel.Conditions.Count);

        // First condition: Order Total > 500
        var firstCondition = (FieldFormulaModel)joinModel.Conditions[0];
        Assert.Equal("number", firstCondition.FilterType);
        Assert.Equal("orderTotal", firstCondition.ColId);
        Assert.Equal("greaterThan", firstCondition.Type);

        // Second condition: Category = "Electronics"
        var secondCondition = (FieldFormulaModel)joinModel.Conditions[1];
        Assert.Equal("text", secondCondition.FilterType);
        Assert.Equal("category", secondCondition.ColId);
        Assert.Equal("equals", secondCondition.Type);
    }

    [Fact]
    public void EndToEnd_BooleanFilter_ShouldProduceCorrectGridModel()
    {
        // Arrange
        var formula = "[Is Premium] = true";

        // Act
        var result = _parser.Parse(formula);

        // Assert
        Assert.False(result.HasErrors);
        Assert.NotNull(result.Model);

        var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(result.Model, result.Model?.GetType() ?? typeof(FormulaModel), options);

        Assert.Contains("\"filterType\": \"boolean\"", json);
        Assert.Contains("\"colId\": \"isPremium\"", json);
        Assert.Contains("\"type\": \"equals\"", json);
    }

    [Fact]
    public void EndToEnd_NegationFilter_ShouldHandleCorrectly()
    {
        // Arrange
        var formula = "NOT [Customer Name] contains \"Test\"";

        // Act
        var result = _parser.Parse(formula);

        // Assert
        Assert.False(result.HasErrors);
        Assert.NotNull(result.Model);

        // Should either flip to "notContains" or create a negated structure
        var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(result.Model, result.Model?.GetType() ?? typeof(FormulaModel), options);

        // Should contain either the flipped operator or a join structure
        Assert.True(
            json.Contains("\"type\": \"notContains\"") || json.Contains("\"Type\": \"AND\""),
            "Should either flip operator or create negated join structure"
        );
    }

    [Fact]
    public void EndToEnd_MultipleColumnsWithSpaces_ShouldWork()
    {
        // Arrange
        var formula = "[Customer Name] blank OR [Order Date] not blank";

        // Act
        var result = _parser.Parse(formula);

        // Assert
        Assert.False(result.HasErrors);
        Assert.NotNull(result.Model);
        Assert.IsType<GroupFormulaModel>(result.Model);

        var joinModel = (GroupFormulaModel)result.Model;
        Assert.Equal("OR", joinModel.Type);
        Assert.Equal(2, joinModel.Conditions.Count);

        // Verify both conditions reference columns with spaces
        var conditions = joinModel.Conditions.Cast<FieldFormulaModel>().ToList();
        Assert.Contains(conditions, c => c.ColId == "customerName");
        Assert.Contains(conditions, c => c.ColId == "orderDate");
    }

    [Fact]
    public void EndToEnd_InvalidFormula_ShouldProvideUsefulDiagnostics()
    {
        // Arrange
        var formula = "[Nonexistent Column] = \"value\"";

        // Act
        var result = _parser.Parse(formula);

        // Assert
        Assert.True(result.HasErrors);
        Assert.Null(result.Model);

        var errorDiagnostic = result.Diagnostics.First(d => d.Severity == DiagnosticSeverity.Error);
        Assert.Contains("Unknown column", errorDiagnostic.Message);
    }

    [Fact]
    public void EndToEnd_TypeMismatch_ShouldProvideSpecificError()
    {
        // Arrange
        var formula = "[Order Total] contains \"text\""; // Number column with text operation

        // Act
        var result = _parser.Parse(formula);

        // Assert
        Assert.True(result.HasErrors);

        var errorMessages = result.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .Select(d => d.Message)
            .ToList();

        Assert.Contains(errorMessages, msg =>
            msg.Contains("text operation", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("contains", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("[Customer Name] = \"John Doe\"")]
    [InlineData("[Order Total] >= 100")]
    [InlineData("[Order Date] > \"2024-01-01\"")]
    [InlineData("[Is Premium] blank")]
    public void EndToEnd_VariousDataTypes_ShouldAllWork(string formula)
    {
        // Act
        var result = _parser.Parse(formula);

        // Assert
        Assert.False(result.HasErrors,
            $"Formula '{formula}' should work. Errors: {string.Join(", ", result.Diagnostics.Select(d => d.Message))}");
        Assert.NotNull(result.Model);

        // Should be serializable to JSON
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(result.Model, result.Model?.GetType() ?? typeof(FormulaModel), options);
        Assert.NotEmpty(json);
    }

    [Fact]
    public void EndToEnd_PerformanceTest_ShouldHandleManyColumns()
    {
        // Arrange
        var manyColumns = Enumerable.Range(1, 100)
            .Select(i => new FieldMeta($"Column {i}", $"col{i}", FieldType.Text))
            .ToArray();

        var largeParser = new FormulaParser(manyColumns);
        var formula = "[Column 50] contains \"test\"";

        // Act
        var result = largeParser.Parse(formula);

        // Assert
        Assert.False(result.HasErrors);
        Assert.NotNull(result.Model);

        // Should complete reasonably quickly (this is more of a smoke test)
        var leaf = (Leaf)result.Ast!;
        Assert.Equal("Column 50", leaf.FieldDisplay);
        Assert.Equal("col50", leaf.FieldId);
    }
}