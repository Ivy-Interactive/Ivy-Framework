// ReSharper disable once CheckNamespace
namespace Ivy;

public static class UseArgsExtensions
{
    public static T? UseArgs<T>(this IViewContext context) where T : class
    {
        var args = context.UseService<AppContext>();
        return args.GetArgs<T>();
    }
}
