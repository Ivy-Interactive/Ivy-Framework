namespace Ivy.Tests.Views;

public class DetailsBuilderDensityTests
{
    private class TestModel
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    private static Density GetDensity<T>(DetailsBuilder<T> builder)
    {
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var field = builder.GetType().GetField("_density", flags)!;
        return (Density)field.GetValue(builder)!;
    }

    [Fact]
    public void Density_DefaultsToMedium()
    {
        var builder = new DetailsBuilder<TestModel>(new TestModel());
        Assert.Equal(Density.Medium, GetDensity(builder));
    }

    [Fact]
    public void Density_SetsValue()
    {
        var builder = new DetailsBuilder<TestModel>(new TestModel());
        builder.Density(Density.Small);
        Assert.Equal(Density.Small, GetDensity(builder));
    }

    [Fact]
    public void Small_SetsDensityToSmall()
    {
        var builder = new DetailsBuilder<TestModel>(new TestModel());
        builder.Small();
        Assert.Equal(Density.Small, GetDensity(builder));
    }

    [Fact]
    public void Large_SetsDensityToLarge()
    {
        var builder = new DetailsBuilder<TestModel>(new TestModel());
        builder.Large();
        Assert.Equal(Density.Large, GetDensity(builder));
    }
}
