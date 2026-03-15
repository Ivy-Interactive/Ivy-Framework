using System.Linq.Expressions;
using Ivy;

namespace Ivy.Test.Helpers;

public class TypeHelperTests
{
    [Fact]
    public void GetCollectionTypeParameter_ListOfInt_ReturnsInt()
    {
        var type = typeof(List<int>);
        Assert.Equal(typeof(int), type.GetCollectionTypeParameter());
    }

    [Fact]
    public void GetCollectionTypeParameter_Array_ReturnsElementType()
    {
        var type = typeof(string[]);
        Assert.Equal(typeof(string), type.GetCollectionTypeParameter());
    }

    [Fact]
    public void GetCollectionTypeParameter_Dictionary_ReturnsKeyValuePair()
    {
        var type = typeof(Dictionary<string, int>);
        var result = type.GetCollectionTypeParameter();
        Assert.NotNull(result);
        Assert.True(result!.IsGenericType);
    }

    [Theory]
    [InlineData(typeof(List<int>), true)]
    [InlineData(typeof(int[]), true)]
    [InlineData(typeof(string), false)]
    [InlineData(null, false)]
    [InlineData(typeof(int), false)]
    public void IsCollectionType_ReturnsExpected(Type? type, bool expected)
    {
        Assert.Equal(expected, type.IsCollectionType());
    }

    [Theory]
    [InlineData(typeof(int?), true)]
    [InlineData(typeof(int), false)]
    [InlineData(typeof(string), false)]
    public void IsNullable_ReturnsExpected(Type type, bool expected)
    {
        Assert.Equal(expected, type.IsNullable());
    }

    [Theory]
    [InlineData(typeof(int?), true)]
    [InlineData(typeof(int), false)]
    public void IsNullableType_ReturnsExpected(Type type, bool expected)
    {
        Assert.Equal(expected, type.IsNullableType());
    }

    [Theory]
    [InlineData(typeof(double), 4)]
    [InlineData(typeof(decimal), 4)]
    [InlineData(typeof(int), 0)]
    [InlineData(typeof(double?), 4)]
    public void SuggestPrecision_ReturnsExpected(Type type, int expected)
    {
        Assert.Equal(expected, type.SuggestPrecision());
    }

    [Theory]
    [InlineData(typeof(int), 1.0)]
    [InlineData(typeof(double), 0.01)]
    [InlineData(typeof(decimal), 0.01)]
    public void SuggestStep_ReturnsExpected(Type type, double expected)
    {
        Assert.Equal(expected, type.SuggestStep());
    }

    [Theory]
    [InlineData(typeof(DateTime), true)]
    [InlineData(typeof(DateTimeOffset), true)]
    [InlineData(typeof(DateOnly), true)]
    [InlineData(typeof(DateTime?), true)]
    [InlineData(typeof(int), false)]
    public void IsDate_ReturnsExpected(Type type, bool expected)
    {
        Assert.Equal(expected, type.IsDate());
    }

    [Theory]
    [InlineData(typeof(int), true)]
    [InlineData(typeof(double), true)]
    [InlineData(typeof(decimal), true)]
    [InlineData(typeof(string), false)]
    [InlineData(typeof(int?), true)]
    public void IsNumeric_ReturnsExpected(Type type, bool expected)
    {
        Assert.Equal(expected, type.IsNumeric());
    }

    [Theory]
    [InlineData(typeof(int), true)]
    [InlineData(typeof(string), true)]
    [InlineData(typeof(DateTime), true)]
    [InlineData(typeof(Guid), true)]
    [InlineData(typeof(List<int>), false)]
    public void IsSimpleType_ReturnsExpected(Type type, bool expected)
    {
        Assert.Equal(expected, TypeHelper.IsSimpleType(type));
    }

    [Fact]
    public void IsObservable_WithObservable_ReturnsTrue()
    {
        var observable = System.Reactive.Linq.Observable.Empty<int>();
        Assert.True(TypeHelper.IsObservable(observable));
    }

    [Fact]
    public void IsObservable_WithNonObservable_ReturnsFalse()
    {
        Assert.False(TypeHelper.IsObservable("hello"));
    }

    [Fact]
    public void GetNameFromMemberExpression_ReturnsMemberName()
    {
        Expression<Func<TestModel, string>> expr = m => m.Name;
        var name = TypeHelper.GetNameFromMemberExpression(expr.Body);
        Assert.Equal("Name", name);
    }

    [Fact]
    public void GetNameFromMemberExpression_UnaryExpression_ReturnsMemberName()
    {
        Expression<Func<TestModel, object>> expr = m => m.Age;
        var name = TypeHelper.GetNameFromMemberExpression(expr.Body);
        Assert.Equal("Age", name);
    }

    [Fact]
    public void SuggestMin_Int32_ReturnsMinValue()
    {
        Assert.Equal(int.MinValue, typeof(int).SuggestMin());
    }

    [Fact]
    public void SuggestMax_Int32_ReturnsMaxValue()
    {
        Assert.Equal(int.MaxValue, typeof(int).SuggestMax());
    }

    private class TestModel
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }
}
