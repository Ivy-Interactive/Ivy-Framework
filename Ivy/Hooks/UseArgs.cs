using Ivy.Apps;
using Ivy.Core.Hooks;

namespace Ivy.Hooks;

public static class UseArgsExtensions
{
    public static T? UseArgs<T>(this IViewContext context) where T : class
    {
        var args = context.UseService<AppArgs>();
        return args.GetArgs<T>();
    }
}