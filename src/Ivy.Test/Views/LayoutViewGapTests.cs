namespace Ivy.Test.Views;

public class LayoutViewGapTests
{
    private static int GetRowGap(LayoutView view)
    {
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var field = view.GetType().GetField("_rowGap", flags)!;
        var responsive = (Responsive<int?>)field.GetValue(view)!;
        return responsive.Default!.Value;
    }

    private static int GetColumnGap(LayoutView view)
    {
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var field = view.GetType().GetField("_columnGap", flags)!;
        var responsive = (Responsive<int?>)field.GetValue(view)!;
        return responsive.Default!.Value;
    }

    [Fact]
    public void Gap_DefaultsTo4()
    {
        var view = Layout.Horizontal();
        Assert.Equal(4, GetRowGap(view));
        Assert.Equal(4, GetColumnGap(view));
    }

    [Fact]
    public void Gap_Int_SetsBoth()
    {
        var view = Layout.Horizontal().Gap(8);
        Assert.Equal(8, GetRowGap(view));
        Assert.Equal(8, GetColumnGap(view));
    }

    [Fact]
    public void Gap_TwoInts_SetsSeparately()
    {
        var view = Layout.Horizontal().Gap(2, 6);
        Assert.Equal(2, GetRowGap(view));
        Assert.Equal(6, GetColumnGap(view));
    }

    [Fact]
    public void Gap_BoolFalse_SetsToZero()
    {
        var view = Layout.Horizontal().Gap(false);
        Assert.Equal(0, GetRowGap(view));
        Assert.Equal(0, GetColumnGap(view));
    }
}
