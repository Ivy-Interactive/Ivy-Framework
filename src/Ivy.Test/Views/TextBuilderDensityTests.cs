namespace Ivy.Test.Views;

public class TextBuilderDensityTests
{
    private static Density? GetDensity(TextBuilder builder)
    {
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var field = builder.GetType().GetField("_density", flags)!;
        return (Density?)field.GetValue(builder);
    }

    [Fact]
    public void Density_DefaultsToNull()
    {
        var builder = Text.P("test");
        Assert.Null(GetDensity(builder));
    }

    [Fact]
    public void Density_SetsValue()
    {
        var builder = Text.P("test");
        builder.Density(Density.Small);
        Assert.Equal(Density.Small, GetDensity(builder));
    }

    [Fact]
    public void Small_SetsDensityToSmall()
    {
        var builder = Text.P("test");
        builder.Small();
        Assert.Equal(Density.Small, GetDensity(builder));
    }

    [Fact]
    public void Large_SetsDensityToLarge()
    {
        var builder = Text.P("test");
        builder.Large();
        Assert.Equal(Density.Large, GetDensity(builder));
    }
}
