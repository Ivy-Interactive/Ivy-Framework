
using Ivy.Views.Builders;
using Xunit;

namespace Ivy.Test;

public class TableBuilderTests
{
    private class TestModel
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }

    [Fact]
    public void Reset_ShouldRestoreInitialState()
    {
        // Arrange
        var data = new[]
        {
            new TestModel { Name = "Alice", Age = 30, IsActive = true },
            new TestModel { Name = "Bob", Age = 25, IsActive = false }
        };

        var builder = new TableBuilder<TestModel>(data);

        // Modify state
        builder.Remove(x => x.Age);
        builder.Align(x => x.Name, Align.Right);

        // Act
        builder.Reset();

        // Assert
        // We need to access internal state to verify, but since we can't easily access internal _columns without reflection or public API,
        // we might need to rely on Build() or expose something.
        // However, looking at TableBuilder.cs, we can verify behavior by ensuring Reset() doesn't break smart defaults.

        // Use reflection to inspect the private _columns field for verification
        var columnsField = typeof(TableBuilder<TestModel>).GetField("_columns", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var columns = (System.Collections.IDictionary)columnsField!.GetValue(builder)!;

        // Helper to get property value from the private TableBuilderColumn instance
        object GetProp(object obj, string propName) =>
            obj.GetType().GetProperty(propName)!.GetValue(obj)!;

        // Check Age column (Numeric) - Should be Right aligned by default scaffolding
        // Reset() sets it to Left currently.

        var ageColumn = columns["Age"];
        Assert.False((bool)GetProp(ageColumn!, "Removed"), "Age column should be visible after Reset");
        Assert.Equal(Align.Right, (Align)GetProp(ageColumn!, "Align")); // This is expected to FAIL if Reset sets it to Left

        var nameColumn = columns["Name"];
        Assert.Equal(Align.Left, (Align)GetProp(nameColumn!, "Align")); // Name is string, default Left. Modified to Right. Reset should back to Left.

        var activeColumn = columns["IsActive"];
        Assert.Equal(Align.Center, (Align)GetProp(activeColumn!, "Align")); // Bool is Center default. Reset sets to Left. expected FAIL.
    }

    [Fact]
    public void ProgressBuilder_ShouldReturnProgressWidget()
    {
        var builder = new ProgressBuilder<TestModel>();
        var result = builder.Build(50, new TestModel());
        Assert.IsType<Progress>(result);
    }

    [Fact]
    public void ProgressBuilder_ShouldReturnNullForNullValue()
    {
        var builder = new ProgressBuilder<TestModel>();
        var result = builder.Build(null, new TestModel());
        Assert.Null(result);
    }

    [Fact]
    public void ProgressBuilder_ShouldReturnNullForNonNumericValue()
    {
        var builder = new ProgressBuilder<TestModel>();
        var result = builder.Build("not a number", new TestModel());
        Assert.Null(result);
    }

    [Fact]
    public void ProgressBuilder_ShouldCalculatePercentageCorrectly()
    {
        var builder = new ProgressBuilder<TestModel>().Min(0).Max(200);
        var result = builder.Build(100, new TestModel()) as Progress;
        Assert.NotNull(result);
        Assert.Equal(50, result.Value);
    }

    [Fact]
    public void ProgressBuilder_ShouldClampPercentageToZeroToHundred()
    {
        var builder = new ProgressBuilder<TestModel>().Min(0).Max(100);

        var resultOver = builder.Build(150, new TestModel()) as Progress;
        Assert.NotNull(resultOver);
        Assert.Equal(100, resultOver.Value);

        var resultUnder = builder.Build(-50, new TestModel()) as Progress;
        Assert.NotNull(resultUnder);
        Assert.Equal(0, resultUnder.Value);
    }

    [Fact]
    public void ProgressBuilder_ShouldApplyAutoColor()
    {
        var builder = new ProgressBuilder<TestModel>().AutoColor();

        var resultHigh = builder.Build(80, new TestModel()) as Progress;
        Assert.NotNull(resultHigh);
        Assert.Equal(Colors.Success, resultHigh.Color);

        var resultMedium = builder.Build(60, new TestModel()) as Progress;
        Assert.NotNull(resultMedium);
        Assert.Equal(Colors.Warning, resultMedium.Color);

        var resultLow = builder.Build(10, new TestModel()) as Progress;
        Assert.NotNull(resultLow);
        Assert.Equal(Colors.Destructive, resultLow.Color);
    }

    [Fact]
    public void ProgressBuilder_ShouldApplyExplicitColor()
    {
        var builder = new ProgressBuilder<TestModel>().Color(Colors.Blue);
        var result = builder.Build(50, new TestModel()) as Progress;
        Assert.NotNull(result);
        Assert.Equal(Colors.Blue, result.Color);
    }

    [Fact]
    public void ProgressBuilder_ShouldReturnLayoutWithFormatString()
    {
        var builder = new ProgressBuilder<TestModel>().Format("%d%");
        var result = builder.Build(50, new TestModel());
        Assert.IsType<LayoutView>(result);
    }
}
