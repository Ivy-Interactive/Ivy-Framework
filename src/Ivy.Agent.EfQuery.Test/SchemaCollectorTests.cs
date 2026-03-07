using Ivy.Samples.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Agent.EfQuery.Test;

public class SchemaCollectorTests
{
    private static SampleDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SampleDbContext>()
            .UseInMemoryDatabase(databaseName: $"SchemaTest_{Guid.NewGuid()}")
            .Options;
        return new SampleDbContext(options);
    }

    [Fact]
    public void CollectSchema_ContainsExpectedTables()
    {
        using var context = CreateContext();
        var schema = SchemaCollector.CollectSchema(context);

        Assert.Contains("Product", schema);
        Assert.Contains("Order", schema);
        Assert.Contains("Customer", schema);
        Assert.Contains("Category", schema);
        Assert.Contains("Department", schema);
    }

    [Fact]
    public void CollectSchema_ContainsProductColumns()
    {
        using var context = CreateContext();
        var schema = SchemaCollector.CollectSchema(context);

        Assert.Contains("Name", schema);
        Assert.Contains("Price", schema);
        Assert.Contains("Rating", schema);
        Assert.Contains("Description", schema);
    }

    [Fact]
    public void CollectSchema_ContainsPrimaryKeys()
    {
        using var context = CreateContext();
        var schema = SchemaCollector.CollectSchema(context);

        Assert.Contains("[PK]", schema);
    }

    [Fact]
    public void CollectSchema_ContainsForeignKeys()
    {
        using var context = CreateContext();
        var schema = SchemaCollector.CollectSchema(context);

        Assert.Contains("FK:", schema);
    }

    [Fact]
    public void CollectSchema_ContainsColumnTypes()
    {
        using var context = CreateContext();
        var schema = SchemaCollector.CollectSchema(context);

        Assert.Contains("Guid", schema);
        Assert.Contains("String", schema);
        Assert.Contains("Int32", schema);
        Assert.Contains("DateTime", schema);
    }
}
