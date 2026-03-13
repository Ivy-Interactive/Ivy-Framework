---
searchHints:
  - useinterval
  - timer
  - interval
  - polling
  - countdown
  - setinterval
---

# UseInterval

<Ingress>
Run a callback on a repeating interval with safe timer lifecycle management. Pass `null` to pause, or a `TimeSpan` to start.
</Ingress>

`UseInterval` is a built-in hook that handles `System.Timers.Timer` lifecycle correctly, eliminating the common race condition where timer callbacks fire after disposal.

## Basic Usage

```csharp
var seconds = UseState(0);
var isRunning = UseState(true);

// Timer ticks every second while isRunning is true
UseInterval(() =>
{
    seconds.Set(seconds.Value + 1);
}, isRunning.Value ? TimeSpan.FromSeconds(1) : null);
```

When `interval` is `null`, the timer is stopped and disposed. When a non-null `TimeSpan` is provided, the timer is started (or restarted if the interval changed).

## Pausing and Resuming

Control the timer by toggling the interval between a value and `null`:

```csharp demo-tabs
public class StopwatchView : ViewBase
{
    public override object? Build()
    {
        var seconds = UseState(0);
        var isRunning = UseState(false);

        UseInterval(() =>
        {
            seconds.Set(seconds.Value + 1);
        }, isRunning.Value ? TimeSpan.FromSeconds(1) : null);

        return Layout.Vertical()
            | Text.H3($"{seconds.Value}s")
            | (Layout.Horizontal()
                | new Button(isRunning.Value ? "Pause" : "Start",
                    () => isRunning.Set(!isRunning.Value))
                | new Button("Reset", () => seconds.Set(0)));
    }
}
```

## Polling

Use `UseInterval` to periodically fetch data:

```csharp
var data = UseState<string?>(null);

UseInterval(() =>
{
    data.Set(FetchLatestData());
}, TimeSpan.FromSeconds(30));
```

## Variable-Rate Interval

Change the interval dynamically based on state:

```csharp
var remaining = UseState(60);
var speed = UseState(1.0);

UseInterval(() =>
{
    remaining.Set(Math.Max(0, remaining.Value - 1));
}, remaining.Value > 0 ? TimeSpan.FromSeconds(speed.Value) : null);
```

When `remaining` reaches zero, passing `null` stops the timer automatically.

## How It Works

Internally, `UseInterval` uses `System.Timers.Timer` with a `CancellationTokenSource` barrier. When the interval changes or the component unmounts:

1. The cancellation token is cancelled (post-disposal callbacks become no-ops)
2. The timer is stopped (no new callbacks are queued)
3. The timer is disposed (resources are released)

This three-step cleanup prevents the race condition where `System.Timers.Timer.Dispose()` alone allows already-queued thread pool callbacks to execute.

## See Also

- [UseEffect](./04_UseEffect.md) - For general side effects and custom cleanup
- [State Management](./03_UseState.md) - Managing component state
