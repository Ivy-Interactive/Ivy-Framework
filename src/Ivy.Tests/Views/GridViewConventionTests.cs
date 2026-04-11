namespace Ivy.Tests.Views;

public class GridViewConventionTests
{
    private static object GetDefinition(GridView view)
    {
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var field = view.GetType().GetField("_definition", flags)!;
        return field.GetValue(view)!;
    }

    private static int GetRowGap(GridView view)
    {
        var def = GetDefinition(view);
        return (int)def.GetType().GetProperty("RowGap")!.GetValue(def)!;
    }

    private static int GetColumnGap(GridView view)
    {
        var def = GetDefinition(view);
        return (int)def.GetType().GetProperty("ColumnGap")!.GetValue(def)!;
    }

    private static Thickness GetPadding(GridView view)
    {
        var def = GetDefinition(view);
        return (Thickness)def.GetType().GetProperty("Padding")!.GetValue(def)!;
    }

    [Fact]
    public void Gap_SetsRowAndColumnGap()
    {
        var view = Layout.Grid("item1").Gap(8);
        Assert.Equal(8, GetRowGap(view));
        Assert.Equal(8, GetColumnGap(view));
    }

    [Fact]
    public void RowGap_SetsOnlyRowGap()
    {
        var view = Layout.Grid("item1").RowGap(6);
        Assert.Equal(6, GetRowGap(view));
        Assert.Equal(4, GetColumnGap(view));
    }

    [Fact]
    public void ColumnGap_SetsOnlyColumnGap()
    {
        var view = Layout.Grid("item1").ColumnGap(6);
        Assert.Equal(4, GetRowGap(view));
        Assert.Equal(6, GetColumnGap(view));
    }

    [Fact]
    public void Padding_SetsDefinitionPadding()
    {
        var view = Layout.Grid("item1").Padding(4);
        Assert.Equal(new Thickness(4), GetPadding(view));
    }
}
