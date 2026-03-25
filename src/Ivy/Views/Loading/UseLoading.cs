using Ivy.Core;
using Ivy.Core.Exceptions;
using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

public interface ILoadingContext
{
    void Message(string message);
    void Status(string? status);
    void Progress(int? progress);
    CancellationToken CancellationToken { get; }
}

public delegate void ShowLoadingDelegate(Func<ILoadingContext, Task> work, bool cancellable = false);

public static class UseLoadingExtensions
{
    public static (IView? loadingView, ShowLoadingDelegate showLoading) UseLoading(this IViewContext context)
    {
        var open = context.UseRef(false);
        var loadingOptions = context.UseRef<LoadingOptions?>();
        var cts = context.UseRef<CancellationTokenSource?>();
        var exceptionHandler = context.UseService<IExceptionHandler>();

        var view = new FuncView(context2 =>
        {
            var openInternal = context2.UseState(false);
            var optionsInternal = context2.UseState<LoadingOptions?>((LoadingOptions?)null);

            context2.UseEffect(() =>
            {
                openInternal.Set(open.Value);
            }, open);

            context2.UseEffect(() =>
            {
                optionsInternal.Set(loadingOptions.Value);
            }, loadingOptions);

            return openInternal.Value && optionsInternal.Value != null
                ? new LoadingView(optionsInternal.Value, cts.Value)
                : null;
        });

        var showLoading = new ShowLoadingDelegate((work, cancellable) =>
        {
            var tokenSource = new CancellationTokenSource();
            cts.Set(tokenSource);

            loadingOptions.Set(new LoadingOptions
            {
                Cancellable = cancellable
            });
            open.Set(true);

            _ = Task.Run(async () =>
            {
                var loadingContext = new LoadingContext(loadingOptions, tokenSource.Token);
                var wasCancelled = false;
                try
                {
                    await work(loadingContext).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    wasCancelled = true;
                }
                catch (Exception ex)
                {
                    exceptionHandler?.HandleException(ex);
                }
                finally
                {
                    if (wasCancelled)
                    {
                        loadingOptions.Set(new LoadingOptions
                        {
                            Message = "Cancelling...",
                            Indeterminate = true,
                            Cancellable = false,
                            IsCancelling = true
                        });
                        await Task.Delay(800).ConfigureAwait(false);
                    }

                    open.Set(false);
                    cts.Set((CancellationTokenSource?)null);
                }
            });
        });

        return (view, showLoading);
    }

    private class LoadingContext(IState<LoadingOptions?> options, CancellationToken cancellationToken) : ILoadingContext
    {
        public CancellationToken CancellationToken => cancellationToken;

        public void Message(string message)
        {
            if (options.Value != null)
                options.Set(options.Value with { Message = message });
        }

        public void Status(string? status)
        {
            if (options.Value != null)
                options.Set(options.Value with { Status = status });
        }

        public void Progress(int? progress)
        {
            if (options.Value != null)
                options.Set(options.Value with
                {
                    Progress = progress,
                    Indeterminate = progress == null
                });
        }
    }
}
