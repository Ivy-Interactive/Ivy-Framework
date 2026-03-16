using Ivy.Core;
using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

public delegate void ShowLoadingDelegate(string message = "Loading...", int? progress = null);
public delegate void HideLoadingDelegate();
public delegate void UpdateLoadingDelegate(string? message = null, int? progress = null, string? status = null);

public static class UseLoadingExtensions
{
    public static (IView? loadingView, ShowLoadingDelegate showLoading, HideLoadingDelegate hideLoading, UpdateLoadingDelegate updateLoading) UseLoading(this IViewContext context)
    {
        var open = context.UseRef(false);
        var loadingOptions = context.UseRef<LoadingOptions?>();

        var view = new FuncView(context2 =>
        {
            var openInternal = context2.UseState(false);

            context2.UseEffect(() =>
            {
                openInternal.Set(open.Value);
            }, open);

            return openInternal.Value && loadingOptions.Value != null
                ? new LoadingView(loadingOptions.Value)
                : null;
        });

        var showLoading = new ShowLoadingDelegate((message, progress) =>
        {
            loadingOptions.Set(new LoadingOptions
            {
                Message = message,
                Progress = progress,
                Indeterminate = progress == null
            });
            open.Set(true);
        });

        var hideLoading = new HideLoadingDelegate(() =>
        {
            open.Set(false);
        });

        var updateLoading = new UpdateLoadingDelegate((message, progress, status) =>
        {
            if (loadingOptions.Value != null)
            {
                loadingOptions.Set(loadingOptions.Value with
                {
                    Message = message ?? loadingOptions.Value.Message,
                    Progress = progress ?? loadingOptions.Value.Progress,
                    Status = status ?? loadingOptions.Value.Status,
                    Indeterminate = progress == null && loadingOptions.Value.Progress == null
                });
            }
        });

        return (view, showLoading, hideLoading, updateLoading);
    }
}
