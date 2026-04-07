namespace Ivy.Tests.Widgets.Charts;

public class AxisTests
{
    [Fact]
    public void TickFormatter_SingleParam_KeepsAutoType()
    {
        var axis = new YAxis("Revenue").TickFormatter("C2");

        Assert.Equal("C2", axis.TickFormatter);
        Assert.Equal(TickFormatterType.Auto, axis.TickFormatterType);
    }

    [Fact]
    public void TickFormatter_TwoParams_SetsBothProperties()
    {
        var axis = new YAxis("Revenue").TickFormatter("C2", TickFormatterType.Number);

        Assert.Equal("C2", axis.TickFormatter);
        Assert.Equal(TickFormatterType.Number, axis.TickFormatterType);
    }

    [Fact]
    public void TickFormatter_DateType_SetsCorrectly()
    {
        var axis = new XAxis().TickFormatter("MM/dd HH", TickFormatterType.Date);

        Assert.Equal("MM/dd HH", axis.TickFormatter);
        Assert.Equal(TickFormatterType.Date, axis.TickFormatterType);
    }

    [Fact]
    public void TickFormatterType_DefaultsToAuto()
    {
        var axis = new YAxis("Revenue");

        Assert.Equal(TickFormatterType.Auto, axis.TickFormatterType);
    }
}
