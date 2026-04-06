namespace Ivy.Test;

public record Foo(string Bar, int Baz, int Boo, int[] Numbers);

public class ExpressionNameHelper
{
    [Fact] public void Test1() => Assert.Equal("Bar", Core.Helpers.ExpressionNameHelper.SuggestName<Foo>(e => e.Bar));
    [Fact] public void Test2() => Assert.Equal("Baz", Core.Helpers.ExpressionNameHelper.SuggestName<Foo>(e => e.Baz));
    [Fact] public void Test3() => Assert.Equal("Sum", Core.Helpers.ExpressionNameHelper.SuggestName<int[]>(e => e.Sum(f => f)));
    [Fact] public void Test4_AggregationMemberAccess() => Assert.Equal("Baz", Core.Helpers.ExpressionNameHelper.SuggestName<IEnumerable<Foo>>(e => e.Sum(x => x.Baz)));

    [Fact]
    public void Test5_WhereFilterLiteral_UsedAsName() =>
        Assert.Equal("Capital Call", Core.Helpers.ExpressionNameHelper.SuggestName<IEnumerable<Foo>>(
            e => e.Where(f => f.Bar == "Capital Call").Sum(f => f.Baz)));

    [Fact]
    public void Test6_WhereFilterLiteral_LowercaseNormalized() =>
        Assert.Equal("Management Fee", Core.Helpers.ExpressionNameHelper.SuggestName<IEnumerable<Foo>>(
            e => e.Where(f => f.Bar == "management fee").Sum(f => f.Baz)));

    [Fact]
    public void Test7_NoWhereClause_FallsBackToMemberName() =>
        Assert.Equal("Baz", Core.Helpers.ExpressionNameHelper.SuggestName<IEnumerable<Foo>>(
            e => e.Sum(x => x.Baz)));

}