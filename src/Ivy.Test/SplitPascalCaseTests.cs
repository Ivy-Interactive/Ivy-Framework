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
    [InlineData("UIDesign", "UI Design")]
    [InlineData("QATesting", "QA Testing")]
    [InlineData("APIClient", "API Client")]
    [InlineData("HTMLParser", "HTML Parser")]
    [InlineData("UAT", "UAT")]
    [InlineData("ToDo", "To Do")]
    [InlineData("InProgress", "In Progress")]
    public void SplitPascalCase_TitleCasesEachWord(string? input, string? expected)
    {
        var result = StringHelper.SplitPascalCase(input);
        Assert.Equal(expected, result);
    }
}
