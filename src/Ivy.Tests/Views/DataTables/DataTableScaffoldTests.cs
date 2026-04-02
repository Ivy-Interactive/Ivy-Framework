using Ivy;

namespace Ivy.Tests.Views.DataTables;

public class DataTableScaffoldTests
{
    private class ChildEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    private class EntityWithICollection
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public ICollection<ChildEntity> Children { get; set; } = [];
    }

    private class EntityWithList
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public List<ChildEntity> Children { get; set; } = [];
    }

    private class EntityWithStringArray
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string[] Tags { get; set; } = [];
    }

    private class EntityWithListOfStrings
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public List<string> Tags { get; set; } = [];
    }

    private class EntityWithPrimitives
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public decimal Amount { get; set; }
    }

    private static bool IsColumnRemoved<T>(DataTableBuilder<T> builder, string columnName)
    {
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var columnsField = builder.GetType().GetField("_columns", flags)!;
        var columnsObj = columnsField.GetValue(builder)!;
        var dictType = columnsObj.GetType();
        var itemProp = dictType.GetProperty("Item")!;
        var internalColumn = itemProp.GetValue(columnsObj, [columnName])!;
        var removedProp = internalColumn.GetType().GetProperty("Removed")!;
        return (bool)removedProp.GetValue(internalColumn)!;
    }

    [Fact]
    public void Scaffold_ExcludesICollectionNavigationProperties()
    {
        var builder = new[] { new EntityWithICollection() }.AsQueryable().ToDataTable();
        Assert.True(IsColumnRemoved(builder, "Children"));
    }

    [Fact]
    public void Scaffold_ExcludesListNavigationProperties()
    {
        var builder = new[] { new EntityWithList() }.AsQueryable().ToDataTable();
        Assert.True(IsColumnRemoved(builder, "Children"));
    }

    [Fact]
    public void Scaffold_KeepsStringArrayAsLabels()
    {
        var builder = new[] { new EntityWithStringArray() }.AsQueryable().ToDataTable();
        Assert.False(IsColumnRemoved(builder, "Tags"));
    }

    [Fact]
    public void Scaffold_KeepsListOfStrings()
    {
        var builder = new[] { new EntityWithListOfStrings() }.AsQueryable().ToDataTable();
        Assert.False(IsColumnRemoved(builder, "Tags"));
    }

    [Fact]
    public void Scaffold_KeepsPrimitiveProperties()
    {
        var builder = new[] { new EntityWithPrimitives() }.AsQueryable().ToDataTable();
        Assert.False(IsColumnRemoved(builder, "Id"));
        Assert.False(IsColumnRemoved(builder, "Name"));
        Assert.False(IsColumnRemoved(builder, "CreatedAt"));
        Assert.False(IsColumnRemoved(builder, "Amount"));
    }
}
