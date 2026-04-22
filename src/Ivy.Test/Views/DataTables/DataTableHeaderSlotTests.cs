namespace Ivy.Test.Views.DataTables;

public class DataTableHeaderSlotTests
{
    private record TestRow(int Id, string Name);

    [Fact]
    public void HeaderLeft_StoresFactory()
    {
        var data = new[] { new TestRow(1, "A") }.AsQueryable();
        var builder = data.ToDataTable()
            .HeaderLeft(ctx => new Badge("Left"));

        var field = builder.GetType()
            .GetField("_headerLeftFactory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(field);
        Assert.NotNull(field.GetValue(builder));
    }

    [Fact]
    public void HeaderRight_StoresFactory()
    {
        var data = new[] { new TestRow(1, "A") }.AsQueryable();
        var builder = data.ToDataTable()
            .HeaderRight(ctx => new Badge("Right"));

        var field = builder.GetType()
            .GetField("_headerRightFactory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(field);
        Assert.NotNull(field.GetValue(builder));
    }

    [Fact]
    public void NoHeaderSlots_FactoriesAreNull()
    {
        var data = new[] { new TestRow(1, "A") }.AsQueryable();
        var builder = data.ToDataTable();

        var leftField = builder.GetType()
            .GetField("_headerLeftFactory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var rightField = builder.GetType()
            .GetField("_headerRightFactory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Assert.NotNull(leftField);
        Assert.NotNull(rightField);
        Assert.Null(leftField.GetValue(builder));
        Assert.Null(rightField.GetValue(builder));
    }

    [Fact]
    public void HeaderLeft_ReturnsBuilder_ForChaining()
    {
        var data = new[] { new TestRow(1, "A") }.AsQueryable();
        var builder = data.ToDataTable();
        var result = builder.HeaderLeft(ctx => new Badge("Left"));
        Assert.Same(builder, result);
    }

    [Fact]
    public void HeaderRight_ReturnsBuilder_ForChaining()
    {
        var data = new[] { new TestRow(1, "A") }.AsQueryable();
        var builder = data.ToDataTable();
        var result = builder.HeaderRight(ctx => new Badge("Right"));
        Assert.Same(builder, result);
    }
}
