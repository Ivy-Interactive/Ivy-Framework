namespace Ivy.Test.Views;

public class TabViewPaddingTests
{
    private static Thickness? GetPadding(TabView view)
    {
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var field = view.GetType().GetField("_padding", flags)!;
        return (Thickness?)field.GetValue(view);
    }

    [Fact]
    public void Padding_DefaultsToThickness4()
    {
        var view = Layout.Tabs(new Tab("t1", "content"));
        Assert.Equal(new Thickness(4), GetPadding(view));
    }

    [Fact]
    public void Padding_Int_SetsUniform()
    {
        var view = Layout.Tabs(new Tab("t1", "content")).Padding(8);
        Assert.Equal(new Thickness(8), GetPadding(view));
    }

    [Fact]
    public void Padding_HorizontalVertical_SetsCorrectly()
    {
        var view = Layout.Tabs(new Tab("t1", "content")).Padding(4, 8);
        Assert.Equal(new Thickness(4, 8), GetPadding(view));
    }

    [Fact]
    public void Padding_FourSided_SetsCorrectly()
    {
        var view = Layout.Tabs(new Tab("t1", "content")).Padding(1, 2, 3, 4);
        Assert.Equal(new Thickness(1, 2, 3, 4), GetPadding(view));
    }
}
