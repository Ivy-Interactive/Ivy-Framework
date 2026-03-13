using System.Reactive.Disposables;
using Ivy.Core.Helpers;

// ReSharper disable once CheckNamespace
namespace Ivy;

public static class UseIntervalExtensions
{
    /// <summary>
    /// Runs <paramref name="callback"/> on a repeating interval.
    /// Pass <c>null</c> for <paramref name="interval"/> to pause (dispose the timer).
    /// Pass a <see cref="TimeSpan"/> to start or restart the timer.
    /// The timer is automatically disposed on component unmount.
    /// </summary>
    public static void UseInterval(this IViewContext context, Action callback, TimeSpan? interval)
    {
        var timerRef = context.UseState<TimerDisposable?>(() => null, buildOnChange: false);
        var prevIntervalRef = context.UseState<TimeSpan?>(() => null, buildOnChange: false);

        // Ensure cleanup on unmount
        context.UseEffect(() =>
        {
            return Disposable.Create(() =>
            {
                timerRef.Value?.Dispose();
                timerRef.Value = null;
            });
        });

        // If interval hasn't changed, nothing to do
        if (prevIntervalRef.Value == interval) return;
        prevIntervalRef.Value = interval;

        // Dispose old timer
        timerRef.Value?.Dispose();
        timerRef.Value = null;

        if (interval is null) return;

        // Create new timer with cancellation barrier
        var cts = new CancellationTokenSource();
        var timer = new System.Timers.Timer(interval.Value.TotalMilliseconds);
        timer.Elapsed += (_, _) =>
        {
            if (cts.Token.IsCancellationRequested) return;
            callback();
        };
        timer.AutoReset = true;
        timer.Start();
        timerRef.Value = new TimerDisposable(timer, cts);
    }
}
