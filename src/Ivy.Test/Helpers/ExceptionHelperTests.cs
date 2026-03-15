using Ivy;

namespace Ivy.Test.Helpers;

public class ExceptionHelperTests
{
    [Fact]
    public void GetInnerMostException_SingleException_ReturnsSame()
    {
        var ex = new InvalidOperationException("test");
        Assert.Same(ex, ExceptionHelper.GetInnerMostException(ex));
    }

    [Fact]
    public void GetInnerMostException_NestedExceptions_ReturnsInnerMost()
    {
        var inner = new ArgumentException("inner");
        var middle = new InvalidOperationException("middle", inner);
        var outer = new Exception("outer", middle);

        Assert.Same(inner, ExceptionHelper.GetInnerMostException(outer));
    }

    [Fact]
    public void UnwrapAggregate_SingleInnerException_ReturnsInner()
    {
        var inner = new InvalidOperationException("test");
        var aggregate = new AggregateException(inner);

        Assert.Same(inner, aggregate.UnwrapAggregate());
    }

    [Fact]
    public void UnwrapAggregate_MultipleInnerExceptions_ReturnsAggregate()
    {
        var inner1 = new InvalidOperationException("test1");
        var inner2 = new InvalidOperationException("test2");
        var aggregate = new AggregateException(inner1, inner2);

        Assert.Same(aggregate, aggregate.UnwrapAggregate());
    }

    [Fact]
    public void UnwrapAggregate_NonAggregateException_ReturnsSame()
    {
        var ex = new InvalidOperationException("test");
        Assert.Same(ex, ex.UnwrapAggregate());
    }

    [Fact]
    public void PrintDetailedException_DoesNotThrow()
    {
        var inner = new ArgumentException("inner error");
        var outer = new InvalidOperationException("outer error", inner);

        // Should not throw
        var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            ExceptionHelper.PrintDetailedException(outer);
        }
        finally
        {
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }

        var output = writer.ToString();
        Assert.Contains("outer error", output);
        Assert.Contains("inner error", output);
    }

    [Fact]
    public void PrintDetailedException_NullException_DoesNotThrow()
    {
        ExceptionHelper.PrintDetailedException(null);
    }
}
