namespace Ivy.Tests.Views;

public class RichTextBuilderDensityTests
{
    private static Density? GetDensity(RichTextBuilder builder)
    {
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var field = builder.GetType().GetField("_density", flags)!;
        return (Density?)field.GetValue(builder);
    }

    [Fact]
    public void Density_DefaultsToNull()
    {
        var builder = Text.Rich();
        Assert.Null(GetDensity(builder));
    }

    [Fact]
    public void Density_SetsValue()
    {
        var builder = Text.Rich();
        builder.Density(Density.Small);
        Assert.Equal(Density.Small, GetDensity(builder));
    }

    [Fact]
    public void Small_SetsDensityToSmall()
    {
        var builder = Text.Rich();
        builder.Small();
        Assert.Equal(Density.Small, GetDensity(builder));
    }

    [Fact]
    public void Large_SetsDensityToLarge()
    {
        var builder = Text.Rich();
        builder.Large();
        Assert.Equal(Density.Large, GetDensity(builder));
    }
}
