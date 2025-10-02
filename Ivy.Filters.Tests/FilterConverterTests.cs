using Ivy.Filters;
using System.Text.Json;

namespace Ivy.Filters.Tests;

public class FilterConverterTests
{
    private readonly FilterConverter _converter;

    public FilterConverterTests()
    {
        _converter = new FilterConverter();
    }

    [Fact]
    public void ConvertToGridModel_SimpleLeaf_ShouldCreateColumnFilter()
    {
        // Arrange
        var leaf = new Leaf("Age", "age", FieldType.Number, Op.GreaterThan, 25);

        // Act
        var result = _converter.ConvertToModel(leaf);

        // Assert
        Assert.IsType<FieldFilterModel>(result);
        var columnFilter = (FieldFilterModel)result;

        Assert.Equal("number", columnFilter.FilterType);
        Assert.Equal("age", columnFilter.ColId);
        Assert.Equal("greaterThan", columnFilter.Type);
        Assert.Equal(25, columnFilter.Filter);
        Assert.Null(columnFilter.FilterTo);
    }

    [Theory]
    [InlineData(FieldType.Text, "text")]
    [InlineData(FieldType.Number, "number")]
    [InlineData(FieldType.Date, "date")]
    [InlineData(FieldType.DateTime, "dateTime")]
    [InlineData(FieldType.Boolean, "boolean")]
    public void ConvertToGridModel_DifferentColumnTypes_ShouldMapCorrectly(FieldType fieldType, string expectedFilterType)
    {
        // Arrange
        var leaf = new Leaf("Test", "test", fieldType, Op.Equals, "value");

        // Act
        var result = _converter.ConvertToModel(leaf);

        // Assert
        Assert.IsType<FieldFilterModel>(result);
        var columnFilter = (FieldFilterModel)result;
        Assert.Equal(expectedFilterType, columnFilter.FilterType);
    }

    [Theory]
    [InlineData(Op.Contains, "contains")]
    [InlineData(Op.NotContains, "notContains")]
    [InlineData(Op.StartsWith, "startsWith")]
    [InlineData(Op.EndsWith, "endsWith")]
    [InlineData(Op.Equals, "equals")]
    [InlineData(Op.NotEqual, "notEqual")]
    [InlineData(Op.GreaterThan, "greaterThan")]
    [InlineData(Op.GreaterThanOrEqual, "greaterThanOrEqual")]
    [InlineData(Op.LessThan, "lessThan")]
    [InlineData(Op.LessThanOrEqual, "lessThanOrEqual")]
    [InlineData(Op.Blank, "blank")]
    [InlineData(Op.NotBlank, "notBlank")]
    public void ConvertToGridModel_DifferentOperators_ShouldMapCorrectly(Op op, string expectedType)
    {
        // Arrange
        var leaf = new Leaf("Test", "test", FieldType.Text, op, "value");

        // Act
        var result = _converter.ConvertToModel(leaf);

        // Assert
        Assert.IsType<FieldFilterModel>(result);
        var columnFilter = (FieldFilterModel)result;
        Assert.Equal(expectedType, columnFilter.Type);
    }

    [Fact]
    public void ConvertToGridModel_AndOperation_ShouldCreateJoinFilter()
    {
        // Arrange
        var left = new Leaf("Age", "age", FieldType.Number, Op.GreaterThan, 25);
        var right = new Leaf("Country", "country", FieldType.Text, Op.Equals, "USA");
        var and = new And(left, right);

        // Act
        var result = _converter.ConvertToModel(and);

        // Assert
        Assert.IsType<GroupFilterModel>(result);
        var joinFilter = (GroupFilterModel)result;

        Assert.Equal("join", joinFilter.FilterType);
        Assert.Equal("AND", joinFilter.Type);
        Assert.Equal(2, joinFilter.Conditions.Count);
        Assert.All(joinFilter.Conditions, condition => Assert.IsType<FieldFilterModel>(condition));
    }

    [Fact]
    public void ConvertToGridModel_OrOperation_ShouldCreateJoinFilter()
    {
        // Arrange
        var left = new Leaf("Age", "age", FieldType.Number, Op.LessThan, 18);
        var right = new Leaf("Age", "age", FieldType.Number, Op.GreaterThan, 65);
        var or = new Or(left, right);

        // Act
        var result = _converter.ConvertToModel(or);

        // Assert
        Assert.IsType<GroupFilterModel>(result);
        var joinFilter = (GroupFilterModel)result;

        Assert.Equal("join", joinFilter.FilterType);
        Assert.Equal("OR", joinFilter.Type);
        Assert.Equal(2, joinFilter.Conditions.Count);
    }

    [Fact]
    public void ConvertToGridModel_NotWithNegatableOperator_ShouldFlipOperator()
    {
        // Arrange
        var inner = new Leaf("Name", "name", FieldType.Text, Op.Contains, "test");
        var not = new Not(inner);

        // Act
        var result = _converter.ConvertToModel(not);

        // Assert
        Assert.IsType<FieldFilterModel>(result);
        var columnFilter = (FieldFilterModel)result;

        Assert.Equal("text", columnFilter.FilterType);
        Assert.Equal("name", columnFilter.ColId);
        Assert.Equal("notContains", columnFilter.Type);
        Assert.Equal("test", columnFilter.Filter);
    }

    [Fact]
    public void ConvertToGridModel_NotWithNonNegatableOperator_ShouldCreateJoin()
    {
        // Arrange
        var inner = new Leaf("Name", "name", FieldType.Text, Op.StartsWith, "test");
        var not = new Not(inner);

        // Act
        var result = _converter.ConvertToModel(not);

        // Assert
        Assert.IsType<GroupFilterModel>(result);
        var joinFilter = (GroupFilterModel)result;

        Assert.Equal("join", joinFilter.FilterType);
        Assert.Equal("AND", joinFilter.Type);
        Assert.Single(joinFilter.Conditions);
    }

    [Fact]
    public void ConvertToGridModel_ComplexNestedStructure_ShouldHandleCorrectly()
    {
        // Arrange: ([Age] > 25 OR [Age] < 18) AND [Country] = "USA"
        var ageGreater = new Leaf("Age", "age", FieldType.Number, Op.GreaterThan, 25);
        var ageLess = new Leaf("Age", "age", FieldType.Number, Op.LessThan, 18);
        var ageOr = new Or(ageGreater, ageLess);
        var country = new Leaf("Country", "country", FieldType.Text, Op.Equals, "USA");
        var finalAnd = new And(ageOr, country);

        // Act
        var result = _converter.ConvertToModel(finalAnd);

        // Assert
        Assert.IsType<GroupFilterModel>(result);
        var rootJoin = (GroupFilterModel)result;

        Assert.Equal("AND", rootJoin.Type);
        Assert.Equal(2, rootJoin.Conditions.Count);

        // First condition should be the OR join
        Assert.IsType<GroupFilterModel>(rootJoin.Conditions[0]);
        var orJoin = (GroupFilterModel)rootJoin.Conditions[0];
        Assert.Equal("OR", orJoin.Type);
        Assert.Equal(2, orJoin.Conditions.Count);

        // Second condition should be the country filter
        Assert.IsType<FieldFilterModel>(rootJoin.Conditions[1]);
    }

    [Fact]
    public void ConvertToGridModel_ResultShouldBeJsonSerializable()
    {
        // Arrange
        var leaf = new Leaf("Age", "age", FieldType.Number, Op.GreaterThan, 25);

        // Act
        var result = _converter.ConvertToModel(leaf);
        var options = new JsonSerializerOptions {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var json = JsonSerializer.Serialize(result, result.GetType(), options);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\"filterType\": \"number\"", json);
        Assert.Contains("\"colId\": \"age\"", json);
        Assert.Contains("\"type\": \"greaterThan\"", json);
        Assert.Contains("\"filter\": 25", json);
    }

    [Theory]
    [InlineData(Op.Contains, Op.NotContains)]
    [InlineData(Op.NotContains, Op.Contains)]
    [InlineData(Op.Equals, Op.NotEqual)]
    [InlineData(Op.NotEqual, Op.Equals)]
    [InlineData(Op.GreaterThan, Op.LessThanOrEqual)]
    [InlineData(Op.GreaterThanOrEqual, Op.LessThan)]
    [InlineData(Op.LessThan, Op.GreaterThanOrEqual)]
    [InlineData(Op.LessThanOrEqual, Op.GreaterThan)]
    [InlineData(Op.Blank, Op.NotBlank)]
    [InlineData(Op.NotBlank, Op.Blank)]
    public void ConvertToGridModel_NegatableOperators_ShouldFlipCorrectly(Op original, Op expectedNegated)
    {
        // Arrange
        var inner = new Leaf("Test", "test", FieldType.Text, original, "value");
        var not = new Not(inner);

        // Act
        var result = _converter.ConvertToModel(not);

        // Assert
        if (result is FieldFilterModel columnFilter)
        {
            // The operator was successfully negated
            var expectedOperatorString = expectedNegated switch
            {
                Op.Contains => "contains",
                Op.NotContains => "notContains",
                Op.Equals => "equals",
                Op.NotEqual => "notEqual",
                Op.GreaterThan => "greaterThan",
                Op.GreaterThanOrEqual => "greaterThanOrEqual",
                Op.LessThan => "lessThan",
                Op.LessThanOrEqual => "lessThanOrEqual",
                Op.Blank => "blank",
                Op.NotBlank => "notBlank",
                _ => throw new ArgumentException($"Unexpected operator: {expectedNegated}")
            };

            Assert.Equal(expectedOperatorString, columnFilter.Type);
        }
        else
        {
            Assert.True(false, $"Expected ColumnFilterModel for negatable operator {original}, got {result.GetType().Name}");
        }
    }
}