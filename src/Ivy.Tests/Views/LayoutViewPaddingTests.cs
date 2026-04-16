namespace Ivy.Tests.Views;

public class LayoutViewPaddingTests
{
    private static Thickness? GetPadding(LayoutView view)
    {
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var field = view.GetType().GetField("_padding", flags)!;
        var value = field.GetValue(view);
        if (value is null) return null;
        return ((Responsive<Thickness?>)value).Default;
    }

    [Fact]
    public void Padding_DefaultsToNull()
    {
        var view = Layout.Horizontal();
        Assert.Null(GetPadding(view));
    }

    [Fact]
    public void Padding_Int_SetsUniform()
    {
        var view = Layout.Horizontal().Padding(8);
        Assert.Equal(new Thickness(8), GetPadding(view));
    }

    [Fact]
    public void Padding_HorizontalVertical_SetsCorrectly()
    {
        var view = Layout.Horizontal().Padding(4, 8);
        Assert.Equal(new Thickness(4, 8), GetPadding(view));
    }

    [Fact]
    public void Padding_FourSided_SetsCorrectly()
    {
        var view = Layout.Horizontal().Padding(1, 2, 3, 4);
        Assert.Equal(new Thickness(1, 2, 3, 4), GetPadding(view));
    }
}
