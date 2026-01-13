using Ivy.Core.Helpers;

namespace Ivy.Test;

public record Foo(string Bar, int Baz, int Boo, int[] Numbers);

public class ExpressionNameHelper
{
    [Fact] public void Test1() => Assert.Equal("Bar", Core.Helpers.ExpressionNameHelper.SuggestName<Foo>(e => e.Bar));
    [Fact] public void Test2() => Assert.Equal("Baz", Core.Helpers.ExpressionNameHelper.SuggestName<Foo>(e => e.Baz));
    [Fact] public void Test3() => Assert.Equal("Sum", Core.Helpers.ExpressionNameHelper.SuggestName<int[]>(e => e.Sum(f => f)));

}