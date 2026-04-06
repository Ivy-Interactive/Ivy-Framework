namespace Ivy.Test;

public class BadgeColorTests
{
    [Fact]
    public void Color_ExtensionMethod_SetsGreen()
    {
        var badge = new Badge("text").Color(Colors.Green);
        Assert.Equal(Colors.Green, badge.Color);
    }

    [Fact]
    public void Color_ExtensionMethod_SetsRed()
    {
        var badge = new Badge("text").Color(Colors.Red);
        Assert.Equal(Colors.Red, badge.Color);
    }

    [Fact]
    public void Color_Constructor_SetsBlue()
    {
        var badge = new Badge("text", color: Colors.Blue);
        Assert.Equal(Colors.Blue, badge.Color);
    }

    [Fact]
    public void Color_DefaultIsNull()
    {
        var badge = new Badge("text");
        Assert.Null(badge.Color);
    }

    [Fact]
    public void Color_ExtensionMethod_ReturnsNewInstance()
    {
        var original = new Badge("text");
        var colored = original.Color(Colors.Green);

        Assert.Null(original.Color);
        Assert.Equal(Colors.Green, colored.Color);
        Assert.NotSame(original, colored);
    }
}
