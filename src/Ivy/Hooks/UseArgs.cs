using Ivy.Apps;
using Ivy.Core.Hooks;
using AppContext = Ivy.Apps.AppContext;

namespace Ivy.Hooks;

public static class UseArgsExtensions
{
    public static T? UseArgs<T>(this IViewContext context) where T : class
    {
        var args = context.UseService<AppContext>();
        return args.GetArgs<T>();
    }
}