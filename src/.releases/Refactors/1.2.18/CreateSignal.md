# CreateSignal Renamed to UseSignal and ISignal Unified - v1.2.18

## Summary

The `CreateSignal<T, TInput, TOutput>()` method on `IViewContext` has been removed and replaced by `UseSignal<T, TInput, TOutput>()`. Additionally, the previous separation between `ISignalSender` and `ISignalReceiver` has been unified into a single `ISignal` interface.

## What Changed

### Before

```csharp
// Sender creation
ISignalSender<string, bool> mySignalSender = context.CreateSignal<MySignal, string, bool>();

// Receiver usage
ISignalReceiver<string, bool> mySignalReceiver = context.UseSignal<MySignal, string, bool>();
```

### After

```csharp
// Both sending and receiving are now handled by ISignal, accessed via UseSignal
ISignal<string, bool> mySignal = context.UseSignal<MySignal, string, bool>();

// The returned signal instance provides both:
// mySignal.Send(input)
// mySignal.Receive(callback)
```

## How to Find Affected Code

Run a search for the old method name or interfaces:

```regex
CreateSignal<
```

Or searches for the obsolete interfaces:

```regex
ISignalSender<
```

```regex
ISignalReceiver<
```

## How to Refactor

1. Find any calls to `context.CreateSignal<...>()` and rename them to `context.UseSignal<...>()`.
2. If your code held a reference to `ISignalSender<TInput, TOutput>` or `ISignalReceiver<TInput, TOutput>`, change the type to `ISignal<TInput, TOutput>`.
3. The `ISignal` interface supports both `.Send()` and `.Receive()`, so no changes are needed to how the signal is actively used.

## Verification

After refactoring, run:

```bash
dotnet build
```
