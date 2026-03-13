namespace Ivy.Test;

public class SplitPascalCaseTests
{
    [Theory]
    [InlineData("avgSalary", "Avg Salary")]
    [InlineData("totalCount", "Total Count")]
    [InlineData("minValue", "Min Value")]
    [InlineData("TotalSalary", "Total Salary")]
    [InlineData("count", "Count")]
    [InlineData(null, null)]
    public void SplitPascalCase_TitleCasesEachWord(string? input, string? expected)
    {
        var result = Utils.SplitPascalCase(input);
        Assert.Equal(expected, result);
    }
}
