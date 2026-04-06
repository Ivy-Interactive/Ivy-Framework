// ReSharper disable once CheckNamespace
namespace Ivy;

public static class UseClipboardExtensions
{
    public static Action<string> UseClipboard(this IViewContext context)
    {
        var client = context.UseService<IClientProvider>();
        return text => client.CopyToClipboard(text);
    }
}
