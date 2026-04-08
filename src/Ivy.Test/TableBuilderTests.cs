
using Ivy.Views.Builders;

namespace Ivy.Test;

public class TableBuilderTests
{
    private class TestModel
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }

    private class CustomerModel
    {
        public string Name { get; set; } = string.Empty;
        public AddressModel Address { get; set; } = new();
    }

    private class AddressModel
    {
        public string City { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
    }

    [Fact]
    public void SubProperty_MultipleHeaderCalls_CreatesSeparateColumns()
    {
        var data = new[]
        {
            new CustomerModel { Name = "Alice", Address = new AddressModel { City = "NYC", Region = "NY" } }
        };

        var builder = new TableBuilder<CustomerModel>(data);
        builder.Header(c => c.Address.City, "City");
        builder.Header(c => c.Address.Region, "Region");

        var columnsField = typeof(TableBuilder<CustomerModel>)
            .GetField("_columns", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var columns = (System.Collections.IDictionary)columnsField!.GetValue(builder)!;

        Assert.True(columns.Contains("Address.City"));
        Assert.True(columns.Contains("Address.Region"));
    }

    [Fact]
    public void SubProperty_DifferentAccessors_BothWork()
    {
        var data = new[]
        {
            new CustomerModel { Name = "Alice", Address = new AddressModel { City = "NYC", Region = "NY" } }
        };

        var builder = new TableBuilder<CustomerModel>(data);
        builder.Header(c => c.Address.City, "City");
        builder.Header(c => c.Address.Region, "Region");

        var columnsField = typeof(TableBuilder<CustomerModel>)
            .GetField("_columns", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var columns = (System.Collections.IDictionary)columnsField!.GetValue(builder)!;

        var cityColumn = columns["Address.City"]!;
        var regionColumn = columns["Address.Region"]!;

        var getValueMethod = cityColumn.GetType().GetMethod("GetValue");
        Assert.Equal("NYC", getValueMethod!.Invoke(cityColumn, new object[] { data[0] }));
        Assert.Equal("NY", getValueMethod!.Invoke(regionColumn, new object[] { data[0] }));
    }

    [Fact]
    public void SimpleProperty_BackwardCompatibility_UsesSimpleName()
    {
        var data = new[]
        {
            new CustomerModel { Name = "Alice", Address = new AddressModel { City = "NYC", Region = "NY" } }
        };

        var builder = new TableBuilder<CustomerModel>(data);

        var columnsField = typeof(TableBuilder<CustomerModel>)
            .GetField("_columns", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var columns = (System.Collections.IDictionary)columnsField!.GetValue(builder)!;

        // Scaffolded columns should use simple property names
        Assert.True(columns.Contains("Name"));
        Assert.True(columns.Contains("Address"));
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
        builder.AlignContent(x => x.Name, Align.Right);

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
        Assert.Equal(Align.Right, (Align)GetProp(ageColumn!, "AlignContent")); // This is expected to FAIL if Reset sets it to Left

        var nameColumn = columns["Name"];
        Assert.Equal(Align.Left, (Align)GetProp(nameColumn!, "AlignContent")); // Name is string, default Left. Modified to Right. Reset should back to Left.

        var activeColumn = columns["IsActive"];
        Assert.Equal(Align.Center, (Align)GetProp(activeColumn!, "AlignContent")); // Bool is Center default. Reset sets to Left. expected FAIL.
    }

    [Fact]
    public void DictionaryModel_Header_CreatesColumnDynamically()
    {
        var data = new List<Dictionary<string, string>>
        {
            new() { ["Name"] = "Alice", ["Age"] = "30" },
            new() { ["Name"] = "Bob",   ["Age"] = "25" }
        };

        var builder = new TableBuilder<Dictionary<string, string>>(data);
        builder.Header(r => r["Name"], "Name");
        builder.Header(r => r["Age"], "Age");

        var columnsField = typeof(TableBuilder<Dictionary<string, string>>)
            .GetField("_columns", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var columns = (System.Collections.IDictionary)columnsField!.GetValue(builder)!;

        Assert.True(columns.Contains("Name"));
        Assert.True(columns.Contains("Age"));
        Assert.False(columns.Contains("Count"));
        Assert.False(columns.Contains("Keys"));
    }

    [Fact]
    public void DictionaryModel_GetValue_ReturnsDictionaryValue()
    {
        var data = new List<Dictionary<string, string>>
        {
            new() { ["Name"] = "Alice" }
        };

        var builder = new TableBuilder<Dictionary<string, string>>(data);
        builder.Header(r => r["Name"], "Name");

        var columnsField = typeof(TableBuilder<Dictionary<string, string>>)
            .GetField("_columns", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var columns = (System.Collections.IDictionary)columnsField!.GetValue(builder)!;
        var nameColumn = columns["Name"]!;

        var getValueMethod = nameColumn.GetType().GetMethod("GetValue");
        var result = getValueMethod!.Invoke(nameColumn, new object[] { data[0] });
        Assert.Equal("Alice", result);
    }

    [Fact]
    public void DictionaryModel_GetValue_MissingKey_ReturnsNull()
    {
        var data = new List<Dictionary<string, string>>
        {
            new() { ["Name"] = "Alice" }
        };

        var builder = new TableBuilder<Dictionary<string, string>>(data);
        builder.Header(r => r["Email"], "Email");

        var columnsField = typeof(TableBuilder<Dictionary<string, string>>)
            .GetField("_columns", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var columns = (System.Collections.IDictionary)columnsField!.GetValue(builder)!;
        var emailColumn = columns["Email"]!;

        var getValueMethod = emailColumn.GetType().GetMethod("GetValue");
        var result = getValueMethod!.Invoke(emailColumn, new object[] { data[0] });
        Assert.Null(result);
    }

    [Fact]
    public void DictionaryModel_AutoScaffolds_ColumnsFromKeys()
    {
        var data = new List<Dictionary<string, string>>
        {
            new() { ["Name"] = "Alice", ["Age"] = "30" },
            new() { ["Name"] = "Bob",   ["Age"] = "25" }
        };

        var builder = new TableBuilder<Dictionary<string, string>>(data);

        var columnsField = typeof(TableBuilder<Dictionary<string, string>>)
            .GetField("_columns", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var columns = (System.Collections.IDictionary)columnsField!.GetValue(builder)!;

        Assert.Equal(2, columns.Count);
        Assert.True(columns.Contains("Name"));
        Assert.True(columns.Contains("Age"));
    }

    [Fact]
    public void DictionaryModel_AutoScaffolds_EmptyCollection_NoColumns()
    {
        var data = new List<Dictionary<string, string>>();

        var builder = new TableBuilder<Dictionary<string, string>>(data);

        var columnsField = typeof(TableBuilder<Dictionary<string, string>>)
            .GetField("_columns", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var columns = (System.Collections.IDictionary)columnsField!.GetValue(builder)!;
        Assert.Empty(columns);
    }

    [Fact]
    public void PocoModel_StillWorksAfterDictionaryChanges()
    {
        var data = new[]
        {
            new TestModel { Name = "Alice", Age = 30, IsActive = true }
        };

        var builder = new TableBuilder<TestModel>(data);

        var columnsField = typeof(TableBuilder<TestModel>)
            .GetField("_columns", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var columns = (System.Collections.IDictionary)columnsField!.GetValue(builder)!;

        Assert.True(columns.Contains("Name"));
        Assert.True(columns.Contains("Age"));
        Assert.True(columns.Contains("IsActive"));
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

    [Fact]
    public void ImageBuilder_ShouldReturnImageWidgetForValidUrl()
    {
        var builder = new ImageBuilder<TestModel>();
        var result = builder.Build("https://example.com/photo.jpg", new TestModel());
        Assert.IsType<Image>(result);
        var image = (Image)result!;
        Assert.Equal("https://example.com/photo.jpg", image.Src);
    }

    [Fact]
    public void ImageBuilder_ShouldReturnNullForNullValue()
    {
        var builder = new ImageBuilder<TestModel>();
        var result = builder.Build(null, new TestModel());
        Assert.Null(result);
    }

    [Fact]
    public void ImageBuilder_ShouldReturnNullForEmptyString()
    {
        var builder = new ImageBuilder<TestModel>();
        var result = builder.Build("", new TestModel());
        Assert.Null(result);
    }

    [Fact]
    public void ImageBuilder_ShouldReturnNullForWhitespaceString()
    {
        var builder = new ImageBuilder<TestModel>();
        var result = builder.Build("   ", new TestModel());
        Assert.Null(result);
    }

    [Fact]
    public void ImageBuilder_ShouldApplyDefaultSize()
    {
        var builder = new ImageBuilder<TestModel>();
        var result = builder.Build("https://example.com/photo.jpg", new TestModel()) as Image;
        Assert.NotNull(result);
        Assert.Equal(Size.Units(8), result.Width);
        Assert.Equal(Size.Units(8), result.Height);
    }

    [Fact]
    public void ImageBuilder_ShouldApplyCustomWidth()
    {
        var builder = new ImageBuilder<TestModel>().Width(Size.Units(12));
        var result = builder.Build("https://example.com/photo.jpg", new TestModel()) as Image;
        Assert.NotNull(result);
        Assert.Equal(Size.Units(12), result.Width);
        Assert.Equal(Size.Units(8), result.Height); // default height
    }

    [Fact]
    public void ImageBuilder_ShouldApplyCustomHeight()
    {
        var builder = new ImageBuilder<TestModel>().Height(Size.Units(16));
        var result = builder.Build("https://example.com/photo.jpg", new TestModel()) as Image;
        Assert.NotNull(result);
        Assert.Equal(Size.Units(8), result.Width); // default width
        Assert.Equal(Size.Units(16), result.Height);
    }

    [Fact]
    public void ImageBuilder_ShouldApplyAltText()
    {
        var builder = new ImageBuilder<TestModel>().Alt("Product thumbnail");
        var result = builder.Build("https://example.com/photo.jpg", new TestModel()) as Image;
        Assert.NotNull(result);
        Assert.Equal("Product thumbnail", result.Alt);
    }

    [Fact]
    public void ImageBuilder_ShouldApplyObjectFit()
    {
        var builder = new ImageBuilder<TestModel>().ObjectFit(ImageFit.Cover);
        var result = builder.Build("https://example.com/photo.jpg", new TestModel()) as Image;
        Assert.NotNull(result);
        Assert.Equal(ImageFit.Cover, result.ObjectFit);
    }

    [Fact]
    public void ImageBuilder_ShouldDefaultToContainObjectFit()
    {
        var builder = new ImageBuilder<TestModel>();
        var result = builder.Build("https://example.com/photo.jpg", new TestModel()) as Image;
        Assert.NotNull(result);
        Assert.Equal(ImageFit.Contain, result.ObjectFit);
    }

    [Fact]
    public void ImageBuilder_ShouldSupportFluentChaining()
    {
        var builder = new ImageBuilder<TestModel>()
            .Width(Size.Units(10))
            .Height(Size.Units(10))
            .Alt("avatar")
            .ObjectFit(ImageFit.Cover);

        var result = builder.Build("https://example.com/photo.jpg", new TestModel()) as Image;
        Assert.NotNull(result);
        Assert.Equal(Size.Units(10), result.Width);
        Assert.Equal(Size.Units(10), result.Height);
        Assert.Equal("avatar", result.Alt);
        Assert.Equal(ImageFit.Cover, result.ObjectFit);
    }
}

