using Ivy.Core;

namespace Ivy.Test;

public class WidgetTreeTests
{
    [Fact]
    public void CalculateMemoizedHashCodeTest()
    {
        var hash1 = WidgetTree.CalculateMemoizedHashCode("foo", [DateTime.Parse("2001-01-01"), DateTime.Parse("2001-01-02")]);
        var hash2 = WidgetTree.CalculateMemoizedHashCode("foo", [DateTime.Parse("2001-01-01"), DateTime.Parse("2001-01-03")]);
        Assert.NotEqual(hash1, hash2);
    }
}