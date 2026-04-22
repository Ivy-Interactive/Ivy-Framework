using Ivy.Views.Builders;

namespace Ivy.Test.Views;

public class DetailsBuilderNavigationTests
{
    #region Test models

    private class CategoryWithName
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    private class CategoryWithTitle
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
    }

    private class CategoryWithDisplayName
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = "";
    }

    private class CategoryWithToString
    {
        public int Id { get; set; }
        public override string ToString() => $"Custom({Id})";
    }

    private class CategoryWithIdOnly
    {
        public int Id { get; set; }
    }

    private class CategoryEmpty;

    private class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public CategoryWithName? Category { get; set; }
        public List<string> Tags { get; set; } = [];
    }

    #endregion

    [Fact]
    public void ResolveDisplayValue_WithNameProperty_ReturnsName()
    {
        var category = new CategoryWithName { Id = 1, Name = "Beer" };
        var result = NavigationPropertyBuilder<object>.ResolveDisplayValue(category);
        Assert.Equal("Beer", result);
    }

    [Fact]
    public void ResolveDisplayValue_WithTitleProperty_ReturnsTitle()
    {
        var category = new CategoryWithTitle { Id = 1, Title = "Beverages" };
        var result = NavigationPropertyBuilder<object>.ResolveDisplayValue(category);
        Assert.Equal("Beverages", result);
    }

    [Fact]
    public void ResolveDisplayValue_WithDisplayNameProperty_ReturnsDisplayName()
    {
        var category = new CategoryWithDisplayName { Id = 1, DisplayName = "My Category" };
        var result = NavigationPropertyBuilder<object>.ResolveDisplayValue(category);
        Assert.Equal("My Category", result);
    }

    [Fact]
    public void ResolveDisplayValue_WithToStringOverride_UsesToString()
    {
        var category = new CategoryWithToString { Id = 42 };
        var result = NavigationPropertyBuilder<object>.ResolveDisplayValue(category);
        Assert.Equal("Custom(42)", result);
    }

    [Fact]
    public void ResolveDisplayValue_WithIdOnly_ReturnsEntityHash()
    {
        var category = new CategoryWithIdOnly { Id = 7 };
        var result = NavigationPropertyBuilder<object>.ResolveDisplayValue(category);
        Assert.Equal("Entity #7", result);
    }

    [Fact]
    public void ResolveDisplayValue_Null_ReturnsNull()
    {
        var result = NavigationPropertyBuilder<object>.ResolveDisplayValue(null);
        Assert.Null(result);
    }

    [Fact]
    public void NavigationPropertyBuilder_Build_ResolvesDisplayValue()
    {
        var builder = new NavigationPropertyBuilder<Product>();
        var product = new Product { Id = 1, Name = "Test", Category = new CategoryWithName { Id = 1, Name = "Beer" } };
        var result = builder.Build(product.Category, product);
        Assert.Equal("Beer", result);
    }

    [Fact]
    public void NavigationPropertyBuilder_Build_NullValue_ReturnsNull()
    {
        var builder = new NavigationPropertyBuilder<Product>();
        var product = new Product { Id = 1, Name = "Test" };
        var result = builder.Build(null, product);
        Assert.Null(result);
    }

    [Fact]
    public void ScalarProperties_StillUseDefaultBuilder()
    {
        // Verify that simple types are not affected by the navigation property logic
        var defaultBuilder = new DefaultBuilder<Product>();
        Assert.Equal(42, defaultBuilder.Build(42, new Product()));
        Assert.Equal("hello", defaultBuilder.Build("hello", new Product()));
        Assert.Equal(3.14m, defaultBuilder.Build(3.14m, new Product()));
    }
}
