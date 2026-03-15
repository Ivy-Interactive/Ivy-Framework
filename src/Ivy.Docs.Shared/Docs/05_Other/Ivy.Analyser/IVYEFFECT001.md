# IVYEFFECT001: Avoid Task.ContinueWith inside UseEffect

**Severity:** Warning

## Description

`Task.ContinueWith()` inside a `UseEffect` callback creates a fire-and-forget continuation that runs on a thread pool thread with no component lifecycle awareness. When the component is disposed (e.g., during navigation or re-render), the continuation may call `.Set()` on disposed state, causing a `NullReferenceException`.

## Cause

```csharp
// ❌ ContinueWith inside UseEffect — triggers IVYEFFECT001
public override object? Build()
{
    var shakeRow = UseState(-1);

    this.UseEffect(() => {
        if (shakeRow.Value >= 0) {
            Task.Delay(300).ContinueWith(_ => shakeRow.Set(-1)); // IVYEFFECT001
        }
        return null!;
    }, shakeRow.Value);

    return new Text("Hello");
}
```

The `ContinueWith` continuation fires after the delay regardless of whether the component is still alive. There is no cleanup mechanism to cancel it on unmount.

## Fix

Use `async`/`await` with a `CancellationTokenSource` and return it as the cleanup disposable:

```csharp
// ✅ Async with cancellation and proper cleanup
public override object? Build()
{
    var shakeRow = UseState(-1);

    this.UseEffect(() => {
        if (shakeRow.Value >= 0) {
            var cts = new CancellationTokenSource();
            _ = ResetAfterDelay(shakeRow, cts.Token);
            return cts; // CancellationTokenSource implements IDisposable
        }
        return null;
    }, shakeRow.Value);

    return new Text("Hello");
}

private static async Task ResetAfterDelay(State<int> state, CancellationToken token)
{
    await Task.Delay(300, token);
    state.Set(-1);
}
```

## See Also

- [UseEffect](../../../03_Hooks/02_Core/04_UseEffect.md)
