namespace Ivy;

public static class ExceptionHelper
{
    public static void PrintDetailedException(Exception? ex)
    {
        while (ex != null)
        {
            Console.WriteLine($@"Exception Type:
{ex.GetType().FullName}");
            Console.WriteLine($@"Message:
{ex.Message}");
            Console.WriteLine($@"Source:
{ex.Source}");
            Console.WriteLine($@"Target Site:
 {ex.TargetSite}");
            Console.WriteLine($@"Stack Trace:
{ex.StackTrace}");
            Console.WriteLine(new string('-', 80));
            ex = ex.InnerException;
        }
    }

    public static Exception GetInnerMostException(Exception exception)
    {
        while (exception.InnerException != null)
        {
            exception = exception.InnerException;
        }

        return exception;
    }

    public static Exception UnwrapAggregate(this Exception e)
    {
        if (e is AggregateException aggregateException && aggregateException.InnerExceptions.Count == 1)
        {
            e = aggregateException.InnerExceptions[0];
        }

        return e;
    }
}
