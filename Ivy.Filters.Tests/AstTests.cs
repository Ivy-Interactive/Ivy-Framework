using Ivy.Filters;

namespace Ivy.Filters.Tests;

public class AstTests
{
    [Fact]
    public void Leaf_Creation_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var leaf = new Leaf("Age", "age", FieldType.Number, Op.GreaterThan, 25);

        // Assert
        Assert.Equal("Age", leaf.FieldDisplay);
        Assert.Equal("age", leaf.FieldId);
        Assert.Equal(FieldType.Number, leaf.Type);
        Assert.Equal(Op.GreaterThan, leaf.Op);
        Assert.Equal(25, leaf.A);
        Assert.Null(leaf.B);
    }

    [Fact]
    public void And_Creation_ShouldSetLeftAndRight()
    {
        // Arrange
        var left = new Leaf("Age", "age", FieldType.Number, Op.GreaterThan, 25);
        var right = new Leaf("Country", "country", FieldType.Text, Op.Equals, "USA");

        // Act
        var and = new And(left, right);

        // Assert
        Assert.Equal(left, and.Left);
        Assert.Equal(right, and.Right);
    }

    [Fact]
    public void Or_Creation_ShouldSetLeftAndRight()
    {
        // Arrange
        var left = new Leaf("Age", "age", FieldType.Number, Op.LessThan, 18);
        var right = new Leaf("Age", "age", FieldType.Number, Op.GreaterThan, 65);

        // Act
        var or = new Or(left, right);

        // Assert
        Assert.Equal(left, or.Left);
        Assert.Equal(right, or.Right);
    }

    [Fact]
    public void Not_Creation_ShouldSetInner()
    {
        // Arrange
        var inner = new Leaf("Country", "country", FieldType.Text, Op.Blank);

        // Act
        var not = new Not(inner);

        // Assert
        Assert.Equal(inner, not.Inner);
    }

    [Fact]
    public void ColumnMeta_Creation_ShouldSetProperties()
    {
        // Arrange & Act
        var column = new FieldMeta("Start Date", "startDate", FieldType.Date);

        // Assert
        Assert.Equal("Start Date", column.DisplayName);
        Assert.Equal("startDate", column.ColId);
        Assert.Equal(FieldType.Date, column.Type);
    }

    [Theory]
    [InlineData(Op.Contains)]
    [InlineData(Op.NotContains)]
    [InlineData(Op.StartsWith)]
    [InlineData(Op.EndsWith)]
    [InlineData(Op.Equals)]
    [InlineData(Op.NotEqual)]
    [InlineData(Op.GreaterThan)]
    [InlineData(Op.GreaterThanOrEqual)]
    [InlineData(Op.LessThan)]
    [InlineData(Op.LessThanOrEqual)]
    [InlineData(Op.Blank)]
    [InlineData(Op.NotBlank)]
    public void Op_EnumValues_ShouldAllBeSupported(Op op)
    {
        // Arrange & Act
        var leaf = new Leaf("Test", "test", FieldType.Text, op);

        // Assert
        Assert.Equal(op, leaf.Op);
    }

    [Theory]
    [InlineData(FieldType.Text)]
    [InlineData(FieldType.Number)]
    [InlineData(FieldType.Date)]
    [InlineData(FieldType.DateTime)]
    [InlineData(FieldType.Boolean)]
    public void ColType_EnumValues_ShouldAllBeSupported(FieldType fieldType)
    {
        // Arrange & Act
        var column = new FieldMeta("Test", "test", fieldType);

        // Assert
        Assert.Equal(fieldType, column.Type);
    }

    [Fact]
    public void Node_Inheritance_ShouldWork()
    {
        // Arrange
        Node leaf = new Leaf("Age", "age", FieldType.Number, Op.Equals, 25);
        Node and = new And(leaf, leaf);
        Node or = new Or(leaf, leaf);
        Node not = new Not(leaf);

        // Act & Assert
        Assert.IsAssignableFrom<Node>(leaf);
        Assert.IsAssignableFrom<Node>(and);
        Assert.IsAssignableFrom<Node>(or);
        Assert.IsAssignableFrom<Node>(not);
    }
}