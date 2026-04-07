using System.ComponentModel.DataAnnotations;
using Ivy.Samples.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Agent.EfQuery.Test;

// Test-specific entities for new SchemaCollector features

public enum MembershipTier
{
    Bronze = 0,
    Silver = 1,
    Gold = 2,
    Platinum = 3
}

[EfQueryDescription("Travel agency customers")]
public class TestCustomerWithEnum
{
    [Key] public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public MembershipTier Tier { get; set; }
    [EfQueryIgnore] public string PasswordHash { get; set; } = "";
    [EfQueryDescription("Annual spending in USD")] public decimal AnnualSpend { get; set; }
}

[EfQueryIgnore]
public class SecretEntity
{
    [Key] public Guid Id { get; set; }
    public string TopSecret { get; set; } = "";
}

public class TestSchemaDbContext(DbContextOptions<TestSchemaDbContext> options) : DbContext(options)
{
    public DbSet<TestCustomerWithEnum> TestCustomers { get; set; }
    public DbSet<SecretEntity> Secrets { get; set; }
}

public class SchemaCollectorTests : IDisposable
{
    public SchemaCollectorTests()
    {
        // Clear cache before each test to avoid cross-test contamination
        SchemaCollector.ClearCache();
    }

    public void Dispose()
    {
        SchemaCollector.ClearCache();
    }

    private static SampleDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SampleDbContext>()
            .UseInMemoryDatabase(databaseName: $"SchemaTest_{Guid.NewGuid()}")
            .Options;
        return new SampleDbContext(options);
    }

    private static TestSchemaDbContext CreateTestSchemaContext()
    {
        var options = new DbContextOptionsBuilder<TestSchemaDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestSchemaTest_{Guid.NewGuid()}")
            .Options;
        return new TestSchemaDbContext(options);
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

    [Fact]
    public void CollectSchema_ContainsDatabaseDialect()
    {
        using var context = CreateContext();
        var schema = SchemaCollector.CollectSchema(context);

        Assert.Contains("Database: InMemory", schema);
    }

    [Fact]
    public void CollectSchema_EnumValues_AppearInOutput()
    {
        using var context = CreateTestSchemaContext();
        var schema = SchemaCollector.CollectSchema(context);

        Assert.Contains("ENUM VALUES:", schema);
        Assert.Contains("0 = Bronze", schema);
        Assert.Contains("1 = Silver", schema);
        Assert.Contains("2 = Gold", schema);
        Assert.Contains("3 = Platinum", schema);
    }

    [Fact]
    public void CollectSchema_EfQueryIgnore_HidesEntity()
    {
        using var context = CreateTestSchemaContext();
        var schema = SchemaCollector.CollectSchema(context);

        Assert.DoesNotContain("SecretEntity", schema);
        Assert.DoesNotContain("TopSecret", schema);
    }

    [Fact]
    public void CollectSchema_EfQueryIgnore_HidesProperty()
    {
        using var context = CreateTestSchemaContext();
        var schema = SchemaCollector.CollectSchema(context);

        Assert.DoesNotContain("PasswordHash", schema);
    }

    [Fact]
    public void CollectSchema_EfQueryDescription_AppearsOnEntity()
    {
        using var context = CreateTestSchemaContext();
        var schema = SchemaCollector.CollectSchema(context);

        Assert.Contains("DESCRIPTION: Travel agency customers", schema);
    }

    [Fact]
    public void CollectSchema_EfQueryDescription_AppearsOnProperty()
    {
        using var context = CreateTestSchemaContext();
        var schema = SchemaCollector.CollectSchema(context);

        Assert.Contains("DESCRIPTION: Annual spending in USD", schema);
    }

    [Fact]
    public void CollectSchema_Caching_ReturnsSameReference()
    {
        using var context = CreateTestSchemaContext();

        var schema1 = SchemaCollector.CollectSchema(context);
        var schema2 = SchemaCollector.CollectSchema(context);

        Assert.Same(schema1, schema2);
    }
}
