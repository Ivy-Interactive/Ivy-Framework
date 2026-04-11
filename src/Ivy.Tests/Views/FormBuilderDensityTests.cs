using Ivy.Core.Hooks;

namespace Ivy.Tests.Views;

public class FormBuilderDensityTests
{
    private class TestModel
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    private static Density GetDensity<T>(FormBuilder<T> builder)
    {
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var field = builder.GetType().GetField("_density", flags)!;
        return (Density)field.GetValue(builder)!;
    }

    private static FormBuilder<TestModel> CreateBuilder()
    {
        var state = new State<TestModel>(new TestModel());
        return new FormBuilder<TestModel>(state);
    }

    [Fact]
    public void Density_DefaultsToMedium()
    {
        var builder = CreateBuilder();
        Assert.Equal(Density.Medium, GetDensity(builder));
    }

    [Fact]
    public void Density_SetsValue()
    {
        var builder = CreateBuilder();
        builder.Density(Density.Small);
        Assert.Equal(Density.Small, GetDensity(builder));
    }

    [Fact]
    public void Small_SetsDensityToSmall()
    {
        var builder = CreateBuilder();
        builder.Small();
        Assert.Equal(Density.Small, GetDensity(builder));
    }

    [Fact]
    public void Large_SetsDensityToLarge()
    {
        var builder = CreateBuilder();
        builder.Large();
        Assert.Equal(Density.Large, GetDensity(builder));
    }
}
